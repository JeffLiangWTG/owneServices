using System.IO;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0.Testing
{
	internal static class TestExtensionsAESVersion3_0
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		public static string AESTestFilesDirectory => Path.Combine(TestCase.BaseSourcePath, TestExtensions.MessageBuildersDirectory, @"AESVersion3_0\TestFiles");
	}
}
