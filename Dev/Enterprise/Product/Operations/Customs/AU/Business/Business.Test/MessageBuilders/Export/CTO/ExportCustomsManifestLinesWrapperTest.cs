using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ExportCustomsManifestLinesWrapperTest : TestCaseWithFactory
	{
		public void TestIsAir()
		{
			AssertEquals(true, wrapper.IsAir);
		}

		public void TestIsSea()
		{
			AssertEquals(false, wrapper.IsSea);
		}

		public void TestExportDeclarationExceptionCode()
		{
			line.EL_TypeOfCAN = CANType.Exemptions.EXDC.Code;
			line.EL_CAN = "mmmbacon";
			AssertEquals(CANType.Exemptions.EXDC.Code, wrapper.ExportDeclarationExemptionCode);
			AssertEquals(ZString.Empty, wrapper.CustomsAuthorityNumber);
			AssertEquals(ZString.Empty, wrapper.CustomsContingencyAuthorityNumber);
		}

		public void TestCarrierPartID()
		{
			AssertEquals(ZString.Empty, wrapper.CarrierPartyID);
		}

		public void TestCustomsAuthorityNumber()
		{
			line.EL_CAN = "foo";
			AssertEquals("foo", wrapper.CustomsAuthorityNumber);
			AssertEquals(ZString.Empty, wrapper.ExportDeclarationExemptionCode);
			AssertEquals(ZString.Empty, wrapper.CustomsContingencyAuthorityNumber);
		}

		public void TestCustomsContingencyAuthorityNumber()
		{
			line.EL_CAN = "bar";
			line.EL_TypeOfCAN = CANType.ContingencyCustomsAuthorityNumber.Code;
			AssertEquals("bar", wrapper.CustomsContingencyAuthorityNumber);
			AssertEquals(ZString.Empty, wrapper.CustomsAuthorityNumber);
			AssertEquals(ZString.Empty, wrapper.ExportDeclarationExemptionCode);
		}

		public void TestCountryOfDestination()
		{
			line.EL_RN_NKCountryOfDestination = Core.Constants.CountryCodes.NewZealand;
			AssertEquals(Core.Constants.CountryCodes.NewZealand, wrapper.CountryOfDestination);
		}

		public void TestGoodsOwnerPartyID()
		{
			line.EL_GoodsOwnerPartyID = "foosh";
			AssertEquals("foosh", wrapper.GoodsOwnerPartyID);
		}

		public void TestOffloadIndicator()
		{
			AssertEquals(false, wrapper.OffloadIndicator);
		}

		public void TestOwnerName()
		{
			line.EL_GoodsOwner = "monkeys";
			AssertEquals("monkeys", wrapper.OwnerName);
		}

		public void TestProposedDateOfDeparture()
		{
			AssertEquals(ZDateTime.Empty, wrapper.ProposedDateOfDeparture);
		}

		public void TestGoodsDescription()
		{
			line.EL_GoodsDescription = "green hats";
			AssertEquals("green hats", wrapper.GoodsDescription);
		}

		public void TestAirWaybill()
		{
			line.EL_AirWayBill = "1234118844";
			AssertEquals("1234118844", wrapper.AirWaybill);
		}

		public void TestVesselID()
		{
			AssertEquals(ZString.Empty, wrapper.VesselID);
		}

		public void TestVoyageNumber()
		{
			AssertEquals(ZString.Empty, wrapper.VoyageNumber);
		}

		public void TestContainerNumber()
		{
			AssertEquals(ZString.Empty, wrapper.ContainerNumber);
		}

		public void TestNonContainerisedIdentifier()
		{
			AssertEquals(ZString.Empty, wrapper.NonContainerisedIdentifier);
		}

		public void TestDontAcceptNullLine()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ExportCustomsManifestLinesWrapper(null));
		}

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<AirCTOExportCustomsManifestHeader>();
			line = header.Lines.AddNew();
			wrapper = new ExportCustomsManifestLinesWrapper(line);
		}

		ExportCustomsManifestLines line;
		ExportCustomsManifestLinesWrapper wrapper;
	}
}
