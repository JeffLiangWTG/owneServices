using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(ProcessTaskCollection<NctsArrivalMovementHeaderProcessTask, NctsArrivalMovementHeader>))]
	class NctsArrivalMovementHeaderTaskCollectionTest : ProcessTaskCollectionTest<ProcessTaskCollection<NctsArrivalMovementHeaderProcessTask, NctsArrivalMovementHeader>>
	{
		protected override ProcessTaskCollection<NctsArrivalMovementHeaderProcessTask, NctsArrivalMovementHeader> GetCollectionToTestCore()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var arrivalHeader = nctsHeader.ArrivalMovementHeader;
			return (ProcessTaskCollection<NctsArrivalMovementHeaderProcessTask, NctsArrivalMovementHeader>)arrivalHeader.WorkflowItems;
		}
	}

	[TestedType(typeof(NctsArrivalMovementHeaderProcessTask))]
	class NctsArrivalMovementHeaderProcessTaskTest : ProcessTaskTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return WorkflowItems.AddNew();
		}

		ProcessTaskCollection WorkflowItems
		{
			get { return ((IWorkflowProvider)ArrivalHeader).WorkflowItems; }
		}

		NctsArrivalMovementHeader ArrivalHeader
		{
			get
			{
				if (arrivalHeader == null)
				{
					var nctsHeader = Factory.New<NctsHeader>();
					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
					arrivalHeader = nctsHeader.ArrivalMovementHeader;
				}

				return arrivalHeader;
			}
		}
		NctsArrivalMovementHeader arrivalHeader;
	}

	[TestedType(typeof(NctsArrivalMovementHeader))]
	class NctsArrivalMovementHeaderWorkflowProviderTest : WorkflowProviderTest<NctsArrivalMovementHeader, ProcessTaskCollection<NctsArrivalMovementHeaderProcessTask, NctsArrivalMovementHeader>>
	{
		public void TestEverything()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			IWorkflowProvider arrival = header.ArrivalMovementHeader;
			AssertType(typeof(ProcessTaskCollection<NctsArrivalMovementHeaderProcessTask, NctsArrivalMovementHeader>), arrival.WorkflowItems);
			AssertType(typeof(ColumnValueRanker), arrival.GetTemplateSelectionCriteria());
		}

		public void TestBoolFieldsInSelectionCriteria_DoNotThrow_ArgumentException()
		{
			using (WorkflowDataRegistry.Instance.EnableTemplateApplicationInMemoryFiltering.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var departureHeader1 = Factory.New<NctsHeader>();
				departureHeader1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				departureHeader1.SetMovementType(NctsMovementType.Codes.Arrival);
				departureHeader1.ArrivalMovementHeader.BM_StateOfSealsBoolean = false;

				AssertNoExceptionThrown("Failure on P0_SubType5 CargoWise.Schema.SchemaStringColumn is not assignable from System.Boolean", Factory.Save);
			}
		}

		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.NctsArrivalMovementHeaderWorkflowDescriptor; }
		}
	}

	[TestedType(typeof(NctsArrivalMovementHeaderWorkflowDescriptor))]
	public class NctsHeaderArrivalWorkflowDescriptorTest : WorkflowDescriptorTestCase<NctsArrivalMovementHeaderWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.NctsArrivalMovementHeaderWorkflowDescriptor, WorkflowDescriptor.Code);
		}

		public void TestClientName()
		{
			AssertEquals("Client", WorkflowDescriptor.ClientName);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "NCTS Arrival", WorkflowDescriptor.Description);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsEventTracking);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestSubTypes()
		{
			AssertEquals("5 sub type", 5, WorkflowDescriptor.SubTypeInformation.Length);
			AssertEquals("Incident Flag", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertEquals("Simplified Type", WorkflowDescriptor.SubTypeInformation[1].Description);
			AssertEquals("Destination Office Code", WorkflowDescriptor.SubTypeInformation[2].Description);
			AssertEquals("Conforms", WorkflowDescriptor.SubTypeInformation[3].Description);
			AssertEquals("State Of Seals", WorkflowDescriptor.SubTypeInformation[4].Description);
		}

		public override void TestSupportsTasks()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsTasks);
		}

		public void TestSupportsBufferManagement()
		{
			AssertEquals("NctsHeaderArrivalWorkflowDescriptor should support BufferManagement.", false, WorkflowDescriptor.SupportsBufferManagement);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var nctsHeader = Factory.New<NctsHeaderPhase5ForTest>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var arrivalHeader = nctsHeader.ArrivalMovementHeader;
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			return new[] { arrivalHeader };
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return MessageRecipientPartyType.Email | MessageRecipientPartyType.OrgProxy;
			}
		}

		protected override BusinessObject NewBusinessObjectInTable(ITableSchema table)
		{
			BusinessObject result = null;
			var header = Factory.NewWithValidTestData<NctsHeader>();
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			switch (table.TableName)
			{
				case CusInBondHeader.Schema.TableName:
					result = header;
					break;
				case CusInBondMoveHeader.Schema.TableName:
					result = header.MovementHeader;
					break;
				default:
					result = base.NewBusinessObjectInTable(table);
					break;
			}
			return result;
		}
	}
}
