using System.Collections;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class FixedPKModuleFieldDefaultingStrategyTest : TestCaseWithFactory
	{
		public void TestDetail()
		{
			ZGuid guid1 = ZGuid.NewZGuid();
			ZGuid guid2 = ZGuid.NewZGuid();
			OperationalActionPKModuleFieldSupporter supporter = new OperationalActionPKModuleFieldSupporter("fieldName", false, (f) => new RefUNLOCOCollection(f));
			FixedPKModuleFieldDefaultingStrategy strategy = new FixedPKModuleFieldDefaultingStrategy(supporter);
			AssertEquals(FieldType.Guid, strategy.DetailFieldType);
			AssertEquals(guid1.ToString().Length, strategy.DetailMaxLength);
			AssertEquals(guid1, strategy.GetDefaultValue(guid1.ToString()));
			AssertEquals(guid2, strategy.GetDefaultValue(guid2.ToString()));
			IList boundCollection = strategy.GetBoundCollection(Factory);
			AssertType(typeof(RefUNLOCOCollection), boundCollection);
			AssertEquals(Factory, ((RefUNLOCOCollection)boundCollection).Factory);
		}
	}
}
