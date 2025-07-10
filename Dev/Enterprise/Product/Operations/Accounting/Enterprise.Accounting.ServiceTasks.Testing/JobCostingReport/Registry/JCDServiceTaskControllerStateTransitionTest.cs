using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	/*
	 
	***********************State transiton Map*************************************
	
											TO 
					|	NON	|	STR	|	CDB	|	POR	|	CUP	|	RST	|	RMV
			-----------------------------------------------------------------
			NON		|	-		Y		N		N		N		N		N
			STR		|	Y		-		SY		N		N		N		N
			CDB		|	N		N		-		SY		N		Y		Y
	FROM	POR		|	N		N		N		-		SY		Y		Y
			CUP		|	N		N		N		N		-		Y		Y
			RST		|	N		N		N		SY		N		-		Y
			RMV		|	SY		N		D		D		D		N		-

			Y = Yes
			N = No
			SY = Both System and User is allowed to do.
			D = Depeneds on what is the state of JCD related DB objects.
	  
	 */
	class JCDServiceTaskControllerStateTransitionTest : TestCaseWithFactory
	{
		public void TestStateTransitionValidation_NON()
		{
			var regItem = AccountingConfigurationRegistry.Instance.JCDServiceTaskController;
			AssertNoExceptionThrown(() => regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "NON"));
			AssertEquals("Value", "NON", regItem.Value);

			var allowedActions = new[] { "STR" };
			var notAllowedActionsBecauseOfDBState = new[] { "RMV", "RST", "CDB", "POR", "CUP" };

			AssertStatusTransition(allowedActions, notAllowedActionsBecauseOfDBState);
		}

		public void TestStateTransitionValidation_STR()
		{
			var regItem = AccountingConfigurationRegistry.Instance.JCDServiceTaskController;
			AssertNoExceptionThrown(() => regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "STR"));
			AssertEquals("Value", "STR", regItem.Value);

			var allowedActions = new[] { "NON" };
			var notAllowedActionsBecauseOfDBState = new[] { "RMV", "RST", "CDB", "POR", "CUP" };

			AssertStatusTransition(allowedActions, notAllowedActionsBecauseOfDBState);
		}

		public void TestStateTransitionValidation_CDB()
		{
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "STR");
			var startAction = new JCDStartActionStrategy(TestConnection, new TestServiceLogger());
			startAction.Process();

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "CDB");
			AssertEquals("Current Registry Value", "CDB", AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

			CreateTranasctionLine(true);

			var allowedActions = new[] { "RMV", "RST", "POR" };
			var notAllowedActionsBecauseOfDBState = new[] { "NON", "STR", "CUP" };

			AssertStatusTransition(allowedActions, notAllowedActionsBecauseOfDBState);
		}

		public void TestStateTransitionValidation_POR()
		{
			CreateTranasctionLine(false);

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "STR");
			var startAction = new JCDStartActionStrategy(TestConnection, new TestServiceLogger());
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				startAction.Process();
			}
			AssertEquals("Current Registry Value", "POR", AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

			var allowedActions = new[] { "RMV", "RST" };
			var notAllowedActionsBecauseOfDBState = new[] { "NON", "STR", "CDB", "POR", "CUP" };

			AssertStatusTransition(allowedActions, notAllowedActionsBecauseOfDBState);
		}

		public void TestStateTransitionValidation_CUP()
		{
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "STR");
				var startAction = new JCDStartActionStrategy(TestConnection, new TestServiceLogger());
				startAction.Process();
				AssertEquals("Current Registry Value", "POR", AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

				var processAction = new JCDDefaultActionStrategy(TestConnection, new TestServiceLogger());
				processAction.Process();
				AssertEquals("Current Registry Value", "CUP", AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

				processAction = new JCDDefaultActionStrategy(TestConnection, new TestServiceLogger());
				processAction.Process();

				var allowedActions = new[] { "RMV", "RST" };
				var notAllowedActionsBecauseOfDBState = new[] { "NON", "STR", "CDB", "POR" };

				AssertStatusTransition(allowedActions, notAllowedActionsBecauseOfDBState);
			}
		}

		public void TestStateTransitionValidation_CUP_BeforeNextServiceTaskRun()
		{
			CreateTranasctionLine(false);
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "STR");
			var startAction = new JCDStartActionStrategy(TestConnection, new TestServiceLogger());
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				startAction.Process();
			}
			AssertEquals("Current Registry Value", "POR", AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

			var processAction = new JCDDefaultActionStrategy(TestConnection, new TestServiceLogger());
			processAction.Process();
			AssertEquals("Current Registry Value", "CUP", AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

			var allowedActions = Array.Empty<string>();
			var notAllowedActionsBecauseOfDBState = new[] { "CDB" };
			AssertStatusTransition(allowedActions, notAllowedActionsBecauseOfDBState);
		}

		#region From RMV to

		public void TestStateTransitionValidation_RMV()
		{
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "STR");
			var startAction = new JCDStartActionStrategy(TestConnection, new TestServiceLogger());
			startAction.Process();
			AssertEquals("Current Registry Value", "POR", AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

			var regItem = AccountingConfigurationRegistry.Instance.JCDServiceTaskController;
			AssertNoExceptionThrown(() => regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "RMV"));
			AssertEquals("Value", "RMV", regItem.Value);

			var allowedActions = new[] { "RST" };
			var notAllowedActionsBecauseOfDBState = new[] { "NON", "STR" };

			AssertStatusTransition(allowedActions, notAllowedActionsBecauseOfDBState, "RST", "NON", "STR");
		}

		public void TestStateTransitionValidation_RMVToCDB()
		{
			var regItem = AccountingConfigurationRegistry.Instance.JCDServiceTaskController;

			regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "STR");
			var startAction = new JCDStartActionStrategy(TestConnection, new TestServiceLogger());
			startAction.Process();
			regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "CDB");
			AssertEquals("Current Registry Value", "CDB", AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

			regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "RMV");
			AssertEquals("Value", "RMV", regItem.Value);

			AssertStatusTransition(new[] { "CDB" }, Array.Empty<string>(), "CDB"); //Allowed to changes status to CDB as temp tables still exist in the database and they are Empty.

			//Now create few transaction lines and try to change status
			CreateTranasctionLine(true);

			AssertNoExceptionThrown(() => regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "RMV"));
			AssertEquals("Value", "RMV", regItem.Value);

			AssertStatusTransition(Array.Empty<string>(), new[] { "CDB" }, "CDB"); //Not allowed to changes status to CDB as temp tables still exist in the database and they are not Empty.
		}

		public void TestStateTransitionValidation_RMVToPOR()
		{
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var regItem = AccountingConfigurationRegistry.Instance.JCDServiceTaskController;

				regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "STR");
				var startAction = new JCDStartActionStrategy(TestConnection, new TestServiceLogger());
				startAction.Process();
				AssertEquals("Current Registry Value", "POR", AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

				regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "RMV");
				AssertEquals("Value", "RMV", regItem.Value);

				AssertStatusTransition(new[] { "POR" }, Array.Empty<string>(), "POR"); //Allowed to changes status to POR as temp tables still exist in the database.

				//Now remove all temp tables 
				var processAction = new JCDDefaultActionStrategy(TestConnection, new TestServiceLogger());
				processAction.Process();
				AssertEquals("Current Registry Value", "CUP", AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

				processAction = new JCDDefaultActionStrategy(TestConnection, new TestServiceLogger());
				processAction.Process();

				regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "RMV");
				AssertEquals("Value", "RMV", regItem.Value);

				AssertStatusTransition(Array.Empty<string>(), new[] { "POR" }, "POR"); //Not Allowed to changes status to POR as temp tables do not exist in the database.
			}
		}

		public void TestStateTransitionValidation_RMVToCUP()
		{
			var regItem = AccountingConfigurationRegistry.Instance.JCDServiceTaskController;

			CreateTranasctionLine(false);

			regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "STR");
			var startAction = new JCDStartActionStrategy(TestConnection, new TestServiceLogger());
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				startAction.Process();
			}
			AssertEquals("Current Registry Value", "POR", AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

			regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "RMV");
			AssertEquals("Value", "RMV", regItem.Value);

			AssertStatusTransition(Array.Empty<string>(), new[] { "CUP" }, "CUP"); //Not Allowed to changes status to CUP as temp tables still exist in the database.

			//Now remove all temp tables 
			regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "POR");
			var processAction = new JCDDefaultActionStrategy(TestConnection, new TestServiceLogger());
			processAction.Process();
			AssertEquals("Current Registry Value", "CUP", AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

			processAction = new JCDDefaultActionStrategy(TestConnection, new TestServiceLogger());
			processAction.Process();

			regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "RMV");
			AssertEquals("Value", "RMV", regItem.Value);

			AssertStatusTransition(new[] { "CUP" }, Array.Empty<string>(), "CUP"); //Allowed to changes status to CUP as temp tables doesn't exist in the database.
		}

		#endregion

		#region From RST to

		public void TestStateTransitionValidation_RST()
		{
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "STR");
			var startAction = new JCDStartActionStrategy(TestConnection, new TestServiceLogger());
			startAction.Process();
			AssertEquals("Current Registry Value", "POR", AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

			var regItem = AccountingConfigurationRegistry.Instance.JCDServiceTaskController;
			AssertNoExceptionThrown(() => regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "RST"));
			AssertEquals("Value", "RST", regItem.Value);

			var allowedActions = new[] { "RMV" };
			var notAllowedActionsBecauseOfDBState = new[] { "NON", "STR" };

			AssertStatusTransition(allowedActions, notAllowedActionsBecauseOfDBState);
		}

		public void TestStateTransitionValidation_RSTToCDB()
		{
			var regItem = AccountingConfigurationRegistry.Instance.JCDServiceTaskController;

			regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "STR");
			var startAction = new JCDStartActionStrategy(TestConnection, new TestServiceLogger());
			startAction.Process();
			regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "CDB");
			AssertEquals("Current Registry Value", "CDB", AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

			regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "RST");
			AssertEquals("Value", "RST", regItem.Value);

			AssertStatusTransition(new[] { "CDB" }, Array.Empty<string>(), "CDB"); //Allowed to changes status to CDB as temp tables still exist in the database and they are Empty.

			//Now create few transaction lines and try to change status
			CreateTranasctionLine(true);

			AssertNoExceptionThrown(() => regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "RST"));
			AssertEquals("Value", "RST", regItem.Value);

			AssertStatusTransition(Array.Empty<string>(), new[] { "CDB" }, "CDB"); //Not allowed to changes status to CDB as temp tables still exist in the database and they are not Empty.
		}

		public void TestStateTransitionValidation_RSTToPOR()
		{
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var regItem = AccountingConfigurationRegistry.Instance.JCDServiceTaskController;

				regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "STR");
				var startAction = new JCDStartActionStrategy(TestConnection, new TestServiceLogger());
				startAction.Process();
				AssertEquals("Current Registry Value", "POR", AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

				regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "RST");
				AssertEquals("Value", "RST", regItem.Value);

				AssertStatusTransition(new[] { "POR" }, Array.Empty<string>(), "POR"); //Allowed to changes status to POR as temp tables still exist in the database.

				//Now remove all temp tables 
				var processAction = new JCDDefaultActionStrategy(TestConnection, new TestServiceLogger());
				processAction.Process();
				AssertEquals("Current Registry Value", "CUP", AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

				processAction = new JCDDefaultActionStrategy(TestConnection, new TestServiceLogger());
				processAction.Process();

				regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "RST");
				AssertEquals("Value", "RST", regItem.Value);

				AssertStatusTransition(Array.Empty<string>(), new[] { "POR" }, "POR"); //Not Allowed to changes status to POR as temp tables do not exist in the database.
			}
		}

		public void TestStateTransitionValidation_RSTToCUP()
		{
			var regItem = AccountingConfigurationRegistry.Instance.JCDServiceTaskController;

			CreateTranasctionLine(false);

			regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "STR");
			var startAction = new JCDStartActionStrategy(TestConnection, new TestServiceLogger());
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				startAction.Process();
			}
			AssertEquals("Current Registry Value", "POR", AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

			regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "RST");
			AssertEquals("Value", "RST", regItem.Value);

			AssertStatusTransition(Array.Empty<string>(), new[] { "CUP" }, "CUP"); //Not Allowed to changes status to CUP as temp tables still exist in the database.

			//Now remove all temp tables 
			regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "POR");
			var processAction = new JCDDefaultActionStrategy(TestConnection, new TestServiceLogger());
			processAction.Process();
			AssertEquals("Current Registry Value", "CUP", AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value);

			processAction = new JCDDefaultActionStrategy(TestConnection, new TestServiceLogger());
			processAction.Process();

			regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "RST");
			AssertEquals("Value", "RST", regItem.Value);

			AssertStatusTransition(new[] { "CUP" }, Array.Empty<string>(), "CUP"); //Allowed to changes status to CUP as temp tables doesn't exist in the database.
		}

		#endregion

		void CreateTranasctionLine(bool populateTempTableAsWell)
		{
			var jobs = Helper.CreateJobsWithPeriod(false);

			var apLine1 = Helper.CreateCSTLine(jobs[0], 1, 113, reverse: true);
			var apLine2 = Helper.CreateCSTLine(jobs[1], 2, 123, reverse: true);

			var arLine1 = Helper.CreateREVLine(jobs[0], 1, 112, reverse: true);
			var arLine2 = Helper.CreateREVLine(jobs[1], 2, 122, reverse: true);

			var wipacr1 = Helper.CreateACRWIPLine(jobs[0], 1, 111, 110, reverseWIP: false, reverseACR: false);
			var wipacr2 = Helper.CreateACRWIPLine(jobs[1], 2, 121, 120, reverseWIP: false, reverseACR: false);

			if (populateTempTableAsWell)
			{
				TestConnection.ExecuteNonQuery("INSERT INTO RptDtUnprocessedAccTransactionLines SELECT AL_PK FROM dbo.AccTransactionLines");
			}
		}

		void AssertStatusTransition(string[] allowedActions, string[] notAllowedActionsBecauseOfDBState, params string[] codes)
		{
			var regItem = AccountingConfigurationRegistry.Instance.JCDServiceTaskController;

			if (codes.Length == 0)
			{
				codes = new JCDActionList().GetAllCodes().Where(x => x != regItem.Value).ToArray();
			}

			foreach (string code in codes)
			{
				if (allowedActions.Contains(code))
				{
					AssertNoExceptionThrown($"Expect No Exception for {code}", () => regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, code));
				}
				else if (notAllowedActionsBecauseOfDBState.Contains(code))
				{
					var expectedErrMsg = string.Empty;
					switch (code)
					{
						case JCDActionList.Codes.NotInitialized:
							expectedErrMsg = "You cannot change the registry value to NON, as Job Costing Data Queue Service task is already initialized.";
							break;
						case JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask:
							expectedErrMsg = "You cannot change the registry value to STR, as Job Costing Data Queue Service task is already initialized.";
							break;
						case JCDActionList.Codes.CompletedAllDatabaseObjectsForJobCostingDataQueueHaveBeenCreated:
							expectedErrMsg = "You cannot change the registry value to CDB, as either there are unprocessed old transaction records in Database or Job Costing Data Queue service task is not initialized.";
							break;
						case JCDActionList.Codes.ProcessingOldTransactionLines:
							expectedErrMsg = "You cannot change the registry value to POR, as either there is no unprocessed transaction record or Job Costing Data Queue service task is not initialized.";
							break;
						case JCDActionList.Codes.CompletedOldTransactionLinesHaveBeenProcessed:
							expectedErrMsg = "You cannot change the registry value to CUP, as either there are unprocessed old transaction records or Job Costing Data Queue service task is not initialized.";
							break;
						case JCDActionList.Codes.ReInitializeJobCostingDataQueueServiceTask:
							expectedErrMsg = "You cannot change the registry value to RST, as Job Costing Data Queue Service task is still non-initialized.";
							break;
						case JCDActionList.Codes.RemoveJobCostingDataQueueServiceTask:
							expectedErrMsg = "You cannot change the registry value to RMV, as Job Costing Data Queue Service task is still non-initialized.";
							break;

						default:
							break;
					}
					AssertExceptionThrown<RegistryValidationException>($"Expected Error for {code} : {expectedErrMsg}", () => regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, code));
				}
			}
		}

		protected JobCostingReportDataTestHelper Helper
		{
			get { return helper ?? (helper = new JobCostingReportDataTestHelper(ObjectCreator)); }
		}

		JobCostingReportDataTestHelper helper;

		protected TestObjectCreator ObjectCreator
		{
			get { return objectCreator ?? (objectCreator = new TestObjectCreator(Factory)); }
		}

		TestObjectCreator objectCreator;
	}
}
