using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class DeltaIEProcedureWrapperTest : Customs.Business.Testing.DataProviderTestCase<DeltaIEProcedureWrapper>
	{
		protected override DeltaIEProcedureWrapper GetProvider()
		{
			var date1 = ZDateTime.MinSmallDateTimeValue;
			var date2 = ZDateTime.MaxSmallDateTimeValue;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tradeGroup = helper.CreateTradeGroup(Env.CurrentCompany.Country.Code, "TEST1", date1, date2);
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Australia, date1.Date, date2.Date);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Env.CurrentCompany.Country.Code, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			Factory.Save();
			var rateType = helper.CreateNewOrGetExistingRateType(Env.CurrentCompany.Country.Code, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode1 = helper.LoadOrCreateNewCusRateCode(Factory, "DTA", rateType.PK);
			Factory.Save();
			var cusTariff = helper.CreateTariff(Env.CurrentCompany.Country.Code, hsnTariffType.PK, "123456789", date1, date2, "dummy Description 0");
			Factory.Save();

			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "IM", "10", "71", "F61", "", "IMP", "10P");
			procedure.ZZ6_IntoWarehouse = "Y";
			procedure.ZZ6_OutOfWarehouse = "N";
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "123456789";
			invoiceLine.JI_Procedure = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode + procedure.ZZ6_Concession;
			return DeltaIEProcedureWrapper.New(invoiceLine);
		}

		public void TestPreviousProcedure()
		{
			AssertEquals("PreviousProcedure should be equal to procedure 3rd to 4th char.", "71", Provider.PreviousProcedure);
		}

		public void TestRequestedProcedure()
		{
			AssertEquals("RequestedProcedure should be equal to procedure 1st to 2nd char.", "10", Provider.RequestedProcedure);
		}

		public void TestAdditionalProcedure()
		{
			AssertEquals("there should be only one element in AdditionalProcedure.", 1, Provider.AdditionalProcedure.Count);
			AssertEquals("AdditionalProcedure value of the first element should be equal to procedure 5th to 7th. char.", "F61", Provider.AdditionalProcedure.ElementAt(0).AdditionalProcedure);

			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var wrapper2 = DeltaIEProcedureWrapper.New(invoiceLine);

			AssertEquals("AdditionalProcedure should be empty when JI_Procedure has no concession code.", 0, wrapper2.AdditionalProcedure.Count);
		}
	}
}
