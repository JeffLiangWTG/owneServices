using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business.Customs.XtCredential;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(MailboxAndRemoteWebPrintClientCredentials))]
	sealed class MailboxAndRemoteWebPrintClientCredentialsTest : RegistryBusinessObjectTemplateTestCase<MailboxAndRemoteWebPrintClientCredentials>
	{
		[TestDate(2024, 05, 01)]
		public void TestIxTCredentialProvider()
		{
			var key = ObjectFactory.Get<IProductRegistration>().Key;
			IxTCredentialProvider provider = MailboxAndRemoteWebPrintClientCredentials;

			CombineAssertions(() =>
			{
				AssertSame("Factory", Factory, provider.Factory);
				AssertSame("SendCredentialFactory", MailboxAndRemoteWebPrintClientCredentials.SendCredentialFactory, provider.Factory);

				AssertNull("Password", provider.Password);
				AssertNull("OldPassword", provider.OldPassword);

				AssertEquals("DatabaseCode", string.Empty, provider.DatabaseCode);
				AssertEquals("DatabaseNumber", string.Empty, provider.DatabaseNumber);

				AssertEquals("RequiredAction", XtCredentialAction.None, provider.RequiredAction);
				AssertEquals("ChangeDateTime", ZDateTime.Now.ToDateTime(), provider.ChangeDateTime);
				AssertEquals("Identifier", XtCredentialConstants.IdentifierList.JPC, provider.Identifier);

				AssertEquals("LinkUniqueID", ZGuid.Empty, provider.LinkUniqueID);
				AssertEquals("LinkTableName", StmDataSchema.Constants.TableName, provider.LinkTableName);

				AssertEquals("EnterpriseCode", $"{key.EnterpriseCode}{key.ServerCode}_JPC", provider.EnterpriseCode);
			});
		}

		public void TestDefaultValue()
		{
			AssertEquals(3, MailboxAndRemoteWebPrintClientCredentials.ReceivingInterval);
			AssertEquals(15, MailboxAndRemoteWebPrintClientCredentials.SendingInterval);
			AssertEquals(XtCredentialStatusList.Codes.Unregistered, MailboxAndRemoteWebPrintClientCredentials.Status);
		}

		public void TestReadOnly()
		{
			Assert(MailboxAndRemoteWebPrintClientCredentials.StatusInfo.ReadOnly);
		}

		public void TestLookupsType()
		{
			AssertType<MailboxAndRemoteWebPrintClientCredentialsLookups>(MailboxAndRemoteWebPrintClientCredentials.Lookups);
		}

		public void TestValidationType()
		{
			AssertType<MailboxAndRemoteWebPrintClientCredentialsValidation>(MailboxAndRemoteWebPrintClientCredentials.Validation);
		}

		public void TestIsEmpty()
		{
			Assert(MailboxAndRemoteWebPrintClientCredentials.IsEmpty);

			MailboxAndRemoteWebPrintClientCredentials.LocalComputerAlias = "LocalComputerAlias";
			MailboxAndRemoteWebPrintClientCredentials.DomainName = "DomainName";

			Assert(!MailboxAndRemoteWebPrintClientCredentials.IsEmpty);
		}

		MailboxAndRemoteWebPrintClientCredentials MailboxAndRemoteWebPrintClientCredentials => mailboxAndRemoteWebPrintClientCredentials ??= new MailboxAndRemoteWebPrintClientCredentials(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);

		MailboxAndRemoteWebPrintClientCredentials mailboxAndRemoteWebPrintClientCredentials;

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override MailboxAndRemoteWebPrintClientCredentials GetBusinessObjectToClone() => (MailboxAndRemoteWebPrintClientCredentials)GetNewBusinessObject();

		protected override MailboxAndRemoteWebPrintClientCredentials GetBusinessObjectToSerialise() => (MailboxAndRemoteWebPrintClientCredentials)GetNewBusinessObject();
	}
}
