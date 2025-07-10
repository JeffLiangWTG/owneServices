using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class APERAKGetterTest : TestCaseWithFactory
	{
		#region TestMessageText
		const string TestMessageText = @"UNH+10151+APERAK:D:00A:UN:ANZ23'
BGM+7+10151+9+AP'
DTM+137:20040715110617:204'
DOC+ERA+EDISYD'
DTM+137:20040715105226:204'
RFF+ERN:CON-C000000001-POCU2819798'
NAD+MS+1STOP'
NAD+MR+EDISRW'
ERC+ERA0100'
FTX+AAO+++MESSAGE RECEIVED WITHOUT ERROR'
RFF+EQD:POCU2819798'
UNT+12+10151'
";
		#endregion

		public void TestAPERAKResult()
		{
			EDIMessage message = Factory.New<EDIMessage>();
			message.EM_MessageText = TestMessageText.Replace("\r\n", "");
			APERAKGetter getter = new APERAKGetter(message);
			AssertEquals("CON-C000000001-POCU2819798", getter.APERAK.Group2[0].RFF[0].Reference.ReferenceIdentifier);
		}

		public void TestNullInConstructorDoesntBlowChunks()
		{
			APERAKGetter getter = new APERAKGetter(null);
			AssertEquals(null, getter.APERAK);
		}
	}
}
