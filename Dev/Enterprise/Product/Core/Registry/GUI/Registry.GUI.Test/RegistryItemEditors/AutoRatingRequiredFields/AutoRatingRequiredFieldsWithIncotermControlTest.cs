using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AutoRatingRequiredFieldsWithIncotermControl))]
	sealed class AutoRatingRequiredFieldsWithIncotermControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new AutoRatingRequiredFields();
		}
	}
}
