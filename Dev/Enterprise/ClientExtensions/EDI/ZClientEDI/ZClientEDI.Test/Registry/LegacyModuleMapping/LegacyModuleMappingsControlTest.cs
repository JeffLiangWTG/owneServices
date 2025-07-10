using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.CustomerService.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(LegacyModuleMappingsControl))]
	class LegacyModuleMappingsControlTest : RegistryZUserControlTestCase
	{
		public void TestModuleMappingCaption()
		{
			using (var control = new LegacyModuleMappingsControl())
			{
				control.ModuleMappingCaption = "Apple Mapping";
				AssertEquals("Apple Mapping", control.MappingGrid.ColumnStyles.Cast<ZGridColumnInfo>().First(col => col.ColumnName == "ModuleMapping").Caption);
				AssertEquals("Apple Mapping Description", control.MappingGrid.ColumnStyles.Cast<ZGridColumnInfo>().First(col => col.ColumnName == "ModuleMappingDescription").Caption);
			}
		}

		#region Implementation
		protected override IBusiness GetNewBusinessEntity()
		{
			return new LegacyModuleMappingCollection(ModuleListType.MenuSection);
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((LegacyModuleMappingsControl)control).MappingGrid.ReadOnly;
		}
		#endregion
	}
}
