using System.Diagnostics.CodeAnalysis;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(StorageDocsCollectionBase))]
	public class StorageDocsCollectionBaseTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StorageDocsCollectionBase(masterFactory);
		}

		protected override BusinessObjectFactory NewFactory()
		{
			return new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return masterFactory.New<StorageDocs>();
		}

		public void TestMasterFactory()
		{
			AssertEquals("MasterFactory instance should be correct", masterFactory, baseCollection.MasterFactory);
		}

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestAdditionalFilterInDifferentCountries()
		{
			ClearStorageContext();
			var initialUserContext = Env.CurrentUserContext;

			try
			{
				var factory = new BusinessObjectFactory();
				var branch1 = CreateTestBranchIn(factory, Core.Constants.CountryCodes.Singapore);
				var branch2 = CreateTestBranchIn(factory, Core.Constants.CountryCodes.Australia);
				var branch3 = CreateTestBranchIn(factory, Core.Constants.CountryCodes.Fiji);
				factory.Save();

				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, branch1.PK.ToGuid(), Env.CurrentDepartment.PK));
				var documentInSGOnly = CreateStorageDocsWithType("CLS", org1);
				masterFactory.Save();

				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, branch2.PK.ToGuid(), Env.CurrentDepartment.PK));
				var documentInAUOnly1 = CreateStorageDocsWithType("IMC", org2);
				var documentInAUOnly2 = CreateStorageDocsWithType("EXC", org3);
				var documentInBothCountries = CreateStorageDocsWithType(Core.Constants.DocManagerCodes.Shipment, null);
				masterFactory.Save();

				var aUCollection = new StorageDocsCollectionBase(masterFactory.GetFactory(1));
				aUCollection.Load();
				AssertEquals("AU Collection count should include the two docs from AU, and one common doc", 3, aUCollection.Count);
				Assert("Contains the AU docs", aUCollection.Contains(documentInAUOnly1.PK));
				Assert("Contains the AU docs", aUCollection.Contains(documentInAUOnly2.PK));
				Assert("Contains the common docs", aUCollection.Contains(documentInBothCountries.PK));
				Assert("Doesn't contain the SG doc", !aUCollection.Contains(documentInSGOnly.PK));

				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, branch1.PK.ToGuid(), Env.CurrentDepartment.PK));
				var sGCollection = new StorageDocsCollectionBase(masterFactory.GetFactory(1));
				sGCollection.Load();
				AssertEquals("SG Colletion count should be 2 - one from SG main, one common doc", 2, sGCollection.Count);
				Assert("Contains the SG doc", sGCollection.Contains(documentInSGOnly.PK));
				Assert("Contains the common doc", sGCollection.Contains(documentInBothCountries.PK));
				Assert("Doesn't contain the AU docs", !sGCollection.Contains(documentInAUOnly1.PK));
				Assert("Doesn't contain the AU docs", !sGCollection.Contains(documentInAUOnly2.PK));

				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, branch3.PK.ToGuid(), Env.CurrentDepartment.PK));
				var eRCollection = new StorageDocsCollectionBase(masterFactory.GetFactory(1));
				eRCollection.Load();
				AssertEquals("ERCollection count should be just 1 - the common doc", 1, eRCollection.Count);
				Assert("Contains the common doc", eRCollection.Contains(documentInBothCountries.PK));
				Assert("Doesn't contain the AU docs", !eRCollection.Contains(documentInAUOnly1.PK));
				Assert("Doesn't contain the AU docs", !eRCollection.Contains(documentInAUOnly2.PK));
				Assert("Doesn't contain the SG doc", !eRCollection.Contains(documentInSGOnly.PK));
			}
			finally
			{
				Env.SetUserContext(initialUserContext);
			}
		}

		public void TestAddingToAdditionalFilter()
		{
			var oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString();
			ClearStorageContext();

			try
			{
				GlbCompany.CurrentCompany.SetCountry("SG");
				var documentInSGOnly = CreateStorageDocsWithType("CLS", org1);

				GlbCompany.CurrentCompany.SetCountry("AU");
				var documentInAU = CreateStorageDocsWithType("IMC", org2);
				var documentInAUDeleted = CreateStorageDocsWithType("EXC", org3);

				masterFactory.Save();
				// need to save before setting this IsDeleted flag, otherwise the doc will just get deleted from the db on saving.
				documentInAUDeleted.SC_IsDeleted = true;

				masterFactory.Save(); // save again to stop exception thrown with non db query
				baseCollection.Load();
				AssertEquals("Should have two documents in the collection from AU", 2, baseCollection.Count);
				AssertEquals("Collection contains the docs from AU", true, baseCollection.Contains(documentInAU.PK));
				AssertEquals("Collection contains the docs from AU", true, baseCollection.Contains(documentInAUDeleted.PK));

				var query = new ZQuery(StorageDocsSchema.SC_IsDeleted, ZBool.False);
				baseCollection.LoadWithMoreFiltering(query);
				AssertEquals("Should have one document in the collection from AU", 1, baseCollection.Count);
				AssertEquals("Collection contains the doc not deleted", true, baseCollection.Contains(documentInAU.PK));
				AssertEquals("document should not be deleted", false, baseCollection[0].SC_IsDeleted);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}

		public void TestAddingFiltersToAdditionalFilterShouldProduceCorrectSQL()
		{
			var anotherFilter = new ZQuery(StorageDocsSchema.SC_IsDeleted, ZBool.True);
			// should not throw an exception; the handwritten SQL must work as an ordinary SQL Filter
			AssertNoExceptionThrown(() => { baseCollection.LoadWithMoreFiltering(anotherFilter); });
		}

		OrgHeader LoadFirstOrgHeaderStartsWith(string orgCode)
		{
			var orgQuery = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, orgCode);
			return masterFactory.LoadTop1(typeof(OrgHeader), orgQuery) as OrgHeader;
		}

		GlbBranch CreateTestBranchIn(BusinessObjectFactory factory, string country)
		{
			var company = factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = country;
			var branch = factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			return branch;
		}

		StorageDocs CreateStorageDocsWithType(string type, OrgHeader org)
		{
			var mainInSGOnly = masterFactory.New<StorageMain>();
			mainInSGOnly.SM_Type = type;
			mainInSGOnly.SM_DB = 1;

			if (org != null)
			{
				mainInSGOnly.SM_ParentFK = org.PK;
			}

			var documentInSGOnly = mainInSGOnly.Documents.AddNew();
			return documentInSGOnly;
		}

		void ClearStorageContext()
		{
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable(new DocManagerDBHelper().GetTableNameWithDatabasePrefix(1, StorageDocsSchema.Constants.TableName));
		}

		protected override void SetUp()
		{
			base.SetUp();
			masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			baseCollection = new StorageDocsCollectionBase(masterFactory.GetFactory(1));
			org1 = LoadFirstOrgHeaderStartsWith("W");
			org2 = LoadFirstOrgHeaderStartsWith("R");
			org3 = LoadFirstOrgHeaderStartsWith("G");
		}

		protected override void MasterSetUp()
		{
			base.MasterSetUp();

			using (var auxConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.CreateDbDropExisting(auxConnection, Db.DatabaseName + "_SD001");
			}
		}

		protected override void FinalTearDown()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(adminConnection, Db.DatabaseName + "_SD001");
			}

			base.FinalTearDown();
		}

		StorageDocsCollectionBase baseCollection;
		DocumentFactory masterFactory;
		OrgHeader org1;
		OrgHeader org2;
		OrgHeader org3;
	}
}
