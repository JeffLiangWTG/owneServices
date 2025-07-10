using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.GPM;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class MessageGatePassMovementDetailsWrapperTest : Customs.Business.Testing.DataProviderTestCase<IMessageGatePassMovementDetails>
	{
		public void TestNewOrNull()
		{
			AssertNull("When gatePassMovement is null", MessageDeliveryOrderWrapper.NewOrNull(null, false));
			AssertNotNull("When gatePassMovement s not null", Provider);
		}

		public void TestExportFromDifferentPortIndication()
		{
			AssertEquals(false, Provider.ExportFromDifferentPortIndication);
		}

		public void TestCargoTypeCode()
		{
			CreateCargoTypesMap();

			AssertCargoTypeCode(1, "FCL");
			AssertCargoTypeCode(1, "LCL");
			AssertCargoTypeCode(2, "ROR");
			AssertCargoTypeCode(3, "BLK");
			AssertCargoTypeCode(4, "LQD");
		}

		public void TestGatepassNumber()
		{
			AssertEquals(0, Provider.GatepassNumber);
		}

		public void TestOriginSiteCode()
		{
			AssertEquals("ITMIL", Provider.OriginSiteCode);
		}

		public void TestProcessTypeCode()
		{
			AssertEquals(1, Provider.ProcessTypeCode);
		}

		[TestDate(2024, 09, 01)]
		public void TestRequestDate()
		{
			AssertEquals(new ZDate(2024, 09, 01), Provider.RequestDate);
		}

		public void TestCustomerActivityType()
		{
			AssertEquals(5, Provider.CustomerActivityType);
		}

		public void TestUpdateCode()
		{
			AssertEquals(1, Provider.UpdateCode);

			var shipment = Factory.New<ForwardingShipment>();
			var gatePassMovement = new GatePassMovementDocDataObject("ForwardingShipment", "S0001001", Factory);

			var wrapper = MessageGatePassMovementDetailsWrapper.NewOrNull(gatePassMovement, true) as IMessageGatePassMovementDetails;
			AssertEquals(2, wrapper.UpdateCode);
		}

		public void TestCargoIdentifier()
		{
			AssertNotNull(Provider.CargoIdentifier);
		}

		public void TestGatepassDestinationSite()
		{
			AssertEquals(1, Provider.GatepassDestinationSite.Count);
			AssertType<MessageGatePassMovementDestinationSiteWrapper>(Provider.GatepassDestinationSite.Single());
		}

		public void TestEscortingCustomerId()
		{
			AssertNull(Provider.EscortingCustomerId);
		}

		public void TestEscortingCustomerType()
		{
			AssertNull(Provider.EscortingCustomerType);
		}

		public void TestIMessageGatePassMovement()
		{
			AssertNull(Provider.InternalPackingQuantity);
		}

		public void TestBackToPortIndication()
		{
			AssertNull(Provider.BackToPortIndication);
		}

		public void TestTpgIdentifier()
		{
			AssertNull(Provider.TpgIdentifier);
		}

		public void TestCargoIdentityInDestinationPort()
		{
			AssertNull(Provider.CargoIdentityInDestinationPort);
		}

		public void TestInternalPackingQuantity()
		{
			AssertNull(Provider.InternalPackingQuantity);
		}

		public void TestExternalId()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Israel))
			{
				AssertNull("Prereq", GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(s => s.OK_CodeType == "VAT"));
				AssertEquals("ExternalId should be empty when no VAT", ZString.Empty, Provider.ExternalId);

				GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew("VAT", "554433");
				AssertEquals("ExternalId should be equal to VAT", "554433", Provider.ExternalId);
			}
		}

		public void TestOriginalExportDeliveryDocumentIdentification()
		{
			AssertNull(Provider.OriginalExportDeliveryDocumentIdentification);
		}

		protected override IMessageGatePassMovementDetails GetProvider()
		{
			return GetProvider("3");
		}

		IMessageGatePassMovementDetails GetProvider(ZString cargoType)
		{
			var shipment = Factory.New<ForwardingShipment>();
			var gatePassMovement = new GatePassMovementDocDataObject("ForwardingShipment", "S0001001", Factory);
			gatePassMovement.CargoType = new CodeDescription(new CodeDescriptionPairList());
			gatePassMovement.CargoType.Code = cargoType;
			gatePassMovement.OriginSite = new CodeDescription(new CodeDescriptionPairList());
			gatePassMovement.OriginSite.Code = "ITMIL";
			gatePassMovement.ProcessType = "1";

			return MessageGatePassMovementDetailsWrapper.NewOrNull(gatePassMovement, false);
		}

		void AssertCargoTypeCode(int expected, ZString cargoType)
		{
			var provider = GetProvider(cargoType);
			AssertEquals("CargoType should be mapped correctly", expected, provider.CargoTypeCode);
		}

		void CreateCargoTypesMap()
		{
			var factory = Factory;
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);
			helper.CreateCusMapType("CRGTY", "OUT", "Cargo Types", true);
			helper.CreateCusMap("CRGTY", "FCL", "1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, Core.Constants.CountryCodes.Israel);
			helper.CreateCusMap("CRGTY", "LCL", "1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, Core.Constants.CountryCodes.Israel);
			helper.CreateCusMap("CRGTY", "ROR", "2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, Core.Constants.CountryCodes.Israel);
			helper.CreateCusMap("CRGTY", "BLK", "3", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, Core.Constants.CountryCodes.Israel);
			helper.CreateCusMap("CRGTY", "LQD", "4", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, Core.Constants.CountryCodes.Israel);
			factory.Save();
		}
	}
}
