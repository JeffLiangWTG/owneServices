using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.GUI
{
	public partial class JobDeclarationForm : BaseJobDeclarationForm
	{
		public JobDeclarationForm()
			: base()
		{
		}

		public JobDeclarationForm(JobDeclaration declaration)
			: base(declaration)
		{
			if (declaration.IsB2Adjustments)
			{
				ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1528, 864);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			Saved += OnSaved;
			HideMenuItemsForB2IM2B3X();
		}

		void OnSaved(object sender, EventArgs eventArgs)
		{
			Declaration.ResendDeferredMessageIfRequired();
			Declaration.AskForMessageActionIfMVFEventPosted();
		}

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			base.InitialiseForm();
		}

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override IEDIMenu GetNewTopLevelMenuCore() => new EDIMenu();

		protected override BaseCustomsBrokerageUserControl GetBrokerageUserControl()
		{
			var declaration = Declaration;
			if (declaration.IsLVX)
			{
				return new LVXCustomsBrokerageUserControl();
			}
			else if (declaration.IsLVS)
			{
				return new LowValueShipmentsUserControl();
			}
			else if (declaration.IsB2Adjustments || declaration.IsB3X)
			{
				return new B2AdjustmentsCustomsBrokerageUserControl();
			}
			else if (declaration.IsIM2)
			{
				return new IM2CustomsBrokerageUserControl();
			}
			else
			{
				return new CustomsBrokerageUserControl();
			}
		}

		protected override bool EnableControlLock
		{
			get
			{
				var declaration = Declaration;
				return !declaration.IsLVX && !declaration.IsLVS && !declaration.IsIM2 && !declaration.IsB3X && !declaration.IsB2Adjustments;
			}
		}

		protected override void AddPlugins()
		{
			var declaration = Declaration;
			if (declaration.IsLVS)
			{
				PlugIns.AddJobInvoicing(declaration.InvoicingSupporter);
				PlugIns.Add(ControllerIDs.eDocsPlugIn);
				PlugIns.Add(ControllerIDs.DocDataPlugIn);
			}
			else if (declaration.IsB2Adjustments || declaration.IsB3X)
			{
				PlugIns.AddJobInvoicing(declaration.InvoicingSupporter);
				PlugIns.Add(ControllerIDs.DocAddresses);
				PlugIns.Add(ControllerIDs.eDocsPlugIn);
				PlugIns.Add(ControllerIDs.DocDataPlugIn);
			}
			else if (declaration.IsIM2)
			{
				PlugIns.AddJobInvoicing(Declaration.InvoicingSupporter);
				PlugIns.Add(ControllerIDs.DocAddresses);
				PlugIns.Add(ControllerIDs.eDocsPlugIn);
				PlugIns.Add(ControllerIDs.DocDataPlugIn);
			}
			else
			{
				base.AddPlugins();
			}
		}

		void HideMenuItemsForB2IM2B3X()
		{
			var shouldHide = Declaration.IsB2OrIM2OrB3X;
			var uCopyMenu = ActionsMenuItem.MenuItems.FindByName("UniversalCopy");
			if (uCopyMenu != null)
			{
				uCopyMenu.Visible = !shouldHide;
			}

			var commercialInvoiceMenu = this.TopLevelMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "Commercial &Invoices");
			if (commercialInvoiceMenu != null)
			{
				commercialInvoiceMenu.Visible = !shouldHide;
			}
		}

		protected override SendsMessagesToCustomsGUI GetNewMessagingActionsController() => new MessagingActionsController();

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();
			if (result == ContinueWithSave.Yes)
			{
				result = Declaration.AskForB3ActionIfDeferredMessageExists();
			}
			return result;
		}

		protected override BaseShipmentAndBrokerageCommon GetShipmentAndBrokergeCommon(Customs.Business.BaseJobDeclaration declaration)
		{
			return new ShipmentAndBrokerageCommon(declaration);
		}

		protected override IEnumerable<PreSaveDialogStrategy> GetPreSaveDialogStrategies()
		{
			foreach (var strategy in base.GetPreSaveDialogStrategies())
			{
				yield return strategy;
			}

			var declaration = Declaration;

			if (declaration.IsIM2 && declaration.PreviousJob != null || declaration.IsB2Adjustments || declaration.IsB3X)
			{
				yield return new CalculateTotalForIM2OrB3XB2Strategy(declaration);
			}
			else if (declaration.IsLVX)
			{
				yield return new LVXPreSaveDialogStrategy(declaration);
			}
			else if (declaration.IsCSA)
			{
				yield return new CSAManualReleaseStrategy(declaration);
			}
			else if (declaration.JE_MessageType == JobMessageTypeList.Codes.Import || declaration.JE_MessageType == JobMessageTypeList.Codes.LowValueShipments)
			{
				var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.CommercialAccountingDeclaration);
				if (entryHeader != null && entryHeader.CH_EntryStatus == CADEntryStatusList.Codes.Approved
					&& (entryHeader.DutyFeeChangedSinceLastResponse || entryHeader.MergedLines.Any(x => x.CL_CommoditySequence.IsEmpty)))
				{
					yield return new CADPreSaveDialogStrategy(declaration);
				}
			}
		}
	}
}
