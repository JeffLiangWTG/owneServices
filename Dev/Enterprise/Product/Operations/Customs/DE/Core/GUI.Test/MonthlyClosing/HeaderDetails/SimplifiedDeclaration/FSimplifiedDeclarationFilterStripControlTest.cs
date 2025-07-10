using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.MonthlyClosing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using CusReconSnapshot = Enterprise.Customs.Business.CusReconSnapshot;

namespace Enterprise.Customs.DE.GUI.Testing
{
	sealed class FSimplifiedDeclarationFilterStripControlTest : TestCaseWithFactory
	{
		public void TestSimplifiedDeclarationsGroupBox_Caption()
		{
			using (var control = new FSimplifiedDeclarationFilterStripControl(declaration))
			{
				var simplifiedDeclarationsGroupBox = control.SimplifiedDeclarationsGroupBox;
				AssertEquals("Caption of SimplifiedDeclarationsGroupBox", "Simplified Declarations (Select one row and double click to open Declaration).",
				simplifiedDeclarationsGroupBox.CaptionResourceString.Caption);
			}
		}

		public void TestControls_SimplifiedDeclarationsGroupBox()
		{
			using (var control = new FSimplifiedDeclarationFilterStripControl(declaration))
			{
				CombineAssertions(() =>
				{
					var simplifiedDeclarationsGroupBox = control.SimplifiedDeclarationsGroupBox;
					var simplifiedDeclarationsGrid = control.Grid;
					AssertEquals("Grid is within GroupBox", true, simplifiedDeclarationsGroupBox.Controls.Contains(simplifiedDeclarationsGrid));

					var columnNames = simplifiedDeclarationsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).ToArray();
					AssertSequencesEqual("Columns", new[] { nameof(CusReconEntry.CRE_SystemCreateUser),nameof(CusReconEntry.CRE_SystemCreateTimeUtc),nameof(CusReconEntry.CRE_SystemLastEditUser),nameof(CusReconEntry.CRE_SystemLastEditTimeUtc),nameof(CusReconEntry.JobNumber), nameof(CusReconEntry.OwnerRef), CusReconEntry.Schema.DeclarantCode, CusReconEntry.Schema.ImporterCode, CusReconEntry.Schema.RepresentativeCode, CusReconEntry.Schema.BuyingAgentCode,
					Customs.Business.CusReconBase.AutoCusReconEntry.Schema.CRE_EntryDate,Customs.Business.CusReconBase.AutoCusReconEntry.Schema.CRE_OriginalEntryNumber, Customs.Business.CusReconBase.AutoCusReconEntry.Schema.CRE_EntryType, Customs.Business.CusReconEntry.Schema.RepresentationType, nameof(CusReconEntry.EntryHasChanges) }, columnNames);

					AssertEquals("JobNumber", 80, simplifiedDeclarationsGrid.GetColumnStyle(nameof(CusReconEntry.JobNumber)).Width);
					AssertEquals("OwnerRef", 120, simplifiedDeclarationsGrid.GetColumnStyle(nameof(CusReconEntry.OwnerRef)).Width);
					AssertEquals("DeclarantCode", 130, simplifiedDeclarationsGrid.GetColumnStyle(CusReconEntry.Schema.DeclarantCode).Width);
					AssertEquals("ImporterCode", 130, simplifiedDeclarationsGrid.GetColumnStyle(CusReconEntry.Schema.ImporterCode).Width);
					AssertEquals("RepresentativeCode", 130, simplifiedDeclarationsGrid.GetColumnStyle(CusReconEntry.Schema.RepresentativeCode).Width);
					AssertEquals("BuyingAgentCode", 130, simplifiedDeclarationsGrid.GetColumnStyle(CusReconEntry.Schema.BuyingAgentCode).Width);
					var entryDateColumnInfo = simplifiedDeclarationsGrid.GetColumnStyle(Customs.Business.CusReconBase.AutoCusReconEntry.Schema.CRE_EntryDate) as ZDateEditColumnStyleInfo;
					AssertEquals("CRE_EntryDate Width", 120, entryDateColumnInfo.Width);
					AssertEquals("CRE_EntryDate DateTimeFormat", ZArchitecture.Core.ZDateTimePickerFormat.Short, entryDateColumnInfo.DateTimeFormat);
					AssertEquals("CRE_OriginalEntryNumber", 123, simplifiedDeclarationsGrid.GetColumnStyle(Customs.Business.CusReconBase.AutoCusReconEntry.Schema.CRE_OriginalEntryNumber).Width);
					AssertEquals("CRE_EntryType", 76, simplifiedDeclarationsGrid.GetColumnStyle(Customs.Business.CusReconBase.AutoCusReconEntry.Schema.CRE_EntryType).Width);
					AssertEquals("RepresentationType", 72, simplifiedDeclarationsGrid.GetColumnStyle(Customs.Business.CusReconEntry.Schema.RepresentationType).Width);
					AssertEquals("EntryHasChanges", 80, simplifiedDeclarationsGrid.GetColumnStyle(nameof(CusReconEntry.EntryHasChanges)).Width);
				});
			}
		}

