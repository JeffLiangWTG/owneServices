using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules.Internal;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class ZGridGuidFindBoxTest : TestCaseWithFactory
	{
		public void TestGuid()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummyChild1 = dummy1.Collection.AddNew();
			dummyChild1.Z0_Code = "Bob";

			var dummy2 = Factory.New<DummyBusinessObject>();
			var dummyChild2 = dummy2.Collection.AddNew();
			dummyChild2.Z0_Code = "Bob";

			Factory.Save();

			using (var findBox = new ZGridGuidFindBox())
			{
				findBox.ModuleID = DummyModuleIDs.Dummy;

				findBox.CodeBox.Text = string.Empty;
				AssertEquals(ZGuid.Empty, findBox.Guid);

				var popupDecisionProvider = new PopupModuleDecisionProvider(findBox);
				popupDecisionProvider.HandleFindBoxOKButton(new[] { dummyChild2 });
				AssertEquals(dummyChild2.PK, findBox.Guid);

				findBox.List = dummy1.Collection;
				findBox.CodeBox.Text = "XXX";
				AssertEquals(ZGuid.Invalid, findBox.Guid);

				findBox.CodeBox.Text = "Bob";
				AssertEquals(dummyChild1.PK, findBox.Guid);
			}
		}

		public void TestGuid_IsUpdatedOnListChange()
		{
			var dummy1 = Factory.New<DummyWithDependentsBusinessObject>();
			var dummyChild1 = dummy1.Dependents.AddNew();
			dummyChild1.ZD1_Code = "Bob";

			var dummy2 = Factory.New<DummyWithDependentsBusinessObject>();
			var dummyChild2 = dummy2.Dependents.AddNew();
			dummyChild2.ZD1_Code = "Bob";

			Factory.Save();

			using (var findBox = new ZGridGuidFindBox())
			{
				findBox.ModuleID = DummyModuleIDs.Dummy;
				findBox.CodeBox.Text = "Bob";

				findBox.List = dummy1.Dependents;
				AssertEquals(dummyChild1.PK, findBox.Guid);

				findBox.List = dummy2.Dependents;
				AssertEquals(dummyChild2.PK, findBox.Guid);
			}
		}

		public void TestCacheIsDisabled()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummyChild1 = dummy1.Collection.AddNew();
			dummyChild1.Z0_Code = "Bob";

			Factory.Save();

			using (var findBox = new ZGridGuidFindBox())
			{
				findBox.IsGuidCacheEnabled = false;

				findBox.ModuleID = DummyModuleIDs.Dummy;
				findBox.CodeBox.Text = string.Empty;
				AssertEquals(ZGuid.Empty, findBox.Guid);

				var popupDecisionProvider = new PopupModuleDecisionProvider(findBox);
				popupDecisionProvider.HandleFindBoxOKButton(new[] { dummyChild1 });
				AssertEquals(ZGuid.Empty, findBox.Guid);

				var guidCache = findBox.GetType().GetField("guidCache", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(findBox);

				AssertNull(guidCache);
			}
		}

		public void TestGetBizObjsToEditOrView()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummyChild1 = dummy1.Collection.AddNew();
			dummyChild1.Z0_Code = "Bob";

			var dummy2 = Factory.New<DummyBusinessObject>();
			var dummyChild2 = dummy2.Collection.AddNew();
			dummyChild2.Z0_Code = "Bob";

			var dummy3 = Factory.New<DummyBusinessObject>();
			dummy3.Z0_Guid = dummyChild2.PK;

			using (var findBox = new ZGridGuidFindBoxForTest())
			{
				findBox.List = dummy1.Collection;
				findBox.CodeBox.Text = "Bob";
				AssertContainsExactElementsInAnyOrder(dummyChild1, findBox.GetBizObjsToEditOrView_Exposed());

				findBox.CurrentItem = dummy3;
				findBox.DataPropertyName = nameof(DummyBusinessObject.Z0_Guid);
				AssertContainsExactElementsInAnyOrder(dummyChild2, findBox.GetBizObjsToEditOrView_Exposed());
			}
		}

		class ZGridGuidFindBoxForTest : ZGridGuidFindBox
		{
			public IEnumerable<BusinessObject> GetBizObjsToEditOrView_Exposed()
			{
				return GetBizObjsToEditOrView();
			}
		}
	}
}
