using System;
using System.Linq;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Res = ZClientEDI.Res;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class AWSPrivateCAControl : RegistryZUserControl
	{
#if DEBUG
		internal ZArchitecture.ZGrid SystemToSystemPrivateCAGridForTest => Grid;
#endif

		public AWSPrivateCAControl()
		{
			InitializeComponent();
			SetupGridContextMenu();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			Grid.ReadOnly = readOnly;
		}

		void SetupGridContextMenu()
		{
			Grid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("22ED0916-0747-4141-86C6-A9D844BF912C", "Show Access Key"), ShowAccessKey));
			Grid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("6D75FB4D-4476-4F70-A6C2-1C3A3C27772E", "Show Secret Key"), ShowSecretKey));
		}

		void ShowAccessKey(object sender, EventArgs e)
		{
			ShowValue(AccessKey);
		}

		void ShowSecretKey(object sender, EventArgs e)
		{
			ShowValue(SecretKey);
		}

		internal void ShowValue(string key)
		{
			if (Grid.SelectedElements.Length == 1)
			{
				var systemToSystemPrivateCa = Grid.GetSelectedElements<AWSPrivateCA>().FirstOrDefault();
				if (DeveloperLoginForm.TryAuthenticate())
				{
					if (key == AccessKey)
					{
						Globals.Message.ShowInformation(systemToSystemPrivateCa.AccessKey, AccessKeyInfo);
						return;
					}

					if (key == SecretKey)
					{
						Globals.Message.ShowInformation(systemToSystemPrivateCa.SecretKey, SecretKeyInfo);
					}
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("44364DAF-9BB1-427D-BA40-32F015BD0CE9", "Please select one data."));
			}
		}

		const string AccessKey = "AccessKey";
		const string SecretKey = "SecretKey";
		string AccessKeyInfo => ResString.GetMultilingualString("A2A3FEF3-A290-4E5E-9DF0-EAFE6E5A835A", "Access-Key");
		string SecretKeyInfo => ResString.GetMultilingualString("DCAA2B65-8CC6-4C7D-AAD1-53A93ACAEB8A", "Secret-Key");
	}
}
