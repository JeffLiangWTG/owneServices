using Enterprise.MailManager.MessageProcessor;
using Enterprise.ZArchitecture.Core.Test;
using NUnit.Framework;

namespace Enterprise.MailManager.Test
{
	[TestedType(typeof(MessageFilterAttribute))]
	sealed class MessageFilterAttributeTest : AssemblyMetaDataAttributeTestCase<MessageFilterAttribute>
	{
		public void TestEquals_AllPropertiesInEquals()
		{
			var attribute1 = GetAssemblyMetaDataAttributeForTesting();
			var attribute2 = GetAssemblyMetaDataAttributeForTesting();
			Assert(attribute1.Equals(attribute2));

			attribute1.ServiceTaskCode = "ServiceTaskCode";
			Assert(!attribute1.Equals(attribute2));

			attribute2.ServiceTaskCode = "ServiceTaskCode";
			Assert(attribute1.Equals(attribute2));

			attribute1.TableName = "TableName";
			Assert(!attribute1.Equals(attribute2));

			attribute2.TableName = "TableName";
			Assert(attribute1.Equals(attribute2));
		}
	}
}
