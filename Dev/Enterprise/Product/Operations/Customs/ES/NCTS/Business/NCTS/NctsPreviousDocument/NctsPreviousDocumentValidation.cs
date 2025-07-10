using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsPreviousDocumentValidation : EU.NCTS.Business.NctsPreviousDocumentPhase4Validation
	{
		public NctsPreviousDocumentValidation(NctsPreviousDocument parent)
			: base(parent)
		{
		}

		protected new NctsPreviousDocument Parent => (NctsPreviousDocument)base.Parent;

		const string SumCode = "SUM";

		protected override void CheckCSI_Code()
		{
			if (Parent.CSI_SubType.Equals(PreviousDocumentClassList.Codes.SummaryDeclaration))
			{
				if (!Parent.CSI_Code.Equals(SumCode))
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.CSI_CodeInfo);
				}
			}
			else
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_CodeInfo);
				if (Parent.CSI_Code.Equals(SumCode))
				{
					Parent.CSI_CodeInfo.AddMessageError(Res.GetString("9A659E77-F738-4FA2-8589-C853E180D5E0", "Please do not enter 'SUM' Type when Class is not 'X'"));
				}
			}

			CheckCSI_CodeDH7();
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo);
		}

		void CheckCSI_CodeDH7()
		{
			if (Parent.CSI_Code.Equals(NctsPreviousDocumentTypeCodeList.Codes.MrnOfLowValueDeclaration) && !Parent.CSI_SubType.Equals(PreviousDocumentClassList.Codes.PreviousDocument))
			{
				Parent.CSI_CodeInfo.AddMessageError(Res.GetString("04AE9548-ED42-4995-B47A-285F73B827A1", "Please do not enter 'DH7' Type when Class is not 'Z'"));
			}
		}

		protected override void CheckCSI_SubType()
		{
			if (!Parent.IsPhase5)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_SubTypeInfo);
				ListValidation.MessageErrorIfInvalidCode(Parent.CSI_SubTypeInfo);
			}
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();

			if (Parent.CSI_Code.Equals(NctsPreviousDocumentTypeCodeList.Codes.MrnOfLowValueDeclaration) && !Parent.CSI_ReferenceNumber.Length.Equals(18))
			{
				Parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("8BF86C82-B14B-4295-9DD5-CAA8B6C44F7C", "Reference should be a valid MRN"));
			}
			else if (Parent.RefCusCode != null)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
			}
		}

		protected override void CheckCSI_LineNo()
		{
			base.CheckCSI_LineNo();
			if (!Parent.IsPhase5)
			{
				if (Parent.CSI_SubType.Equals(PreviousDocumentClassList.Codes.SummaryDeclaration) && Parent.CSI_ReferenceNumber.Length == 11)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_LineNoInfo);
					if ((Parent.CSI_LineNo < 1) || (Parent.CSI_LineNo > 99999))
					{
						Parent.CSI_LineNoInfo.AddMessageError(Res.GetString("CDA2AD06-81EA-4FC9-ACA0-B07B28AE09DF", "Line No. should be between {0} and {1}.", 1, 99999));
					}
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.CSI_LineNoInfo);
				}
			}
		}
	}
}
