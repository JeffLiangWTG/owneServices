using System;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	sealed class CFCRECReconEntryBuilderTest : TestCaseWithFactory
	{
		public void TestGenerateEntry()
		{
			var reconEntry = entryBuilder.CreateLodgedEntryAndSnapshot();
			var snapshot = reconEntry.CusReconSnapshots.First();
			CombineAssertions(() =>
			{
				AssertContains("<Type>A123</Type>", snapshot.CRS_SnapshotXml);
				AssertContains("<ReferenceNumber>ABC123</ReferenceNumber>", snapshot.CRS_SnapshotXml);
				AssertContains("<IssuingDate>2021-11-10</IssuingDate>", snapshot.CRS_SnapshotXml);
			});
		}

		public void TestHasChangesNotInSnapshotProvider()
		{
			entryBuilder.CreateLodgedEntryAndSnapshot();
			Factory.Save();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.Addresses.AddNew();
			cusEntryHeader.Declaration.SupplierDocumentaryAddress.E2_OA_Address = address.PK;
			AssertEquals(true, entryBuilder.HasChangesNotInSnapshotProvider());
		}

		protected override void SetUp()
		{
			base.SetUp();

			mockHeader = new Mock<ICFCRECHeader>();
			mockHeader.Setup(p => p.DeclarationType).Returns("x");
			var mock = new Mock<IImportDocument>();
			mock.Setup(d => d.Type).Returns("A123");
			mock.Setup(d => d.ReferenceNumber).Returns("ABC123");
			mock.Setup(d => d.IssuingDate).Returns(new DateTime(2021, 11, 10));
			mockHeader.Setup(p => p.Documents).Returns(new IImportDocument[] { mock.Object });
			mockHeader.Setup(m => m.Lines).Returns(Array.Empty<ICFCRECLine>());

			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryBuilder = new CFCRECReconEntryBuilder(cusEntryHeader, mockHeader.Object);
		}
		Mock<ICFCRECHeader> mockHeader;
		CusEntryHeader cusEntryHeader;
		CFCRECReconEntryBuilder entryBuilder;
	}
}
