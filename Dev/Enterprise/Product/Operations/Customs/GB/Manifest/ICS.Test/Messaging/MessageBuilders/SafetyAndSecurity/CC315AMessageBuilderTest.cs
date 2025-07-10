using System.IO;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.GB.SafetyAndSecurity.Messaging.CC315A;
using NUnit.Framework;

namespace Enterprise.Customs.GB.ICS.Testing.Messaging.MessageBuilders.SafetyAndSecurity
{
	sealed class CC315AMessageBuilderTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestTimeZone]
		[TestDate(2020, 12, 1, 15, 30, 45)]
		public void TestBuildCC315AMessage()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var decWrapper = new DeclarationWrapper(manifest);
			var messageBuilder = new CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity.CC315AMessageBuilder(decWrapper);
			var xml = ((IXmlMessageBuilder)messageBuilder).GenerateXmlMessage();
			var expectedCC315A = File.ReadAllText(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\GB\Manifest\ICS.Test\Messaging\MessageBuilders\SafetyAndSecurity\TestFiles\CC315A.txt")).TrimEnd();
			AssertXMLContains(expectedCC315A, xml.GetSerializedString());
		}
	}
}
