using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class DigitalCertificateControlWithExport : DigitalCertificateControl
	{
		[Obsolete("Use the constructor that takes a FileUpLoaderX509CertificateRegistryEditorInfo, this constructor is just for the designer", true)]
		public DigitalCertificateControlWithExport()
			: base(new FileUpLoaderX509CertificateRegistryEditorInfo())
		{
			InitializeComponent();
		}

		public DigitalCertificateControlWithExport(FileUpLoaderX509CertificateRegistryEditorInfo editorInfo)
			: base(editorInfo)
		{
			InitializeComponent();
		}

		#region Save to Disk

		void SaveToDiskButton_Click(object sender, System.EventArgs e)
		{
			SaveToDisk();
		}

		void SaveToDisk()
		{
			if (!IsBinaryFileDataEmpty)
			{
				using (DeveloperLoginForm loginForm = new DeveloperLoginForm())
				{
					if (GetLoginDialogResultWithoutDispose(loginForm) == DialogResult.OK)
					{
						if (IsValidPassword(loginForm))
						{
							using (var fileDialog = new ZSaveFileDialog())
							{
								string filterName = Res.GetString("081c679b-fb3b-44a3-8cf2-5b456d4003ff", "Security Certificate") + " ";

								fileDialog.Filter = filterName + (NoResString)"(*.cer)|*.cer";

								if (GetSaveDialogResultWithoutDispose(fileDialog) == DialogResult.OK)
								{
									try
									{
										using (var stream = fileDialog.OpenFile())
										{
											SaveToDisk(stream);
										}
									}
									catch (Exception ex) when (!ex.IsCriticalException())
									{
										string errorMessage = Res.GetString("89ff9a04-047a-4b4b-b940-b2327ba6ea43", "An error occurred while saving.\r\n\r\n{0}", ex.Message);
										Globals.Message.ShowError(errorMessage);
									}
								}
							}
						}
						else
						{
							loginForm.ShowIncorrectPasswordMessage();
						}
					}
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("e902f4a2-e108-405c-be02-2a49c1030f80", "There is currently no Certificate to save to disk."));
			}
		}

#if DEBUG
		protected virtual
#endif
 DialogResult GetLoginDialogResultWithoutDispose(DeveloperLoginForm loginForm)
		{
			return ZFormModaliser.ShowDialogWithoutDispose(loginForm);
		}

#if DEBUG
		protected virtual
#endif
 bool IsValidPassword(DeveloperLoginForm loginForm)
		{
			return loginForm.IsValidPassword;
		}

#if DEBUG
		protected virtual
#endif
 DialogResult GetSaveDialogResultWithoutDispose(ZSaveFileDialog fileDialog)
		{
			return ZFormModaliser.ShowCommonDialogWithoutDispose(fileDialog);
		}

#if DEBUG
		protected virtual
#endif
 void SaveToDisk(Stream stream)
		{
			stream.Write(FileDataAsBinary, 0, FileDataAsBinary.Length);
		}

		#endregion
	}
}
