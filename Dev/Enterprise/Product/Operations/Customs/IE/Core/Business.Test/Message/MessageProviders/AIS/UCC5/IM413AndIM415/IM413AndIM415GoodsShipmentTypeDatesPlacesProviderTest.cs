using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	public class IM413AndIM415GoodsShipmentTypeDatesPlacesProviderTest : DataProviderTestCase<IM413AndIM415GoodsShipmentTypeDatesPlacesProvider>
	{
		public void TestCountryDestination()
		{
			SetUpTestData();
			declaration.JE_GoodsDestination = "FR";
			var destination = Provider.CountryDestination;
			AssertEquals("CountryOfDestination", "FR", destination);
			AssertSame("Cached", destination, Provider.CountryDestination);
		}

		public void TestRegionDestination()
		{
			AssertNull(Provider.RegionDestination);
		}

		public void TestCountryDispatch()
		{
			SetUpTestData();
			declaration.JE_GoodsOrigin = "AU";
			var countryDispatch = Provider.CountryDispatch;
			AssertEquals("CountryOfDestination", "AU", countryDispatch);
			AssertSame("Cached", countryDispatch, Provider.CountryDispatch);
		}

		public void TestLocationGoods()
		{
			SetUpTestData();
			var locationGoods = Provider.LocationGoods;
			AssertNotNull(locationGoods);
			AssertSame("Cached", locationGoods, Provider.LocationGoods);
		}

		public void TestAcceptanceDate()
		{
			SetUpTestData();

			AssertNull("DateOfAcceptance not set", Provider.AcceptanceDate);

			instruction.CEI_DateForDuty = new ZDateTime(2023, 1, 1);
			instruction.CEI_SubStyle = EU.Business.EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF;
			AssertEquals("CEI_SubStyle = Y", new ZDateTime(2023, 1, 1), Provider.AcceptanceDate);

			instruction.CEI_DateForDuty = new ZDateTime(2023, 2, 2);
			instruction.CEI_SubStyle = EU.Business.EntrySubStyleList.Codes.NormalDeclaration;
			AssertNull("CEI_SubStyle = A", Provider.AcceptanceDate);

			instruction.CEI_DateForDuty = new ZDateTime(2023, 3, 3);
			instruction.CEI_SubStyle = EU.Business.EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic;
			AssertEquals("CEI_SubStyle = Z", new DateTime(2023, 3, 3), Provider.AcceptanceDate);
		}

		protected override IM413AndIM415GoodsShipmentTypeDatesPlacesProvider GetProvider()
		{
			SetUpTestData();
			return new IM413AndIM415GoodsShipmentTypeDatesPlacesProvider(new EntryHeaderWrapper(entryHeader));
		}

		void SetUpTestData()
		{
			if (entryHeader == null)
			{
				declaration = Factory.New<JobDeclaration>();
				instruction = declaration.CustomsEntryInstructions.AddNew();
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = instruction.PK;
			}
		}

		JobDeclaration declaration;
		CusEntryInstruction instruction;
		CusEntryHeader entryHeader;
	}
}
