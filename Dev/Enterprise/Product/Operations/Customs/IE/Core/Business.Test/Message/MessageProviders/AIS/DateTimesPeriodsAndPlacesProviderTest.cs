using System;
using System.Collections.Generic;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class DateTimesPeriodsAndPlacesProviderTest : DataProviderTestCase<DateTimesPeriodsAndPlacesProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("EntryInstruction missing", () => new DateTimesPeriodsAndPlacesProvider(null));
		}

		public void TestFirstPlaceOfProcessing()
		{
			AssertType<PlaceOfProcessingProvider>(Provider.FirstPlaceOfProcessing);
		}

		public void TestPlaceOfUseOrProcessing()
		{
			Assert("Should be IReadOnlyCollection<PlaceOfProcessingProvider>", Provider.PlaceOfUseOrProcessing is IReadOnlyCollection<PlaceOfProcessingProvider>);
		}

		public void TestCustomsOfficeOfDischarge()
		{
			AssertEquals("CustomsOfficeOfDischarge", declaration.CustomsOffices.GetOfficeOfDischarge().CY_Data, Provider.CustomsOfficeOfDischarge);
		}

		public void TestSupervisingCustomsOffice()
		{
			AssertEquals("SupervisingCustomsOffice", declaration.CustomsOffices.GetSupervisingOffice().CY_Data, Provider.SupervisingCustomsOffice);
		}

		public void TestPeriodForDischarge()
		{
			var periodForDischarge = Provider.PeriodForDischarge;
			entryInstruction.ZG_PeriodForDischarge = 1;
			entryInstruction.ZG_PeriodForDischargeAutoExtension = true;
			entryInstruction.PeriodForDischargeDetails = "12";

			CombineAssertions(() =>
			{
				AssertType<PeriodForDischargeProvider>("Type", periodForDischarge);
				AssertEquals("Period", "1", periodForDischarge.Period);
				AssertEquals("AutomaticExtension", true, periodForDischarge.AutomaticExtension);
				AssertEquals("Details", "12", periodForDischarge.Details);
			});
		}

		public void TestBillOfDischarge()
		{
			var billOfDischarge = Provider.BillOfDischarge;
			entryInstruction.ZG_BillOfDischargeIsNecessary = true;
			entryInstruction.ZG_BillOfDischargeDeadline = 1;
			entryInstruction.BillOfDischargeDetails = "23";

			CombineAssertions(() =>
			{
				AssertType<BillOfDischargeProvider>("Type", billOfDischarge);
				AssertEquals("Period", true, billOfDischarge.UseOfTheBillOfDischarge);
				AssertEquals("Period", "1", billOfDischarge.Deadline);
				AssertEquals("Period", "23", billOfDischarge.Details);
			});
		}

		protected override DateTimesPeriodsAndPlacesProvider GetProvider()
		{
			return new DateTimesPeriodsAndPlacesProvider(entryInstruction);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.SupervisingOffice, "IE3333333");
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfDischarge, "IE4444444");

			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.PlaceOfUseOrProcessingCollection.AddNew();
		}
		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
	}
}
