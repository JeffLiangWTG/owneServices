using System;
using System.Drawing;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class ImporterDeclarationStateForm : ZChildForm
	{
		public ImporterDeclarationStateForm(MultilingualString state)
		{
			InitializeComponent();
			TitleLabel.Font = new Font(TitleLabel.Font.FontFamily, 18);
			StateLabel.Text = state;
		}

		public override string FormHeading => Res.GetString("474a1587-5bae-4166-919f-f274e4c18e06", "Importer Declaration State");

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
