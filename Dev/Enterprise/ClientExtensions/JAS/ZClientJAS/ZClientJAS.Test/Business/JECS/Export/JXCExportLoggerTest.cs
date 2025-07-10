using System;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class JXCExportLoggerTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals("Should be assigned in the constructor", Consol, Logger.ExportSource);
		}

		[TestDate(2006, 2, 27, 8, 32, 2)]
		public void TestNotify()
		{
			object lazyLoadConsol = Consol;
			Factory.Save();
			InfoNotification notification = new InfoNotification("HAHA");
			AssertEquals("Pre-condition", 0, Consol.Notes.FindByDescription(JASPredefinedNoteTypes.Instance.JXCExportLog.Description).Length);
			Logger.Notify(notification);
			StmNote[] exportLogNotes = Consol.Notes.FindByDescription(JASPredefinedNoteTypes.Instance.JXCExportLog.Description);
			AssertEquals("Export Log note should be created", 1, exportLogNotes.Length);
			AssertExportLogNote(exportLogNotes[0], "27-Feb-2006 08:32:02     HAHA");
			TestDateAttribute.Date = new DateTime(2006, 12, 3, 22, 3, 21);
			Consol.JK_AgentsReference = "SOMETHING";
			ErrorNotification errorNotification = new ErrorNotification(ErrorType.Error, "THIS IS THE ERROR");
			Logger.Notify(errorNotification);
			AssertEquals("Should not create another one if already exist", 1, Consol.Notes.FindByDescription(JASPredefinedNoteTypes.Instance.JXCExportLog.Description).Length);
			AssertExportLogNote(exportLogNotes[0], "27-Feb-2006 08:32:02     HAHA\r\n03-Dec-2006 22:03:21     Error: THIS IS THE ERROR");
			Assert("Note should be saved with a separate factory (not the consol's factory)", Consol.HasChanges);
		}

		#region Implementation
		void AssertExportLogNote(StmNote exportLogNote, ZString expectedNoteText)
		{
			Assert("Should not be specified as custom description", !exportLogNote.ST_IsCustomDescription);
			AssertEquals("Should be internal", nameof(StmNoteVisibility.INT), exportLogNote.ST_NoteType);
			Assert("Should be saved into the database", exportLogNote.IsInDatabase);
			AssertEquals(expectedNoteText, exportLogNote.ST_NoteText);
		}

		JASForwardingConsol Consol
		{
			get
			{
				if (fConsol == null)
				{
					fConsol = Factory.NewWithValidTestData<JASForwardingConsol>();
				}

				return fConsol;
			}
		}

		JXCExportLogger Logger
		{
			get
			{
				if (fLogger == null)
				{
					fLogger = new JXCExportLogger(Consol);
				}

				return fLogger;
			}
		}

		JXCExportLogger fLogger;
		JASForwardingConsol fConsol;
		#endregion
	}
}
