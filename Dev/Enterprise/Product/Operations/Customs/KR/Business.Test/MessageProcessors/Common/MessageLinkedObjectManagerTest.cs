using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class MessageLinkedObjectManagerTest : TestCaseWithFactory
	{
		public void TestGetLinkedObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryNumFTA = Factory.New<CusEntryNumber>();
			entryNumFTA.CE_ParentID = entry.PK;
			entryNumFTA.CE_EntryNum = "~1";
			entryNumFTA.CE_EntryType = "IMP";
			entryNumFTA.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			entryNumFTA.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			var entryNumGold = Factory.New<CusEntryNumber>();
			entryNumGold.CE_ParentID = entry2.PK;
			entryNumGold.CE_EntryNum = "~1";
			entryNumGold.CE_EntryType = ElectronicDocumentTypeList.Codes._5TM;
			entryNumGold.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			entryNumGold.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			Factory.Save();

			AssertEquals("entry is linked", entry.PK, MessageLinkedObjectManager.GetLinkedObject(declaration.Factory, GlbCompany.CurrentCompany, "~1", "IMP").PK);
		}
	}
}
