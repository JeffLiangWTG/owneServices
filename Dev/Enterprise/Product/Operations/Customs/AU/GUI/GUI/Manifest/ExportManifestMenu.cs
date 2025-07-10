using System;
using System.Threading;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using ResString = Enterprise.Customs.AU.Declaration.GUI.ResString;

namespace Enterprise.Customs.AU.ExportManifest.GUI
{
	public class ExportManifestMenu : KMenuItem
	{
		public static ExportManifestMenu GetMenu()
		{
			ExportManifestMenu result;
			Type concreteType = TypeDecider.GetTypeForBinding(typeof(ExportManifestMenu));
			if (concreteType == typeof(ExportManifestMenu))
			{
				result = new ExportManifestMenu();
			}
			else
			{
				result = (ExportManifestMenu)Activator.CreateInstance(concreteType);
			}
			return result;
		}

		protected ExportManifestMenu()
		{
			this.Text = "ED&I";
			SetupTopLevelMenu();
		}

		protected MenuItem fEdificeMenuItem;
		public MenuItem EdificeMenuItem
		{
			get { return fEdificeMenuItem; }
			set { fEdificeMenuItem = value; }
		}

		public MenuItem SubmitDeclarationMenuItem;

		public MenuItem DeclareManifestMenuItem;
		public MenuItem WithdrawManifestMenuItem;
		public MenuItem ResetToOriginalMenuItem;

		public MenuItem DeclareDepartureReportMenuItem;
		public MenuItem WithdrawDepartureReportMenuItem;

		public MenuItem MessagingHelpMenuItem;

		public MenuItem Separator;

		public MenuItem ImportDataMenuItem;
		public ExportCustomsManifestHeader Header;

		#region Implementation

		protected void SetupTopLevelMenu()
		{
			DeclareManifestMenuItem = new ZMenuItem(ResString.GetMultilingualString("Customs.AU.ExportManifestMenu.DeclareManifest", "&Declare Manifest"), new EventHandler(DeclareManifestMenuHandler));
			WithdrawManifestMenuItem = new ZMenuItem(ResString.GetMultilingualString("Customs.AU.ExportManifestMenu.WithdrawManifest", "&Withdraw Manifest"), new EventHandler(WithdrawManifestMenuHandler));
			ResetToOriginalMenuItem = new ZMenuItem(ResString.GetMultilingualString("Customs.AU.ExportManifestMenu.ResetManifest", "&Reset to original"), new EventHandler(ResetToOriginalMenuHandler));
			MenuItems.Add(DeclareManifestMenuItem);
			MenuItems.Add(WithdrawManifestMenuItem);
			MenuItems.Add(ResetToOriginalMenuItem);

			DeclareDepartureReportMenuItem = new ZMenuItem(ResString.GetMultilingualString("Customs.AU.ExportManifestMenu.DeclareDepartureReport", "&Declare Departure Report"), new EventHandler(DeclareDepartureReportMenuHandler));
			WithdrawDepartureReportMenuItem = new ZMenuItem(ResString.GetMultilingualString("Customs.AU.ExportManifestMenu.WithdrawDepartureReport", "&Withdraw Departure Report"), new EventHandler(WithdrawDepartureReportMenuHandler));
			MenuItems.Add(DeclareDepartureReportMenuItem);
			MenuItems.Add(WithdrawDepartureReportMenuItem);

			MessagingHelpMenuItem = new ZMenuItem(CMRMessage.MessagingHelpMenuCaption, new EventHandler(MessagingHelp_Click));
			MenuItems.Add(MessagingHelpMenuItem);

			Separator = new ZMenuItem("-");
			MenuItems.Add(Separator);

			ImportDataMenuItem = new ZMenuItem(ResString.GetMultilingualString("Customs.AU.ExportManifestMenu.ImportData", "&Import Data"), new EventHandler(ImportDataMenuHandler));
			MenuItems.Add(ImportDataMenuItem);

			SetDepartureMenuItemVisibility(false);
			SetManifestMenuItemVisibility(false);
			MessagingHelpMenuItem.Visible = false;
			Separator.Visible = false;
		}

		#region Menu Items

		internal void ShowManifestMenuItems()
		{
			SetManifestMenuItemVisibility(true);
			SetDepartureMenuItemVisibility(false);
			MessagingHelpMenuItem.Visible = true;
		}

		internal void ShowDepartureMenuItems()
		{
			SetManifestMenuItemVisibility(false);
			SetDepartureMenuItemVisibility(true);
			MessagingHelpMenuItem.Visible = true;
		}

		internal void ShowOld()
		{
			SetManifestMenuItemVisibility(true);
			SetDepartureMenuItemVisibility(true);
			MessagingHelpMenuItem.Visible = false;
		}

		void SetManifestMenuItemVisibility(bool state)
		{
			DeclareManifestMenuItem.Visible = state;
			WithdrawManifestMenuItem.Visible = state;
			Separator.Visible = true;
		}

		void SetDepartureMenuItemVisibility(bool state)
		{
			DeclareDepartureReportMenuItem.Visible = state;
			WithdrawDepartureReportMenuItem.Visible = state;
			Separator.Visible = true;
		}

		#endregion

		#region Menu Handlers

		CancellationTokenSource CancellationTokenSource => cancellationTokenSource ?? (cancellationTokenSource = new CancellationTokenSource()); // Bind this to a cancel button.
		CancellationTokenSource cancellationTokenSource;

		void ResetToOriginalMenuHandler(object sender, EventArgs e)
		{
			if (Header != null)
			{
				Header.MessageManager.ResetToOriginal(new SendsMessagesToCustomsGUI());
			}
		}

		void DeclareManifestMenuHandler(object sender, EventArgs e)
		{
			if (Header != null)
			{
				Header.MessageManager.DeclareManifest(new SendsMessagesToCustomsGUI(), CancellationTokenSource.Token);
			}
		}

		void WithdrawManifestMenuHandler(object sender, EventArgs e)
		{
			if (Header != null)
			{
				Header.MessageManager.WithdrawManifest(new SendsMessagesToCustomsGUI(), CancellationTokenSource.Token);
			}
		}

		void DeclareDepartureReportMenuHandler(object sender, EventArgs e)
		{
			if (Header != null)
			{
				Header.MessageManager.DeclareDepartureReport(new SendsMessagesToCustomsGUI(), CancellationTokenSource.Token);
			}
		}

		void WithdrawDepartureReportMenuHandler(object sender, EventArgs e)
		{
			if (Header != null)
			{
				Header.MessageManager.WithdrawDepartureReport(new SendsMessagesToCustomsGUI(), CancellationTokenSource.Token);
			}
		}

		protected virtual void ImportDataMenuHandler(object sender, EventArgs e)
		{
			ExportCustomsManifestHeader tempHeader = Header;
			new ManifestDataImporter().DoImport(tempHeader.Factory, ref tempHeader);
		}

		void MessagingHelp_Click(object sender, EventArgs e)
		{
			WebUrlLauncher.Launch(CMRMessage.MessagingHelpUpdateNoteURL);
		}

		#endregion

		#endregion
	}
}
