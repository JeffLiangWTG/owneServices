using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(MessageSubTypePurgeTypeObjCollection))]
	sealed class MessageSubTypePurgeTypeObjCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<MessageSubTypePurgeTypeObjCollection>
	{
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override MessageSubTypePurgeTypeObjCollection GetCollectionToTest()
		{
			return new MessageSubTypePurgeTypeObjCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new MessageTypeObj();
		}
	}
}
