using System;
using System.IO;
using System.Windows.Forms;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

sealed class CertificateOfOriginXmlUploaderGridContextMenuItemComponent : GridContextMenuItemComponent<ITEDIMessage>
{
	public CertificateOfOriginXmlUploaderGridContextMenuItemComponent(ZGrid grid) : base(grid)
	{
	}

	protected override void Execute(ZMenuItem menuItem)
	{
		if (DataContext is ITEDIMessage { EM_LinkedObject: CusEntryHeader entryHeader })
		{
			using var openFileDialog = new ZOpenFileDialog()
			{
				Filter = Res.GetString("47E69817-A63D-453D-ACBC-F39F7B757754", "XML Files (*.xml)|*.xml")
			};

			if (ZFormModaliser.ShowCommonDialogWithoutDispose(openFileDialog) == DialogResult.OK)
			{
				UploadFile(entryHeader, openFileDialog.OpenFile());
			}
		}
	}

	protected override ZMenuItem GetMenuItem() => new ZMenuItem(Res.GetString("98CE8DC3-251C-42C0-AA36-7EA8E8B37821", "Upload certificate of origin XML"));

	protected override bool IsMenuItemVisible(ZMenuItem menuItem)
	{
		return DataContext is ITEDIMessage
		{
			EM_LinkedObject: CusEntryHeader
			{
				IsExport: true,
				MovementReferenceNumber.IsEmpty: false
			}
		};
	}

	#region Implementation

	void UploadFile(CusEntryHeader entryHeader, Stream stream)
	{
		try
		{
			using var streamReader = new StreamReader(stream);
			var certificateOfOriginUploader = new CertificateOfOriginXmlUploader(entryHeader);
			certificateOfOriginUploader.Upload(streamReader);
			Globals.Message.Show(Res.GetString("ECECF5CD-2D57-40AC-A041-AC581F555689", "File has been uploaded."));
		}
		catch (NotSupportedException ex)
		{
			Globals.Message.ShowError(ex.Message);
		}
	}

	#endregion
}
