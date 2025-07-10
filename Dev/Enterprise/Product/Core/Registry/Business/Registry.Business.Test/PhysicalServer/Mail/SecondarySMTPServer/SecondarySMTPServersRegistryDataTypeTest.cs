using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SecondarySMTPServersRegistryDataType))]
	sealed class SecondarySMTPServersRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<SecondarySMTPServersRegistryDataType>
	{
		protected override string ExpectedEditorName => "SecondarySMTPServerRegistryItemEditor";

		protected override SecondarySMTPServersRegistryDataType GetNewDataType() => new SecondarySMTPServersRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection1 = new SecondarySMTPServerCollection();
			var server1 = collection1.AddNew();
			server1.SMTPServer = "mail.server1.com";
			server1.SMTPPort = 587;
			server1.SMTPSecureConnection = SecureConnectionTypes.SSL;
			server1.SMTPUsername = "user1@server1.com";
			server1.SMTPPassword = "password1";
			server1.AllowEmailsToBeSentFromUsersAddress = true;
			server1.SMTPSenderAddress = "sender@server1.com";
			server1.SupportedDomains = "domain1.com, domain2.com";

			var server2 = collection1.AddNew();
			server2.SMTPServer = "mail.server2.com";
			server2.SMTPPort = 110;
			server2.SMTPSecureConnection = SecureConnectionTypes.TLS;
			server2.SMTPUsername = "user2@server2.com";
			server2.SMTPPassword = "password2";
			server2.AllowEmailsToBeSentFromUsersAddress = false;
			server2.SMTPSenderAddress = "sender@server2.com";
			server2.SupportedDomains = "domain3.com, domain4.com";

			var collection2 = new SecondarySMTPServerCollection();
			var server3 = collection2.AddNew();
			server3.SMTPServer = "mail.server3.com";
			server3.SMTPPort = 25;
			server3.SMTPSecureConnection = SecureConnectionTypes.TLS;
			server3.SMTPUsername = "user3@server3.com";
			server3.SMTPPassword = "password3";
			server3.AllowEmailsToBeSentFromUsersAddress = true;
			server3.SMTPSenderAddress = "sender@server3.com";
			server3.SupportedDomains = "domain5.com";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection1, DataType.Serialise(collection1)),
				new ValidSampleAndBinaryValueInDB(collection2, DataType.Serialise(collection2))
			};
		}
	}
}
