using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(SystemProductRegistryControl))]
	class SystemProductRegistryControlTest : RegistryZUserControlTestCase
	{
		public void TestModuleCaption()
		{
			using (var control = new SystemProductRegistryControl())
			{
				control.ModuleCaption = "Apple";
				AssertEquals("Apple Mappings", control.ModuleMappingsLabel.Text);
				AssertEquals("Apple Code", control.ModuleGrid.ColumnStyles.Cast<ZGridColumnInfo>().First(col => col.ColumnName == "ModuleCode").Caption);
				AssertEquals("Apple Description", control.ModuleGrid.ColumnStyles.Cast<ZGridColumnInfo>().First(col => col.ColumnName == "ModuleDescriptionMultilingual").Caption);
			}
		}

		#region Implementation
		protected override IBusiness GetNewBusinessEntity()
		{
			return new SystemProductCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control1, IBusiness businessEntity)
		{
			var control = (SystemProductRegistryControl)control1;
			return control.ModuleGrid.ReadOnly && control.SourceModuleGrid.ReadOnly && control.ProductsGrid.ReadOnly;
		}
		#endregion
	}
}
