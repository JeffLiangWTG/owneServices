#if DEBUG

namespace Enterprise.Accounting.GUI.PeriodManagement
{
	public partial class ComPayRegisteredOrganisationsControl
	{
		public void FindButton_Click_ForTestOnly(object sender, System.EventArgs e)
		{
			FindButton_Click(sender, e);
		}

		public Business.eNett.ComPayRegisteredOrganisationDataSource DataSource_ForTestOnly
		{
			get { return dataSource; }
			set { dataSource = value; }
		}
	}
}

#endif
