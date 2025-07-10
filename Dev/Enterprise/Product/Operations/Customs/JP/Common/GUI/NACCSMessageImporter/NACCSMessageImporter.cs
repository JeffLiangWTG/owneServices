using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.JP.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.Shared.GUI;

public class NACCSMessageImporter
{
	public NACCSMessageImporter(IZForm form, INACCSMessageImportSupporter supporter)
	{
		this.supporter = supporter;
		this.form = form;
		parentFinder = new NACCSMessageParentFinder();
	}

	readonly INACCSMessageImportSupporter supporter;
	readonly NACCSMessageParentFinder parentFinder;
	readonly IZForm form;

	public void ImportFromFile()
	{
		var dialogResult = DialogResult.Cancel;
		ZOpenFileDialog.FileInfo selectedFileInfo = null;
		using (var openFileDialog = new ZOpenFileDialog())
		{
			dialogResult = ZFormModaliser.ShowCommonDialogWithoutDispose(openFileDialog);
			selectedFileInfo = openFileDialog.SelectedFiles.FirstOrDefault();
		}
		if (dialogResult == DialogResult.OK)
		{
			try
			{
				byte[] messageData;
				using (var stream = selectedFileInfo.OpenFile())
				{
					messageData = stream.ReadFully();
					stream.Close();
				}
				messageData = Encoding.Convert(JPMessageUtils.MessageDataEncodingShiftJIS, JPMessageUtils.MessageDataEncoding, messageData);

				var factory = new BusinessObjectFactory();
				factory.RefreshEnabled = false;

				var linkParent = FindParent(factory, messageData, out var parentNotFoundErrorMessage);
				if (linkParent != null)
				{
					var message = EDIMessage.CreateFromInboundMessageLoad(factory, messageData);
					message.EM_LinkTable = linkParent.TableName;
					message.EM_LinkUniqueID = linkParent.PK;
					message.EM_ApplicationReference = EDIMessage.FlatFile;
					message.EM_Status = EDIMessage.Status.Queued;
					message.EM_GB = (linkParent as IBranchProvider)?.Branch?.PK ?? GlbBranch.CurrentBranch.PK;

					var processor = MessageProcessorFactory.GetMessageProcessor(message, new LoggingInformation());
					if (processor is NACCSMessageProcessor naccsProcessor)
					{
						naccsProcessor.ProcessMessage(message);
						var additionalWarning = naccsProcessor.AdditionalWarning;
						if (!additionalWarning.IsEmpty)
						{
							Globals.Message.ShowWarning(additionalWarning);
						}

						message.Factory.Save();
						var notificationMessage = Res.GetString("3AD182CA-33CF-4328-8DA6-236D3218A093", "Message has been successfully attached, please find by message number: {0}.\r\nDo you want to reload this form now to reflect the changes?", message.EM_MessageNum);
						var notificationCaption = Res.GetString("11F2681D-016B-4966-ADF9-594729C09314", "Reload Now?");
						if (Globals.Message.Show(notificationMessage, notificationCaption, MessageBoxButtons.YesNo, DialogResult.Yes) == DialogResult.Yes && form is ZForm mainForm)
						{
							mainForm.ReloadForm();
						}
					}
					else
					{
						Globals.Message.ShowError("There is not a processor for the message.");
					}
				}
				else
				{
					Globals.Message.ShowError(parentNotFoundErrorMessage, "Cannot import message");
				}
			}
			catch (JPMessageSchemaException ex)
			{
				Globals.Message.ShowError(Res.GetString("368E0FDF-D775-4AFA-B6ED-C08A5B717FAF", "The selected file is not a valid NACCS message:\r\n\r\n{0}", ex.Message));
			}
			catch (ZSaveException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
		}
	}

	BusinessObject FindParent(BusinessObjectFactory factory, byte[] messageData, out string parentNotFoundErrorMessage)
	{
		parentNotFoundErrorMessage = string.Empty;
		parentFinder.ClearErrorMessage();

		var possibleParent = new List<BusinessObject>() { parentFinder.FindParentFromMessageData(factory, messageData), parentFinder.FindParentFromSubject(factory, messageData) }
			.Where(parent => parent != null)
			.FirstOrDefault(parent => supporter?.IsValidParent(parent) ?? false);

		if (!string.IsNullOrEmpty(parentFinder.ParseErrorMessages))
		{
			parentNotFoundErrorMessage = parentFinder.ParseErrorMessages;
		}
		else
		{
			if (possibleParent == null)
			{
				parentNotFoundErrorMessage = Res.GetString("2912E5DC-CE34-4955-9992-C1B41E9B4281", "This message cannot be imported. It does not match this job by message reference, input reference nor subject.");
			}
		}

		return possibleParent;
	}
}
