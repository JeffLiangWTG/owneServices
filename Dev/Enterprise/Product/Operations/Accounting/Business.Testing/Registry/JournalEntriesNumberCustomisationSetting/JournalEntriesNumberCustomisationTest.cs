using System;
using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.Registry.Business.TransactionNumberSequenceCustomisation;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JournalEntriesNumberCustomisation))]
	public class JournalEntriesNumberCustomisationTest : TransactionNumberSequenceCustomisationTest
	{
		#region Properties

		public void TestDescription()
		{
			AssertEquals("Accounting Period as 2 digits, 202401 = 01, 202402 = 02 ... 202412 = 12", journalEntriesNumberCustomisationAccountingPeriodAs2Digits.Description);
			AssertEquals("Accounting Year as 1, 2 or 4 digits. For example: as a single digit(2024 = 4, 2020 = 0), as a double digit(2024 = 24, 2020 = 20) and as 4 digits(2024 = 2024)", journalEntriesNumberCustomisationAccountingYearAsDigits.Description);
			AssertEquals("Jan = A, Feb = B ... Dec = L", journalEntriesNumberCustomisationCalendarMonthAsLetter.Description);
			AssertEquals("2001 = A, 2002 = B ... 2026 = Z", journalEntriesNumberCustomisationCalendarYearAsLetter.Description);
			AssertEquals("Calendar Month as 2 digits, JAN = 01, FEB = 02 ... DEC = 12", journalEntriesNumberCustomisationCalendarMonthAs2Digits.Description);
			AssertEquals("Calendar Year as 1, 2 or 4 digits. For example: as a single digit(2024 = 4, 2020 = 0), as a double digit(2024 = 24, 2020 = 20) and as 4 digits(2024 = 2024)", journalEntriesNumberCustomisationCalendarYearAsDigits.Description);
			AssertEquals("Group codes as configured in the 'Journal Entries Classification Group' registry", journalEntriesNumberCustomisationJournalEntriesClassificationGroupCode.Description);
			AssertEquals("Prefix as configured in the 'Transaction Type Prefix' Registry", journalEntriesNumberCustomisationTransactionTypePrefix.Description);
		}

		public void TestLength()
		{
			AssertEquals(GlbBranchSchema.GB_Code.MaxLength, journalEntriesNumberCustomisationJournalEntriesBranchCode.Length);
			AssertEquals(GlbDepartmentSchema.GE_Code.MaxLength, journalEntriesNumberCustomisationJournalEntriesDepartmentCode.Length);
			AssertEquals(2, journalEntriesNumberCustomisationCalendarMonthAs2Digits.Length);
			AssertEquals(3, journalEntriesNumberCustomisationJournalEntriesClassificationGroupCode.Length);
			AssertEquals(1, journalEntriesNumberCustomisationCalendarMonthAsLetter.Length);
			AssertEquals(1, journalEntriesNumberCustomisationCalendarYearAsLetter.Length);
			AssertEquals(4, journalEntriesNumberCustomisationAccountingYearAsDigits.Length);
			AssertEquals(4, journalEntriesNumberCustomisationCalendarYearAsDigits.Length);
			AssertEquals(0, journalEntriesNumberCustomisationJournalEntriesClassificationGroupCodePrefix.Length);

			var journalEntriesClassificationGroupCodeCollection = new JournalEntriesClassificationGroupCodeCollection();
			var journalEntriesClassificationGroupCode = journalEntriesClassificationGroupCodeCollection.AddNew();
			journalEntriesClassificationGroupCode.Code = "TST";
			journalEntriesClassificationGroupCode.Description = "Description";
			var journalEntriesClassificationGroupCode2 = journalEntriesClassificationGroupCodeCollection.AddNew();
			journalEntriesClassificationGroupCode2.Code = "AAA";
			journalEntriesClassificationGroupCode2.Description = "Description";

			AssertEquals(0, journalEntriesNumberCustomisationJournalEntriesClassificationGroupCodePrefix.Length);
			journalEntriesClassificationGroupCode.Prefix = "dd";

			AccountingConfigurationRegistry.Instance.JournalEntriesClassificationGroupCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, journalEntriesClassificationGroupCodeCollection);
			AssertEquals(2, journalEntriesNumberCustomisationJournalEntriesClassificationGroupCodePrefix.Length);

			journalEntriesClassificationGroupCode2.Prefix = "CCC";
			AccountingConfigurationRegistry.Instance.JournalEntriesClassificationGroupCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, journalEntriesClassificationGroupCodeCollection);
			AssertEquals(3, journalEntriesNumberCustomisationJournalEntriesClassificationGroupCodePrefix.Length);
		}

		#endregion

		#region ReadOnly

		public void TestFountain_ReadOnly()
		{
			AssertFountainReadOnly(journalEntriesNumberCustomisationAccountingPeriodAs2Digits, false);
			AssertFountainReadOnly(journalEntriesNumberCustomisationAccountingYearAsDigits, false);
			AssertFountainReadOnly(journalEntriesNumberCustomisationAccountingYearAsLetter, false);
			AssertFountainReadOnly(journalEntriesNumberCustomisationCalendarMonthAs2Digits, false);
			AssertFountainReadOnly(journalEntriesNumberCustomisationCalendarMonthAsLetter, false);
			AssertFountainReadOnly(journalEntriesNumberCustomisationCalendarYearAsDigits, false);
			AssertFountainReadOnly(journalEntriesNumberCustomisationCalendarYearAsLetter, false);
			AssertFountainReadOnly(journalEntriesNumberCustomisationCustomElement1, false);
			AssertFountainReadOnly(journalEntriesNumberCustomisationCustomElement2, false);
			AssertFountainReadOnly(journalEntriesNumberCustomisationJournalEntriesBranchCode, false);
			AssertFountainReadOnly(journalEntriesNumberCustomisationJournalEntriesClassificationGroupCode, true);
			AssertFountainReadOnly(journalEntriesNumberCustomisationJournalEntriesDepartmentCode, false);
			AssertFountainReadOnly(journalEntriesNumberCustomisationSequenceNumber, true);
			AssertFountainReadOnly(journalEntriesNumberCustomisationTransactionTypePrefix, true);
			AssertFountainReadOnly(journalEntriesNumberCustomisationJournalEntriesClassificationGroupCodePrefix, true);

			void AssertFountainReadOnly(JournalEntriesNumberCustomisation journalEntriesNumberCustomisation, bool expectReadOnly)
			{
				journalEntriesNumberCustomisation.Include = true;
				AssertEquals(expectReadOnly, journalEntriesNumberCustomisation.FountainInfo.ReadOnly);
			}
		}

		public void TestCode_ReadOnly()
		{
			AssertCodeReadOnly(journalEntriesNumberCustomisationAccountingPeriodAs2Digits, true);
			AssertCodeReadOnly(journalEntriesNumberCustomisationAccountingYearAsDigits, false);
			AssertCodeReadOnly(journalEntriesNumberCustomisationAccountingYearAsLetter, true);
			AssertCodeReadOnly(journalEntriesNumberCustomisationCalendarMonthAs2Digits, true);
			AssertCodeReadOnly(journalEntriesNumberCustomisationCalendarMonthAsLetter, true);
			AssertCodeReadOnly(journalEntriesNumberCustomisationCalendarYearAsDigits, false);
			AssertCodeReadOnly(journalEntriesNumberCustomisationCalendarYearAsLetter, true);
			AssertCodeReadOnly(journalEntriesNumberCustomisationCustomElement1, false);
			AssertCodeReadOnly(journalEntriesNumberCustomisationCustomElement2, false);
			AssertCodeReadOnly(journalEntriesNumberCustomisationJournalEntriesBranchCode, true);
			AssertCodeReadOnly(journalEntriesNumberCustomisationJournalEntriesClassificationGroupCode, true);
			AssertCodeReadOnly(journalEntriesNumberCustomisationJournalEntriesDepartmentCode, true);
			AssertCodeReadOnly(journalEntriesNumberCustomisationSequenceNumber, true);
			AssertCodeReadOnly(journalEntriesNumberCustomisationTransactionTypePrefix, true);

			void AssertCodeReadOnly(JournalEntriesNumberCustomisation journalEntriesNumberCustomisation, bool expectReadOnly)
			{
				journalEntriesNumberCustomisation.Include = true;
				AssertEquals(expectReadOnly, journalEntriesNumberCustomisation.CodeInfo.ReadOnly);
			}
		}

		#endregion

		#region Validation

		public void TestValidateLength()
		{
			Parent.NumberRule = AccountingConstants.JournalEntriesNumberCustomisationNumberRule.ALL;
			journalEntriesNumberCustomisationSequenceNumber.Length = 0;
			journalEntriesNumberCustomisationSequenceNumber.RunPreSaveValidation();
			AssertHasErrors("The Length of 'Sequence Number' cannot be smaller than 5.", journalEntriesNumberCustomisationSequenceNumber.LengthInfo);
			Assert(journalEntriesNumberCustomisationSequenceNumber.LengthInfo.HasWarning("The Length of 'Sequence Number' should not be smaller than 10 if there are no other data element used to generate the number fountain when the 'Number Rule' is set to 'ALL' or 'GRP'."));

			journalEntriesNumberCustomisationSequenceNumber.Length = 8;
			journalEntriesNumberCustomisationSequenceNumber.RunPreSaveValidation();
			AssertEquals(false, journalEntriesNumberCustomisationSequenceNumber.LengthInfo.HasErrors());
			Assert(journalEntriesNumberCustomisationSequenceNumber.LengthInfo.HasWarning("The Length of 'Sequence Number' should not be smaller than 10 if there are no other data element used to generate the number fountain when the 'Number Rule' is set to 'ALL' or 'GRP'."));

			Parent.NumberRule = AccountingConstants.JournalEntriesNumberCustomisationNumberRule.TRN;
			journalEntriesNumberCustomisationSequenceNumber.RunPreSaveValidation();
			AssertEquals(false, journalEntriesNumberCustomisationSequenceNumber.LengthInfo.HasWarnings());

			Parent.NumberRule = AccountingConstants.JournalEntriesNumberCustomisationNumberRule.GRP;
			journalEntriesNumberCustomisationSequenceNumber.RunPreSaveValidation();
			Assert(journalEntriesNumberCustomisationSequenceNumber.LengthInfo.HasWarning("The Length of 'Sequence Number' should not be smaller than 10 if there are no other data element used to generate the number fountain when the 'Number Rule' is set to 'ALL' or 'GRP'."));

			journalEntriesNumberCustomisationCalendarYearAsLetter.Fountain = true;
			journalEntriesNumberCustomisationSequenceNumber.RunPreSaveValidation();
			AssertEquals(false, journalEntriesNumberCustomisationSequenceNumber.LengthInfo.HasWarnings());
		}

		public void TestValidateInclude()
		{
			var journalEntriesClassificationGroupCodeCollection = new JournalEntriesClassificationGroupCodeCollection();
			var journalEntriesClassificationGroupCode = journalEntriesClassificationGroupCodeCollection.AddNew();
			journalEntriesClassificationGroupCode.Code = "TST";
			journalEntriesClassificationGroupCode.Description = "Description";
			AccountingConfigurationRegistry.Instance.JournalEntriesClassificationGroupCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, journalEntriesClassificationGroupCodeCollection);

			var journalEntriesClassificationGroupCollection = new JournalEntriesClassificationGroupCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			var journalEntriesClassificationGroup = journalEntriesClassificationGroupCollection.AddNew();
			journalEntriesClassificationGroup.GroupCode = string.Empty;
			AccountingConfigurationRegistry.Instance.JournalEntriesClassificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, journalEntriesClassificationGroupCollection);

			journalEntriesNumberCustomisationJournalEntriesClassificationGroupCode.Include = true;
			journalEntriesNumberCustomisationJournalEntriesClassificationGroupCode.RunPreSaveValidation();
			AssertHasErrors("Group Code in registry 'Journal Entries Classification Group' should be set when include 'Journal Entries Classification Group Code'.", journalEntriesNumberCustomisationJournalEntriesClassificationGroupCode.IncludeInfo);

			journalEntriesClassificationGroup.GroupCode = "TST";
			AccountingConfigurationRegistry.Instance.JournalEntriesClassificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, journalEntriesClassificationGroupCollection);

			journalEntriesNumberCustomisationJournalEntriesClassificationGroupCode.RunPreSaveValidation();
			AssertEquals(false, journalEntriesNumberCustomisationJournalEntriesClassificationGroupCode.IncludeInfo.HasErrors());

			journalEntriesNumberCustomisationJournalEntriesClassificationGroupCodePrefix.Include = true;
			journalEntriesNumberCustomisationJournalEntriesClassificationGroupCodePrefix.RunPreSaveValidation();
			AssertHasErrors("Group Code Prefix in registry 'Journal Entries Classification Group Code' should be set when include 'Journal Entries Classification Group Code Prefix'.", journalEntriesNumberCustomisationJournalEntriesClassificationGroupCodePrefix.IncludeInfo);

			var journalEntriesClassificationGroupCode2 = journalEntriesClassificationGroupCodeCollection.AddNew();
			journalEntriesClassificationGroupCode2.Code = "TST";
			journalEntriesClassificationGroupCode2.Description = "Description";
			journalEntriesClassificationGroupCode2.Prefix = "付";
			journalEntriesNumberCustomisationJournalEntriesClassificationGroupCodePrefix.RunPreSaveValidation();
			AssertHasErrors("Group Code Prefix in registry 'Journal Entries Classification Group Code' should be set when include 'Journal Entries Classification Group Code Prefix'.", journalEntriesNumberCustomisationJournalEntriesClassificationGroupCodePrefix.IncludeInfo);

			journalEntriesClassificationGroupCode.Prefix = "收";
			journalEntriesNumberCustomisationJournalEntriesClassificationGroupCodePrefix.RunPreSaveValidation();
			AssertEquals(false, journalEntriesNumberCustomisationJournalEntriesClassificationGroupCode.IncludeInfo.HasErrors());
		}

		#endregion

		#region Code

		public override void TestCode()
		{
			journalEntriesNumberCustomisationCalendarYearAsDigits = new JournalEntriesNumberCustomisation { ElementName = JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.CalendarYearAsDigits, Include = true, Order = 1 };
			journalEntriesNumberCustomisationCalendarYearAsDigits.RunPreSaveValidation();

			AssertYearCode(journalEntriesNumberCustomisationCalendarYearAsDigits);
		}

		#endregion

		#region Implement

		protected override void SetUp()
		{
			base.SetUp();

			Parent = new JournalEntriesNumberCustomisationSetting(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			var numberSequenceCustomisations = Parent.NumberSequenceCustomisations.Cast<JournalEntriesNumberCustomisation>();
			journalEntriesNumberCustomisationAccountingPeriodAs2Digits = numberSequenceCustomisations.First(x => x.ElementName == TransactionNumberSequenceCustomisation.ElementNames.AccountingPeriodAs2Digits);
			journalEntriesNumberCustomisationAccountingYearAsDigits = numberSequenceCustomisations.First(x => x.ElementName == TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits);
			journalEntriesNumberCustomisationAccountingYearAsLetter = numberSequenceCustomisations.First(x => x.ElementName == TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsLetter);
			journalEntriesNumberCustomisationCalendarMonthAs2Digits = numberSequenceCustomisations.First(x => x.ElementName == JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.CalendarMonthAs2Digits);
			journalEntriesNumberCustomisationCalendarMonthAsLetter = numberSequenceCustomisations.First(x => x.ElementName == JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.CalendarMonthAsLetter);
			journalEntriesNumberCustomisationCalendarYearAsDigits = numberSequenceCustomisations.First(x => x.ElementName == JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.CalendarYearAsDigits);
			journalEntriesNumberCustomisationCalendarYearAsLetter = numberSequenceCustomisations.First(x => x.ElementName == JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.CalendarYearAsLetter);
			journalEntriesNumberCustomisationCustomElement1 = numberSequenceCustomisations.First(x => x.ElementName == TransactionNumberSequenceCustomisation.ElementNames.CustomElement1);
			journalEntriesNumberCustomisationCustomElement2 = numberSequenceCustomisations.First(x => x.ElementName == TransactionNumberSequenceCustomisation.ElementNames.CustomElement2);
			journalEntriesNumberCustomisationJournalEntriesBranchCode = numberSequenceCustomisations.First(x => x.ElementName == JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.JournalEntriesBranchCode);
			journalEntriesNumberCustomisationJournalEntriesClassificationGroupCode = numberSequenceCustomisations.First(x => x.ElementName == JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.JournalEntriesClassificationGroupCode);
			journalEntriesNumberCustomisationJournalEntriesDepartmentCode = numberSequenceCustomisations.First(x => x.ElementName == JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.JournalEntriesDepartmentCode);
			journalEntriesNumberCustomisationSequenceNumber = numberSequenceCustomisations.First(x => x.ElementName == TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber);
			journalEntriesNumberCustomisationTransactionTypePrefix = numberSequenceCustomisations.First(x => x.ElementName == TransactionNumberSequenceCustomisation.ElementNames.TransactionTypePrefix);
			journalEntriesNumberCustomisationJournalEntriesClassificationGroupCodePrefix = numberSequenceCustomisations.First(x => x.ElementName == JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.JournalEntriesClassificationGroupCodePrefix);
		}

		JournalEntriesNumberCustomisationSetting Parent;
		JournalEntriesNumberCustomisation journalEntriesNumberCustomisationAccountingPeriodAs2Digits, journalEntriesNumberCustomisationAccountingYearAsDigits, journalEntriesNumberCustomisationAccountingYearAsLetter, journalEntriesNumberCustomisationCalendarMonthAs2Digits
			, journalEntriesNumberCustomisationCalendarMonthAsLetter, journalEntriesNumberCustomisationCalendarYearAsDigits, journalEntriesNumberCustomisationCalendarYearAsLetter, journalEntriesNumberCustomisationCustomElement1, journalEntriesNumberCustomisationCustomElement2
			, journalEntriesNumberCustomisationJournalEntriesBranchCode, journalEntriesNumberCustomisationJournalEntriesClassificationGroupCode, journalEntriesNumberCustomisationJournalEntriesDepartmentCode
			, journalEntriesNumberCustomisationSequenceNumber, journalEntriesNumberCustomisationTransactionTypePrefix, journalEntriesNumberCustomisationJournalEntriesClassificationGroupCodePrefix;

		protected override TransactionNumberSequenceCustomisation GetBusinessObjectToClone()
		{
			return new JournalEntriesNumberCustomisation { ElementName = ElementNames.SequenceNumber, Include = true, Order = 1 };
		}

		protected override TransactionNumberSequenceCustomisation GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
