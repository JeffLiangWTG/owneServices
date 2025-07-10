using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.CN.Module
{
	public class EntryHeaderOperationalActionMethodProvider : OperationalActionMethodProvider
	{
		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter) => new OperationalActionMethod[]
		{
			new ArchiveEntriesOperationalActionMethod(),
			new SendACDAOperationalActionMethod(),
		};
	}
}
