using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Workflow.Testing
{
	namespace Mawb
	{
		[TestedType(typeof(CusMAWB))]
		class MawbWorkflowProviderTest : WorkflowProviderTest<CusMAWB, CusMAWBProcessTaskCollection>
		{
			public void TestEverything()
			{
				IWorkflowProvider mawb = Factory.New<CusMAWB>();
				AssertType(typeof(Customs.Business.CusMAWBWorkflowInformationProvider), mawb.GetWorkflowInformationProvider());
				AssertType(typeof(CusMAWBProcessTaskCollection), mawb.WorkflowItems);
				AssertType(typeof(ColumnValueRanker), mawb.GetTemplateSelectionCriteria());
			}

			public void TestSetStatusOneFiresWorkflow()
			{
				var basic = Factory.New<CusMAWB>();
				basic.NumberOfPiecesExpected = 10;
				var triggerProcess = ((IWorkflowProvider)basic).WorkflowItems.Triggers.AddNew();
				triggerProcess.TriggerConditions.TriggerEventCode = "STC";
				triggerProcess.TriggerConditions.TriggerCondition = "REF";
				triggerProcess.TriggerConditions.TriggerConditionValue = "ST1";
				Factory.Save();
				AssertEquals(ZDateTime.Empty, triggerProcess.P9_ActualDate);
				basic.Status1Date = new ZDateTime(1986, 3, 12, 4, 27, 0);
				Factory.Save();
				AssertEquals((new ZDateTime(1986, 3, 12, 4, 27, 0)), triggerProcess.P9_ActualDate.ToZDateTime());
			}

			protected override ZString ExpectedWorkflowType
			{
				get { return JobInvoicingConsumerTypes.CusMAWB.Code; }
			}
		}

		[TestedType(typeof(CusMAWBProcessTask))]
		class CusMAWBProcessTaskTest : EnterpriseBusinessObjectTestCase
		{
			public void TestParentControllerID()
			{
				var task = Factory.New<CusMAWBProcessTask>();
				AssertEquals(ControllerIDs.Customs.GB.CcsukAirInventory, task.ParentControllerID);
			}

			public void TestParent()
			{
				var cusMAWB = Factory.New<CusMAWB>();
				var task = (CusMAWBProcessTask)((IWorkflowProvider)cusMAWB).WorkflowItems.AddNew();
				AssertEquals(cusMAWB, task.Parent);
			}

			protected override BusinessObject GetNewBusinessObject()
			{
				var cusMAWB = Factory.New<CusMAWB>();
				return ((IWorkflowProvider)cusMAWB).WorkflowItems.AddNew();
			}
		}

		[TestedType(typeof(CusMAWBProcessTaskCollection))]
		class CusMAWBProcessTaskCollectionTest : ProcessTaskCollectionTest<CusMAWBProcessTaskCollection>
		{
			protected override CusMAWBProcessTaskCollection GetCollectionToTestCore()
			{
				var mawb = Factory.New<CusMAWB>();
				return new CusMAWBProcessTaskCollection(mawb);
			}

			public void TestConditions()
			{
				RunTestConditions((ProcessTaskCollection)GetCollectionToTest());
			}

			public static void RunTestConditions(ProcessTaskCollection collection)
			{
				AssertEquals(true, collection.IsCondition1Met(JobDeclarationWorkflowCondition1CodeList.Codes.NotExport));
				AssertEquals(true, collection.IsCondition1Met(JobDeclarationWorkflowCondition1CodeList.Codes.Import));
				AssertEquals(false, collection.IsCondition1Met(JobDeclarationWorkflowCondition1CodeList.Codes.Export));
			}
		}
	}

	namespace Hawb
	{
		[TestedType(typeof(CusHAWB))]
		class HawbWorkflowProviderTest : WorkflowProviderTest<CusHAWB, CusHAWBProcessTaskCollection>
		{
			public void TestSetStatusOneFiresWorkflow()
			{
				var mawb = Factory.New<CusMAWB>();
				var hawb = mawb.ChildBills.AddNew();
				hawb.CS_PiecesManifested = 10;
				var triggerProcess = ((IWorkflowProvider)hawb).WorkflowItems.Triggers.AddNew();
				triggerProcess.TriggerConditions.TriggerEventCode = "STC";
				triggerProcess.TriggerConditions.TriggerCondition = "REF";
				triggerProcess.TriggerConditions.TriggerConditionValue = "ST1";
				Factory.Save();
				AssertEquals(ZDateTime.Empty, triggerProcess.P9_ActualDate.ToZDateTime());
				hawb.Status1Date = new ZDateTime(1986, 3, 12, 4, 27, 0);
				Factory.Save();
				AssertEquals(new ZDateTime(1986, 3, 12, 4, 27, 0), triggerProcess.P9_ActualDate.ToZDateTime());
			}

			public void TestEverything()
			{
				var mawb = Factory.New<CusMAWB>();
				IWorkflowProvider hawb = mawb.ChildBills.AddNew();
				AssertType(typeof(Customs.Business.CusHAWBWorkflowInformationProvider), hawb.GetWorkflowInformationProvider());
				AssertType(typeof(CusHAWBProcessTaskCollection), hawb.WorkflowItems);
				AssertType(typeof(ColumnValueRanker), hawb.GetTemplateSelectionCriteria());
			}

			protected override ZString ExpectedWorkflowType
			{
				get { return WorkflowDescriptors.CustomsHouseAirCargoCode; }
			}
		}

		[TestedType(typeof(CusHAWBProcessTask))]
		class CusHAWBProcessTaskTest : EnterpriseBusinessObjectTestCase
		{
			public void TestParentControllerID()
			{
				var task = Factory.New<CusHAWBProcessTask>();
				AssertEquals(ControllerIDs.Customs.GB.CcsukAirInventoryHouse, task.ParentControllerID);
			}

			public void TestParent()
			{
				var cusHAWB = Factory.New<CusHAWB>();
				var task = (CusHAWBProcessTask)((IWorkflowProvider)cusHAWB).WorkflowItems.AddNew();
				AssertEquals(cusHAWB, task.Parent);
			}

			protected override BusinessObject GetNewBusinessObject()
			{
				var cusMAWB = Factory.New<CusMAWB>();
				var cusHAWB = cusMAWB.ChildBills.AddNew();
				return ((IWorkflowProvider)cusHAWB).WorkflowItems.AddNew();
			}
		}

		[TestedType(typeof(CusHAWBProcessTaskCollection))]
		class CusHAWBProcessTaskCollectionTest : ProcessTaskCollectionTest<CusHAWBProcessTaskCollection>
		{
			protected override CusHAWBProcessTaskCollection GetCollectionToTestCore()
			{
				var hawb = Factory.New<CusHAWB>();
				return new CusHAWBProcessTaskCollection(hawb);
			}

			public void TestConditions()
			{
				Mawb.CusMAWBProcessTaskCollectionTest.RunTestConditions((ProcessTaskCollection)GetCollectionToTest());
			}
		}
	}
}
