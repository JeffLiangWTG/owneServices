using System;
using System.Windows.Forms;
using CargoWise.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Licencing.GUI
{
	public partial class MessageChooserForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public MessageChooserForm()
		{
			InitializeComponent();
		}

		public MessageChooserForm(ICodeDescriptionPairList list)
			: base(null)
		{
			InitializeComponent();

			foreach (ICodeDescription item in list)
			{
				listBox.Items.Add(item.Description);
			}
		}

		public string SelectedText
		{
			get
			{
				var item = listBox.SelectedItem;
				return item != null ? item.ToString() : string.Empty;
			}
		}

		void listBox_DoubleClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}

