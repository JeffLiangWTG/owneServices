using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	[TestedType(typeof(AttachedDocumentProvider))]
	sealed class AttachedDocumentProviderTest : TestCase
	{
		public void TestProperties()
		{
			CombineAssertions(() =>
			{
				AssertEquals("DocumentDate", "Test_Document_Date", provider.DocumentDate);
				AssertEquals("DocumentIdentifier", "Test_Document_Identifier", provider.DocumentIdentifier);
				AssertEquals("DocumentType", "Test_Document_Type", provider.DocumentType);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new AttachedDocumentProvider(new AttachedDocumentType
			{
				DocumentDate = "Test_Document_Date",
				DocumentIdentifier = "Test_Document_Identifier",
				DocumentType = "Test_Document_Type",
			});
		}

		AttachedDocumentProvider provider;
	}
}
