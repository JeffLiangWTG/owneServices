using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;
using Contants = Enterprise.Customs.IE.Business.Constants.SupportingDocumentCodes;

namespace Enterprise.Customs.IE.H7.Business
{
	public class SupportingDocumentValidation : EU.H7.Business.SupportingDocumentValidation
	{
		public SupportingDocumentValidation(AutoCusSupportingInfo parent)
			: base(parent)
		{
		}

		protected new SupportingDocument Parent => (SupportingDocument)base.Parent;

		#region CheckCSI_Code

		protected override void CheckCSI_Code()
		{
			if (Parent.CSI_ParentTableCode == AsycudaPackedItemSchema.Constants.Prefix && Parent.Parent is AsycudaPackedItem item)
			{
				if (Contants.H7InvalidCodes.Contains(Parent.CSI_Code))
				{
					Parent.CSI_CodeInfo.AddMessageError(Parent.ValidationMessage.GetBR600006RuleMessage());
				}

				if (item.Bill.Header.AMA_ApplicationCode == SubmitTypeList.Codes.V1)
				{
					if (Contants.H7InvalidCodesV1Only.Contains(Parent.CSI_Code))
					{
						Parent.CSI_CodeInfo.AddMessageError(Parent.ValidationMessage.GetBR20318RuleMessage());
					}

					if (Parent.CSI_Code == Contants._1A01 || Parent.CSI_Code == Contants._1A05)
					{
						Parent.CSI_CodeInfo.AddMessageError(Parent.ValidationMessage.GetBR2040RuleMessage());
					}
				}
				else if (Contants.H7ExclusiveCodes.Contains(Parent.CSI_Code))
				{
					ValidatePackedItemExclusiveSupportingDocuments();
				}

				if (Parent.CSI_Code.IsEmpty && !Parent.CSI_ReferenceNumber.IsEmpty)
				{
					Parent.CSI_CodeInfo.AddMessageError(Parent.ValidationMessage.GetBR20313RuleMessage(Parent.CSI_CodeInfo));
				}

				ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo, Parent.ValidationMessage.InvalidValueRuleMessage);
			}
			else if (Parent is CusSupportingInfo { Parent: AsycudaBill bill })
			{
				CheckCSI_Code_LV1AndLV2(bill);
				CheckCSI_Code_LV1(bill);
			}
			else
			{
				base.CheckCSI_Code();
			}
		}

