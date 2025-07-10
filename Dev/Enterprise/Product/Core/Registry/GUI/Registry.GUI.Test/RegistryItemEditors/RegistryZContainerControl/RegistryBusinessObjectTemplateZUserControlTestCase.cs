using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestsSubclassesOf(typeof(RegistryBusinessObjectTemplateZUserControl))]
	public abstract class RegistryBusinessObjectTemplateZUserControlTestCase : RegistryZUserControlTestCase
	{
		#region Implementation

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((RegistryBusinessObjectTemplate)businessEntity).ReadOnly;
		}

		#endregion
	}
}
