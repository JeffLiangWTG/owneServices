using System.IO;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
	static class TestExtensionsATLASVersion10_1
	{
		public static string ImportTestFilesDirectory => Path.Combine(TestCase.BaseSourcePath, TestExtensions.MessageBuildersDirectory, @"ATLASVersion10_1\Import\TestFiles");

		public static string ImportTestFilesResourcePath => "Enterprise.Customs.DE.Messaging.Testing.MessageBuilders.ATLASVersion10_1.Import.TestFiles";

		public static string NctsTestFilesDirectory => Path.Combine(TestCase.BaseSourcePath, TestExtensions.MessageBuildersDirectory, @"ATLASVersion10_1\NCTS\TestFiles");

		public static string CollectiveMessagesTestFilesDirectory => Path.Combine(TestCase.BaseSourcePath, TestExtensions.MessageBuildersDirectory, @"ATLASVersion10_1\CollectiveMessages\TestFiles");

		public static string CollectiveTestFilesResourcePath => "Enterprise.Customs.DE.Messaging.Testing.MessageBuilders.ATLASVersion10_1.CollectiveMessages.TestFiles";

		public static string MonthlyClosingTestFilesDirectory => Path.Combine(TestCase.BaseSourcePath, TestExtensions.MessageBuildersDirectory, @"ATLASVersion10_1\MonthlyClosing\TestFiles");

		public static string MonthlyClosingTestFilesResourcePath => "Enterprise.Customs.DE.Messaging.Testing.MessageBuilders.ATLASVersion10_1.MonthlyClosing.TestFiles";
	}
}
