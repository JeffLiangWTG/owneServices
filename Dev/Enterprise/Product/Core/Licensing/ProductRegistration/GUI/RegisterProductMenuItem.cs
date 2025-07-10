using System;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProductRegistration.GUI
{
	public static class RegisterProductMenuItem
	{
		public static void AddRegisterProductMenuItem(ZToolStripMenuItem parentMenu, Action afterClick)
		{
			var item = new ZToolStripMenuItem(DynamicCaption);
			item.Click += (sender, args) =>
			{
				OnClick(sender, args);
				if (afterClick != null)
				{
					afterClick();
				}
			};
			parentMenu.DropDownItems.Add(item);
		}

		static MultilingualString DynamicCaption
		{
			get
			{
				var rego = ObjectFactory.Get<IProductRegistration>();
				if (rego.LocalVerify() == ProductRegistrationVerifyResult.OK)
				{
					return UnregisterCaption;
				}
				else
				{
					return RegisterCaption;
				}
			}
		}

		static MultilingualString RegisterCaption
		{
			get { return ResString.GetMultilingualString("87A15B73-DFD7-4FDB-8E4B-02C5B48B920E", "Register Product");  }
		}

		static MultilingualString UnregisterCaption
		{
			get { return ResString.GetMultilingualString("33A23E86-E1AC-41A4-B47D-74BB7A23A17E", "Unregister Product"); }
		}

		static void OnClick(object sender, EventArgs e)
		{
			var user = EnvProxy.Instance.CurrentUser;
			if (IsAllowedToRegisterProduct(user))
			{
				using (var form = new UpdateRegistrationForm())
				{
					ZFormModaliser.ShowDialogWithoutDispose(form);
				}

				((ToolStripItem)sender).Text = DynamicCaption;
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("7370C0C8-ED02-4774-9269-D2ECB8D58BE8", "This requires a controller or a non-operational user, such as sysadmin, or a support user"),
					Res.GetString("DD6CDF3A-5AF3-4B71-A3D7-E10E097D1DCF", "Access Denied"));
			}
		}

		public static bool IsAllowedToRegisterProduct(IUser user)
		{
			return !user.IsOperational || user.LoggedInWithMasterPassword || user.IsController;
		}
	}
}
