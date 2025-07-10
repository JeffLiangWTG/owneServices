using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business
{
	public class CusExitDetailValidation : AutoCusExitDetailValidation
	{
		public CusExitDetailValidation(AutoCusExitDetail parent) : base(parent)
		{
		}

		protected virtual bool IsStatusMandatory => true;

		new CusExitDetail Parent => (CusExitDetail)base.Parent;

		protected override void CheckCED_CustomsOffice()
		{
			base.CheckCED_CustomsOffice();
			ListValidation.MessageErrorIfInvalidCode(Parent.CED_CustomsOfficeInfo);
		}

		protected override void CheckCED_Status()
		{
			base.CheckCED_Status();

			var parent = Parent;
			var status = parent.CED_Status;
			var statusInfo = parent.CED_StatusInfo;
			if (IsStatusMandatory)
			{
				MandatoryValidation.CheckEntered(statusInfo);
			}

			if (!status.IsEmpty && status.Length != 3)
			{
				statusInfo.AddError(Res.GetString("CB692C18-65D0-4CCB-8746-083346059690", "Status length must be 3"));
			}
		}

		protected override void CheckCED_ArrivalNotificationDate()
		{
			base.CheckCED_ArrivalNotificationDate();
			if (Parent.CED_ArrivalNotificationDate < ZDate.Today)
			{
				Parent.CED_ArrivalNotificationDateInfo.AddWarning(Res.GetString("EDC6C5B5-CD29-46D2-A187-F60E803D9D3F", "Notification Date is older than the current date"));
			}
		}
	}
}
