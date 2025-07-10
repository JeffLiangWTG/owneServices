using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class NctsTadEdocSaverOptionsTest : TestCase
	{
		public void TestOptions()
		{
			AssertEquals("When default constructor is used to instantiate options, Language", "", new NctsTadEdocSaverOptions().Language);
			AssertEquals("When 'null' parameter is provided, Language", "", new NctsTadEdocSaverOptions(language: null).Language);
			AssertEquals("When filled parameter is provided, Language", "IT-IT", new NctsTadEdocSaverOptions(language: "IT-IT").Language);
		}
	}
}
