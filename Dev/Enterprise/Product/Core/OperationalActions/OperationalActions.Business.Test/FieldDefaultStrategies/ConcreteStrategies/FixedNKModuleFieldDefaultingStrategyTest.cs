using System.Collections;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class FixedNKModuleFieldDefaultingStrategyTest : TestCaseWithFactory
	{
		public void TestDetail()
		{
			OperationalActionNKModuleFieldSupporter supporter = new OperationalActionNKModuleFieldSupporter("fieldName", false, 5, (f) => new RefUNLOCOCollection(f));
			FixedNKModuleFieldDefaultingStrategy strategy = new FixedNKModuleFieldDefaultingStrategy(supporter);
			AssertEquals(FieldType.TextCodeFindBox, strategy.DetailFieldType);
			AssertEquals(5, strategy.DetailMaxLength);
			AssertEquals("AUBNE", strategy.GetDefaultValue("AUBNE"));
			AssertEquals("NLAMS", strategy.GetDefaultValue("NLAMS"));
			IList boundCollection = strategy.GetBoundCollection(Factory);
			AssertType(typeof(RefUNLOCOCollection), boundCollection);
			AssertEquals(Factory, ((RefUNLOCOCollection)boundCollection).Factory);
		}
	}
}
