using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.CLE.MattelARInvoiceExport.Testing
{
	public abstract class INVOICMessageBuilderTest : TransactionedTestCase
	{
		[TestDate(2006, 12, 12, 12, 12, 12)]
		public void TestGenerateMessageText()
		{
			OutPutFile = MesgBuilder.GenerateMessageText();
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var expectedFile = resourceRetriever.SaveResourceToFile(ExpectedResult);
				AssertASCIIFilesHasSameData(expectedFile, OutPutFile);
			}
		}

		#region Implementation
		protected abstract string ExpectedResult { get; }

		protected abstract INVOICMessageBuilder MesgBuilder { get; }

		protected override void SetUp()
		{
			base.SetUp();
			var company = GlbCompany.GetCurrentCompany(new BusinessObjectFactory());
			company.GC_BusinessRegNo = "BusinessReg";
			company.Factory.Save();
			CLEDataRegistry.Instance.ClemengerMailboxNumber = "123:ZZ";
			CLEDataRegistry.Instance.MattelMailboxNumber = "456:ZZ";
			OutPutFile = "";
			SetupOrganisation();
		}

		protected ForwardingConsol Consol;
		protected OrgHeader Supplier, Importer;
		string OutPutFile;
		protected AccChargeCode CreateChargeCode(string code, string description, string chargeType)
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "T" + code;
			chargeCode.AC_Desc = description;
			chargeCode.AC_ChargeType = chargeType;
			chargeCode.AC_MarginPercentage = 0m;
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode.FillWithValidTestData();
			return chargeCode;
		}

		protected AccChargeCode ChargeCodeDisbursementDuty
		{
			get
			{
				if (chargeCodeDisbursementDuty == null)
				{
					chargeCodeDisbursementDuty = CreateChargeCode("DUT", "Customs Disbursements Duty", Core.Constants.ChargeType.Disbursement);
				}

				return chargeCodeDisbursementDuty;
			}
		}

		AccChargeCode chargeCodeDisbursementDuty;
		#region InvoiceRecord
		protected JobInvoiceRecord InvoiceRecord
		{
			get
			{
				if (fInvoiceRecord == null)
				{
					Shipment = SetupShipment();
					BaseJobDeclaration jobDec = SetupDeclaration();
					fInvoiceRecord = new JobInvoiceRecord(Shipment, jobDec, ARInvoices);
				}

				return fInvoiceRecord;
			}
		}

		JobInvoiceRecord fInvoiceRecord;
		protected ForwardingShipment Shipment;
		#endregion
		#region SetupDeclaration
		protected abstract BaseJobDeclaration SetupDeclaration();
		protected abstract ZString Origin { get; }

		protected abstract ZString Dest { get; }

		#endregion
		protected ForwardingShipment SetupShipment()
		{
			Consol = Factory.New<ForwardingConsol>();
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Consol.JK_MasterBillNum = "MAWB";
			Consol.JK_RL_NKLoadPort = Origin;
			Consol.JK_RL_NKDischargePort = Dest;
			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = Origin;
			transport.JW_RL_NKDiscPort = Dest;
			RefContainer cont20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			RefContainer cont40GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			ForwardingContainer cont1 = Consol.Containers.AddNew();
			cont1.JC_ContainerNum = "ABCD1234560";
			cont1.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			cont1.JC_RC = cont20GP.PK;
			ForwardingContainer cont2 = Consol.Containers.AddNew();
			cont2.JC_ContainerNum = "DEFG1234560";
			cont2.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			cont2.JC_RC = cont40GP.PK;
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HAWB";
			shipment.JS_RL_NKDestination = Dest;
			shipment.JS_RL_NKOrigin = Origin;
			shipment.JS_ActualVolume = 10.5m;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			shipment.Consols.Add(Consol);
			shipment.DocsAndCartage.JP_OrderItemsAsString = "ORDER1, ORDER2";
			ForwardingPackLine pack = shipment.OuterPackLines.AddNew();
			pack.JL_PackageCount = 10;
			pack.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			pack.JL_JC = cont1.PK;
			cont1.PackLines.Add(pack);
			ForwardingPackLine pack2 = shipment.OuterPackLines.AddNew();
			pack2.JL_PackageCount = 20;
			pack2.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			pack2.JL_JC = cont2.PK;
			cont2.PackLines.Add(pack2);
			Factory.Save();
			return shipment;
		}

		#region SetupOrganisation
		void SetupOrganisation()
		{
			Supplier = Factory.New<OrgHeader>();
			Supplier.OH_FullName = "Supplier";
			Supplier.OH_Code = "Supp";
			Supplier.OH_IsConsignor = true;
			Importer = Factory.New<OrgHeader>();
			Importer.OH_FullName = "Importer";
			Importer.OH_Code = "Import";
			Importer.OH_IsConsignee = true;
		}

		#endregion
		protected InvoicingBase[] ARInvoices;
		protected override void TearDown()
		{
			base.TearDown();
			if (!string.IsNullOrEmpty(OutPutFile))
			{
				File.Delete(OutPutFile);
			}
		}

		#region CreateInvoiceLine
		protected ARInvoiceLine CreateInvoiceLine(ARInvoice invoice, ZGuid headerPK, ZDecimal amount, ZDecimal tax, ZGuid chargeCodePK)
		{
			ARInvoiceLine line = (ARInvoiceLine)invoice.Lines.AddNew();
			line.AL_JH = headerPK;
			line.AL_AC = chargeCodePK;
			line.AL_ExchangeRate = 1m;
			line.AL_OSExTaxAmount = amount;
			line.AL_OSTaxAmount = tax;
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AL_ARLine = line.PK;
			charge.JR_JH = headerPK;
			charge.SetAmountsFromLinkedLinesForTests();
			return line;
		}

		protected ARInvoice CreateInvoice(ZGuid headerPK, bool isDisbursement, ZGuid debtorPK, ZString invoiceNo, bool isCancelled, Action<ARInvoice> extraSetup = null)
		{
			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_JH = headerPK;
			if (isDisbursement)
			{
				invoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			}

			invoice.AH_OH = debtorPK;
			invoice.AH_IsCancelled = isCancelled;
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			if (extraSetup != null)
			{
				extraSetup(invoice);
			}

			invoice.AH_TransactionNum = invoiceNo;
			invoice.IsManuallySetTransactionNumber_ForTestOnly = true;
			Factory.Save();

			invoice.AH_FullyPaidDate = ZDateTime.Empty;
			Factory.Save();

			return invoice;
		}

		#endregion
		#region Factory
		protected BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = new BusinessObjectFactory();
				}

				return fFactory;
			}
		}

		BusinessObjectFactory fFactory;
		#endregion
		#endregion
	}
}
