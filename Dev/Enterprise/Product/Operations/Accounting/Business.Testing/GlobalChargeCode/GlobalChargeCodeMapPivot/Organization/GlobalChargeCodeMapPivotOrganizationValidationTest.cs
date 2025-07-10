namespace Enterprise.Accounting.Business.GlobalChargeCode.Testing
{
	using CargoWise.EntityFramework;

	internal class GlobalChargeCodeMapPivotOrganizationValidationTest : AccGlobalChargeCodeMapPivotValidationTest
	{
		protected override BusinessObjectCollection GetGlobalChargeCodePivotCollection
		{
			get
			{
				return new GlobalChargeCodeMapPivotOrganizationCollection(Factory);
			}
		}
	}
}