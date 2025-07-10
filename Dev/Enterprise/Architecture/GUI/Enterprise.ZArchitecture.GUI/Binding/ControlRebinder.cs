using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	public class ControlRebinder
	{
		public void Rebind(Control control, string searchString, string replaceString, bool useGlobalReplaceInsteadOfStartsOnly = false)
		{
			if (string.IsNullOrEmpty(searchString))
			{
				throw new ArgumentException("Search string was blank", nameof(searchString));
			}
			SearchString = searchString;
			ReplaceString = replaceString;
			UseGlobalReplaceInsteadOfStartsOnly = useGlobalReplaceInsteadOfStartsOnly;
			RebindInternal(control);
		}
		string SearchString;
		string ReplaceString;

		void RebindInternal(Control control)
		{
			var currentBindTo = control.GetBindingMember();
			if (!UseGlobalReplaceInsteadOfStartsOnly && currentBindTo.StartsWith(SearchString, StringComparison.OrdinalIgnoreCase))
			{
				control.SetBindingMember(ReplaceString + currentBindTo.Remove(0, SearchString.Length));
			}
			else if (UseGlobalReplaceInsteadOfStartsOnly && currentBindTo.Contains(SearchString))
			{
				control.SetBindingMember(currentBindTo.Replace(SearchString, ReplaceString));
			}
			foreach (Control innerControl in control.Controls)
			{
				RebindInternal(innerControl);
			}
		}

		bool UseGlobalReplaceInsteadOfStartsOnly;
	}
}
