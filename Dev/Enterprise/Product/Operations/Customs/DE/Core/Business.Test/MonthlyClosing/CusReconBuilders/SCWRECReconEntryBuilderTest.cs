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
	sealed class SCWRECReconEntryBuilderTest : TestCaseWithFactory
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
			var reconEntry = entryBuilder.CreateLodgedEntryAndSnapshot();
			Factory.Save();

			var reconDeclaration = Factory.New<CusReconDeclaration>();
			reconEntry.CRE_CRD = reconDeclaration.PK;
			var consigeeAddress = Factory.New<OrgHeader>().MainAddress;
			reconEntry.CRE_OA_ImporterAddress = consigeeAddress.PK;
			reconDeclaration.CRD_OA_ImporterAddress = Factory.New<OrgHeader>().MainAddress.PK;
			reconDeclaration.CRD_OA_DeclarantAddress = consigeeAddress.PK;
			AssertEquals(true, entryBuilder.HasChangesNotInSnapshotProvider());
		}

		protected override void SetUp()
		{
			base.SetUp();

			var subMock = new Mock<IImportDocument>();
			subMock.Setup(d => d.Type).Returns("A123");
			subMock.Setup(d => d.ReferenceNumber).Returns("ABC123");
			subMock.Setup(d => d.IssuingDate).Returns(new DateTime(2021, 11, 10));
			mockHeader = new Mock<ISCWRECHeader>();
			mockHeader.Setup(p => p.DeclarationType).Returns("x");
			mockHeader.Setup(p => p.Documents).Returns(new IImportDocument[] { subMock.Object });
			mockHeader.Setup(m => m.Lines).Returns(Array.Empty<ISCWRECLine>());
			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryBuilder = new SCWRECReconEntryBuilder(cusEntryHeader, mockHeader.Object);
		}
		Mock<ISCWRECHeader> mockHeader;
		CusEntryHeader cusEntryHeader;
		SCWRECReconEntryBuilder entryBuilder;
	}
}
