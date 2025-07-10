using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public class CompanyCredentialsPlugIn : MasterFiles.GUI.CompanyCredentialsPlugIn, Integration.Customs.AU.IAUCompanyCredentialsPlugIn
	{
		public CompanyCredentialsPlugIn(GlbCompany company) : base(company)
		{
			Company = company;
		}

		public GlbCompany Company { get; }

		protected override MenuItem GetNewTopLevelMenu()
		{
			var user = GlbStaff.CurrentUser;
			if (user.IsSupportUser && Form != null)
			{
				var menuText = ResString.GetMultilingualString("3C63BCCD-453A-43CC-82C3-4839A97A5383", "Increase Interchange Number");
				var menu = new ZMenuItem(menuText, IncreaseInterchangeNumberMenuItem_Click);
				menu.Name = "IncreaseInterchangeNumberMenuItem";
				ZFormMenuStrategy.AddActionsMenuItem(Form, menu);
			}
			return null;
		}

		void IncreaseInterchangeNumberMenuItem_Click(object sender, System.EventArgs e)
		{
			var interchangeNumberDetails = new InterchangeNumberDetails(Company);
			if (interchangeNumberDetails.CurrentInterchangeNumber.IsEmpty)
			{
				var errorMessage = ResString.GetMultilingualString("20D43428-BA44-493E-A702-8DD7A909C95D", "There are no existing messages for the EDI Site listed against this Company, please send a message before trying again");
				Globals.Message.ShowError(errorMessage, "Not Find a Interchange Number");
			}
			else
			{
				ZFormModaliser.ShowDialogAndDispose(new InterchangeNumberDetailsForm(Company));
			}
		}
	}
}
