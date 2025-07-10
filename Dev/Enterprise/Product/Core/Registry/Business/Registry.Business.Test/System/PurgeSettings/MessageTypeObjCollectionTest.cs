using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(MessageTypeObjCollection))]
	sealed class MessageTypeObjCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<MessageTypeObjCollection>
	{
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override MessageTypeObjCollection GetCollectionToTest()
		{
			return new MessageTypeObjCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new MessageTypeObj();
		}
	}
}
