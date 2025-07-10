using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class ZDescriptionGridFindBoxTest : BaseFindBoxTest
	{
		public void TestCodeForFinding()
		{
			var dummy = CreateDummyCodes();

			using (var testForm = new ZForm(dummy))
			using (var descriptionFindBox = new ZDescriptionGridFindBoxForTest())
			{
				descriptionFindBox.BindTo = "Code";
				testForm.Controls.Add(descriptionFindBox);

				testForm.Show();
				Application.DoEvents();

				descriptionFindBox.FindBoxExposed.Code = "Aaa Aaa";
				AssertNotNull(descriptionFindBox.GetBizObjsToEditOrViewExposed().FirstOrDefault());
				AssertEquals("A1", descriptionFindBox.CodeForFindingExposed);

				descriptionFindBox.FindBoxExposed.Code = "Bbb Bbb";
				AssertEquals("B2", descriptionFindBox.CodeForFindingExposed);

				descriptionFindBox.FindBoxExposed.Code = "XYZ";
				AssertEquals("?", descriptionFindBox.CodeForFindingExposed);

				descriptionFindBox.FindBoxExposed.Code = "A1";
				AssertEquals("?", descriptionFindBox.CodeForFindingExposed);
			}
		}

		public void TestGetBizObjsToEditOrView()
		{
			var dummy = CreateDummyCodes();

			using (var testForm = new ZForm(dummy))
			using (var descriptionFindBox = new ZDescriptionGridFindBoxForTest())
			{
				descriptionFindBox.BindTo = "Code";
				testForm.Controls.Add(descriptionFindBox);

				testForm.Show();
				Application.DoEvents();

				descriptionFindBox.FindBoxExposed.Code = "Aaa Aaa";
				AssertNotNull(descriptionFindBox.GetBizObjsToEditOrViewExposed().FirstOrDefault());

				descriptionFindBox.FindBoxExposed.Code = "Bbb Bbb";
				AssertNotNull(descriptionFindBox.GetBizObjsToEditOrViewExposed().FirstOrDefault());

				descriptionFindBox.FindBoxExposed.Code = "XYZ";
				AssertNull(descriptionFindBox.GetBizObjsToEditOrViewExposed().FirstOrDefault());
			}
		}

		DummyWithCodes CreateDummyCodes()
		{
			var dummy = Factory.New<DummyWithCodes>();

			var dummy1 = dummy.Codes.AddNew();
			dummy1.Z0_Code = "A1";
			dummy1.Z0_Description = "Aaa Aaa";

			var dummy2 = dummy.Codes.AddNew();
			dummy2.Z0_Code = "B2";
			dummy2.Z0_Description = "Bbb Bbb";
			return dummy;
		}

		protected override ZFindBoxUserControl NewFindBoxTester => new ZDescriptionGridFindBoxForTest();

		class ZDescriptionGridFindBoxForTest : ZDescriptionGridFindBox
		{
			public IFindBox FindBoxExposed => IFindBox;

			public string CodeForFindingExposed => CodeForFinding;

			public IEnumerable<BusinessObject> GetBizObjsToEditOrViewExposed()
			{
				return GetBizObjsToEditOrView();
			}
		}
	}
}
