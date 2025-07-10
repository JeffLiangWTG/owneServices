using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	sealed class SimplifiedDeclarationReconEntryBuilderBaseOnlyTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("both parameters null", () => new SimplifiedDeclarationReconEntryBuilderForTest(null, null));
				AssertExceptionThrown<ArgumentException>("provider null", () => new SimplifiedDeclarationReconEntryBuilderForTest(entryHeader, null));
				AssertExceptionThrown<ArgumentException>("entry header null", () => new SimplifiedDeclarationReconEntryBuilderForTest(null, providerMock.Object));
			});
		}

		public void TestRecordsCreated()
		{
			CombineAssertions(() =>
			{
				AssertNull("no Recon entry yet", entryHeader.GetCusReconEntry());
				var entryCreated = entryBuilder.CreateLodgedEntryAndSnapshot();
				AssertEquals("x", entryCreated.CRE_EntryType);
				AssertEquals(entryHeader.Branch.PK, entryCreated.CRE_GB_Branch);
				AssertEquals(entryHeader.Declaration.DeclarantAddress.PK, entryCreated.CRE_OA_DeclarantAddress);
				AssertEquals(Guid.Empty, entryCreated.CRE_OA_ImporterAddress);
				AssertEquals(GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, entryCreated.CRE_OA_RepresentativeAddress);
				AssertEquals(Guid.Empty, entryCreated.CRE_OA_BuyingAgentAddress);
				AssertEquals(entryHeader.PK, entryCreated.CRE_CH_OriginalEntry);

				var query = new ZQuery(CusReconSnapshotSchema.CRS_CRE_Entry, entryCreated.PK);
				var snapshot = entryCreated.Factory.LoadTop1<Customs.Business.CusReconSnapshot>(query);
				AssertEquals(entryCreated.PK, snapshot.CRS_CRE_Entry);
				AssertEquals("LDG", snapshot.CRS_Type);
				AssertContains("contains XML", "<DEMonthlyClosingEntrySnapshot", snapshot.CRS_SnapshotXml);
				AssertNotContains("no xml declaration found", "<?xml", snapshot.CRS_SnapshotXml);
			});
		}

		public void TestImporterAddress()
		{
			entryHeader.Declaration.ImporterDocumentaryAddress.E2_OA_Address = Factory.New<OrgAddress>().PK;
			CombineAssertions(() =>
			{
				var entryCreated = entryBuilder.CreateLodgedEntryAndSnapshot();
				AssertEquals(entryHeader.Declaration.ImporterDocumentaryAddress.Address.PK, entryCreated.CRE_OA_ImporterAddress);
			});
		}

		public void TestRepresentativeAddress()
		{
			entryHeader.Declaration.JE_OA_Representative = Factory.NewWithValidTestData<OrgAddress>().PK;
			CombineAssertions(() =>
			{
				var entryCreated = entryBuilder.CreateLodgedEntryAndSnapshot();
				AssertEquals(entryHeader.Declaration.JE_OA_Representative, entryCreated.CRE_OA_RepresentativeAddress);
			});
		}

		public void TestBuyingAgentAddress()
		{
			entryHeader.Declaration.JE_OA_BuyingAgentAddress = Factory.New<OrgAddress>().PK;
			CombineAssertions(() =>
			{
				var entryCreated = entryBuilder.CreateLodgedEntryAndSnapshot();
				AssertEquals(entryHeader.Declaration.JE_OA_BuyingAgentAddress, entryCreated.CRE_OA_BuyingAgentAddress);
			});
		}

		public void TestBuildLodgedLinesCalled()
		{
			CombineAssertions(() =>
			{
				AssertEquals(false, entryBuilder.BuildLodgedLinesCalled);
				entryBuilder.CreateLodgedEntryAndSnapshot();
				AssertEquals(true, entryBuilder.BuildLodgedLinesCalled);
			});
		}

		public void TestBuildCurrentLinesCalled()
		{
			entryBuilder.CreateLodgedEntryAndSnapshot();
			Factory.Save(); // CUR Snapshot loads Entry from Database

			CombineAssertions(() =>
			{
				AssertEquals(false, entryBuilder.BuildCurrentLinesCalled);
				entryBuilder.CreateCurrentSnapshot();
				AssertEquals(true, entryBuilder.BuildCurrentLinesCalled);
			});
		}

		public void TestHasChangesNotInSnapshotProviderCalled()
		{
			entryBuilder.CreateLodgedEntryAndSnapshot();
			Factory.Save(); // CUR Snapshot loads Entry from Database

			CombineAssertions(() =>
			{
				AssertEquals(false, entryBuilder.HasChangesNotInSnapshotProviderCalled);
				entryBuilder.CreateCurrentSnapshot();
				AssertEquals(true, entryBuilder.HasChangesNotInSnapshotProviderCalled);
			});
		}

		public void TestBuildCurrentSnapshotNoLDG()
		{
			CombineAssertions(() =>
			{
				var entryCreated = entryBuilder.CreateCurrentSnapshot();
				AssertNull("no entry created", entryCreated);

				var query = new ZQuery(CusReconSnapshotSchema.CRS_Type, CusReconConstants.Current);
				AssertEquals("no snapshot in DB", false, Factory.Exists(typeof(Customs.Business.CusReconSnapshot), query));
			});
		}

		public void TestBuildCurrentSnapshotNoChanges()
		{
			entryBuilder.CreateLodgedEntryAndSnapshot();
			Factory.Save(); // CUR Snapshot loads Entry from Database

			entryBuilder.CreateCurrentSnapshot();
			var query = new ZQuery(CusReconSnapshotSchema.CRS_Type, CusReconConstants.Current);
			AssertEquals("no snapshot in DB", false, Factory.Exists(typeof(Customs.Business.CusReconSnapshot), query));
		}

		[TestDate(2022, 01, 20, 13, 20, 20)]
		public void TestBuildCurrentSnapshotNoChanges_HasChangesNotInSnapshotProvider()
		{
			var entryBuilder = new SimplifiedDeclarationReconEntryBuilderForTest(entryHeader, providerMock.Object, true);
			entryBuilder.CreateLodgedEntryAndSnapshot();
			Factory.Save(); // CUR Snapshot loads Entry from Database

			entryBuilder.CreateCurrentSnapshot();
			var query = new ZQuery(CusReconSnapshotSchema.CRS_Type, CusReconConstants.Current);
			var snapshot = Factory.LoadTop1<Customs.Business.CusReconSnapshot>(query);
			AssertEquals(@"<DEMonthlyClosingEntrySnapshot LastUpdateTimeUtc=""2022-01-20T13:20:20Z"" xmlns=""http://www.cargowise.com/Schemas/DEMonthlyClosing"" />", snapshot.CRS_SnapshotXml);
		}

		public void TestBuildCurrentSnapshotMultipleDocsEqualOrderDifferent()
		{
			providerMock
				.Setup(p => p.Documents).Returns(new IImportDocument[] {
					CreateDocument("ABC", "REFERENCE1", new DateTime(2021, 12, 22)),
					CreateDocument("CDE", "REFERENCE2", new DateTime(2021, 12, 23))
				});
			entryBuilder.CreateLodgedEntryAndSnapshot();
			Factory.Save(); // CUR Snapshot loads Entry from Database

			providerMock
				.Setup(p => p.Documents).Returns(new IImportDocument[] {
					CreateDocument("CDE", "REFERENCE2", new DateTime(2021, 12, 23)),
					CreateDocument("ABC", "REFERENCE1", new DateTime(2021, 12, 22))
				});

			entryBuilder.CreateCurrentSnapshot();
			var query = new ZQuery(CusReconSnapshotSchema.CRS_Type, CusReconConstants.Current);
			AssertEquals("no snapshot in DB", false, Factory.Exists(typeof(Customs.Business.CusReconSnapshot), query));
		}

		public void TestBuildCurrentSnapshotDocsChangedContentAndCurrentRemovedWhenEqualAgain()
		{
			providerMock.Setup(m => m.Documents).Returns(new IImportDocument[] { CreateDocument("ABC", "REFERENCE1", new DateTime(2021, 12, 22)) });
			entryBuilder.CreateLodgedEntryAndSnapshot();
			Factory.Save(); // CUR Snapshot loads Entry from Database

			providerMock.Setup(m => m.Documents).Returns(new IImportDocument[] { CreateDocument("ABC", "REFERENCE1", new DateTime(2021, 12, 23)) });

			entryBuilder.CreateCurrentSnapshot();
			var query = new ZQuery(CusReconSnapshotSchema.CRS_Type, CusReconConstants.Current);
			CombineAssertions(() =>
			{
				AssertEquals("snapshot in DB", true, Factory.Exists(typeof(Customs.Business.CusReconSnapshot), query));

				providerMock.Setup(m => m.Documents).Returns(new IImportDocument[] { CreateDocument("ABC", "REFERENCE1", new DateTime(2021, 12, 22)) });
				entryBuilder.CreateCurrentSnapshot();
				AssertEquals("no snapshot in DB", false, Factory.Exists(typeof(Customs.Business.CusReconSnapshot), query));
			});
		}

		public void TestBuildCurrentSnapshotDocsChangedCount()
		{
			providerMock.Setup(m => m.Documents).Returns(new IImportDocument[] { CreateDocument("ABC", "REFERENCE1", new DateTime(2021, 12, 22)) });
			entryBuilder.CreateLodgedEntryAndSnapshot();
			Factory.Save(); // CUR Snapshot loads Entry from Database

			providerMock.Setup(p => p.Documents).Returns(new IImportDocument[]
			{
				CreateDocument("ABC", "REFERENCE1", new DateTime(2021, 12, 22)),
				CreateDocument("DEF", "REFERENCE2", new DateTime(2021, 12, 22)),
			});

			entryBuilder.CreateCurrentSnapshot();
			var query = new ZQuery(CusReconSnapshotSchema.CRS_Type, CusReconConstants.Current);
			AssertEquals("snapshot in DB", true, Factory.Exists(typeof(Customs.Business.CusReconSnapshot), query));
		}

		public void TestBuildCurrentSnapshotDocsLDGEmpty()
		{
			entryBuilder.CreateLodgedEntryAndSnapshot();
			Factory.Save(); // CUR Snapshot loads Entry from Database

			providerMock.Setup(p => p.Documents).Returns(new IImportDocument[]
			{
				CreateDocument("ABC", "REFERENCE1", new DateTime(2021, 12, 22)),
				CreateDocument("DEF", "REFERENCE2", new DateTime(2021, 12, 22)),
			});

			entryBuilder.CreateCurrentSnapshot();
			var query = new ZQuery(CusReconSnapshotSchema.CRS_Type, CusReconConstants.Current);
			AssertEquals("snapshot in DB", true, Factory.Exists(typeof(Customs.Business.CusReconSnapshot), query));
		}

		public void TestBuildCurrentSnapshotDocsCurrentEmpty()
		{
			providerMock.Setup(m => m.Documents).Returns(new IImportDocument[] { CreateDocument("ABC", "REFERENCE1", new DateTime(2021, 12, 22)) });
			entryBuilder.CreateLodgedEntryAndSnapshot();
			Factory.Save(); // CUR Snapshot loads Entry from Database

			providerMock.Setup(m => m.Documents).Returns(Array.Empty<IImportDocument>());

			entryBuilder.CreateCurrentSnapshot();
			var query = new ZQuery(CusReconSnapshotSchema.CRS_Type, CusReconConstants.Current);
			AssertEquals("snapshot in DB", true, Factory.Exists(typeof(Customs.Business.CusReconSnapshot), query));
		}

		protected override void SetUp()
		{
			base.SetUp();
			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			providerMock = new Mock<IImportDecHeader>();
			providerMock.Setup(p => p.DeclarationType).Returns("x");
			providerMock.Setup(m => m.Documents).Returns(Array.Empty<IImportDocument>());

			entryBuilder = new SimplifiedDeclarationReconEntryBuilderForTest(entryHeader, providerMock.Object);
		}

		static IImportDocument CreateDocument(string type, string reference, DateTime? issuingDate)
		{
			var mock = new Mock<IImportDocument>();
			mock.Setup(d => d.Type).Returns(type);
			mock.Setup(d => d.ReferenceNumber).Returns(reference);
			mock.Setup(d => d.IssuingDate).Returns(issuingDate);
			return mock.Object;
		}

		SimplifiedDeclarationReconEntryBuilderForTest entryBuilder;
		CusEntryHeader entryHeader;
		Mock<IImportDecHeader> providerMock;
	}

	sealed class SimplifiedDeclarationReconEntryBuilderForTest : SimplifiedDeclarationReconEntryBuilder
	{
		public SimplifiedDeclarationReconEntryBuilderForTest(CusEntryHeader entryHeader, IImportDecHeader provider, bool hasReconEntryChangesNotInSnapshot = false)
			: base(entryHeader, provider)
		{
			this.hasReconEntryChangesNotInSnapshot = hasReconEntryChangesNotInSnapshot;
		}
		readonly bool hasReconEntryChangesNotInSnapshot;

		internal bool BuildLodgedLinesCalled { get; private set; }

		internal bool BuildCurrentLinesCalled { get; private set; }

		internal bool HasChangesNotInSnapshotProviderCalled { get; private set; }

		protected override void BuildLodgedLines()
		{
			BuildLodgedLinesCalled = true;
		}

		protected override void BuildCurrentLines()
		{
			BuildCurrentLinesCalled = true;
		}

		protected internal override bool HasChangesNotInSnapshotProvider()
		{
			HasChangesNotInSnapshotProviderCalled = true;
			return hasReconEntryChangesNotInSnapshot;
		}
	}
}