		void CheckCSI_Code_LV1AndLV2(AsycudaBill bill)
		{
			ValidateInvalidCertificateType();
			SupportingDocumentValidationHelper.ValidateRequiredCodesForProcedureNonC08(bill, Parent.CSI_CodeInfo.AddMessageError);
			ValidateInvalidDocumentType();
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo, Parent.ValidationMessage.InvalidValueRuleMessage);
			ValidateEmptyValue(bill, Parent.CSI_CodeInfo);
		}

		void CheckCSI_Code_LV1(AsycudaBill bill)
		{
			if (bill.Header.IsLV1)
			{
				SupportingDocumentValidationHelper.Validate1D24CodeRequired(bill, Parent.CSI_CodeInfo.AddMessageError);
				ValidateNoDuplicated1D24(bill);
				ValidateInvalidAuthorizationType();
			}
		}

		void ValidateInvalidCertificateType()
		{
			ValidateInvalidType(InvalidCertificateCodes, Parent.ValidationMessage.GetBR20318RuleMessage());
		}

		void ValidateInvalidDocumentType()
		{
			ValidateInvalidType(InvalidDocumentCodes, Parent.ValidationMessage.GetBR2040RuleMessage());
		}

		void ValidateNoDuplicated1D24(AsycudaBill bill)
		{
			if (bill.SupportingDocuments.Count(d => d.CSI_Code == Contants._1D24) >= 2)
			{
				Parent.CSI_CodeInfo.AddMessageError(Parent.ValidationMessage.GetBR20319RuleOnlyOneSUP1D24Message());
			}
		}

		void ValidateInvalidAuthorizationType()
		{
			ValidateInvalidType(InvalidAuthorizationCodes, Parent.ValidationMessage.GetBR600006RuleMessage());
		}

		void ValidateInvalidType(List<string> invalidCodes, string errorMessage)
		{
			if (invalidCodes.Contains(Parent.CSI_Code))
			{
				Parent.CSI_CodeInfo.AddMessageError(errorMessage);
			}
		}

		#endregion

		#region CheckCSI_ReferenceNumber

		protected override void CheckCSI_ReferenceNumber()
		{
			if (Parent.CSI_ParentTableCode == AsycudaPackedItemSchema.Constants.Prefix)
			{
				if (Parent.Parent is AsycudaPackedItem item)
				{
					if (item.Bill.Header.AMA_ApplicationCode == SubmitTypeList.Codes.V1)
					{
						if (Parent.CSI_Code == Contants._N741 && (Parent.CSI_ReferenceNumber.Length != CSI_ReferenceNumberN741MaxLength || !Regex.IsMatch(Parent.CSI_ReferenceNumber, @"^[0-9]+$")))
						{
							Parent.CSI_ReferenceNumberInfo.AddMessageError(Parent.ValidationMessage.GetBR2038RuleMessage());
						}

						if (Parent.CSI_Code == Contants._1D96 && Parent.CSI_ReferenceNumber != CSI_ReferenceNumber1D96MandatoryValue)
						{
							Parent.CSI_ReferenceNumberInfo.AddMessageError(Parent.ValidationMessage.GetBR2316RuleMessage());
						}
					}
					else if (Contants.H7ExclusiveCodes.Contains(Parent.CSI_Code) && !DateTime.TryParseExact(Parent.CSI_ReferenceNumber, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out _))
					{
						Parent.CSI_ReferenceNumberInfo.AddMessageError(Parent.ValidationMessage.GetBR20311RuleMessage());
					}
				}

				if (Parent.CSI_ReferenceNumber.IsEmpty && !Parent.CSI_Code.IsEmpty)
				{
					Parent.CSI_ReferenceNumberInfo.AddMessageError(Parent.ValidationMessage.GetBR20313RuleMessage(Parent.CSI_ReferenceNumberInfo));
				}
			}
			else if (Parent is CusSupportingInfo { Parent: AsycudaBill bill })
			{
				CheckCSI_ReferenceNumber_LV1AndLV2(bill);
				CheckCSI_ReferenceNumber_LV1(bill);
			}
			else
			{
				base.CheckCSI_ReferenceNumber();
			}
		}

		void ValidatePackedItemExclusiveSupportingDocuments()
		{
			if (Parent.Parent is AsycudaPackedItem item)
			{
				var otherItemSupportingDocCodes = item.SupportingDocuments.Where(doc => doc != Parent).Select(doc => doc.CSI_Code);
				var otherExclusiveCodes = otherItemSupportingDocCodes.Intersect(Contants.H7ExclusiveCodes);

				if (Parent.CSI_Code == Contants._U165 && otherExclusiveCodes.Any(code => code != Contants._U167) ||
					Parent.CSI_Code == Contants._U167 && otherExclusiveCodes.Any(code => code != Contants._U165) ||
					Parent.CSI_Code != Contants._U165 && Parent.CSI_Code != Contants._U167 && otherExclusiveCodes.Any())
				{
					Parent.CSI_CodeInfo.AddMessageError(Parent.ValidationMessage.GetBR20312RuleMessage());
				}
			}
		}

		void CheckCSI_ReferenceNumber_LV1AndLV2(AsycudaBill bill)
		{
			ValidateFormatForN741();
			ValidateEmptyValue(bill, Parent.CSI_ReferenceNumberInfo);
		}

		void CheckCSI_ReferenceNumber_LV1(AsycudaBill bill)
		{
			if (bill.Header.IsLV1)
			{
				ValidateFormatFor1D24();
				ValidateFormatFor1D96();
			}
		}

		void ValidateFormatForN741()
		{
			ValidateReferenceFormat(Contants._N741, IsValidFormatForN741, Parent.ValidationMessage.GetBR2038RuleMessage());
		}

		void ValidateFormatFor1D96()
		{
			ValidateReferenceFormat(Contants._1D96, IsValidFormatFor1D96, Parent.ValidationMessage.GetBR2316RuleMessage());
		}

		void ValidateFormatFor1D24()
		{
			ValidateReferenceFormat(Contants._1D24, IsValidFormatFor1D24, Parent.ValidationMessage.GetBR20319RuleReferenceFormatForSUP1D24Message());
		}

		void ValidateReferenceFormat(string code, bool isValidFormat, string errorMessage)
		{
			if (Parent.CSI_Code == code && !isValidFormat)
			{
				Parent.CSI_ReferenceNumberInfo.AddMessageError(errorMessage);
			}
		}

		bool IsValidFormatForN741 => Parent.CSI_ReferenceNumber.Length == 11 && Parent.CSI_ReferenceNumber.IsNumbersOnlyOrEmpty;

		bool IsValidFormatFor1D96 => Parent.CSI_ReferenceNumber == "1";

		bool IsValidFormatFor1D24 => ZDateTime.TryParseExact(Parent.CSI_ReferenceNumber, out _, IE.Business.Constants.DateTimeFormat.AdditionalReferenceDateTime);

		#endregion

		void ValidateEmptyValue(AsycudaBill bill, ZPropertyInfo info)
		{
			if (info.Value.IsEmpty)
			{
				info.AddMessageError(Parent.ValidationMessage.GetBR20313RuleMessage(info));
			}
		}

		protected readonly List<string> InvalidCertificateCodes = [Contants._C644, Contants._C640, Contants._C678, Contants._N853, Contants._C100];

		protected readonly List<string> InvalidAuthorizationCodes = [Contants._1D02, Contants._1D03, Contants._1D04, Contants._1Q75];

		protected readonly List<string> InvalidDocumentCodes = [Contants._1A01, Contants._1A05];

		const int CSI_ReferenceNumberN741MaxLength = 11;

		const string CSI_ReferenceNumber1D96MandatoryValue = "1";
	}
}
