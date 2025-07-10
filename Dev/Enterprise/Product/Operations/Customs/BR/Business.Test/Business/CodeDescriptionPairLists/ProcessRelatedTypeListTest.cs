using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class ProcessRelatedTypeListTest : TestCase
	{
		public void GetProcessRelatedTypeValue()
		{
			var result = ProcessRelatedTypeList.MapToCustomsCode("");
			AssertEquals("Result empty", ZString.Empty, result);

			result = ProcessRelatedTypeList.MapToCustomsCode(ProcessRelatedTypeList.Codes.ADM);
			AssertEquals("ADM", "1", result);

			result = ProcessRelatedTypeList.MapToCustomsCode(ProcessRelatedTypeList.Codes.JUD);
			AssertEquals("JUD", "2", result);

			result = ProcessRelatedTypeList.MapToCustomsCode(ProcessRelatedTypeList.Codes.PRE);
			AssertEquals("PRE", "3", result);

			result = ProcessRelatedTypeList.MapToCustomsCode(ProcessRelatedTypeList.Codes.EJD);
			AssertEquals("EJD", "4", result);
		}
	}
}
