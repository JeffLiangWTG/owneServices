using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Declaration
{
	public class CDSCusEntryLineValidation : CusEntryLineValidation
	{
		public CDSCusEntryLineValidation(CusEntryLine parent)
		: base(parent)
		{
		}

		protected override void CheckCL_CustomsPostedStatus()
		{
			base.CheckCL_CustomsPostedStatus();

			if (Parent.CL_CustomsPostedStatus == Customs.Business.EntryLineStatusList.Codes.DeletePending)
			{
				Parent.CL_CustomsPostedStatusInfo.AddMessageError("It is not allowed to add or remove Entry Lines when the Entry is (pre-)lodged with CDS");
			}
		}
	}
}
