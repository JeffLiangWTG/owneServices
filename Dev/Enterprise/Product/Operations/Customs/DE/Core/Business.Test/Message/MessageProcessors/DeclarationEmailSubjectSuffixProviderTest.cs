using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class DeclarationEmailSubjectSuffixProviderTest : Customs.Business.Testing.DataProviderTestCase<DeclarationEmailSubjectSuffixProvider>
	{
		public void TestSetEmailSubjectSuffix()
		{
			var dataProvider = GetProvider();

			var email = new EmailDef();
			email.Subject = "Test Subject";
			dataProvider.SetEmailSubjectSuffix(email);

			AssertEquals("Suffix Added", "Test Subject - LRN: DE1234567", email.Subject);
		}

		protected override DeclarationEmailSubjectSuffixProvider GetProvider() => new (entry);

		protected override void SetUp()
		{
			var declaration = Factory.New<JobDeclaration>();
			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.LocalReferenceNumber = "DE1234567";
		}

		CusEntryHeader entry;
	}
}
