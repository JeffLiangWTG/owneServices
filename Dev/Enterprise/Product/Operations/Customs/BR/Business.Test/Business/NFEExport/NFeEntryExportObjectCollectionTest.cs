using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(NFeEntryExportObjectCollection))]
	class NFeEntryExportObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NFeEntryExportObjectCollection>
	{
		protected override NFeEntryExportObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new NFeEntryExportObjectCollection(declaration);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new NFeEntryExportObject(Factory.New<CusEntryHeader>());
		}

		public void TestAllowNew()
		{
			var nfeObjectCollection = GetCollectionToTest();
			AssertEquals("Should not allow new", false, nfeObjectCollection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var nfeObjectCollection = GetCollectionToTest();
			AssertEquals("Should not allow delete", false, nfeObjectCollection.AllowRemove);
		}

		public void TestLoadEntries()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			var cusEntryHeaders1 = declaration.ActiveEntryHeaders.AddNew();
			cusEntryHeaders1.CH_Status = BRMessageStatusList.Codes.Accepted;
			cusEntryHeaders1.MovementReferenceNumberSetter("2000010001");

			var cusEntryHeaders2 = declaration.ActiveEntryHeaders.AddNew();
			cusEntryHeaders2.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;

			var nfeExport = declaration.NFeExportObject;

			var nfeEntry = nfeExport.Entries;
			AssertEquals("NFeExportObjectCollection Count should be", 2, nfeEntry.Count);
		}
	}
}
