using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(StorageMainCollection))]
	public class StorageMainCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StorageMainCollection(MasterFactory);
		}

		protected override BusinessObjectFactory NewFactory()
		{
			return MasterFactory;
		}

		public void TestSort()
		{
			StorageMain x = MasterFactory.New<StorageMain>();
			x.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			x.IsTopLevelParent = true;

			StorageMain y = MasterFactory.New<StorageMain>();
			y.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			StorageMain z = MasterFactory.New<StorageMain>();
			z.SM_Type = "UNA";

			StorageMainCollection mainCollection = new StorageMainCollection(MasterFactory);
			mainCollection.Add(x);
			mainCollection.Add(y);
			mainCollection.Add(z);

			mainCollection.Sort(StorageMainSchema.Constants.SM_Type, ListSortDirection.Ascending);

			AssertEquals("The record marked TopLevelParent should be in the first position", x, mainCollection[0]);
			AssertEquals("The second position should be the ORG record (going alphabetically)", y, mainCollection[1]);
			AssertEquals("The third position shoudl be the UNA record (going alphabetically)", z, mainCollection[2]);

			x.IsTopLevelParent = false;
			z.IsTopLevelParent = true;

			mainCollection.Sort(StorageMainSchema.Constants.SM_Type, ListSortDirection.Ascending);

			AssertEquals("Changed the top level parent, now that record (Z) should be in the first position", z, mainCollection[0]);
			AssertEquals("The second position shoudl be the ORG record (going alphabetically)", y, mainCollection[1]);
			AssertEquals("The third position should be the SHP record (going alphabetically)", x, mainCollection[2]);
		}

		public void TestAdditionalFilter()
		{
			TestCaseHelper.ClearTable("StorageMain");

			BusinessObjectFactory factory = new BusinessObjectFactory();

			GlbCompany company1 = factory.NewWithValidTestData<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			GlbBranch branch1 = factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company1.PK;

			factory.Save();

			ZQuery orgQuery = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "W");
			OrgHeader org1 = MasterFactory.LoadTop1(typeof(OrgHeader), orgQuery) as OrgHeader;

			orgQuery = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "R");
			OrgHeader org2 = MasterFactory.LoadTop1(typeof(OrgHeader), orgQuery) as OrgHeader;

			orgQuery = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "G");
			OrgHeader org3 = MasterFactory.LoadTop1(typeof(OrgHeader), orgQuery) as OrgHeader;

			GlbCompany.CurrentCompany.SetCountry("AU");

			StorageMain sGMain = MasterFactory.New<StorageMain>();
			sGMain.SM_ParentFK = org1.PK;
			sGMain.SM_Type = "CLS"; // available only in SG
			sGMain.SM_DB = 1;

			StorageMain aUMain = MasterFactory.New<StorageMain>();
			aUMain.SM_Type = "DEC"; // available only in AU
			aUMain.SM_ParentFK = org2.PK;
			aUMain.SM_DB = 1;

			StorageMain aUMain2 = MasterFactory.New<StorageMain>();
			aUMain2.SM_Type = "IMC"; // available only in AU
			aUMain2.SM_ParentFK = org3.PK;
			aUMain2.SM_DB = 1;

			MasterFactory.Save();

			StorageMainCollection mainCollection = new StorageMainCollection(MasterFactory);
			mainCollection.Load();
			AssertEquals("Collection count should be 2", 2, mainCollection.Count);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch1.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				mainCollection = new StorageMainCollection(MasterFactory);
				mainCollection.Load();
				AssertEquals("Collection count should be 1", 1, mainCollection.Count);
			}
		}

		DocumentFactory MasterFactory
		{
			get
			{
				if (fMasterFactory == null)
				{
					fMasterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				}
				return fMasterFactory;
			}
		}
		DocumentFactory fMasterFactory;
	}
}
