using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.MasterFiles;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DE.Business
{
	public class AdditionalInfoValidation : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoValidation
	{
		public AdditionalInfoValidation(AdditionalInfo parent) : base(parent)
		{
		}

		protected new AdditionalInfo Parent => (AdditionalInfo)base.Parent;

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();

			var parent = Parent;
			var code = parent.CSI_Code;
			if (!code.IsEmpty)
			{
				var subType = parent.CSI_SubType;
				var targetInfo = parent.CSI_CodeInfo;

				if (parent.ParentAsInvoiceHeader is JobComInvoiceHeader invoiceHeader)
				{
					ValidateCSI_CodeUnique(targetInfo, invoiceHeader.AdditionalInfos);
					ValidateCSI_CodeX0004(targetInfo, subType, invoiceHeader);
				}
				else if (parent.ParentAsInvoiceLine is JobComInvoiceLine invoiceLine)
				{
					var additionalInfos = invoiceLine.AdditionalInfos;
					ValidateCSI_CodeUnique(targetInfo, additionalInfos);
					ValidateCSI_CodeC019(targetInfo, subType, invoiceLine);
					ValidateCSI_CodeInvalidCodesREF(targetInfo, subType);
					ValidateCSI_CodeMutuallyExclusiveCodesINF(targetInfo, subType, additionalInfos);
				}
				else if (parent.ParentAsCusClassPartPivot is CusClassPartPivot cusClassPartPivot)
				{
					var additionalInfos = cusClassPartPivot.AdditionalInfos;
					ValidateCSI_CodeUnique(targetInfo, additionalInfos);
					ValidateCSI_CodeInvalidCodesREF(targetInfo, subType);
					ValidateCSI_CodeMutuallyExclusiveCodesINF(targetInfo, subType, additionalInfos);
				}
			}

			void ValidateCSI_CodeUnique(ZPropertyInfo targetInfo, AdditionalInfoCollection additionalInfos)
			{
				if (additionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == code && x.PK != parent.PK))
				{
					targetInfo.AddMessageError(Res.GetString("611015E7-D2A7-4C08-BD38-8D2A03C81113", "An Additional Document of Type {0} has already been entered.", code));
				}
			}

			void ValidateCSI_CodeC019(ZPropertyInfo targetInfo, ZString subType, JobComInvoiceLine invoiceLine)
			{
				if (subType == AdditionalDocTypeList.Codes.Authorization &&
					code != UniversalReferenceConstants.RefCusCodeList.Codes.Code_C019 &&
					(invoiceLine.EntryInstruction?.CEI_Style ?? ZString.Empty).StartsWith("12") &&
					invoiceLine.JI_Procedure.SubstringSafe(2, 2) == CustomsProcedureCodeList.Import.ProcedureCode._48)
				{
					targetInfo.AddMessageError(Res.GetString("46D8F257-CCFB-4EBA-83CF-AB1CEDA9757E", "For the selected Type (Procedure) and CPC, only 'C019' can be selected."));
				}
			}

			void ValidateCSI_CodeInvalidCodesREF(ZPropertyInfo targetInfo, ZString subType)
			{
				if (subType == AdditionalDocTypeList.Codes.AdditionalReference && InvalidReferenceCodes.Contains(code))
				{
					targetInfo.AddMessageError(Res.GetString("FB71544B-46E6-42CA-86FC-17364DAE5CBD", "Additional References of Type 9ZZX, 9ZZY and 9ZZZ are currently not valid."));
				}
			}

			void ValidateCSI_CodeMutuallyExclusiveCodesINF(ZPropertyInfo targetInfo, ZString subType, AdditionalInfoCollection additionalInfos)
			{
				if (subType == AdditionalDocTypeList.Codes.AdditionalInformation && MutuallyExclusiveInformationCodes.Contains(code))
				{
					if (IEnumerableExtensions.DistinctBy(additionalInfos.Cast<AdditionalInfo>()
					.Where(x => x.CSI_SubType == AdditionalDocTypeList.Codes.AdditionalInformation && MutuallyExclusiveInformationCodes.Contains(x.CSI_Code))
					, x => x.CSI_Code).Count() > 1)
					{
						targetInfo.AddMessageError(Res.GetString("C9DA99D8-08FF-497D-B0BF-CF839623CD32", "You only may enter one of the Additional Information Types 00700, 00800 or 00900 at the same time."));
					}
				}
			}

			void ValidateCSI_CodeX0004(ZPropertyInfo targetInfo, ZString subType, JobComInvoiceHeader invoiceHeader)
			{
				var jobDeclaration = invoiceHeader.JobDeclaration;
				if (parent.IsExport && jobDeclaration != null &&
					jobDeclaration.JE_GoodsDestination != UniversalReferenceConstants.CountryCodes.CountryCodeQQ &&
					subType == AdditionalDocTypeList.Codes.AdditionalInformation &&
					code == UniversalReferenceConstants.AdditionalInfoCodes.X0004)
				{
					targetInfo.AddMessageError(Res.GetString("1fd9b9dc-91fa-4cac-8bfd-6c5c4b6aab5f", "Type 'X0004' is only valid for Destination Country/Region Code 'QQ'."));
				}
			}
		}

		protected override void CheckCSI_SubType()
		{
			base.CheckCSI_SubType();

			var parent = Parent;
			if (parent.IsExport)
			{
				var targetInfo = parent.CSI_SubTypeInfo;
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(targetInfo);
				ValidateCSI_SubTypeMaximumNumberOfAuthorizationRecords();

				void ValidateCSI_SubTypeMaximumNumberOfAuthorizationRecords()
				{
					if (parent.CSI_SubType == AdditionalDocTypeList.Codes.Authorization)
					{
						if (parent.ParentAsInvoiceLine is JobComInvoiceLine invoiceLine)
						{
							CheckCSI_SubTypeMaximumNumberOfAuthorizationRecords(targetInfo, invoiceLine.AdditionalInfos);
						}
						else if (parent.ParentAsCusClassPartPivot is CusClassPartPivot cusClassPartPivot)
						{
							CheckCSI_SubTypeMaximumNumberOfAuthorizationRecords(targetInfo, cusClassPartPivot.AdditionalInfos);
						}
					}
				}
			}

			void CheckCSI_SubTypeMaximumNumberOfAuthorizationRecords(ZPropertyInfo targetInfo, AdditionalInfoCollection additionalInfos)
			{
				if (additionalInfos.Cast<AdditionalInfo>().Count(x => x.CSI_SubType == AdditionalDocTypeList.Codes.Authorization) > 9)
				{
					targetInfo.AddMessageError(Res.GetString("7D32CC3E-B155-4FE0-87D6-1B2379EA5970", "You are only allowed a maximum of 9 Authorization records"));
				}
			}
		}

		protected override void CheckCSI_Description()
		{
			base.CheckCSI_Description();

			var parent = Parent;
			if (parent.CSI_Code == UniversalReferenceConstants.AdditionalInfoCodes.X0000)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_DescriptionInfo);
			}
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();

			if (FullTypeRefCusCodeHasAttributeWithValueY(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Reference))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
			}

			if (!Parent.CSI_ReferenceNumber.IsEmpty)
			{
				ValidateCSI_ReferenceNumberAuthorizationType();
				ValidateCSI_ReferenceNumberAuthorizationCountry();
			}
		}

		protected override void CheckCSI_ReferenceNumber2()
		{
			base.CheckCSI_ReferenceNumber2();

			var parent = Parent;
			var targetInfo = parent.CSI_ReferenceNumber2Info;

			if (parent.CSI_SubType == AdditionalDocTypeList.Codes.Authorization &&
				parent.CSI_Code.In(new ZString[] { UniversalReferenceConstants.RefCusCodeList.Codes.Code_C626, UniversalReferenceConstants.RefCusCodeList.Codes.Code_C627 }))
			{
				MessageErrorIfInvalidEORI(targetInfo);
			}
			else
			{
				if (FullTypeRefCusCodeHasAttributeWithValueY(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Detail))
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}
				MessageErrorIfLongerThanMaximum(targetInfo, 12);
			}
		}

		protected override void CheckCSI_RX_NKCurrency()
		{
			base.CheckCSI_RX_NKCurrency();

			var parent = Parent;
			if (UsedInExportInvoiceLine || parent.UsedInExportCusClassPartPivot)
			{
				var targetInfo = parent.CSI_RX_NKCurrencyInfo;
				if (FullTypeRefCusCodeHasAttributeWithValueY(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Value))
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}

				var list = ZZRefCusCodeListCombinedCollection.GetCachedCollection(parent.Factory, Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CURRE, ZDateTime.Today);
				ListValidation.MessageErrorIfInvalidCode(targetInfo, list);
			}
		}

		protected override void CheckCSI_Value()
		{
			base.CheckCSI_Value();

			var parent = Parent;
			if (FullTypeRefCusCodeHasAttributeWithValueY(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Value))
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_ValueInfo);
			}

			if (!parent.CSI_Value.IsInteger && UniversalValidationHelper.IsInAESTransitionPeriod && parent.ParentAsInvoiceLine is JobComInvoiceLine invoiceLine && !invoiceLine.EntryInstruction.Style4thDigitIs4())
			{
				parent.CSI_ValueInfo.AddMessageError(Res.GetString("33902DBB-A498-42AF-B876-4CBE005005AE", "Amount should be an integer value."));
			}
		}

		protected override bool IsCodeEnabled => Parent.IsExport || Parent.ParentIsExitDetail;

		protected override bool IsOtherFieldsEnabled => !(UsedInInvoiceLine || Parent.UsedInExportCusClassPartPivot || UsedInExportInvoiceHeader);

		bool UsedInInvoiceLine => Parent.ParentAsInvoiceLine != null;

		bool UsedInExportInvoiceLine => Parent.ParentAsInvoiceLine?.IsExport ?? false;

		bool UsedInExportInvoiceHeader => Parent.ParentAsInvoiceHeader?.IsExport ?? false;

		bool FullTypeRefCusCodeHasAttributeWithValueY(string attributeName)
		{
			return Parent.FullTypeRefCusCode?.HasAttribute(attributeName, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes) ?? false;
		}

		void MessageErrorIfInvalidEORI(ZPropertyInfo propertyInfo)
		{
			var value = (ZString)propertyInfo.Value;
			var isValid = false;
			if (Regex.IsMatch(value, @"^[A-Z]{2}[\x21-\x7E]{1,15}$"))
			{
				var countryCode = value.Left(2);
				isValid = Parent.Factory.LoadFromNaturalKey<Enterprise.MasterFiles.Integration.IRefCountry>(ZArchitecture.Schema.RefCountrySchema.RN_Code, countryCode) != null;
			}

			if (!isValid)
			{
				propertyInfo.AddMessageError(Res.GetString("AC374250-A697-4F6A-8985-11C32A6964C2", "Please enter a valid EORI-Number (Country/Region code followed by up to 15 alphanumeric characters)."));
			}
		}

		void MessageErrorIfLongerThanMaximum(ZPropertyInfo propertyInfo, int maxLength)
		{
			var value = (ZString)propertyInfo.Value;
			if (value.Length > maxLength)
			{
				propertyInfo.AddMessageError(Res.GetString("5FF41D17-83EF-43D5-B951-846104BC863E", "The maximum length of {0} ({1} characters) has been exceeded.", propertyInfo.HumanReadableName, maxLength));
			}
		}

		void ValidateCSI_ReferenceNumberAuthorizationType()
		{
			var parent = Parent;
			if (parent.CSI_SubType == AdditionalDocTypeList.Codes.Authorization &&
				RequiredAuthorizationTypes.TryGetValue(parent.CSI_Code, out var requiredType) &&
				parent.CSI_ReferenceNumber.SubstringSafe(2, 3) != requiredType)
			{
				parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("00CCA66C-B301-4DFC-AD7B-8DE54F12BE2C", "For the selected Full Type, digit 3-5 of Reference must be '{0}'", requiredType));
			}
		}

		void ValidateCSI_ReferenceNumberAuthorizationCountry()
		{
			var parent = Parent;
			var invoiceLine = parent.ParentAsInvoiceLine;
			if (parent.CSI_SubType == AdditionalDocTypeList.Codes.Authorization &&
				parent.CSI_Code == UniversalReferenceConstants.RefCusCodeList.Codes.Code_C601 &&
				invoiceLine != null)
			{
				var procedure = invoiceLine.JI_Procedure.SubstringSafe(2, 2);
				var targetInfo = parent.CSI_ReferenceNumberInfo;
				var referenceStartsWithDE = parent.CSI_ReferenceNumber.StartsWith(Core.Constants.CountryCodes.Germany);

				if (procedure == CustomsProcedureCodeList.Import.ProcedureCode._51 && !referenceStartsWithDE)
				{
					targetInfo.AddMessageError(Res.GetString("0B79FC3E-A7AF-46C9-9EBC-B8B3A1DCB54B", "For the selected Full Type and CPC, digit 1-2 of Reference must be '{0}'", Core.Constants.CountryCodes.Germany));
				}
				else if (procedure == CustomsProcedureCodeList.Import.ProcedureCode._54 && referenceStartsWithDE)
				{
					targetInfo.AddMessageError(Res.GetString("2AAEDFE9-619B-4A70-9BB1-022B15FA3E40", "For the selected Full Type and CPC, digit 1-2 of Reference must NOT be '{0}'", Core.Constants.CountryCodes.Germany));
				}
			}
		}

		static readonly ImmutableHashSet<string> InvalidReferenceCodes = new HashSet<string>(new[]
		{
			UniversalReferenceConstants.RefCusCodeList.Codes.Code_9ZZX,
			UniversalReferenceConstants.RefCusCodeList.Codes.Code_9ZZY,
			UniversalReferenceConstants.RefCusCodeList.Codes.Code_9ZZZ
		}).ToImmutableHashSet();

		static readonly ImmutableHashSet<string> MutuallyExclusiveInformationCodes = new HashSet<string>(new[]
		{
			UniversalReferenceConstants.RefCusCodeList.Codes.Code_00700,
			UniversalReferenceConstants.RefCusCodeList.Codes.Code_00800,
			UniversalReferenceConstants.RefCusCodeList.Codes.Code_00900
		}).ToImmutableHashSet();

		static readonly ImmutableDictionary<string, string> RequiredAuthorizationTypes = new Dictionary<string, string>
		{
			{ UniversalReferenceConstants.RefCusCodeList.Codes.Code_C019, CusAuthorizationHeaderTypeList.Codes.OutwardProcessing },
			{ UniversalReferenceConstants.RefCusCodeList.Codes.Code_C516, CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission },
			{ UniversalReferenceConstants.RefCusCodeList.Codes.Code_C601, CusAuthorizationHeaderTypeList.Codes.InwardProcessing },
			{ UniversalReferenceConstants.RefCusCodeList.Codes.Code_C626, EU.Business.UniversalReferenceConstants.CusAuthorisationHeaderType.BindingTariffInformation },
			{ UniversalReferenceConstants.RefCusCodeList.Codes.Code_C627, EU.Business.UniversalReferenceConstants.CusAuthorisationHeaderType.BindingOriginInformation }
		}.ToImmutableDictionary();
	}
}
