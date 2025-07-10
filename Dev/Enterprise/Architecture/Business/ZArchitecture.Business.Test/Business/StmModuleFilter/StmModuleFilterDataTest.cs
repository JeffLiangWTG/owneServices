using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(StmModuleFilterUserData))]
	sealed class StmModuleFilterDataTest : EnterpriseBusinessObjectTestCase
	{
		#region TestLayout

		public void TestLayout()
		{
			StmModuleFilterUserData layoutUserData = Factory.New<StmModuleFilterUserData>();
			StmModuleFilter layout = Factory.New<StmModuleFilter>();
			layoutUserData.S0_S9 = layout.PK;

			AssertEquals(layout, layoutUserData.Layout);
		}

		#endregion

		public void TestCustomLogReferenceSuffix()
		{
			var layout = Factory.NewWithValidTestData<StmModuleFilter>();
			layout.S9_FilterName = "filter1";
			var layoutUserData = Factory.New<StmModuleFilterUserDataForTest>();

			layoutUserData.S0_S9 = Guid.NewGuid();
			AssertEquals("When S0_S9 is incorrect CustomLogReferenceSuffix should return blank rule name", layoutUserData.GetCustomLogReferenceSuffix(), "Rule Name: '(Unknown)'");

			layoutUserData.S0_S9 = layout.PK;
			AssertEquals("When S0_S9 is correct CustomLogReferenceSuffix should return rule name filter1", layoutUserData.GetCustomLogReferenceSuffix(), "Rule Name: 'filter1'");
		}

		#region TestInitialise

		public void TestInitialise()
		{
			ZGuid staffPk = ZGuid.NewZGuid();
			DummyFilterStripLayoutsHelper helper = new DummyFilterStripLayoutsHelper();
			helper.CurrentUserPkForTest = staffPk;
			helper.CurrentUserTablePrefixForTest = "XX";

			LayoutData.Initialise(helper);
			AssertEquals(staffPk, LayoutData.S0_RelatedEntityID);
			AssertEquals("XX", LayoutData.S0_RelatedEntityTableCode);
		}

		#endregion

		#region TestSupportsClone

		public void TestSupportsClone()
		{
			StmModuleFilterUserData userData = Factory.New<StmModuleFilterUserData>();
			Assert(userData.SupportsClone());
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewLayoutUserData(factory);
		}

		StmModuleFilterUserData GetNewLayoutUserData(BusinessObjectFactory factory)
		{
			StmModuleFilterUserData result = factory.New<StmModuleFilterUserData>();
			StmModuleFilter filter = factory.New<StmModuleFilter>();
			filter.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			result.S0_S9 = filter.PK;
			result.S0_RelatedEntityID = ZGuid.NewZGuid();
			filter.S9_RelatedEntityID = result.S0_RelatedEntityID;
			return result;
		}

		StmModuleFilterUserData LayoutData
		{
			get { return fLayoutData ?? (fLayoutData = GetNewLayoutUserData(Factory)); }
		}

		StmModuleFilterUserData fLayoutData;

		class StmModuleFilterUserDataForTest : StmModuleFilterUserData
		{
			public StmModuleFilterUserDataForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZString GetCustomLogReferenceSuffix()
			{
				return CustomLogReferenceSuffix;
			}
		}
		#endregion
	}
}
