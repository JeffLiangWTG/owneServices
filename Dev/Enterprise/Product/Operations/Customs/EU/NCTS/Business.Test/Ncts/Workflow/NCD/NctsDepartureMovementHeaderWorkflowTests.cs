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
	[TestedType(typeof(ProcessTaskCollection<NctsDepartureMovementHeaderProcessTask, NctsDepartureMovementHeader>))]
	class NctsDepartureMovementHeaderProcessTaskCollectionTest : ProcessTaskCollectionTest<ProcessTaskCollection<NctsDepartureMovementHeaderProcessTask, NctsDepartureMovementHeader>>
	{
		protected override ProcessTaskCollection<NctsDepartureMovementHeaderProcessTask, NctsDepartureMovementHeader> GetCollectionToTestCore()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var moveHeader = nctsHeader.MovementHeader;
			return (ProcessTaskCollection<NctsDepartureMovementHeaderProcessTask, NctsDepartureMovementHeader>)moveHeader.WorkflowItems;
		}
	}

	[TestedType(typeof(NctsDepartureMovementHeaderProcessTask))]
	class NctsDepartureMovementHeaderProcessTaskTest : ProcessTaskTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return WorkflowItems.AddNew();
		}

		ProcessTaskCollection WorkflowItems
		{
			get { return ((IWorkflowProvider)MoveHeader).WorkflowItems; }
		}

		NctsDepartureMovementHeader MoveHeader
		{
			get
			{
				if (moveheader == null)
				{
					var nctsHeader = Factory.New<NctsHeader>();
					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
					moveheader = nctsHeader.MovementHeader;
				}

				return moveheader;
			}
		}
		NctsDepartureMovementHeader moveheader;
	}

	[TestedType(typeof(NctsDepartureMovementHeader))]
	class NctsDepartureMovementHeaderWorkflowProviderTest : WorkflowProviderTest<NctsDepartureMovementHeader, ProcessTaskCollection<NctsDepartureMovementHeaderProcessTask, NctsDepartureMovementHeader>>
	{
		public void TestEverything()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			IWorkflowProvider departure = header.MovementHeader;
			AssertType(typeof(ProcessTaskCollection<NctsDepartureMovementHeaderProcessTask, NctsDepartureMovementHeader>), departure.WorkflowItems);
			AssertType(typeof(ColumnValueRanker), departure.GetTemplateSelectionCriteria());
		}

		public void TestBoolFieldsInSelectionCriteria_DoNotThrow_ArgumentException()
		{
			using (WorkflowDataRegistry.Instance.EnableTemplateApplicationInMemoryFiltering.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var departureHeader1 = Factory.New<NctsHeader>();
				departureHeader1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				departureHeader1.SetMovementType(NctsMovementType.Codes.Departure);
				departureHeader1.MovementHeader.BM_GONumber = NctsControlResult.Codes.AuthorizedTrader;

				AssertNoExceptionThrown("Failure on P0_SubType4 CargoWise.Schema.SchemaStringColumn is not assignable from System.Boolean", Factory.Save);
			}
		}

		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.NctsDepartureMovementHeaderWorkflowDescriptor; }
		}
	}

	[TestedType(typeof(NctsDepartureMovementHeaderWorkflowDescriptor))]
	public class NctsHeaderDepartureWorkflowDescriptorTest : WorkflowDescriptorTestCase<NctsDepartureMovementHeaderWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.NctsDepartureMovementHeaderWorkflowDescriptor, WorkflowDescriptor.Code);
		}

		public void TestClientName()
		{
			AssertEquals("Client", WorkflowDescriptor.ClientName);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "NCTS Departure", WorkflowDescriptor.Description);
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
			AssertEquals("Declaration Type", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertEquals("Additional Declaration type", WorkflowDescriptor.SubTypeInformation[1].Description);
			AssertEquals("Inland Transport Mode At Departure", WorkflowDescriptor.SubTypeInformation[2].Description);
			AssertEquals("Is Simplified NCTS Procedure", WorkflowDescriptor.SubTypeInformation[3].Description);
			AssertEquals("Destination Ctry./Rgn.", WorkflowDescriptor.SubTypeInformation[4].Description);
		}

		public override void TestSupportsTasks()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsTasks);
		}

		public void TestSupportsBufferManagement()
		{
			AssertEquals("NctsHeaderDepartureWorkflowDescriptor should support BufferManagement.", false, WorkflowDescriptor.SupportsBufferManagement);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var nctsHeader = Factory.New<NctsHeaderPhase5ForTest>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var moveHeader = nctsHeader.GetNewMovementHeaderExposed();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			return new[] { moveHeader };
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
			header.SetMovementType(NctsMovementType.Codes.Departure);
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
