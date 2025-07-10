using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.eHub.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business.Customs.XtCredential;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(MailboxAndRemoteWebPrintClientCredentialsRegistryItem))]
	sealed class MailboxAndRemoteWebPrintClientCredentialsRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<MailboxAndRemoteWebPrintClientCredentials>
	{
		public void TestSendCredential()
		{
			var credential = new MailboxAndRemoteWebPrintClientCredentials { LocalComputerAlias = "NJGCO-WTST-1", DomainName = "TEST.WTG.COM" };
			var registryItem = new MailboxAndRemoteWebPrintClientCredentialsRegistryItem("TestSendCredentialWithNormalData", null, null, null, RegistryStorageFlags.System);
			var tag = new RegistryItemTag(registryItem);

			void SaveRegistryItem(bool hasValue)
			{
				tag.NewValue = credential;
				tag.IsChanged = true;
				tag.HasValue = hasValue;

				TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);
				TestCaseHelper.ClearTable(EDIInterchangeSchema.Constants.TableName);
				tag.SaveAllValues();
			}

			SaveRegistryItem(true);

			var expectedOldPassword = string.Empty;
			var expectedPassword = SHA512Encryptor.Encrypt(credential.LocalComputerAlias + credential.DomainName);

			AssertInterchangeAndCredentialChange(XtCredentialAction.Update, expectedPassword, expectedOldPassword);

			credential = new MailboxAndRemoteWebPrintClientCredentials { LocalComputerAlias = "NJGCO-WTST-2", DomainName = "TEST.WTG.COM" };
			credential.Status = XtCredentialStatusList.Codes.Error;
			AssertEquals(XtCredentialStatusList.Codes.Error, credential.Status);

			SaveRegistryItem(true);

			expectedOldPassword = expectedPassword;
			expectedPassword = SHA512Encryptor.Encrypt(credential.LocalComputerAlias + credential.DomainName);

			AssertInterchangeAndCredentialChange(XtCredentialAction.Update, expectedPassword, expectedOldPassword);
			AssertEquals(XtCredentialStatusList.Codes.Unregistered, credential.Status);

			credential = new MailboxAndRemoteWebPrintClientCredentials { LocalComputerAlias = "NJGCO-WTST-2", DomainName = "TEST.WTG.COM", ReceivingInterval = 8 };

			SaveRegistryItem(true);

			var interchange = GetLatestInterchange(new BusinessObjectFactory());
			AssertNull("Should not send the credential as the computer name and the domain name are not changed.", interchange);

			SaveRegistryItem(false);

			expectedOldPassword = expectedPassword;
			expectedPassword = string.Empty;

			AssertInterchangeAndCredentialChange(XtCredentialAction.Delete, expectedPassword, expectedOldPassword);
		}

		void AssertInterchangeAndCredentialChange(XtCredentialAction expectedAction, string expectedPassword, string expectedOldPassword)
		{
			var factory = new BusinessObjectFactory();

			var interchange = GetLatestInterchange(factory);
			var message = (Enterprise.Messaging.Business.EDIMessage)interchange.ContainedMessages.Single();

			CombineAssertions(() =>
			{
				using (var reader = new StringReader(message.EM_MessageText))
				{
					var key = ObjectFactory.Get<IProductRegistration>().Key;
					var serializer = new XmlSerializer(typeof(CredentialChanges));
					var credentialChange = ((CredentialChanges)serializer.Deserialize(reader)).CredentialChange;

					AssertEquals("DatabaseCode", string.Empty, credentialChange.DatabaseCode);
					AssertEquals("DatabaseNumber", string.Empty, credentialChange.DatabaseNumber);

					AssertEquals("ChangeType", expectedAction.ToString(), credentialChange.ChangeType);
					AssertEquals("Password", expectedPassword, credentialChange.Password);
					AssertEquals("OldPassword", expectedOldPassword, credentialChange.OldPassword);

					AssertEquals("EnterpriseCode", $"{key.EnterpriseCode}{key.ServerCode}_JPC", credentialChange.EnterpriseCode);
				}
			});
		}

		EDIInterchange GetLatestInterchange(BusinessObjectFactory factory)
		{
			var query = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, "CFG");
			query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, "JPC");
			query.AddToFilter(EDIInterchangeSchema.EI_To, "CustomsCredentialChange");
			query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, "TRX");
			query.AddToFilter(EDIInterchangeSchema.EI_TransportType, "XTT");

			return factory.Load<EDIInterchange>(query).OrderBy(c => c.EI_SystemCreateTimeUtc).LastOrDefault();
		}

		protected override StronglyTypedRegistryItem<MailboxAndRemoteWebPrintClientCredentials, MailboxAndRemoteWebPrintClientCredentials> GetNewRegistryItem() => new MailboxAndRemoteWebPrintClientCredentialsRegistryItem("", null, null, null, RegistryStorageFlags.BranchDepartment);

		protected override MailboxAndRemoteWebPrintClientCredentials ValidValue
		{
			get
			{
				var result = new MailboxAndRemoteWebPrintClientCredentials(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);
				result.LocalComputerAlias = "TYO";
				result.DomainName = "JPCUS";

				return result;
			}
		}
	}
}
