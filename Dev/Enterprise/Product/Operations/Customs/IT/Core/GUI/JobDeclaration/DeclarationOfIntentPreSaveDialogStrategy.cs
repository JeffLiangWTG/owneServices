using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IT.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IT.GUI;

public class DeclarationOfIntentPreSaveDialogStrategy : PreSaveDialogStrategy
{
	public DeclarationOfIntentPreSaveDialogStrategy(IDeclarationOfIntentRefresher doiRefresher)
	{
		DOIRefresher = Argument.NotNull(doiRefresher, nameof(doiRefresher));
	}

	IDeclarationOfIntentRefresher DOIRefresher { get; }

	protected override ContinueWithSave RunPreSaveAction()
	{
		DOIRefresher.OverwritePlaceholderSupportingDocumentValues();
		return ContinueWithSave.Yes;
	}

	protected override bool ShouldRunPreSaveAction()
	{
		return DOIRefresher.ShouldAskToOverwrite() && Globals.Message.Show(UserNotificationMessage, UserNotificationCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes) == DialogResult.Yes;
	}

	static string UserNotificationMessage => Res.GetString("1EA0AFBE-A3B3-4441-A135-B7F9EA000F22", "Do you want to update the data for document 01DI with a placeholder ('X')?");
	static string UserNotificationCaption => Res.GetString("31A3814A-5ED1-4709-92FC-04E8CA848ACF", "Overwrite 01DI supporting documents");
}
