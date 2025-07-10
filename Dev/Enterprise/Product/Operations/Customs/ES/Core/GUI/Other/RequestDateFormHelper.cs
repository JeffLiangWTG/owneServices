using System.Windows.Forms;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public static class RequestDateFormHelper
	{
		public static ZDateTime GetDateFromRequestDateForm(ResourceString formTitle, ZString message, ZDateTime defaultDate, ResourceStringData dateCaption)
		{
			using (var dialog = new RequestDateForm(formTitle, message, defaultDate, dateCaption))
			{
				var answer = ZFormModaliser.ShowDialogWithoutDispose(dialog);
				return (answer == DialogResult.OK || answer == DialogResult.Yes) ? dialog.DateEdit.DateTimeValue : ZDateTime.Empty;
			}
		}

		public static ZDateTime GetDateFromRequestDateForm(ResourceString formTitle, ZString message, ZDateTime defaultDate)
		{
			return GetDateFromRequestDateForm(formTitle, message, defaultDate, null);
		}

		public static ZDateTime GetDateFromRequestDateForm(ResourceString formTitle, ZString message)
		{
			return GetDateFromRequestDateForm(formTitle, message, ZDateTime.Empty);
		}

		public static ZDateTime GetDateFromRequestDateForm(ResourceString formTitle, ZString message, ResourceStringData dateCaption)
		{
			return GetDateFromRequestDateForm(formTitle, message, ZDateTime.Empty, dateCaption);
		}
	}
}
