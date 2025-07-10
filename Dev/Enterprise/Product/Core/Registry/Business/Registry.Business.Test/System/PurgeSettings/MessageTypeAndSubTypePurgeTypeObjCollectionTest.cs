using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(MessageTypeAndSubTypePurgeTypeObjCollection))]
	sealed class MessageTypeAndSubTypePurgeTypeObjCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<MessageTypeAndSubTypePurgeTypeObjCollection>
	{
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override MessageTypeAndSubTypePurgeTypeObjCollection GetCollectionToTest()
		{
			return new MessageTypeAndSubTypePurgeTypeObjCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new MessageTypeObj();
		}
	}
}
