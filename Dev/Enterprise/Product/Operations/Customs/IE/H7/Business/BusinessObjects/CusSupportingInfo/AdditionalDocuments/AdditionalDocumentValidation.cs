using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Customs.IE.Business.Constants;

namespace Enterprise.Customs.IE.H7.Business
{
	public class AdditionalDocumentValidation : EU.H7.Business.AdditionalDocumentValidation
	{
		public AdditionalDocumentValidation(AdditionalDocument parent)
			: base(parent)
		{
		}

		protected new AdditionalDocument Parent => (AdditionalDocument)base.Parent;

		#region CheckCSI_Code

		protected override void CheckCSI_Code()
		{
			var codeValue = Parent.CSI_Code;
			var referenceNumberValue = Parent.CSI_ReferenceNumber;
			var propertyInfo = Parent.CSI_CodeInfo;

			if (Parent.CSI_ParentTableCode == AsycudaPackedItemSchema.Constants.Prefix)
			{
				if (Parent.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference)
				{
					if (InvalidDocumentCodes.Contains(Parent.CSI_Code))
					{
						propertyInfo.AddMessageError(Parent.ValidationMessage.GetBR2040RuleMessage());
					}
					if (codeValue.IsEmpty && !referenceNumberValue.IsEmpty)
					{
						propertyInfo.AddMessageError(Parent.ValidationMessage.GetBR2048RuleMessage(propertyInfo));
					}
				}
				ListValidation.MessageErrorIfInvalidCode(propertyInfo, Parent.ValidationMessage.InvalidValueRuleMessage);
			}
			else if (Parent is CusSupportingInfo { Parent: AsycudaBill bill })
			{
				CheckCSI_Code_LV2(bill);
				CheckCSI_Code_LV1AndLV2(bill);
			}
			else
			{
				base.CheckCSI_Code();
			}
		}

		void CheckCSI_Code_LV2(AsycudaBill bill)
		{
			if (bill.Header.IsLV2)
			{
				ValidateRequiredTypeForTransportDocument(bill);
				ValidateInvalidAuthorizationType();
				Validate1D24TypeRequiredForAdditionalReference();
				ValidateNoDuplicatedID24Type(bill);
				ValidateInvalidDocumentType();
			}
		}

