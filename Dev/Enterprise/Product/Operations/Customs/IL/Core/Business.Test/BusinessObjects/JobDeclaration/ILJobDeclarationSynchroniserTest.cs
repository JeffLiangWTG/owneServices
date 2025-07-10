using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILJobDeclarationSynchroniserTest : SynchroniserTestCase
	{
		public void TestSynchronization()
		{
			var shipment = CreateTestShipment();

			var declaration = CreateTestDeclaration(shipment);

			var consol1 = shipment.Consols.AddNew();
			CreateConsolTestData(consol1, "1", "AUSYD", "ITVCE", "ILASH");

			var consol2 = shipment.Consols.AddNew();
			CreateConsolTestData(consol2, "2", "AUSYD", "ITVCE", "ILTLV");

			Factory.Save();

			var synchronizer = declaration.ShipmentSynchroniser;
			synchronizer.SetEnabled(true, false);

			AssertSynchronizedData(declaration, "Initial", "C0001", "S0001", "ROUTE1_2");

			var fdn = shipment.Numbers.Cast<CusEntryNumber>().FirstOrDefault(n => n.CE_EntryType == IsraelShipmentAdditionalReferenceNumberTypes.Codes.ForwarderDealNumber);

			fdn.CE_EntryNum = "S0001A";
			AssertSynchronizedData(declaration, "When FDN value changed", "C0001", "S0001A", "ROUTE1_2");

			fdn.CE_EntryType = "XYZ";
			AssertSynchronizedData(declaration, "When FDN type changed", "C0001", "", "ROUTE1_2");

			var pdn = consol1.Numbers.Cast<CusEntryNumber>().FirstOrDefault(n => n.CE_EntryType == IsraelConsolAdditionalReferenceNumberTypes.Codes.ParentDealNumber);

			pdn.CE_EntryNum = "C0001A";
			Factory.Save();
			synchronizer.Synchronise(true);
			AssertSynchronizedData(declaration, "When PDN value changed", "C0001A", "", "ROUTE1_2");

			pdn.CE_EntryType = "XYZ";
			Factory.Save();
			synchronizer.Synchronise(true);
			AssertSynchronizedData(declaration, "When PDN type changed", "", "", "ROUTE1_2");

			fdn.CE_EntryType = IsraelShipmentAdditionalReferenceNumberTypes.Codes.ForwarderDealNumber;
			pdn.CE_EntryType = IsraelConsolAdditionalReferenceNumberTypes.Codes.ParentDealNumber;
			Factory.Save();
			synchronizer.Synchronise(true);

			AssertSynchronizedData(declaration, "Prereq: FDN and PDB restored", "C0001A", "S0001A", "ROUTE1_2");

			shipment.Numbers.RemoveAndDelete(fdn);
			AssertSynchronizedData(declaration, "FDN deleted", "C0001A", "", "ROUTE1_2");

			consol1.Numbers.RemoveAndDelete(pdn);
			Factory.Save();
			synchronizer.Synchronise(true);
			AssertSynchronizedData(declaration, "PDN deleted", "", "", "ROUTE1_2");
		}

		public void TestSynchronization_WhenConsolDeleted()
		{
			var shipment = CreateTestShipment();

			var declaration = CreateTestDeclaration(shipment);

			var consol1 = shipment.Consols.AddNew();
			CreateConsolTestData(consol1, "1", "AUSYD", "ITVCE", "ILASH");

			var consol2 = shipment.Consols.AddNew();
			CreateConsolTestData(consol2, "2", "AUSYD", "ITVCE", "ILTLV");

			Factory.Save();

			var synchronizer = declaration.ShipmentSynchroniser;
			synchronizer.SetEnabled(true, true);

			AssertSynchronizedData(declaration, "Prereq", "C0001", "S0001", "ROUTE1_2");

			shipment.Consols.RemoveAndDelete(consol1);

			AssertSynchronizedData(declaration, "After Consol Removal", "C0002", "S0001", "ROUTE2_2");
		}

		public void TestSynchronization_WhenConsolDestinationPortChanged()
		{
			var shipment = CreateTestShipment();

			var declaration = CreateTestDeclaration(shipment);

			var consol1 = shipment.Consols.AddNew();
			CreateConsolTestData(consol1, "1", "AUSYD", "ITVCE", "ILASH");

			var consol2 = shipment.Consols.AddNew();
			CreateConsolTestData(consol2, "2", "AUSYD", "ITVCE", "ILTLV");

			Factory.Save();

			var synchronizer = declaration.ShipmentSynchroniser;
			synchronizer.SetEnabled(true, true);

			AssertSynchronizedData(declaration, "Prereq", "C0001", "S0001", "ROUTE1_2");

			consol1.JK_RL_NKDischargePort = "DEHAM";

			AssertSynchronizedData(declaration, "After Consol port changed", "C0002", "S0001", "ROUTE2_2");
		}

		public void TestSynchronization_WhenConsolTransportsChanged()
		{
			var shipment = CreateTestShipment();

			var declaration = CreateTestDeclaration(shipment);

			var consol1 = shipment.Consols.AddNew();
			CreateConsolTestData(consol1, "1", "AUSYD", "ITVCE", "ILASH");

			var consol2 = shipment.Consols.AddNew();
			CreateConsolTestData(consol2, "2", "AUSYD", "ITVCE", "DEHAM");

			Factory.Save();

			var synchronizer = declaration.ShipmentSynchroniser;
			synchronizer.SetEnabled(true, true);
			AssertSynchronizedData(declaration, "Prereq", "C0001", "S0001", "ROUTE1_2");

			var transport = consol1.Transports.AddNew();
			transport.JW_TransportMode = TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "ILASH";
			transport.JW_RL_NKDiscPort = "ILTLV";
			transport.JW_ArrivalPortRouteId = $"ROUTE1_3";

			AssertSynchronizedData(declaration, "After transport added but console port remained", "C0001", "S0001", "ROUTE1_2");

			consol1.JK_RL_NKDischargePort = "ILTLV";

			AssertSynchronizedData(declaration, "After transport added and console port aligned", "C0001", "S0001", "ROUTE1_3");

			consol1.Transports.RemoveAndDelete(transport);

			AssertSynchronizedData(declaration, "After transport deleted but console port remained", "C0001", "S0001", "");

			consol1.JK_RL_NKDischargePort = "ILASH";

			AssertSynchronizedData(declaration, "After transport deleted and console port aligned", "C0001", "S0001", "ROUTE1_2");
		}

		public void TestSynchronization_IssueDate()
		{
			var shipment = CreateTestShipment();

			var declaration = CreateTestDeclaration(shipment);

			shipment.JS_HouseBillIssueDate = issueDate;
			Factory.Save();

			var synchronizer = declaration.ShipmentSynchroniser;
			synchronizer.SetEnabled(true, false);

			AssertEquals("Issue Date should be equal to the expected value", issueDate, declaration.JE_MasterBillIssuedDate);

			var issueDate2 = issueDate.AddDays(10);
			shipment.JS_HouseBillIssueDate = issueDate2;
			synchronizer.Synchronise(true);

			AssertEquals("Updated Issue Date should be equal to the expected value", issueDate2, declaration.JE_MasterBillIssuedDate);
		}

		ForwardingShipment CreateTestShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			var fdn1 = shipment.Numbers.AddNew();
			fdn1.CE_EntryType = IsraelShipmentAdditionalReferenceNumberTypes.Codes.ForwarderDealNumber;
			fdn1.CE_EntryNum = "S0001";
			return shipment;
		}

		JobDeclaration CreateTestDeclaration(ForwardingShipment shipment)
		{
			var declaration = Factory.New<JobDeclarationForTest>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_TransportMode = TransportModes.Sea;
			declaration.JE_CustomsLoadPort = "AUSYD";
			declaration.JE_CustomsDischargePort = "ILASH";
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			return declaration;
		}

		static void CreateConsolTestData(ForwardingConsol consol, string prefix, params string[] routePorts)
		{
			AssertNotNull("Route ports must be provided", routePorts);
			Assert("Route ports must contain at least 2 ports", routePorts.Length >= 2);

			consol.JK_TransportMode = TransportModes.Sea;
			var pdn1 = consol.Numbers.AddNew();
			pdn1.CE_EntryType = IsraelConsolAdditionalReferenceNumberTypes.Codes.ParentDealNumber;
			pdn1.CE_EntryNum = $"C000{prefix}";

			for (int i = 0; i < routePorts.Length - 1; i++)
			{
				var transport = i == 0 ? consol.Transports[0] : consol.Transports.AddNew();
				transport.JW_TransportMode = TransportModes.Sea;
				transport.JW_RL_NKLoadPort = routePorts[i];
				transport.JW_RL_NKDiscPort = routePorts[i + 1];
				transport.JW_ArrivalPortRouteId = $"ROUTE{prefix}_{i + 1}";
			}

			consol.JK_RL_NKLoadPort = routePorts[0];
			consol.JK_RL_NKDischargePort = routePorts[routePorts.Length - 1];
		}

		static void AssertSynchronizedData(JobDeclaration declaration, string assertionTitle, ZString expectedPdn, ZString expectedFdn, ZString expectedManifestNumber)
		{
			var destPdn = declaration.AdditionalReferenceNumbers.Cast<CusEntryNumber>().FirstOrDefault(n => n.CE_EntryType == IsraelConsolAdditionalReferenceNumberTypes.Codes.ParentDealNumber);
			var destFdn = declaration.AdditionalReferenceNumbers.Cast<CusEntryNumber>().FirstOrDefault(n => n.CE_EntryType == IsraelShipmentAdditionalReferenceNumberTypes.Codes.ForwarderDealNumber);

			CombineAssertions(assertionTitle, () =>
			{
				if (expectedPdn.IsEmpty)
				{
					AssertNull("PDN must not exist in declaration", destPdn);
				}
				else
				{
					AssertNotNull("PDN must exist in declaration", destPdn);
					AssertEquals("PDN should be equal to the expected value", expectedPdn, destPdn.CE_EntryNum);
				}

				if (expectedFdn.IsEmpty)
				{
					AssertNull("FDN must not exist in declaration", destFdn);
				}
				else
				{
					AssertNotNull("FDN must exist in declaration", destFdn);
					AssertEquals("FDN should be equal to the expected value", expectedFdn, destFdn.CE_EntryNum);
				}

				AssertEquals("Manifet Number should be equal to the expected value", expectedManifestNumber, declaration.JE_ManifestNumber);
			});
		}

		readonly ZDateTime issueDate = ZDateTime.FromSqlFormat("2025-01-01 00:00:00.000");
	}

	public class ILJobDeclarationSynchroniserForTest : ILJobDeclarationSynchroniser
	{
		public ILJobDeclarationSynchroniserForTest(BaseJobDeclaration declaration) : base(declaration)
		{
		}

		protected override void ForceSynchroniseCore()
		{
			UnHookConsolToDeclarationSynchronisers();
			HookConsolToDeclarationSynchronisers();

			base.ForceSynchroniseCore();
		}
	}

	public class JobDeclarationForTest : JobDeclaration
	{
		public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override JobDeclarationSynchroniser GetNewShipmentSynchroniser() => new ILJobDeclarationSynchroniserForTest(this);
	}
}
