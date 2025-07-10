using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(MessageTypePurgeTypeObjCollection))]
	sealed class MessageTypePurgeTypeObjCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<MessageTypePurgeTypeObjCollection>
	{
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override MessageTypePurgeTypeObjCollection GetCollectionToTest()
		{
			return new MessageTypePurgeTypeObjCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new MessageTypeObj();
		}
	}
}