		void CheckCSI_Code_LV1AndLV2(AsycudaBill bill)
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo, Parent.ValidationMessage.InvalidValueRuleMessage);
			ValidateRequiredTypeForAdditionalReference(bill);
			ValidateEmptyValue(Parent.CSI_CodeInfo, Res.GetString("2f81df2e-e187-4c48-8958-d8d327c3d3ff", "You have not entered a type."));
		}

		void ValidateRequiredTypeForTransportDocument(AsycudaBill bill)
		{
			if (IsNotAValidTransportDocument() && ItemsDoNotHaveAValidTransportDocument(bill))
			{
				Parent.CSI_CodeInfo.AddMessageError(Parent.ValidationMessage.GetBR1106RuleMessage());
			}
		}

		bool IsNotAValidTransportDocument() => Parent.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument && !AcceptedTypesForTransportDocunment.Contains(Parent.CSI_Code);

		public static bool ItemsDoNotHaveAValidTransportDocument(AsycudaBill bill) => !bill.PackedItems.SelectMany(a => a.AdditionalDocuments).Any(a => a.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument && AcceptedTypesForTransportDocunment.Contains(a.CSI_Code));

		void ValidateInvalidAuthorizationType()
		{
			ValidateInvalidType(InvalidAuthorizationCodes, Parent.ValidationMessage.GetBR600006RuleMessage());
		}

		void Validate1D24TypeRequiredForAdditionalReference()
		{
			if (Parent.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference && Parent.CSI_Code != Constants.SupportingDocumentCodes._1D24)
			{
				Parent.CSI_CodeInfo.AddMessageError(Parent.ValidationMessage.GetBR20319Rule1D24REFRequiredMessage());
			}
		}

		void ValidateNoDuplicatedID24Type(AsycudaBill bill)
		{
			if (bill.AdditionalDocuments.Count(a => a.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference && a.CSI_Code == Constants.SupportingDocumentCodes._1D24) >= 2)
			{
				Parent.CSI_CodeInfo.AddMessageError(Parent.ValidationMessage.GetBR20319RuleOnlyOneREF1D24Message());
			}
		}

		void ValidateInvalidDocumentType()
		{
			ValidateInvalidType(InvalidDocumentCodes, Parent.ValidationMessage.GetBR2040RuleMessage());
		}

		void ValidateInvalidType(List<string> invalidCodes, string errorMessage)
		{
			if (Parent.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference && invalidCodes.Contains(Parent.CSI_Code))
			{
				Parent.CSI_CodeInfo.AddMessageError(errorMessage);
			}
		}

		void ValidateRequiredTypeForAdditionalReference(AsycudaBill bill)
		{
			if (bill.ABL_Procedure == EU.H7.Business.EUH7AdditionalProcedureCodeList.Codes.C07F49 && Parent.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference && Parent.CSI_Code != Constants.AdditionalReferenceCodes._1A06)
			{
				Parent.CSI_CodeInfo.AddMessageError(Parent.ValidationMessage.GetBR600008RuleMessage());
			}
		}

		#endregion

		#region CheckCSI_ReferenceNumber

		protected override void CheckCSI_ReferenceNumber()
		{
			var codeValue = Parent.CSI_Code;
			var referenceNumberValue = Parent.CSI_ReferenceNumber;
			var propertyInfo = Parent.CSI_ReferenceNumberInfo;

			if (Parent.CSI_ParentTableCode == AsycudaPackedItemSchema.Constants.Prefix)
			{
				if (Parent.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference)
				{
					if (referenceNumberValue.IsEmpty && !codeValue.IsEmpty)
					{
						propertyInfo.AddMessageError(Parent.ValidationMessage.GetBR2048RuleMessage(propertyInfo));
					}
				}
				if (referenceNumberValue.Length > 70)
				{
					var maxLengthErrorMessage = Res.GetString("4255eee9-3d77-472d-8be9-3cfa4d29e613", "Reference number length cannot exceed 70 alphanumeric characters.");
					propertyInfo.AddMessageError(maxLengthErrorMessage);
				}
			}
			else if (Parent is CusSupportingInfo { Parent: AsycudaBill bill })
			{
				CheckCSI_ReferenceNumber_LV2(bill);
				CheckCSI_ReferenceNumber_LV1AndLV2();
			}
			else
			{
				base.CheckCSI_ReferenceNumber();
			}
		}

		void CheckCSI_ReferenceNumber_LV2(AsycudaBill bill)
		{
			if (bill.Header.IsLV2)
			{
				ValidateFormatForN741();
				ValidateFormatFor1D96();
				ValidateFormatFor1D24();
			}
		}

		void CheckCSI_ReferenceNumber_LV1AndLV2()
		{
			ValidateEmptyValue(Parent.CSI_ReferenceNumberInfo, Res.GetString("463109b9-0ccd-4ffc-8a1c-728cd3e21972", "You have not entered a reference number."));
		}

		void ValidateFormatForN741()
		{
			ValidateReferenceFormat(AdditionalInfoSubTypeList.Codes.TransportDocument, Constants.TransportDocumentCodes._N741, IsValidFormatForN741, Parent.ValidationMessage.GetBR2038RuleMessage());
		}

		void ValidateFormatFor1D96()
		{
			ValidateReferenceFormat(AdditionalInfoSubTypeList.Codes.AdditionalReference, Constants.SupportingDocumentCodes._1D96, IsValidFormatFor1D96, Parent.ValidationMessage.GetBR2316RuleMessage());
		}

		void ValidateFormatFor1D24()
		{
			ValidateReferenceFormat(AdditionalInfoSubTypeList.Codes.AdditionalReference, Constants.SupportingDocumentCodes._1D24, IsValidFormatFor1D24, Parent.ValidationMessage.GetBR20319RuleReferenceFormatForREF1D24Message());
		}

		void ValidateReferenceFormat(string subType, string code, bool isValidFormat, string errorMessage)
		{
			if (Parent.CSI_SubType == subType && Parent.CSI_Code == code && !isValidFormat)
			{
				Parent.CSI_ReferenceNumberInfo.AddMessageError(errorMessage);
			}
		}

		bool IsValidFormatForN741 => Parent.CSI_ReferenceNumber.Length == 11 && Parent.CSI_ReferenceNumber.IsNumbersOnlyOrEmpty;

		bool IsValidFormatFor1D96 => Parent.CSI_ReferenceNumber == "1";

		bool IsValidFormatFor1D24 => ZDateTime.TryParseExact(Parent.CSI_ReferenceNumber, out _, Constants.DateTimeFormat.AdditionalReferenceDateTime);

		#endregion

		protected override void CheckCSI_SubType()
		{
			base.CheckCSI_SubType();
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_SubTypeInfo);
		}

		void ValidateEmptyValue(ZPropertyInfo info, string errorMessage)
		{
			if (Parent.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference && info.Value.IsEmpty)
			{
				info.AddMessageError(Parent.ValidationMessage.GetBR2048RuleMessage(errorMessage));
			}
		}

		protected readonly List<string> InvalidAuthorizationCodes = [Constants.SupportingDocumentCodes._1D02, Constants.SupportingDocumentCodes._1D03, Constants.SupportingDocumentCodes._1D04, Constants.SupportingDocumentCodes._1Q75];

		protected readonly List<string> InvalidDocumentCodes = [Constants.SupportingDocumentCodes._1A01, Constants.SupportingDocumentCodes._1A05];

		protected static readonly List<string> AcceptedTypesForTransportDocunment = new List<string> { Constants.TransportDocumentCodes._N235, Constants.TransportDocumentCodes._N271, Constants.TransportDocumentCodes._N703, Constants.TransportDocumentCodes._N704, Constants.TransportDocumentCodes._N705,
			Constants.TransportDocumentCodes._N710, Constants.TransportDocumentCodes._N714, Constants.TransportDocumentCodes._N720, Constants.TransportDocumentCodes._N722, Constants.TransportDocumentCodes._N730, Constants.TransportDocumentCodes._N740, Constants.TransportDocumentCodes._N741,
			Constants.TransportDocumentCodes._N750, Constants.TransportDocumentCodes._N760, Constants.TransportDocumentCodes._N785, Constants.TransportDocumentCodes._N787, Constants.TransportDocumentCodes._N952, Constants.TransportDocumentCodes._N955 };
	}
}
