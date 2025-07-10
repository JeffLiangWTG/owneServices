using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.CN.Module
{
	public sealed class EntryHeaderOperationalActionSupporter : Customs.Module.EntryHeaderOperationalActionSupporter
	{
		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.CNCusEntry);
		}
	}
}
