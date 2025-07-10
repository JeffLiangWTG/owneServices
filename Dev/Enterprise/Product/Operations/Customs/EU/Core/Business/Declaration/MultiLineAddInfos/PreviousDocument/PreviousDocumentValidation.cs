using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public class PreviousDocumentValidation : Customs.Business.CusSupportingInfoValidation
	{
		public PreviousDocumentValidation(PreviousDocument parent)
			: base(parent)
		{
		}

		protected new PreviousDocument Parent => (PreviousDocument)base.Parent;

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();

			if (Parent.CSI_Code.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_CodeInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo);
			}
			Parent.Validation.ValidateCSI_DateOfIssue();
		}

		protected override void CheckCSI_SubType()
		{
			base.CheckCSI_SubType();

			if (Parent.CSI_SubType.IsEmpty && IsSubTypeMandatory)
			{
				Parent.CSI_SubTypeInfo.AddMessageError(SubTypeEmpty);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CSI_SubTypeInfo, Parent.Lookups.SubTypeList);
			}
		}

		public virtual string SubTypeEmpty => Res.GetString("1308A6AE-95C0-4323-8EEA-A650BB34C7DF", "Please enter a Class.");

		protected virtual bool IsSubTypeMandatory => true;

		const string ReferenceDateOfEntryOfTheGoodsInTheRecordsCode = "CLE";

		protected override void CheckCSI_DateOfIssue()
		{
			base.CheckCSI_DateOfIssue();

			if (Parent.CSI_Code == ReferenceDateOfEntryOfTheGoodsInTheRecordsCode)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_DateOfIssueInfo);
			}
		}
	}
}
