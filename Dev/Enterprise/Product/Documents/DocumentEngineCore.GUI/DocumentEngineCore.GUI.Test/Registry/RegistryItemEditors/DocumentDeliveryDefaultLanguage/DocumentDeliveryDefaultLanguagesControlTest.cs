using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.GUI.Registry.Testing
{
	[TestedType(typeof(DocumentDeliveryDefaultLanguagesControl))]
	sealed class DocumentDeliveryDefaultLanguagesControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			var collection = new DocumentDeliveryDefaultLanguagesCollection();
			collection.AddNew();
			return collection;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((DocumentDeliveryDefaultLanguagesControl)control).ReadOnly;
		}
	}
}
