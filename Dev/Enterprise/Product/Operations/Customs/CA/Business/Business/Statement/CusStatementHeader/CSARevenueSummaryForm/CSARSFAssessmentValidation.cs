using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CSARSFAssessmentValidation : AutoCSARSFAssessmentValidation
	{
		public CSARSFAssessmentValidation(AutoCSARSFAssessment parent) : base(parent)
		{
		}

		protected override void CheckType()
		{
			base.CheckType();
			ListValidation.WarnIfInvalidCode(Parent.TypeInfo);
		}

		protected override void CheckReferenceNumber()
		{
			base.CheckReferenceNumber();
			var parent = Parent as CSARSFAssessment;
			var referenceNumber = parent.ReferenceNumber;
			if (!referenceNumber.IsEmpty)
			{
				var type = parent.Type;
				if (type == CustomsAssessmentsCodes.Codes.B2Dash1)
				{
					if (!referenceNumber.IsNumbersOnlyOrEmpty || referenceNumber.Length != TransactionNumber.Schema.FormattedTransactionNumberMaxLength)
					{
						parent.ReferenceNumberInfo.AddWarning(Res.GetString("0015F004-F9F4-4D91-BDCE-54D84F8F44D5", "Reference Number should be composed of 14 digits numbers."));
					}
				}
				else if (type == CustomsAssessmentsCodes.Codes.K23
					|| type == CustomsAssessmentsCodes.Codes.PAAndCP)
				{
					if (!referenceNumber.IsNumbersOnlyOrEmpty || referenceNumber.Length > 10)
					{
						parent.ReferenceNumberInfo.AddWarning(Res.GetString("F1F1C7B8-54F2-482E-B63F-00F4208ECFA5", "Reference Number should be number and up to 10 digits."));
					}
				}
				else if (type == CustomsAssessmentsCodes.Codes.K100B
					|| type == CustomsAssessmentsCodes.Codes.K9
					|| type == CustomsAssessmentsCodes.Codes.K25
					|| type == CustomsAssessmentsCodes.Codes.K29)
				{
					if (!referenceNumber.IsLettersAndNumbersOnlyOrEmpty || referenceNumber.Length > 17)
					{
						parent.ReferenceNumberInfo.AddWarning(Res.GetString("CB35C6D3-EA6E-49BA-A140-FEEF711B7AB8", "Reference Number should be alpha/number and up to 17 digits."));
					}
				}
			}
		}
	}
}
