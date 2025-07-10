using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EXDOCTransportModeCodesTest : TestCase
	{
		public void TestGetSingleCharFromThreeCharCode()
		{
			AssertEquals("Sea", EXDOCTransportModeCodes.Codes.Sea, EXDOCTransportModeCodes.GetEXDOCCodeFromTransportModeCode(Core.Constants.TransportModes.Sea));
			AssertEquals("Air", EXDOCTransportModeCodes.Codes.Air, EXDOCTransportModeCodes.GetEXDOCCodeFromTransportModeCode(Core.Constants.TransportModes.Air));
			AssertEquals("Mail", EXDOCTransportModeCodes.Codes.Mail, EXDOCTransportModeCodes.GetEXDOCCodeFromTransportModeCode(Core.Constants.TransportModes.Mail));
			AssertEquals("NotExistingCode", string.Empty, EXDOCTransportModeCodes.GetEXDOCCodeFromTransportModeCode("BLA"));
		}
	}
}
