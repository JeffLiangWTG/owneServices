using System;
using CargoWise.BuildTools.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Builder.DataUpgradeSetup;
using Enterprise.DbUpgrader.Data;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Business
{
	[TestedType(typeof(GlbReleaseNoteManagerForSourceSafe))]
	class GlbReleaseNoteManagerForSourceSafeTest : GlbReleaseNoteManagerTest
	{
		[StressTest]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		[StressTest]
		public void TestShouldFilterByModule()
		{
			TestCaseHelper.ClearTable(GlbReleaseNoteReadSchema.Constants.TableName);
			TestCaseHelper.ClearTable(GlbReleaseNoteSchema.Constants.TableName);
			GlbReleaseNote note1 = Factory.NewWithValidTestData<GlbReleaseNote>();
			note1.GF_Category = "SAL";
			note1.GF_ReleaseNoteDate = ZDateTime.Today;
			GlbReleaseNote note2 = Factory.NewWithValidTestData<GlbReleaseNote>();
			note2.GF_Category = "FOR";
			note2.GF_ReleaseNoteDate = ZDateTime.Today;
			Factory.Save();
			Env.Registry.ModuleToShowNotesFor = "SAL"; // fake the remembering of the last shown module
			GlbReleaseNoteManagerForSourceSafe note = new GlbReleaseNoteManagerForSourceSafe(Factory);
			AssertEquals(false, note.InternalShouldFilterByModule);
			AssertEquals("ReleaseNotes.Contains(Note1)", true, Manager.ReleaseNotes.Contains(note1));
			AssertEquals("ReleaseNotes.Contains(Note2)", true, Manager.ReleaseNotes.Contains(note2));
		}

		public void TestAdditionalReleaseNoteSectionsToShow()
		{
			TestCaseHelper.ClearTable(GlbReleaseNoteReadSchema.Constants.TableName);
			TestCaseHelper.ClearTable(GlbReleaseNoteSchema.Constants.TableName);
			var note1 = Factory.NewWithValidTestData<GlbReleaseNote>();
			note1.GF_Section = NewsSectionTypeList.Codes.WiseNews;
			note1.GF_Category = "SAL";
			note1.GF_ReleaseNoteDate = ZDateTime.Today;
			var note2 = Factory.NewWithValidTestData<GlbReleaseNote>();
			note2.GF_Section = NewsSectionTypeList.Codes.WiseLearningUpdates;
			note2.GF_Category = "FOR";
			note2.GF_ReleaseNoteDate = ZDateTime.Today;
			var note3 = Factory.NewWithValidTestData<GlbReleaseNote>();
			note3.GF_Section = NewsSectionTypeList.Codes.TechnicalAdvisoryNotes;
			note3.GF_Category = "FOR";
			note3.GF_ReleaseNoteDate = ZDateTime.Today;
			var note4 = Factory.NewWithValidTestData<GlbReleaseNote>();
			note4.GF_Section = NewsSectionTypeList.Codes.BorderWise;
			note4.GF_Category = "";
			note4.GF_ReleaseNoteDate = ZDateTime.Today;
			var note5 = Factory.NewWithValidTestData<GlbReleaseNote>();
			note5.GF_Section = NewsSectionTypeList.Codes.WiseTechAcademy;
			note5.GF_Category = "";
			note5.GF_ReleaseNoteDate = ZDateTime.Today;
			Factory.Save();
			var note = new GlbReleaseNoteManagerForSourceSafe(Factory);
			note.InternalSetDefaultValues();
			CombineAssertions(() =>
			{
				AssertEquals("ReleaseNotes.Contains(Note1)", true, Manager.ReleaseNotes.Contains(note1));
				AssertEquals("ReleaseNotes.Contains(Note2)", true, Manager.ReleaseNotes.Contains(note2));
				AssertEquals("ReleaseNotes.Contains(Note3)", true, Manager.ReleaseNotes.Contains(note3));
				AssertEquals("ReleaseNotes.Contains(Note4)", true, Manager.ReleaseNotes.Contains(note4));
				AssertEquals("ReleaseNotes.Contains(Note5)", true, Manager.ReleaseNotes.Contains(note5));
			});
		}

		[StressTest]
		public override void TestDefaultValues()
		{
			GlbReleaseNoteManagerForSourceSafe note = new GlbReleaseNoteManagerForSourceSafe(Factory);
			Assert(note.ShowAllCountryNotes);
			Assert(note.ShowReadNotes);
		}

		[StressTest]
		public override void TestReleaseNotes()
		{
			Env.Registry.DoNotDisplayUpdatesOnLogin = false;
			Env.Registry.DoNotDisplayUpdatesOnModuleEntry = false;
			Env.Registry.ModuleToShowNotesFor = "";
			ZQuery filter = new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, SQLComparisonOperator.Equal, Core.Constants.CountryCodes.NewZealand);
			AssertEquals("Precondition: There should not be a GlbCompany in New Zealand.", 0, Factory.GetDatabaseCount(typeof(GlbCompany), filter));
			Env.Registry.EnterpriseCDDate = new DateTime(2005, 2, 1);
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			GlbReleaseNote note1 = Factory.New<GlbReleaseNote>();
			GlbReleaseNote note2 = Factory.New<GlbReleaseNote>();
			GlbReleaseNote note3 = Factory.New<GlbReleaseNote>();
			GlbReleaseNote note4 = Factory.New<GlbReleaseNote>();
			note1.GF_RN_NKCountryForReleaseNote = Core.Constants.CountryCodes.Australia;
			note2.GF_RN_NKCountryForReleaseNote = Core.Constants.CountryCodes.NewZealand;
			note3.GF_RN_NKCountryForReleaseNote = "";
			note4.GF_RN_NKCountryForReleaseNote = "";
			note1.GF_ReleaseNoteDate = ZDateTime.Now;
			note2.GF_ReleaseNoteDate = ZDateTime.Now;
			note3.GF_ReleaseNoteDate = ZDateTime.Now;
			AssertEquals("ReleaseNotes.ReadOnly", false, Manager.ReleaseNotes.ReadOnly);
			AssertEquals("ReleaseNotes.Contains(Note1)", true, Manager.ReleaseNotes.Contains(note1));
			AssertEquals("ReleaseNotes.Contains(Note2)", true, Manager.ReleaseNotes.Contains(note2));
			AssertEquals("ReleaseNotes.Contains(Note3)", true, Manager.ReleaseNotes.Contains(note3));
			note4.GF_ReleaseNoteDate = new ZDateTime(2005, 1, 1);
			Manager.DateToFilterAfter = ZDateTime.Empty;
			AssertEquals("Notes older than the Enterprise CD Date should be shown.", true, Manager.ReleaseNotes.Contains(note4));
		}

		[StressTest]
		public void TestCheckoutSuccessfully()
		{
			MockSourceControl.Setup();
			try
			{
				DummyGlbReleaseNoteManagerForSourceSafe manager = new DummyGlbReleaseNoteManagerForSourceSafe(Factory);
				string errorMessage;
				bool checkoutResult = manager.Checkout(out errorMessage);
				AssertEquals("ErrorMessage", "", errorMessage);
				AssertEquals("Manager.Checkout()", true, checkoutResult);
				AssertEquals("ReleaseNotes.ReadOnly", false, manager.ReleaseNotes.ReadOnly);
			}
			finally
			{
				MockSourceControl.TearDown();
			}
		}

		[StressTest]
		public void TestCheckoutUnsuccessful()
		{
			MockSourceControl.Setup();
			try
			{
				DummyGlbReleaseNoteManagerForSourceSafe manager = new DummyGlbReleaseNoteManagerForSourceSafe(Factory);
				manager.ExceptionToThrowOnCheckout = new Exception("Cannot Checkout!");
				string errorMessage;
				bool checkoutResult = manager.Checkout(out errorMessage);
				AssertEquals("ErrorMessage", "Cannot Checkout!", errorMessage);
				AssertEquals("Manager.Checkout()", false, checkoutResult);
				AssertEquals("ReleaseNotes.ReadOnly", false, manager.ReleaseNotes.ReadOnly);
			}
			finally
			{
				MockSourceControl.TearDown();
			}
		}

		#region Implementation
		protected override GlbReleaseNoteManager GetNewManager()
		{
			return new GlbReleaseNoteManagerForSourceSafe(Factory);
		}

		protected new GlbReleaseNoteManagerForSourceSafe Manager
		{
			get
			{
				return (GlbReleaseNoteManagerForSourceSafe)base.Manager;
			}
		}

		#region class DummyGlbReleaseNoteManagerForSourceSafe
		class DummyGlbReleaseNoteManagerForSourceSafe : GlbReleaseNoteManagerForSourceSafe
		{
			public DummyGlbReleaseNoteManagerForSourceSafe(BusinessObjectFactory factory) : base(factory)
			{
			}

			public Exception ExceptionToThrowOnCheckout
			{
				get
				{
					return Controller.ExceptionToThrowOnFullCheckOut;
				}

				set
				{
					Controller.ExceptionToThrowOnFullCheckOut = value;
				}
			}

			protected override DataUpgradeSetupController GetNewDataUpgradeSetupController(UpgradeTask[] tasks)
			{
				return new DummyDataUpgradeSetupController(tasks);
			}

			new DummyDataUpgradeSetupController Controller
			{
				get
				{
					return (DummyDataUpgradeSetupController)base.Controller;
				}
			}

			#region class DummyDataUpgradeSetupController
			class DummyDataUpgradeSetupController : DataUpgradeSetupController
			{
				public DummyDataUpgradeSetupController(UpgradeTask[] tasks) : base(tasks)
				{
				}

				public override void FullCheckOut()
				{
					if (ExceptionToThrowOnFullCheckOut != null)
					{
						throw ExceptionToThrowOnFullCheckOut;
					}
				}

				public Exception ExceptionToThrowOnFullCheckOut;
			}
			#endregion
		}
		#endregion
		#endregion
	}
}
