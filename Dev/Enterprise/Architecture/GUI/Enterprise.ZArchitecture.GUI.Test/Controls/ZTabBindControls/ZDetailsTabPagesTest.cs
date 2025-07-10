using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZDetailsTabPagesTest : TestCaseWithDummy
	{
		public void TestTabHeadingChanging()
		{
			var testParent = Factory.New<ATestDummyParent>();

			using (var testForm = new ATestForm(testParent))
			{
				testForm.Show();
				Application.DoEvents();

				AssertEquals("Tab Text", "Dummy Dummy1 Thing", testForm.ADetailsTabPage.Text);

				testForm.AGrid.ListManager.Position = 1;
				AssertEquals("Tab Text", "Dummy Dummy2 Thing", testForm.ADetailsTabPage.Text);

				testForm.AGrid.ListManager.Position = 2;
				AssertEquals("Tab Text", "Dummy Dummy3 Thing", testForm.ADetailsTabPage.Text);

				testForm.AGrid.ListManager.Position = 0;
				AssertEquals("Tab Text", "Dummy Dummy1 Thing", testForm.ADetailsTabPage.Text);

				testParent.Dummies[0].Delete();
				AssertEquals("Tab Text", "Dummy Dummy2 Thing", testForm.ADetailsTabPage.Text);

				testParent.Dummies[0].Delete();
				AssertEquals("Tab Text", "Dummy Dummy3 Thing", testForm.ADetailsTabPage.Text);

				testParent.Dummies[0].Delete();
				AssertEquals("Tab Text", "Thing", testForm.ADetailsTabPage.Text);
			}
		}

		class ATestForm : ZForm
		{
			public ZTabControl ATabControl = new ZTabControl();
			public ZDetailsTabPage ADetailsTabPage = new ZDetailsTabPage();
			public ZGrid AGrid = new ZGrid();

			public ATestForm(IBusiness businessEntity)
				: base(businessEntity)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				AGrid.Columns.AddTextColumn("Z0_VarCharMax", 200);
				AGrid.BindTo = "Dummies";
				Controls.Add(AGrid);

				ATabControl.TabPages.Insert(ADetailsTabPage, 0);
				ATabControl.Location = new Point(200, 0);
				Controls.Add(ATabControl);

				ADetailsTabPage.AdditionalText = "Thing";
			}

			protected override void OnShown(EventArgs e)
			{
				base.OnShown(e);

				ADetailsTabPage.Hook(null); //this should not blow up.
				ADetailsTabPage.Hook(AGrid.ListManager);
			}
		}

		class ATestDummy : DummyBusinessObject, IDetailsTabPageHeadingProvider
		{
			public ATestDummy(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region IDetailsTabPageHeadingProvider Members

			public string Heading
			{
				get
				{
					return "Dummy " + this.Z0_VarCharMax;
				}
			}

			#endregion
		}

		class ATestDummyCollection : BusinessObjectCollection<ATestDummy>
		{
			public ATestDummyCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}

		class ATestDummyParent : DummyBusinessObject
		{
			public ATestDummyParent(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
				fDummies = new ATestDummyCollection(factory);

				var dummy1 = fDummies.AddNew(typeof(ATestDummy));
				dummy1.Z0_VarCharMax = "Dummy1";

				var dummy2 = fDummies.AddNew(typeof(ATestDummy));
				dummy2.Z0_VarCharMax = "Dummy2";

				var dummy3 = fDummies.AddNew(typeof(ATestDummy));
				dummy3.Z0_VarCharMax = "Dummy3";
			}

			public ATestDummyCollection Dummies
			{
				get { return fDummies; }
			}
			readonly ATestDummyCollection fDummies;
		}
	}
}
