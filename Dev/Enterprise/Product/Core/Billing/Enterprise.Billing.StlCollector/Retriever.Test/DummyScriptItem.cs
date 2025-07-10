using Enterprise.Billing.StlCollector.Retriever.Scripts;

namespace Enterprise.Billing.StlCollector.Retriever
{
	public class DummyScriptItem : MockScript
	{
		readonly bool isActive;
		readonly string code;

		public DummyScriptItem(bool isActive, string code)
		{
			this.isActive = isActive;
			this.code = code;
		}

		public override string Code => code;
		public override bool IsActive => isActive;
	}
}
