using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(TemplateRegistryItemControl))]
	public class TemplateRegistryItemControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new TemplateCriteria();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var checkbox = (ZCheckBox)control.Controls.Find("EnabledCheckBox", true).Single();

			return checkbox.ReadOnly;
		}
	}
}
