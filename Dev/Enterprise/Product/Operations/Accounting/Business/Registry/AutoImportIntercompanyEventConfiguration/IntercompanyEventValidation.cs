using System.Linq;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Res = Enterprise.Accounting.Business.Res;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	public class IntercompanyEventValidation
	{
		public IntercompanyEventValidation(IntercompanyEventSetting parent)
		{
			Parent = parent;
		}

		readonly IntercompanyEventSetting Parent;

		public void ValidateStmEventCode()
		{
			Parent.StmEventCodeInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(Parent.StmEventCodeInfo, (IMultilingualString)ResString.GetMultilingualString("5C3049AA-5AD9-4AE0-A316-93803A7942EB", "Event Code"));

			if (Parent.ParentCollection != null && Parent.ParentCollection.Cast<IntercompanyEventSetting>().Any(x => x.StmEventCode == Parent.StmEventCode && x.PK != Parent.PK))
			{
				Parent.StmEventCodeInfo.AddError(Res.GetString("823D1225-B5DA-4242-B05D-91C2422325F7", "Same Event not allowed more than once. Please select another Event."));
			}

			ListValidation.ErrorIfInvalidCode(Parent.StmEventCodeInfo, Parent.EventsList);
		}

		public void ValidateStartDate()
		{
			Parent.StartDateInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(Parent.StartDateInfo, (IMultilingualString)ResString.GetMultilingualString("7665890B-9BE9-4EF0-BEE1-15F98753D6C8", "Start Date"));

			if (!Parent.StartDate.IsEmpty && !Parent.StartDate.IsValid)
			{
				Parent.StartDateInfo.AddError(Res.GetString("A3C498D5-26EA-44CB-99DF-13104CB9DC64", "Please enter a valid date."));
			}
		}
	}
}
