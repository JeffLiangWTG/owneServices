using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.FR.Module
{
	public class EntryHeaderOperationalActionSupporter : Customs.Module.EntryHeaderOperationalActionSupporter
	{
		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.FRCusEntry);
		}
	}
}
