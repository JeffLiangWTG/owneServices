using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class LoginOptions : RoutingEnabledPage
	{
		protected override bool PageRequiresLogin(Uri url) => false;
		protected override bool ShowLoginStatus => false;

		#region Binding

		protected override BusinessObject GetNewDataSource() => new LoginOptionsHelper(Factory, IdentityManager.Contact, IdentityManager.UserAccount);

		protected override bool IsPersistDataSourceBetweenPostbacks => true;

		#endregion Binding

		override protected void OnInit(EventArgs e)
		{
			InitializeComponent();
			base.OnInit(e);
		}

		void InitializeComponent()
		{
			CopyrightYear.Text = ZDateTime.Now.Year.ToString(CultureInfo.InvariantCulture);
		}
	}
}
