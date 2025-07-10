using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Accounting.DataTransfer.Testing.Universal
{
	partial class TransactionImporterTest : TestCaseWithFactory
	{
		public void TestImportJournalLineWhenDeactiveSupplyType()
		{
			DeactiveSomeSupplyType("INT");
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			AssertTransactionLines(new AssertTransactionLinesDelegate[] { CreateTestingJournalLineWithValidSupplyType, CreateTestingJournalLineWithNullSupplyType, CreateTestingJournalLineWithEmptySupplyType, CreateTestingJournalLineWithInvalidSupplyType });
		}

		public void TestImportJournalLineWithSupplyType()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			AssertTransactionLines(new AssertTransactionLinesDelegate[] { CreateTestingJournalLineWithValidSupplyType, CreateTestingJournalLineWithNullSupplyType, CreateTestingJournalLineWithEmptySupplyType });
		}

		public void TestImportJournalLineWithSupplyType_FailedByInvalidSupplyType()
		{
			DeactiveSomeSupplyType("INT");
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			AssertTransactionLines(
				new AssertTransactionLinesDelegate[] { CreateTestingJournalLineWithInvalidSupplyType }
				, (postingJournal, line) =>
				{
					line.RunPreSaveValidation();
					AssertHasError(line.AL_SupplyTypeInfo, "Enter a valid selection.");
				});
		}

		public void TestImportJournalLineWithSupplyTypeMandatory()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			AssertTransactionLines(new AssertTransactionLinesDelegate[] { CreateTestingJournalLineWithValidSupplyType });
		}

		public void TestImportJournalLineWithSupplyTypeMandatory_FailedByEmptySupplyType()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			AssertTransactionLines(
				new AssertTransactionLinesDelegate[] { CreateTestingJournalLineWithNullSupplyType, CreateTestingJournalLineWithEmptySupplyType }
				, (postingJournal, line) =>
				{
					line.RunPreSaveValidation();
					AssertHasError(line.AL_SupplyTypeInfo, "Please enter a value.");
				});
		}

		public void TestImportJournalLineWithSupplyTypeMandatory_FailedByInvalidSupplyType()
		{
			DeactiveSomeSupplyType("INT");
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			AssertTransactionLines(
				new AssertTransactionLinesDelegate[] { CreateTestingJournalLineWithInvalidSupplyType }
				, (postingJournal, line) =>
				{
					line.RunPreSaveValidation();
					AssertHasError(line.AL_SupplyTypeInfo, "Enter a valid selection.");
				});
		}

		void DeactiveSomeSupplyType(params string[] supplyTypeCodes)
		{
			var settingCollection = AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.Value;
			foreach (var supplyTypeCode in supplyTypeCodes)
			{
				var target = settingCollection.FindByCode(supplyTypeCode) as CodeDescriptionBool;
				if (target != null)
				{
					target.Bool = false;
				}
			}

			AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, settingCollection);
		}

		PostingJournal CreateTestingJournalLineWithValidSupplyType()
		{
			var postingJournal = CreateTestingJournalLine();
			postingJournal.SupplyType = new CodeDescriptionPair { Code = "DSB" };
			return postingJournal;
		}

		PostingJournal CreateTestingJournalLineWithInvalidSupplyType()
		{
			var postingJournal = CreateTestingJournalLine();
			postingJournal.SupplyType = new CodeDescriptionPair { Code = "INT" };
			return postingJournal;
		}

		PostingJournal CreateTestingJournalLineWithNullSupplyType()
		{
			var postingJournal = CreateTestingJournalLine();
			postingJournal.SupplyType = null;
			return postingJournal;
		}

		PostingJournal CreateTestingJournalLineWithEmptySupplyType()
		{
			var postingJournal = CreateTestingJournalLine();
			postingJournal.SupplyType = new CodeDescriptionPair { Code = "" };
			return postingJournal;
		}
	}
}
