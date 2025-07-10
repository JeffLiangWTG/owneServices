using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentScanning.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(CusInBondMoveHeaderCollection))]
	class CusInBondMoveHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<CusInBondMoveHeaderCollection>
	{
		protected override CusInBondMoveHeaderCollection GetCollectionToTest()
		{
			var dec = Factory.New<JobDeclaration>();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = dec.PK;
			return entryInstruction.CusInBondPermitsHeaders;
		}

		public void TestAddNewCreateHeader()
		{
			var moveHeaders = GetCollectionToTest();
			var moveHeader = moveHeaders.AddNew();
			var header = moveHeader.Header;
			Assert("header is created", !header.IsDeleted);
		}

		public void TestDeleteLastWillDeleteHeader()
		{
			var moveHeaders = GetCollectionToTest();
			moveHeaders.Reload();
			var moveHeader = moveHeaders.AddNew();
			var header = moveHeader.Header;
			var moveHeader1 = moveHeaders.AddNew();
			moveHeader.Delete();
			CombineAssertions(() =>
			{
				Assert("header is not deleted", !header.IsDeleted);
				moveHeader1.Delete();
				Assert("header is deleted", header.IsDeleted);
			});
		}

		public void TestStorageMainChangeWithListChange()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var moveHeader1 = entryInstruction.CusInBondPermitsHeaders.AddNew();
			var masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var parentMain = masterFactory.NewWithValidTestData<StorageMain>();
			parentMain.SM_DB = 1;
			parentMain.SM_Type = Core.Constants.DocManagerCodes.JobDeclaration;
			parentMain.SM_ParentFK = declaration.PK;
			var storageMain1 = masterFactory.NewWithValidTestData<StorageMain>();
			storageMain1.SM_Type = Core.Constants.DocManagerCodes.AsycudaCusInBondMoveHeader;
			storageMain1.SM_ParentFK = moveHeader1.PK;
			storageMain1.SM_DB = 1;
			Factory.Save();
			masterFactory.Save();
			CombineAssertions(() =>
			{
				var storageMain = (StorageMain)declaration.DocManagerInfo.MasterFactory.GetStorageMainForPK(declaration.PK);
				var relatedParentMains = storageMain.RelatedParentMains.Cast<StorageMain>().Where(x => x.SM_Type == Core.Constants.DocManagerCodes.AsycudaCusInBondMoveHeader).ToArray();
				AssertEquals("StorageMain related StorageMains count", 1, relatedParentMains.Length);
				AssertEquals("storageMain1 added", moveHeader1.PK, relatedParentMains[0].SM_ParentFK);
				var storageMainToDelete = relatedParentMains[0];
				entryInstruction.CusInBondPermitsHeaders.Delete(moveHeader1);
				relatedParentMains = storageMain.RelatedParentMains.Cast<StorageMain>().Where(x => x.SM_Type == Core.Constants.DocManagerCodes.AsycudaCusInBondMoveHeader).ToArray();
				AssertEquals("moveHeader1 storage main removed", 0, relatedParentMains.Length);
				Assert("storageMain1 deleted", storageMainToDelete.IsDeleted);
			});
		}
	}
}
