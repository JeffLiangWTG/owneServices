using System;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal abstract class PseudoApplicatorTest<ApplicatorT> : OperationalActionMethodApplicatorTest where ApplicatorT : PseudoApplicator
	{
		public void TestAddToCollection()
		{
			OperationalActionRunner runner;
			ConfigureForInclusion(Action);
			var selectedRecords = new SelectedRecords()
			{ PrimaryKeys = new[] { Dummy1.PK, Dummy3.PK } };
			runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), selectedRecords);
			AssertEquals(string.Format("should have a {0}", typeof(ApplicatorT).Name), 1, CountApplicatorsOfTestedType(runner));
			ConfigureForExclusion(Action);
			runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), selectedRecords);
			AssertEquals(string.Format("should not have a {0}", typeof(ApplicatorT).Name), 0, CountApplicatorsOfTestedType(runner));
		}

		#region Implementation
		protected static void RunRunner(OperationalActionRunner runner, string expectedLog)
		{
			RunRunner(runner, expectedLog, true);
		}

		protected static void RunRunner(OperationalActionRunner runner, string expectedLog, bool verifyLog)
		{
			DummyOperationalActionLog dummyLog = new DummyOperationalActionLog();
			dummyLog.IncludeDebug = true;
			BusinessObjectFactory factoryForChanges = new BusinessObjectFactory();
			runner.Run(dummyLog, factoryForChanges);
			if (verifyLog)
			{
				dummyLog.Verify();
			}

			factoryForChanges.Save();
			AssertMultilineASCIIEquals("", expectedLog, string.Join("\n", dummyLog.messages.ToArray()));
		}

		protected static int CountApplicatorsOfTestedType(OperationalActionRunner runner)
		{
			int count = 0;
			foreach (OperationalActionMethodApplicator applicator in runner.MethodApplicators)
			{
				if (applicator is ApplicatorT)
				{
					count++;
				}
			}

			return count;
		}

		protected abstract void ConfigureForInclusion(OperationalAction action);
		protected abstract void ConfigureForExclusion(OperationalAction action);
		protected abstract ApplicatorT NewApplicator(OperationalActionRunner runner);
		protected virtual Type TargetType
		{
			get
			{
				return typeof(DummyBusinessObjectWithDocumentSupport);
			}
		}

		protected sealed override BusinessObject GetNewBusinessObject()
		{
			return NewApplicator(Runner);
		}

		public OperationalActionRunner Runner
		{
			get
			{
				return runner ?? (runner = new OperationalActionRunner(Action, TargetType, new SelectedRecords()));
			}
		}

		OperationalActionRunner runner;
		public OperationalAction Action
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(action))
				{
					action = Factory.New<OperationalAction>();
					action.Context = Context;
				}

				return action;
			}
		}

		OperationalAction action;
		public OperationalActionContext Context
		{
			get
			{
				return context ?? (context = new OperationalActionContext(ActionSupporter, "Module Name"));
			}
		}

		OperationalActionContext context;
		public OperationalActionSupporter ActionSupporter
		{
			get
			{
				return actionSupporter ?? (actionSupporter = NewActionSupporter());
			}
		}

		protected virtual OperationalActionSupporter NewActionSupporter()
		{
			return new MockOperationalActionSupportable(TargetType).OperationalActionSupporter;
		}

		OperationalActionSupporter actionSupporter;
		public DummyBusinessObjectWithDocumentSupport Dummy1
		{
			get
			{
				return dummy1 ?? (dummy1 = Factory.New<DummyBusinessObjectWithDocumentSupport>());
			}
		}

		DummyBusinessObjectWithDocumentSupport dummy1;
		public DummyBusinessObjectWithDocumentSupport Dummy2
		{
			get
			{
				return dummy2 ?? (dummy2 = Factory.New<DummyBusinessObjectWithDocumentSupport>());
			}
		}

		DummyBusinessObjectWithDocumentSupport dummy2;
		public DummyBusinessObjectWithDocumentSupport Dummy3
		{
			get
			{
				return dummy3 ?? (dummy3 = Factory.New<DummyBusinessObjectWithDocumentSupport>());
			}
		}

		DummyBusinessObjectWithDocumentSupport dummy3;
		#endregion
	}
}
