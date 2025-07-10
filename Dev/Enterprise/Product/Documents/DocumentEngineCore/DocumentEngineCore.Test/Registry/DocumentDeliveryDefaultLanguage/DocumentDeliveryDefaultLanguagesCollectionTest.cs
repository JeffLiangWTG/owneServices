using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(DocumentDeliveryDefaultLanguagesCollection))]
	public class DocumentDeliveryDefaultLanguagesCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<DocumentDeliveryDefaultLanguagesCollection>
	{
		#region Implementation

		protected override bool RequiresFactory => false;
		protected override bool RequiresFallbackLevel => false;

		protected override DocumentDeliveryDefaultLanguagesCollection GetCollectionToTest()
		{
			return new DocumentDeliveryDefaultLanguagesCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DocumentDeliveryDefaultLanguages { Fallback = Constants.DocumentDeliveryDefaultLanguagesFallbackType.System, Order = 1 };
		}

		#endregion
	}
}
