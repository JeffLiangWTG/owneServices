using System.Text.RegularExpressions;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.GB.SafetyAndSecurity.Messaging.CC315A;

namespace Enterprise.Customs.GB.ICS.Testing
{
	public sealed class CC315AMessagePrettierTest : TestCaseWithFactory
	{
		public void TestMakeHumanReadable()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var wrapper = new DeclarationWrapper(manifest);
			var prettier = new CC315AMessagePrettier(wrapper);

			var expected = IgnoreBreaksAndIndentations(@"
<p><strong>Message number: </strong>{{MSGNO PLACEHOLDER}}<br>
<strong>Reference number: </strong>C123456<br><strong>Number of items: </strong>0<br>
<strong>Number of packages: </strong>0<br>
<strong>Gross mass: </strong>0<br>
<strong>Declaration place: </strong>UNIT 3, 480 NUDGEE ROAD<br>
<strong>Commercial reference: </strong>C123456</p>");
			AssertContains(expected, prettier.MakeHumanReadable());
		}

		string IgnoreBreaksAndIndentations(string input)
		{
			return Regex.Replace(input, @"[\r\n]+\s*", string.Empty);
		}
	}
}
