using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.GuidTextBox.Internals
{
	interface IButtonWithHelper
	{
		void SetHelper(AutoCompleteHelper helper);
		ZAutoCompleteTextBox AutoCompleteTextBox { get; set; }
	}
}