		public void TestSimplifiedDeclarationsGrid_Properties()
		{
			using (var control = new FSimplifiedDeclarationFilterStripControl(declaration))
			{
				var simplifiedDeclarationsGrid = control.FilteredGrid;
				CombineAssertions(() =>
				{
					AssertEquals("AllowReadOnlyRowsToBeDeleted", true, simplifiedDeclarationsGrid.AllowReadOnlyRowsToBeDeleted);
					AssertEquals("RemoveAction", RemoveAction.Remove, simplifiedDeclarationsGrid.RemoveAction);
					Assert("ForceShowExportToExcelMenuItem", simplifiedDeclarationsGrid.ForceShowExportToExcelMenuItem);
				});
			}
		}

		public void TestSimplifiedDeclarationsGrid_RowsDeleting()
		{
			var entryHeader = FSimplifiedDeclarationHelper.CreateEntryHeader(jobDeclaration);

			var entry1 = FSimplifiedDeclarationHelper.CreateCusReconEntry(declaration, entryHeader);
			var entry2 = FSimplifiedDeclarationHelper.CreateCusReconEntry(declaration, entryHeader);
			var entry3 = FSimplifiedDeclarationHelper.CreateCusReconEntry(declaration, entryHeader);

			using (var form = new ZForm(declaration))
			using (var control = new FSimplifiedDeclarationFilterStripControl(declaration))
			{
				form.Controls.Add(control);
				form.Show();

				var simplifiedDeclarationsGrid = control.FilteredGrid;
				var deleteMenuItem = simplifiedDeclarationsGrid.DeleteMenuItem;
				simplifiedDeclarationsGrid.Select(0);
				simplifiedDeclarationsGrid.Select(2);

				CombineAssertions(() =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
					deleteMenuItem.PerformClick();
					AssertEquals("Confirmation dialog", "This action will remove the Simplified Declaration(s) from the Monthly Closing Declaration.\r\nThe Simplified Declaration(s) must be linked into a new Monthly Closing Declaration.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Cancel, entry1 not removed from collection", true, declaration.CusReconEntries.Contains(entry1));
					AssertEquals("Cancel, entry2 not removed from collection", true, declaration.CusReconEntries.Contains(entry2));
					AssertEquals("Cancel, entry3 not removed from collection", true, declaration.CusReconEntries.Contains(entry3));

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					deleteMenuItem.PerformClick();
					AssertEquals("Proceed, entry1 removed from collection", false, declaration.CusReconEntries.Contains(entry1));
					AssertEquals("Proceed, entry1 is not deleted", false, entry1.IsDeleted);
					AssertEquals("Proceed, entry2 not removed from collection", true, declaration.CusReconEntries.Contains(entry2));
					AssertEquals("Proceed, entry3 removed from collection", false, declaration.CusReconEntries.Contains(entry3));
					AssertEquals("Proceed, entry3 is not deleted", false, entry3.IsDeleted);
				});
			}
		}

		public void TestSimplifiedDeclarationsGrid_RowDeleted()
		{
			var entryHeader1 = FSimplifiedDeclarationHelper.CreateEntryHeader(jobDeclaration);
			var entryHeader2 = FSimplifiedDeclarationHelper.CreateEntryHeader(jobDeclaration);
			var entryHeader3 = FSimplifiedDeclarationHelper.CreateEntryHeader(jobDeclaration);

			Factory.Save();

			var entry1 = FSimplifiedDeclarationHelper.CreateCusReconEntry(declaration, entryHeader1);
			var line1 = FSimplifiedDeclarationHelper.CreateCusReconEntryLine(entry1, 1, entryHeader1.AllEntryLines.First().CL_LineNumber);

			var entry2 = FSimplifiedDeclarationHelper.CreateCusReconEntry(declaration, entryHeader2);
			var line2 = FSimplifiedDeclarationHelper.CreateCusReconEntryLine(entry2, 2, entryHeader2.AllEntryLines.First().CL_LineNumber);

			var entry3 = FSimplifiedDeclarationHelper.CreateCusReconEntry(declaration, entryHeader3);
			var line3 = FSimplifiedDeclarationHelper.CreateCusReconEntryLine(entry3, 3, entryHeader3.AllEntryLines.First().CL_LineNumber);
			var line4 = FSimplifiedDeclarationHelper.CreateCusReconEntryLine(entry3, 4, entryHeader3.AllEntryLines.First().CL_LineNumber);

			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var control = new FSimplifiedDeclarationFilterStripControl(declaration))
			{
				form.Controls.Add(control);
				form.Show();

				var simplifiedDeclarationsGrid = control.Grid;
				var deleteMenuItem = simplifiedDeclarationsGrid.DeleteMenuItem;
				simplifiedDeclarationsGrid.Select(0);
				simplifiedDeclarationsGrid.Select(2);

				CombineAssertions(() =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					deleteMenuItem.PerformClick();
					AssertEquals("Line1 removed, CRL_LineNumber updated", ZShort.Zero, line1.CRL_LineNumber);
					AssertEquals("Line2 not removed, CRL_LineNumber not updated", (ZShort)2, line2.CRL_LineNumber);
					AssertEquals("Line3 removed, CRL_LineNumber updated", ZShort.Zero, line3.CRL_LineNumber);
					AssertEquals("Line4 removed, CRL_LineNumber updated", ZShort.Zero, line4.CRL_LineNumber);
				});
			}
		}

		public void TestSimplifiedDeclarationsGrid_MouseDown()
		{
			var entryHeader = FSimplifiedDeclarationHelper.CreateEntryHeader(jobDeclaration);
			FSimplifiedDeclarationHelper.CreateCusReconEntry(declaration, entryHeader);

			using (var form = new ZForm(declaration))
			using (var control = new FSimplifiedDeclarationFilterStripControl(declaration))
			{
				form.Controls.Add(control);
				form.Show();
				Factory.Save();

				var simplifiedDeclarationsGrid = control.Grid;
				simplifiedDeclarationsGrid.PerformMouseDownForTest(0, 1);
				using (var childForm = Application.OpenForms.OfType<JobDeclarationForm>().SingleOrDefault())
				{
					AssertNull("Didn't open JobDeclarationForm", childForm);
				}

				simplifiedDeclarationsGrid.PerformMouseDownForTest(0, 2);
				using (var childForm = Application.OpenForms.OfType<JobDeclarationForm>().SingleOrDefault())
				{
					AssertNotNull("Open JobDeclarationForm Successfully!", childForm);
				}
			}
		}

		public void TestLinesGrid_Properties()
		{
			using (var control = new FSimplifiedDeclarationFilterStripControl(declaration))
			{
				var linesGrid = control.LinesGrid;
				CombineAssertions(() =>
				{
					AssertEquals("AllowReadOnlyRowsToBeDeleted", true, linesGrid.AllowReadOnlyRowsToBeDeleted);
					AssertEquals("RemoveAction", RemoveAction.Remove, linesGrid.RemoveAction);
				});
			}
		}

		public void TestLinesGrid_RowsDeleting()
		{
			var entryHeader = FSimplifiedDeclarationHelper.CreateEntryHeader(jobDeclaration);
			var entry = FSimplifiedDeclarationHelper.CreateCusReconEntry(declaration, entryHeader);
			entry.CRE_OriginalEntryNumber = "12345";
			var entryLine = FSimplifiedDeclarationHelper.CreateCusReconEntryLine(entry, 1, entryHeader.AllEntryLines.First().CL_LineNumber);
			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var control = new FSimplifiedDeclarationFilterStripControl(declaration))
			{
				form.Controls.Add(control);
				form.Show();

				var simplifiedDeclarationsGrid = control.Grid;
				simplifiedDeclarationsGrid.Select(0);
				var linesGrid = control.LinesGrid;
				var deleteMenuItem = linesGrid.DeleteMenuItem;
				linesGrid.Select(0);

				CombineAssertions(() =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
					deleteMenuItem.PerformClick();
					AssertEquals("Confirmation dialog", "This action will remove the Simplified Declaration Line(s) from the Monthly Closing Declaration.\r\nThe Simplified Declaration Line(s) must be linked into a new Monthly Closing Declaration.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Cancel, entryLine LineNumber remains", (ZShort)1, entryLine.CRL_LineNumber);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					deleteMenuItem.PerformClick();
					AssertEquals("Proceed, entryLine LineNumber set to 0", ZShort.Zero, entryLine.CRL_LineNumber);
				});
			}
		}

		public void TestLinesGrid_RowsDeleted_AllLinesRemovedAtOnce()
		{
			var entryHeader = FSimplifiedDeclarationHelper.CreateEntryHeader(jobDeclaration);
			var originalEntry = FSimplifiedDeclarationHelper.CreateCusReconEntry(declaration, entryHeader);
			originalEntry.CRE_OriginalEntryNumber = "12345";
			var entryLine1 = FSimplifiedDeclarationHelper.CreateCusReconEntryLine(originalEntry, 1, entryHeader.AllEntryLines.First().CL_LineNumber);
			var entryLine2 = FSimplifiedDeclarationHelper.CreateCusReconEntryLine(originalEntry, 2, entryHeader.AllEntryLines.First().CL_LineNumber);

			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var control = new FSimplifiedDeclarationFilterStripControl(declaration))
			{
				form.Controls.Add(control);
				form.Show();

				var simplifiedDeclarationsGrid = control.Grid;
				simplifiedDeclarationsGrid.Select(0);
				var linesGrid = control.LinesGrid;
				var deleteMenuItem = linesGrid.DeleteMenuItem;
				linesGrid.Select(0);
				linesGrid.Select(1);

				CombineAssertions(() =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					deleteMenuItem.PerformClick();

					var dummyEntry = GetDummyCusReconEntry("12345");
					AssertEquals("dummyEntry not linked to any declaration", ZGuid.Empty, dummyEntry.CRE_CRD);
					AssertEquals("dummyEntry has two lines", 2, dummyEntry.CusReconEntryLines.Count);
					AssertEquals("entryLine1 LineNumber 0", ZShort.Zero, dummyEntry.CusReconEntryLines[0].CRL_LineNumber);
					AssertEquals("entryLine2 LineNumber 0", ZShort.Zero, dummyEntry.CusReconEntryLines[1].CRL_LineNumber);
					AssertEquals("originalEntry Deleted", true, originalEntry.IsDeleted);
				});
			}
		}

		public void TestLinesGrid_RowsDeleted_AllLinesRemovedIn2Steps()
		{
			var entryHeader = FSimplifiedDeclarationHelper.CreateEntryHeader(jobDeclaration);
			var originalEntry = FSimplifiedDeclarationHelper.CreateCusReconEntry(declaration, entryHeader);
			originalEntry.CRE_OriginalEntryNumber = "12345";
			var originalEntrySnapshot = originalEntry.CusReconSnapshots.AddNew();
			originalEntrySnapshot.CRS_Type = CusReconConstants.Lodged;
			originalEntrySnapshot.CRS_SnapshotXml = @"<DEMonthlyClosingEntrySnapshot xmlns=""http://www.cargowise.com/Schemas/DEMonthlyClosing""></DEMonthlyClosingEntrySnapshot>";
			var entryLine1 = FSimplifiedDeclarationHelper.CreateCusReconEntryLine(originalEntry, 1, entryHeader.AllEntryLines.First().CL_LineNumber);
			var entryLine2 = FSimplifiedDeclarationHelper.CreateCusReconEntryLine(originalEntry, 2, entryHeader.AllEntryLines.First().CL_LineNumber);
			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var control = new FSimplifiedDeclarationFilterStripControl(declaration))
			{
				form.Controls.Add(control);
				form.Show();

				var simplifiedDeclarationsGrid = control.Grid;
				simplifiedDeclarationsGrid.Select(0);
				var linesGrid = control.LinesGrid;
				var deleteMenuItem = linesGrid.DeleteMenuItem;
				linesGrid.Select(0);

				CombineAssertions(() =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					deleteMenuItem.PerformClick();
					var dummyEntry = GetDummyCusReconEntry("12345");
					AssertEquals("entry remains linked to declaration", true, declaration.CusReconEntries.Contains(originalEntry));
					AssertEquals("dummyEntry not linked to any declaration", ZGuid.Empty, dummyEntry.CRE_CRD);
					AssertEquals("entryLine1 linked to dummyEntry", true, dummyEntry.CusReconEntryLines.Contains(entryLine1));
					AssertEquals("entryLine1 LineNumber 0", ZShort.Zero, entryLine1.CRL_LineNumber);
					AssertEquals("entryLine2 remains linked to originalEntry", true, originalEntry.CusReconEntryLines.Contains(entryLine2));
					AssertEquals("entryLine2 remains", (ZShort)2, entryLine2.CRL_LineNumber);
					AssertDummyEntryWithSnapshot(dummyEntry, originalEntry, originalEntrySnapshot);

					linesGrid.Select(0);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					deleteMenuItem.PerformClick();
					AssertEquals("entryLine2 linked to dummyEntry", true, dummyEntry.CusReconEntryLines.Contains(entryLine2));
					AssertEquals("entryLine2 LineNumber 0", ZShort.Zero, entryLine2.CRL_LineNumber);
					Assert("originalEntry Deleted", originalEntry.IsDeleted);
					Assert("originalEntry snapshot deleted", originalEntrySnapshot.IsDeleted);
				});
			}
		}

		public void TestControls_LinesGroupBox()
		{
			var entryHeader = FSimplifiedDeclarationHelper.CreateEntryHeader(jobDeclaration);
			var entry = FSimplifiedDeclarationHelper.CreateCusReconEntry(declaration, entryHeader);
			var entryLine = FSimplifiedDeclarationHelper.CreateCusReconEntryLine(entry, 1, entryHeader.AllEntryLines.First().CL_LineNumber);

			using (var form = new ZForm(declaration))
			using (var control = new FSimplifiedDeclarationFilterStripControl(declaration))
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					var linesGroupBox = control.LinesGroupBox;
					var linesGrid = control.LinesGrid;
					AssertEquals("Grid is within GroupBox", true, linesGroupBox.Controls.Contains(linesGrid));

					var columnNames = linesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).ToArray();
					AssertSequencesEqual("Columns", new[] { Customs.Business.AutoCusReconEntryLine.Schema.CRL_LineNumber, Customs.Business.AutoCusReconEntryLine.Schema.CRL_OriginalEntryLineNumber, Customs.Business.AutoCusReconEntryLine.Schema.CRL_Description, Customs.Business.AutoCusReconEntryLine.Schema.CRL_CustomsStatus, nameof(CusReconEntryLine.CustomsStatusDescription), nameof(CusReconEntryLine.EntryLineHasChanges) }, columnNames);
					AssertEquals("All properties are readonly", true, columnNames.All(x => ((IAccessBusinessObject)entryLine).IsPropertyReadOnly(x)));
					AssertEquals("Set AllowReadOnlyRowsToBeDeleted to true cause all properties are readonly", true, linesGrid.AllowReadOnlyRowsToBeDeleted);
					AssertEquals("CRL_LineNumber", 80, linesGrid.GetColumnStyle(Customs.Business.AutoCusReconEntryLine.Schema.CRL_LineNumber).Width);
					AssertEquals("CRL_OriginalEntryLineNumber", 80, linesGrid.GetColumnStyle(Customs.Business.AutoCusReconEntryLine.Schema.CRL_OriginalEntryLineNumber).Width);
					AssertEquals("CRL_Description", 80, linesGrid.GetColumnStyle(Customs.Business.AutoCusReconEntryLine.Schema.CRL_Description).Width);
					AssertEquals("CRL_CustomsStatus", 80, linesGrid.GetColumnStyle(Customs.Business.AutoCusReconEntryLine.Schema.CRL_CustomsStatus).Width);
					AssertEquals("CustomsStatusDescription", 100, linesGrid.GetColumnStyle(nameof(CusReconEntryLine.CustomsStatusDescription)).Width);
					AssertEquals("EntryLineHasChanges", 80, linesGrid.GetColumnStyle(nameof(CusReconEntryLine.EntryLineHasChanges)).Width);
				});
			}
		}

		CusReconEntry GetDummyCusReconEntry(ZString originalEntryNumber)
		{
			var query = new ZQuery(CusReconEntrySchema.CRE_OriginalEntryNumber, originalEntryNumber);
			query.AddToFilter(CusReconEntrySchema.CRE_CRD, null);
			return Factory.Load<CusReconEntry>(query).SingleOrDefault();
		}

		void AssertDummyEntryWithSnapshot(CusReconEntry dummyEntry, CusReconEntry originalEntry, CusReconSnapshot originalEntrySnapshot)
		{
			AssertEquals("CRE_CRD", ZGuid.Empty, dummyEntry.CRE_CRD);
			AssertEquals("CRE_OriginalEntryNumber", originalEntry.CRE_OriginalEntryNumber, dummyEntry.CRE_OriginalEntryNumber);
			AssertEquals("CRE_EntryDate", originalEntry.CRE_EntryDate, dummyEntry.CRE_EntryDate);
			AssertEquals("CRE_EntryType", originalEntry.CRE_EntryType, dummyEntry.CRE_EntryType);
			AssertEquals("CRE_GB_Branch", originalEntry.CRE_GB_Branch, dummyEntry.CRE_GB_Branch);
			AssertEquals("CRE_OA_DeclarantAddress", originalEntry.CRE_OA_DeclarantAddress, dummyEntry.CRE_OA_DeclarantAddress);
			AssertEquals("CRE_OA_ImporterAddress", originalEntry.CRE_OA_ImporterAddress, dummyEntry.CRE_OA_ImporterAddress);
			AssertEquals("CRE_OA_RepresentativeAddress", originalEntry.CRE_OA_RepresentativeAddress, dummyEntry.CRE_OA_RepresentativeAddress);
			AssertEquals("CRE_OA_BuyingAgentAddress", originalEntry.CRE_OA_BuyingAgentAddress, dummyEntry.CRE_OA_BuyingAgentAddress);
			AssertEquals("CRE_CH_OriginalEntry", originalEntry.CRE_CH_OriginalEntry, dummyEntry.CRE_CH_OriginalEntry);

			var copiedEntrySnapshot = dummyEntry.CusReconSnapshots.SingleOrDefault();
			AssertNotNull("dummyEntry contains snapshot", copiedEntrySnapshot);
			AssertEquals("CRS_Type", originalEntrySnapshot.CRS_Type, copiedEntrySnapshot.CRS_Type);
			AssertEquals("CRS_SnapshotXml", originalEntrySnapshot.CRS_SnapshotXml, copiedEntrySnapshot.CRS_SnapshotXml);
		}

		protected override void SetUp()
		{
			base.SetUp();

			jobDeclaration = Factory.New<JobDeclaration>();
			declaration = Factory.New<CusReconDeclaration>();
			declaration.CRD_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
		}

		JobDeclaration jobDeclaration;
		CusReconDeclaration declaration;
	}
}
