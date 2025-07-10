using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public static class SpecialComparisonOperator
	{
		public static SQLComparisonOperator IsBlank => SQLComparisonOperator.IsBlank;

		public static SQLComparisonOperator IsNotBlank => SQLComparisonOperator.IsNotBlank;
	}
}
