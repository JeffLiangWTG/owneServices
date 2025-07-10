using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.GenericJob;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.DataTransfer.Testing;
using Enterprise.Accounting.DataTransfer.XmlMapping;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.WipsAndAccruals.Testing
{
	[TestedType(typeof(WIPAndAccrualDataAdapter))]
	sealed class WIPAndAccrualDataAdapterTest : BaseAccountingDataAdapterTest<WIPAccrualPRBusinessObject, Xsd.WipOrAccrual>
	{
		#region Exporting

		public void TestDepartmentActivity()
		{
			FillWIPAccrualBizObjWithTestData(typeof(Accrual), ChargeCodeCC1, 100.0M, false);
			Xsd.WipOrAccrual xmlWIPOrAccrual = new Xsd.WipOrAccrual();
			WIPAndAccrualDataAdapter.PopulateValuesForXmlWIPOrAccrualExposed(WIPAccrualBizObj, xmlWIPOrAccrual, new NotificationBuffer());
			AssertEquals(Xsd.DepartmentActivity.Miscellaneous, xmlWIPOrAccrual.DepartmentActivity);
			Assert("DepartmentActivity not specified", xmlWIPOrAccrual.DepartmentActivitySpecified);
		}

		public void TestExportingLineWithAttachedJobDecWillProvideAgentsReference()
		{
			const string AgentRef = "AGENTREF123";
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			AccChargeCode chargeCode = testFactory.NewWithValidTestData<AccChargeCode>();

			BaseJobDeclaration jobDec = testFactory.NewWithValidTestData<BaseJobDeclaration>();
			jobDec.JE_AgentsReference = AgentRef;

			Job testJob = testFactory.NewJobForTesting<Job>();
			testJob.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			testJob.JH_ParentID = jobDec.PK;
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			testJob.JH_JobNum = "B123456789";

			testFactory.Save();

			FillWIPAccrualBizObjWithTestData(typeof(Accrual), chargeCode, 500.23m, false);
			WIPAccrualBizObj.WIP.AL_JH = testJob.PK;

			Xsd.WipOrAccrual xmlWIPOrAccrual = new Xsd.WipOrAccrual();
			WIPAndAccrualDataAdapter.PopulateValuesForXmlWIPOrAccrualExposed(WIPAccrualBizObj, xmlWIPOrAccrual, new NotificationBuffer());

			AssertEquals(AgentRef, xmlWIPOrAccrual.AgentsReference);
		}

		public void TestExportingLineWithAttachedShipmentThatHasDeclaration()
		{
			const string AgentRef = "AGENTREF123";
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			AccChargeCode chargeCode = testFactory.NewWithValidTestData<AccChargeCode>();

			BaseJobDeclaration jobDec = testFactory.NewWithValidTestData<BaseJobDeclaration>();
			jobDec.JE_AgentsReference = AgentRef;

			ForwardingShipment shipment = testFactory.New<ForwardingShipment>();
			jobDec.JE_JS = shipment.PK;

			Job testJob = testFactory.NewJobForTesting<Job>();
			testJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			testJob.JH_ParentID = shipment.PK;
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			testJob.JH_JobNum = "1234567890";

			testFactory.Save();

			FillWIPAccrualBizObjWithTestData(typeof(Accrual), chargeCode, 500.23m, false);
			WIPAccrualBizObj.WIP.AL_JH = testJob.PK;

			Xsd.WipOrAccrual xmlWIPOrAccrual = new Xsd.WipOrAccrual();
			WIPAndAccrualDataAdapter.PopulateValuesForXmlWIPOrAccrualExposed(WIPAccrualBizObj, xmlWIPOrAccrual, new NotificationBuffer());

			AssertEquals(AgentRef, xmlWIPOrAccrual.AgentsReference);
		}

		void SetupExportBatchAndSequenceNumbers()
		{
			ObjectCreator.CreateGenExportBatchSequencePostLine(155, WIPAccrualBizObj.WIP.PK, 100);
			ObjectCreator.CreateGenExportBatchSequenceReverseLine(183, WIPAccrualBizObj.WIP.PK, 15);
			postTransactionReference = "000015500100";
			reverseTransactionReference = "000018300015";
		}

		public void TestExportOfPostedWIP()
		{
			AccChargeCode chargeCode = ChargeCodeCC1;
			FillWIPAccrualBizObjWithTestData(typeof(Accrual), chargeCode, 100.0M, false);

			AccGLHeader glHeader = ObjectCreator.GLHeader1;
			glHeader.AG_AccountNum = "1030.30.30";
			WIPAccrualBizObj.WIP.AL_AG = glHeader.PK;
			AssertNotEquals("ChargeCode GL Account should not equal GL Header Account", chargeCode.WIPAccount.AG_AccountNum, glHeader.AG_AccountNum);

			SetupExportBatchAndSequenceNumbers();
			AssertNotNull("Must Be Exported as Posted", WIPAccrualBizObj.WIP.ExportBatchSequencePostedObject);
			AssertNotNull("Must Be Exported as Reversed", WIPAccrualBizObj.WIP.ExportBatchSequenceReversedObject);

			Xsd.WipOrAccrual xmlWIPOrAccrual = new Xsd.WipOrAccrual();
			WIPAndAccrualDataAdapter.PopulateValuesForXmlWIPOrAccrualExposed(WIPAccrualBizObj, xmlWIPOrAccrual, new NotificationBuffer());
			AssertCommonFunctionalityForAllWIPOrAccrual(WIPAccrualBizObj, xmlWIPOrAccrual, postTransactionReference);
		}

		public void TestExportOfPostedAccrual()
		{
			AccChargeCode chargeCode = ChargeCodeCC1;
			FillWIPAccrualBizObjWithTestData(typeof(Accrual), chargeCode, 100.0M, false);

			AccGLHeader glHeader = ObjectCreator.GLHeader1;
			glHeader.AG_AccountNum = "1030.30.30";
			WIPAccrualBizObj.WIP.AL_AG = glHeader.PK;
			AssertNotEquals("ChargeCode GL Account should not equal GL Header Account", chargeCode.AccrualAccount.AG_AccountNum, glHeader.AG_AccountNum);

			SetupExportBatchAndSequenceNumbers();
			AssertNotNull("Must Be Exported as Posted", WIPAccrualBizObj.WIP.ExportBatchSequenceReversedObject);
			AssertNotNull("Must Be Exported as Reversed", WIPAccrualBizObj.WIP.ExportBatchSequenceReversedObject);

			Xsd.WipOrAccrual xmlWIPOrAccrual = new Xsd.WipOrAccrual();
			WIPAndAccrualDataAdapter.PopulateValuesForXmlWIPOrAccrualExposed(WIPAccrualBizObj, xmlWIPOrAccrual, new NotificationBuffer());
			AssertCommonFunctionalityForAllWIPOrAccrual(WIPAccrualBizObj, xmlWIPOrAccrual, postTransactionReference);
		}

		public void TestExportOfReversedWIP()
		{
			FillWIPAccrualBizObjWithTestData(typeof(WIP), ChargeCodeCC1, 100.0M, true);
			SetupExportBatchAndSequenceNumbers();
			AssertNotNull("Must Be Exported as Posted", WIPAccrualBizObj.WIP.ExportBatchSequenceReversedObject);
			AssertNotNull("Must Be Exported as Reversed", WIPAccrualBizObj.WIP.ExportBatchSequenceReversedObject);

			Xsd.WipOrAccrual xmlWIPOrAccrual = new Xsd.WipOrAccrual();
			WIPAndAccrualDataAdapter.PopulateValuesForXmlWIPOrAccrualExposed(WIPAccrualBizObj, xmlWIPOrAccrual, new NotificationBuffer());
			AssertCommonFunctionalityForAllWIPOrAccrual(WIPAccrualBizObj, xmlWIPOrAccrual, reverseTransactionReference);
		}

		public void TestExportOfReversedAccrual()
		{
			FillWIPAccrualBizObjWithTestData(typeof(WIP), ChargeCodeCC1, 100.0M, true);
			SetupExportBatchAndSequenceNumbers();
			AssertNotNull("Must Be Exported as Posted", WIPAccrualBizObj.WIP.ExportBatchSequenceReversedObject);
			AssertNotNull("Must Be Exported as Reversed", WIPAccrualBizObj.WIP.ExportBatchSequenceReversedObject);

			Xsd.WipOrAccrual xmlWIPOrAccrual = new Xsd.WipOrAccrual();
			xmlWIPOrAccrual.PostOrReverse = Enterprise.DataTransfer.Xml.XsdVersion1.WipOrAccrualPostOrReverse.R;
			WIPAndAccrualDataAdapter.PopulateValuesForXmlWIPOrAccrualExposed(WIPAccrualBizObj, xmlWIPOrAccrual, new NotificationBuffer());
			AssertCommonFunctionalityForAllWIPOrAccrual(WIPAccrualBizObj, xmlWIPOrAccrual, reverseTransactionReference);
		}

		[ExpectException(typeof(InvalidOperationException))]
		public void TestExportOfUnhandledWIPOrAccrualLineType()
		{
			WIPAndAccrualDataAdapter.GetWIPOrAccrualLineTypeExposed(TransactionLineTypes.Cost);
		}

		public void TestExportOfFinancialValueDecimalPlaces_0()
		{
			var resultWithZeroDecimalPlaces = ArrangeFinancialValueForRatio(0);

			AssertEquals("Result with zero decimal places", "1", resultWithZeroDecimalPlaces.Value.ToString());
			AssertDecimalPlacesWhenExporting("1");
		}

		public void TestExportOfFinancialValueDecimalPlaces_1()
		{
			var resultWith1DecimalPlace = ArrangeFinancialValueForRatio(10);

			AssertEquals("Result with 1 decimal place", "1.1", resultWith1DecimalPlace.Value.ToString());
			AssertDecimalPlacesWhenExporting("1.1");
		}

		public void TestExportOfFinancialValueDecimalPlaces_2()
		{
			var resultWith2DecimalPlaces = ArrangeFinancialValueForRatio(100);

			AssertEquals("Result with 2 decimal places", "1.12", resultWith2DecimalPlaces.Value.ToString());
			AssertDecimalPlacesWhenExporting("1.12");
		}

		public void TestExportOfFinancialValueDecimalPlaces_3()
		{
			var resultWith3DecimalPlaces = ArrangeFinancialValueForRatio(1000);

			AssertEquals("Result with 3 decimal places", "1.120", resultWith3DecimalPlaces.Value.ToString());
			AssertDecimalPlacesWhenExporting("1.120");
		}

		[TestDate(2008, 12, 29)]
		public void TestJobRevenueRecognitionDateMin()
		{
			var xsd = new Xsd.WipOrAccrual();

			WIPAccrualBizObj.WIP.AL_PostDate = new ZDateTime(2009, 1, 1);
			WIPAndAccrualDataAdapter.ExportToValueObjectCoreExposed(WIPAccrualBizObj, xsd, new NotificationBuffer());

			AssertEquals(xsd.PostOrReverseDate, xsd.JobRevenueRecognitionDate);
		}

		[TestDate(2009, 05, 30)]
		public void TestJobRevenueRecognitionDateNormal()
		{
			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting = valuesForTest.AddNew();
			setting.JobType = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			((Job)WIPAccrualBizObj.WIP.Job).ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			TestObjectCreator creator = new TestObjectCreator(Factory);
			creator.CreateJobChargeRevRecognition((Job)WIPAccrualBizObj.WIP.Job, RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction, new ZDateTime(2008, 12, 25));

			var xsd = new Xsd.WipOrAccrual();

			WIPAccrualBizObj.WIP.AL_PostDate = new ZDateTime(2009, 1, 1);
			WIPAccrualBizObj.WIP.AL_AC = WIPAccrualBizObj.WIP.AL_AC; // to set revenue recognition type
			WIPAndAccrualDataAdapter.ExportToValueObjectCoreExposed(WIPAccrualBizObj, xsd, new NotificationBuffer());

			AssertEquals(new ZDateTime(2008, 12, 25), xsd.JobRevenueRecognitionDate);
		}

		Xsd.FinancialValue ArrangeFinancialValueForRatio(ZInt ratio)
		{
			RefCurrency currentCompanyCurrency = Factory.Load<RefCurrency>(GlbCompany.CurrentCompany.LocalCurrency.PK);
			currentCompanyCurrency.RX_SubUnitRatio = ratio;

			ZDecimal originalValue = 1.1200000M;
			WIPAccrualBizObj.WIP.AL_OSExTaxAmount = originalValue;
			WIPAccrualBizObj.WIP.AL_LocalExTaxAmount = WIPAccrualBizObj.WIP.AL_OSExTaxAmount;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(WIPAccrualBizObj.WIP);
			Factory.Save();

			return WIPAndAccrualDataAdapter.GetXmlFinancialValue(originalValue, currentCompanyCurrency, typeof(WIP), Xsd.WipOrAccrualPostOrReverse.P);
		}

		void AssertDecimalPlacesWhenExporting(ZString expectedValue)
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			BaseWIPAccrual reloadedWip = newFactory.Load<BaseWIPAccrual>(WIPAccrualBizObj.WIP.PK);
			WIPAccrualPRBusinessObject reloadedWipAccrualPRBusinessObject = new WIPAccrualPRBusinessObject(reloadedWip, Xsd.WipOrAccrualPostOrReverse.P);

			Xsd.WipOrAccrual wipOrAccrualXml = new Xsd.WipOrAccrual();

			WIPAndAccrualDataAdapter.ExportToValueObjectCoreExposed(reloadedWipAccrualPRBusinessObject, wipOrAccrualXml, new NotificationBuffer());

			AssertEquals("Local TransactionHeader Amount Excluding Tax", expectedValue, wipOrAccrualXml.LocalInvoiceAmtExclTax.Value.ToString());
		}

		#endregion

		#region Importing

		[ExpectException(typeof(NotSupportedException))]
		public void TestImporting()
		{
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			WIPAccrualPRBusinessObject bizobj = new WIPAccrualPRBusinessObject(Factory.NewWithValidTestData<WIP>(), Xsd.WipOrAccrualPostOrReverse.P);
			WIPAndAccrualDataAdapter.ImportFromValueObjectCoreExposed(bizobj, new Xsd.WipOrAccrual(), context);
		}

		#endregion

		#region Assert Helpers

		void AssertCommonFunctionalityForAllWIPOrAccrual(WIPAccrualPRBusinessObject wIPOrAccrualBizObj, Xsd.WipOrAccrual xmlWIPOrAccrual, string transactionReference)
		{
			AssertEquals("Branch", WIPAccrualBizObj.WIP.Branch.GB_Code, xmlWIPOrAccrual.Branch);
			AssertEquals("Department", WIPAccrualBizObj.WIP.Department.GE_Code, xmlWIPOrAccrual.Department);
			AssertEquals("Charge Code", WIPAccrualBizObj.WIP.ChargeCode.AC_Code, xmlWIPOrAccrual.ChargeCode);
			AssertEquals("Created User ID", WIPAccrualBizObj.WIP.AL_Calc_CreatingUserName, xmlWIPOrAccrual.CreatedUserId);
			AssertEquals("Description", WIPAccrualBizObj.WIP.AL_Desc, xmlWIPOrAccrual.Description);
			AssertEquals("Post or Reverse", wIPOrAccrualBizObj.PostedOrReverseStatus, xmlWIPOrAccrual.PostOrReverse);
			AssertEquals("WIPOrAccrual GUID", "GUID", xmlWIPOrAccrual.WipOrAccrualGUID);
			AssertEquals("Transaction Reference", transactionReference, xmlWIPOrAccrual.TransactionReference);

			if (WIPAccrualBizObj.WIP.Header != null)
			{
				AssertEquals("Debtor or Creditor", WIPAccrualBizObj.WIP.Header.OH_FullName, xmlWIPOrAccrual.DebtorOrCreditor.EDICode);
			}

			if (WIPAccrualBizObj.WIP.AL_LineType == TransactionLineTypes.WIP)
			{
				AssertEquals("WIP or Accrual Line Type (WIP/REV)", Xsd.WipOrAccrualLineType.REV, xmlWIPOrAccrual.LineType);
			}
			else
			{
				AssertEquals("WIP or Accrual Line Type (ACR/CST)", Xsd.WipOrAccrualLineType.CST, xmlWIPOrAccrual.LineType);
			}
			AssertEquals("GL Account", WIPAccrualBizObj.WIP.GLHeader.AG_AccountNum, xmlWIPOrAccrual.GLAccount);

			if (wIPOrAccrualBizObj.PostedOrReverseStatus == Xsd.WipOrAccrualPostOrReverse.P)
			{
				AssertEquals("Post Date (Post or Reverse Date)", WIPAccrualBizObj.WIP.AL_PostDate, xmlWIPOrAccrual.PostOrReverseDate);
			}
			else
			{
				AssertEquals("Reverse Date (Post or Reverse Date)", WIPAccrualBizObj.WIP.AL_ReverseDate, xmlWIPOrAccrual.PostOrReverseDate);
			}

			if (WIPAccrualBizObj.WIP.AL_LineType == TransactionLineTypes.WIP)
			{
				AssertEquals("Line Type", Xsd.WipOrAccrualLineType.REV, xmlWIPOrAccrual.LineType);
			}
			else
			{
				AssertEquals("Line Type", Xsd.WipOrAccrualLineType.CST, xmlWIPOrAccrual.LineType);
			}

			GenericJob jobDetails = null;

			if (WIPAccrualBizObj.WIP.AL_JH.IsValid)
			{
				jobDetails = (GenericJob)wIPOrAccrualBizObj.Factory.LoadGenericJob(WIPAccrualBizObj.WIP.Job);
			}

			if (jobDetails != null)
			{
				AssertEquals("Destination Port Code", jobDetails.InvoicingSupporter.Destination.RL_PortName, xmlWIPOrAccrual.DestinationPortCode.City);
				AssertEquals("Origin Port Code", jobDetails.InvoicingSupporter.Origin.RL_PortName, xmlWIPOrAccrual.OriginPortCode.City);
				AssertEquals("Master Bill Number", jobDetails.InvoicingSupporter.MasterBillNumber, xmlWIPOrAccrual.MasterBillNo);
				AssertEquals("House Bill Number", jobDetails.InvoicingSupporter.HouseBillNumber, xmlWIPOrAccrual.HouseBIllNo);

				var paymentTermInfo = jobDetails.InvoicingSupporter.PaymentTerm.GetPaymentTermInfo(CostSell.Revenue);
				AssertEquals("Incoterm", paymentTermInfo != null && paymentTermInfo.InfoType == PaymentTermType.Incoterm ? paymentTermInfo.Value : string.Empty, xmlWIPOrAccrual.Incoterm);

				AssertEquals("Job Number", jobDetails.JobNumber, xmlWIPOrAccrual.JobNo);
				AssertEquals("Job Type", WipOrAccrualJobTypeXmlMapping.Instance.GetExternalCode(jobDetails.VJ_JobType, "", new NotificationBuffer()), xmlWIPOrAccrual.JobType);
				AssertEquals("Job Type Specified", true, xmlWIPOrAccrual.JobTypeSpecified);
			}
		}

		#endregion

		#region AgentReference

		public void TestAgentReferenceForShipment()
		{
			BaseJobDeclaration declaration = Factory.NewWithValidTestData(typeof(BaseJobDeclaration)) as BaseJobDeclaration;
			declaration.JE_JS = Shipment.PK;
			declaration.JE_AgentsReference = "TESTAGENT";

			BaseWIPAccrual testWIP = Factory.NewWithValidTestData(typeof(WIP)) as BaseWIPAccrual;
			testWIP.AL_JH = Job.PK;
			Xsd.WipOrAccrual testXmlWIPAccrual = new Xsd.WipOrAccrual();

			WIPAndAccrualDataAdapter.SetAgentsReference(testXmlWIPAccrual, testWIP, new NotificationBuffer());

			AssertEquals(declaration.JE_AgentsReference, testXmlWIPAccrual.AgentsReference);
		}

		public void TestAgentReferenceForDeclaration()
		{
			BaseJobDeclaration declaration = Factory.NewWithValidTestData(typeof(BaseJobDeclaration)) as BaseJobDeclaration;
			declaration.JE_AgentsReference = "TESTAGENT";
			declaration.JE_DeclarationReference = "B010101";

			Job testJob = Factory.NewJobForTesting<Job>();
			testJob.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			testJob.JH_ParentID = declaration.PK;
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			testJob.JH_JobNum = declaration.JE_DeclarationReference;

			BaseWIPAccrual testWIP = Factory.NewWithValidTestData(typeof(WIP)) as BaseWIPAccrual;
			testWIP.AL_JH = testJob.PK;

			Xsd.WipOrAccrual testXmlWIPAccrual = new Xsd.WipOrAccrual();
			WIPAndAccrualDataAdapter.SetAgentsReference(testXmlWIPAccrual, testWIP, new NotificationBuffer());

			AssertEquals(declaration.JE_AgentsReference, testXmlWIPAccrual.AgentsReference);
		}

		#endregion

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			retriever?.Dispose();
			retriever = null;
		}

		EmbeddedResourceRetriever Retriever => retriever ??= new ();
		EmbeddedResourceRetriever retriever;

		protected override WIPAccrualPRBusinessObject NewBusinessObject()
		{
			return WIPAccrualBizObj;
		}

		protected override bool IsImportFromValueObjectSupported
		{
			get { return false; }
		}

		protected override ValueObjectDataAdapter<WIPAccrualPRBusinessObject, Xsd.WipOrAccrual> GetNewBizObjXmlDataAdapter()
		{
			return new WIPAndAccrualDataAdapter();
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return "FinancialTransactions"; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "FinancialWIPOrAccrual"; }
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return ReversedWIP;
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			return PostedWIP;
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return new BusinessObjectAndExpectedOutputFileName[] { PostedWIP, PostedAccrual, ReversedWIP, ReversedAccrual };
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return PostedAccrual;
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[] { "DebtorOrCreditor", "CreatedUserId", "DepartmentActivity", "AgentsReference" };
			}
		}

		#region Business Object Samples

		#region Posted WIP

		BusinessObjectAndExpectedOutputFileName PostedWIP
		{
			get
			{
				if (fPostedWIP == null)
				{
					FillWIPAccrualBizObjWithTestData(typeof(WIP), ChargeCodeCC1, 123.45M, false);
					WIPAccrualBizObj.WIP.AL_Desc = "WIP for S00001234";
					fPostedWIP = new BusinessObjectAndExpectedOutputFileName(WIPAccrualBizObj, Retriever.SaveResourceToFile("PostedWIP.xml"), ValidationKind.None, "Posted WIP");
				}

				return fPostedWIP;
			}
		}

		BusinessObjectAndExpectedOutputFileName fPostedWIP;

		#endregion

		#region Posted Accrual

		BusinessObjectAndExpectedOutputFileName PostedAccrual
		{
			get
			{
				if (fPostedAccrual == null)
				{
					FillWIPAccrualBizObjWithTestData(typeof(Accrual), ChargeCodeCC1, 100.00M, false);
					WIPAccrualBizObj.WIP.AL_Desc = "Accrual for S00001234";
					fPostedAccrual = new BusinessObjectAndExpectedOutputFileName(WIPAccrualBizObj, Retriever.SaveResourceToFile("PostedAccrual.xml"), ValidationKind.None, "Posted Accrual");
				}

				return fPostedAccrual;
			}
		}

		BusinessObjectAndExpectedOutputFileName fPostedAccrual;

		#endregion

		#region Reversed WIP

		BusinessObjectAndExpectedOutputFileName ReversedWIP
		{
			get
			{
				if (fReversedWIP == null)
				{
					FillWIPAccrualBizObjWithTestData(typeof(WIP), ChargeCodeCC1, 123.45M, false);
					WIPAccrualBizObj.WIP.AL_Desc = "WIP for S00001234 (Reversal)";
					using (MasterFiles.Business.Testing.SkipReportingWhenReversedWIPACRLinkedToJobChargeAttribute.ActivateTemporary())
					{
						WIPAccrualBizObj.WIP.AL_ReverseDate = PostDate.AddDays(1);
					}
					fReversedWIP = new BusinessObjectAndExpectedOutputFileName(WIPAccrualBizObj, Retriever.SaveResourceToFile("ReversedWIP.xml"), ValidationKind.None, "Reversed WIP");
				}

				return fReversedWIP;
			}
		}

		BusinessObjectAndExpectedOutputFileName fReversedWIP;

		#endregion

		#region Reversed Accrual

		BusinessObjectAndExpectedOutputFileName ReversedAccrual
		{
			get
			{
				if (fReversedAccrual == null)
				{
					FillWIPAccrualBizObjWithTestData(typeof(Accrual), ChargeCodeCC1, 123.45M, false);
					WIPAccrualBizObj.WIP.AL_Desc = "Accrual for S00001234 (Reversal)";
					using (MasterFiles.Business.Testing.SkipReportingWhenReversedWIPACRLinkedToJobChargeAttribute.ActivateTemporary())
					{
						WIPAccrualBizObj.WIP.AL_ReverseDate = PostDate.AddDays(1);
					}
					fReversedAccrual = new BusinessObjectAndExpectedOutputFileName(WIPAccrualBizObj, Retriever.SaveResourceToFile("ReversedAccrual.xml"), ValidationKind.None, "Reversed Accrual");
				}

				return fReversedAccrual;
			}
		}

		BusinessObjectAndExpectedOutputFileName fReversedAccrual;

		#endregion

		#endregion

		void FillWIPAccrualBizObjWithTestData(Type type, AccChargeCode chargeCode, decimal localExTaxAmount, bool makeReversal)
		{
			BaseWIPAccrual wIP = (BaseWIPAccrual)Factory.NewWithValidTestData(type);
			chargeCode.AC_ChargeSubGroup = "FUM";
			wIP.AL_AC = chargeCode.PK;
			wIP.AL_LocalExTaxAmount = localExTaxAmount;
			wIP.AL_OSExTaxAmount = localExTaxAmount;
			wIP.AL_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			wIP.AL_PostDate = PostDate;
			wIP.AL_AC = chargeCode.PK;
			wIP.AL_Desc = "Description of Accrual or WIP";
			wIP.AL_JH = Job.PK;
			BaseCharge charge = Factory.NewWithValidTestData<BaseCharge>();
			charge.JR_JH = Job.PK;
			if (wIP.AL_LineType == TransactionLineTypes.WIP)
			{
				charge.JR_AL_ARLine = wIP.PK;
				charge.SetChargeValuesFromLinkedARLineForTests();
			}
			if (wIP.AL_LineType == TransactionLineTypes.Accrual)
			{
				charge.JR_AL_APLine = wIP.PK;
				charge.SetChargeValuesFromLinkedAPLineForTests();
			}

			Xsd.WipOrAccrualPostOrReverse postOrReverse =
					makeReversal ? Enterprise.DataTransfer.Xml.XsdVersion1.WipOrAccrualPostOrReverse.R :
						Enterprise.DataTransfer.Xml.XsdVersion1.WipOrAccrualPostOrReverse.P;
			WIPAccrualBizObj = new WIPAccrualPRBusinessObject(wIP, postOrReverse);

			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(wIP);
		}

		protected override void SetUp()
		{
			base.SetUp();

			BusinessObjectFactory testFactory = new BusinessObjectFactory();

			Consol = testFactory.NewWithValidTestData<ForwardingConsol>();
			Consol.JK_MasterBillNum = "ABCDEFGH";

			Shipment = Consol.Shipments.AddNew();
			Shipment.JS_INCO = "FOB";
			Shipment.JS_UniqueConsignRef = "S00001234";
			Shipment.JS_HouseBill = "UVWXYZ";
			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_RL_NKDestination = "USLAX";

			Job = testFactory.NewJobForTesting<Job>();
			Job.JH_ParentTableCode = "JS";
			Job.JH_ParentID = Shipment.PK;
			Job.JH_GB = GlbBranch.CurrentBranch.PK;
			Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Job.JH_JobNum = Shipment.JS_UniqueConsignRef;

			RefCurrency currentCompanyCurrency = testFactory.Load<RefCurrency>(GlbCompany.CurrentCompany.LocalCurrency.PK);
			currentCompanyCurrency.RX_SubUnitRatio = 100;

			testFactory.Save();

			WIPAndAccrualDataAdapter = new WIPAndAccrualDataAdapterForTest();
			FillWIPAccrualBizObjWithTestData(typeof(WIP), ChargeCodeCC1, 100.0M, false);
		}

		WIPAccrualPRBusinessObject WIPAccrualBizObj;
		WIPAndAccrualDataAdapterForTest WIPAndAccrualDataAdapter;
		ForwardingConsol Consol;
		ForwardingShipment Shipment;
		Job Job;
		string postTransactionReference;
		string reverseTransactionReference;

		#endregion

		#region Test Class

		class WIPAndAccrualDataAdapterForTest : WIPAndAccrualDataAdapter
		{
			public void PopulateValuesForXmlWIPOrAccrualExposed(WIPAccrualPRBusinessObject wIPOrAccrualBizObj, Xsd.WipOrAccrual xmlWIPOrAccrual, INotifications notify)
			{
				base.PopulateValuesForXmlWIPOrAccrual(wIPOrAccrualBizObj, xmlWIPOrAccrual, new ValueObjectExportContext(notify));
			}

			public void ExportToValueObjectCoreExposed(WIPAccrualPRBusinessObject bizObj, Xsd.WipOrAccrual constructedValueObject, INotifications notify)
			{
				base.ExportToValueObjectCore(bizObj, constructedValueObject, new ValueObjectExportContext(notify));
			}

			public Xsd.WipOrAccrualLineType GetWIPOrAccrualLineTypeExposed(ZString jobTypeDescription)
			{
				return base.GetWIPOrAccrualLineType(jobTypeDescription);
			}

			public void ImportFromValueObjectCoreExposed(WIPAccrualPRBusinessObject bizObj, Xsd.WipOrAccrual value, ValueObjectImportContext context)
			{
				base.ImportFromValueObjectCore(bizObj, value, context);
			}

			public new Xsd.FinancialValue GetXmlFinancialValue(ZDecimal value, RefCurrency currency, Type transactionType, Xsd.WipOrAccrualPostOrReverse postOrReverse)
			{
				return base.GetXmlFinancialValue(value, currency, transactionType, postOrReverse);
			}

			public new void SetAgentsReference(Xsd.WipOrAccrual xmlWIPAccrualLine, BaseWIPAccrual wIPAccrualLine, INotifications notify)
			{
				base.SetAgentsReference(xmlWIPAccrualLine, wIPAccrualLine, notify);
			}
		}

		#endregion
	}
}
