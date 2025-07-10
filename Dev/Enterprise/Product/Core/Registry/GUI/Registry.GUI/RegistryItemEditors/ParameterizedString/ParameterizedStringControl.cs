using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class ParameterizedStringControl : ZUserControl
	{
		public ParameterizedStringControl(ParameterizedStringRegistryItem registryItem)
		{
			this.registryItem = registryItem;
			InitializeComponent();
			Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
			valueTextBox.TextChanged += valueTextBox_TextChanged;
		}

		void valueTextBox_TextChanged(object sender, System.EventArgs e)
		{
			var info = new StringBuilder();
			for (int i = 0; i < ParameterDescriptions.Count; i++)
			{
				info.AppendLine(Res.GetString("16c127b7-d899-4521-8843-69042769fec3", "{0} will be replaced with the value of {1}", "{" + i.ToString(CultureInfo.InvariantCulture) + "}", ParameterDescriptions[i]));
				info.AppendLine(Res.GetString("2227def8-6935-4dd9-94c4-a08532e3ad40", "Preview:"));
				info.Append(registryItem.Deserialise(valueTextBox.Text));
			}
			informationTextBox.Text = info.ToString();
		}

		IList<MultilingualString> ParameterDescriptions
		{
			get { return ((ParameterizedStringRegistryItemEditorInfo)registryItem.EditorInfo).ParameterDescriptions; }
		}

		readonly ParameterizedStringRegistryItem registryItem;
	}
}
