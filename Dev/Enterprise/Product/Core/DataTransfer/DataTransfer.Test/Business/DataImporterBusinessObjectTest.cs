using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	[TestedType(typeof(DataImporterBusinessObject))]
	public class DataImporterBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestRecordsAddedUpdatedReadOnly()
		{
			AssertEquals("Must be readonly for the gui", true, BO.RecordsAddedInfo.ReadOnly);
			AssertEquals("Must be readonly for the gui", true, BO.RecordsUpdatedInfo.ReadOnly);
		}

		public void TestDelayNotificationWithMessageForAfterSaveToEnd()
		{
			BO.OnBeforeImport();
			TestBusinessObjectToImport mockBusinessObjectToImport = new TestBusinessObjectToImport();
			BO.Notify(new BusinessObjectCreatedOrUpdatedNotification(mockBusinessObjectToImport));

			mockBusinessObjectToImport.MockOnSaving();
			BO.OnAfterImport(false);
			AssertEquals(
				"Should show business object updated with an identifier which is populated on saving near the end of the import" +
				"(not populated at the time of BusinessObjectCreatedOrUpdatedNotification notification)",
				true, ProgressText.Contains("Mock with id newid1234 created"));
		}

		public void TestUpdateRecordCountOnly()
		{
			BO.OnAppendProgressText += new EventHandler<DataImporterBusinessObject.TextAppendedEventArgs>(BO_OnAppendProgressText);
			BO.OnBeforeImport();
			TestBusinessObjectToImport mockBusinessObjectToImport = new TestBusinessObjectToImport();
			BO.Notify(new BusinessObjectCreatedOrUpdatedNotification(mockBusinessObjectToImport));

			mockBusinessObjectToImport.MockOnSaving();
			BO.OnAfterImport(false);
			AssertContains("Mock with id newid1234 created", ProgressText);
			AssertEquals((ZInt)1, BO.RecordsAdded);

			mockBusinessObjectToImport = new TestBusinessObjectToImport();
			mockBusinessObjectToImport.MockNumberFountainNumber = "IDThatShouldntShow";
			BusinessObjectCreatedOrUpdatedNotification notify = new BusinessObjectCreatedOrUpdatedNotification(mockBusinessObjectToImport);
			notify.UpdateRecordCountOnlyWithoutMessage = true;
			BO.Notify(notify);

			mockBusinessObjectToImport.MockOnSaving();
			BO.OnAfterImport(false);
			AssertNotContains("IDThatShouldntShow", ProgressText);
			AssertEquals((ZInt)2, BO.RecordsAdded);
		}

		public void TestOnBeforeImport_OnAfterImport()
		{
			BO.OnBeforeImport();
			TestBusinessObjectToImport mockBusinessObjectToImport = new TestBusinessObjectToImport();
			BO.Notify(new BusinessObjectCreatedOrUpdatedNotification(mockBusinessObjectToImport));
			BO.OnAfterImport(false);

			AssertEquals("Should think it updated 1 record", 1, BO.RecordsAdded);
			AssertNotNullOrEmpty("Should have a progress", ProgressText);
			BO.OnBeforeImport();
			AssertEquals("Should reset due to another import", "", ProgressText.Trim());
		}

		public void TestShowNotification()
		{
			BO.OnBeforeImport();
			AssertEquals("Initially there is no text", "", ProgressText);
			BO.Notify(new InfoNotification(null));
			AssertEquals("After a null info notification", "", ProgressText);
			BO.Notify(new InfoNotification("Some Info"));
			AssertEquals("After an info notification", "Some Info\r\n", ProgressText);
			BO.Notify(new NewlineNotification());
			AssertEquals("After a 'newline' notification", "Some Info\r\n\r\n", ProgressText);
		}

		public void TestProgressText_ForSuccessfulImport()
		{
			BO.ImportOperationDescription = "Importing Stuff";

			BO.OnBeforeImport();
			BO.Notify(new InfoNotification("Some Info"));
			BO.OnAfterImport(false);

			AssertEquals("Importing Stuff\r\n\r\nSome Info\r\n\r\nData Import completed. See notifications above", ProgressText.Trim());
		}

		public void TestProgressText_ForFailedImport()
		{
			BO.ImportOperationDescription = "Importing Stuff";
			BO.OnBeforeImport();
			BO.Notify(new InfoNotification("Some Info"));
			BO.RecordsAdded = 10;
			BO.RecordsUpdated = 5;
			BO.OnAfterImport(true);
			AssertEquals("Importing Stuff\r\n\r\nSome Info\r\n\r\nNo changes were made due to the above errors. Please fix the errors and try again.", ProgressText.Trim());
			AssertEquals(0, BO.RecordsAdded);
			AssertEquals(0, BO.RecordsUpdated);
		}

		public void TestRecordAddedUpdatedCounts()
		{
			OrgHeader bizObjInDB = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			OrgHeader bizObjNotInDB = Factory.NewWithValidTestData<OrgHeader>();

			DataImporterBusinessObject importBO = new DataImporterBusinessObject(Factory);
			AssertEquals("No records added initially for test", 0, importBO.RecordsAdded);
			AssertEquals("No records updated initially for test", 0, importBO.RecordsUpdated);

			importBO.Notify(new BusinessObjectCreatedOrUpdatedNotification(bizObjNotInDB));
			AssertEquals("Record added", 1, importBO.RecordsAdded);
			AssertEquals("No records updated initially", 0, importBO.RecordsUpdated);
			importBO.Notify(new BusinessObjectCreatedOrUpdatedNotification(bizObjInDB));
			AssertEquals("Record still added", 1, importBO.RecordsAdded);
			AssertEquals("Record updated", 1, importBO.RecordsUpdated);
		}

		protected override void SetUp()
		{
			base.SetUp();
			BO = new DataImporterBusinessObject(Factory);
			BO.OnResetProgressText += new EventHandler(BO_OnResetProgressText);
			BO.OnAppendProgressText += new EventHandler<DataImporterBusinessObject.TextAppendedEventArgs>(BO_OnAppendProgressText);
			ProgressText = string.Empty;
		}

		void BO_OnResetProgressText(object sender, EventArgs e)
		{
			ProgressText = string.Empty;
		}

		DataImporterBusinessObject BO;
		string ProgressText;

		void BO_OnAppendProgressText(object sender, DataImporterBusinessObject.TextAppendedEventArgs e)
		{
			ProgressText += e.NewText;
		}

		class TestBusinessObjectToImport : NonPersistentBusinessObject
		{
			public void MockOnSaving()
			{
				MockNumberFountainNumber = "newid1234";
			}

			public string MockNumberFountainNumber;

			protected override ZString HumanReadableNameCore
			{
				get { return "Mock with id " + MockNumberFountainNumber; }
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DataImporterBusinessObject(Factory);
		}
	}
}
