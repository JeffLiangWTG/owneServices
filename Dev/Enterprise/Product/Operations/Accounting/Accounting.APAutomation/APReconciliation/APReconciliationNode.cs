namespace Enterprise.Accounting.APAutomation.APReconciliation
{
	public class APReconciliationNode
	{
		public APReconciliationNode(string nodeName, IAPReconciliationLineProvider lineProvider)
		{
			Name = nodeName;
			LineProvider = lineProvider;
		}

		public string Name { get; }
		public IAPReconciliationLineProvider LineProvider { get; }
	}
}
