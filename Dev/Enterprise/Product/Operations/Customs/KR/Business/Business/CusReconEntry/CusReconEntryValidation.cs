using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class CusReconEntryValidation(CusReconEntry parent) : AutoKRCusReconEntryValidation(parent)
	{
		new CusReconEntry Parent => (CusReconEntry)base.Parent;
		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateAmendment5WNVersionNumber();
		}
		protected override void CheckCRE_OriginalEntryNumber()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CRE_OriginalEntryNumberInfo);
		}

		protected override void CheckCRE_CustomsBillNumber()
		{
			base.CheckCRE_CustomsBillNumber();
			ListValidation.MessageErrorIfInvalidCode(Parent.CRE_CustomsBillNumberInfo);
		}

		protected override void CheckCRE_OA_DeclarantAddressIsNotEmpty() { }

		public void ValidateAmendment5WNVersionNumber()
		{
			ValidateCalculatedProperty(parent.Amendment5WNVersionNumberInfo);
		}

		protected void CheckAmendment5WNVersionNumber()
		{
			ListValidation.MessageErrorIfInvalidCode(parent.Amendment5WNVersionNumberInfo);
		}
	}
}
