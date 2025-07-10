using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public class FileManager : IFindBoxPopup
	{
		public FileManager(ZString fileType)
		{
			this.fileType = fileType;
		}

		readonly ZString fileType;

		IFindBox findBox;

		ZString fileFilter
		{
			get
			{
				switch (fileType)
				{
					case ".wav":
						return (NoResString)"Wave Files (*.wav)|*.wav";
					case ".mp3":
						return (NoResString)"MP3 Files (*.mp3)|*.mp3";
					default:
						return ZString.Empty;
				}
			}
		}

		void ShowFileDialog()
		{
			using (var openDialog = new ZOpenFileDialog())
			{
				openDialog.Filter = fileFilter;
				var result = openDialog.ShowDialog();
				if (result == DialogResult.OK)
				{
					findBox.Code = openDialog.UnmappedFileName;
				}
			}
		}

		public event System.EventHandler Closed
		{
			add
			{
			}
			remove
			{
			}
		}

		public void Dispose()
		{
		}

		public SilentSelectResult SelectFromPopupWithoutDisplaying(IFindBox findBox, ZArchitecture.GUI.Internal.EmbeddedModulePopup popup)
		{
			throw new System.NotImplementedException();
		}

		public void SelectRowByPK(ZGuid pk)
		{
			throw new System.NotImplementedException();
		}

		public void ShowModal(IFindBox findBox, System.Windows.Forms.Form parentForm)
		{
			this.findBox = findBox;
			ShowFileDialog();
		}
	}
}
