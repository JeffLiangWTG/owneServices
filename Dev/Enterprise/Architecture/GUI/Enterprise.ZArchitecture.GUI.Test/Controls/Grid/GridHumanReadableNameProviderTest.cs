using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class GridHumanReadableNameProviderTest : TestCaseWithDummy
	{
		public void TestGetHumanReadableNameForGridHumanReadableNameProvider()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var child1 = dummy.Collection.AddNew();
			var child2 = dummy.Collection.AddNew();
			var child3 = dummy.Collection.AddNew();
			using (var form = new ZForm(dummy))
			using (var grid = new ZGrid())
			{
				grid.ColumnStyles.Add(
						new ZTextBoxColumnStyleInfo
						{
							ColumnName = AutoDummyBizo.Schema.Z0_VarCharMax,
							Caption = "Default VarCharMax"
						}
						);
				grid.ColumnStyles.Add
					(
					new ZTextBoxColumnStyleInfo
					{
						ColumnName = AutoDummyBizo.Schema.Z0_Description,
						Caption = "Default Description"
					}
					);
				form.Controls.Add(grid);
				grid.CopyCaptionsToPropertyHumanReadableNameForTest = true;
				grid.CopyColumnCaptionsToBoundFields = true;
				grid.SetDataBinding(null, "");
				grid.SetDataBinding(dummy, "Collection");
				grid.TableStyles[0].GridColumnStyles[DummyChildBusinessObject.Schema.Z0_VarCharMax].HeaderText = "New Caption";
				GridHumanReadableNameProvider serviceProvider;
				serviceProvider = new GridHumanReadableNameProvider(grid);
				AssertEquals("New Caption", serviceProvider.GetHumanReadableName(child1.Z0_VarCharMaxInfo));
				AssertEquals("New Caption", serviceProvider.GetHumanReadableName(child2.Z0_VarCharMaxInfo));
				AssertEquals("New Caption", serviceProvider.GetHumanReadableName(child3.Z0_VarCharMaxInfo));
				grid.SetDataBinding(null, "");
			}
		}

		public void TestHumanReadbleNamesArePropagatedToWrappedProperties()
		{
			var parent = Factory.New<DummyBusinessObject>();
			var dummy = Factory.New<DummyBusinessObject>();
			var wrappedDummy = new DummyBusinessObjectWrapperWithInfo(dummy);
			parent.Collection.Add(wrappedDummy);
			dummy = Factory.New<DummyBusinessObject>();
			wrappedDummy = new DummyBusinessObjectWrapperWithInfo(dummy);
			parent.Collection.Add(wrappedDummy);
			using (var form = new ZForm(parent))
			using (var grid = new ZGrid())
			{
				grid.ColumnStyles.Add(
						new ZTextBoxColumnStyleInfo
						{
							ColumnName = "WrappedZ0_Code",
							Caption = "Default Code"
						}
						);
				grid.ColumnStyles.Add
					(
					new ZTextBoxColumnStyleInfo
					{
						ColumnName = "WrappedZ0_Description",
						Caption = "Default Description"
					}
					);
				form.Controls.Add(grid);
				grid.CopyCaptionsToPropertyHumanReadableNameForTest = true;
				grid.CopyColumnCaptionsToBoundFields = true;
				grid.SetDataBinding(null, "");
				grid.SetDataBinding(parent, "Collection");
				form.Show();
				grid.TableStyles[0].GridColumnStyles["WrappedZ0_Description"].HeaderText = "New Caption";
				AssertEquals("New Caption", dummy.Z0_DescriptionInfo.HumanReadableName);
				AssertEquals("New Caption", wrappedDummy.WrappedZ0_DescriptionInfo.HumanReadableName);
				grid.TableStyles[0].GridColumnStyles["WrappedZ0_Description"].HeaderText = "Different Caption";
				AssertEquals("Different Caption", dummy.Z0_DescriptionInfo.HumanReadableName);
				AssertEquals("Different Caption", wrappedDummy.WrappedZ0_DescriptionInfo.HumanReadableName);
				grid.TableStyles[0].GridColumnStyles["WrappedZ0_Description"].HeaderText = "Caption Three";
				AssertEquals("Caption Three", wrappedDummy.WrappedZ0_DescriptionInfo.HumanReadableName);
				AssertEquals("Caption Three", dummy.Z0_DescriptionInfo.HumanReadableName);
				grid.SetDataBinding(null, "");
			}
		}

		#region TestGetHumanReadableNameForGridHumanReadableNameProviderPerformance

		public void TestGetHumanReadableNameForGridHumanReadableNameProviderPerformance()
		{
			var parent = Factory.New<DummyBusinessObject>();
			for (var i = 0; i < 1000; i++)
			{
				parent.Collection.Add(CreateDummyWithRelated());
			}

			using (var form = new ZForm(parent))
			using (var grid = new ZGrid())
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "Z0_Code", Caption = "Column 1" });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "RelatedDummy+Z0_Code", Caption = "Column 2" });
				grid.CopyCaptionsToPropertyHumanReadableNameForTest = true;
				grid.CopyColumnCaptionsToBoundFields = true;
				form.Controls.Add(grid);

				grid.SetDataBinding(parent, "Collection");
				form.Show();
				Application.DoEvents();

				foreach (DummyBusinessObject dummy in parent.Collection)
				{
					AssertEquals("Column 1", dummy.Z0_CodeInfo.HumanReadableName);
					AssertEquals("Column 2", dummy.RelatedDummy.Z0_CodeInfo.HumanReadableName);
				}

				var dummyNotInCollection = CreateDummyWithRelated();
				AssertEquals("Element is not shown in the grid and has no matching column with same property and bizo type - use default caption", "Code", dummyNotInCollection.Z0_CodeInfo.HumanReadableName);
				AssertEquals("Use matching column with same property and bizo type", "Column 2", dummyNotInCollection.RelatedDummy.Z0_CodeInfo.HumanReadableName);

				grid.SetDataBinding(null, "");
			}
		}

		public DummyBusinessObject CreateDummyWithRelated()
		{
			var dummy = Factory.New<DummyWithRelatedBizo>();
			var related = Factory.New<Dummy2Bizo>();
			dummy.Z0_Guid = related.PK;
			AssertSame(related, dummy.RelatedDummy);
			AssertEquals(typeof(Dummy2Bizo), dummy.RelatedDummy.GetType());
			return dummy;
		}

		class DummyWithRelatedBizo : DummyBusinessObject
		{
			public DummyWithRelatedBizo(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public override DummyBusinessObject RelatedDummy
			{
				get { return Factory.Load<Dummy2Bizo>(Z0_Guid); }
			}
		}

		class Dummy2Bizo : DummyBusinessObject
		{
			public Dummy2Bizo(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		#endregion
	}
}
