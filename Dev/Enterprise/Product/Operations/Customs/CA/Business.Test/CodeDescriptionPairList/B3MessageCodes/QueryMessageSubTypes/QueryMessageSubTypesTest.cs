using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class QueryMessageSubTypesTest : TestCase
	{
		public void TestGet3CharCode()
		{
			AssertEquals("CLASSFILE", QueryMessageSubType3CharCodes.Codes.CLASSFILE, QueryMessageSubTypes.Get3CharCode(QueryMessageSubTypes.Codes.CLASSFILE));
			AssertEquals("TARIFFCODE", QueryMessageSubType3CharCodes.Codes.TARIFFCODE, QueryMessageSubTypes.Get3CharCode(QueryMessageSubTypes.Codes.TARIFFCODE));
			AssertEquals("GSTFILE", QueryMessageSubType3CharCodes.Codes.GSTFILE, QueryMessageSubTypes.Get3CharCode(QueryMessageSubTypes.Codes.GSTFILE));
			AssertEquals("EXCISETAX", QueryMessageSubType3CharCodes.Codes.EXCISETAX, QueryMessageSubTypes.Get3CharCode(QueryMessageSubTypes.Codes.EXCISETAX));
			AssertEquals("QRCLASSTAR", QueryMessageSubType3CharCodes.Codes.QRCLASSTAR, QueryMessageSubTypes.Get3CharCode(QueryMessageSubTypes.Codes.QRCLASSTAR));
			AssertEquals("EXCHANGERATE", QueryMessageSubType3CharCodes.Codes.EXCHANGERATE, QueryMessageSubTypes.Get3CharCode(QueryMessageSubTypes.Codes.EXCHANGERATE));
			AssertEquals("QREXCHANGE", QueryMessageSubType3CharCodes.Codes.QREXCHANGE, QueryMessageSubTypes.Get3CharCode(QueryMessageSubTypes.Codes.QREXCHANGE));
			AssertEquals("BROADCAST", QueryMessageSubType3CharCodes.Codes.BROADCAST, QueryMessageSubTypes.Get3CharCode(QueryMessageSubTypes.Codes.BROADCAST));
			AssertEquals("UNKNOWN", string.Empty, QueryMessageSubTypes.Get3CharCode("XXX"));
		}
	}
}
