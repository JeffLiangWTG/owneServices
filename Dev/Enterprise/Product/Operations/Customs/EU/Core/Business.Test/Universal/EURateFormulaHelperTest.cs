using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	public class EURateFormulaHelperTest : TestCase
	{
		public void TestHasMeursingExpression()
		{
			Assert("Rate formula patten matched", EURateFormulaHelper.ContainsMursingPattern("#ADFM(2)#"));
			Assert("Rate formula patten matched", EURateFormulaHelper.ContainsMursingPattern("#ADFMR(5)#"));
			Assert("Rate formula patten matched", EURateFormulaHelper.ContainsMursingPattern("#ADSZ(6)#"));
			Assert("Rate formula patten matched", EURateFormulaHelper.ContainsMursingPattern("#ADSZR(7)#"));
			Assert("Rate formula patten matched", EURateFormulaHelper.ContainsMursingPattern("#EA(1)#"));
			Assert("Rate formula patten matched", EURateFormulaHelper.ContainsMursingPattern("#EAR(8)#"));
			Assert("Rate formula patten matched", EURateFormulaHelper.ContainsMursingPattern("MIN(0 + #EAR(1)#, VFD * 0.242 +#ADSZR(1)#)"));

			Assert("Rate formula patten not matched", !EURateFormulaHelper.ContainsMursingPattern("#NO(8)#"));
			Assert("Rate formula patten not matched", !EURateFormulaHelper.ContainsMursingPattern("#ADFM(23)#"));
			Assert("Rate formula patten not matched", !EURateFormulaHelper.ContainsMursingPattern("#ADFM(2)"));
			Assert("Rate formula patten not matched", !EURateFormulaHelper.ContainsMursingPattern("#ADFM[2]#"));
		}
	}
}
