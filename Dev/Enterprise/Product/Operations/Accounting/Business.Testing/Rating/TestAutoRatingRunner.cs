using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common.Collections;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Integration;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class TestAutoRatingRunner : AutoRatingRunner
	{
		public TestAutoRatingRunner(IBusiness parentConsumer, GetNewAutoRaterDelegate newAutoRater, IRatingContext ratingContext) : base(parentConsumer, ratingContext)
		{
			this.newAutoRater = newAutoRater;
		}

		protected override AutoRater GetNewAutoRater()
		{
			return newAutoRater != null ? newAutoRater() : base.GetNewAutoRater();
		}

		public delegate AutoRater GetNewAutoRaterDelegate();
		readonly GetNewAutoRaterDelegate newAutoRater;
		public void CreateFallbackDescriptionNote(AutoRateInfoCollection jobRates, CostSell costOrSell, IAutoRating ratingAdapter)
		{
			base.LogChargesCalculated(jobRates, costOrSell);
		}

		public int AutoratingLogUpdateCount { get; private set; }
		protected override void AppendLogToNote(StmNote note, CargoWise.Types.ZStringBuilder noteBuilder)
		{
			base.AppendLogToNote(note, noteBuilder);
			AutoratingLogUpdateCount++;
		}
	}

	public class AutoRatingRunnerTestCase : TestCaseWithFactory
	{
		public void TestAutoratingLogNotUpdatedOnceFull()
		{
			var maxLength = PredefinedNoteTypes.Instance.AutoRatingAuditLog.TextOnlyMaxLength;
			var longMessage = new string('A', maxLength + 1);
			var ratingContext = new RatingContext();
			ratingContext.Logger.Information(longMessage);

			var consumer = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ICommonShipment>();
			var dummy = new AutoRatingStarterCoreTest.DummyAutoRatingObject(Factory);
			var autoRatingRunner = new TestAutoRatingRunner(dummy, GetNewAutoRater, ratingContext);
			var noteParent = (IStmNoteParent)consumer;

			AssertEquals("Precondition", 0, autoRatingRunner.AutoratingLogUpdateCount);
			autoRatingRunner.WriteAutoratingLogIntoNote(noteParent);
			AssertEquals("Updated once", 1, autoRatingRunner.AutoratingLogUpdateCount);

			ratingContext.Logger.Information("A bit more text");
			autoRatingRunner.WriteAutoratingLogIntoNote(noteParent);
			AssertEquals("Did not update again", 1, autoRatingRunner.AutoratingLogUpdateCount);
		}

		public void TestFactorySaving_InDB_DbHits()
		{
			var consumer = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ICommonShipment>();

			var interactor = new TestInteractor();
			using (_Rating.Start(interactor, isEqualization: false))
			using (interactor.StartRatingSession())
			using (_Rating.StartSubSession(consumer))
			{
				for (int index = 0; index < 10; index++)
				{
					var dummy = new AutoRatingStarterCoreTest.DummyAutoRatingObject(Factory);
					dummy.IsInDatabaseOverride = true;
					new TestAutoRatingRunner(dummy, GetNewAutoRater, new RatingContext());
				}

				var expectedHits = new Dictionary<string, int>
				{
					{ CusEntryNumSchema.Constants.TableName, 1 },
					{ RefServiceLevelSchema.Constants.TableName, 1 },
					{ StmNoteSchema.Constants.TableName, 1 },
					{ RefCurrencySchema.Constants.TableName, 1 },
				};

				using (AssertDbHitsWithUsefulQueryInformation(expectedHits, Factory))
				{
					Factory.Save();
					AssertEquals("Should be no fetch hints left for StmNote.", 0, Factory.ActiveFetchHintsForTable(StmNoteSchema.Constants.TableName));

					Factory.ClearQueryCache(StmNoteSchema.Constants.TableName);
					Factory.Save(); // make sure event is unhooked on first save, should be no hits to StmNote
				}
			}
		}

		public void TestFactorySaving_NotInDB_DbHits()
		{
			var consumer = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ICommonShipment>();

			var interactor = new TestInteractor();
			using (_Rating.Start(interactor, isEqualization: false))
			using (interactor.StartRatingSession())
			using (_Rating.StartSubSession(consumer))
			{
				for (int index = 0; index < 10; index++)
				{
					var dummy = new AutoRatingStarterCoreTest.DummyAutoRatingObject(Factory);
					dummy.IsInDatabaseOverride = false;
					new TestAutoRatingRunner(dummy, GetNewAutoRater, new RatingContext());
				}

				var expectedHits = new Dictionary<string, int>
				{
					{ CusEntryNumSchema.Constants.TableName, 1 },
					{ RefServiceLevelSchema.Constants.TableName, 1 },
					{ RefCurrencySchema.Constants.TableName, 1 },
				};

				using (AssertDbHitsWithUsefulQueryInformation(expectedHits, Factory))
				{
					Factory.Save();
				}
			}
		}

		public void TestFactorySaving_NonStmNoteParent_DbHits()
		{
			var consumer = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ICommonShipment>();

			var interactor = new TestInteractor();
			using (_Rating.Start(interactor, isEqualization: false))
			using (interactor.StartRatingSession())
			using (_Rating.StartSubSession(consumer))
			{
				for (int index = 0; index < 10; index++)
				{
					var dummy = new DummyAutoRaterObject(Factory);
					new TestAutoRatingRunner(dummy, GetNewAutoRater, new RatingContext());
				}

				var expectedHits = new Dictionary<string, int>
				{
					{ CusEntryNumSchema.Constants.TableName, 1 },
					{ RefServiceLevelSchema.Constants.TableName, 1 },
					{ RefCurrencySchema.Constants.TableName, 1 },
				};

				using (AssertDbHitsWithUsefulQueryInformation(expectedHits, Factory))
				{
					Factory.Save();
				}
			}
		}

		class DummyAutoRaterObject : NonPersistentBusinessObject
		{
			public DummyAutoRaterObject(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public override bool IsInDatabase => true;
		}

		public void TestMissingSpecialServiceNotification()
		{
			BusinessObject consumer = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ICommonShipment>();
			AutoRatingRunner runner = new TestAutoRatingRunner(consumer, GetNewAutoRater, new RatingContext());
			IAutoRating[] jobs = new IAutoRating[] { new AutoRatingProxy(null), new AutoRatingProxy(null) };
			((AutoRatingProxy)jobs[0]).ValuesCanBeSet = true;
			((AutoRatingProxy)jobs[1]).ValuesCanBeSet = true;
			var services = new JobServicesCollection();
			services.Add(new JobServiceInfo(true, "ORG", "FUM", "Fumigation"));
			((AutoRatingProxy)jobs[0]).JobServices = services;
			services = new JobServicesCollection();
			services.Add(new JobServiceInfo(true, "ORG", "QIN", "Quarantine Inspection"));
			((AutoRatingProxy)jobs[1]).JobServices = services;
			string notificationMessage = string.Empty;
			runner.OnAfterRatingNotification += message => notificationMessage = message;
			AutoRateInfoCollection results = runner.RetrieveAllCharges(jobs, new Mock<IRatingAdaptersProvider>().Object, CostSell.Revenue);
			AssertEquals(@"Rates for the below job services were not found. Please either create these charge codes, or ensure that your rate contains these charges and have the correct commodity code, service level and validity dates.
Until you do this, these charges will not be rated.

  • Fumigation: ORG / FUM for Shipment
  • Quarantine Inspection: ORG / QIN for Shipment", notificationMessage);
		}

		public void TestTestAutoRatingRunner()
		{
			var consumer = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ICommonShipment>();
			AutoRatingRunner runner = new TestAutoRatingRunner(consumer, GetNewAutoRater, new RatingContext());
			var jobs = new IAutoRating[] { new AutoRatingProxy(null), new AutoRatingProxy(null) };
			var results = runner.RetrieveAllCharges(jobs, new Mock<IRatingAdaptersProvider>().Object, CostSell.Revenue);
			AssertEquals(3, results.Count);
		}

		public void TestTrimToFit()
		{
			var noteText = "Rates for the below job services were not found. Please either create these";
			var actualNoteText = noteText.TrimToFit(75);
			var expectedNoteText = "Rates for the below job services were not found. Please either create these";
			AssertEquals(expectedNoteText, actualNoteText);
			noteText = @"Rates for the below job services were not found. Please either create these.";
			actualNoteText = noteText.TrimToFit(75);
			expectedNoteText = "Rates for the below job services were not found. Please ei...trimmed to fit";
			AssertEquals(expectedNoteText, actualNoteText);
		}

		public void TestCreateFallbackNoteUsesLogger()
		{
			IAutoRating[] jobs = { new AutoRatingProxy(null), new AutoRatingProxy(null) };
			var consumer = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ICommonShipment>();
			var ratingContext = new RatingContext();
			var runner = new TestAutoRatingRunner(consumer, GetNewAutoRater, ratingContext);
			var charges = runner.RetrieveAllCharges(jobs, new Mock<IRatingAdaptersProvider>().Object, CostSell.Revenue);
			ratingContext.Logger.Information("Test info");
			ratingContext.Logger.Warning("Test warning");
			ratingContext.Logger.Error("Test error");
			runner.CreateFallbackDescriptionNote(charges, CostSell.Revenue, new AutoRatingProxy(null));
			AssertMultilineASCIIEquals("Log should contain fallback information", @"Information: AUTORATING REVENUE FOR Enterprise.Accounting.Integration.AutoRatingProxy
Information: CHARGES CALCULATED:
Information: AUTORATING REVENUE FOR Enterprise.Accounting.Integration.AutoRatingProxy
Information: CHARGES CALCULATED:
Information: Test info
Warning: Test warning
Error: Test error
Information: CHARGES CALCULATED:", ratingContext.Logger.DumpLog().ToStringWithNewLineBetweenStrings());
		}

		[TestDate(2016, 02, 10, 10, 0, 0)]
		public void TestCreateFallbackNoteHasChangesWhenUpdated()
		{
			DataRegistryRating.Instance.AllowSavingOfAutoRatingLogNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var consumer = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ICommonShipment>();
			var ratingContext = new RatingContext();
			var runner = new TestAutoRatingRunner(consumer, GetNewAutoRater, ratingContext);
			ratingContext.Logger.Warning("Light the Beacons");
			Factory.Save();
			var noteParent = (IStmNoteParent)consumer;
			runner.WriteAutoratingLogIntoNote(noteParent);
			var expectedNote = string.Join("\r\n", "User:\t\t\t\tCargoWise Support", "Time:\t\t\t\t10-Feb-16 10:00\r\n", "Warning: Light the Beacons");
			var autoRatingLogNotes = noteParent.Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description);
			Assert("Even if no charges were found we still expect the parent to have a log explaining this", consumer.HasChanges);
			AssertMultilineASCIIEquals("Log should contain fallback information", expectedNote, autoRatingLogNotes.First().ST_NoteText);
			ratingContext.Logger.Warning("Gondor calls for aid");
			Factory.Save();
			runner.WriteAutoratingLogIntoNote(noteParent);
			Assert("Updating the existing note should mean the note has changes", autoRatingLogNotes.First().HasChanges);
			AssertMultilineASCIIEquals("New warning should be added to the first note", expectedNote + System.Environment.NewLine + "Warning: Gondor calls for aid", autoRatingLogNotes.First().ST_NoteText);
			AssertEquals("There should be no more than one autorating log for current company", 1, autoRatingLogNotes.Count(x => x.IsBelongingToCurrentLoginCompany));
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var note = noteParent.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description;
			note.ST_GC_RelatedCompany = company.PK;
			runner.WriteAutoratingLogIntoNote(noteParent);
			autoRatingLogNotes = noteParent.Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description);
			AssertEquals("Should have warning as the note has not yet been made readonly by saving", true, autoRatingLogNotes.Any(x => x.ST_DescriptionInfo.HasWarnings()));
			Factory.Save();
			foreach (var autoRatingLogNote in autoRatingLogNotes)
			{
				autoRatingLogNote.Validation.ValidateAll();
			}

			AssertEquals("There will be autorating notes for two companies", 2, autoRatingLogNotes.Length);
			AssertEquals("There should be exactly one one note for current company", 1, autoRatingLogNotes.Count(x => x.ST_GC_RelatedCompany == company.PK));
			AssertEquals("No notes should have any warnings against manually adding this note", false, autoRatingLogNotes.Any(x => x.ST_DescriptionInfo.HasWarnings()));
		}

		public void TestWriteAutoratingLogIntoNote_RepeatedCallsDoNotAddDbHits()
		{
			var consumer = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ICommonShipment>();
			Factory.Save();

			var noteParent = (IStmNoteParent)consumer;
			var ratingContext = new RatingContext();

			Factory.ResetDatabaseLoadCount();

			var expected = new Dictionary<string, int> { { StmNote.Schema.TableName, 1 } };
			using (AssertDbHitsForAllFactories(expected, ignoreUnspecified: true))
			{
				// Simulate a number of additional jobs
				for (int i = 0; i < 10; ++i)
				{
					var runner = new TestAutoRatingRunner(consumer, GetNewAutoRater, ratingContext);
					ratingContext.Logger.Warning("Log " + i);
					runner.WriteAutoratingLogIntoNote(noteParent);
				}
			}
		}

		public void TestMergeCharges()
		{
			var consumer = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ICommonShipment>();
			AutoRatingRunner runner = new TestAutoRatingRunner(consumer, GetNewAutoRater, new RatingContext());
			IAutoRating[] jobs = { new AutoRatingProxy(null), new AutoRatingProxy(null), new AutoRatingProxy(null) };
			((AutoRatingProxy)jobs[1]).ValuesCanBeSet = true;
			((AutoRatingProxy)jobs[2]).ValuesCanBeSet = true;
			((AutoRatingProxy)jobs[1]).MergeCharges = MergeChargeOptions.CrossAdapter;
			((AutoRatingProxy)jobs[2]).MergeCharges = MergeChargeOptions.CrossAdapter;
			var results = runner.RetrieveAllCharges(jobs, new Mock<IRatingAdaptersProvider>().Object, CostSell.Revenue).Cast<AutoRateInfo>().ToList().StableSort(x => x.Amount);
			AssertEquals(4, results.Count);
			AssertEquals(100m, results[0].Amount);
			AssertEquals(102m, results[1].Amount);
			AssertEquals(104m, results[2].Amount);
			AssertEquals(204m, results[3].Amount);
		}

		public void TestMergeOverrideDebtor()
		{
			var dictionary = new Dictionary<IAutoRating, AutoRateResult>();
			var job = new AutoRatingProxy(null);
			job.ValuesCanBeSet = true;
			job.MergeCharges = MergeChargeOptions.CrossAdapter;

			var infos = new AutoRateInfoCollection(Factory);
			var orgPK = new Guid("12300000-0000-0000-0000-000000000000");
			var frt = Factory.New<AccChargeCode>();
			infos.AddNew(frt, "USD", 100m);
			infos[0].DebtorOverridePK = orgPK;
			var result = new AutoRateResult(Factory, CostSell.Revenue);
			result.RateInfoCollection.AddRange(infos);

			dictionary.Add(job, result);
			var mergedCharges = new RateResultsSummarizer(Factory).MergeInfosCrossAdapter(dictionary);

			AssertEquals(1, mergedCharges.Count);
			AssertEquals(orgPK, mergedCharges[0].DebtorOverridePK);
		}

		#region Remove FallbackNote

		public void TestExecuteAutoratingCreatesAndRemovesFallbackNote_AllowSavingOfAutoRatingLogNote() => TestExecuteAutoratingCreatesAndRemovesFallbackNote(true, true);

		public void TestExecuteAutoratingCreatesAndRemovesFallbackNote_NotAllowSavingOfAutoRatingLogNote() => TestExecuteAutoratingCreatesAndRemovesFallbackNote(false, false);

		void TestExecuteAutoratingCreatesAndRemovesFallbackNote(bool allowSavingOfAutoRatingLogNote, bool expectedNoteAfterSave)
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			DataRegistryRating.Instance.AllowSavingOfAutoRatingLogNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, allowSavingOfAutoRatingLogNote);

			testObjectCreator.CreateFlatCalculatorCosting("AIR", "LSE", "AUSYD", "NZAKL", null, "FRT", 200m);

			var consol = testObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001");
			var shipment = testObjectCreator.CreateShipment("S00001", "AUSYD", "NZAKL", consol);
			shipment.JS_INCO = "CIF";
			var job = testObjectCreator.CreateJob(shipment, false);

			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			job.JH_OA_LocalChargesAddr = testObjectCreator.LocalClient.MainAddress.PK;

			Factory.Save();

			var testInteractor = new TestInteractor();
			var autoRatingStarter = new AutoRatingStarter(consol, testInteractor);
			autoRatingStarter.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);

			AssertEquals("AutoRate should create AutoRatingLog", true, consol.Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description).Any());
			Factory.Save();
			AssertEquals("ExpectedNoteAfterSave", expectedNoteAfterSave, consol.Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description).Any());

			var charges = ((Job)shipment.Job).Charges;
			AssertEquals("Number of job charges", 1, charges.Count);
			AssertEquals("Charge code on charge line", testObjectCreator.FRT.AC_Code, charges[0].ChargeCode.AC_Code);
			AssertEquals("Cost amount on charge line", 200m, charges[0].JR_OSCostAmt);
		}

		#endregion

		#region Implementation

		AutoRater GetNewAutoRater()
		{
			var frt = Factory.New<AccChargeCode>();
			var baf = Factory.New<AccChargeCode>();
			var infos1 = new AutoRateInfoCollection(Factory);
			infos1.AddNew(frt, "USD", 100m);
			var infos2 = new AutoRateInfoCollection(Factory);
			infos2.AddNew(frt, "USD", 101m);
			infos2.AddNew(baf, "USD", 102m);
			var infos3 = new AutoRateInfoCollection(Factory);
			infos3.AddNew(frt, "USD", 103m);
			infos3.AddNew(baf, "AUD", 104m);
			var autoRater = new TestAutoRater();
			var result1 = new AutoRateResult(Factory, CostSell.Revenue);
			var result2 = new AutoRateResult(Factory, CostSell.Revenue);
			var result3 = new AutoRateResult(Factory, CostSell.Revenue);
			result1.RateInfoCollection.AddRange(infos1);
			result2.RateInfoCollection.AddRange(infos2);
			result3.RateInfoCollection.AddRange(infos3);
			autoRater.Results = new[] { result1, result2, result3 };
			return autoRater;
		}

		class TestAutoRater : AutoRater
		{
			public override AutoRateResult AutoRate(BusinessObjectFactory factory, IAutoRating itemToRate, CostSell costOrSell, IRatingContext ratingContext)
			{
				if (Results != null && callNum < Results.Length)
				{
					return Results[callNum++];
				}

				return new AutoRateResult(factory, costOrSell);
			}

			public AutoRateResult[] Results;
			int callNum;
		}

		#endregion
	}
}
