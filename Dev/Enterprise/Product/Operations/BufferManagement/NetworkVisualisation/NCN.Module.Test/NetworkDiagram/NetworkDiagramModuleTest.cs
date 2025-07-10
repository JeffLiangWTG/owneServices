using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Module.Test
{
	[TestedType(typeof(NetworkDiagramModule))]
	class NetworkDiagramModuleTest : ZModuleBasherTest
	{
		#region New Menu Item

		public void TestNewMenuItem_ShouldCreateNonScaledDiagram()
		{
			using (var testForm = new ZForm())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.NetworkDiagram))
			{
				module.SetFormsModalTo(testForm);
				AssertNotNull(module.ToolBarButtons.FindByText("New"));

				AssertEquals("&New", module.NewMenuItem.Text);
				module.NewMenuItem.PerformClick();

				using (var form = Application.OpenForms.OfType<NetworkDiagramForm>().SingleOrDefault())
				{
					AssertNotNull(form);

					var shape = (BMNCNShape)form.BusinessEntity;

					AssertEquals(false, shape.IsScaled);
					AssertEquals("New Diagram", shape.BNS_Name);
					AssertEquals(typeof(BMNCNRootDiagramShape), shape.GetType());
				}
			}
		}

		public void TestNewNonScaledMenuItem_ShouldCreateNonScaledDiagram()
		{
			using (var testForm = new ZForm())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.NetworkDiagram))
			{
				module.SetFormsModalTo(testForm);
				AssertNotNull(module.ToolBarButtons.FindByText("New"));

				AssertEquals("Non-scaled Diagram", module.NewMenuItem.MenuItems[0].Text);
				module.NewMenuItem.MenuItems[0].PerformClick();

				using (var form = Application.OpenForms.OfType<NetworkDiagramForm>().SingleOrDefault())
				{
					AssertNotNull(form);

					var shape = (BMNCNShape)form.BusinessEntity;

					AssertEquals(false, shape.IsScaled);
					AssertEquals("New Diagram", shape.BNS_Name);
					AssertEquals(typeof(BMNCNRootDiagramShape), shape.GetType());
				}
			}
		}

		public void TestNewScaledMenuItem_ShouldCreateScaledDiagram()
		{
			using (var testForm = new ZForm())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.NetworkDiagram))
			{
				module.SetFormsModalTo(testForm);
				AssertNotNull(module.ToolBarButtons.FindByText("New"));

				AssertEquals("Scaled Diagram", module.NewMenuItem.MenuItems[1].Text);
				module.NewMenuItem.MenuItems[1].PerformClick();

				using (var form = Application.OpenForms.OfType<NetworkDiagramForm>().SingleOrDefault())
				{
					AssertNotNull(form);

					var shape = (BMNCNShape)form.BusinessEntity;

					AssertEquals(true, shape.IsScaled);
					AssertEquals("New Scaled Diagram", shape.BNS_Name);
					AssertEquals(new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes(), shape.Scale);
					AssertEquals(new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes(), shape.ResolutionIncrement);
					AssertEquals(typeof(BMNCNRootDiagramShape), shape.GetType());
				}
			}
		}

		public void TestNoNewAndDeleteMenuItem_WhenAdditionalFilterApplied()
		{
			using (var testForm = new ZForm())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.NetworkDiagram))
			{
				module.AddAdditionalDisplayFilter = q => new CargoWise.EntityFramework.ZQuery();
				module.SetFormsModalTo(testForm);
				AssertNull(module.ToolBarButtons.FindByText("New"));
				AssertNull(module.ToolBarButtons.FindByText("Delete"));
			}
		}

		#endregion

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.NetworkDiagram;
		}

		#endregion
	}
}
