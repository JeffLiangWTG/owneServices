namespace Enterprise.Client.EDI.ServiceTask.Testing
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
using System.Threading;
	using CargoWise.Common;
	using CargoWise.Data;
	using Enterprise.Client.EDI.AmnestyReporter.Business;
	using Enterprise.Client.EDI.IncidentManager.Business;
	using Enterprise.Client.EDI.ServiceTask;
	using Enterprise.MasterFiles.Business;
	using NUnit.Framework;
	class AmnestiesProcessorServiceTaskGroupingTests : TransactionedTestCase
	{
		public void TestMethodGroupingSameClassDifferentMethod()
		{
			var serviceTask = new AmnestiesProcessorServiceTask();
			var datState = GenerateDatState();
			var existingWI = datState.WorkItems.First();
			datState.AmnestyFailures.First().AF_IM = existingWI.WKI_PK;
			datState.Responsibilities.First().ST_Amnesty_WI_Grouping = "MET";

			var newFailure = new AmnestyFailures(new Guid());
			var anotherMethod = new TestMethod(new Guid(), "DifferentTestMethod");
			anotherMethod.AmnestyFailures.Add(newFailure);

			datState.Assemblies.First().Classes.First().Methods.Add(anotherMethod);
			InsertWrapperIntoDB(datState);

			var enumerable = serviceTask.FilterAndTranformWorkItems(serviceTask.Download());
			AssertEquals("Expect one DatAmnestyFailure to process", 1, enumerable.Count());
			var unprocessedAmnestyFailure = enumerable.First();
			AssertDatAmnestyFailureNoMatchingWorkItem(unprocessedAmnestyFailure);
		}

		public void TestMethodGroupingSameDllDifferentClass()
		{
			var serviceTask = new AmnestiesProcessorServiceTask();
			var datState = GenerateDatState();
			var existingWI = datState.WorkItems.First();
			datState.AmnestyFailures.First().AF_IM = existingWI.WKI_PK;
			datState.Responsibilities.First().ST_Amnesty_WI_Grouping = "MET";

			var newFailure = new AmnestyFailures(new Guid());
			var differentMethod = new TestMethod(new Guid(), "DifferentTestMethod");
			differentMethod.AmnestyFailures.Add(newFailure);
			var differentClass = new TestClass(new Guid(), "DifferentTestClass");
			differentClass.Methods.Add(differentMethod);
			datState.Assemblies.First().Classes.Add(differentClass);
			InsertWrapperIntoDB(datState);

			var enumerable = serviceTask.FilterAndTranformWorkItems(serviceTask.Download());
			AssertEquals("Expect one DatAmnestyFailure to process", 1, enumerable.Count());
			var unprocessedAmnestyFailure = enumerable.First();
			AssertDatAmnestyFailureNoMatchingWorkItem(unprocessedAmnestyFailure);
		}

		public void TestMethodGroupingSameNodeDifferentDll()
		{
			var serviceTask = new AmnestiesProcessorServiceTask();
			var datState = GenerateDatState();
			var existingWI = datState.WorkItems.First();
			datState.AmnestyFailures.First().AF_IM = existingWI.WKI_PK;
			datState.Responsibilities.First().ST_Amnesty_WI_Grouping = "MET";

			var newFailure = new AmnestyFailures(new Guid());
			var differentMethod = new TestMethod(new Guid(), "DifferentTestMethod");
			differentMethod.AmnestyFailures.Add(newFailure);
			var differentClass = new TestClass(new Guid(), "DifferentTestClass");
			differentClass.Methods.Add(differentMethod);
			var differentDll = new Assembly(new Guid(), "Different.dll.same.path", "$/path1/dll");
			differentDll.Classes.Add(differentClass);

			datState.Assemblies.Add(differentDll);
			InsertWrapperIntoDB(datState);

			var enumerable = serviceTask.FilterAndTranformWorkItems(serviceTask.Download());
			AssertEquals("Expect one DatAmnestyFailure to process", 1, enumerable.Count());
			var unprocessedAmnestyFailure = enumerable.First();
			AssertDatAmnestyFailureNoMatchingWorkItem(unprocessedAmnestyFailure);
		}

		public void TestClassGroupingSameClassDifferentMethod()
		{
			var serviceTask = new AmnestiesProcessorServiceTask();
			var datState = GenerateDatState();
			var existingWI = datState.WorkItems.First();
			datState.AmnestyFailures.First().AF_IM = existingWI.WKI_PK;
			datState.Responsibilities.First().ST_Amnesty_WI_Grouping = "CLS";

			var newFailure = new AmnestyFailures(new Guid());
			var differentMethod = new TestMethod(new Guid(), "DifferentTestMethod");

			differentMethod.AmnestyFailures.Add(newFailure);

			datState.Assemblies.First().Classes.First().Methods.Add(differentMethod);
			InsertWrapperIntoDB(datState);

			var enumerable = serviceTask.FilterAndTranformWorkItems(serviceTask.Download());
			AssertEquals("Expect one DatAmnestyFailure to process", 1, enumerable.Count());
			var unprocessedAmnestyFailure = enumerable.First();
			AssertDatAmnestyFailureEqualsWorkItem(existingWI, unprocessedAmnestyFailure);
		}

		public void TestClassGroupingSameDllDifferentClass()
		{
			var serviceTask = new AmnestiesProcessorServiceTask();
			var datState = GenerateDatState();
			var existingWI = datState.WorkItems.First();
			datState.AmnestyFailures.First().AF_IM = existingWI.WKI_PK;
			datState.Responsibilities.First().ST_Amnesty_WI_Grouping = "CLS";

			var newFailure = new AmnestyFailures(new Guid());
			var differentMethod = new TestMethod(new Guid(), "DifferentTestMethod");
			differentMethod.AmnestyFailures.Add(newFailure);
			var differentClass = new TestClass(new Guid(), "DifferentTestClass");
			differentClass.Methods.Add(differentMethod);

			datState.Assemblies.First().Classes.Add(differentClass);
			InsertWrapperIntoDB(datState);

			var enumerable = serviceTask.FilterAndTranformWorkItems(serviceTask.Download());
			AssertEquals("Expect one DatAmnestyFailure to process", 1, enumerable.Count());
			var unprocessedAmnestyFailure = enumerable.First();
			AssertDatAmnestyFailureNoMatchingWorkItem(unprocessedAmnestyFailure);
		}

		public void TestClassGroupingSameNodeDifferentDll()
		{
			var serviceTask = new AmnestiesProcessorServiceTask();
			var datState = GenerateDatState();
			var existingWI = datState.WorkItems.First();
			datState.AmnestyFailures.First().AF_IM = existingWI.WKI_PK;
			datState.Responsibilities.First().ST_Amnesty_WI_Grouping = "CLS";

			var newFailure = new AmnestyFailures(new Guid());
			var differentMethod = new TestMethod(new Guid(), "DifferentTestMethod");
			differentMethod.AmnestyFailures.Add(newFailure);
			var differentClass = new TestClass(new Guid(), "DifferentTestClass");
			differentClass.Methods.Add(differentMethod);
			var differentDll = new Assembly(new Guid(), "Different.dll.same.path", "$/path1/dll");
			differentDll.Classes.Add(differentClass);

			datState.Assemblies.Add(differentDll);
			InsertWrapperIntoDB(datState);

			var enumerable = serviceTask.FilterAndTranformWorkItems(serviceTask.Download());
			AssertEquals("Expect one DatAmnestyFailure to process", 1, enumerable.Count());
			var unprocessedAmnestyFailure = enumerable.First();
			AssertDatAmnestyFailureNoMatchingWorkItem(unprocessedAmnestyFailure);
		}

		public void TestDllGroupingSameClassDifferentMethod()
		{
			var serviceTask = new AmnestiesProcessorServiceTask();
			var datState = GenerateDatState();
			var existingWI = datState.WorkItems.First();
			datState.AmnestyFailures.First().AF_IM = existingWI.WKI_PK;
			datState.Responsibilities.First().ST_Amnesty_WI_Grouping = "DLL";

			var newFailure = new AmnestyFailures(new Guid());
			var differentMethod = new TestMethod(new Guid(), "DifferentTestMethod");

			differentMethod.AmnestyFailures.Add(newFailure);

			datState.Assemblies.First().Classes.First().Methods.Add(differentMethod);
			InsertWrapperIntoDB(datState);

			var enumerable = serviceTask.FilterAndTranformWorkItems(serviceTask.Download());
			AssertEquals("Expect one DatAmnestyFailure to process", 1, enumerable.Count());
			var unprocessedAmnestyFailure = enumerable.First();
			AssertDatAmnestyFailureEqualsWorkItem(existingWI, unprocessedAmnestyFailure);
		}

		public void TestDllGroupingSameDllDifferentClass()
		{
			var serviceTask = new AmnestiesProcessorServiceTask();
			var datState = GenerateDatState();
			var existingWI = datState.WorkItems.First();
			datState.AmnestyFailures.First().AF_IM = existingWI.WKI_PK;
			datState.Responsibilities.First().ST_Amnesty_WI_Grouping = "DLL";

			var newFailure = new AmnestyFailures(new Guid());
			var differentMethod = new TestMethod(new Guid(), "DifferentTestMethod");
			differentMethod.AmnestyFailures.Add(newFailure);
			var differentClass = new TestClass(new Guid(), "DifferentTestClass");
			differentClass.Methods.Add(differentMethod);

			datState.Assemblies.First().Classes.Add(differentClass);
			InsertWrapperIntoDB(datState);

			var enumerable = serviceTask.FilterAndTranformWorkItems(serviceTask.Download());
			AssertEquals("Expect one DatAmnestyFailure to process", 1, enumerable.Count());
			var unprocessedAmnestyFailure = enumerable.First();
			AssertDatAmnestyFailureEqualsWorkItem(existingWI, unprocessedAmnestyFailure);
		}

		public void TestDllGroupingSameNodeDifferentDll()
		{
			var serviceTask = new AmnestiesProcessorServiceTask();
			var datState = GenerateDatState();
			var existingWI = datState.WorkItems.First();
			datState.AmnestyFailures.First().AF_IM = existingWI.WKI_PK;
			datState.Responsibilities.First().ST_Amnesty_WI_Grouping = "DLL";

			var newFailure = new AmnestyFailures(new Guid());
			var differentMethod = new TestMethod(new Guid(), "DifferentTestMethod");
			differentMethod.AmnestyFailures.Add(newFailure);
			var differentClass = new TestClass(new Guid(), "DifferentTestClass");
			differentClass.Methods.Add(differentMethod);
			var differentDll = new Assembly(new Guid(), "Different.dll.same.path", "$/path1/dll");
			differentDll.Classes.Add(differentClass);

			datState.Assemblies.Add(differentDll);
			InsertWrapperIntoDB(datState);

			var enumerable = serviceTask.FilterAndTranformWorkItems(serviceTask.Download());
			AssertEquals("Expect one DatAmnestyFailure to process", 1, enumerable.Count());
			var unprocessedAmnestyFailure = enumerable.First();
			AssertDatAmnestyFailureNoMatchingWorkItem(unprocessedAmnestyFailure);
		}

		public void TestNodeGroupingSameClassDifferentMethod()
		{
			var serviceTask = new AmnestiesProcessorServiceTask();
			var datState = GenerateDatState();
			var existingWI = datState.WorkItems.First();
			datState.AmnestyFailures.First().AF_IM = existingWI.WKI_PK;
			datState.Responsibilities.First().ST_Amnesty_WI_Grouping = "STN";

			var newFailure = new AmnestyFailures(new Guid());
			var differentMethod = new TestMethod(new Guid(), "DifferentTestMethod");

			differentMethod.AmnestyFailures.Add(newFailure);

			datState.Assemblies.First().Classes.First().Methods.Add(differentMethod);
			InsertWrapperIntoDB(datState);

			var enumerable = serviceTask.FilterAndTranformWorkItems(serviceTask.Download());
			AssertEquals("Expect one DatAmnestyFailure to process", 1, enumerable.Count());
			var unprocessedAmnestyFailure = enumerable.First();
			AssertDatAmnestyFailureEqualsWorkItem(existingWI, unprocessedAmnestyFailure);
		}

		public void TestNodeGroupingSameDllDifferentClass()
		{
			var serviceTask = new AmnestiesProcessorServiceTask();
			var datState = GenerateDatState();
			var existingWI = datState.WorkItems.First();
			datState.AmnestyFailures.First().AF_IM = existingWI.WKI_PK;
			datState.Responsibilities.First().ST_Amnesty_WI_Grouping = "STN";

			var newFailure = new AmnestyFailures(new Guid());
			var differentMethod = new TestMethod(new Guid(), "DifferentTestMethod");
			differentMethod.AmnestyFailures.Add(newFailure);
			var differentClass = new TestClass(new Guid(), "DifferentTestClass");
			differentClass.Methods.Add(differentMethod);

			datState.Assemblies.First().Classes.Add(differentClass);
			InsertWrapperIntoDB(datState);

			var enumerable = serviceTask.FilterAndTranformWorkItems(serviceTask.Download());
			AssertEquals("Expect one DatAmnestyFailure to process", 1, enumerable.Count());
			var unprocessedAmnestyFailure = enumerable.First();
			AssertDatAmnestyFailureEqualsWorkItem(existingWI, unprocessedAmnestyFailure);
		}
		public void TestNodeGroupingSameNodeDifferentDll()
		{
			var serviceTask = new AmnestiesProcessorServiceTask();
			var datState = GenerateDatState();
			var existingWI = datState.WorkItems.First();
			datState.AmnestyFailures.First().AF_IM = existingWI.WKI_PK;
			datState.Responsibilities.First().ST_Amnesty_WI_Grouping = "STN";

			var newFailure = new AmnestyFailures(new Guid());
			var differentMethod = new TestMethod(new Guid(), "DifferentTestMethod");
			differentMethod.AmnestyFailures.Add(newFailure);
			var differentClass = new TestClass(new Guid(), "DifferentTestClass");
			differentClass.Methods.Add(differentMethod);
			var differentDll = new Assembly(new Guid(), "Different.dll.same.path", "$/path1/dll");
			differentDll.Classes.Add(differentClass);

			datState.Assemblies.Add(differentDll);
			InsertWrapperIntoDB(datState);

			var enumerable = serviceTask.FilterAndTranformWorkItems(serviceTask.Download());
			AssertEquals("Expect one DatAmnestyFailure to process", 1, enumerable.Count());
			var unprocessedAmnestyFailure = enumerable.First();
			AssertDatAmnestyFailureEqualsWorkItem(existingWI, unprocessedAmnestyFailure);
		}

		public void TestNodeGroupingDifferentNodeSameModule()
		{
			var serviceTask = new AmnestiesProcessorServiceTask();
			var datState = GenerateDatState();
			var existingWI = datState.WorkItems.First();
			datState.AmnestyFailures.First().AF_IM = existingWI.WKI_PK;

			datState.Responsibilities.First().ST_Amnesty_WI_Grouping = "STN";
			datState.Responsibilities.Add(new SourceTreeResponsibility("$/", "STN"));
			datState.Responsibilities.Add(new SourceTreeResponsibility("$/New", "STN"));

			var newFailure = new AmnestyFailures(new Guid());
			var differentMethod = new TestMethod(new Guid(), "DifferentTestMethod");
			differentMethod.AmnestyFailures.Add(newFailure);
			var differentClass = new TestClass(new Guid(), "DifferentTestClass");
			differentClass.Methods.Add(differentMethod);
			var differentDll = new Assembly(new Guid(), "Different.dll", "$/New/node");
			differentDll.Classes.Add(differentClass);

			datState.Assemblies.Add(differentDll);
			InsertWrapperIntoDB(datState);

			var enumerable = serviceTask.FilterAndTranformWorkItems(serviceTask.Download());
			AssertEquals("Expect one DatAmnestyFailure to process", 1, enumerable.Count());
			var unprocessedAmnestyFailure = enumerable.First();
			AssertDatAmnestyFailureNoMatchingWorkItem(unprocessedAmnestyFailure);
		}

		public void TestThatMostSpecificNodeIsSelected()
		{
			var serviceTask = new AmnestiesProcessorServiceTask();
			var datState = GenerateDatState();
			var existingWI = datState.WorkItems.First();
			datState.Responsibilities.First().ST_Amnesty_WI_Grouping = "STN";
			datState.AmnestyFailures.First().AF_ExpiryDate = null;

			var decoyWI = new OdysseyWorkItem(new Guid(), 2);
			datState.WorkItems.Add(decoyWI);
			var decoyFailure = new AmnestyFailures(new Guid());
			decoyFailure.AF_IM = decoyWI.WKI_PK;
			var decoyMethod = new TestMethod(new Guid(), "DecoyMethod");
			decoyMethod.AmnestyFailures.Add(decoyFailure);
			var decoyClass = new TestClass(new Guid(), "DecoyClass");
			decoyClass.Methods.Add(decoyMethod);
			var decoyDll = new Assembly(new Guid(), "Decoy.Assembly", "$/path1");
			decoyDll.Classes.Add(decoyClass);
			datState.Assemblies.Add(decoyDll);

			var node = new SourceTreeResponsibility("$/path1/dll", "CLS");
			datState.Responsibilities.Add(node);

			InsertWrapperIntoDB(datState);

			var enumerable = serviceTask.FilterAndTranformWorkItems(serviceTask.Download());
			AssertEquals("Expect one DatAmnestyFailures to process", 1, enumerable.Count());
			var unprocessedAmnestyFailure = enumerable.First();
			AssertDatAmnestyFailureNoMatchingWorkItem(unprocessedAmnestyFailure);
		}

		public void TestExistingAmnestyData()
		{
			var sql = @"
INSERT [dbo].[Assembly] ([E8_PK], [E8_AssemblyName], [E8_IsActive], [E8_SourcePath]) VALUES (N'cf28f7a7-e436-4dd1-9c4b-77277e08c3dd', N'all\WiseClouldClientServerTesting.dll', 1, N'$/WiseCloud/WiseCloudAccessor/WiseCloudClientServerTesting')
INSERT [dbo].[TestClass] ([E2_PK], [E2_TestClass], [E2_E8]) VALUES (N'94898db9-79ee-4462-b924-ce1b9a954fd1', N'WiseClouldClientServerTesting.WiseCloudServerSearchForestTest',                       N'cf28f7a7-e436-4dd1-9c4b-77277e08c3dd')
INSERT [dbo].[TestClass] ([E2_PK], [E2_TestClass], [E2_E8]) VALUES (N'c424a8a7-85cc-4d37-9237-3e92ec7bcc7f', N'WiseClouldClientServerTesting.ServerWebTest',                                         N'cf28f7a7-e436-4dd1-9c4b-77277e08c3dd')
INSERT [dbo].[TestClass] ([E2_PK], [E2_TestClass], [E2_E8]) VALUES (N'e2c10dd0-34b6-4daa-aec9-6d6228f01560', N'WiseClouldClientServerTesting.BackendServicesTest',                                   N'cf28f7a7-e436-4dd1-9c4b-77277e08c3dd')
INSERT [dbo].[TestClass] ([E2_PK], [E2_TestClass], [E2_E8]) VALUES (N'20040fe6-4cca-445b-a591-8ff8ed0f8e5a', N'WiseClouldClientServerTesting.ChangePasswordTest',                                    N'cf28f7a7-e436-4dd1-9c4b-77277e08c3dd')
INSERT [dbo].[TestClass] ([E2_PK], [E2_TestClass], [E2_E8]) VALUES (N'efa58806-0d94-4fb1-b3b8-f82fa2b2e584', N'WiseClouldClientServerTesting.GetClientUpgradeRequiredTest',                          N'cf28f7a7-e436-4dd1-9c4b-77277e08c3dd')
INSERT [dbo].[TestClass] ([E2_PK], [E2_TestClass], [E2_E8]) VALUES (N'a2f84fe4-5615-4195-a62e-51671a495c57', N'WiseClouldClientServerTesting.GetRdpFileTest',                                        N'cf28f7a7-e436-4dd1-9c4b-77277e08c3dd')
INSERT [dbo].[TestClass] ([E2_PK], [E2_TestClass], [E2_E8]) VALUES (N'8b09e41d-e276-4b86-b6aa-d7751bb390b0', N'WiseClouldClientServerTesting.ServerAdminIETest',                                     N'cf28f7a7-e436-4dd1-9c4b-77277e08c3dd')
INSERT [dbo].[TestClass] ([E2_PK], [E2_TestClass], [E2_E8]) VALUES (N'efb5796b-db7b-4c82-a0a7-ebe22fe35ec4', N'WiseClouldClientServerTesting.ServerAdminTest',                                       N'cf28f7a7-e436-4dd1-9c4b-77277e08c3dd')
INSERT [dbo].[TestClass] ([E2_PK], [E2_TestClass], [E2_E8]) VALUES (N'81aa8cc2-c0ea-4a29-8272-fb0379ddf6d1', N'WiseClouldClientServerTesting.WebFailoverTest',                                       N'cf28f7a7-e436-4dd1-9c4b-77277e08c3dd')
INSERT [dbo].[TestClass] ([E2_PK], [E2_TestClass], [E2_E8]) VALUES (N'fd4c790b-72e1-4795-9d6a-db259e21150d', N'WiseClouldClientServerTesting.WiseCloudClientMultiServerTestWithAllWorkingFallbacks', N'cf28f7a7-e436-4dd1-9c4b-77277e08c3dd')
INSERT [dbo].[TestClass] ([E2_PK], [E2_TestClass], [E2_E8]) VALUES (N'd5360c17-fafd-4225-8bbb-f703164bcc9c', N'WiseClouldClientServerTesting.WiseCloudClientMultiServerTestWithOneWorkingFallback',  N'cf28f7a7-e436-4dd1-9c4b-77277e08c3dd')
INSERT [dbo].[TestClass] ([E2_PK], [E2_TestClass], [E2_E8]) VALUES (N'633e8ae8-290c-4fd1-ba57-22717655de40', N'WiseClouldClientServerTesting.WiseCloudClientMultiServerTestWithPrimaryOnly',         N'cf28f7a7-e436-4dd1-9c4b-77277e08c3dd')
INSERT [dbo].[TestClass] ([E2_PK], [E2_TestClass], [E2_E8]) VALUES (N'e7cd5b75-6071-441f-be18-7beb6c2a6439', N'WiseClouldClientServerTesting.WiseCloudClientServerTest',                             N'cf28f7a7-e436-4dd1-9c4b-77277e08c3dd')
INSERT [dbo].[TestClass] ([E2_PK], [E2_TestClass], [E2_E8]) VALUES (N'0e353d6b-d35f-45df-a4b2-00e86058580d', N'WiseClouldClientServerTesting.WiseCloudCrossDomainLoginTest',                         N'cf28f7a7-e436-4dd1-9c4b-77277e08c3dd')
INSERT [dbo].[TestClass] ([E2_PK], [E2_TestClass], [E2_E8]) VALUES (N'1886fefb-f4cc-4a01-b24b-487578afe15c', N'WiseClouldClientServerTesting.WiseCloudSecurityProviderServiceTests',                 N'cf28f7a7-e436-4dd1-9c4b-77277e08c3dd')
INSERT [dbo].[TestClass] ([E2_PK], [E2_TestClass], [E2_E8]) VALUES (N'da3bd26a-02e7-470e-b9a3-bfcb5d668d29', N'WiseClouldClientServerTesting.WiseCloudServerMultiBackendTest',                       N'cf28f7a7-e436-4dd1-9c4b-77277e08c3dd')
INSERT [dbo].[TestClass] ([E2_PK], [E2_TestClass], [E2_E8]) VALUES (N'8d70a923-3a7b-4032-b19a-f3ba07df71c3', N'WiseClouldClientServerTesting.ServerAdminMultipleDomainsTest',                        N'cf28f7a7-e436-4dd1-9c4b-77277e08c3dd')
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'7702ace4-1369-4592-a04d-3587f6427e9b', N'LoginInvalidPassword',                              N'94898db9-79ee-4462-b924-ce1b9a954fd1', CAST(N'2017-08-31T16:47:58.040' AS DateTime), 3002, 182, CAST(N'2018-02-28T07:59:46.507' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'5c9a249b-e219-4a29-bddf-1531abf1787f', N'LoginSucceed',                                      N'94898db9-79ee-4462-b924-ce1b9a954fd1', CAST(N'2017-08-31T16:47:58.040' AS DateTime), 7051, 170, CAST(N'2018-02-28T07:58:29.157' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'f85b2655-3ddb-4bcf-9d40-5784669bb598', N'LoginInvalidPassword',                              N'da3bd26a-02e7-470e-b9a3-bfcb5d668d29', CAST(N'2016-05-24T16:22:55.760' AS DateTime), 13027, 454, CAST(N'2018-02-28T07:59:03.873' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'0531a351-0882-42ba-9c72-816f7a8e39a4', N'LoginSucceed',                                      N'da3bd26a-02e7-470e-b9a3-bfcb5d668d29', CAST(N'2016-05-24T16:22:55.760' AS DateTime), 25307, 448, CAST(N'2018-02-28T01:39:26.937' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'a7789c54-1519-4dd7-a1fb-628bd0e0bb32', N'ConcurrentMachineStatusQueriesJoined',              N'e2c10dd0-34b6-4daa-aec9-6d6228f01560', CAST(N'2016-05-24T16:22:55.210' AS DateTime), 4635, 454, CAST(N'2018-02-28T08:00:13.720' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'a9e6af53-b46c-4114-9b3f-4e22e78c1ad9', N'MachineStatusExceptionsJoinedNotCached',            N'e2c10dd0-34b6-4daa-aec9-6d6228f01560', CAST(N'2016-05-24T16:22:55.223' AS DateTime), 2153, 454, CAST(N'2018-02-28T08:00:13.720' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'2d8ed9b1-7ccf-4ea8-996e-333059e44c6d', N'MachineStatusQueriesCached',                        N'e2c10dd0-34b6-4daa-aec9-6d6228f01560', CAST(N'2016-05-24T16:22:55.227' AS DateTime), 2124, 454, CAST(N'2018-02-28T08:00:13.720' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'1a5ad68d-6336-4f77-b5ff-42be28d9310c', N'ChangePassswordLoginFailure',                       N'20040fe6-4cca-445b-a591-8ff8ed0f8e5a', CAST(N'2016-05-24T16:22:55.230' AS DateTime), 7278, 388, CAST(N'2018-02-28T08:00:13.720' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'a1571182-8a83-4ab4-9bc3-2a40e43be2db', N'ChangePassswordPolicyFailure',                      N'20040fe6-4cca-445b-a591-8ff8ed0f8e5a', CAST(N'2016-05-24T16:22:55.233' AS DateTime), 4806, 388, CAST(N'2018-02-28T08:00:13.720' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'634121bf-8810-4988-bb56-766b54c8c704', N'InstallerVersion',                                  N'efa58806-0d94-4fb1-b3b8-f82fa2b2e584', CAST(N'2016-05-24T16:22:55.290' AS DateTime), 859, 454, CAST(N'2018-02-28T08:00:13.720' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'c891acc2-29f5-4ec6-becf-0d8c3c9a75d7', N'NoUpgradeRequired',                                 N'efa58806-0d94-4fb1-b3b8-f82fa2b2e584', CAST(N'2016-05-24T16:22:55.293' AS DateTime), 3224, 454, CAST(N'2018-02-28T08:00:13.720' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'abce453f-6780-4119-b308-9598d141c1ce', N'UpgradeRequired',                                   N'efa58806-0d94-4fb1-b3b8-f82fa2b2e584', CAST(N'2016-05-24T16:22:55.360' AS DateTime), 3892, 454, CAST(N'2018-02-28T08:00:13.720' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'98fef35c-9061-4cce-9d84-6c5c3eca7c49', N'ErrorMessageWhenNoTServersAvailable',               N'a2f84fe4-5615-4195-a62e-51671a495c57', CAST(N'2016-05-24T16:22:55.370' AS DateTime), 6809, 436, CAST(N'2018-02-28T07:59:18.927' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'4f01bac1-3442-4ece-adc3-390b716dbc21', N'ErrorMessageWhenNoTServersAvailableOnShortcut',     N'a2f84fe4-5615-4195-a62e-51671a495c57', CAST(N'2016-05-24T16:22:55.373' AS DateTime), 3363, 436, CAST(N'2018-02-28T07:59:11.413' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'cca0042a-8f20-4df7-b6e4-51bc3fca8905', N'AccessDenied',                                      N'8b09e41d-e276-4b86-b6aa-d7751bb390b0', CAST(N'2016-05-24T16:22:55.377' AS DateTime), NULL, NULL, NULL)
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'5eb2e38c-e76e-4d01-bfc3-3c867db09081', N'InvalidUserName',                                   N'8b09e41d-e276-4b86-b6aa-d7751bb390b0', CAST(N'2016-05-24T16:22:55.380' AS DateTime), NULL, NULL, NULL)
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'7f5b2ecb-037c-4ec1-ad23-cb8ee0e61c0a', N'LoginSuccess',                                      N'8b09e41d-e276-4b86-b6aa-d7751bb390b0', CAST(N'2016-05-24T16:22:55.383' AS DateTime), NULL, NULL, NULL)
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'8896e487-27d4-451b-8b87-285f02d5ebb6', N'AccessDenied',                                      N'efb5796b-db7b-4c82-a0a7-ebe22fe35ec4', CAST(N'2016-05-24T16:22:55.387' AS DateTime), 5327, 269, CAST(N'2018-02-28T07:59:05.387' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'c11dd049-1681-4072-8d10-efd87992cb72', N'InvalidUserName',                                   N'efb5796b-db7b-4c82-a0a7-ebe22fe35ec4', CAST(N'2016-05-24T16:22:55.390' AS DateTime), 4924, 283, CAST(N'2018-02-28T08:00:02.053' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'89a2fdba-4871-4e65-b9c3-054b0ee19de0', N'LoginSuccess',                                      N'efb5796b-db7b-4c82-a0a7-ebe22fe35ec4', CAST(N'2016-05-24T16:22:55.400' AS DateTime), 5349, 283, CAST(N'2018-02-28T08:00:02.053' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'b52b2fb0-dc5b-46c8-a7d0-9e2c70e248b1', N'FailoverTasksAreAwaited',                           N'81aa8cc2-c0ea-4a29-8272-fb0379ddf6d1', CAST(N'2016-05-24T16:22:55.420' AS DateTime), 3106, 453, CAST(N'2018-02-28T07:59:52.027' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'495e2a1c-2fb0-46ed-89a2-3cf1390aed97', N'CacheWithDifferentLoginDisplaysActualErrorMessage', N'fd4c790b-72e1-4795-9d6a-db259e21150d', CAST(N'2016-05-24T16:22:55.427' AS DateTime), 32664, 454, CAST(N'2018-02-28T07:57:56.517' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'bf2ae7a2-ae61-4627-adcc-8176b44e05f0', N'LoginInvaldUsername',                               N'fd4c790b-72e1-4795-9d6a-db259e21150d', CAST(N'2016-05-24T16:22:55.430' AS DateTime), 7146, 450, CAST(N'2018-02-28T07:59:52.027' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'944861d4-c5a3-4181-97a6-8bf8443cdaf4', N'LoginInvalidPassword',                              N'fd4c790b-72e1-4795-9d6a-db259e21150d', CAST(N'2016-05-24T16:22:55.490' AS DateTime), 6144, 452, CAST(N'2018-02-28T07:59:52.027' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'6010f56f-40b4-4106-8ce6-b31cee1b4a12', N'LoginSucceed',                                      N'fd4c790b-72e1-4795-9d6a-db259e21150d', CAST(N'2016-05-24T16:22:55.493' AS DateTime), 12185, 452, CAST(N'2018-02-28T07:58:59.313' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'0a127b4b-d9a3-44f8-9948-8921b6d65f30', N'LoginSucceedCached',                                N'fd4c790b-72e1-4795-9d6a-db259e21150d', CAST(N'2016-05-24T16:22:55.543' AS DateTime), 23055, 453, CAST(N'2018-02-28T07:58:29.687' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'5573230c-85dd-45ea-bb19-f9b8e4b844de', N'CacheWithDifferentLoginDisplaysActualErrorMessage', N'd5360c17-fafd-4225-8bbb-f703164bcc9c', CAST(N'2016-05-24T16:22:55.550' AS DateTime), 28446, 453, CAST(N'2018-02-28T07:59:02.833' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'03601443-3b4e-4adc-98cc-e1949e65ec54', N'LoginInvaldUsername',                               N'd5360c17-fafd-4225-8bbb-f703164bcc9c', CAST(N'2016-05-24T16:22:55.550' AS DateTime), 4971, 452, CAST(N'2018-02-28T07:59:52.027' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'd7f98500-48d1-4c18-84f9-13f8e7f6380f', N'LoginInvalidPassword',                              N'd5360c17-fafd-4225-8bbb-f703164bcc9c', CAST(N'2016-05-24T16:22:55.553' AS DateTime), 4584, 453, CAST(N'2018-02-28T07:59:52.027' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'303debdf-2df2-4826-8147-fd50e1904b0f', N'LoginSucceed',                                      N'd5360c17-fafd-4225-8bbb-f703164bcc9c', CAST(N'2016-05-24T16:22:55.557' AS DateTime), 11032, 449, CAST(N'2018-02-28T07:58:57.797' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'77bd4d85-176a-4132-b71c-6101a7384722', N'LoginSucceedCached',                                N'd5360c17-fafd-4225-8bbb-f703164bcc9c', CAST(N'2016-05-24T16:22:55.560' AS DateTime), 19254, 454, CAST(N'2018-02-28T07:57:59.027' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'6f510702-ba74-4a24-bc27-c44507cdf0e3', N'CacheWithDifferentLoginDisplaysActualErrorMessage', N'633e8ae8-290c-4fd1-ba57-22717655de40', CAST(N'2016-05-24T16:22:55.563' AS DateTime), 21400, 454, CAST(N'2018-02-28T07:57:59.027' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'd1fbea0e-a27c-4ad9-9838-a2b41e0928ea', N'LoginInvaldUsername',                               N'633e8ae8-290c-4fd1-ba57-22717655de40', CAST(N'2016-05-24T16:22:55.563' AS DateTime), 2991, 453, CAST(N'2018-02-28T07:59:52.027' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'dfd708a5-e4fe-4a6e-88ae-b9649fb30456', N'LoginInvalidPassword',                              N'633e8ae8-290c-4fd1-ba57-22717655de40', CAST(N'2016-05-24T16:22:55.567' AS DateTime), 3698, 454, CAST(N'2018-02-28T07:59:52.027' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'a33a0023-ff6b-4301-af6d-2a6f35bf8246', N'LoginSucceed',                                      N'633e8ae8-290c-4fd1-ba57-22717655de40', CAST(N'2016-05-24T16:22:55.570' AS DateTime), 6538, 444, CAST(N'2018-02-28T07:58:49.770' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'5569ef26-d194-4261-9e18-be16f3ab113b', N'LoginSucceedCached',                                N'633e8ae8-290c-4fd1-ba57-22717655de40', CAST(N'2016-05-24T16:22:55.573' AS DateTime), 15045, 453, CAST(N'2018-02-28T07:59:03.873' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'84d6b0d5-265d-42fc-8f88-364f2e58744f', N'CacheWithDifferentLoginDisplaysActualErrorMessage', N'e7cd5b75-6071-441f-be18-7beb6c2a6439', CAST(N'2016-05-24T16:22:55.580' AS DateTime), 11184, 452, CAST(N'2018-02-28T07:58:51.277' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'55eada3c-91f8-488b-8cf6-8f410d0529fe', N'LoginInvaldUsername',                               N'e7cd5b75-6071-441f-be18-7beb6c2a6439', CAST(N'2016-05-24T16:22:55.617' AS DateTime), 3185, 454, CAST(N'2018-02-28T07:59:45.487' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'721bd124-9a12-4d80-a944-4a4856d989fe', N'LoginInvalidPassword',                              N'e7cd5b75-6071-441f-be18-7beb6c2a6439', CAST(N'2016-05-24T16:22:55.620' AS DateTime), 3558, 454, CAST(N'2018-02-28T07:59:45.487' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'a16c4448-1cfe-498d-a5bb-de090476a001', N'LoginSucceed',                                      N'e7cd5b75-6071-441f-be18-7beb6c2a6439', CAST(N'2016-05-24T16:22:55.623' AS DateTime), 5431, 450, CAST(N'2018-02-28T07:58:43.750' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'751b612f-b3e1-4a75-813d-3aec4de46831', N'LoginSucceedCached',                                N'e7cd5b75-6071-441f-be18-7beb6c2a6439', CAST(N'2016-05-24T16:22:55.727' AS DateTime), 10593, 452, CAST(N'2018-02-28T01:39:11.233' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'8aa78963-eaa2-44f3-91b9-5e43069c799a', N'InvalidPasswordToMainDomainUsingAt',                N'0e353d6b-d35f-45df-a4b2-00e86058580d', CAST(N'2016-05-24T16:22:55.730' AS DateTime), 3037, 454, CAST(N'2018-02-28T07:59:45.487' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'23de9bc3-0bd2-4617-a985-5200269b776b', N'LoginToAnotherDomainUsingAt',                       N'0e353d6b-d35f-45df-a4b2-00e86058580d', CAST(N'2016-05-24T16:22:55.733' AS DateTime), 5807, 388, CAST(N'2018-02-28T07:59:45.487' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'57172583-f24e-4457-bf36-3343f5572208', N'LoginToAnotherDomainUsingPrefix',                   N'0e353d6b-d35f-45df-a4b2-00e86058580d', CAST(N'2016-05-24T16:22:55.740' AS DateTime), 6650, 390, CAST(N'2018-02-28T07:59:45.487' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'b4a929bc-855f-405d-a706-a6fd78b43210', N'LoginToMainDomainUsingAt',                          N'0e353d6b-d35f-45df-a4b2-00e86058580d', CAST(N'2016-05-24T16:22:55.740' AS DateTime), 6415, 447, CAST(N'2018-02-28T07:58:38.737' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'9d973f15-e934-4d21-98c8-9026959e4817', N'LoginToMainDomainUsingPrefix',                      N'0e353d6b-d35f-45df-a4b2-00e86058580d', CAST(N'2016-05-24T16:22:55.743' AS DateTime), 6248, 452, CAST(N'2018-02-28T07:58:38.237' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'98e0c519-09e0-4452-865c-0dcf7bb50e52', N'LoginWithInvalidDomainAt',                          N'0e353d6b-d35f-45df-a4b2-00e86058580d', CAST(N'2016-05-24T16:22:55.743' AS DateTime), 5553, 455, CAST(N'2018-02-28T07:59:45.487' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'830bef6b-bc9a-4268-833c-5e4bb3732a67', N'GetClientIPAddressInvalidTimestamp',                N'1886fefb-f4cc-4a01-b24b-487578afe15c', CAST(N'2016-05-24T16:22:55.747' AS DateTime), 4477, 455, CAST(N'2018-02-28T07:59:45.487' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'ac54bd89-da47-476b-8ad7-016e860a4197', N'GetClientIPAddressSuccess',                         N'1886fefb-f4cc-4a01-b24b-487578afe15c', CAST(N'2016-05-24T16:22:55.750' AS DateTime), 3460, 454, CAST(N'2018-02-28T07:59:46.507' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'97a0a594-a388-4b39-8290-5d95f130438f', N'GetClientIPAddressTimestampTooOld',                 N'1886fefb-f4cc-4a01-b24b-487578afe15c', CAST(N'2016-05-24T16:22:55.753' AS DateTime), 2844, 454, CAST(N'2018-02-28T07:59:46.507' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'05f13230-adad-4f17-b06d-74efd9c1d40c', N'XFrameOptionsHeader',                               N'c424a8a7-85cc-4d37-9237-3e92ec7bcc7f', CAST(N'2016-06-21T10:17:32.197' AS DateTime), 2916, 454, CAST(N'2018-02-28T08:00:02.053' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'682ca5ad-d6df-4bb2-882b-75d2aa7d6b0d', N'SetDrainStop',                                      N'8d70a923-3a7b-4032-b19a-f3ba07df71c3', CAST(N'2017-05-30T16:02:07.000' AS DateTime), 11003, 260, CAST(N'2018-02-28T07:59:38.470' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'461d6241-8843-49ce-b861-9c52d21917e2', N'SetDrainStop',                                      N'efb5796b-db7b-4c82-a0a7-ebe22fe35ec4', CAST(N'2017-05-30T16:02:07.017' AS DateTime), 6927, 259, CAST(N'2018-02-28T08:00:02.053' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'20275d14-aa98-486f-8264-d3a63b4184b3', N'SetDrainStopAccessDeined',                          N'efb5796b-db7b-4c82-a0a7-ebe22fe35ec4', CAST(N'2017-05-30T16:02:07.030' AS DateTime), 6260, 249, CAST(N'2018-02-28T07:59:09.907' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'12f46d09-1a89-4d06-8d32-9a18e98fb379', N'SetDrainStopConfirmation',                          N'efb5796b-db7b-4c82-a0a7-ebe22fe35ec4', CAST(N'2017-05-30T16:02:07.033' AS DateTime), 6753, 283, CAST(N'2018-02-28T08:00:02.053' AS DateTime))
INSERT [dbo].[TestMethod] ([E6_PK], [E6_MethodName], [E6_E2], [E6_DateCreated], [E6_AverageDuration], [E6_DurationCount], [E6_LastDurationRecorded]) VALUES (N'd60d887e-4939-4b61-9fa4-c9a39769274e', N'UnSetDrainStop',                                    N'efb5796b-db7b-4c82-a0a7-ebe22fe35ec4', CAST(N'2017-05-30T16:02:07.037' AS DateTime), 5536, 260, CAST(N'2018-02-28T08:00:02.053' AS DateTime))
INSERT [dbo].[AmnestyFailures] ([AF_PK], [AF_E6], [AF_StartDate], [AF_ExpiryDate], [AF_IM]) VALUES (N'341d9ec6-c3b2-4dad-ba18-087a778ada85', N'303debdf-2df2-4826-8147-fd50e1904b0f', CAST(N'2017-09-23T18:34:59.433' AS DateTime), CAST(N'2017-10-10T09:46:07.420' AS DateTime), N'b6714d0a-5792-4368-af31-11e473755cc4')
INSERT [dbo].[AmnestyFailures] ([AF_PK], [AF_E6], [AF_StartDate], [AF_ExpiryDate], [AF_IM]) VALUES (N'9b0eedc9-a0d9-4a08-9547-0b786d225330', N'bf2ae7a2-ae61-4627-adcc-8176b44e05f0', CAST(N'2017-06-25T08:40:13.830' AS DateTime), CAST(N'2017-07-12T16:17:50.950' AS DateTime), N'6e68db5f-1b65-4efc-b472-ac55d97414d1')
INSERT [dbo].[AmnestyFailures] ([AF_PK], [AF_E6], [AF_StartDate], [AF_ExpiryDate], [AF_IM]) VALUES (N'098d4f2c-fc2b-459c-b254-0c5ff51bedb7', N'd60d887e-4939-4b61-9fa4-c9a39769274e', CAST(N'2017-08-24T14:26:38.723' AS DateTime), CAST(N'2017-09-06T15:23:08.037' AS DateTime), N'b75b3849-d1b4-40c3-9216-5058b9709790')
INSERT [dbo].[AmnestyFailures] ([AF_PK], [AF_E6], [AF_StartDate], [AF_ExpiryDate], [AF_IM]) VALUES (N'5ed2c782-7e77-40dd-a012-1f03a72fcc0e', N'98fef35c-9061-4cce-9d84-6c5c3eca7c49', CAST(N'2017-12-11T10:00:03.040' AS DateTime), CAST(N'2017-12-11T16:10:04.193' AS DateTime), N'e8521047-0659-44e7-b498-4a8b998f6e45')
INSERT [dbo].[AmnestyFailures] ([AF_PK], [AF_E6], [AF_StartDate], [AF_ExpiryDate], [AF_IM]) VALUES (N'c8e5bc1a-10ff-4b39-883f-208dec466857', N'23de9bc3-0bd2-4617-a985-5200269b776b', CAST(N'2016-07-03T23:28:41.250' AS DateTime), CAST(N'2017-03-22T20:09:28.580' AS DateTime), N'b1af01df-a2a6-4b89-ab89-9a4c368d604f')
INSERT [dbo].[AmnestyFailures] ([AF_PK], [AF_E6], [AF_StartDate], [AF_ExpiryDate], [AF_IM]) VALUES (N'eb9c2d7f-2d63-496e-b4a9-27fb3d7ad635', N'84d6b0d5-265d-42fc-8f88-364f2e58744f', CAST(N'2018-02-27T15:35:46.107' AS DateTime), NULL,                                         N'0cc39412-ca4e-48fd-8bf5-725d9b145954')
INSERT [dbo].[AmnestyFailures] ([AF_PK], [AF_E6], [AF_StartDate], [AF_ExpiryDate], [AF_IM]) VALUES (N'53f6a429-3b96-438b-b9f1-3b397ce7decd', N'461d6241-8843-49ce-b861-9c52d21917e2', CAST(N'2017-08-24T14:26:23.940' AS DateTime), CAST(N'2017-09-06T15:23:08.037' AS DateTime), N'b75b3849-d1b4-40c3-9216-5058b9709790')
INSERT [dbo].[AmnestyFailures] ([AF_PK], [AF_E6], [AF_StartDate], [AF_ExpiryDate], [AF_IM]) VALUES (N'9d75fc40-2178-4348-af9a-46822860ec00', N'9d973f15-e934-4d21-98c8-9026959e4817', CAST(N'2018-02-23T11:24:45.500' AS DateTime), NULL,                                         N'0cc39412-ca4e-48fd-8bf5-725d9b145954')
INSERT [dbo].[AmnestyFailures] ([AF_PK], [AF_E6], [AF_StartDate], [AF_ExpiryDate], [AF_IM]) VALUES (N'61e32e24-4bdd-45d1-889c-4c26b3b4fddf', N'a33a0023-ff6b-4301-af6d-2a6f35bf8246', CAST(N'2018-02-22T13:27:28.427' AS DateTime), NULL,                                         NULL)
INSERT [dbo].[AmnestyFailures] ([AF_PK], [AF_E6], [AF_StartDate], [AF_ExpiryDate], [AF_IM]) VALUES (N'728825d2-51e6-4333-a1fe-52ce6503cc36', N'8896e487-27d4-451b-8b87-285f02d5ebb6', CAST(N'2018-02-17T19:22:21.853' AS DateTime), NULL,                                         N'c781ba86-a21c-48bd-9e05-9086a0d0d050')
INSERT [dbo].[AmnestyFailures] ([AF_PK], [AF_E6], [AF_StartDate], [AF_ExpiryDate], [AF_IM]) VALUES (N'9c28afdf-449d-44b1-b346-5866135ab13b', N'b4a929bc-855f-405d-a706-a6fd78b43210', CAST(N'2018-02-22T13:27:37.957' AS DateTime), NULL,                                         NULL)
INSERT [dbo].[AmnestyFailures] ([AF_PK], [AF_E6], [AF_StartDate], [AF_ExpiryDate], [AF_IM]) VALUES (N'cad0cc33-96ee-42b3-adb3-591a47d1d032', N'0531a351-0882-42ba-9c72-816f7a8e39a4', CAST(N'2018-02-23T14:27:11.203' AS DateTime), NULL,                                         N'c781ba86-a21c-48bd-9e05-9086a0d0d050')
INSERT [dbo].[AmnestyFailures] ([AF_PK], [AF_E6], [AF_StartDate], [AF_ExpiryDate], [AF_IM]) VALUES (N'0ca188f5-da2e-478b-a3f2-5a4507e2def5', N'84d6b0d5-265d-42fc-8f88-364f2e58744f', CAST(N'2018-02-27T15:34:46.993' AS DateTime), CAST(N'2018-02-27T15:35:32.370' AS DateTime), N'e6bca524-cd0c-4a1c-935f-d85dbdd331bb')
INSERT [dbo].[AmnestyFailures] ([AF_PK], [AF_E6], [AF_StartDate], [AF_ExpiryDate], [AF_IM]) VALUES (N'21d6c586-974a-43a4-97a1-6ccba6e72fca', N'98fef35c-9061-4cce-9d84-6c5c3eca7c49', CAST(N'2018-02-17T19:22:21.570' AS DateTime), NULL,                                         N'e0d559a4-f3f4-431f-aa15-4d3a93271ffc')
INSERT [dbo].[AmnestyFailures] ([AF_PK], [AF_E6], [AF_StartDate], [AF_ExpiryDate], [AF_IM]) VALUES (N'c0b500a4-077a-4ac0-b172-8718053086c6', N'303debdf-2df2-4826-8147-fd50e1904b0f', CAST(N'2018-02-27T15:34:42.867' AS DateTime), NULL,                                         N'c781ba86-a21c-48bd-9e05-9086a0d0d050')
INSERT [dbo].[AmnestyFailures] ([AF_PK], [AF_E6], [AF_StartDate], [AF_ExpiryDate], [AF_IM]) VALUES (N'795dffef-49ab-464b-9540-8c7c803f3326', N'bf2ae7a2-ae61-4627-adcc-8176b44e05f0', CAST(N'2017-03-12T11:06:19.443' AS DateTime), CAST(N'2017-03-22T20:09:28.580' AS DateTime), N'b1af01df-a2a6-4b89-ab89-9a4c368d604f')
INSERT [dbo].[AmnestyFailures] ([AF_PK], [AF_E6], [AF_StartDate], [AF_ExpiryDate], [AF_IM]) VALUES (N'c2b4c46c-9dfd-48cd-b2a7-96ca1d69ad74', N'20275d14-aa98-486f-8264-d3a63b4184b3', CAST(N'2017-08-24T14:26:32.160' AS DateTime), CAST(N'2017-09-06T15:23:08.037' AS DateTime), N'b75b3849-d1b4-40c3-9216-5058b9709790')
INSERT [dbo].[AmnestyFailures] ([AF_PK], [AF_E6], [AF_StartDate], [AF_ExpiryDate], [AF_IM]) VALUES (N'a4b0aa4d-e831-4d9e-91e3-b0d67f70cb40', N'4f01bac1-3442-4ece-adc3-390b716dbc21', CAST(N'2016-06-15T06:17:58.160' AS DateTime), CAST(N'2016-06-15T09:37:30.743' AS DateTime), NULL)
INSERT [dbo].[AmnestyFailures] ([AF_PK], [AF_E6], [AF_StartDate], [AF_ExpiryDate], [AF_IM]) VALUES (N'41118d77-4f24-4a2f-8153-b71b97d39308', N'a1571182-8a83-4ab4-9bc3-2a40e43be2db', CAST(N'2016-07-03T23:28:41.590' AS DateTime), CAST(N'2017-03-22T20:09:28.580' AS DateTime), N'b1af01df-a2a6-4b89-ab89-9a4c368d604f')
INSERT [dbo].[AmnestyFailures] ([AF_PK], [AF_E6], [AF_StartDate], [AF_ExpiryDate], [AF_IM]) VALUES (N'a97662d0-b723-43e4-a689-ba034ffedf45', N'5c9a249b-e219-4a29-bddf-1531abf1787f', CAST(N'2018-02-22T13:27:50.833' AS DateTime), NULL,                                         N'c781ba86-a21c-48bd-9e05-9086a0d0d050')
INSERT [dbo].[AmnestyFailures] ([AF_PK], [AF_E6], [AF_StartDate], [AF_ExpiryDate], [AF_IM]) VALUES (N'2560470c-cf21-4e10-a5a2-c4d5e778f71e', N'57172583-f24e-4457-bf36-3343f5572208', CAST(N'2016-07-03T23:28:41.427' AS DateTime), CAST(N'2017-03-22T20:09:28.580' AS DateTime), N'b1af01df-a2a6-4b89-ab89-9a4c368d604f')
INSERT [dbo].[AmnestyFailures] ([AF_PK], [AF_E6], [AF_StartDate], [AF_ExpiryDate], [AF_IM]) VALUES (N'34b6171a-8bd1-472d-849b-c9b5ebbcf549', N'6010f56f-40b4-4106-8ce6-b31cee1b4a12', CAST(N'2018-02-27T15:34:25.117' AS DateTime), NULL,                                         N'c781ba86-a21c-48bd-9e05-9086a0d0d050')
INSERT [dbo].[AmnestyFailures] ([AF_PK], [AF_E6], [AF_StartDate], [AF_ExpiryDate], [AF_IM]) VALUES (N'411d2782-9fd6-443b-ab4c-c9f3f4d88868', N'a16c4448-1cfe-498d-a5bb-de090476a001', CAST(N'2018-02-23T11:24:27.860' AS DateTime), NULL,                                         NULL)
INSERT [dbo].[AmnestyFailures] ([AF_PK], [AF_E6], [AF_StartDate], [AF_ExpiryDate], [AF_IM]) VALUES (N'6912f006-b902-4ea9-a1a8-cfc80bcb2f86', N'682ca5ad-d6df-4bb2-882b-75d2aa7d6b0d', CAST(N'2017-08-21T02:37:00.637' AS DateTime), CAST(N'2017-09-06T15:23:08.037' AS DateTime), N'b75b3849-d1b4-40c3-9216-5058b9709790')
INSERT [dbo].[AmnestyFailures] ([AF_PK], [AF_E6], [AF_StartDate], [AF_ExpiryDate], [AF_IM]) VALUES (N'10c419f8-9aab-4942-acc6-d2afba85d66d', N'4f01bac1-3442-4ece-adc3-390b716dbc21', CAST(N'2018-02-17T19:22:21.260' AS DateTime), NULL,                                         NULL)
INSERT [dbo].[AmnestyFailures] ([AF_PK], [AF_E6], [AF_StartDate], [AF_ExpiryDate], [AF_IM]) VALUES (N'ed6e424d-a151-4c18-aa67-dc9edefc7722', N'4f01bac1-3442-4ece-adc3-390b716dbc21', CAST(N'2017-12-09T12:21:47.070' AS DateTime), CAST(N'2017-12-11T16:10:04.193' AS DateTime), N'e8521047-0659-44e7-b498-4a8b998f6e45')
INSERT [dbo].[AmnestyFailures] ([AF_PK], [AF_E6], [AF_StartDate], [AF_ExpiryDate], [AF_IM]) VALUES (N'18fe9fcd-3223-4c97-9f4b-e98064b903e6', N'20275d14-aa98-486f-8264-d3a63b4184b3', CAST(N'2018-02-17T19:22:20.853' AS DateTime), NULL,                                         N'c781ba86-a21c-48bd-9e05-9086a0d0d050')
INSERT [dbo].[AmnestyFailures] ([AF_PK], [AF_E6], [AF_StartDate], [AF_ExpiryDate], [AF_IM]) VALUES (N'd0ad159d-4450-42b5-acb1-f643f3fee07d', N'1a5ad68d-6336-4f77-b5ff-42be28d9310c', CAST(N'2016-07-03T23:28:41.097' AS DateTime), CAST(N'2017-03-22T20:09:28.580' AS DateTime), N'b1af01df-a2a6-4b89-ab89-9a4c368d604f')
INSERT [dbo].[SourceTreeResponsibility] ([ST_Path], [ST_User_Responsible], [ST_Product], [ST_Product_Area], [ST_Module], [ST_Amnesty_WI_Grouping]) VALUES (N'$/WiseCloud',                       NULL,                     N'ENT', N'PER', N'APP', N'DLL')
INSERT [dbo].[SourceTreeResponsibility] ([ST_Path], [ST_User_Responsible], [ST_Product], [ST_Product_Area], [ST_Module], [ST_Amnesty_WI_Grouping]) VALUES (N'$/WiseCloud/WiseCloudAccessor',     N'CORP\Bret.Ehlert', N'ENT', N'PER', N'APP', N'DLL')
INSERT [dbo].[SourceTreeResponsibility] ([ST_Path], [ST_User_Responsible], [ST_Product], [ST_Product_Area], [ST_Module], [ST_Amnesty_WI_Grouping]) VALUES (N'$/WiseCloud/WiseCloudOrchestrator', N'CORP\ir.malin',    N'ENT', N'PER', N'APP', N'DLL')
			";
			var workItems = new List<OdysseyWorkItem> {
				new OdysseyWorkItem(new Guid("E0D559A4-F3F4-431F-AA15-4D3A93271FFC"), 1, status: "CLS"),
				new OdysseyWorkItem(new Guid("B1AF01DF-A2A6-4B89-AB89-9A4C368D604F"), 2, status: "CLS"),
				new OdysseyWorkItem(new Guid("0CC39412-CA4E-48FD-8BF5-725D9B145954"), 100, status: ProcessTaskStatusCodeList.Codes.Open),
				new OdysseyWorkItem(new Guid("C781BA86-A21C-48BD-9E05-9086A0D0D050"), 101, status: ProcessTaskStatusCodeList.Codes.Working),
				new OdysseyWorkItem(new Guid("B6714D0A-5792-4368-AF31-11E473755CC4"), 10, status: "CAN"),
				new OdysseyWorkItem(new Guid("E8521047-0659-44E7-B498-4A8B998F6E45"), 11, status: "CAN"),
				new OdysseyWorkItem(new Guid("B75B3849-D1B4-40C3-9216-5058B9709790"), 12, status: "CAN"),
				new OdysseyWorkItem(new Guid("6E68DB5F-1B65-4EFC-B472-AC55D97414D1"), 13, status: "CAN"),
				new OdysseyWorkItem(new Guid("E6BCA524-CD0C-4A1C-935F-D85DBDD331BB"), 14, status: "CAN")
				};
			DbCrikey.ExecuteNonQuery(sql);
			TestConnection.ExecuteNonQuery(InsertStatement(workItems));

			var serviceTask = new AmnestiesProcessorServiceTask();

			var downloaded = serviceTask.Download();
			var filtered = serviceTask.FilterAndTranformWorkItems(downloaded);

			var amenstiesWithoutWorkItems = new List<Guid>() { Guid.Parse("61e32e24-4bdd-45d1-889c-4c26b3b4fddf"), Guid.Parse("9c28afdf-449d-44b1-b346-5866135ab13b"), Guid.Parse("411d2782-9fd6-443b-ab4c-c9f3f4d88868"), Guid.Parse("10c419f8-9aab-4942-acc6-d2afba85d66d") };
			AssertContainsExactElementsInAnyOrder(amenstiesWithoutWorkItems, filtered.Select(af => af.AF_PK));

			serviceTask.Process(filtered, TimeSpan.FromMinutes(1), CancellationToken.None);
			var amnesties = new List<DatAmnestyFailure>();
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			using (var cmd = connection.Command($@"
SELECT [AF_PK], [AF_E6], [AF_StartDate], [AF_ExpiryDate], [AF_IM]
FROM [dbo].[AmnestyFailures]
WHERE [AF_PK] IN ({AmnestiesProcessorServiceTask.JoinGuidsToSafeSqlInClause(filtered, failure => failure.AF_PK)})"))
			{
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						amnesties.Add(new DatAmnestyFailure
						{
							AF_PK = reader.GetGuid(0),
							AF_IM = reader.GetGuid(4)
						});
					}
				}
			}

			foreach (var amnesty in amnesties)
			{
				AssertEquals(Guid.Parse("0CC39412-CA4E-48FD-8BF5-725D9B145954"), amnesty.AF_IM);
			}
		}

		public void TestDownloadAndFilterMultipleExistingMatchingAmnesties()
		{
			var serviceTask = new AmnestiesProcessorServiceTask();
			var guids = SetupMultipleExistingMatchingAmnesties(serviceTask, false, 1, "DLL", "CLS", "CLS");
			var firstDecoyWI_PK = guids.workitemPKs[0];
			var secondDecoyWI_PK = guids.workitemPKs[1];
			var targetPK = guids.amnestyPK;

			var downloaded = serviceTask.Download();
			AssertEquals("Expect two potential to process", 2, downloaded.Count());
			AssertEquals("Expect first decoy", 1, downloaded.Count(a => a.EDI_IM == firstDecoyWI_PK));
			AssertEquals("Expect second decoy", 1, downloaded.Count(a => a.EDI_IM == secondDecoyWI_PK));

			AssertEquals("Expect pk of unprocessed amnesty", targetPK, downloaded.First().AF_PK);
			AssertEquals("Expect pk of unprocessed amnesty", targetPK, downloaded.Last().AF_PK);

			var amnestyWithWorkItems = serviceTask.FilterAndTranformWorkItems(downloaded);
			AssertEquals("Expect one filtered amnesty", 1, amnestyWithWorkItems.Count());
			var unprocessedAmnestyFailure = amnestyWithWorkItems.First();
			AssertEquals("Expect pk of filtered amnesty", targetPK, unprocessedAmnestyFailure.AF_PK);
			AssertNull(unprocessedAmnestyFailure.EDI_IM);
			AssertDatAmnestyFailureNoMatchingWorkItem(unprocessedAmnestyFailure);
		}

		public void TestIgnoreClosedAmnesties()
		{
			var serviceTask = new AmnestiesProcessorServiceTask();
			var guids = SetupMultipleExistingMatchingAmnesties(serviceTask, true, 1, "DLL", ProcessTaskStatusCodeList.Codes.Assigned, ProcessTaskStatusCodeList.Codes.Cancelled, ProcessTaskStatusCodeList.Codes.Closed, ProcessTaskStatusCodeList.Codes.Open, ProcessTaskStatusCodeList.Codes.Suspended, ProcessTaskStatusCodeList.Codes.Working);

			var downloaded = serviceTask.Download();
			AssertEquals("Expect potential to process", 1, downloaded.Count());
			var amenstyFailure = downloaded.First();

			AssertEquals("Expect pk of unprocessed amnesty", guids.amnestyPK, amenstyFailure.AF_PK);
			AssertNull("Expect no matching WI's as all amnesties are closed", amenstyFailure.EDI_IM);
		}

		public void TestOnlyOpenAndAssignAmnestiesShouldMatch()
		{
			var serviceTask = new AmnestiesProcessorServiceTask();
			var guids = SetupMultipleExistingMatchingAmnesties(serviceTask, false, 1, "DLL", ProcessTaskStatusCodeList.Codes.Cancelled, ProcessTaskStatusCodeList.Codes.Closed, ProcessTaskStatusCodeList.Codes.Suspended, ProcessTaskStatusCodeList.Codes.Working);

			var downloaded = serviceTask.Download();
			AssertEquals("Expect potential to process", 4, downloaded.Count());
			Assert("Expect pk of unprocessed amnesty", downloaded.All(af => af.AF_PK == guids.amnestyPK));

			AssertContainsExactElementsInAnyOrder("All work items should be flagged", guids.workitemPKs, downloaded.Select(af => af.EDI_IM));

			var amnestyWithMatchingWorkitem = serviceTask.FilterAndTranformWorkItems(downloaded).Single();
			AssertNull("None are open or assigned so EDI_IM should be null", amnestyWithMatchingWorkitem.EDI_IM);

			TestConnection.ExecuteNonQuery($"update dbo.WorkItem set WKI_Status = '{ProcessTaskStatusCodeList.Codes.Open}' where WKI_PK = '{guids.workitemPKs[0]}'");
			amnestyWithMatchingWorkitem = serviceTask.FilterAndTranformWorkItems(downloaded).Single();
			AssertEquals("One WI is Open so this should match", guids.workitemPKs[0], amnestyWithMatchingWorkitem.EDI_IM);

			TestConnection.ExecuteNonQuery($"update dbo.WorkItem set WKI_Status = '{ProcessTaskStatusCodeList.Codes.Assigned}' where WKI_PK = '{guids.workitemPKs[0]}'");
			amnestyWithMatchingWorkitem = serviceTask.FilterAndTranformWorkItems(downloaded).Single();
			AssertEquals("One WI is Assigned so this should match", guids.workitemPKs[0], amnestyWithMatchingWorkitem.EDI_IM);

			TestConnection.ExecuteNonQuery($"update dbo.WorkItem set WKI_Status = '{ProcessTaskStatusCodeList.Codes.Closed}' where WKI_PK = '{guids.workitemPKs[0]}'");
			amnestyWithMatchingWorkitem = serviceTask.FilterAndTranformWorkItems(downloaded).Single();
			AssertNull("None are open or assigned so EDI_IM should be null", amnestyWithMatchingWorkitem.EDI_IM);
		}

		public void TestOnlyAssignedWithMultipleSets()
		{
			var serviceTask = new AmnestiesProcessorServiceTask();
			var guidsOpen = SetupMultipleExistingMatchingAmnesties(serviceTask, false, 1, "DLL", ProcessTaskStatusCodeList.Codes.Open, ProcessTaskStatusCodeList.Codes.Closed, ProcessTaskStatusCodeList.Codes.Suspended, ProcessTaskStatusCodeList.Codes.Working);
			var guidsAssigned = SetupMultipleExistingMatchingAmnesties(serviceTask, false, 2, "DLL", ProcessTaskStatusCodeList.Codes.Cancelled, ProcessTaskStatusCodeList.Codes.Assigned, ProcessTaskStatusCodeList.Codes.Suspended, ProcessTaskStatusCodeList.Codes.Working);
			var guidsNone = SetupMultipleExistingMatchingAmnesties(serviceTask, false, 3, "DLL", ProcessTaskStatusCodeList.Codes.Cancelled, ProcessTaskStatusCodeList.Codes.Closed, ProcessTaskStatusCodeList.Codes.Suspended, ProcessTaskStatusCodeList.Codes.Working);

			var downloaded = serviceTask.Download();
			AssertEquals("Expect potential to process 3 amnesties x 4 WI's", 12, downloaded.Count());
			Assert("Expect pk of unprocessed amnesty", downloaded.All(af => af.AF_PK == guidsOpen.amnestyPK || af.AF_PK == guidsAssigned.amnestyPK || af.AF_PK == guidsNone.amnestyPK));

			AssertContainsExactElementsInAnyOrder("All work items / amnesty combinations should be included", guidsOpen.workitemPKs, downloaded.Where(af => af.AF_PK == guidsOpen.amnestyPK).Select(af => af.EDI_IM));
			AssertContainsExactElementsInAnyOrder("All work items / amnesty combinations should be included", guidsAssigned.workitemPKs, downloaded.Where(af => af.AF_PK == guidsAssigned.amnestyPK).Select(af => af.EDI_IM));
			AssertContainsExactElementsInAnyOrder("All work items / amnesty combinations should be included", guidsNone.workitemPKs, downloaded.Where(af => af.AF_PK == guidsNone.amnestyPK).Select(af => af.EDI_IM));

			var amnestyWithMatchingWorkitem = serviceTask.FilterAndTranformWorkItems(downloaded);
			AssertEquals("Expect 3 failures", 3, amnestyWithMatchingWorkitem.Count());

			AssertEquals("One WI is Open so this should match", guidsOpen.workitemPKs[0], amnestyWithMatchingWorkitem.Single(af => af.AF_PK == guidsOpen.amnestyPK).EDI_IM);
			AssertEquals("One WI is Assigned so this should match", guidsAssigned.workitemPKs[1], amnestyWithMatchingWorkitem.Single(af => af.AF_PK == guidsAssigned.amnestyPK).EDI_IM);
			AssertNull("None are open or assigned so EDI_IM should be null", amnestyWithMatchingWorkitem.Single(af => af.AF_PK == guidsNone.amnestyPK).EDI_IM);
		}

		public void TestGroupingsWithMultipleSets()
		{
			var serviceTask = new AmnestiesProcessorServiceTask();
			var guidsClass = SetupMultipleExistingMatchingAmnesties(serviceTask, false, 1, "CLS", ProcessTaskStatusCodeList.Codes.Open, ProcessTaskStatusCodeList.Codes.Assigned, ProcessTaskStatusCodeList.Codes.Suspended, ProcessTaskStatusCodeList.Codes.Closed);
			var guidsDll = SetupMultipleExistingMatchingAmnesties(serviceTask, false, 2, "DLL", ProcessTaskStatusCodeList.Codes.Assigned, ProcessTaskStatusCodeList.Codes.Open, ProcessTaskStatusCodeList.Codes.Suspended, ProcessTaskStatusCodeList.Codes.Closed);
			var guidsSourceTree = SetupMultipleExistingMatchingAmnesties(serviceTask, false, 3, "STN", ProcessTaskStatusCodeList.Codes.Open, ProcessTaskStatusCodeList.Codes.Assigned, ProcessTaskStatusCodeList.Codes.Suspended, ProcessTaskStatusCodeList.Codes.Closed);

			//close firsrt amnesty - should be ignored and matched WI should be second in array
			DbCrikey.ExecuteNonQuery($"update AmnestyFailures set AF_ExpiryDate = getdate() where AF_IM in ('{guidsClass.workitemPKs[0]}','{guidsDll.workitemPKs[0]}','{guidsSourceTree.workitemPKs[0]}') ");

			var downloaded = serviceTask.Download();
			AssertEquals("Expect potential to process 3 amnesties x 3 WI's (one is closed)", 9, downloaded.Count());

			AssertCollectionNotContains("we closed this amnesty so no match", guidsDll.workitemPKs[0], downloaded.Select(af => af.EDI_IM));
			AssertCollectionContains("open amnesty so possible match", guidsDll.workitemPKs[1], downloaded.Select(af => af.EDI_IM));
			AssertCollectionContains("open amnesty so possible match", guidsDll.workitemPKs[2], downloaded.Select(af => af.EDI_IM));

			var amnestyWithMatchingWorkitem = serviceTask.FilterAndTranformWorkItems(downloaded);
			AssertEquals("Expect 3 failures", 3, amnestyWithMatchingWorkitem.Count());

			AssertEquals("One WI is Assigned so this should match", guidsClass.workitemPKs[1], amnestyWithMatchingWorkitem.Single(af => af.AF_PK == guidsClass.amnestyPK).EDI_IM);
			AssertEquals("One WI is Open so this should match", guidsDll.workitemPKs[1], amnestyWithMatchingWorkitem.Single(af => af.AF_PK == guidsDll.amnestyPK).EDI_IM);
			AssertEquals("One WI is Assigned so this should match", guidsSourceTree.workitemPKs[1], amnestyWithMatchingWorkitem.Single(af => af.AF_PK == guidsSourceTree.amnestyPK).EDI_IM);
		}

		#region Setup

		DbConnection DbCrikey;
		IDisposable disposables;

		(Guid amnestyPK, Guid[] workitemPKs) SetupMultipleExistingMatchingAmnesties(AmnestiesProcessorServiceTask serviceTask, bool expired, int uniqueNumber, string grouping, params string[] statuses)
		{
			var datState = GenerateDatState(uniqueNumber);
			var amnestyPK = datState.AmnestyFailures.First().AF_PK;
			datState.Responsibilities.First().ST_Amnesty_WI_Grouping = grouping;
			datState.AmnestyFailures.First().AF_ExpiryDate = null;

			var workitemPKs = new List<Guid>();
			for (var i = 0; i < statuses.Length; i++)
			{
				var workitem = new OdysseyWorkItem(Guid.NewGuid(), uniqueNumber * 10 + i, statuses[i]);
				datState.WorkItems.Add(workitem);
				var failure = new AmnestyFailures(Guid.NewGuid(), expiry: expired ? "2018-03-01 10:44:00" : null);
				failure.AF_IM = workitem.WKI_PK;
				var method = new TestMethod(Guid.NewGuid(), "TestMethodX" + i);
				method.AmnestyFailures.Add(failure);
				datState.Classes.First().Methods.Add(method);
				workitemPKs.Add(workitem.WKI_PK);
			}

			InsertWrapperIntoDB(datState);
			return (amnestyPK, workitemPKs.ToArray());
		}

		protected override void SetUp()
		{
			base.SetUp();

			DbCrikey = DbConnectionCrikey.GetAutoTesterUserTestsConnection();
			disposables = new DisposableList(new IDisposable[] { DbCrikey });
		}

		protected override void TearDown()
		{
			disposables.Dispose();
			base.TearDown();
		}

		void AssertDatAmnestyFailureEqualsWorkItem(OdysseyWorkItem wi, DatAmnestyFailure amnestyFailure)
		{
			AssertEquals(nameof(wi.WKI_PK), wi.WKI_PK, amnestyFailure.EDI_IM);
		}

		void AssertDatAmnestyFailureNoMatchingWorkItem(DatAmnestyFailure amnestyFailure)
		{
			AssertNull(amnestyFailure.EDI_IM);
		}

		static DateTime ParseSQLDT(string dateTime)
		{
			return DateTime.ParseExact(dateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
		}

		DatSystemState GenerateDatState(int uniqueNumber = 1)
		{
			var failures = new List<AmnestyFailures> { new AmnestyFailures(Guid.NewGuid()) };
			var methods = new List<TestMethod> { new TestMethod(Guid.NewGuid(), methodName: "TestMethod" + uniqueNumber, amnestyFailures: failures) };
			var classes = new List<TestClass> { new TestClass(Guid.NewGuid(), testClass: "TestClass" + uniqueNumber, methods: methods) };
			var dlls = new List<Assembly> { new Assembly(Guid.NewGuid(), dllName: "Test.dll.Name" + uniqueNumber, sourcePath: "$/path" + uniqueNumber + "/dll", classes: classes) };
			var resp = new List<SourceTreeResponsibility> { new SourceTreeResponsibility(path: "$/path" + uniqueNumber) };
			var workItems = new List<OdysseyWorkItem> { new OdysseyWorkItem(Guid.NewGuid(), uniqueNumber) };
			var users = new List<User> { new User(Guid.NewGuid(), @"CHEF\Dat.Boi" + uniqueNumber) };
			return new DatSystemState(resp, dlls, workItems, users);
		}

		string InsertStatement<T>(IEnumerable<T> objects)
		{
			Assert(objects.Any());
			var type = objects.First().GetType();
			var tableName = type.Name;

			var columns = type.GetProperties().Select(p => p.Name).Where(c => c.Contains("_")).ToArray();

			var builder = new StringBuilder("insert into [dbo].[");
			builder.Append(tableName);
			builder.Append("] ([");
			if (tableName == "OdysseyWorkItem")
			{
				builder = new StringBuilder("insert into [dbo].[WorkItem] ([");
			}
			builder.Append(columns.First());
			builder.Append("]");
			foreach (var c in columns.Skip(1))
			{
				builder.Append(",[");
				builder.Append(c);
				builder.Append("]");
			}
			builder.Append(") values ");
			foreach (var o in objects)
			{
				builder.Append("(");
				var values = columns.Select(c => o.GetType().GetProperty(c).GetValue(o));
				foreach (var v in values)
				{
					if (v == null)
					{
						builder.Append("NULL,");
						continue;
					}
					builder.Append("'");
					if (v.GetType() == typeof(DateTime))
					{
						builder.Append(((DateTime)v).ToString("yyyy-MM-dd HH:mm:ss"));
					}
					else
					{
						builder.Append(v.ToString());
					}
					builder.Append("',");
				}
				builder.Remove(builder.Length - 1, 1);
				builder.Append("),");
			}
			builder.Remove(builder.Length - 1, 1);
			builder.Append(";");
			return builder.ToString();
		}

		void InsertWrapperIntoDB(DatSystemState wrapper)
		{
			wrapper.Build();
			DbCrikey.ExecuteNonQuery(@"
IF COL_LENGTH('SourceTreeResponsibility','ST_Amnesty_WI_Grouping') IS NULL
BEGIN
	ALTER TABLE SourceTreeResponsibility
ADD [ST_Amnesty_WI_Grouping] [char](3) NOT NULL DEFAULT 'DLL',
CONSTRAINT IX_ST_Amnesty_WI_Grouping CHECK (ST_Amnesty_WI_Grouping IN ('STN', 'DLL', 'CLS', 'MET'))
END
");
			DbCrikey.ExecuteNonQuery(InsertStatement(wrapper.Users));
			DbCrikey.ExecuteNonQuery(InsertStatement(wrapper.Assemblies));
			DbCrikey.ExecuteNonQuery(InsertStatement(wrapper.Responsibilities));
			DbCrikey.ExecuteNonQuery(InsertStatement(wrapper.Classes));
			DbCrikey.ExecuteNonQuery(InsertStatement(wrapper.Methods));
			DbCrikey.ExecuteNonQuery(InsertStatement(wrapper.AmnestyFailures));
			TestConnection.ExecuteNonQuery(InsertStatement(wrapper.WorkItems));
		}

		#region Models

		class DatSystemState
		{
			public List<SourceTreeResponsibility> Responsibilities;
			public List<Assembly> Assemblies;
			public List<OdysseyWorkItem> WorkItems;
			public List<User> Users;
			public IEnumerable<TestClass> Classes
			{
				get
				{
					return Assemblies.SelectMany(a => a.Classes);
				}
			}
			public IEnumerable<TestMethod> Methods
			{
				get
				{
					return Classes.SelectMany(c => c.Methods);
				}
			}
			public IEnumerable<AmnestyFailures> AmnestyFailures
			{
				get
				{
					return Methods.SelectMany(m => m.AmnestyFailures);
				}
			}
			public DatSystemState(List<SourceTreeResponsibility> resp,
				List<Assembly> dlls,
				List<OdysseyWorkItem> wIs,
				List<User> users)
			{
				Responsibilities = resp;
				Assemblies = dlls;
				WorkItems = wIs;
				Users = users;
			}

			public void Build()
			{
				foreach (var a in Assemblies)
				{
					foreach (var c in a.Classes)
					{
						AssertEquals("Expect class ->- assembly relationship on class to be set exactly once.", Guid.Empty, c.E2_E8);
						c.E2_E8 = a.E8_PK;
						foreach (var m in c.Methods)
						{
							AssertEquals("Expect method ->- class relationship on method to be set exactly once.", Guid.Empty, m.E6_E2);
							m.E6_E2 = c.E2_PK;
							foreach (var f in m.AmnestyFailures)
							{
								AssertEquals("Expect amnesty-failure ->- method relationship on amnesty-failure to be set exactly once.", Guid.Empty, f.AF_E6);
								f.AF_E6 = m.E6_PK;
							}
						}
					}
				}
			}
		}

		class User
		{
			public Guid U1_PK { get; set; }
			public string U1_Name { get; set; }
			public User(Guid pk, string name)
			{
				U1_PK = pk;
				U1_Name = name;
			}
		}

		class OdysseyWorkItem
		{
			public Guid WKI_PK { get; set; }
			public string WKI_WorkItemNumber { get; set; }
			public string WKI_Status { get; set; }
			public DateTime WKI_SystemCreateTimeUtc { get; set; }
			public DateTime WKI_SystemLastEditTimeUtc { get; set; }
			public string WKI_SystemCreateUser { get; set; }
			public string WKI_SystemLastEditUser { get; set; }
			public OdysseyWorkItem(Guid pk,
				int number,
				string status = "OPN",
				string time = "2017-08-04 01:44:00",
				string user = "E")
			{
				WKI_PK = pk;
				Assert("Maximum wi number is WI90009999", number < 9999);
				Assert("Minimum wi number is WI90000001", number > 0);
				WKI_Status = status;
				WKI_WorkItemNumber = string.Format("WI9{0:D7}", number);
				var dateTime = ParseSQLDT(time);
				WKI_SystemCreateTimeUtc = dateTime;
				WKI_SystemLastEditTimeUtc = dateTime;
				WKI_SystemCreateUser = user;
				WKI_SystemLastEditUser = user;
			}
		}

		class SourceTreeResponsibility
		{
			public string ST_Path { get; set; }
			public string ST_Amnesty_WI_Grouping { get; set; }
			public SourceTreeResponsibility(string path = "$", string grouping = "DLL")
			{
				ST_Path = path;
				ST_Amnesty_WI_Grouping = grouping;
			}
		}

		class Assembly
		{
			public Guid E8_PK { get; set; }
			public string E8_AssemblyName { get; set; }
			public string E8_SourcePath { get; set; }
			public List<TestClass> Classes { get; set; }
			public Assembly(Guid pk,
				string dllName = "NSA.Spy.Module",
				string sourcePath = "$/Compliance/XFiles",
				List<TestClass> classes = null)
			{
				E8_PK = pk;
				E8_AssemblyName = dllName;
				E8_SourcePath = sourcePath;
				Classes = classes ?? new List<TestClass>();
			}
		}

		class TestClass
		{
			public Guid E2_PK { get; set; }
			public Guid E2_E8 { get; set; }
			public string E2_TestClass { get; set; }
			public List<TestMethod> Methods { get; set; }
			public TestClass(Guid pk,
				string testClass = "XKeyscore",
				List<TestMethod> methods = null)
			{
				E2_PK = pk;
				E2_TestClass = testClass;
				Methods = methods ?? new List<TestMethod>();
			}
		}

		class TestMethod
		{
			public Guid E6_PK { get; set; }
			public Guid E6_E2 { get; set; }
			public string E6_MethodName { get; set; }
			public DateTime E6_DateCreated { get; set; }
			public List<AmnestyFailures> AmnestyFailures { get; set; }
			public TestMethod(Guid pk,
				string methodName = "TestListen",
				string created = "2016-03-04 15:18:25",
				List<AmnestyFailures> amnestyFailures = null)
			{
				E6_PK = pk;
				E6_MethodName = methodName;
				E6_DateCreated = ParseSQLDT(created);
				AmnestyFailures = amnestyFailures ?? new List<AmnestyFailures>();
			}
		}

		class AmnestyFailures
		{
			public Guid AF_PK { get; set; }
			public Guid AF_E6 { get; set; }
			public Guid? AF_IM { get; set; }
			public DateTime AF_StartDate { get; set; }
			public DateTime? AF_ExpiryDate { get; set; }

			public AmnestyFailures(Guid pk, string start = "2014-05-29 07:00:24", string expiry = null, string ediWIPK = null)
			{
				AF_PK = pk;
				AF_StartDate = ParseSQLDT(start);
				AF_ExpiryDate = (expiry == null) ? null : ParseSQLDT(expiry);
				AF_IM = (ediWIPK == null) ? null : new Guid(ediWIPK);
			}
		}
		#endregion
		#endregion
	}
}
