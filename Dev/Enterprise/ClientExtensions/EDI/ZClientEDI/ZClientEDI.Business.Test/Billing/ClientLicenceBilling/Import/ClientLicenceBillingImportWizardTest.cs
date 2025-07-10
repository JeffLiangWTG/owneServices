using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.DataMapping.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(ClientLicenceBillingImportWizard))]
	public class ClientLicenceBillingImportWizardTest : NonPersistentBusinessObjectTestCase
	{
		public void TestImportIntoBizObjCore()
		{
			var collection = new ClientLicenceBillingFlattenedCollection(Factory);
			var wizard = new ClientLicenceBillingImportWizardForTest(new ClientLicenceBillingImportInfo(collection), GetSettingsStorage(), new FileMapperForTest());
			wizard.Mapping[0].AddFileColumnIndex(0);
			wizard.Mapping[1].AddFileColumnIndex(1);
			wizard.Mapping[2].AddFileColumnIndex(2);
			wizard.Mapping[3].AddFileColumnIndex(3);
			wizard.Mapping[4].AddFileColumnIndex(4);

			wizard.FakeFileContent.Add(new[] { "ORG1", "100", "AUD", "200", "USD" });
			wizard.FakeFileContent.Add(new[] { "ORG2", "300", "GBP", "0", "" });
			wizard.FakeFileContent.Add(new[] { "ORG3", "0", "", "500", "CAD" });

			UnitTestUserNotification.Instance.AddYesAnswer();
			UnitTestUserNotification.Instance.AddYesAnswer();

			wizard.ImportIntoCollection(collection);
			AssertImportedResult("ORG1", 100, "AUD", 200, "USD", collection[0]);
			AssertImportedResult("ORG2", 300, "GBP", 0, "", collection[1]);
			AssertImportedResult("ORG3", 0, "", 500, "CAD", collection[2]);
		}

		void AssertImportedResult(string expectedOrgCode,
						ZDecimal expectedCurrentPrepaymentBalance, string expectedCurrentPrepaymentCurrency,
						ZDecimal expectedFuturePrepaymentBalance, string expectedFuturePrepaymentCurrency,
						ClientLicenceBillingFlattened bizObj)
		{
			AssertEquals(expectedOrgCode, bizObj.OrgCode);
			AssertEquals(expectedCurrentPrepaymentBalance, bizObj.CurrentPrepaymentBalance);
			AssertEquals(true, bizObj.IsCurrentPrepaymentBalanceProvided);
			AssertEquals(expectedCurrentPrepaymentCurrency, bizObj.CurrentPrepaymentCurrency);
			AssertEquals(true, bizObj.IsCurrentPrepaymentBalanceProvided);

			AssertEquals(expectedFuturePrepaymentBalance, bizObj.FuturePrepaymentBalance);
			AssertEquals(true, bizObj.IsFuturePrepaymentBalanceProvided);
			AssertEquals(expectedFuturePrepaymentCurrency, bizObj.FuturePrepaymentCurrency);
			AssertEquals(true, bizObj.IsFuturePrepaymentCurrencyProvided);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var collection = new ClientLicenceBillingFlattenedCollection(Factory);
			return new ClientLicenceBillingImportWizard(new ClientLicenceBillingImportInfo(collection), GetSettingsStorage(), new FileMapperForTest());
		}

		static ISettingsStorage GetSettingsStorage()
		{
			var settingsStorageStub = new Mock<ISettingsStorage>();
			settingsStorageStub.Setup(m => m.GetSavedSettings()).Returns(
				new string[] {
					ClientLicenceBillingFlattened.Schema.OrgCode,
					ClientLicenceBillingFlattened.Schema.CurrentPrepaymentBalance,
					ClientLicenceBillingFlattened.Schema.CurrentPrepaymentCurrency,
					ClientLicenceBillingFlattened.Schema.FuturePrepaymentBalance,
					ClientLicenceBillingFlattened.Schema.FuturePrepaymentCurrency
			});

			return settingsStorageStub.Object;
		}

		class ClientLicenceBillingImportWizardForTest : ClientLicenceBillingImportWizard
		{
			public ClientLicenceBillingImportWizardForTest(IImportCollectionInfo collectionInfo, ISettingsStorage settingsStorage, IFileMapper fileMapper) : base(collectionInfo, settingsStorage, fileMapper)
			{
			}

			public List<string[]> FakeFileContent = new List<string[]>();

			public override List<string[]> LoadFile(int startingRow, int maximumRows, bool forceFileLoad = false)
			{
				return FakeFileContent;
			}
		}
	}
}
