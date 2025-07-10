using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.CDS.Declaration
{
	public class CDSCusEntryInstructionValueSetStrategy : IValueSetStrategy
	{
		public CDSCusEntryInstructionValueSetStrategy(CusEntryInstruction cusEntryInstruction)
		{
			this.cusEntryInstruction = cusEntryInstruction;
		}

		public void ValueSet(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			switch (valueThatHasChanged.Name)
			{
				case CusEntryInstruction.Schema.CEI_Style:
					HandleStyleChange();
					break;
				case CusEntryInstruction.Schema.CEI_SubStyle:
					HandleStyleChange();
					DefaultEXRRAuthorisationForArrivedROROExports();
					break;

				case CusEntryInstruction.Schema.CEI_OA_Warehouse:
				case CusEntryInstruction.Schema.CEI_OA_Warehouse2:
					HandleWarehouseChange(valueThatHasChanged);
					break;
			}
		}

		void DefaultEXRRAuthorisationForArrivedROROExports()
		{
			if (GBCustomsDataRegistry.Instance.CDSEnableEXRRAutomationForArrivedROROExports.Value)
			{
				var declaration = cusEntryInstruction?.JobDeclaration;
				if (declaration != null
						&& declaration.JE_OH_Supplier != ZGuid.Empty
						&& declaration.JE_MessageType == MessageTypeList.Codes.Export
						&& declaration.JE_TransportMode == GBTransportTypeList.Codes.ROR
						&& CDSJobDeclarationValueSetStrategy.ArrivedFroniterSubstyleCodes.Contains(cusEntryInstruction.CEI_SubStyle)
						&& (declaration.Supplier?.GetEuIdentificationNumber() ?? ZString.Empty) != ZString.Empty
						&& !HasCustomsSupervisedExportsAtARoroLocationAuthorizationUsage(CDSAuthorisationHeaderTypeList.Codes.CustomsSupervisedExportsAtARoroLocation, declaration.JE_OH_Supplier))
				{
					AddCusAuthorizationUsage(CDSAuthorisationHeaderTypeList.Codes.CustomsSupervisedExportsAtARoroLocation, declaration.JE_OH_Supplier);
				}
			}
		}

		void AddCusAuthorizationUsage(ZString authorisationCode, ZGuid authorisationOwner)
		{
			var cusAuthorisation = cusEntryInstruction.CusAuthorizationUsages.AddNew();
			cusAuthorisation.AGC_Code = authorisationCode;
			cusAuthorisation.AGC_OH_Owner = authorisationOwner;
		}

		bool HasCustomsSupervisedExportsAtARoroLocationAuthorizationUsage(ZString authorisationCode, ZGuid authorisationOwner)
		{
			return cusEntryInstruction.CusAuthorizationUsages.Cast<CusAuthorizationUsage>().Any(x => x.AGC_Code == authorisationCode && x.AGC_OH_Owner == authorisationOwner);
		}

		void HandleWarehouseChange(ZPropertyInfo valueThatHasChanged)
		{
			var orgAddress = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, valueThatHasChanged.Value));

			if (orgAddress != null)
			{
				var orgHeader = Factory.Load<OrgHeader>(orgAddress.OA_OH);

				var codeTypesToLoad = new ZString[]
				{
					OrgCusCode.CodeTypes.ControlledPremisesID,
					OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator
				};

				OrgCusCode orgCusCode;

				foreach (var code in codeTypesToLoad)
				{
					orgCusCode = orgHeader.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(code, orgAddress.Country.Code);

					FillInvoiceLineSupportingDoc(orgCusCode);
				}

				orgCusCode = orgHeader.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, orgAddress.Country.Code);
				FillMiscAuthorisations(orgCusCode, orgHeader);
			}
		}

		void FillInvoiceLineSupportingDoc(OrgCusCode orgCusCode)
		{
			if (!string.IsNullOrWhiteSpace(orgCusCode?.OK_CustomsRegNo) && cusEntryInstruction?.JobDeclaration != null)
			{
				var invLine = cusEntryInstruction?.JobDeclaration?.InvoiceLines?.FirstOrDefault() as JobComInvoiceLine;

				if (invLine != null)
				{
					var code = string.Empty;
					var reference = string.Empty;

					switch (orgCusCode.OK_CodeType)
					{
						case OrgCusCode.CodeTypes.ControlledPremisesID:
							code = Constants.DocumentCodes.C517;
							reference = string.Format(CultureInfo.InvariantCulture, "{0}CWP{1}", orgCusCode.OK_RN_NKCodeCountry, orgCusCode.OK_CustomsRegNo);
							break;

						case OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator:
							code = Constants.DocumentCodes.Y027;
							reference = string.Format(CultureInfo.InvariantCulture, "{0}{1}", orgCusCode.OK_RN_NKCodeCountry, orgCusCode.OK_CustomsRegNo);
							break;
					}

					if (!invLine.SupportingDocuments.Cast<SupportingDocument>().Any(x =>
						x.CSI_Code == code &&
						x.CSI_ReferenceNumber == reference))
					{
						var sd = invLine?.SupportingDocuments?.AddNew();

						sd.CSI_Code = code;
						sd.CSI_ReferenceNumber = reference;
					}
				}
			}
		}

		void FillMiscAuthorisations(OrgCusCode orgCusCode, OrgHeader orgHeader)
		{
			if (!string.IsNullOrWhiteSpace(orgCusCode?.OK_CustomsRegNo) && cusEntryInstruction?.JobDeclaration != null)
			{
				var holder = string.Format(CultureInfo.InvariantCulture, "{0}{1}", orgCusCode.OK_RN_NKCodeCountry, orgCusCode.OK_CustomsRegNo);

				if (!cusEntryInstruction.CusAuthorizationUsages.Cast<CusAuthorizationUsage>().Any(x =>
						x.AGC_Code == CDSAuthorisationHeaderTypeList.Codes.CustomsWarehousingCWP &&
						x.AGC_Number == holder))
				{
					var authLine = cusEntryInstruction.CusAuthorizationUsages.AddNew();

					authLine.AGC_Code = CDSAuthorisationHeaderTypeList.Codes.CustomsWarehousingCWP;
					authLine.AGC_Number = holder;
					authLine.AGC_OH_Owner = orgHeader.PK;
				}
			}
		}

		void HandleStyleChange()
		{
			if (!cusEntryInstruction.CEI_Style.IsEmpty && !cusEntryInstruction.CEI_SubStyle.IsEmpty)
			{
				var mapping = GetAuthorisationMapping(cusEntryInstruction);

				if (!mapping.code.IsEmpty)
				{
					if (!cusEntryInstruction.CusAuthorizationUsages.Any(x => x.AGC_Code == mapping.code))
					{
						var auth = cusEntryInstruction.CusAuthorizationUsages.AddNew();

						auth.AGC_Code = mapping.code;
						auth.AGC_OH_Owner = mapping.owner;
					}
				}
			}
		}

		internal static (ZString code, ZGuid owner) GetAuthorisationMapping(CusEntryInstruction cei)
		{
			var pair = $"{cei.CEI_Style}/{cei.CEI_SubStyle}";

			switch (pair)
			{
				case "I1/C":
				case "I1/F":
					return HasImporter(cei) ? (CDSAuthorisationHeaderTypeList.Codes.SimplifiedDeclaration, cei.JobDeclaration.JE_OH_Importer) : (ZString.Empty, ZGuid.Empty);
				default:
					return (ZString.Empty, ZGuid.Empty);
			}
		}

		static bool HasImporter(CusEntryInstruction cei)
		{
			var importer = cei.JobDeclaration.JE_OH_Importer;
			if (importer.IsDefault || importer.IsEmpty)
			{
				return false;
			}
			else
			{
				return cei.Factory.Load<OrgHeader>(importer)?.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori).Any() ?? false;
			}
		}

		readonly CusEntryInstruction cusEntryInstruction;

		BusinessObjectFactory Factory => cusEntryInstruction.Factory;
	}
}
