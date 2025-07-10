using System.IO;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
	static class MessageBuilderTestHelper
	{
		public static string ExportOutgoingTestFilePath => Path.Combine(TestCase.BaseSourcePath, "Enterprise", "Product", "Operations", "Customs", "KR", "Messaging.Test", "TestFiles", "Export", "OutGoing");

		public static string ImportOutgoingTestFilePath => Path.Combine(TestCase.BaseSourcePath, "Enterprise", "Product", "Operations", "Customs", "KR", "Messaging.Test", "TestFiles", "Import", "Outgoing");

		public static string LocalExportOutgoingTestFilePath => Path.Combine(TestCase.BaseSourcePath, "Enterprise", "Product", "Operations", "Customs", "KR", "Messaging.Test", "TestFiles", "LocalExport", "Outgoing");
	}
}
