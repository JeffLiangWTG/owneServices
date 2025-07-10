using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class MessageManagerCreatorTest : TestCaseWithFactory
	{
		public void TestCreateNew()
		{
			var entryHeader = Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			AssertType<ExportDeclarationMessageManager>(MessageManagerCreator.CreateNew(new ExportDeclarationMessageSendingObject(entryHeader)));
			AssertType<DuimpMessageManager>(MessageManagerCreator.CreateNew(new DuimpMessageSendingObject(entryHeader)));
			AssertType<ImportSiscomexDeclarationMessageManager>(MessageManagerCreator.CreateNew(new ImportSiscomexMessageSendingObject(entryHeader)));
			AssertType<ImportLicenseMessageManager>(MessageManagerCreator.CreateNew(new ImportLicenseMessageSendingObject(entryHeader)));
			AssertType<LPCODeclarationMessageManager>(MessageManagerCreator.CreateNew(new LPCODeclarationMessageSendingObject(entryHeader)));

			var subscription = Factory.New<GlbExternalPassword_BRS>();
			AssertType<SubscriptionMessageManager>(MessageManagerCreator.CreateNew(new SubscriptionMessageSendingObject(subscription)));

			var orgHeader = Factory.New<OrgHeader>();
			var foreignOperator = Factory.New<CusBRForeignOperator>();
			foreignOperator.BFR_OH_Owner = orgHeader.PK;
			AssertType<ForeignOperatorMessageManager>(MessageManagerCreator.CreateNew(new ForeignOperatorMessageSendingObject(foreignOperator)));

			var lpcoMessageSending = new LPCOMessageSendingObjectParent(Factory.New<CusLPCOHeader>());
			AssertType<LPCOMessageManager>(MessageManagerCreator.CreateNew(new LPCOMessageSendingObject(lpcoMessageSending)));

			var catalogMessageSending = new GoodsCatalogMessageSendingObject(Factory.New<CusGoodsCatalog>());
			AssertType<GoodsCatalogMessageManager>(MessageManagerCreator.CreateNew(catalogMessageSending));
		}
	}
}
