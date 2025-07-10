using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Business.Test
{
	public class MethodOfPaymentCodesTest : TestCaseWithFactory
	{
		public void TestMOPList()
		{
			AssertArrayEqualsByElements(cDSDeferredMethodsOfPayment, MethodOfPaymentCodes.CDS.DeferredMethodsOfPayment);
			AssertArrayEqualsByElements(cDSGuaranteeDeferredMethodsOfPayment, MethodOfPaymentCodes.CDS.GuaranteeDeferredMethodsOfPayment);
			AssertArrayEqualsByElements(cDSImmediateCashMethodsOfPayment, MethodOfPaymentCodes.CDS.ImmediateCashMethodsOfPayment);
			AssertArrayEqualsByElements(cHIEFDeferredMethodsOfPayment, MethodOfPaymentCodes.CHIEF.DeferredMethodsOfPayment);
			AssertArrayEqualsByElements(cHIEFGuaranteeDeferredMethodsOfPayment, MethodOfPaymentCodes.CHIEF.GuaranteeDeferredMethodsOfPayment);
		}
		readonly ZString[] cDSDeferredMethodsOfPayment = { "E", "R" };
		readonly ZString[] cDSGuaranteeDeferredMethodsOfPayment = { "R", "S", "T", "U", "V", "Z" };
		readonly ZString[] cDSImmediateCashMethodsOfPayment = { "A", "P" };
		readonly ZString[] cHIEFDeferredMethodsOfPayment = { "F", "Q" };
		readonly ZString[] cHIEFGuaranteeDeferredMethodsOfPayment = { "Q", "S", "T", "U", "V", "W", "X", "Y" };
	}
}
