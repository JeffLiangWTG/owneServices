using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	class ZDummyTextBox : ZTextBoxBase
	{
		protected override string ValidationPattern
		{
			get { return fValidationPattern; }
		}
		string fValidationPattern = "";

		public void SetValidationPattern(string pattern)
		{
			fValidationPattern = pattern;
		}

		protected override IZType SelectedValue
		{
			get { return (ZString)Text; }
			set { Text = (value != null) ? value.ToString() : ""; }
		}
	}
}
