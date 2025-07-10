using System;

namespace CargoWise.EntityFramework
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class RowFetchStrategyAttribute : Attribute
	{
		public Type FetchStrategyType { get; set; }
	}
}
