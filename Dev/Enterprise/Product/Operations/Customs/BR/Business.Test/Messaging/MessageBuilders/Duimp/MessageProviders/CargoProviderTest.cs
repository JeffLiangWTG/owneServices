using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Duimp.Testing
{
	class CargoProviderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertNull(CargoProvider.New(null));
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertType<CargoProvider>(CargoProvider.New(entryInstruction));
		}

		public void TestProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var dataProvider = CargoProvider.New(entryInstruction);
			Assert(dataProvider.Identification.IsEmpty());
			Assert(dataProvider.DeclaredUnitCode.IsEmpty());
			Assert(dataProvider.DispatchModality.IsEmpty());

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_DispatchModality = DispatchModalityCodes.Codes.Normal;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.BillNumber = "123456789";
			declaration.JE_CustomsOffice = "0000100";
			dataProvider = CargoProvider.New(instruction);

			AssertEquals("123456789", dataProvider.Identification);
			AssertEquals("0000100", dataProvider.DeclaredUnitCode);
			AssertEquals("1", dataProvider.DispatchModality);
			AssertNullOrEmpty("TypeOfIdentification should be empty", dataProvider.TypeOfIdentification);
			AssertNull("Freight should not be empty", dataProvider.Freight);
		}

		public void TestTypeOfIdentification()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var dataProvider = CargoProvider.New(entryInstruction);

			declaration.JE_TransportMode = ZString.Empty;
			AssertEquals("TypeOfIdentification should be empty when JE_TransportMode is empty", ZString.Empty, dataProvider.TypeOfIdentification);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("TypeOfIdentification should be CE when TransportMode=Sea, because BillType will be = HBL", Constants.CargoIdentificationType.CE, dataProvider.TypeOfIdentification);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("TypeOfIdentification should be RUC when TransportMode=Air, because BillType will be = UCR", Constants.CargoIdentificationType.RUC, dataProvider.TypeOfIdentification);
		}

		public void TestInsurance()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var dataProvider = CargoProvider.New(instruction);
			AssertNull("Insurance should be null when JE_DispatchModality is empty", dataProvider.Insurance);

			declaration.JE_DispatchModality = DispatchModalityCodes.Codes.Normal;
			AssertEquals(0d, dataProvider.Insurance.Amount);
			AssertNull(dataProvider.Insurance.CurrencyCode);

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var insurance = invoiceLine.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance);
			insurance.J7_Amount = 56m;
			insurance.J7_RX_NKCurrency = "USD";

			dataProvider = CargoProvider.New(instruction);
			AssertEquals(56d, dataProvider.Insurance.Amount);
			AssertEquals("USD", dataProvider.Insurance.CurrencyCode);
		}

		public void TestCountryOfOrigin()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var dataProvider = CargoProvider.New(instruction);
			Assert("CountryOfOrigin should be empty when JE_TransportMode is not AIR", dataProvider.CountryOfOrigin.IsEmpty());

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_DispatchModality = DispatchModalityCodes.Codes.Normal;
			dataProvider = CargoProvider.New(instruction);
			Assert("CountryOfOrigin should be empty when JE_DispatchModality is not empty", dataProvider.CountryOfOrigin.IsEmpty());

			declaration.JE_DispatchModality = ZString.Empty;
			dataProvider = CargoProvider.New(instruction);
			Assert("CountryOfOrigin should be empty", dataProvider.CountryOfOrigin.IsEmpty());

			declaration.JE_GoodsOrigin = "US";
			dataProvider = CargoProvider.New(instruction);
			AssertEquals("CountryOfOrigin should be US", "US", dataProvider.CountryOfOrigin);
		}
	}
}
