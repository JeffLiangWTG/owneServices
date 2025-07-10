using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(UOMPackTypeControl))]
	sealed class UOMPackTypeControlTest : RegistryZUserControlTestCase
	{
		#region Implementation

		protected override IBusiness GetNewBusinessEntity()
		{
			return new UOMPackTypeCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var typesControl = (UOMPackTypeControl)control;
			return typesControl.PackTypeGrid.ReadOnly;
		}

		#endregion
	}
}
