using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class EventIdListTest : TestCase
	{
		public void TestGetCustomsEventValue()
		{
			var result = EventIdList.GetCustomsEventValue("");
			AssertEquals("Result empty", ZString.Empty, result);

			result = EventIdList.GetCustomsEventValue(EventIdList.Codes.DuexHistoric);
			AssertEquals("DuexHistoric", "duex-historico", result);

			result = EventIdList.GetCustomsEventValue(EventIdList.Codes.ProductCatalog);
			AssertEquals("ProductCatalog", "catp-prod-desativado", result);

			result = EventIdList.GetCustomsEventValue(EventIdList.Codes.CctReleasedCargo);
			AssertEquals("CctReleasedCargo", "cctr-carga-liberada", result);

			result = EventIdList.GetCustomsEventValue(EventIdList.Codes.CctBlockedCargo);
			AssertEquals("CctBlockedCargo", "cctr-carga-bloqueio", result);

			result = EventIdList.GetCustomsEventValue(EventIdList.Codes.CctRedChannel);
			AssertEquals("CctRedChannel", "cctr-canal-vermelho", result);

			result = EventIdList.GetCustomsEventValue(EventIdList.Codes.LpcoStatusChange);
			AssertEquals("LpcoStatusChange", "talp-altsit-lpco-anu", result);

			result = EventIdList.GetCustomsEventValue(EventIdList.Codes.LpcoExigencyInclusion);
			AssertEquals("LpcoExigencyInclusion", "talp-inclusao-exig", result);

			result = EventIdList.GetCustomsEventValue(EventIdList.Codes.LpcoExigencyCancelation);
			AssertEquals("LpcoExigencyCancelation", "talp-cancela-exig", result);

			result = EventIdList.GetCustomsEventValue(EventIdList.Codes.DuimpDiagnosis);
			AssertEquals("DuimpDiagnosis", "dimp-diag-import", result);

			result = EventIdList.GetCustomsEventValue(EventIdList.Codes.DuimpRegister);
			AssertEquals("DuimpRegister", "dimp-registro-import", result);

			result = EventIdList.GetCustomsEventValue(EventIdList.Codes.DuimpStatus);
			AssertEquals("DuimpStatus", "dimp-situacao-import", result);
		}
	}
}
