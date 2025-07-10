using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.Environment;

namespace Enterprise.PAVE.MENT.Business.Test
{
	class MENTAgedScoreQueryValidationTest : BusinessObjectValidationTestCase
	{
		public void TestQueryTextSecurity()
		{
			Env.Security.MENTAgedScoreQueryChangeQueryText.IsAllowed = true;

			var query = MENTTestHelper.CreateQuery(Factory, "WSHINGBRD");
			const string sqlText1 = "SELECT GS_Code AS score, GS_PK AS releaseGroup, GS_PK AS component, GS_Code AS attributeValue, GS_Code AS staff FROM dbo.GlbStaff";
			const string sqlText2 = sqlText1 + " --modified";

			query.MAQ_SqlText = sqlText1;
			AssertNoErrors(query.MAQ_SqlTextInfo);

			Env.Security.MENTAgedScoreQueryChangeQueryText.IsAllowed = false;

			AssertNoErrors(query.MAQ_SqlTextInfo);

			Factory.Save();

			query.MAQ_SqlText = sqlText2;
			AssertHasError(query.MAQ_SqlTextInfo, Env.Security.MENTAgedScoreQueryChangeQueryText.ErrorMessageForNotAllowed);

			Env.Security.MENTAgedScoreQueryChangeQueryText.IsAllowed = true;

			query.MAQ_SqlText = sqlText1;
			AssertNoErrors(query.MAQ_SqlTextInfo);
			query.MAQ_SqlText = sqlText2;

			Factory.Save();

			Env.Security.MENTAgedScoreQueryChangeQueryText.IsAllowed = false;
			query.MAQ_SqlText = sqlText1;
			AssertHasError(query.MAQ_SqlTextInfo, Env.Security.MENTAgedScoreQueryChangeQueryText.ErrorMessageForNotAllowed);
		}

		public void TestWarningIfFaulty()
		{
			var query = MENTTestHelper.CreateQuery(Factory, "XOXOGOSSIP");

			query.MAQ_IsFaulty = true;
			AssertHasWarning(query.MAQ_IsFaultyInfo, "Check the notes tab to view the reason why this has been marked as faulty.");

			query.MAQ_IsFaulty = false;
			AssertNoWarning(query.MAQ_IsFaultyInfo, "Check the notes tab to view the reason why this has been marked as faulty.");
		}

		public void TestCodeHasToBeAlphaNumeric()
		{
			var query = Factory.NewWithValidTestData<MENTAgedScoreQuery>();
			query.MAQ_Code = ";./?";
			query.Validation.ValidateMAQ_Code();

			AssertHasError(query.MAQ_CodeInfo, "Codes can only contain Alpha Numeric Characters");

			query.MAQ_Code = "CHILLIN";
			query.Validation.ValidateMAQ_Code();

			AssertNoError(query.MAQ_CodeInfo, "Codes can only contain Alpha Numeric Characters");
		}

		public void TestCodeIsUnique()
		{
			var query = Factory.NewWithValidTestData<MENTAgedScoreQuery>();
			query.MAQ_Code = "WHOISWORKN";
			Factory.Save();

			var query2 = Factory.NewWithValidTestData<MENTAgedScoreQuery>();

			query2.MAQ_Code = "WHOISWORKN";
			query2.Validation.ValidateMAQ_Code();
			AssertHasError(query2.MAQ_CodeInfo, "The Code you have entered is not unique.");

			query2.MAQ_Code = "NOONE";
			query2.Validation.ValidateMAQ_Code();
			AssertNoError(query2.MAQ_CodeInfo, "The Code you have entered is not unique.");
		}

