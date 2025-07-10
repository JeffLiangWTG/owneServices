using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(SystemDefinedOrganisationControl))]
	sealed class UnmatchedOrganisationControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		#region Implementation

		protected override IBusiness GetNewBusinessEntity()
		{
			return new UnmatchedOrganisation();
		}

		#endregion
	}
}
