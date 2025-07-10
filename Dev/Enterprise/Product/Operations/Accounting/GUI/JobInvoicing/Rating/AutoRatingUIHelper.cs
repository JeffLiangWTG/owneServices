using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public static class AutoRatingUIHelper
	{
		public static void SetNoNoteExistsErrorText(ZStmNotePopupButton button)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				button.NoNoteExistsError = Res.GetString("7dd11f26-4774-4f00-84f8-30d477f711d2", "You must perform Autorating in this session before you can see Full Autorating log information. This information is available until the form is closed.");
				button.NoNoteExistsError += System.Environment.NewLine + System.Environment.NewLine;
				button.NoNoteExistsError += Res.GetString("d21c44aa-26c9-4896-93eb-c32fd0144b03", "However, each charge line contains an Autorating description available on the 'Revenue Rate Audit' and 'Cost Rate Audit' tabs.");
			}
		}
	}
}