		public void TestSqlTextHasRequiredColumns()
		{
			var query = Factory.NewWithValidTestData<MENTAgedScoreQuery>();
			query.MAQ_IsActive = true;

			var incorrectText = @"THIS IS INCORRECT TEXT";
			var oneColumnText = @"SELECT GS_Code from dbo.GlbStaff";
			var incorrectNamedColumnText = @"SELECT GS_Code as incorrect, GS_PK as releasegroup, GS_PK as component, GS_Code as attributevalue, GS_Code as staff from dbo.GlbStaff";
			var correctSqlText = @"SELECT GS_Code as score, GS_PK as releaseGroup, GS_PK as component, GS_Code as attributeValue, GS_Code as staff from dbo.GlbStaff";

			query.MAQ_SqlText = incorrectText;
			query.Validation.ValidateMAQ_SqlText();
			AssertHasError(query.MAQ_SqlTextInfo, "Invalid SQL Statement\r\nIncorrect syntax near 'THIS'.");

			query.MAQ_SqlText = oneColumnText;
			query.Validation.ValidateMAQ_SqlText();
			AssertHasError(query.MAQ_SqlTextInfo, "The SQL statement must return 5 columns: score, releaseGroup, component, attributeValue, staff");

			query.MAQ_SqlText = incorrectNamedColumnText;
			query.Validation.ValidateMAQ_SqlText();
			AssertHasError(query.MAQ_SqlTextInfo, "The column at position 1 must have the name score.");

			query.MAQ_SqlText = correctSqlText;
			query.Validation.ValidateMAQ_SqlText();
			AssertNoErrors(query.MAQ_SqlTextInfo);
		}

		public void TestOnlyValidateSqlWithActiveQuery()
		{
			var query = Factory.NewWithValidTestData<MENTAgedScoreQuery>();
			query.MAQ_SqlText = @"THIS IS INCORRECT TEXT";
			query.MAQ_IsActive = false;

			query.Validation.ValidateMAQ_SqlText();
			AssertNoErrors(query.MAQ_SqlTextInfo);

			query.MAQ_IsActive = true;

			query.Validation.ValidateMAQ_SqlText();
			AssertHasError(query.MAQ_SqlTextInfo, "Invalid SQL Statement\r\nIncorrect syntax near 'THIS'.");
		}

		public void TestOnlyValidateSqlOnNonLinkedQuery()
		{
			var band = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();

			var query = Factory.NewWithValidTestData<MENTAgedScoreQuery>();
			query.MAQ_BAB_RelatedAcceptabilityBand = band.PK;
			query.MAQ_SqlText = @"THIS IS INCORRECT TEXT";
			query.MAQ_IsActive = true;

			query.Validation.ValidateMAQ_SqlText();
			AssertNoErrors(query.MAQ_SqlTextInfo);
			AssertHasWarning(query.MAQ_SqlTextInfo, "This query is linked and will use the linked query's text.");
		}

		public void TestSqlIsMandatoryForActiveQuery()
		{
			var query = Factory.New<MENTAgedScoreQuery>();

			query.Validation.ValidateAll();

			AssertMandatoryValidationError(query.MAQ_SqlTextInfo, true);

			query.MAQ_IsActive = false;
			query.Validation.ValidateAll();

			AssertMandatoryValidationError(query.MAQ_SqlTextInfo, false);
		}

		public void TestErrorIfPurgeValuesAreNegative()
		{
			var query = MENTTestHelper.CreateQuery(Factory, "OHNO");

			query.MAQ_PurgeDays = -1;
			AssertHasError(query.MAQ_PurgeDaysInfo, "Please enter a 'Purge Days' greater than or equal to 0.");

			query.MAQ_PurgeDays = 1;
			AssertNoError(query.MAQ_PurgeDaysInfo, "Please enter a 'Purge Days' greater than or equal to 0.");

			query.MAQ_PurgeAllButLatestQuantity = -1;
			AssertHasError(query.MAQ_PurgeAllButLatestQuantityInfo, "Please enter a 'Purge All But Latest Quantity' greater than or equal to 0.");

			query.MAQ_PurgeAllButLatestQuantity = 0;
			AssertNoError(query.MAQ_PurgeAllButLatestQuantityInfo, "Please enter a 'Purge All But Latest Quantity' greater than or equal to 0.");
		}
	}
}
