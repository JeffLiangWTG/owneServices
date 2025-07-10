using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(WorkflowTransferDiagnosisCollection))]
	class WorkflowTransferDiagnosisCollectionTest : NonPersistentBusinessObjectCollectionTestCase<WorkflowTransferDiagnosisCollection>
	{
		public void TestConstructor_ShouldCreateElementForCurrentComponentLinks()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var bucket1 = system.Components.AddNew();
			bucket1.FC_Name = "bucket1";
			bucket1.FC_DisplaySequence = 1;
			var bucket2 = system.Components.AddNew();
			bucket2.FC_Name = "bucket2";
			bucket2.FC_DisplaySequence = 4;
			var bucket3 = system.Components.AddNew();
			bucket3.FC_Name = "bucket3";
			bucket3.FC_DisplaySequence = 3;
			var buffer = system.Components.AddNew();
			buffer.FC_Name = "buffer";
			buffer.FC_Type = BMComponentTypeList.Codes.Buffer;
			buffer.FC_BufferTimespanInMinutes = 96 * 60;
			buffer.FC_DisplaySequence = 2;

			var link1_2 = bucket1.FromMeToOthersLinks.AddNew();
			link1_2.FL_FC_ComponentTo = bucket2.PK;
			var link1_3 = bucket1.FromMeToOthersLinks.AddNew();
			link1_3.FL_FC_ComponentTo = bucket3.PK;
			var link1_buffer = bucket1.FromMeToOthersLinks.AddNew();
			link1_buffer.FL_FC_ComponentTo = buffer.PK;

			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders.AddNew();
			var collection = new WorkflowTransferDiagnosisCollection(workflow);

			AssertEquals(3, collection.Count);
			AssertEquals(buffer.FC_Name, collection[0].ComponentToName);
			AssertEquals(bucket3.FC_Name, collection[1].ComponentToName);
			AssertEquals(bucket2.FC_Name, collection[2].ComponentToName);
		}

		protected override WorkflowTransferDiagnosisCollection GetCollectionToTest()
		{
			return new WorkflowTransferDiagnosisCollection(ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders.AddNew());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var bucket1 = system.Components.AddNew();
			bucket1.FC_Name = "bucket1";
			var bucket2 = system.Components.AddNew();
			bucket2.FC_Name = "bucket2";

			var link = bucket1.FromMeToOthersLinks.AddNew();
			link.FL_FC_ComponentTo = bucket2.PK;

			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders.AddNew();

			return new WorkflowTransferDiagnosis(link, workflow);
		}
	}
}
