using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Internal.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class BaseZGuidFindBoxTest : BaseZCodeFindBoxTest
	{
		public void TestCodeAndDescriptionSetFromRetrieve()
		{
			CreateDummies();
			Dummy.SS_DummyGuid = Dummy1.PK;

			using (var testForm = new ZChildForm())
			{
				CreateControls(testForm, "SS_Dummy");

				testForm.Show();

				AssertEquals("List after Show", Dummy.Dummies, ZGuidFindBox.List);
				AssertEquals("Code after Show", "AABCD", ZGuidFindBox.CodeBox.Text);
				UserIdleWorker.Flush();
				AssertEquals("Description after Show", "AABCD Description", ZGuidFindBox.DescriptionBox.Text);
			}
		}

		public void TestBindingAfterShow()
		{
			CreateDummies();
			Dummy.SS_DummyGuid = Dummy1.PK;

			using (var testForm = new ZChildForm())
			{
				testForm.Show();
				CreateControls(testForm, "");

				AssertEquals("List after Show", Dummy.Dummies, ZGuidFindBox.List);
				AssertEquals("Code after Show", "AABCD", ZGuidFindBox.CodeBox.Text);
				UserIdleWorker.Flush();
				AssertEquals("Description after Show", "AABCD Description", ZGuidFindBox.DescriptionBox.Text);
			}
		}

		public void TestCodeAndDescriptionSetFromInputAndValidate()
		{
			DeleteAllDummies();
			CreateDummies();
			Dummy = Factory.New<FindBoxDummyBusinessObject>();

			using (var testForm = new ZChildForm())
			{
				CreateControls(testForm, "");
				testForm.Show();
				FindBox.Focus();

				AssertEquals("List after Show", Dummy.Dummies, ZGuidFindBox.List);
				AssertEquals("Code after Show", "", ZGuidFindBox.CodeBox.Text);
				UserIdleWorker.Flush();
				AssertEquals("Description after Show", "{None Selected}", ZGuidFindBox.DescriptionBox.Text);

				SendKeyPressToCodeBox('A');
				SendKeyPressToCodeBox('=');
				UserIdleWorker.Flush();

				AssertEquals("Code set FindBox after AutoComplete", "AABCD", ZGuidFindBox.CodeBox.Text);
				AssertEquals("Description of AABCD", "AABCD Description", ((IFindBoxListProvider)FindBox).DescriptionFromCode("AABCD"));
				AssertEquals("Description set FindBox after AutoComplete", "AABCD Description", ZGuidFindBox.DescriptionBox.Text);
				AssertEquals("Code set in BusinessObject before changing focus", ZGuid.Empty, Dummy.SS_DummyGuid);

				ChangeFocusToInvokeBinding();

				AssertEquals("Code set in BusinessObject after changing focus", Dummy1.PK, Dummy.SS_DummyGuid);
				AssertEquals("Code set FindBox after changing focus", "AABCD", ZGuidFindBox.CodeBox.Text);
				AssertEquals("Code set FindBox after changing focus", "AABCD Description", ZGuidFindBox.DescriptionBox.Text);
			}
		}

		public void TestReadOnly()
		{
			using (var testForm = new ZChildForm())
			{
				CreateControls(testForm, "");
				testForm.Show();

				AssertEquals("Initial FindBox DescriptionBox ReadOnly state", true, ZGuidFindBox.DescriptionBox.ReadOnly);
				AssertEquals("Initial FindBox CodeBox ReadOnly state", false, ZGuidFindBox.CodeBox.ReadOnly);

				Dummy.SS_DummyGuid_ReadOnly = true;

				AssertEquals("FindBox DescriptionBox ReadOnly state after setting ReadOnly on BizObj to true", true, ZGuidFindBox.DescriptionBox.ReadOnly);
				AssertEquals("FindBox CodeBox ReadOnly state after setting ReadOnly on BizObj to true", true, ZGuidFindBox.CodeBox.ReadOnly);

				Dummy.SS_DummyGuid_ReadOnly = false;

				AssertEquals("FindBox DescriptionBox ReadOnly state after setting ReadOnly on BizObj to false", true, ZGuidFindBox.DescriptionBox.ReadOnly);
				AssertEquals("FindBox CodeBox ReadOnly state after setting ReadOnly on BizObj to false", false, ZGuidFindBox.CodeBox.ReadOnly);
			}
		}

		public void TestBindToList()
		{
			using (var testForm = new ZChildForm())
			{
				CreateControls(testForm, "");
				testForm.Show();

				AssertEquals("FindBox List after binding", Dummy.Dummies, FindBox.List);
			}
		}

		#region Implementation

		public bool IsBindingToDetail;

		protected override ZFindBoxUserControl NewFindBoxTester
		{
			get { return new ZGuidFindBox(); }
		}

		protected virtual ZGuidFindBox ZGuidFindBox
		{
			get { return (ZGuidFindBox)FindBox; }
		}

		protected override void SetBindTo(ZFindBoxUserControl findBox)
		{
			if (IsBindingToDetail)
			{
				findBox.BindTo = "Collection.Z0_Guid";
				findBox.BindToList = "Dummies";
			}
			else
			{
				findBox.BindTo = "SS_DummyGuid";
				findBox.BindToList = "Dummies";
			}
			if (!string.IsNullOrEmpty(BindToForDescription))
			{
				((ZCodeFindBox)findBox).BindToForDescription = BindToForDescription;
			}
		}

		protected override void SetValueBoundToToGetAABCDTestValues()
		{
			Dummy.SS_DummyGuid = Dummy1.PK;
		}

		protected override void ClearValueBoundTo()
		{
			Dummy.SS_DummyGuid = ZGuid.Empty;
		}

		#endregion
	}
}
