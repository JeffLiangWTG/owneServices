using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	class IncoTermConverterTest : TestCase
	{
		public void TestGetConvertedIncoTerm()
		{
			var converter = new IncoTermConverter();
			CombineAssertions(() =>
			{
				AssertEquals("CostInsuranceAndFreight", CNInvoiceHeaderIncoTermList.Codes.CIF, converter.GetConvertedIncoTerm(Core.Constants.IncoTerms.CostInsuranceAndFreight, true));
				AssertEquals("CarriageAndInsurancePaidTo", CNInvoiceHeaderIncoTermList.Codes.CIF, converter.GetConvertedIncoTerm(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, true));
				AssertEquals("DeliveredDutyPaid", CNInvoiceHeaderIncoTermList.Codes.CIF, converter.GetConvertedIncoTerm(Core.Constants.IncoTerms.DeliveredDutyPaid, true));
				AssertEquals("DeliveredAtPlace", CNInvoiceHeaderIncoTermList.Codes.CIF, converter.GetConvertedIncoTerm(Core.Constants.IncoTerms.DeliveredAtPlace, true));
				AssertEquals("DeliveredAtTerminal", CNInvoiceHeaderIncoTermList.Codes.CIF, converter.GetConvertedIncoTerm(Core.Constants.IncoTerms.DeliveredAtTerminal, true));
				AssertEquals("CostAndFreight", CNInvoiceHeaderIncoTermList.Codes.CFR, converter.GetConvertedIncoTerm(Core.Constants.IncoTerms.CostAndFreight, true));
				AssertEquals("CostFreightWithAmpersand", CNInvoiceHeaderIncoTermList.Codes.CFR, converter.GetConvertedIncoTerm(Core.Constants.IncoTerms.CostFreightWithAmpersand, true));
				AssertEquals("CarriagePaidTo", CNInvoiceHeaderIncoTermList.Codes.CFR, converter.GetConvertedIncoTerm(Core.Constants.IncoTerms.CarriagePaidTo, true));
				AssertEquals("ExWorks", CNInvoiceHeaderIncoTermList.Codes.ExWorks, converter.GetConvertedIncoTerm(Core.Constants.IncoTerms.ExWorks, true));
				AssertEquals("FreeCarrier", CNInvoiceHeaderIncoTermList.Codes.FOB, converter.GetConvertedIncoTerm(Core.Constants.IncoTerms.FreeCarrier, true));
				AssertEquals("FreeAlongsideShip", CNInvoiceHeaderIncoTermList.Codes.FOB, converter.GetConvertedIncoTerm(Core.Constants.IncoTerms.FreeAlongsideShip, true));
				AssertEquals("FreeOnBoard", CNInvoiceHeaderIncoTermList.Codes.FOB, converter.GetConvertedIncoTerm(Core.Constants.IncoTerms.FreeOnBoard, true));
				AssertEquals("CostAndInsurance", CNInvoiceHeaderIncoTermList.Codes.CAI, converter.GetConvertedIncoTerm(Core.Constants.IncoTerms.CostAndInsurance, true));
				AssertEquals("Other", ZString.Empty, converter.GetConvertedIncoTerm("~!@", true));
			});
		}
	}
}
