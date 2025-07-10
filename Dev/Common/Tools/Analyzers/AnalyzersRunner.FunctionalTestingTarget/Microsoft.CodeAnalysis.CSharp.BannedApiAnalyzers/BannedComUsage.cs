using System;
using System.Runtime.InteropServices;

namespace AnalyzersRunner.FunctionalTestingTarget.Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers
{
	class BannedComUsage
	{
		const string Clsid = "{000209FF-0000-0000-C000-000000000046}";

		const string Progid = "DIRECT.ddPalette.3";

		public void MethodAboutCLSID()
		{
			//Banned in BannedSymbols.txt

			//CW1165:Do not used Type.GetTypeFromCLSID API
			//https://learn.microsoft.com/en-us/dotnet/api/system.type.gettypefromclsid?view=net-8.0
			Type.GetTypeFromCLSID(Guid.Parse(Clsid));
			Type.GetTypeFromCLSID(Guid.Parse(Clsid), throwOnError: true);
			Type.GetTypeFromCLSID(Guid.Parse(Clsid), "test");
			Type.GetTypeFromCLSID(Guid.Parse(Clsid), "test", throwOnError: true);
		}

		public void MethodAboutProgID()
		{
			//Banned in BannedSymbols.txt

			//CW1165:Do not used Type.GetTypeFromProgID API
			//https://learn.microsoft.com/en-us/dotnet/api/system.type.gettypefromprogid?view=net-8.0
			Type.GetTypeFromProgID(Progid);
			Type.GetTypeFromProgID(Progid, throwOnError: true);
			Type.GetTypeFromProgID(Progid, "test");
			Type.GetTypeFromProgID(Progid, "test", throwOnError: true);
		}

		//Banned in BannedSymbols.txt
		//CW1165:Do not used ProgIdAttribute
		[ProgId("InteropSample.MyClass")]
		public class MyClassWithProgIdAttribute
		{
			public MyClassWithProgIdAttribute() { }
		}

		//Banned in BannedSymbols.txt
		//CW1165:Do not used GuidAttribute
		//CW1165:Do not used ComImportAttribute
		[ComImport]
		public class MyClassWithComImportAttributes
		{ }

		//Banned in BannedSymbols.txt
		//CW1165:Do not used GuidAttribute
		//CW1165:Do not used ComImportAttribute
		[ComImport]
		[Guid("9ED54F84-A89D-4fcd-A854-44251E925F09")]
		public class MyClassWithGUIDAndComImportAttributes
		{ }
	}
}
