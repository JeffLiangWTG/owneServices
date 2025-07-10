using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Internal.Testing;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZGuidFindBoxWithSelectedEventWithCollectionTest : ZGuidFindBoxTest
	{
		public void TestZGuidFindBoxWithSelectedEventWithCollection()
		{
			DeleteAllDummies();
			CreateDummies();
			Dummy = Factory.New<FindBoxDummyBusinessObject>();
			Dummy.Dummies.Add(Dummy1);
			Dummy.Dummies.Add(Dummy2);
			using (var testForm = new ZChildForm())
			{
				IsBindingToDetail = true;
				CreateControls(testForm, "");
				testForm.Show();
				ZGuidFindBox.CodeBox.Focus();
				ZGuidFindBox.SelectFromPopupFormWithoutDisplaying();
				var popupCollection = ((ZGuidFindBoxWithSelectedEvent)ZGuidFindBox).ModuleForTest.ModuleDecisionProvider.List;
				AssertNotNull(popupCollection);
				AssertEquals("Should contain one Dummy", 1, popupCollection.Count);
				AssertEquals(DummyBO, popupCollection[0]);
				AssertNotEquals("Lists should be different", ZGuidFindBox.List, popupCollection);
			}
		}

		#region implementation
		protected override ZFindBoxUserControl NewFindBoxTester
		{
			get
			{
				var collection = new DummyBusinessObjectCollection(Factory);
				DummyBO = Factory.New<DummyBusinessObject>();
				DummyBO.Z0_Code = "AABCD";
				DummyBO.Z0_Description = "AABCD Description";
				collection.Add(DummyBO);
				return new ZGuidFindBoxWithSelectedEvent(delegate { return collection; }, codeFindBox) { ModuleID = DummyModuleIDs.Dummy };
			}
		}

		protected override void CreateControls(ZChildForm testForm, string acceptableBindForTextBox)
		{
			base.CreateControls(testForm, "SS_Dummy");
			testForm.Controls.Add(codeFindBox);
		}

		protected override void SetUp()
		{
			base.SetUp();
			codeFindBox = new ZCodeFindBox();
			codeFindBox.ModuleID = DummyModuleIDs.Dummy;
			codeFindBox.BindTo = "SS_Name";
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (codeFindBox != null && !codeFindBox.IsDisposed)
			{
				codeFindBox.Dispose();
			}
		}

		DummyBusinessObject DummyBO;
		ZCodeFindBox codeFindBox;
		#endregion
	}
}
