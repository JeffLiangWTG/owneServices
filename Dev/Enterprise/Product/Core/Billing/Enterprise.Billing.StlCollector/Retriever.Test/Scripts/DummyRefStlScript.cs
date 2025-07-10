using System;
using CargoWise.Billing.Collectors;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts
{
	sealed class DummyRefStlScript : IRefStlScript
	{
		public string ActiveOn => throw new NotImplementedException();
		public string FeatureCode => throw new NotImplementedException();
		public string FeatureName => throw new NotImplementedException();
		public string DataGranularity => RefStlItemGrain.Transactional;
		public string TransactionDateUtc => throw new NotImplementedException();
		public string GuidReference => throw new NotImplementedException();
		public string TransactionCount => throw new NotImplementedException();
		public string FromClause => throw new NotImplementedException();
		public string RoleName => throw new NotImplementedException();
		public string ModuleName => throw new NotImplementedException();
		public string FunctionName => throw new NotImplementedException();
		public string CompanyCode => throw new NotImplementedException();
		public string BranchCode => throw new NotImplementedException();
		public string BillingReference1 => throw new NotImplementedException();
		public string BillingReference2 => throw new NotImplementedException();
		public string BillingReference3 => throw new NotImplementedException();
		public string BillingReference4 => throw new NotImplementedException();
		public string WhereClause => throw new NotImplementedException();
		public string CreatingUserCode => throw new NotImplementedException();
		public string AdditionalRefs => throw new NotImplementedException();
		public string PreparationScript => throw new NotImplementedException();
		public bool WithOptionRecompile => throw new NotImplementedException();
		public bool UsedInBilling => false;
		public string MinCW1Version => throw new NotImplementedException();
		public string MaxCW1Version => throw new NotImplementedException();
		public string DateType => throw new NotImplementedException();
		public DateTime CollectionStartDateUtc => throw new NotImplementedException();
	}
}
