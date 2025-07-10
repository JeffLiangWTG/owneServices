using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public sealed class ResponseFileManualUploaderGridContextMenuItemComponent : GridContextMenuItemComponent<ITEDIMessage>
{
	public ResponseFileManualUploaderGridContextMenuItemComponent(ZGrid grid) : base(grid)
	{
	}

	protected override ZMenuItem GetMenuItem() => new ZMenuItem(ResString.GetMultilingualString("FB5EC717-51D6-4292-B047-5CCDA3371196", "Upload Response file"));

	protected override void Execute(ZMenuItem menuItem)
	{
		using (var openFileDialog = new ZOpenFileDialog())
		{
			openFileDialog.Filter = (NoResString)"Customs Response Files (*.U*,*.X*)|*.U*;*.X*|All files (*.*)|*.*";
			openFileDialog.RestoreDirectory = true;
			openFileDialog.CheckFileExists = true;
			openFileDialog.CheckPathExists = true;

			if (ZFormModaliser.ShowCommonDialogWithoutDispose(openFileDialog) == DialogResult.OK)
			{
				ProcessExternalResponseMessage(openFileDialog);
			}
		}
	}

	protected override bool IsMenuItemVisible(ZMenuItem menuItem)
	{
		var currentMessage = DataContext;

		return base.IsMenuItemVisible(menuItem)
			&& currentMessage?.Interchange != null
			&& (currentMessage.EM_MessageType == SADConstants.CustomsInterchangeType.IdocR || currentMessage.EM_MessageType == SADConstants.CustomsInterchangeType.IdocT);
	}

	void ProcessExternalResponseMessage(ZOpenFileDialog openFileDialog)
	{
		try
		{
			using (var streamReader = new StreamReader(openFileDialog.OpenFile(), Encoding.UTF8))
			{
				var responseFileProcessor = new ResponseFileManualUploader(DataContext, streamReader.ReadToEnd());
				responseFileProcessor.ProcessExternalResponseMessage();
			}
			Globals.Message.Show(Res.GetString("874BDE7F-8853-4559-819A-5B4DDB3C0078", "Customs response file '{0}' has been uploaded and waiting to be processed. Please refresh the form to view the results in the grid.", Path.GetFileName(openFileDialog.UnmappedFileName)));
		}
		catch (ZSaveException ex)
		{
			ZExceptionReporting.HandleSaveException(ex);
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
			Globals.Message.ShowError(ex.Message);
		}
	}

	protected override ITEDIMessage GetDataContext()
	{
		return Grid.GetCurrent() as ITEDIMessage;
	}
}
