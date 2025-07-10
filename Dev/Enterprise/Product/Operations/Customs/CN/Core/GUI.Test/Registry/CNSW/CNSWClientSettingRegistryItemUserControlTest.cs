using CargoWise.EntityFramework;
using Enterprise.Customs.CN.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CN.GUI.Testing
{
	[TestedType(typeof(CNSWClientSettingRegistryItemUserControl))]
	class CNSWClientSettingRegistryItemUserControlTest : RegistryZUserControlTestCase
	{
		public void TestFolderControls()
		{
			using (var control = new CNSWClientSettingRegistryItemUserControl())
			{
				var declarationGroupBox = control.FindSingle<ZGroupBox>("DeclarationGroupBox");
				var sendFolderTextBox = control.FindSingle<ZTextBox>("SendFolderTextBox");
				var receiveFolderTextBox = control.FindSingle<ZTextBox>("ReceiveFolderTextBox");
				var errorResponseFolderTextBox = control.FindSingle<ZTextBox>("ErrorResponseFolderTextBox");
				var archiveFolderTextBox = control.FindSingle<ZTextBox>("ArchiveFolderTextBox");
				AssertSame(declarationGroupBox, sendFolderTextBox.Parent);
				AssertSame(declarationGroupBox, receiveFolderTextBox.Parent);
				AssertSame(declarationGroupBox, errorResponseFolderTextBox.Parent);
				AssertSame(declarationGroupBox, archiveFolderTextBox.Parent);
				var acdaGroupBox = control.FindSingle<ZGroupBox>("ACDAGroupBox");
				var acdaSendFolderTextBox = control.FindSingle<ZTextBox>("AcdaSendFolderTextBox");
				var acdaReceiveFolderTextBox = control.FindSingle<ZTextBox>("AcdaReceiveFolderTextBox");
				var acdaErrorResponseFolderTextBox = control.FindSingle<ZTextBox>("AcdaErrorResponseFolderTextBox");
				var acdaArchiveFolderTextBox = control.FindSingle<ZTextBox>("AcdaArchiveFolderTextBox");
				AssertSame(acdaGroupBox, acdaSendFolderTextBox.Parent);
				AssertSame(acdaGroupBox, acdaReceiveFolderTextBox.Parent);
				AssertSame(acdaGroupBox, acdaErrorResponseFolderTextBox.Parent);
				AssertSame(acdaGroupBox, acdaArchiveFolderTextBox.Parent);
				AssertEquals("AcdaSendFolder", acdaSendFolderTextBox.BindTo);
				AssertEquals("AcdaReceiveFolder", acdaReceiveFolderTextBox.BindTo);
				AssertEquals("AcdaErrorResponseFolder", acdaErrorResponseFolderTextBox.BindTo);
				AssertEquals("AcdaArchiveFolder", acdaArchiveFolderTextBox.BindTo);
			}
		}

		protected override IBusiness GetNewBusinessEntity() => new CNSWClientSetting();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => ((CNSWClientSettingRegistryItemUserControl)control).ReadOnly;
	}
}
