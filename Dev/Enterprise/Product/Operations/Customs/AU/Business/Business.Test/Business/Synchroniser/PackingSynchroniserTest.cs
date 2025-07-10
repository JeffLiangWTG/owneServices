using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class PackingSynchroniserTest : Customs.Business.Testing.PackingSynchroniserTest<JobDeclaration>
	{
		public override void TestSychroniseCore()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "KRBUS";
			consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CRUX123456";

			consol.Shipments.Add(shipment);

			declaration.CusContainers.AddNew().CO_ContainerNumber = container.JC_ContainerNum;

			var containerised = shipment.OuterPackLines.AddNew();
			containerised.JL_PackageCount = 10;
			containerised.JL_JC = container.PK;
			containerised.JL_MarksAndNumbers = "MarksAndNumbers1";

			containerised = shipment.OuterPackLines.AddNew();
			containerised.JL_PackageCount = 15;
			containerised.JL_JC = container.PK;
			containerised.JL_MarksAndNumbers = "MarksAndNumbers2";

			var nonContainerised = shipment.OuterPackLines.AddNew();
			nonContainerised.JL_PackageCount = 20;
			nonContainerised.JL_JC = ZGuid.Empty;
			nonContainerised.JL_Calc_ContainerNumber = "";
			nonContainerised.JL_MarksAndNumbers = "MarksAndNumbers3";

			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("There should be two rows", 2, declaration.PackingGroups.Count);

			PackingGroup containerisedPack = null;
			PackingGroup nonContainerisedPack = null;

			foreach (PackingGroup pack in declaration.PackingGroups)
			{
				if (pack.CR_CO_Container.IsEmpty)
				{
					nonContainerisedPack = pack;
				}
				else
				{
					containerisedPack = pack;
				}
			}

			AssertNotNull(containerisedPack);
			AssertNotNull(nonContainerisedPack);

			AssertEquals(25, containerisedPack.TotalNumberOfPackages);
			AssertEquals("MarksAndNumbers1\r\nMarksAndNumbers2", containerisedPack.MarksAndNumbers);

			AssertEquals(20, nonContainerisedPack.TotalNumberOfPackages);
			AssertEquals("MarksAndNumbers3", nonContainerisedPack.MarksAndNumbers);
		}

		[ExpectNoExceptions]
		public void TestSyncroniseDoesNotCauseConstraintViolationOnPackPivot()
		{
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 10;
			declaration.Bills.RemoveAndDeleteAll();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.ShipmentSynchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			shipment.PackLineSynchroniser.MarkSyncDirty();
			DoPackSyncronise();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			AssertEquals("PreCondition:One house bill is added", 1, declaration.Bills.Count);
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var shipmentInFactory2 = factory2.Load<ForwardingShipment>(shipment.PK);
			AssertEquals("Only 1 declaration on shipment", 1, shipmentInFactory2.Declarations.Length);
			var declarationInFactory2 = (JobDeclaration)shipmentInFactory2.Declarations[0];
			declarationInFactory2.ShipmentSynchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			DoPackSyncronise();
			declarationInFactory2.ThrowAwayMerge();
			factory2.Save();
		}

		protected override JobDeclaration GetDeclarationPackingRelevant()
		{
			var result = Factory.New<JobDeclaration>();
			result.JE_MessageType = JobMessageTypeList.Codes.Import;
			return result;
		}

		protected override JobDeclaration GetDeclarationPackingNotRelevant()
		{
			var result = Factory.New<JobDeclaration>();
			result.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			return result;
		}
	}
}
