using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CTOCusMAWB))]
	sealed class CTOCusMAWBTest : CusMAWBBaseAbstractTest
	{
		public void TestIDocManagerSupportIncudingRelatedObjectsMembers()
		{
			var ctoMAWB = Factory.New<CTOCusMAWB>();
			var underbond = ctoMAWB.AllUnderbonds.AddNew();
			var supporter = ctoMAWB as IDocManagerSupportIncudingRelatedObjects;
			AssertEquals("Self reference", ctoMAWB, supporter.SelfReference);
			AssertEquals(1, supporter.GetRelatedBusinessObjects().Count());
			Assert(supporter.GetRelatedBusinessObjects().Contains(underbond));
			AssertType(typeof(DocManagerIncludingRelatedObjectsInfo), ((IDocManagerSupport)ctoMAWB).DocManagerInfo);
		}

		public void TestIDataExportCSVFileNameProviderMembers()
		{
			MAWB.CM_MAWB = "MB001";
			var fileNameProvider = MAWB as IDataExportCSVFileNameProvider;
			AssertEquals("Suffix name is as the same as CM_MAWB", "MB001", fileNameProvider.FileNameSuffix);
		}

		public void TestIMessageManageableBizObj()
		{
			Customs.Business.IMessageManageableBizObj mawb = MAWB;

			AssertEquals("MessageManager", typeof(CTOCusMAWBMessageManager), mawb.GetMessageManagerForAmendmentDetection().GetType());
		}

		public void TestUsesTranshipmentPortOnUnderbond()
		{
			AssertEquals(false, ((ICusUnderbondDependentCollectionParent)MAWB).UsesTranshipmentPortOnUnderbond);
			AssertEquals(ZString.Empty, ((ICusUnderbondDependentCollectionParent)MAWB).DefaultTranshipmentPort);
		}

		public override void TestIsStandAlone()
		{
			AssertEquals("AirCTO is always Stand-alone", true, MAWB.IsStandAlone);
		}

		public void TestValidation()
		{
			AssertEquals(typeof(CTOCusMAWBValidation), MAWB.Validation.GetType());
		}

		public void TestChildrenbills()
		{
			AssertNotNull("Children Bill", MAWB.ChildBills);
			AssertEquals("Children Bill is registered", true, MAWB.IsRegisteredEditableChildObject(MAWB.ChildBills));
		}

		public void TestCanSendWithoutDelay()
		{
			var mAWB = Factory.New<CTOCusMAWB>();
			Assert(((ICusUnderbondDependentCollectionParent)mAWB).CanSendWithoutDelay);
		}

		public void TestSetDefaultValuesFromCTOCusMAWB()
		{
			var mAWB = Factory.New<CTOCusMAWB>();
			mAWB.CM_FlightNo = "QF123";
			CTOCusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_PiecesManifested = 125;
			CusUnderbond testUnderbond = hAWB.Underbonds.AddNew();
			testUnderbond.C4_ParentID = hAWB.PK;
			testUnderbond.C4_ParentTableCode = "CS";
			testUnderbond.SetDefaultValuesFromParent();
			AssertEquals("QF123", testUnderbond.C4_FlightNo);
			AssertEquals(125u, testUnderbond.C4_PiecesManifested);
		}

		public void TestSetDefaultValuesFromCTOCusMAWBMaxLengthExceeded()
		{
			var mAWB = Factory.New<CTOCusMAWB>();
			CTOCusHAWB hAWB1 = mAWB.ChildBills.AddNew();
			hAWB1.CS_PiecesManifested = 32000;
			CTOCusHAWB hAWB2 = mAWB.ChildBills.AddNew();
			hAWB2.CS_PiecesManifested = 32000;
			CusUnderbond testUnderbond = mAWB.Underbonds.AddNew();
			testUnderbond.C4_ParentID = mAWB.PK;
			testUnderbond.C4_ParentTableCode = "CM";
			testUnderbond.SetDefaultValuesFromParent();
			AssertEquals(0u, testUnderbond.C4_PiecesManifested);
		}

		public void TestGetAllPossibleCollectionProviders()
		{
			var masterBill = Factory.New<CTOCusMAWB>();
			CTOCusHAWB houseBill = masterBill.ChildBills.AddNew();
			CusPartShip partShip = houseBill.PartShips.AddNew();
			ICusUnderbondUnionCollectionParent masterBillUnion = masterBill;

			AssertNotNull("GetAllPossibleCollectionProviders should not return null", masterBillUnion.GetAllPossibleCollectionProviders());
			AssertEquals("Length of AllPossibleCollectionProviders", 2, masterBillUnion.GetAllPossibleCollectionProviders().Length);
			AssertEquals("Element 0", houseBill, masterBillUnion.GetAllPossibleCollectionProviders()[0]);
			AssertEquals("Element 1", partShip, masterBillUnion.GetAllPossibleCollectionProviders()[1]);
		}

		public override void TestAllUnderbonds()
		{
			var masterBill = Factory.New<CTOCusMAWB>();
			masterBill.ChildBills.AddNew();
			ICusUnderbondUnionCollectionParent masterBillUnder = masterBill;
			AssertNotNull(masterBillUnder.AllUnderbonds);
			CusUnderbond underbond = masterBill.ChildBills.AddNew().Underbonds.AddNew();
			masterBill.AllUnderbonds.Load();
			AssertEquals(1, masterBill.AllUnderbonds.Count);
			AssertEquals(underbond, masterBill.AllUnderbonds[0]);
		}

		public void TestFakeFlightOuturnUnderbond()
		{
			var masterBill = Factory.New<CTOCusMAWB>();
			AssertNotNull(masterBill.FakeFlightOuturnUnderbond);
		}

		public void TestFakeFlightOuturnUnderbondIsRegisteredEditable()
		{
			var masterBill = Factory.New<CTOCusMAWB>();
			AssertEquals(true, masterBill.IsRegisteredEditableChildObject(masterBill.FakeFlightOuturnUnderbond));
		}

		public void TestCM_IsCTOMAWB()
		{
			var masterBill = Factory.New<CTOCusMAWB>();
			AssertEquals("CM_IsCTOMAWB", true, masterBill.CM_IsCTOMAWB);
		}

		public void TestOutturnableLines()
		{
			var masterBill = Factory.New<CTOCusMAWB>();
			CTOCusHAWB hAWB = masterBill.ChildBills.AddNew();
			AssertEquals(1, ((ICusUnderbondDependentCollectionParent)masterBill).OutturnableLines.Length);
			AssertEquals(hAWB, ((ICusUnderbondDependentCollectionParent)masterBill).OutturnableLines[0]);
		}

		public void TestOutturnableLinesIncludesPartShips()
		{
			var otherMasterBill = Factory.New<CTOCusMAWB>();
			CTOCusHAWB otherHAWB = otherMasterBill.ChildBills.AddNew();
			CusPartShip partShip = otherHAWB.PartShips.AddNew();
			partShip.CG_FlightNo = "QF123";
			partShip.CG_ArrivalDate = new ZDateTime(2005, 7, 14);
			partShip.CG_RL_NKDischargePort = "AUSYD";

			var masterBill = Factory.New<CTOCusMAWB>();
			masterBill.CM_FlightNo = "QF123";
			masterBill.CM_ArrivalDate = new ZDateTime(2005, 7, 14);
			masterBill.CM_RL_NKDischargePort = "AUSYD";

			CTOCusHAWB hAWB = masterBill.ChildBills.AddNew();
			AssertEquals(2, ((ICusUnderbondDependentCollectionParent)masterBill).OutturnableLines.Length);
		}

		public void TestIAirOutturnReportHeaderInformationProvider_GetHeader()
		{
			var underbond = Factory.New<CusUnderbond>();
			var masterBill = Factory.New<CTOCusMAWB>();
			AssertNotNull((masterBill as IAirOutturnReportHeaderInformationProvider).GetHeader(underbond));
		}

		public void TestOutturnsAreRegisteredEditableChildObjects()
		{
			Assert("is outturn registered editable child object", MAWB.IsRegisteredEditableChildObject(MAWB.FakeFlightOuturnUnderbond));
		}

		public void TestFakeUnderbondsValidation()
		{
			var mAWB = Factory.New<CTOCusMAWB>();
			AssertEquals("FakeFlightOuturnUnderbond.Validation.GetType()", typeof(Customs.Business.CusUnderbondValidation), mAWB.FakeFlightOuturnUnderbond.Validation.GetType());
			mAWB.FakeFlightOuturnUnderbond.RunPreSaveValidation();
			AssertEquals("HasMessageErrors", false, mAWB.FakeFlightOuturnUnderbond.HasMessageErrors);
		}

		public void TestAuditSecurity()
		{
			AssertEquals("AuditSecurity", Env.Security.AUCustomsAirCTOImportAuditBilling, ((IJobInvoicingPlugIn)Factory.New<CTOCusMAWB>()).InvoicingSupporter.AuditSecurity);
		}

		public void TestJobNumber()
		{
			IJobNumber jobNumber = MAWB;
			MAWB.CM_FlightNo = "Flight";
			MAWB.CM_ArrivalDate = new ZDateTime(2000, 1, 2, 3, 4, 5);
			MAWB.CM_RL_NKFirstArrivalPort = "AUSYD";
			AssertEquals("GFlight000102AUSYD", jobNumber.JobNumber);
		}

		public void TestJobNumber_WithLongFlightNo()
		{
			IJobNumber jobNumber = MAWB;
			MAWB.CM_FlightNo = "FlightNumb";
			MAWB.CM_ArrivalDate = new ZDateTime(2000, 1, 2, 3, 4, 5);
			MAWB.CM_RL_NKFirstArrivalPort = "AUSYD";
			AssertEquals("JobNumber length must be within JobHeaderSchema.JH_JobNum.MaxLength", true, jobNumber.JobNumber.Length <= JobHeaderSchema.JH_JobNum.MaxLength);
		}

		public void TestJobHeaderIsNotDeactivatedWhenFactoryHasInvoicingPlugInGUIBusinessContext()
		{
			AssertJobHeaderDeactivation(true);
		}

		public void TestJobHeaderIsDeactivatedWhenFactoryDoesNotHaveInvoicingPlugInGUIBusinessContext()
		{
			AssertJobHeaderDeactivation(false);
		}

		public void TestEditSecurity()
		{
			IJobInvoicingPlugIn testJob = Factory.New<CTOCusMAWB>();
			AssertEquals("EditSecurityCheckPoint should be Empty", Env.Security.None, testJob.InvoicingSupporter.EditSecurityCheckpoint);
			AssertEquals("EditSecuritytMessage should be Empty", ZString.Empty, testJob.InvoicingSupporter.EditSecurityMessage);
			AssertEquals("EditSecurityLock should be False", ZBool.False, testJob.InvoicingSupporter.EditSecurityLock);
		}

		public void TestDefaultChargeGroup()
		{
			IJobInvoicingPlugIn testJob = Factory.New<CTOCusMAWB>();
			AssertEquals("DefaultChargeGroup should be Empty", ZString.Empty, testJob.InvoicingSupporter.DefaultChargeGroup);
		}

		public void TestIJobHeaderParent_AllowInvoiceDeletion()
		{
			IJobHeaderParent ctoCusMAWB = Factory.New<CTOCusMAWB>();
			Assert(ctoCusMAWB.AllowInvoiceDeletion);
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<CTOCusMAWB>();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			MAWB.ChildBills.AddNew();
			return MAWB;
		}

		CTOCusMAWB mawb;
		CTOCusMAWB MAWB => mawb ?? (mawb = Factory.New<CTOCusMAWB>());

		void AssertJobHeaderDeactivation(bool hasInvoicingPlugInGUIContext)
		{
			var cusMAWB = Factory.New<CTOCusMAWB>();
			var jobLoader = new JobHeader.Loader(cusMAWB);
			var job = jobLoader.TryCreate();
			Factory.Save();

			cusMAWB.IsCancelled = true;
			if (hasInvoicingPlugInGUIContext)
			{
				cusMAWB.SetContext(BusinessContext.InvoicingPlugInGUI);
			}

			var assertionMessage1 = string.Format("cusMAWB {0} have InvoicingPluginGUI Business Context", hasInvoicingPlugInGUIContext ? "should" : "should not");
			var assertionMessage2 = string.Format("Job {0} be deactivated by OnSaving method", hasInvoicingPlugInGUIContext ? "should not" : "should");

			Assert("Deactivating cusMAWB, IsCancelled flag should be set to true", cusMAWB.IsCancelled);
			Assert("Deactivating cusMAWB, IsCancelledInfo should have changes", cusMAWB.IsCancelledHasChanged);
			AssertEquals(assertionMessage1, hasInvoicingPlugInGUIContext, cusMAWB.HasContext(BusinessContext.InvoicingPlugInGUI));
			Assert("Job is not yet deactivated", !job.IsCancelled);

			Factory.Save();

			AssertEquals(assertionMessage2, !hasInvoicingPlugInGUIContext, job.IsCancelled);
		}
	}
}
