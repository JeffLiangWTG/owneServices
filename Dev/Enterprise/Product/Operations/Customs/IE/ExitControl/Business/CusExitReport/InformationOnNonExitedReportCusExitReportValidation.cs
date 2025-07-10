using CargoWise.EntityFramework;
using UniversalReferenceConstants = Enterprise.Customs.IE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class InformationOnNonExitedReportCusExitReportValidation : CusExitReportValidation
	{
		public InformationOnNonExitedReportCusExitReportValidation(CusExitReport parent) : base(parent)
		{
		}

		protected override void CheckCER_EnquiryInformationCode()
		{
			base.CheckCER_EnquiryInformationCode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CER_EnquiryInformationCodeInfo);
			if (Parent.CER_EnquiryInformationCode == UniversalReferenceConstants.ExitReportEnquiryInformationCodeTypes.Codes.ExitedAlternativeEvidence)
			{
				if (Parent.AlternativeEvidences.Count == 0)
				{
					Parent.CER_EnquiryInformationCodeInfo.AddMessageError(Res.GetString("648D36A8-4469-4B12-88D4-C09CA2DAF874", "Alternative Evidence must be provided for this report"));
				}
			}
			else if (Parent.AlternativeEvidences.Count > 0)
			{
				Parent.CER_EnquiryInformationCodeInfo.AddMessageError(Res.GetString("C63EA7DC-2BFD-4002-98AB-091E37886C29", "Alternative Evidence is not allowed for this report"));
			}
		}

		protected override void CheckCER_DeclarantType()
		{
			if (!Parent.Representative.IsEmpty)
			{
				if (Parent.CER_DeclarantType.IsEmpty)
				{
					MandatoryValidation.AddYouHaveNotEnteredMessage(Parent.CER_DeclarantTypeInfo);
				}
				else if (Parent.CER_DeclarantType != EU.Business.RepresentationTypeList.Codes._2Direct)
				{
					Parent.CER_DeclarantTypeInfo.AddMessageError(Res.GetString("0858306F-497B-46B7-8B7D-CB887570EAF1", "Only 'DIR - Direct Representation' is valid for this report"));
				}
			}
		}

		protected override void CheckCER_OfficeOfExit()
		{
			if (Parent.CER_OfficeOfExit.IsEmpty)
			{
				if (Parent.CER_EnquiryInformationCode == UniversalReferenceConstants.ExitReportEnquiryInformationCodeTypes.Codes.ExitedNoAlternativeEvidence)
				{
					MandatoryValidation.AddYouHaveNotEnteredMessage(Parent.CER_OfficeOfExitInfo);
				}
			}
			else if (Parent.CER_EnquiryInformationCode == UniversalReferenceConstants.ExitReportEnquiryInformationCodeTypes.Codes.WillNotExit || Parent.CER_EnquiryInformationCode == UniversalReferenceConstants.ExitReportEnquiryInformationCodeTypes.Codes.ExpectedToExit)
			{
				var info = Parent.CER_OfficeOfExitInfo;
				info.AddMessageError(MandatoryValidation.DoNotEnterMessage(info.Description));
			}
		}

		protected override void CheckCER_DateTime()
		{
			if (Parent.CER_DateTime.IsEmpty)
			{
				if (Parent.IsExitDateRequired)
				{
					MandatoryValidation.AddYouHaveNotEnteredMessage(Parent.CER_DateTimeInfo);
				}
			}
			else if (!Parent.IsExitDateRequired)
			{
				var info = Parent.CER_DateTimeInfo;
				info.AddMessageError(MandatoryValidation.DoNotEnterMessage(info.Description));
			}
		}

		protected override void CheckCER_OfficeOfExport()
		{
			base.CheckCER_OfficeOfExport();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CER_OfficeOfExportInfo);
		}
	}
}
