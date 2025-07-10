using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.CA.Module.OperationalActions
{
	public class CADeclarationOperationalActionMethodProvider : OperationalActionMethodProvider
	{
		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			if (actionSupporter is LVXOperationalActionSupporter)
			{
				return new OperationalActionMethod[]
				{
					new LVXConsolidationOperationalActionMethod(),
					new LVXDeConsolidationOperationalActionMethod()
				};
			}
			else
			{
				return new OperationalActionMethod[]
				{
					new CAB3OperationalActionMethod(),
					new CARNSQueryOperationalActionMethod(),
					new MergeEntriesOperationalActionMethod()
				};
			}
		}
	}
}
