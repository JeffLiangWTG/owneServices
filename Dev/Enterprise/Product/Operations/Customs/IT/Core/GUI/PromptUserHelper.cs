using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IT.GUI;

public static class PromptUserHelper
{
	public static ZDialogResult ShowEntryAmendmentConfirmation()
	{
		return Globals.Message.ShowConfirmation(EntryAmendmentConfirmationMessage, EntryAmendmentConfirmationCaption, EntryAmendmentConfirmationPrompt, EntryAmendmentConfirmationString, ZMessageBoxIcon.None);
	}

	public static string EntryAmendmentCompleteMessage => Res.GetString("B054F2BE-6B02-449D-8A65-F0BE3E4D1DE7", "One Entry was set to Amending");

	static string EntryAmendmentConfirmationMessage => Res.GetString("A1C64B05-2B70-4979-A2F0-49CB1C014E53", "Are you sure you want to set this Entry as AMENDING?");

	static string EntryAmendmentConfirmationCaption => Res.GetString("CCFBFAB2-D020-40BC-876D-7CCE9E68B137", "Amendment");

	static string EntryAmendmentConfirmationPrompt => Res.GetString("621C2B98-9437-4269-AD86-D32EBC39243A", "If you are absolutely sure you want to set this Entry as AMENDING, please type");

	static string EntryAmendmentConfirmationString => Res.GetString("A7357915-ACB8-4AD9-937D-F4020F4EF088", "confirm");
}
