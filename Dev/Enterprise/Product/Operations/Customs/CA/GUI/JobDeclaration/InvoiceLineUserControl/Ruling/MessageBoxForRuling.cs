using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class MessageBoxForRuling : ZMessageBox
	{
		public MessageBoxForRuling()
			: base(Content, Caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question)
		{
			InitializeComponent();
			WithImporterRadioButton.Checked = true;
		}

		static string Caption => ResString.GetMultilingualString("2110FA3E-2C15-47CB-94BD-ABB9E5FF33B4", "Create Remission");

		static string Content => ResString.GetMultilingualString("C8905E91-45E3-4531-8A7D-526922479116", "The Special Authority Number does not exist in the Remissions Maintenance. Would you like to add it now?");

		public bool IsCreateForAllOrgs => WithoutImporterRadioButton.Checked;
	}
}
