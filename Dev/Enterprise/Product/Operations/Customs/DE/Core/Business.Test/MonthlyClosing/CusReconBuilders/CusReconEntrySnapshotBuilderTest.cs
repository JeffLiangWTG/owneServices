using System;
using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	sealed class CusReconEntrySnapshotBuilderTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2022, 01, 20, 13, 20, 20)]
		public void TestCompleteSnapshot()
		{
			AssertASCIIFileSameAsString(CusReconBuildersTestHelper.TestFilesDirectory + @"\EntrySnapshot.xml", snapshotBuilder.GetXMLMessage());
		}

		public void TestGenerateMessage_Document()
		{
			CombineAssertions(() =>
			{
				var snapshot = snapshotBuilder.GenerateMessage();
				AssertEquals("Document Length", 2, snapshot.Document.Length);
				AssertEquals("Division String", "Item4", snapshot.Document[0].Division.ToString());
			});
		}

		public void TestGenerateMessage_DocumentEmpty()
		{
			providerMock.Setup(m => m.Documents).Returns(Array.Empty<IImportDocument>());
			AssertEquals(0, snapshotBuilder.GenerateMessage().Document.Length);
		}

		[TestDate(2022, 01, 20, 13, 20, 20)]
		public void TestGenerateEmptyMessage()
		{
			CombineAssertions(() =>
			{
				var snapshot = snapshotBuilder.GenerateEmptyMessage();
				AssertEquals("Document Length", 0, snapshot.Document.Length);
				AssertEquals("LastUpdateTimeUtc", new DateTime(2022, 01, 20, 13, 20, 20), snapshot.LastUpdateTimeUtc);
				AssertEquals("LastUpdateTimeUtcSpecified", true, snapshot.LastUpdateTimeUtcSpecified);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			providerMock = new Mock<IMonthlyClosingEntrySnapshot>();
			providerMock.Setup(p => p.Documents).Returns(new List<IImportDocument>()
			{
				CusReconBuildersTestHelper.GetImportDocument("A123", "Refnr", new DateTime(2021, 10, 22)),
				CusReconBuildersTestHelper.GetImportDocument("A124", "Refnr2", new DateTime(2021, 09, 21)),
			});

			snapshotBuilder = new CusReconEntrySnapshotBuilder(providerMock.Object);
		}
		Mock<IMonthlyClosingEntrySnapshot> providerMock;
		CusReconEntrySnapshotBuilder snapshotBuilder;
	}
}
