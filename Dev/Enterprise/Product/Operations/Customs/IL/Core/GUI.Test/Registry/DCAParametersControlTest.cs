using CargoWise.EntityFramework;
using Enterprise.Customs.IL.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IL.GUI.Testing
{
	[TestedType(typeof(DCAParametersControl))]
	sealed class DCAParametersControlTest : RegistryZUserControlTestCase
	{
		public void TestPeekWayZDropEdit()
		{
			using (var control = new DCAParametersControl())
			{
				var peekWayZDropEdit = control.FindSingle<ZDropEdit>("peekWayZDropEdit");
				AssertNotNull(peekWayZDropEdit);
			}
		}

		public void TestAllServicesRadioButton()
		{
			using (var control = new DCAParametersControl())
			{
				var allServicesZRadioButton = control.FindSingle<ZRadioButton>("allServicesZRadioButton");
				AssertEquals("All Services radio button should be visible", true, allServicesZRadioButton.Visible);
			}
		}

		public void TestSpecificServicesRadioButton()
		{
			using (var control = new DCAParametersControl())
			{
				var specificServicesZRadioButton = control.FindSingle<ZRadioButton>("specificServicesZRadioButton");
				AssertEquals("Specific Services radio button should be visible", true, specificServicesZRadioButton.Visible);
			}
		}

		public void TestServiceNamesGrid()
		{
			using (var control = new DCAParametersControl())
			{
				var serviceNamesZDisplayGrid = control.FindSingle<ZDisplayGrid>("serviceNamesZDisplayGrid");
				AssertEquals("Service Names grid should be visible", true, serviceNamesZDisplayGrid.Visible);
				AssertEquals("Service Names grid should have only a column with names", 1, serviceNamesZDisplayGrid.ColumnStyles.Count);
				var nameColumn = serviceNamesZDisplayGrid.ColumnStyles[0] as ZTextBoxColumnStyleInfo;
				AssertNotNull("Service Names grid's column should be a ZTextBoxColumnStyleInfo", nameColumn);
				AssertEquals("Service Names grid's column should be visible", true, nameColumn.IsVisible);
				AssertEquals("Service Names grid's column should be bounded to the property 'Name'", "Name", nameColumn.ColumnName);
				AssertEquals("Service Names grid's column width must be as expected", 320, nameColumn.Width);
			}
		}

		public void TestServiceNamesGrid_WhenAllServicesChanges()
		{
			using (var zForm = new ZForm())
			using (var control = new DCAParametersControl())
			{
				zForm.Controls.Add(control);
				var serviceNamesZDisplayGrid = control.FindSingle<ZDisplayGrid>("serviceNamesZDisplayGrid");
				var allServicesZRadioButton = control.FindSingle<ZRadioButton>("allServicesZRadioButton");

				zForm.Show();

				allServicesZRadioButton.Checked = false;
				AssertEquals("Service Names grid should not be readonly", false, serviceNamesZDisplayGrid.ReadOnly);

				allServicesZRadioButton.Checked = true;
				AssertEquals("Service Names grid should be readonly", true, serviceNamesZDisplayGrid.ReadOnly);
			}
		}

		public void TestServiceNamesGrid_WhenBindingAllServicesTrue()
		{
			using (var zForm = new ZForm())
			using (var control = new DCAParametersControl())
			{
				zForm.Controls.Add(control);
				var serviceNamesZDisplayGrid = control.FindSingle<ZDisplayGrid>("serviceNamesZDisplayGrid");

				zForm.Show();

				var dCAParameters = new DCAParameters();
				zForm.SetDataBinding(dCAParameters, "");
				AssertEquals("Service Names grid should be readonly", false, serviceNamesZDisplayGrid.ReadOnly);
			}
		}

		public void TestServiceNamesGrid_WhenBindingAllServicesFalse()
		{
			using (var zForm = new ZForm())
			using (var control = new DCAParametersControl())
			{
				zForm.Controls.Add(control);
				var serviceNamesZDisplayGrid = control.FindSingle<ZDisplayGrid>("serviceNamesZDisplayGrid");

				zForm.Show();

				var dCAParameters = new DCAParameters()
				{
					AllServices = false
				};
				zForm.SetDataBinding(dCAParameters, "");
				AssertEquals("Service Names grid should not be readonly", false, serviceNamesZDisplayGrid.ReadOnly);
			}
		}

		public void TestMaxMessagesIntEdit()
		{
			using (var control = new DCAParametersControl())
			{
				var maxMessagesZIntEdit = control.FindSingle<ZIntEdit>("maxMessagesZIntEdit");
				AssertEquals("Max Messages should be visible", true, maxMessagesZIntEdit.Visible);
			}
		}

		protected override IBusiness GetNewBusinessEntity() => new DCAParameters();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => control.ReadOnly;
	}
}
