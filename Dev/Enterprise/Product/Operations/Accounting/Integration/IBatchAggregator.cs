using System;

namespace Enterprise.Accounting.Integration
{
	public interface IBatchAggregator
	{
		bool Aggregate();
		Exception LastException { get; }
		string AggregateResult { get; set; }
	}
}
