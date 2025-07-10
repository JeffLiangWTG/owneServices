using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class CMRMessagingForm : ZTemplateForm
	{
		protected CMRMessagingForm(IBusiness businessEntity) : base(businessEntity)
		{
			SetupMenuItem();
		}

		protected CMRMessagingForm()
			: base()
		{
		}

		MultiMessageManager manager;

		protected internal MultiMessageManager Manager
		{
			get
			{
				if (manager == null)
				{
					manager = GetManager();
				}
				return manager;
			}
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			if (BusinessEntity is CusOutturn || BusinessEntity is CusOutturnHeader)
			{
				return base.ShowPreSaveDialogs();
			}

			ContinueWithSave result = base.ShowPreSaveDialogs();
			if (result == ContinueWithSave.Yes && Manager != null)
			{
				result = GetNewMessagingActionsController().DetermineRequiredMessagesAndSendThem(Manager);
			}
			return result;
		}

		protected virtual SendsMessagesToCustomsGUI GetNewMessagingActionsController()
		{
			return new SendsMessagesToCustomsGUI();
		}

		void SetupMenuItem()
		{
			MenuItem messagingMenu = GetMessagingMenu();
			Menu.MenuItems.Add((Menu.MenuItems.Count - 1), messagingMenu);
		}

		protected virtual MenuItem GetMessagingMenu()
		{
			throw new ApplicationException("Override Me!");
		}

		protected virtual MultiMessageManager GetManager()
		{
			throw new ApplicationException("Override Me!");
		}
	}
}
