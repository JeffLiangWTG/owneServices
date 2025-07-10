using System;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	[TestsSubclassesOf(typeof(CusReconEntryLineBuilder<IImportDecLine, IMonthlyClosingDecLine>))]
	abstract class CusReconEntryLineBuilderAbstractTest<TCusReconEntryLineBuilder, TImportDecLine, TImportDecHeader, TMonthlyClosingDecLine> : TestCaseWithFactory
		where TCusReconEntryLineBuilder : CusReconEntryLineBuilder<TImportDecLine, TMonthlyClosingDecLine>
		where TImportDecLine : IImportDecLine
		where TImportDecHeader : IImportDecHeader
		where TMonthlyClosingDecLine : IMonthlyClosingDecLine
	{
		public void TestConstructor()
		{
			var nullLineProvider = default(TImportDecLine);
			var nullHeaderProvider = default(TImportDecHeader);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("All parameters provided", () => GetEntryLineBuilder(LineProvider, HeaderProvider, cusReconEntry));
				AssertExceptionThrown<ArgumentException>("LineProvider null", () => GetEntryLineBuilder(nullLineProvider, HeaderProvider, cusReconEntry));
				AssertExceptionThrown<ArgumentException>("HeaderProvider null", () => GetEntryLineBuilder(LineProvider, nullHeaderProvider, cusReconEntry));
				AssertExceptionThrown<ArgumentException>("CusReconEntry null", () => GetEntryLineBuilder(LineProvider, HeaderProvider, null));
				AssertExceptionThrown<ArgumentException>("All parameters null", () => GetEntryLineBuilder(nullLineProvider, nullHeaderProvider, null));
			});
		}

		public void TestCreateLodgedEntryLineAndSnapshot()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No CusReconEntryLine created yet", 0, cusReconEntry.CusReconEntryLines.Count);

				entryLineBuilder.CreateLodgedEntryLineAndSnapshot();
				var cusReconEntryLine = cusReconEntry.CusReconEntryLines.SingleOrDefault();
				AssertEquals("CusReconEntryLine CRL_CRE", cusReconEntry.PK, cusReconEntryLine.CRL_CRE);
				AssertEquals("CusReconEntryLine CRL_Description", "TestDescription", cusReconEntryLine.CRL_Description);
				AssertEquals("CusReconEntryLine CRL_OriginalEntryLineNumber", new ZShort(2), cusReconEntryLine.CRL_OriginalEntryLineNumber);

				var query = new ZQuery(CusReconSnapshotSchema.CRS_CRL_Line, cusReconEntryLine.PK);
				var snapshot = cusReconEntryLine.Factory.Load<Customs.Business.CusReconSnapshot>(query).Single();
				AssertEquals("CusReconSnapshot CRS_CRL_Line", cusReconEntryLine.PK, snapshot.CRS_CRL_Line);
				AssertEquals("CusReconSnapshot CRS_Type", CusReconConstants.Lodged, snapshot.CRS_Type);
				Assert("CusReconSnapshot Contains XML", snapshot.CRS_SnapshotXml.StartsWith("<DEMonthlyClosingEntryLineSnapshot"));
			});
		}

		public void TestCreateCurrentEntryLineAndSnapshot_NewCurrentEntryLineSnapshotXMLMessageNotEqualsNormalXMLDeclaration()
		{
			CombineAssertions(() =>
			{
				entryLineBuilder.CreateLodgedEntryLineAndSnapshot();
				var cusReconEntryLine = cusReconEntry.CusReconEntryLines.SingleOrDefault();

				var currentQuery = GetCusReconSnapshotQuery(cusReconEntryLine.PK, CusReconConstants.Current);
				AssertEquals("CUR-Snapshot count should be 0", 0, Factory.Load<Customs.Business.CusReconSnapshot>(currentQuery).Length);

				var logedQuery = GetCusReconSnapshotQuery(cusReconEntryLine.PK, CusReconConstants.Lodged);
				var logedSnapshot = cusReconEntryLine.Factory.Load<Customs.Business.CusReconSnapshot>(logedQuery).Single();
				logedSnapshot.CRS_SnapshotXml = EmptySnapshotXML;

				entryLineBuilder.CreateCurrentEntryLineAndSnapshot();
				AssertEquals("LDG Snapshot count should still be 1", 1, Factory.Load<Customs.Business.CusReconSnapshot>(logedQuery).Length);
				var currentSnapshot = cusReconEntryLine.Factory.Load<Customs.Business.CusReconSnapshot>(currentQuery).Single();
				Assert("CUR-Snapshot Contains XML", currentSnapshot.CRS_SnapshotXml.StartsWith("<DEMonthlyClosingEntryLineSnapshot"));

				entryLineBuilder.CreateCurrentEntryLineAndSnapshot();
				AssertNull("Previous CUR-Snapshot was deleted", cusReconEntryLine.Factory.Load<Customs.Business.CusReconSnapshot>(currentSnapshot.PK));
				AssertEquals("New CUR-Snapshot was created", 1, Factory.Load<Customs.Business.CusReconSnapshot>(currentQuery).Length);
			});
		}

		public void TestCreateCurrentEntryLineAndSnapshot_HasChangesNotInSnapshot_False()
		{
			var currentSnapshot = CreateLodgedAndCurrentSnapshots();
			AssertNull("No CUR-snapshot created because is NormalXMLDeclaration and HasChangesNotInSnapshotProvider = False", currentSnapshot);
		}

		public void TestCreateCurrentEntryLineAndSnapshot_HasChangesNotInSnapshot_True()
		{
			var currentSnapshot = CreateLodgedAndCurrentSnapshots(expectToHaveChangesNotInSnapshot: true);
			AssertEquals("CUR-snapshot created because HasChangesNotInSnapshotProvider = True", EmptySnapshotXML, currentSnapshot.CRS_SnapshotXml);
		}

		public void TestCreateCurrentEntryLineAndSnapshot_CurrentSnapshotDontGetsDeleted()
		{
			entryLineBuilder.CreateLodgedEntryLineAndSnapshot();
			Factory.Save();
			var cusReconEntryLine = cusReconEntry.CusReconEntryLines.SingleOrDefault();

			var logedSnapshot = cusReconEntryLine.Factory.LoadTop1<Customs.Business.CusReconSnapshot>(GetCusReconSnapshotQuery(cusReconEntryLine.PK, CusReconConstants.Lodged));
			var logedSnapshotXmlTemp = logedSnapshot.CRS_SnapshotXml;
			logedSnapshot.CRS_SnapshotXml = EmptySnapshotXML;

			entryLineBuilder.CreateCurrentEntryLineAndSnapshot();
			var currentSnapshot = Factory.Load<Customs.Business.CusReconSnapshot>(GetCusReconSnapshotQuery(cusReconEntryLine.PK, CusReconConstants.Current)).SingleOrDefault();
			AssertNotNull("CUR-Snapshot created", currentSnapshot);

			var currentSnapshotPK = currentSnapshot.PK;
			logedSnapshot.CRS_SnapshotXml = logedSnapshotXmlTemp;
			entryLineBuilder.CreateCurrentEntryLineAndSnapshot();
			currentSnapshot = Factory.Load<Customs.Business.CusReconSnapshot>(GetCusReconSnapshotQuery(cusReconEntryLine.PK, CusReconConstants.Current)).SingleOrDefault();
			AssertNotNull("CUR-Snapshot not deleted", currentSnapshot);
			AssertEquals("Still the same CUR-Snapshot", currentSnapshotPK, currentSnapshot.PK);
		}

		public void TestCreateCurrentEntryLineAndSnapshot_CurrentSnapshotGetsDeletedBeforeNewOneIsCreated()
		{
			entryLineBuilder.CreateLodgedEntryLineAndSnapshot();
			Factory.Save();
			var cusReconEntryLine = cusReconEntry.CusReconEntryLines.SingleOrDefault();

			var logedSnapshot = cusReconEntryLine.Factory.LoadTop1<Customs.Business.CusReconSnapshot>(GetCusReconSnapshotQuery(cusReconEntryLine.PK, CusReconConstants.Lodged));
			logedSnapshot.CRS_SnapshotXml = EmptySnapshotXML;

			entryLineBuilder.CreateCurrentEntryLineAndSnapshot();
			var currentSnapshot = Factory.Load<Customs.Business.CusReconSnapshot>(GetCusReconSnapshotQuery(cusReconEntryLine.PK, CusReconConstants.Current)).SingleOrDefault();
			AssertNotNull("CUR-Snapshot created", currentSnapshot);

			var firstCurrentSnapshotPK = currentSnapshot.PK;
			entryLineBuilder.CreateCurrentEntryLineAndSnapshot();
			currentSnapshot = Factory.Load<Customs.Business.CusReconSnapshot>(GetCusReconSnapshotQuery(cusReconEntryLine.PK, CusReconConstants.Current)).SingleOrDefault();
			AssertNotNull("Still 1 CUR-Snapshot exists (old one deleted)", currentSnapshot);
			Assert("New CUR-Snapshot exists (old one deleted)", currentSnapshot.PK != firstCurrentSnapshotPK);
		}

		protected abstract TCusReconEntryLineBuilder GetEntryLineBuilder(TImportDecLine lineProvider, TImportDecHeader headerProvider, CusReconEntry reconEntry);

		protected abstract TImportDecHeader HeaderProvider { get; }

		protected abstract TImportDecLine LineProvider { get; }

		protected Customs.Business.CusReconSnapshot CreateLodgedAndCurrentSnapshots(bool expectToHaveChangesNotInSnapshot = false)
		{
			entryLineBuilder.CreateLodgedEntryLineAndSnapshot();
			Factory.Save();
			if (expectToHaveChangesNotInSnapshot)
			{
				declaration.JE_GoodsDestination = "MZ";
			}
			var cusReconEntryLine = cusReconEntry.CusReconEntryLines.SingleOrDefault();
			entryLineBuilder.CreateCurrentEntryLineAndSnapshot();
			var currentQuery = GetCusReconSnapshotQuery(cusReconEntryLine.PK, CusReconConstants.Current);
			return cusReconEntryLine.Factory.Load<Customs.Business.CusReconSnapshot>(currentQuery).SingleOrDefault();
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusReconEntry = Factory.NewWithValidTestData<CusReconEntry>();
			var entryHeader = (CusEntryHeader)cusReconEntry.EntryHeader;
			cusReconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 2;

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			entryHeader.CH_JE = declaration.PK;
			invoiceLine.JI_CL = entryLine.PK;
			entryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_JE = declaration.PK;
			Factory.Save();

			entryLineBuilder = GetEntryLineBuilder(LineProvider, HeaderProvider, cusReconEntry);
		}
		protected JobComInvoiceLine invoiceLine;
		protected TCusReconEntryLineBuilder entryLineBuilder;
		protected CusReconEntry cusReconEntry;
		JobDeclaration declaration;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		CusEntryInstruction entryInstruction;

		ZQuery GetCusReconSnapshotQuery(ZGuid cusReconEntryLinePK, ZString type)
		{
			var query = new ZQuery(CusReconSnapshotSchema.CRS_CRL_Line, cusReconEntryLinePK);
			query.AddToFilter(CusReconSnapshotSchema.CRS_Type, type);
			return query;
		}

		const string EmptySnapshotXML = "<DEMonthlyClosingEntryLineSnapshot xmlns=\"http://www.cargowise.com/Schemas/DEMonthlyClosing\" />";
	}
}
