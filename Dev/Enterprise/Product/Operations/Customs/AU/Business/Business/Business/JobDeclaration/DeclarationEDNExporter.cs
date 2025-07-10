using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DeclarationEDNExporter : CMRDataExporterCSV
	{
		public DeclarationEDNExporter(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public JobDeclaration Declaration
		{
			get { return (JobDeclaration)BizObj; }
		}

		public override string PartFileName
		{
			get { return "EDN"; }
		}

		public override string MailSubject
		{
			get
			{
				return "Contingency Export Declaration";
			}
		}

		protected override IDataExportCSVFileNameProvider FileNameProvider
		{
			get { return Declaration; }
		}

		protected override IDocManagerSupport DocManagerSupporter
		{
			get { return Declaration; }
		}

		public override AdditionalContingencyData AdditionalData
		{
			get
			{
				if (fAdditionalData == null)
				{
					fAdditionalData = new AdditionalContingencyData(Factory);
					fAdditionalData.IsOriginPremiseReadOnly = true;
				}
				return fAdditionalData;
			}
		}
		AdditionalContingencyData fAdditionalData;

		protected override StringCollectionX[] Values
		{
			get
			{
				ArrayList list = new ArrayList();

				foreach (JobComInvoiceLine aHECCLine in Declaration.InvoiceLines)
				{
					StringCollectionX result = new StringCollectionX();

					result.Add(GlbCompany.CurrentCompany.GC_Name);
					result.Add(GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number);
					result.Add(GlbStaff.CurrentUser.GS_EmailAddress);
					result.Add(JobReference);
					result.Add(Declaration.JE_MasterBill);
					result.Add(Declaration.JE_HouseBill);
					result.Add(Declaration.Supplier != null ? Declaration.Supplier.OH_FullNameTruncated : ZString.Empty);
					if (Declaration.Supplier == null)
					{
						result.Add(ZString.Empty);
					}
					else
					{
						result.Add(!Declaration.Supplier.LocalBusinessRegNo.IsEmpty ? Declaration.Supplier.LocalBusinessRegNo : Declaration.Supplier.GetCustomsClientID());
					}

					result.Add(Declaration.Importer != null ? Declaration.Importer.OH_FullNameTruncated : ZString.Empty);
					result.Add(Declaration.JE_RL_NKFinalDestination.SubstringSafe(0, 2));
					result.Add(CMRDataExporterCSV.CMRDateString(Declaration.JE_ExportDate));
					result.Add(Declaration.JE_TransportMode);

					if (Declaration.JE_TransportMode == Core.Constants.TransportModes.Air)
					{
						result.Add(Declaration.JE_VoyageFlightNo);
					}
					else if (Declaration.JE_TransportMode == Core.Constants.TransportModes.Sea && !Declaration.VesselNumber.IsEmpty)
					{
						result.Add(Declaration.VesselNumber);
					}
					else
					{
						result.Add(ZString.Empty);
					}

					result.Add(aHECCLine.JI_Tariff.Replace(".", ""));
					result.Add(aHECCLine.JI_Description.Substring(0, 128));
					result.Add(aHECCLine.AddInfo.ZA_PermitNumbers_Hidden);
					var invoiceCurrency = aHECCLine.InvoiceHeader.Invoice_Currency;
					result.Add(aHECCLine.JI_Calc_FOB_InLocalCurrency.ToString("f"));
					result.Add(Declaration.JE_RL_NKPortOfLoading);
					if (Declaration.JE_TransportMode == Core.Constants.TransportModes.Sea)
					{
						result.Add(Declaration.JE_VoyageFlightNo);
					}
					else
					{
						result.Add(ZString.Empty);
					}

					list.Add(result);
				}

				return (StringCollectionX[])list.ToArray(typeof(StringCollectionX));
			}
		}

		#region Implementation

		ZString JobReference
		{
			get { return Declaration.Shipment != null ? Declaration.Shipment.JS_UniqueConsignRef : Declaration.JE_DeclarationReference; }
		}

		#endregion
	}
}
