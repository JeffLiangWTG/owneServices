using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(EntryInstructionAttachmentCollection))]
	class EntryInstructionAttachmentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EntryInstructionAttachmentCollection>
	{
		protected override EntryInstructionAttachmentCollection GetCollectionToTest()
		{
			return new EntryInstructionAttachmentCollection(Factory.New<CusEntryInstruction>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new EntryInstructionAttachment(new EntryInstructionAttachmentCollection(Factory.New<CusEntryInstruction>()));
		}

		public void TestLoad()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			GetNewCusAttachment(instruction, CSDDocTypeList.Codes._10000002, "code1");
			GetNewCusAttachment(instruction, CSDDocTypeList.Codes._10000003, "code2");
			var zGuid = ZGuid.NewZGuid();
			var pivot = Factory.New<CusStorageDocPivot>();
			pivot.CSD_ParentTableCode = instruction.TablePrefix;
			pivot.CSD_ParentID = instruction.PK;
			pivot.CSD_DocType = CSDDocTypeList.Codes._00000001;
			pivot.CSD_StorageDocReference = zGuid;
			var attachments = instruction.Attachments;
			AssertEquals(3, attachments.Count);
			Assert(attachments.Cast<EntryInstructionAttachment>().Any(x => x.AttachmentType == CSDDocTypeList.Codes._10000002 && x.AttachmentNumber == "code1"));
			Assert(attachments.Cast<EntryInstructionAttachment>().Any(x => x.AttachmentType == CSDDocTypeList.Codes._10000003 && x.AttachmentNumber == "code2"));
			Assert(attachments.Cast<EntryInstructionAttachment>().Any(x => x.AttachmentType == CSDDocTypeList.Codes._00000001 && x.EDoc == zGuid));
		}

		CusAttachment GetNewCusAttachment(CusEntryInstruction instruction, string code, string data)
		{
			var cusAttachment = Factory.New<CusAttachment>();
			cusAttachment.CY_ParentTableCode = instruction.TablePrefix;
			cusAttachment.CY_ParentID = instruction.PK;
			cusAttachment.CY_Type = Constants.CusCodeDataTypes.Codes.CusAttachment;
			cusAttachment.CY_Code = code;
			cusAttachment.CY_Data = data;
			return cusAttachment;
		}

		public void TestGetNewAndRemove()
		{
			var collection = GetCollectionToTest();
			AssertEquals(0, collection.Count);
			var cusAttachment = collection.CreateNewCusAttachment();
			collection.Load();
			AssertEquals(1, collection.Count);
			collection.RemoveAndDeleteCusAttachment(cusAttachment);
			collection.Load();
			AssertEquals(0, collection.Count);
			var cusStorageDocPivot = collection.CreateNewCusStorageDocPivot();
			collection.Load();
			AssertEquals(1, collection.Count);
			collection.RemoveAndDeleteCusStorageDocPivot(cusStorageDocPivot);
			collection.Load();
			AssertEquals(0, collection.Count);
		}
	}
}
