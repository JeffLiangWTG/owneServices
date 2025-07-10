using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class ZCodeFindBoxPopupTest : FindBoxTestFramework
	{
		public void TestBindingWorksBasedOnAttributesForCodeAndDescription()
		{
			DeleteAllDummies();
			CreateLotsOfDummies();

			List = Dummy.Dummies;

			using (var popup = new ZCodeFindBoxPopupTester())
			{
				using (var testForm = new ZForm(Dummy))
				{
					popup.SetList(List);
					popup.Grid.SetDataBinding(List, "");

					using (var findBox = new ZCodeFindBox())
					{
						findBox.List = Dummy.Collection;
						popup.ShowModal(findBox, testForm);
						popup.LoadList();

						AssertEquals("2 columns - code & description", 2, popup.Grid.TableStyles[0].GridColumnStyles.Count);

						AssertNotNull("Code column should be bound", popup.Grid.TableStyles[0].GridColumnStyles[0].PropertyDescriptor);
						AssertNotNull("Description column should be bound", popup.Grid.TableStyles[0].GridColumnStyles[1].PropertyDescriptor);
					}
				}
			}
		}

		public void TestModalThrowsExceptionWhenPropertyIsNotBound()
		{
			DeleteAllDummies();
			CreateLotsOfDummies();
			const string errorMessage = @"Error: Cannot show popup due to internal exception. 
Table Styles.Count: 1
Column Styles.Count: 2
Columns With Null Property Descriptor: 
Name: Code, Style: Enterprise.ZArchitecture.ZTextBoxColumnStyle
Name: Description, Style: Enterprise.ZArchitecture.ZTextBoxColumnStyle
Parent Type: Enterprise.ZArchitecture.GUI.ZForm
Parent Caption: 
FindBox Type: Enterprise.ZArchitecture.GUI.ZCodeFindBox
FindBox.ListProvider Type: Enterprise.ZArchitecture.GUI.ZCodeFindBox
FindBox.ListProvider.List Type: CargoWise.EntityFramework.Testing.DummyBusinessObjectCollection
FindBox.ListProvider.List TypeOfElements: System.RuntimeType";
			List = Dummy.Dummies;
			try
			{
				using (var popup = new ZCodeFindBoxPopupTester())
				{
					using (var testForm = new ZForm(Dummy))
					{
						popup.SetList(List);
						popup.Grid.SetDataBinding(List, "");

						using (var findBox = new ZCodeFindBox())
						{
							findBox.List = List;
							popup.ShowModal(findBox, testForm);
							AssertEquals("Error should be reported", errorMessage, ErrorReporter.LastMessageReported);
							AssertEquals("Popup cannot be shown due to internal error", UnitTestUserNotification.Instance.LastMessage.Text);
						}
					}
				}
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestAcceptSelection()
		{
			using (var popup = new ZCodeFindBoxPopupTester())
			{
				AssertNoExceptionThrown("AcceptedSelection() does not throw NRE", () => { popup.AcceptSelectionExposed(); });
			}
		}

		public void TestLoadList()
		{
			DeleteAllDummies();
			CreateLotsOfDummies();

			List = Dummy.Dummies;

			using (var popup = new ZCodeFindBoxPopupTester())
			{
				popup.SetList(List);
				popup.Grid.SetDataBinding(List, "", List.GetType().Name);

				popup.Show();
				popup.LoadList();

				AssertEquals("FindBoxPopup populates 100", 100, List.Count);
				AssertEquals("First Element Code", "000", List[0].Z0_Code);
				AssertEquals("Last Element Code", "099", List[LastIndex].Z0_Code);
			}
		}

		public void TestLoadListWithFilter()
		{
			Dummy.DummiesFilter = new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.GreaterThan, "019");

			CreateLotsOfDummies();
			List = Dummy.Dummies;

			using (var popup = new ZCodeFindBoxPopupTester())
			{
				popup.SetList(List);
				popup.Grid.SetDataBinding(List, "", List.GetType().Name);

				popup.Show();
				popup.LoadList();

				AssertEquals("FindBoxPopup populates 100", 100, List.Count);
				AssertEquals("First Element Code", "020", List[0].Z0_Code);
				AssertEquals("Last Element Code", "119", List[LastIndex].Z0_Code);

				popup.CodeTextBox.Text = "0";
				popup.LoadList();

				AssertEquals("First Element Code", "020", List[0].Z0_Code);
				AssertEquals("Last Element Code", "099", List[LastIndex].Z0_Code);

				popup.DescriptionTextBox.Text = "9";
				popup.LoadList();

				AssertEquals("First Element Code", "029", List[0].Z0_Code);
				AssertEquals("Last Element Code", "099", List[LastIndex].Z0_Code);
			}
		}

		#region Implementation

		#region Support Classes

		protected class ZCodeFindBoxPopupTester : ZCodeFindBoxPopup
		{
			public new void LoadList()
			{
				base.LoadList();
			}

			public void SetList(BusinessObjectCollection list)
			{
				this.List = list;
			}

			public new ZGrid Grid
			{
				get { return base.Grid; }
			}

			public new TextBox CodeTextBox
			{
				get { return base.CodeTextBox; }
			}

			public new TextBox DescriptionTextBox
			{
				get { return base.DescriptionTextBox; }
			}

			public void AcceptSelectionExposed()
			{
				AcceptSelection();
			}
		}

		#endregion

		void CreateLotsOfDummies()
		{
			for (var index = 0; index < 120; index++)
			{
				var newDummy = Factory.New<DummyBusinessObject>();
				newDummy.Z0_Code = index.ToString("000");
				newDummy.Z0_Description = newDummy.Z0_Code + " Description";
			}
		}

		DummyBusinessObjectCollection List;
		int LastIndex
		{
			get { return List.Count - 1; }
		}

		#endregion
	}
}
