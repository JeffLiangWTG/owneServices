using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.CN.Business.Testing
{
	class EntryInstructionAttachmentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAttachmentTypeList()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			var attachment = instruction.Attachments.AddNew();
			AssertEquals(0, attachment.Lookups.AttachmentTypeList.Count);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			instruction.CEI_JE = declaration.PK;
			AssertSame(CSDDocTypeList.GetCachedCSDDocTypeList(Factory, true), attachment.Lookups.AttachmentTypeList);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertSame(CSDDocTypeList.GetCachedCSDDocTypeList(Factory, false), attachment.Lookups.AttachmentTypeList);
		}

		public void TestEDocListAndCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Test1.pdf", "PDF");
			var eDoc2 = ((IDocManagerSupport)entry).DocManagerInfo.AddFileOrDocument(new byte[1], "Test2.txt", "TXT");
			var eDoc3 = shipment.DocManagerInfo.AddFileOrDocument(new byte[1], "Test3.pdf", "PDF");
			var attachment = instruction.Attachments.AddNew();
			AssertEquals(2, attachment.Lookups.EDocList.Count);
			AssertEquals(eDoc1.UniqueKey, ((AvailableEDocList)attachment.Lookups.EDocList)[0].PK);
			AssertEquals(eDoc3.UniqueKey, ((AvailableEDocList)attachment.Lookups.EDocList)[1].PK);
			AssertEquals(3, attachment.Lookups.GetEDocCollections().Length);
			var eDocList = new List<IeDoc>
			{
				attachment.Lookups.GetEDocCollections()[0].Cast<IeDoc>().First(),
				attachment.Lookups.GetEDocCollections()[1].Cast<IeDoc>().First(),
				attachment.Lookups.GetEDocCollections()[2].Cast<IeDoc>().First()
			};
			AssertCollectionContains(eDoc1, eDocList);
			AssertCollectionContains(eDoc2, eDocList);
			AssertCollectionContains(eDoc3, eDocList);
		}
	}
}
