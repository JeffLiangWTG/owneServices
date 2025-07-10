using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Philippines.Credential.Testing
{
	sealed class PhilippinesCredentialLoaderTest : PasswordTypeCredentialLoaderTest<PhilippinesCredentialLoader>
	{
		public override void TestLoadForGEIRequest_Throws_WhenNullArguments()
		{
			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			var branch = Factory.New<GlbBranch>();

			AssertExceptionThrown<ArgumentNullException>(() =>
				CreateDefaultObjectForTest().LoadForGEIRequest(null, CreateTestBatch(), countryFactoryMock.Object).ToArray()
			);

			AssertExceptionThrown<ArgumentNullException>(() =>
				CreateDefaultObjectForTest().LoadForGEIRequest(branch, null, countryFactoryMock.Object).ToArray()
			);

			AssertExceptionThrown<ArgumentNullException>(() =>
				CreateDefaultObjectForTest().LoadForGEIRequest(branch, CreateTestBatch(), null).ToArray()
			);

			AssertExceptionThrown<ArgumentNullException>(() =>
				CreateDefaultObjectForTest().LoadForGEIRequest(branch, CreateTestBatch(), countryFactoryMock.Object).ToArray()
			);

			var company = Factory.New<GlbCompany>();
			branch.GB_GC = company.PK;

			AssertNoExceptionThrown(() =>
				CreateDefaultObjectForTest().LoadForGEIRequest(branch, CreateTestBatch(), countryFactoryMock.Object).ToArray()
			);
		}

		public void TestConstructor_SetsProperty_CertificateOrderByColumn()
		{
			var loader = CreateDefaultObjectForTest();
			AssertEquals(nameof(loader.CertificateOrderByColumn), GlbExternalPasswordSchema.GP_SystemLastEditTimeUtc.Name, loader.CertificateOrderByColumn.Name);
		}

		[TestDate]
		public void TestLoadForGEIRequest()
		{
			AssertLoadForGEIRequest(isBranchLevelDataInDB: true, isCompanyLevelDataInDB: true);
		}

		[TestDate]
		public void TestLoadForGEIRequest_OnlyBranchDataInDB()
		{
			AssertLoadForGEIRequest(isBranchLevelDataInDB: true, isCompanyLevelDataInDB: false);
		}

		[TestDate]
		public void TestLoadForGEIRequest_OnlyCompanyDataInDB()
		{
			AssertLoadForGEIRequest(isBranchLevelDataInDB: false, isCompanyLevelDataInDB: true);
		}

		[TestDate]
		public void TestLoadForGEIRequest_NoDataInDB()
		{
			AssertLoadForGEIRequest(isBranchLevelDataInDB: false, isCompanyLevelDataInDB: false);
		}

		void AssertLoadForGEIRequest(bool isBranchLevelDataInDB, bool isCompanyLevelDataInDB)
		{
			var branch = CreateBranchAndCompany();
			PreparePasswordTestingData(branch, isBranchLevelDataInDB, isCompanyLevelDataInDB);
			AssertLoadForGEIRequestCore(branch, isBranchLevelDataInDB, isCompanyLevelDataInDB);
		}

		void AssertLoadForGEIRequestCore(GlbBranch branch, bool isDbContainsBranchLevelData, bool isDbContainsCompanyLevelData)
		{
			var loader = CreateDefaultObjectForTest();

			var result = loader.LoadForGEIRequest(branch, CreateTestBatch(), null);

			if (isDbContainsBranchLevelData)
			{
				AssertResult(
					comment: $@"{GetAssertionInfo()}Branch Level result, sorting result by.GP_SystemLastEditTimeUtc.",
					isBranchLevel: !isDbContainsCompanyLevelData
				);
			}
			else if (isDbContainsCompanyLevelData)
			{
				AssertResult(
					comment: $@"{GetAssertionInfo()}Company Level result, value could be branch level data or company level data, sorting result by.GP_SystemLastEditTimeUtc.",
					isBranchLevel: false
				);
			}
			else
			{
				AssertEquals($"{GetAssertionInfo()}no result for settings, like restricting only branch level but only company level data.", 0, result.Count());
			}

			string GetAssertionInfo() => $"[IsDbContainsBranchLevelData:{isDbContainsBranchLevelData}][IsDbContainsCompanyLevelData:{isDbContainsCompanyLevelData}]";

			void AssertResult(string comment, bool isBranchLevel)
			{
				var expectedApplicationId = isBranchLevel
					? "PHA_Branch_ApplicationId_20230102"
					: "PHA_Company_ApplicationId_20230104";
				var expectedAccreditationId = isBranchLevel
					? "PHA_Branch_AccreditationId_20230102"
					: "PHA_Company_AccreditationId_20230104";
				var expectedUsername = isBranchLevel
					? "PHU_Branch_UserId_20230102"
					: "PHU_Company_UserId_20230104";
				var expectedPassword = isBranchLevel
					? "PHU_Branch_Password_20230102"
					: "PHU_Company_Password_20230104";
				CombineAssertions(comment,
					() =>
					{
						AssertEquals("4 rows for PHA & PHU.", 4, result.Count());
						AssertCredentialTag("ApplicationId", expectedApplicationId, false);
						AssertCredentialTag("AccreditationId", expectedAccreditationId, false);
						AssertCredentialTag("Username", expectedUsername, false);
						AssertCredentialTag("Password", expectedPassword, false);
					}
				);
			}

			void AssertCredentialTag(string tagKey, string expectedValue, bool expectedEncrypted)
			{
				var tag = result.FirstOrDefault(x => x.Key == tagKey);
				AssertNotNull($"{tagKey}", tag);
				AssertEquals($"{tagKey} Value", expectedValue, tag.Value.Value);
				AssertEquals($"{tagKey} Encrypted", expectedEncrypted, tag.Value.Encrypted);
			}
		}

		void PreparePasswordTestingData(GlbBranch branch, bool shouldCreateBranchLevel, bool shouldCreateCompanyLevel)
		{
			var passwordTypes = new PasswordTypesList().GetAllCodes();
			foreach (var passwordType in passwordTypes)
			{
				if (shouldCreateBranchLevel)
				{
					CreateCredential(passwordType, true, new ZDateTime(2023, 01, 01));
					CreateCredential(passwordType, true, new ZDateTime(2023, 01, 02));
				}

				if (shouldCreateCompanyLevel)
				{
					CreateCredential(passwordType, false, new ZDateTime(2023, 01, 03));
					CreateCredential(passwordType, false, new ZDateTime(2023, 01, 04));
				}
			}

			void CreateCredential(string passwordType, bool isBranchLevel, ZDateTime lastUpdateTime)
			{
				TestDateAttribute.Date = lastUpdateTime.ToDateTime();

				var level = isBranchLevel
					? "Branch"
					: "Company";
				var user = passwordType == PasswordTypesList.Codes.PHA
					? "ApplicationId"
					: "UserId";
				var password = passwordType == PasswordTypesList.Codes.PHA
					? "AccreditationId"
					: "Password";

				var credential = Factory.New<GlbExternalPassword>();
				credential.GP_PasswordType = passwordType;
				credential.GP_GB = isBranchLevel
					? branch.PK
					: ZGuid.Empty;
				credential.GP_GC = branch.GB_GC;
				credential.GP_UserID = $"{passwordType}_{level}_{user}_{lastUpdateTime:yyyyMMdd}";
				credential.CurrentDecryptedPassword = $"{passwordType}_{level}_{password}_{lastUpdateTime:yyyyMMdd}";

				Factory.Save();
			}
		}

		protected override PhilippinesCredentialLoader CreateDefaultObjectForTest()
			=> new PhilippinesCredentialLoader();
	}
}
