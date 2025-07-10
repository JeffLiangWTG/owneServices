using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IN.Business.Helpers;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing.Helper
{
	[TestedType(typeof(CleanupHelper))]
	public sealed class CleanupHelperTest : TestCaseWithFactory
	{
		public void TestCleanupIfNotApplicable()
		{
			BusinessObject.StringValue = "Test";
			BusinessObject.NumericValue = new ZDecimal(1);

			CleanupHelper.CleanupIfNotApplicable(true, BusinessObject.StringValueInfo);
			AssertEquals("No CleanUp when feild isAplicable", "Test", BusinessObject.StringValue);

			CleanupHelper.CleanupIfNotApplicable(false, BusinessObject.StringValueInfo);
			AssertEquals("CleanUp when feild is not Aplicable", ZString.Empty, BusinessObject.StringValue);

			CleanupHelper.CleanupIfNotApplicable(true, BusinessObject.NumericValueInfo);
			AssertEquals("No CleanUp when feild isAplicable", new ZDecimal(1), BusinessObject.NumericValue);

			CleanupHelper.CleanupIfNotApplicable(false, BusinessObject.NumericValueInfo);
			AssertEquals("CleanUp when feild is not Aplicable", new ZDecimal(0), BusinessObject.NumericValue);
		}

		BusinessObjectForTesting BusinessObject => businessObject ?? (businessObject = new BusinessObjectForTesting(Factory));
		BusinessObjectForTesting businessObject;

		class BusinessObjectForTesting : NonPersistentBusinessObject
		{
			internal BusinessObjectForTesting(BusinessObjectFactory factory) : base(factory) { }

			public ZString StringValue { get; set; }

			public ZPropertyInfo StringValueInfo => GetZPropertyInfo(nameof(StringValue));

			public ZDecimal NumericValue { get; set; }

			public ZPropertyInfo NumericValueInfo => GetZPropertyInfo(nameof(NumericValue));
		}
	}
}
