using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Module.Testing
{
	public class EdiOrgMembershipFlattenedDataTransferProcessorTest : TestCaseWithFactory
	{
		public void TestImport_MultipleOverlaps()
		{
			var org1 = Factory.New<EDIOrgHeader>();
			org1.OH_Code = "SOMEORG1";
			var item1 = org1.Memberships.AddNew();
			item1.EOR_MembershipType = "FTA";
			item1.EOR_ValidFrom = new ZDate(2018, 5, 1);
			item1.EOR_ValidTo = new ZDate(2018, 5, 31);
			var item2 = org1.Memberships.AddNew();
			item2.EOR_MembershipType = "FTA";
			item2.EOR_ValidFrom = new ZDate(2018, 7, 1);
			item2.EOR_ValidTo = new ZDate(2018, 7, 31);
			Factory.Save();
			var flattenedCollection = new EdiOrgMembershipFlattenedCollection(Factory);
			var collectionInfo = new EdiOrgMembershipImportInfo(flattenedCollection);
			var rec1 = flattenedCollection.AddNew();
			rec1.OrgCode = org1.OH_Code;
			rec1.MembershipType = "FTA";
			rec1.ValidFrom = new ZDate(2018, 6, 1);
			rec1.ValidTo = new ZDate(2018, 6, 30);
			var collection = EdiOrgMembershipCollection.CreateAdhocCollection(Factory);
			var processor = new EdiOrgMembershipFlattenedDataTransferProcessor(collection, collectionInfo);
			processor.Import();
			var importedList = collection.Cast<EdiOrgMembership>().ToList();
			AssertEquals("Imported count", 1, importedList.Count);
			AssertEquals(1, org1.Memberships.Count);
			AssertEquals(new ZDateTime(2018, 5, 1), org1.Memberships[0].EOR_ValidFrom);
			AssertEquals(new ZDateTime(2018, 7, 31), org1.Memberships[0].EOR_ValidTo);
		}

		public void TestImport_Duplicate()
		{
			var org1 = Factory.New<EDIOrgHeader>();
			org1.OH_Code = "SOMEORG1";
			var item1 = org1.Memberships.AddNew();
			item1.EOR_MembershipType = "FTA";
			item1.EOR_ValidFrom = new ZDate(2018, 5, 1);
			item1.EOR_ValidTo = new ZDate(2018, 6, 30);
			Factory.Save();
			var flattenedCollection = new EdiOrgMembershipFlattenedCollection(Factory);
			var collectionInfo = new EdiOrgMembershipImportInfo(flattenedCollection);
			var rec1 = flattenedCollection.AddNew();
			rec1.OrgCode = org1.OH_Code;
			rec1.MembershipType = "FTA";
			rec1.ValidFrom = new ZDate(2018, 7, 1);
			rec1.ValidTo = new ZDate(2018, 7, 31);
			var rec2 = flattenedCollection.AddNew();
			rec2.OrgCode = org1.OH_Code;
			rec2.MembershipType = "FTA";
			rec2.ValidFrom = new ZDate(2018, 7, 1);
			rec2.ValidTo = new ZDate(2018, 7, 31);
			var collection = EdiOrgMembershipCollection.CreateAdhocCollection(Factory);
			var processor = new EdiOrgMembershipFlattenedDataTransferProcessor(collection, collectionInfo);
			processor.Import();
			var importedList = collection.Cast<EdiOrgMembership>().ToList();
			AssertEquals("Imported count", 1, importedList.Count);
			AssertEquals(1, org1.Memberships.Count);
			AssertEquals(new ZDateTime(2018, 5, 1), org1.Memberships[0].EOR_ValidFrom);
			AssertEquals(new ZDateTime(2018, 7, 31), org1.Memberships[0].EOR_ValidTo);
		}

		public void TestImport()
		{
			var flattenedCollection = new EdiOrgMembershipFlattenedCollection(Factory);
			var collectionInfo = new EdiOrgMembershipImportInfo(flattenedCollection);
			var org1 = Factory.New<EDIOrgHeader>();
			org1.OH_Code = "SOMEORG1";
			var item1 = org1.Memberships.AddNew();
			item1.EOR_MembershipType = "FTA";
			item1.EOR_ValidFrom = new ZDate(2018, 3, 1);
			var org2 = Factory.New<EDIOrgHeader>();
			org2.OH_Code = "SOMEORG2";
			var item2 = org2.Memberships.AddNew();
			item2.EOR_MembershipType = "FTA";
			item2.EOR_ValidFrom = new ZDate(2018, 1, 1);
			item2.EOR_ValidTo = new ZDate(2018, 1, 31);
			var org3 = Factory.New<EDIOrgHeader>();
			org3.OH_Code = "SOMEORG3";
			var item3 = org3.Memberships.AddNew();
			item3.EOR_MembershipType = "FTA";
			item3.EOR_ValidFrom = new ZDate(2018, 5, 1);
			item3.EOR_ValidTo = new ZDate(2018, 5, 31);
			Factory.Save();
			var rec1 = flattenedCollection.AddNew();
			rec1.OrgCode = org1.OH_Code;
			rec1.MembershipType = "FTA";
			rec1.ValidFrom = new ZDate(2018, 4, 1);
			var rec2 = flattenedCollection.AddNew();
			rec2.OrgCode = org2.OH_Code;
			rec2.MembershipType = "FTA";
			rec2.ValidFrom = new ZDate(2018, 4, 1);
			var rec3 = flattenedCollection.AddNew();
			rec3.OrgCode = org2.OH_Code;
			rec3.MembershipType = "FTA";
			rec3.ValidFrom = new ZDate(2018, 2, 1);
			rec3.ValidTo = new ZDate(2018, 2, 28);
			var recWithBadOrgCode = flattenedCollection.AddNew();
			recWithBadOrgCode.OrgCode = "ZZZZZZZ";
			recWithBadOrgCode.MembershipType = "FTA";
			recWithBadOrgCode.ValidFrom = new ZDateTime(2018, 5, 1);
			var recWithNoValidFrom = flattenedCollection.AddNew();
			recWithNoValidFrom.OrgCode = org1.OH_Code;
			recWithNoValidFrom.MembershipType = "FTA";
			recWithNoValidFrom.ValidFrom = ZDateTime.Empty;
			var recWithToBeforeFrom = flattenedCollection.AddNew();
			recWithToBeforeFrom.OrgCode = org1.OH_Code;
			recWithToBeforeFrom.MembershipType = "FTA";
			recWithToBeforeFrom.ValidFrom = new ZDate(2018, 9, 2);
			recWithToBeforeFrom.ValidTo = new ZDate(2018, 9, 1);
			var recDupeExisting = flattenedCollection.AddNew();
			recDupeExisting.OrgCode = org3.OH_Code;
			recDupeExisting.MembershipType = item3.EOR_MembershipType;
			recDupeExisting.ValidFrom = item3.EOR_ValidFrom;
			recDupeExisting.ValidTo = item3.EOR_ValidTo;
			var collection = EdiOrgMembershipCollection.CreateAdhocCollection(Factory);
			var processor = new EdiOrgMembershipFlattenedDataTransferProcessor(collection, collectionInfo);
			processor.Import();
			var importedList = collection.Cast<EdiOrgMembership>().ToList();
			AssertEquals("EdiOrgMembership count", 2, collection.Count);
			var importOrg2 = importedList.Where(x => x.EOR_OH == org2.PK).ToList();
			AssertEquals(2, importOrg2.Count);
			{
				var import2 = importOrg2.Single(x => x.PK != item2.PK);
				import2.Validation.ValidateAll();
				AssertNoErrors(import2);
				AssertEquals("FTA", import2.EOR_MembershipType);
				AssertEquals(new ZDate(2018, 4, 1), import2.EOR_ValidFrom);
				Assert(import2.EOR_ValidTo.IsEmpty);
			}

			{
				var import3 = importOrg2.Single(x => x.PK == item2.PK);
				import3.Validation.ValidateAll();
				AssertNoErrors(import3);
				AssertEquals("FTA", import3.EOR_MembershipType);
				AssertEquals(new ZDate(2018, 1, 1), import3.EOR_ValidFrom);
				AssertEquals(new ZDate(2018, 2, 28), import3.EOR_ValidTo);
			}
		}
	}
}
