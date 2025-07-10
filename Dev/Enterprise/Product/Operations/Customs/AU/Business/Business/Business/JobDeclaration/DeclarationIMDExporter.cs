using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DeclarationIMDExporter : CMRDataExporterCSV
	{
		public DeclarationIMDExporter(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public JobDeclaration Declaration
		{
			get { return (JobDeclaration)BizObj; }
		}

		public override string PartFileName
		{
			get { return "IMD"; }
		}

		protected override IDataExportCSVFileNameProvider FileNameProvider
		{
			get { return Declaration; }
		}

		protected override IDocManagerSupport DocManagerSupporter
		{
			get { return Declaration; }
		}

		public override string MailSubject
		{
			get
			{
				return "Contingency Import Declaration";
			}
		}

		public override ZString BodyText
		{
			get
			{
				return RevenueUndertaking;
			}
		}
		public const string RevenueUndertaking = "If clearance is granted to take the goods identified in the attached contingency import declaration file into home consumption or for warehousing, I undertake to give Customs a declaration, in any case not later than 24 hours after the CEO declares that the Integrated Cargo System is operative, providing all particulars in accordance with section 71L in respect of the goods, and pay any duty, pay or defer GST/WET/LCT or any other charge owing at the rate applicable at the time the clearance is granted and to comply [sic] with any condition to which this clearance is subject. Failure to comply with any of the conditions may result in penalty action being undertaken.";

		public override AdditionalContingencyData AdditionalData
		{
			get
			{
				if (fAdditionalData == null)
				{
					fAdditionalData = new AdditionalContingencyData(Factory);
					if (Declaration.DepotDocAddress.Address != null)
					{
						fAdditionalData.OriginPremise = Declaration.DepotDocAddress.Address.LocalControlledPremisesID;
					}
					if (fAdditionalData.OriginPremise.IsEmpty && Declaration.ContainerTerminalOperatorDocAddress.Address != null)
					{
						fAdditionalData.OriginPremise = Declaration.ContainerTerminalOperatorDocAddress.Address.LocalControlledPremisesID;
					}
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

				foreach (CusEntryHeader entryHeader in Declaration.CustomsEntryHeaders)
				{
					foreach (CusEntryLine entryLine in entryHeader.MergedLines)
					{
						StringCollectionX result = new StringCollectionX();

						ZString firstElement = Declaration.IsSea ? Declaration.VesselNumber : ZString.Empty;
						if (firstElement.IsEmpty)
						{
							firstElement = " ";// first element is lost if null 
						}

						result.Add(firstElement);
						result.Add(Declaration.JE_VoyageFlightNo);
						result.Add(Declaration.JE_MasterBill);
						result.Add(Declaration.JE_HouseBill);
						if (Declaration.IsSea && !Declaration.VesselNumber.IsEmpty)
						{
							result.Add(Declaration.JE_ContainerMode);
							result.Add(Declaration.CusContainers.Count > 0 ? Declaration.CusContainers[0].CO_ContainerNumber : ZString.Empty);
						}
						else
						{
							result.Add(ZString.Empty);
							result.Add(ZString.Empty);
						}

						result.Add(entryLine.CL_Description.Substring(0, 128));
						OrgHeader supplier = entryLine.Supplier ?? Declaration.Supplier;
						result.Add(supplier != null ? supplier.OH_FullNameTruncated : ZString.Empty);
						JobDocAddress consignee = Declaration.ImporterDeliveryAddress;
						if (consignee != null)
						{
							result.Add(consignee.E2_CompanyNameTruncated);
							result.Add(CMRDataExporterCSV.AddressAsASingleLine(consignee.E2_Address1, consignee.E2_Address2, consignee.E2_City, consignee.E2_State, consignee.E2_Postcode));
						}
						else
						{
							if (Declaration.Importer != null && Declaration.Importer.MainAddress != null)
							{
								OrgAddress importer = Declaration.Importer.MainAddress;
								result.Add(Declaration.Importer.OH_FullNameTruncated);
								result.Add(CMRDataExporterCSV.AddressAsASingleLine(importer.OA_Address1, importer.OA_Address2, importer.OA_City, importer.OA_State, importer.OA_PostCode));
							}
							else
							{
								result.Add(ZString.Empty);
								result.Add(ZString.Empty);
							}
						}
						result.Add(Declaration.JE_RL_NKPortOfArrival);
						result.Add(Declaration.Packages.HasDangerousGoods ? "YES" : "");
						result.Add(Declaration.IsSAC ? "YES" : "NO");
						if (Declaration.Importer == null)
						{
							result.Add(ZString.Empty);
						}
						else
						{
							result.Add(!Declaration.Importer.LocalBusinessRegNo.IsEmpty ? Declaration.Importer.LocalBusinessRegNo : Declaration.Importer.GetCustomsClientID());
						}
						result.Add(Declaration.Importer != null ? Declaration.Importer.OH_FullNameTruncated : ZString.Empty);
						result.Add(Declaration.JE_OwnerRef);
						result.Add(Declaration.CustomsEntryHeaders.Count > 0 ? Declaration.CustomsEntryHeaders[0].CH_BGMReference : ZString.Empty);
						result.Add(Declaration.JE_TransportMode);
						result.Add(AdditionalData.OriginPremise);
						result.Add(entryLine.TariffNumber);
						result.Add(entryLine.StatCode);
						result.Add(entryLine.RandomLine.AggregatedZA_ORG);
						result.Add(Declaration.Supplier != null ? Declaration.Supplier.GetCustomsClientID() : ZString.Empty);
						result.Add(entryLine.CustomsValue.Amount.ToString("f2"));
						result.Add(entryLine.DutyAmount.ToString("f2"));
						result.Add(entryLine.GSTVATAmount.ToString("f2"));
						result.Add(GlbStaff.CurrentUser.GS_EmailAddress);
						result.Add(GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number);
						result.Add(GlbCompany.CurrentCompany.GC_Name);
						result.Add(entryLine.InvoiceQuantity.ToString("f0"));

						list.Add(result);
					}
				}
				return (StringCollectionX[])list.ToArray(typeof(StringCollectionX));
			}
		}
	}
}
