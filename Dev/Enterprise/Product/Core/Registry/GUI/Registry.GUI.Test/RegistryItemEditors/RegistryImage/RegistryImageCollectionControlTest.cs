using System.ComponentModel;
using System.IO;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class RegistryImageCollectionControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestRegistryFormHasChangesIsSetAfterNewImageIsChosen()
		{
			var imageFileWithValidSize = Path.Combine(Env.TempPath, "ImageSize20mb.jpg");
			try
			{
				using var form = new RegistryFormForTest();
				using var control = new RegistryImageCollectionControlForTest();

				form.Controls.Add(control);

				var fileDialog = control.FileDialog;
				using (TestFileHelper.GenerateRandomJpg(imageFileWithValidSize, 19))
				{
					control.FileDialog.FileName = imageFileWithValidSize;

					var onFileOKMethodInfo = fileDialog.GetType().GetMethod("OnFileOk", BindingFlags.NonPublic | BindingFlags.Instance);
					AssertNotNull(onFileOKMethodInfo);

					Assert(!form.IsUpdateHasChangesTriggered);
					onFileOKMethodInfo.Invoke(fileDialog, new object[] { new CancelEventArgs() });
					Assert(form.IsUpdateHasChangesTriggered);
					using (control.RegistryImage)
					{
					}
				}
			}
			finally
			{
				File.Delete(imageFileWithValidSize);
			}
		}
	}
}
