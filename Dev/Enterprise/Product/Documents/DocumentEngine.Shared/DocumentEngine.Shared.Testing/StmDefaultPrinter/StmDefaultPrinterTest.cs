using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Shared.Testing
{
	[TestedType(typeof(StmDefaultPrinter))]
	class StmDefaultPrinterTest : EnterpriseBusinessObjectTestCase
	{
		#region TestDeleteOfSubjectDeletesDefaultPrinter

		public void TestDeleteOfSubjectDeletesDefaultPrinter()
		{
			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";

			foreach (var tablePrefix in StmDefaultPrinter.ValidSubjectTables
				.Where(t => !BizOsThatCannotBeDeleted.Contains(t.TableName))
				.Select(t => ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(t.TableName)))
			{
				var subject = Factory.NewWithValidTestData(BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(tablePrefix));
				var defaultPrinter = StmDefaultPrinter.LoadOrCreateDefaultPrinter(Factory, subject);
				defaultPrinter.SDP_SQ_Printer = printer.PK;
				Factory.Save();
				AssertEquals("Subject should have successfully saved.", true, subject.IsInDatabase);
				AssertEquals("Default Printer should have successfully saved.", true, defaultPrinter.IsInDatabase);

				subject.Delete();

				AssertEquals(string.Format("Default Printer should have been deleted when subject [{0}] was deleted.", subject.GetType().FullName), true, defaultPrinter.IsDeleted);
				Factory.Save();
			}
		}

		HashSet<string> BizOsThatCannotBeDeleted
		{
			get
			{
				return bizOsThatCannotBeDeleted ?? (bizOsThatCannotBeDeleted = new HashSet<string>(new[]
					{
						DummyBizoSchema.Constants.TableName,
						WhsWarehouseSchema.Constants.TableName,
					}));
			}
		}

		HashSet<string> bizOsThatCannotBeDeleted;

		#endregion

		#region TestLoadDefaultPrinter

		public void TestLoadDefaultPrinter()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Test";
			menuItem.SU_BusinessContext = "Test";
			Factory.Save();

			var dummy = Factory.New<DummyBusinessObject>();
			AssertExceptionThrown<ArgumentNullException>(() => StmDefaultPrinter.LoadDefaultPrinter(Factory, null));
			AssertExceptionThrown<ArgumentNullException>(() => StmDefaultPrinter.LoadDefaultPrinter(null, dummy));
			AssertExceptionThrown<ArgumentNullException>(() => StmDefaultPrinter.LoadDefaultPrinter(Factory, null, menuItem));
			AssertNull("No Default Printer exists for this Dummy.", StmDefaultPrinter.LoadDefaultPrinter(Factory, dummy));
			AssertNull("No Default Printer exists for this Dummy.", StmDefaultPrinter.LoadDefaultPrinter(Factory, dummy, menuItem));

			AssertNull("No Default Printer exists for this Dummy.", StmDefaultPrinter.LoadDefaultPrinterUnsafe(Factory, dummy.PK, null, null));
			AssertNull("No Default Printer exists for this Dummy.", StmDefaultPrinter.LoadDefaultPrinterUnsafe(Factory, dummy.PK, new ZGuid("5F438223-1C1A-43F0-95D3-E857AAE268F9"), null));
			AssertNull("No Default Printer exists for this Dummy.", StmDefaultPrinter.LoadDefaultPrinterUnsafe(Factory, dummy.PK, menuItem.PK, null));

			var defaultPrinterWithMenuItem = Factory.New<StmDefaultPrinter>();
			defaultPrinterWithMenuItem.SDP_SubjectID = dummy.PK;
			defaultPrinterWithMenuItem.SDP_SU_Document = menuItem.PK;
			AssertNull("Default Printer exists for this Dummy but only for a specific Document.", StmDefaultPrinter.LoadDefaultPrinter(Factory, dummy));
			AssertEquals("Default Printer should be returned.", defaultPrinterWithMenuItem, StmDefaultPrinter.LoadDefaultPrinter(Factory, dummy, menuItem));
			AssertNull("Default Printer exists for this Dummy but only for a specific Document.", StmDefaultPrinter.LoadDefaultPrinterUnsafe(Factory, dummy.PK, null, null));
			AssertEquals("Default Printer should be returned even though it's unsafe.", defaultPrinterWithMenuItem, StmDefaultPrinter.LoadDefaultPrinterUnsafe(Factory, dummy.PK, menuItem.PK, null));
			AssertEquals("Default Printer should be registered child-editable.", true, dummy.IsRegisteredEditableChildObject(defaultPrinterWithMenuItem));

			defaultPrinterWithMenuItem.Delete();
			var defaultPrinterWithNoMenuItem = Factory.New<StmDefaultPrinter>();
			defaultPrinterWithNoMenuItem.SDP_SubjectID = dummy.PK;
			AssertNull("Default Printer exists but not for this specific Document.", StmDefaultPrinter.LoadDefaultPrinter(Factory, dummy, menuItem));
			AssertEquals("Default Printer should be returned.", defaultPrinterWithNoMenuItem, StmDefaultPrinter.LoadDefaultPrinter(Factory, dummy));
			AssertEquals("Default Printer should be registered child-editable.", true, dummy.IsRegisteredEditableChildObject(defaultPrinterWithNoMenuItem));
		}

		public void TestLoadDefaultPrinter_DocumentPivot()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Test";
			menuItem.SU_BusinessContext = "Test";

			var stmTemplate = Factory.NewWithValidTestData<StmTemplate>();

			var documentPivot = Factory.NewWithValidTestData<StmMenuTemplatePivot>();
			documentPivot.SI_SU = menuItem.PK;
			documentPivot.SI_SO = stmTemplate.PK;

			var subject = Factory.New<GlbStaff>();

			var defaultPrinter = Factory.NewWithValidTestData<StmDefaultPrinter>();
			defaultPrinter.SDP_SubjectID = subject.PK;
			defaultPrinter.SDP_SubjectTableCode = subject.TablePrefix;
			defaultPrinter.SDP_SU_Document = menuItem.PK;
			defaultPrinter.SDP_SI = documentPivot.PK;

			Factory.Save();

			AssertEquals("Default Printer should be returned.", defaultPrinter, StmDefaultPrinter.LoadDefaultPrinter(Factory, subject, menuItem, true, documentPivot));
			AssertEquals("Default Printer should be returned.", defaultPrinter, StmDefaultPrinter.LoadDefaultPrinterUnsafe(Factory, subject.PK, menuItem.PK, documentPivot.PK));

			var newPivot1 = Factory.NewWithValidTestData<StmMenuTemplatePivot>();
			newPivot1.SI_SU = menuItem.PK;
			newPivot1.SI_SO = stmTemplate.PK;

			var newDefaultPrinter1 = StmDefaultPrinter.LoadOrCreateDefaultPrinter(Factory, subject, menuItem, true, newPivot1);
			Assert("should be new created.", !newDefaultPrinter1.IsInDatabase);
			AssertEquals("should be newpivot1 pk.", newPivot1.PK, newDefaultPrinter1.SDP_SI);

			var newPivot2 = Factory.NewWithValidTestData<StmMenuTemplatePivot>();
			newPivot2.SI_SU = menuItem.PK;
			newPivot2.SI_SO = stmTemplate.PK;

			var newDefaultPrinter2 = StmDefaultPrinter.LoadOrCreateDefaultPrinterUnsafe(Factory, subject.PK, subject.TablePrefix, menuItem.PK, newPivot2.PK);
			Assert("should be new created.", !newDefaultPrinter2.IsInDatabase);
			AssertEquals("should be newpivot2 pk.", newPivot2.PK, newDefaultPrinter2.SDP_SI);
		}

		#endregion

		#region TestLoadOrCreateDefaultPrinter

		public void TestLoadOrCreateDefaultPrinter()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Test";
			menuItem.SU_BusinessContext = "Test";
			Factory.Save();

			var subject = Factory.New<GlbStaff>();
			AssertExceptionThrown<ArgumentNullException>(() => StmDefaultPrinter.LoadOrCreateDefaultPrinter(null, null, null));
			AssertExceptionThrown<ArgumentNullException>(() => StmDefaultPrinter.LoadOrCreateDefaultPrinter(Factory, null, null));
			AssertExceptionThrown<ArgumentNullException>(() => StmDefaultPrinter.LoadOrCreateDefaultPrinter(Factory, null, menuItem));

			var defaultPrinterCreatedWithNoMenuItem = StmDefaultPrinter.LoadOrCreateDefaultPrinter(Factory, subject);
			AssertEquals("Default Printer should be created with no Document Specified.", ZGuid.Empty, defaultPrinterCreatedWithNoMenuItem.SDP_SU_Document);
			AssertEquals("Default Printer should point to the Subject.", subject.PK, defaultPrinterCreatedWithNoMenuItem.SDP_SubjectID);
			AssertEquals("Default Printer should have the Correct Subject Table Code.", subject.TablePrefix, defaultPrinterCreatedWithNoMenuItem.SDP_SubjectTableCode);
			AssertEquals("Default Printer should be registered child-editable.", true, subject.IsRegisteredEditableChildObject(defaultPrinterCreatedWithNoMenuItem));
			AssertEquals("LoadOrCreate should return the Default Printer if it already exists.", defaultPrinterCreatedWithNoMenuItem, StmDefaultPrinter.LoadOrCreateDefaultPrinter(Factory, subject));

			var defaultPrinterCreatedWithMenuItem = StmDefaultPrinter.LoadOrCreateDefaultPrinter(Factory, subject, menuItem);
			AssertEquals("Default Printer should be created with Document Specified.", menuItem.PK, defaultPrinterCreatedWithMenuItem.SDP_SU_Document);
			AssertEquals("Default Printer should point to the Subject.", subject.PK, defaultPrinterCreatedWithMenuItem.SDP_SubjectID);
			AssertEquals("Default Printer should have the Correct Subject Table Code.", subject.TablePrefix, defaultPrinterCreatedWithMenuItem.SDP_SubjectTableCode);
			AssertEquals("Default Printer should be registered child-editable.", true, subject.IsRegisteredEditableChildObject(defaultPrinterCreatedWithMenuItem));
			AssertEquals("LoadOrCreate should return the Default Printer if it already exists.", defaultPrinterCreatedWithMenuItem, StmDefaultPrinter.LoadOrCreateDefaultPrinter(Factory, subject, menuItem));

			var defaultPrinterCreatedWithNoMenuItemPK = StmDefaultPrinter.LoadOrCreateDefaultPrinterUnsafe(Factory, subject.PK, subject.TablePrefix, null, null);
			AssertEquals("Default Printer should be created with no Document Specified.", ZGuid.Empty, defaultPrinterCreatedWithNoMenuItemPK.SDP_SU_Document);
			AssertEquals("Default Printer should point to the Subject.", subject.PK, defaultPrinterCreatedWithNoMenuItemPK.SDP_SubjectID);
			AssertEquals("Default Printer should have the Correct Subject Table Code.", subject.TablePrefix, defaultPrinterCreatedWithNoMenuItemPK.SDP_SubjectTableCode);
			AssertEquals("Default Printer should be registered child-editable.", true, subject.IsRegisteredEditableChildObject(defaultPrinterCreatedWithNoMenuItemPK));
			AssertEquals("LoadOrCreate should return the Default Printer if it already exists.", defaultPrinterCreatedWithNoMenuItemPK, StmDefaultPrinter.LoadOrCreateDefaultPrinterUnsafe(Factory, subject.PK, subject.TablePrefix, null, null));

			var newPK = new ZGuid("77B590FC-5A4F-428E-ADBC-104651F7A6D8");
			var defaultPrinterCreatedWithBadMenuItemPK = StmDefaultPrinter.LoadOrCreateDefaultPrinterUnsafe(Factory, subject.PK, subject.TablePrefix, newPK, null);
			AssertEquals("Default Printer should be created with Document Specified.", newPK, defaultPrinterCreatedWithBadMenuItemPK.SDP_SU_Document);
			AssertEquals("Default Printer should point to the Subject.", subject.PK, defaultPrinterCreatedWithBadMenuItemPK.SDP_SubjectID);
			AssertEquals("Default Printer should have the Correct Subject Table Code.", subject.TablePrefix, defaultPrinterCreatedWithBadMenuItemPK.SDP_SubjectTableCode);
			AssertEquals("Default Printer should not be registered child-editable.", false, subject.IsRegisteredEditableChildObject(defaultPrinterCreatedWithBadMenuItemPK));
			AssertEquals("LoadOrCreate should return the Default Printer if it already exists.", defaultPrinterCreatedWithBadMenuItemPK, StmDefaultPrinter.LoadOrCreateDefaultPrinterUnsafe(Factory, subject.PK, subject.TablePrefix, newPK, null));

			var defaultPrinterLoadedWithMenuItemPK = StmDefaultPrinter.LoadOrCreateDefaultPrinterUnsafe(Factory, subject.PK, subject.TablePrefix, menuItem.PK, null);
			AssertEquals("Default Printer should be created with Document Specified.", menuItem.PK, defaultPrinterLoadedWithMenuItemPK.SDP_SU_Document);
			AssertEquals("Default Printer should point to the Subject.", subject.PK, defaultPrinterLoadedWithMenuItemPK.SDP_SubjectID);
			AssertEquals("Default Printer should have the Correct Subject Table Code.", subject.TablePrefix, defaultPrinterLoadedWithMenuItemPK.SDP_SubjectTableCode);
			AssertEquals("Default Printer should be registered child-editable.", true, subject.IsRegisteredEditableChildObject(defaultPrinterLoadedWithMenuItemPK));
			AssertEquals("LoadOrCreate should return the Default Printer if it already exists.", defaultPrinterLoadedWithMenuItemPK, StmDefaultPrinter.LoadOrCreateDefaultPrinterUnsafe(Factory, subject.PK, subject.TablePrefix, menuItem.PK, null));
		}

		#endregion

		#region TestSetPrinterPKOrDeleteIfEmpty

		public void TestSetPrinterPKOrDeleteIfEmpty()
		{
			var printer1 = Factory.New<IStmPrintQueue>();
			var printer2 = Factory.New<IStmPrintQueue>();
			printer1.QueueName = "PRINTER1";
			printer2.QueueName = "PRINTER2";

			var subject = Factory.New<GlbStaff>();
			AssertExceptionThrown<ArgumentNullException>(() => StmDefaultPrinter.SetPrinterPKOrDeleteIfEmpty(null, null, printer1.PK));
			AssertExceptionThrown<ArgumentNullException>(() => StmDefaultPrinter.SetPrinterPKOrDeleteIfEmpty(Factory, null, printer1.PK));

			StmDefaultPrinter.SetPrinterPKOrDeleteIfEmpty(Factory, subject, ZGuid.Empty);
			AssertNull("No Default Printer should be created if setting to Empty Printer.", StmDefaultPrinter.LoadDefaultPrinter(Factory, subject));

			StmDefaultPrinter.SetPrinterPKOrDeleteIfEmpty(Factory, subject, printer1.PK);
			var defaultPrinter = StmDefaultPrinter.LoadDefaultPrinter(Factory, subject);
			AssertEquals("Should create a Default Printer if it does not exist and set the Printer PK.", defaultPrinter.SDP_SQ_Printer, printer1.PK);

			bool isValidationSuspended = false;
			defaultPrinter.SDP_SQ_PrinterInfo.ValueChanged += (sender, e) => isValidationSuspended = defaultPrinter.IsValidationSuspended;
			StmDefaultPrinter.SetPrinterPKOrDeleteIfEmpty(Factory, subject, printer2.PK);
			AssertEquals("If default printer exists it should update it.", defaultPrinter.SDP_SQ_Printer, printer2.PK);
			AssertEquals("Validation should be suspended if specified to.", true, isValidationSuspended);

			StmDefaultPrinter.SetPrinterPKOrDeleteIfEmpty(Factory, subject, printer1.PK, suspendValidation: false);
			AssertEquals("If default printer exists it should update it.", defaultPrinter.SDP_SQ_Printer, printer1.PK);
			AssertEquals("Validation should not be suspended if not specified to.", false, isValidationSuspended);

			StmDefaultPrinter.SetPrinterPKOrDeleteIfEmpty(Factory, subject, ZGuid.Empty);
			AssertEquals("If setting to Empty Printer PK, the Default Printer should be deleted.", true, defaultPrinter.IsDeleted);
		}

		#endregion

		#region TestSubjectTableCodeConstraints

		public void TestSubjectTableCodeConstraint_Allowed_GS() => AssertSubjectTableCodeConstraint("GS", true);
		public void TestSubjectTableCodeConstraint_Allowed_KP() => AssertSubjectTableCodeConstraint("KP", true);
		public void TestSubjectTableCodeConstraint_Allowed_WA() => AssertSubjectTableCodeConstraint("WA", true);
		public void TestSubjectTableCodeConstraint_Allowed_WW() => AssertSubjectTableCodeConstraint("WW", true);
		public void TestSubjectTableCodeConstraint_NotAllowedOtherValues_ZZZ() => AssertSubjectTableCodeConstraint("ZZZ", false);
		public void TestSubjectTableCodeConstraint_NotAllowedOtherValues_VV() => AssertSubjectTableCodeConstraint("VV", false);

		public void AssertSubjectTableCodeConstraint(string subjectTableCode, bool expectedAllowed)
		{
			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "PRINTER";
			Factory.Save();

			var cmd = TestConnection.Command(@"
INSERT INTO
	StmDefaultPrinter (SDP_PK, SDP_SubjectTableCode, SDP_SubjectID, SDP_SQ_Printer, SDP_SystemLastEditUser, SDP_SystemLastEditTimeUtc, SDP_SystemCreateUser, SDP_SystemCreateTimeUtc)
VALUES
	(NEWID(), @SubjectTableCode, @SubjectID, @PrinterPK, 'A', GETUTCDATE(), 'A', GETUTCDATE())");
			cmd.AddParameter("@SubjectTableCode", SqlDbType.VarChar, subjectTableCode);
			cmd.AddParameter("@SubjectID", SqlDbType.UniqueIdentifier, Guid.NewGuid());
			cmd.AddParameter("@PrinterPK", SqlDbType.UniqueIdentifier, printer.PK.ToGuid());

			if(expectedAllowed)
			{
				AssertNoExceptionThrown(() => cmd.ExecuteNonQuery());
			}
			else
			{
				var expectedErrorMsg = "The INSERT statement conflicted with the CHECK constraint \"Constraint_SDP_SubjectTableCode\".";
				AssertInnermostException("SqlException should throw", typeof(SqlException), expectedErrorMsg, () => cmd.ExecuteNonQuery(), true);
			}
		}

		#endregion
	}
}
