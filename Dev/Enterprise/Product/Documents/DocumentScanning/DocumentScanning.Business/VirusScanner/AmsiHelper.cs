using System.Linq;
using CargoWise.Application;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DocumentScanning.Business
{
	public static class AmsiHelper
	{
		public static bool IsMalware(string fileName, byte[] fileContent)
		{
			if (fileContent == null || fileContent.Length == 0)
			{
				return false;
			}

			if (fileContent.SequenceEqual(WTGVirusTestBytes))
			{
				return true;
			}

			using (var application = ObjectFactory.Get<IAmsiContext>("IAmsiContext", "CargoWiseOne"))
			using (var session = application.CreateSession())
			{
				return session.IsMalware(fileContent, fileName);
			}
		}

		[ThreadSafe]
		// This is a virus test bytes to test eDocs virus scanning. This bytes represent the following string:
		// eijB8fm_gpDCFii0))7%rJ~=BzDiWTGVirusTest!YRF]7@uHUZ+]knv!*PMxrD:
		static readonly byte[] WTGVirusTestBytes = new byte[64] {
101, 105, 106, 66, 56, 102, 109, 95, 103, 112, 68, 67, 70, 105, 105, 48, 41, 41, 55, 37, 114, 74, 126, 61, 66, 122, 68, 105, 87, 84, 71, 86,
105, 114, 117, 115, 84, 101, 115, 116, 33, 89, 82, 70, 93, 55, 64, 117, 72, 85, 90, 43, 93, 107, 110, 118, 33, 42, 80, 77, 120, 114, 68, 58 };
	}
}
