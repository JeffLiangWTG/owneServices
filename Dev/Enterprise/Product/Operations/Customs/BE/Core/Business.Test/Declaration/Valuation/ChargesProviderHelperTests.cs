using ChargeTypes = Enterprise.Customs.BE.Business.Declaration.BECustomsChargeTypeList.Codes;
using IncoTerms = Enterprise.Core.Constants.IncoTerms;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

public class ChargesProviderHelperTests : NUnit.Framework.TestCase
{
	public void TestIsListed()
	{
		var testList = new (string, string)[]
		{
			(string.Empty, ChargeTypes.OverseasFreight),
			(IncoTerms.DeliveredAtPlace, ChargeTypes.OverseasInsurance),
			(IncoTerms.DeliveredAtTerminal, ChargeTypes.OverseasInsurance),
			(IncoTerms.DeliveredDutyPaid, ChargeTypes.OverseasInsurance),
			(IncoTerms.CarriageAndInsurancePaidTo, ChargeTypes.OverseasInsurance),
			(IncoTerms.CostInsuranceAndFreight, ChargeTypes.OverseasInsurance),
		};
		Assert(testList.IsListed(IncoTerms.FreeOnBoard, ChargeTypes.OverseasFreight));
		Assert(testList.IsListed(IncoTerms.DeliveredAtPlace, ChargeTypes.OverseasFreight));
		Assert(testList.IsListed(string.Empty, ChargeTypes.OverseasFreight));

		Assert(!testList.IsListed(IncoTerms.FreeOnBoard, ChargeTypes.OverseasInsurance));
		Assert(testList.IsListed(IncoTerms.DeliveredAtPlace, ChargeTypes.OverseasInsurance));
		Assert(!testList.IsListed(string.Empty, ChargeTypes.OverseasInsurance));

		Assert(!testList.IsListed(IncoTerms.FreeOnBoard, ChargeTypes.Discount));
		Assert(!testList.IsListed(IncoTerms.DeliveredAtPlace, ChargeTypes.Discount));
		Assert(!testList.IsListed(string.Empty, ChargeTypes.Discount));

		Assert(!testList.IsListed(IncoTerms.FreeOnBoard, string.Empty));
		Assert(!testList.IsListed(IncoTerms.DeliveredAtPlace, string.Empty));
		Assert(!testList.IsListed(string.Empty, string.Empty));
	}
}
