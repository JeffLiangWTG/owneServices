using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.JP.Common;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.Shared.GUI;

public interface IMessageSendingForm
{
	void UpdateDialogResult(DialogResult result);
}

public static class Utils
{
	public static void ShowEditableForm<T, U>(this U form, T parentProvider)
		where T : IMessageVisualObjectParentProvider
		where U : ZForm, IMessageSendingForm
	{
		if (parentProvider.Context.EnableMessageVisual)
		{
			parentProvider.UseVisualData = false;

			var contentProviders = parentProvider.GetContentProviders();
			var count = contentProviders.Count();

			switch (form.ShowPreviewForm(count, parentProvider.Context))
			{
				case DialogResult.Yes:
					parentProvider.VisualObjectParent.InitializeVisualObjects(contentProviders);
					form.UpdateDialogResult(ZFormModaliser.ShowDialogAndDispose(new EditableMessageSendingForm<T>(parentProvider), form));
					break;

				case DialogResult.No:
					form.UpdateDialogResult(DialogResult.OK);
					break;

				default:
					form.UpdateDialogResult(DialogResult.Cancel);
					break;
			}
		}
	}

	static DialogResult ShowPreviewForm(this Form form, int objsCountForSending, IMessageSendingContext context)
	{
		var result = DialogResult.None;
		var isNormalSendingTarget = context.SendTarget == SendTarget.Normal;

		var caption = isNormalSendingTarget ? Res.GetString("1F30A322-5688-4D5C-BEA4-AE528FE323C6", "Preview and modify before sending?")
			: Res.GetString("00EB30B2-4840-4C40-A011-45B7CE1BBB77", "Preview and modify before exporting?");

		var exactMessageWord = objsCountForSending == 1 ? "1 message" : $"{objsCountForSending} messages";

		var message = isNormalSendingTarget ? Res.GetString("1F30A322-5688-4D5C-BEA4-AE528FE323C7", "There will {0} in total to be sent. Would you like to preview them and modify before sending to customs?", exactMessageWord)
			: Res.GetString("063C91E6-5D18-4B2C-912E-8E788F5B7699", "There will {0} in total to be exported. Would you like to preview them and modify before exporting?", exactMessageWord);
		var sendText = isNormalSendingTarget ? Res.GetString("16A8DABD-6C87-468D-BF31-618477B629B4", "Send") : Res.GetString("614FCE50-E42C-48C6-B110-94A79E0564D6", "Export");
		var previewText = Res.GetString("1F30A322-5688-4D5C-BEA4-AE528FE323C9", "Preview");

		using (var msgBox = new ZMessageBox(message, caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, previewText, sendText))
		{
			result = ZFormModaliser.ShowMessageBoxWithoutDispose(msgBox, form);
		}

		return result;
	}
}
