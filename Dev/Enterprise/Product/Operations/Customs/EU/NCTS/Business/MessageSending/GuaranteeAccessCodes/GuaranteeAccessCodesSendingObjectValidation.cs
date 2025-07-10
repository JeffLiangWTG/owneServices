using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class GuaranteeAccessCodesSendingObjectValidation : AutoGuaranteeAccessCodesSendingObjectValidation
	{
		public GuaranteeAccessCodesSendingObjectValidation(AutoGuaranteeAccessCodesSendingObject parent) : base(parent)
		{
		}

		protected new GuaranteeAccessCodesSendingObject Parent => (GuaranteeAccessCodesSendingObject)base.Parent;

		protected override void CheckOfficeOfGuarantee()
		{
			MandatoryValidation.CheckEntered(Parent.OfficeOfGuaranteeInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.OfficeOfGuaranteeInfo, Parent.Lookups.OfficeCodeList);
		}

		protected override void CheckMasterCode()
		{
			MandatoryValidation.CheckEntered(Parent.MasterCodeInfo);
		}

		protected override void CheckCurrentCode()
		{
			MandatoryValidation.CheckEntered(Parent.CurrentCodeInfo);
		}

		protected override void CheckNewAccessCode()
		{
			MandatoryValidation.CheckEntered(Parent.NewAccessCodeInfo);
		}
	}
}
