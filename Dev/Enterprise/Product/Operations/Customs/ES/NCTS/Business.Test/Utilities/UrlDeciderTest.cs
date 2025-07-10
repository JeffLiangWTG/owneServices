using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class UrlDeciderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null NctsHeader", () => new UrlDecider(null));
		}

		public void TestGetUrlNcts_Departure()
		{
			CombineAssertions(() =>
			{
				var nctsHeader = SetNctsDeclaration(NctsMovementType.Codes.Departure);
				AssertGetUrlNcts(nctsHeader);
			});
		}

		public void TestGetUrlNcts_Arrival_Phase4()
		{
			CombineAssertions(() =>
			{
				var nctsHeader = SetNctsDeclaration(NctsMovementType.Codes.Arrival);
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				AssertGetUrlNcts(nctsHeader);
			});
		}

		public void TestGetUrlNcts_Arrival_Phase5()
		{
			CombineAssertions(() =>
			{
				var nctsHeader = SetNctsDeclaration(NctsMovementType.Codes.Arrival);
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				AssertGetUrlNcts(nctsHeader);
			});
		}

		public void TestGetUrlNcts_DepartureAndArrival()
		{
			CombineAssertions(() =>
			{
				var nctsHeader = SetNctsDeclaration(NctsMovementType.Codes.DepartureAndArrival);
				AssertGetUrlNcts(nctsHeader);
			});
		}

		void AssertGetUrlNcts(NctsHeader nctsHeader)
		{
			var expectedMRN = "AACCRRRRRRNNNNNNNN";
			var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADTR-JDIT/Ncts5Detalle?CLAVE=" + expectedMRN;

			AssertNullOrEmpty("No url was returned when no mrn", new UrlDecider(nctsHeader).GetUrlNcts());

			var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = expectedMRN;
			newEntryNumber.CE_EntryIsSystemGenerated = true;
			AssertEquals("The correct url has been launched", expectedUrl, new UrlDecider(nctsHeader).GetUrlNcts());
		}

		NctsHeader SetNctsDeclaration(ZString nctsMovementType)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(nctsMovementType);
			return nctsHeader;
		}
	}
}
