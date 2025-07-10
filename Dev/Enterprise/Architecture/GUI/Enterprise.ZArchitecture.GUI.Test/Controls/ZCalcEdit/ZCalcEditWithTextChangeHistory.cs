using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZCalcEditWithTextChangeHistory : ZCalcEdit
	{
		[BindingMetaDataProperty(MetaDataTypes.MaxLength, "MaxLength", Enabled = false)]
		public override string Text
		{
			get { return base.Text; }
			set
			{
				if (base.Text != value)
				{
					base.Text = value;
					decimalChangeHistory += value + " ";
				}
			}
		}

		public string decimalChangeHistory = "";
	}
}
