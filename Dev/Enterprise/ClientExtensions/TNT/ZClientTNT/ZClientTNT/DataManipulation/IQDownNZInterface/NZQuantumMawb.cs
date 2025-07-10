using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using JobDeclarationSynchroniser = Enterprise.Customs.NZ.Business.Declaration.JobDeclarationSynchroniser;
using JobMessageTypeList = Enterprise.Customs.NZ.Business.JobMessageTypeList;

namespace Enterprise.Client.TNT.NZ
{
	public class NZQuantumMawb : QuantumMawb
	{
		public NZQuantumMawb(BusinessObjectFactory factory, QuantumSegment quantumSegment, bool isX2)
			: base(factory, quantumSegment)
		{
			this.isX2 = isX2;
		}

		readonly bool isX2;

		public event EventHandler MissingShippingLineOnConsolEvent;

		#region Property

		protected override void SetLinkedConsolValue(ForwardingConsol value)
		{
			if (value != null)
			{
				if (value.ShippingLine != null)
				{
					fLinkedConsol = value;
				}
				else if (MissingShippingLineOnConsolEvent != null)
				{
					MissingShippingLineOnConsolEvent(value.JK_UniqueConsignRef, EventArgs.Empty);
				}
			}
			else
			{
				fLinkedConsol = value;
			}
		}

		#endregion

		protected override void AddGoodsDescriptionNote(QuantumShipmentNotesRecord noteRecord, ForwardingShipment shipment)
		{
			if (shipment != null && shipment.IsInDatabase && shipment.Declarations.Length > 0 && !((JobDeclaration)shipment.Declarations[0]).HasNotBeenSentToCustoms)
			{ }
			else
			{
				base.AddGoodsDescriptionNote(noteRecord, shipment);
			}
		}

		protected override void UpdateShipmentAndLinkToConsol(ForwardingShipment shipment, ZString branchCode, int percentComplete)
		{
			if (shipment == null || shipment.Declarations.Length == 0 || ((JobDeclaration)shipment.Declarations[0]).HasNotBeenSentToCustoms)
			{
				base.UpdateShipmentAndLinkToConsol(shipment, branchCode, percentComplete);
			}
		}

		protected override void RemoveMawbShipmentLinksNotInInterfaceFileIfConsolNotProcessedBefore()
		{
			// do nothing for NZ
		}

		protected override void CreateAndSyncroniseJobDeclaration(ForwardingShipment shipment, ZString branchCode)
		{
			if (shipment.Declarations.Length == 0)
			{
				JobDeclaration declaration = (JobDeclaration)shipment.Factory.New(typeof(JobDeclaration));
				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
				declaration.JE_JS = shipment.PK;
				JobDeclarationSynchroniser plugInSynchroniser = new JobDeclarationSynchroniser(declaration);
				plugInSynchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
				declaration.IsImportingData = true;
				declaration.JE_JS = shipment.PK;
				if (!isX2 && ((declaration.TotalInvoiceAmount.Amount > 0 && declaration.TotalInvoiceAmountInLocalCurrency == 0) ||
					declaration.TotalInvoiceAmountInLocalCurrency > TNTDataRegistry.Instance.INDFileImportConsignmentValueThreshold))
				{
					declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
				}
				declaration.JE_EntryStatus = (declaration.JE_MessageSubType == JobMessageSubTypeList.Codes.Simplified) ? LowValueConsignmentStatusList.Codes.NotSentToCustoms : LowValueConsignmentStatusList.Codes.ReadyForManifesting;
				if (!declaration.JE_DateAtOrigin.IsValid)
				{
					declaration.JE_DateAtOrigin = ZDateTime.Today;
				}
				declaration.JE_ExportDate = declaration.JE_DateAtOrigin;
				GlbBranch decBranch = (GlbBranch)shipment.Factory.LoadFromNaturalKey(typeof(GlbBranch), GlbBranchSchema.GB_Code, branchCode);
				declaration.JE_GB = decBranch != null ? decBranch.PK : ZGuid.Empty;
				declaration = CreateDefaultInvoiceForDeclaration(shipment, declaration);

				if (shipment.Consols.Count > 0)
				{
					SetMessageTypeBaseOnConsolsLoadAndDischargePorts(declaration, shipment.Consols[0]);
				}

				declaration.JE_GoodsLocatedAt = GoodsLocatedAtList.Codes.FW;
				declaration.WarehouseDocAddress.OrganisationPK = GlbBranch.CurrentBranch.OrgProxy.PK;
				declaration.JE_OH_Forwarder = GlbBranch.CurrentBranch.OrgProxy.PK;
			}
		}

		internal
 JobDeclaration CreateDefaultInvoiceForDeclaration(ForwardingShipment shipment, JobDeclaration declaration)
		{
			JobComInvoiceHeader defaultInv = declaration.Invoices.Count == 1 && !declaration.IsInDatabase ?
				declaration.Invoices[0] : declaration.Invoices.AddNew();

			defaultInv.JZ_InvoiceNumber = "1";
			defaultInv.JZ_OH_Supplier = shipment.ConsignorPK;
			defaultInv.JZ_InvoiceAmount = shipment.JS_GoodsValue;
			if (shipment.GoodsValueCurr != null)
			{
				defaultInv.JZ_RX_NKInvoice_Currency = shipment.GoodsValueCurr.RX_Code;
			}
			defaultInv.JZ_IncoTerm = shipment.JS_INCO;
			defaultInv.JZ_Weight = shipment.JS_ActualWeight;
			defaultInv.JZ_WeightUQ = shipment.JS_UnitOfWeight;
			defaultInv = CreateDefaultInvoiceLine(shipment, defaultInv, declaration);
			return declaration;
		}

		JobComInvoiceHeader CreateDefaultInvoiceLine(ForwardingShipment shipment, JobComInvoiceHeader defaultInv, JobDeclaration declaration)
		{
			JobComInvoiceLine defaultInvLine = defaultInv.JobComInvoiceLines.AddNew();
			defaultInvLine.JI_Calc_Invoice = defaultInv.JZ_InvoiceNumber;
			defaultInvLine.JI_LineNo = 1;
			defaultInvLine.JI_InvoiceQuantity = Convert.ToDecimal(shipment.JS_OuterPacks);
			defaultInvLine.JI_InvoiceUQ = shipment.JS_F3_NKPackType;
			defaultInvLine.JI_LinePrice = shipment.JS_GoodsValue;
			defaultInvLine.JI_Weight = shipment.JS_ActualWeight;
			defaultInvLine.JI_WeightUQ = shipment.JS_UnitOfWeight;
			defaultInvLine.JI_Description = declaration.JE_GoodsDescription;
			defaultInvLine.JI_CountryOfOrigin = declaration.JE_RL_NKOrigin.SubstringSafe(0, 2);
			defaultInvLine.JI_InvoiceUQ = declaration.JE_TotalNoOfPacksPackType;

			return defaultInv;
		}

		void SetMessageTypeBaseOnConsolsLoadAndDischargePorts(JobDeclaration jobDec, ForwardingConsol consols)
		{
			if (consols.JK_RL_NKDischargePort.Left(2) == Core.Constants.CountryCodes.NewZealand)
			{
				jobDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			}
			else if (consols.JK_RL_NKLoadPort.Left(2) == Core.Constants.CountryCodes.NewZealand)
			{
				jobDec.JE_MessageType = JobMessageTypeList.Codes.Export;
			}
		}
	}
}
