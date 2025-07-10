using System;
using System.IO;
using System.Reflection;
using System.Text;
using CargoWise.IO;

namespace Enterprise.RemoteDesktopServices.Testing
{
	public static class TestMessage
	{
		public enum Action
		{
			TestDragAndDropMultipleSessions,
			TestDragAndDropDragEffects,
			TestDragAndDropDragUnsupported,
			TestDragAndDropDropTextFile,
			TestDragAndDropDropMultipleFiles,
			TestDragAndDropDropBitmap,
			TestDragAndDropDropFilesWithLongNames,
			TestDragAndDropLiteDropFilesWithLongNames,
			SetDragAndDropLite,
			SetDragAndDrop,
			TestDragAndDropWithFilePath,
			TestDragAndDropNullMessage,
		}

		public static byte[] GetMessage(Action action)
			=> GetMessage(action, string.Empty);

		public static byte[] GetMessage(string filePath)
			=> GetMessage(Action.TestDragAndDropWithFilePath, filePath);

		static byte[] GetMessage(Action action, string filePath)
			=> Encoding.UTF8.GetBytes($"{action}{Separator}{filePath}");

		public static (Action action, string filePath) ParseMessage(Stream message)
		{
			var parts = new StreamReader(message, Encoding.UTF8).ReadToEnd().Split(Separator);
			var action = (Action)Enum.Parse(typeof(Action), parts[0]);
			var filePath = parts[1];
			return (action, filePath);
		}

		public static string GetTestFilePath(string fileName)
		{
			return Path.Combine(TestDocFolder, fileName);
		}

		public static IDisposable ReleaseResourcesFiles()
		{
			var testDocDirectory = new TempDirectory(TestDocFolder);

			var assembly = Assembly.GetExecutingAssembly();
			var resourceNamePrefix = assembly.GetName().Name + ".Resources.";

			foreach (string resourceFullName in assembly.GetManifestResourceNames())
			{
				var resourceName = resourceFullName.Substring(resourceNamePrefix.Length);
				using (var resource = assembly.GetManifestResourceStream(resourceFullName))
				{
					using (var file = new FileStream(GetTestFilePath(resourceName), FileMode.Create, FileAccess.Write))
					{
						resource?.CopyTo(file);
					}
				}
			}

			return testDocDirectory;
		}

		static readonly string TestDocFolder = Path.Combine(Path.GetDirectoryName(Temp.TempPathWithoutCreating), "TestDoc");

		const char Separator = '|';
	}
}
