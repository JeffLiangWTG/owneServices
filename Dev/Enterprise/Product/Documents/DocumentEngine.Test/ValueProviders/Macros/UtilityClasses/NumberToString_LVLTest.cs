using System;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.ReportErrorManagement;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	sealed class NumberToString_LVLTest : NumberToWordsTestCase
	{
		//http://www.unicode.org/cldr/charts/28/verify/numbers/lv.html

		public void TestReplacement()
		{
			CombineAssertions(delegate
			{
				Assert(0, "nulle");
				Assert(1, "viens");
				Assert(2, "divi");
				Assert(3, "trīs");
				Assert(4, "četri");
				Assert(5, "pieci");
				Assert(6, "seši");
				Assert(7, "septiņi");
				Assert(8, "astoņi");
				Assert(9, "deviņi");
				Assert(10, "desmit");
				Assert(11, "vienpadsmit");
				Assert(12, "divpadsmit");
				Assert(13, "trīspadsmit");
				Assert(14, "četrpadsmit");
				Assert(15, "piecpadsmit");
				Assert(16, "sešpadsmit");
				Assert(17, "septiņpadsmit");
				Assert(18, "astoņpadsmit");
				Assert(19, "deviņpadsmit");
				Assert(20, "divdesmit");
				Assert(21, "divdesmit viens");
				Assert(22, "divdesmit divi");
				Assert(23, "divdesmit trīs");
				Assert(25, "divdesmit pieci");
				Assert(26, "divdesmit seši");
				Assert(30, "trīsdesmit");
				Assert(31, "trīsdesmit viens");
				Assert(40, "četrdesmit");
				Assert(43, "četrdesmit trīs");
				Assert(50, "piecdesmit");
				Assert(54, "piecdesmit četri");
				Assert(55, "piecdesmit pieci");
				Assert(60, "sešdesmit");
				Assert(67, "sešdesmit septiņi");
				Assert(70, "septiņdesmit");
				Assert(79, "septiņdesmit deviņi");
				Assert(80, "astoņdesmit");
				Assert(90, "deviņdesmit");
				Assert(100, "simts");
				Assert(101, "simts viens");
				Assert(112, "simts divpadsmit");
				Assert(150, "simts piecdesmit");
				Assert(157, "simts piecdesmit septiņi");
				Assert(199, "simts deviņdesmit deviņi");
				Assert(200, "divi simti");
				Assert(203, "divi simti trīs");
				Assert(231, "divi simti trīsdesmit viens");
				Assert(287, "divi simti astoņdesmit septiņi");
				Assert(300, "trīs simti");
				Assert(356, "trīs simti piecdesmit seši");
				Assert(400, "četri simti");
				Assert(410, "četri simti desmit");
				Assert(434, "četri simti trīsdesmit četri");
				Assert(500, "pieci simti");
				Assert(578, "pieci simti septiņdesmit astoņi");
				Assert(600, "seši simti");
				Assert(689, "seši simti astoņdesmit deviņi");
				Assert(700, "septiņi simti");
				Assert(729, "septiņi simti divdesmit deviņi");
				Assert(800, "astoņi simti");
				Assert(894, "astoņi simti deviņdesmit četri");
				Assert(900, "deviņi simti");
				Assert(999, "deviņi simti deviņdesmit deviņi");
				Assert(1000, "tūkstotis");
				Assert(1001, "tūkstotis viens");
				Assert(1063, "tūkstotis sešdesmit trīs");
				Assert(1097, "tūkstotis deviņdesmit septiņi");
				Assert(1104, "tūkstotis simts četri");
				Assert(1243, "tūkstotis divi simti četrdesmit trīs");
				Assert(1876, "tūkstotis astoņi simti septiņdesmit seši");
				Assert(2018, "divi tūkstoši astoņpadsmit");
				Assert(2385, "divi tūkstoši trīs simti astoņdesmit pieci");
				Assert(3766, "trīs tūkstoši septiņi simti sešdesmit seši");
				Assert(4196, "četri tūkstoši simts deviņdesmit seši");
				Assert(5846, "pieci tūkstoši astoņi simti četrdesmit seši");
				Assert(6459, "seši tūkstoši četri simti piecdesmit deviņi");
				Assert(7232, "septiņi tūkstoši divi simti trīsdesmit divi");
				Assert(8569, "astoņi tūkstoši pieci simti sešdesmit deviņi");
				Assert(9539, "deviņi tūkstoši pieci simti trīsdesmit deviņi");
				Assert(10000, "desmit tūkstoši");
				Assert(20000, "divdesmit tūkstoši");
				Assert(21000, "divdesmit viens tūkstotis");
				Assert(111201, "simts vienpadsmit tūkstoši divi simti viens");
				Assert(131201, "simts trīsdesmit viens tūkstotis divi simti viens");
				Assert(132201, "simts trīsdesmit divi tūkstoši divi simti viens");
				Assert(1000000, "miljons");
				Assert(11000000, "vienpadsmit miljoni");
				Assert(21000000, "divdesmit viens miljons");
				Assert(131201000, "simts trīsdesmit viens miljons divi simti viens tūkstotis");
				Assert(132202123, "simts trīsdesmit divi miljoni divi simti divi tūkstoši simts divdesmit trīs");
				Assert(999999999, "deviņi simti deviņdesmit deviņi miljoni deviņi simti deviņdesmit deviņi tūkstoši deviņi simti deviņdesmit deviņi");
			});
		}

		public void TestNegative()
		{
			var numberToString = new NumberToString_LVL();
			for (long i = -10; i < 0; ++i)
			{
				AssertExceptionThrown(typeof(FormulaProviderException), () => numberToString.GetNumberAsString(i));
				try
				{
					numberToString.GetNumberAsString(i);
				}
				catch (Exception ex)
				{
					AssertNoExceptionThrown(() => new ReportErrorManager(new Report(null, null)).ReportFatalException(ex));
				}
			}
		}

		internal override INumberToWords GetInstance()
		{
			return new NumberToString_LVL();
		}
	}
}
