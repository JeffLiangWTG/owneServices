using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.DevTools;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class BizoDiffColumnProviderTest : TestCaseWithFactory
	{
		public void TestGetPropertyList_ExcludeSpecifiedTypesAndNonDbFields()
		{
			var provider = new BizoDiffColumnProvider();
			var obj = Factory.New<BizoDiffDummyBusinessObject>();
			obj.Z0_Guid = Guid.NewGuid();

			var result = provider.GetPropertyList(obj);

			AssertEquals(false, result.Contains("SomeOtherProperty"));
			AssertEquals(false, result.Contains("Z0_Guid"));
			AssertEquals(false, result.Contains("Z0_Geography"));
		}

		public void TestGetPropertyList_GuidShouldNotBeExcludeWhenEmpty()
		{
			var provider = new BizoDiffColumnProvider();
			var obj = Factory.New<BizoDiffDummyBusinessObject>();

			var result = provider.GetPropertyList(obj);

			AssertEquals(false, result.Contains("SomeOtherProperty"));
			AssertEquals(true, result.Contains("Z0_Guid"));
			AssertEquals(false, result.Contains("Z0_Geography"));
		}

		public void TestGetPropertyList_ExcludeAuditFields()
		{
			var provider = new BizoDiffColumnProvider();
			var obj = Factory.New<DummyLogged>();

			var result = provider.GetPropertyList(obj);

			AssertEquals(false, result.Contains("ZL2_SystemCreateTimeUtc"));
			AssertEquals(false, result.Contains("ZL2_SystemLastEditTimeUtc"));
			AssertEquals(false, result.Contains("ZL2_SystemCreateUser"));
			AssertEquals(false, result.Contains("ZL2_SystemLastEditUser"));
		}

		public class BizoDiffDummyBusinessObject : DummyBaseBusinessObject
		{
			public BizoDiffDummyBusinessObject(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public int SomeOtherProperty { get; set; }
		}
	}
}
