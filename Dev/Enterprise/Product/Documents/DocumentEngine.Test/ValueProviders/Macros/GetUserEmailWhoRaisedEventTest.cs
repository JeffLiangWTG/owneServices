using System.Globalization;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(GetUserEmailWhoRaisedEvent))]
	sealed class GetUserEmailWhoRaisedEventTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("Should not pass", !ValueProviderToTest.IsResponsibleForReplacing("GetUserEmailWhoRaisedEvent", Passes.FirstPass));
			Assert("Should not pass", !ValueProviderToTest.IsResponsibleForReplacing("<GetUserEmailWhoRaisedEvent meh>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<GETUserEmailWHORAISEDEVENT(\"ABC\",\"DEF\",\"6E449683-C509-11CF-AAFA-00AA00B6015C\")>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<GetUserEmailWhoRaisedEvent(\"ABC\",\"DEF\",\"6E449683-C509-11CF-AAFA-00AA00B6015C\")>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("< GetUserEmailWhoRaisedEvent(          \"IFL\",  \"Confirmed\", \"6E449683-C509-11CF-AAFA-00AA00B6015C\" )    >", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("< GetUserEmailWhoRaisedEvent (\"\",\"Hello World\", \"6E449683-C509-11CF-AAFA-00AA00B6015C\")>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			// Setup
			var dummyBO = Factory.New<DummyBusinessObject>();
			SetupStmALogForTesting(Report.BODocDataProvider.ParentBusinessObject.PK, "NonPersistent", AutoEvents.AddedARecordToTheSystemCode, "Confirmed|B5EE43EE-86EF-4A07-AF35-105A27503CB1", userForDataProviderBusinessObject.GS_Code);
			SetupStmALogForTesting(dummyBO.PK, "DummyBizo", AutoEvents.IncidentClosedCode, "Confirmed|B5EE43EE-86EF-4A07-AF35-105A27503CB1", userForDummy.GS_Code);
			Factory.Save();
			// Act
			var replacementWithTwoArguments = ValueProviderToTest.GetReplacement(string.Format(CultureInfo.InvariantCulture, "<GetUserEmailWhoRaisedEvent(\"{0}\",\"{1}\")>", AutoEvents.AddedARecordToTheSystemCode, "Confirmed"), Report);
			var replacementWithThreeArguments = ValueProviderToTest.GetReplacement(string.Format(CultureInfo.InvariantCulture, "<GetUserEmailWhoRaisedEvent(\"{0}\",\"{1}\",\"{2}\")>", AutoEvents.IncidentClosedCode, "Confirmed", dummyBO.PK), Report);
			// Assert
			AssertEquals("The 2 parameter version macro does not work properly.", userForDataProviderBusinessObject.GS_EmailAddress, replacementWithTwoArguments);
			AssertEquals("The 3 parameter version macro does not work properly.", userForDummy.GS_EmailAddress, replacementWithThreeArguments);
		}

		public void TestReplacementWithAsteriskWildcard()
		{
			SetupStmALogForTesting(Report.BODocDataProvider.ParentBusinessObject.PK, "DummyBizo", AutoEvents.DocumentAllocatedCode, "STR|Committed To Go", userForDataProviderBusinessObject.GS_Code);
			SetupStmALogForTesting(Report.BODocDataProvider.ParentBusinessObject.PK, "DummyBizo", AutoEvents.DocumentAllocatedCode, "Prefix|STR", userForDataProviderBusinessObject.GS_Code);
			SetupStmALogForTesting(Report.BODocDataProvider.ParentBusinessObject.PK, "DummyBizo", AutoEvents.DocumentAllocatedCode, "Prefix|STR|Committed To Go", userForDataProviderBusinessObject.GS_Code);
			Factory.Save();

			var replacementWithAsteriskAtEnd = ValueProviderToTest.GetReplacement(string.Format(CultureInfo.InvariantCulture, "<GetUserEmailWhoRaisedEvent(\"{0}\",\"{1}\")>", AutoEvents.DocumentAllocatedCode, "STR|*"), Report);
			var replacementWithAsteriskAtStart = ValueProviderToTest.GetReplacement(string.Format(CultureInfo.InvariantCulture, "<GetUserEmailWhoRaisedEvent(\"{0}\",\"{1}\")>", AutoEvents.DocumentAllocatedCode, "*STR"), Report);
			var replacementWithAsterisksForPrefixAndSuffix = ValueProviderToTest.GetReplacement(string.Format(CultureInfo.InvariantCulture, "<GetUserEmailWhoRaisedEvent(\"{0}\",\"{1}\")>", AutoEvents.DocumentAllocatedCode, "*|STR|*"), Report);

			AssertEquals(userForDataProviderBusinessObject.GS_EmailAddress, replacementWithAsteriskAtEnd);
			AssertEquals(userForDataProviderBusinessObject.GS_EmailAddress, replacementWithAsteriskAtStart);
			AssertEquals(userForDataProviderBusinessObject.GS_EmailAddress, replacementWithAsterisksForPrefixAndSuffix);
		}

		public void TestReplacementWithQuestionMarkWildcard()
		{
			SetupStmALogForTesting(Report.BODocDataProvider.ParentBusinessObject.PK, "DummyBizo", AutoEvents.DocumentImportedCode, "Confirmed DataProvider", userForDataProviderBusinessObject.GS_Code);
			SetupStmALogForTesting(Report.BODocDataProvider.ParentBusinessObject.PK, "DummyBizo", AutoEvents.DocumentImportedCode, "onfirmed DataProvider!", userForDataProviderBusinessObject.GS_Code);
			SetupStmALogForTesting(Report.BODocDataProvider.ParentBusinessObject.PK, "DummyBizo", AutoEvents.DocumentImportedCode, "Confirmed DataProvider!", userForDataProviderBusinessObject.GS_Code);
			Factory.Save();

			var replacementWithQuestionMarkAtEnd = ValueProviderToTest.GetReplacement(string.Format(CultureInfo.InvariantCulture, "<GetUserEmailWhoRaisedEvent(\"{0}\",\"{1}\")>", AutoEvents.DocumentImportedCode, "Confirmed DataProvide?"), Report);
			var replacementWithQuestionMarkAtStart = ValueProviderToTest.GetReplacement(string.Format(CultureInfo.InvariantCulture, "<GetUserEmailWhoRaisedEvent(\"{0}\",\"{1}\")>", AutoEvents.DocumentImportedCode, "?nfirmed DataProvider!"), Report);
			var replacementWithQuestionMarks = ValueProviderToTest.GetReplacement(string.Format(CultureInfo.InvariantCulture, "<GetUserEmailWhoRaisedEvent(\"{0}\",\"{1}\")>", AutoEvents.DocumentImportedCode, "Confi?med DataPr?vider!"), Report);

			AssertEquals(userForDataProviderBusinessObject.GS_EmailAddress, replacementWithQuestionMarkAtEnd);
			AssertEquals(userForDataProviderBusinessObject.GS_EmailAddress, replacementWithQuestionMarkAtStart);
			AssertEquals(userForDataProviderBusinessObject.GS_EmailAddress, replacementWithQuestionMarks);
		}

		public void TestReplacement_WithInvalidBusinessObjectPK()
		{
			// Arrange
			var dummyDocWrapper = new DummyDocWrapper();
			((IReportForUnitTesting)Report).SetBusinessObjectForTesting(dummyDocWrapper);
			// Act
			string result = (string)ValueProviderToTest.GetReplacement("<GetUserEmailWhoRaisedEvent(\"ABC\",\"Hello World\",\"123123\")>", Report);
			// Assert
			AssertEquals(string.Empty, result);
		}

		public void TestCancelledNotUsed()
		{
			var userForCancelledRecord = Factory.NewWithValidTestData<GlbStaff>();
			userForCancelledRecord.GS_Code = "CAN";
			userForCancelledRecord.GS_EmailAddress = "cancelled@test.com";

			SetupStmALogForTesting(Report.BODocDataProvider.ParentBusinessObject.PK, "DummyBizo", AutoEvents.DocumentDeletedCode, "STR|IsCancelled", userForCancelledRecord.GS_Code, false, true);
			SetupStmALogForTesting(Report.BODocDataProvider.ParentBusinessObject.PK, "DummyBizo", AutoEvents.DocumentDeletedCode, "STR|IsValid", userForDataProviderBusinessObject.GS_Code);
			Factory.Save();

			var replacementValidRecord = ValueProviderToTest.GetReplacement(string.Format(CultureInfo.InvariantCulture, "<GetUserEmailWhoRaisedEvent(\"{0}\",\"{1}\")>", AutoEvents.DocumentDeletedCode, "STR|*?"), Report);

			AssertEquals(userForDataProviderBusinessObject.GS_EmailAddress, replacementValidRecord);
		}

		public void TestEstimateNotUsed()
		{
			var userForEstiamte = Factory.NewWithValidTestData<GlbStaff>();
			userForEstiamte.GS_Code = "EST";
			userForEstiamte.GS_EmailAddress = "estimate@test.com";

			SetupStmALogForTesting(Report.BODocDataProvider.ParentBusinessObject.PK, "DummyBizo", AutoEvents.HandedOverCode, "STR|IsEstimate", userForEstiamte.GS_Code, true);
			SetupStmALogForTesting(Report.BODocDataProvider.ParentBusinessObject.PK, "DummyBizo", AutoEvents.HandedOverCode, "STR|IsActual", userForDataProviderBusinessObject.GS_Code);
			Factory.Save();

			var replacementActualRecord = ValueProviderToTest.GetReplacement(string.Format(CultureInfo.InvariantCulture, "<GetUserEmailWhoRaisedEvent(\"{0}\",\"{1}\")>", AutoEvents.HandedOverCode, "STR|*?"), Report);

			AssertEquals(userForDataProviderBusinessObject.GS_EmailAddress, replacementActualRecord);
		}

		public void TestExplanation()
		{
			AssertContains("Returns the email address of the user who created the most recent actual event that is not canceled. It also meets the following criteria.", ValueProviderToTest.Documentation.Explanation);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var dummyDocWrapper = new DummyDocWrapper();
			((IReportForUnitTesting)Report).SetBusinessObjectForTesting(dummyDocWrapper);
			userForDataProviderBusinessObject = Factory.NewWithValidTestData<GlbStaff>();
			userForDataProviderBusinessObject.GS_Code = "TS1";
			userForDataProviderBusinessObject.GS_EmailAddress = "test1@test1.com";
			userForDummy = Factory.NewWithValidTestData<GlbStaff>();
			userForDummy.GS_Code = "TS2";
			userForDummy.GS_EmailAddress = "test2@test2.com";
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new GetUserEmailWhoRaisedEvent();
		}

		#region Implementation

		void SetupStmALogForTesting(ZGuid parentPK, ZString table, ZString eventCode, ZString reference, ZString userCode, bool estimate = false, bool cancelled = false)
		{
			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Parent = parentPK;
				log.SL_Table = table;
				log.SL_SE_NKEvent = eventCode;
				log.SL_Reference = reference;
				log.SL_GS_NKUser = userCode;
				log.SL_IsEstimate = estimate;
				if (cancelled)
				{
					log.Cancel();
				}
			}
		}

		GlbStaff userForDataProviderBusinessObject;
		GlbStaff userForDummy;

		protected override void PrepareDataForExamplesEvaluate()
		{
			userForDataProviderBusinessObject.GS_EmailAddress = "test1@test1.com";
			userForDummy.GS_EmailAddress = "test2@test2.com";
			var dummyBO = Factory.New<DummyBusinessObject>();
			SetupStmALogForTesting(Report.BODocDataProvider.ParentBusinessObject.PK, "NonPersistent", AutoEvents.AddedARecordToTheSystemCode, "Confirmed|B5EE43EE-86EF-4A07-AF35-105A27503CB1", userForDataProviderBusinessObject.GS_Code);
			SetupStmALogForTesting(dummyBO.PK, "DummyBizo", AutoEvents.IncidentClosedCode, "Confirmed|B5EE43EE-86EF-4A07-AF35-105A27503CB1", userForDummy.GS_Code);
			Factory.Save();
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("JS_PK", dummyBO.PK));
		}

		#endregion
	}
}
