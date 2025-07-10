using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class SimplifiedLVSForm : ZForm
	{
		public SimplifiedLVSForm(SimplifiedLVS simplifiedLVS)
			: base(simplifiedLVS)
		{
			InitializeComponent();
			if (!DesignModeFinder.IsDesigning)
			{
				this.saveButtonUserControl = new Core.Forms.ZPostingButtonsUserControl();
				ZFormPostingButtonsStrategy.SetupPosting(this, saveButtonUserControl);
			}
		}

		SimplifiedLVS SimplifiedLVS
		{
			get { return (SimplifiedLVS)base.DataSource; }
		}

		public SimplifiedLVSForm()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			EntryMode = initialEntryMode;
		}

#if DEBUG
		internal SimplifiedLVSForm(SimplifiedLVS simplifiedLVS, Mode initialEntryMode)
			: this(simplifiedLVS)
		{
			this.initialEntryMode = initialEntryMode;
		}
#endif
		readonly Mode initialEntryMode = Mode.CargoListHeader;

		public Mode EntryMode
		{
			get { return fEntryMode; }
			set
			{
				if (value == Mode.InvoiceDetails)
				{
					this.simplifiedLVSUserControl.LVSEntryVisible = true;
					if (!this.simplifiedLVSUserControl.LVSEntryVisible)
					{
						return;
					}
				}

				fEntryMode = value;
				var isInvoiceDetailsMode = EntryMode == SimplifiedLVSForm.Mode.InvoiceDetails;

				SimplifiedLVS.OnlyValidateCargoListHeaderProperty = EntryMode == Mode.CargoListHeader;
				this.simplifiedLVSUserControl.ShipmentHeaderVisible = EntryMode != Mode.CargoListHeader;
				this.simplifiedLVSUserControl.LVSEntryVisible = isInvoiceDetailsMode;

				this.GaveUpButton.CaptionResourceString = isInvoiceDetailsMode ? Res.GetData("C55B1DBF-F3D3-4A4A-B4F8-A54B4958A629", "&Cancel") : Res.GetData("C63865AB-ADA7-4C51-A7BE-E6AA33606154", "&Close");
				this.ContinueAndSaveButton.CaptionResourceString = isInvoiceDetailsMode ? Res.GetData("D62C2FA5-9816-4FBB-92A0-74076759E5EF", "&Save && Continue") : Res.GetData("C4A951AE-712A-4B2F-816D-9D8F8CBEEDEB", "Co&ntinue");
				this.continueAndSaveMenuItem.Caption = isInvoiceDetailsMode ? ResString.GetMultilingualString("C225D4DF-D7A8-4C1F-BA7B-80B445ED280F", "Save && Continue") : ResString.GetMultilingualString("3144E0EB-908F-4CAA-A315-7AF0F1A82A18", "Continue");
				this.quitMenuItem.Caption = isInvoiceDetailsMode ? ResString.GetMultilingualString("EE1B3AC1-9716-4695-825F-D30A6574C9DF", "Cancel") : ResString.GetMultilingualString("F2570B1B-FF65-4A25-BED9-46E0D9231933", "Close");

				this.GaveUpButton.UpdateCaption();
				this.ContinueAndSaveButton.UpdateCaption();
				this.CalculateDutyButton.Visible = isInvoiceDetailsMode;
				this.LastJobButton.Visible = EntryMode == Mode.ShipmentHeader;
				this.LastJobButton.Enabled = LastJobPK.IsValid;

				switch (EntryMode)
				{
					case Mode.CargoListHeader:
						this.simplifiedLVSUserControl.LVSCarrierCodeFindBox.Focus();
						CargoWise.Windows.UI.ControlDpiScalingHelper.SetHeight(this, cargoListHeaderModeHeight, true);
						break;
					case Mode.ShipmentHeader:
						this.simplifiedLVSUserControl.ImporterOrganisationControl.Focus();
						CargoWise.Windows.UI.ControlDpiScalingHelper.SetHeight(this, shipmentHeaderModeHeight, true);
						break;
					case Mode.InvoiceDetails:
						this.simplifiedLVSUserControl.LvsLinesUserControl.Focus();
						CargoWise.Windows.UI.ControlDpiScalingHelper.SetHeight(this, invoiceDetailsModeHeight, true);
						break;
				}
			}
		}
		Mode fEntryMode;
		readonly int cargoListHeaderModeHeight = 185;
		readonly int shipmentHeaderModeHeight = 300;
		int invoiceDetailsModeHeight = 800;

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			if (EntryMode == Mode.InvoiceDetails)
			{
				invoiceDetailsModeHeight = this.Height;
			}
		}

		public override ODisplayMode DisplayMode
		{
			get { return ODisplayMode.Edit; }
			set { }
		}

		void MoveToNextLVSEntry()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration);
			var newSimplifiedLVS = SimplifiedLVS.CreateNewSimplifiedLVS(controller.Factory);
			var newSimplifiedLVSForm = (SimplifiedLVSForm)controller.ShowFormForNewEntity(newSimplifiedLVS);
			if (SimplifiedLVS.Declaration != null)
			{
				newSimplifiedLVSForm.LastJobPK = SimplifiedLVS.Declaration.PK;
			}
			newSimplifiedLVSForm.EntryMode = Mode.ShipmentHeader;
#if DEBUG
			LastFormShownForTest = newSimplifiedLVSForm;
#endif
			SimplifiedLVS.Invoices.RemoveAll();
			SimplifiedLVS.SupplierDocumentaryAddress.Delete();
		}

		readonly Core.Forms.ZPostingButtonsUserControl saveButtonUserControl;
#if DEBUG
		internal IZForm LastFormShownForTest;
#endif

		protected override void PerformValidation()
		{
			base.PerformValidation();

			this.simplifiedLVSUserControl.SimplifiedLVSHeaderDetailsUserControl.SupplierDocAddressControl.ForceBindingIncludingParents();
			this.simplifiedLVSUserControl.SimplifiedLVSHeaderDetailsUserControl.ShipperDocAddressControl.ForceBindingIncludingParents();
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();
			if (result == ContinueWithSave.Yes && EntryMode == Mode.InvoiceDetails)
			{
				if (SimplifiedLVS.Invoices.Count == 0 || SimplifiedLVS.Invoices[0].InvoiceLines.Count == 0)
				{
					Globals.Message.ShowInformation(ResString.GetMultilingualString("8086C827-C175-4D03-9065-D722BB290FFA", "You have not entered LVS Shipment Details."),
						ResString.GetMultilingualString("FED30FFB-8C2B-4BB2-99C6-651A02B1C227", "No LVS Shipment Details information."));
					result = ContinueWithSave.No;
				}

				if (result == ContinueWithSave.Yes && SimplifiedLVS.HasMessageErrors)
				{
					result = Globals.Message.Show(ResString.GetMultilingualString("eeabb518-67c8-49a4-a496-e08bb82e9616", "There are message errors, are you sure you want to save? If you press YES the job will be saved with these message errors which should be fixed later, or if you press NO then you will have the opportunity to fix the errors now and then save."),
						ResString.GetMultilingualString("a431b17e-7431-45bf-a9a6-ddd8174be862", "Save With Message Errors"),
						MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.No) == DialogResult.Yes ? ContinueWithSave.Yes : ContinueWithSave.No;
				}

				if (result == ContinueWithSave.Yes && SimplifiedLVS.BuyerSupplierLinksHelper.ShouldPromptToSaveSupplierBuyerRelationship)
				{
					if (ShowConfirmationForNewSupplierBuyerRelationship() == DialogResult.Yes)
					{
						SimplifiedLVS.BuyerSupplierLinksHelper.AddNewBuyerSupplierLink();
					}
				}
			}
			return result;
		}

		void ContinueAndSaveButton_Click(object sender, EventArgs e)
		{
			if (EntryMode != Mode.InvoiceDetails)
			{
				SimplifiedLVS.RunPreSaveValidation();
				if (SimplifiedLVS.HasErrors)
				{
					ShowErrorsDialog();
				}
				else
				{
					EntryMode++;
				}
			}
			else
			{
				var declaration = SimplifiedLVS.Declaration;
				if (declaration != null)
				{
					declaration.ResumeApportionment();

					if (!declaration.IsLVX || declaration.DoMerge(new CASendsMessagesToCustomsGUI()))
					{
						this.FormClosed -= SimplifiedLVSForm_FormClosed;
						this.FormClosed += SimplifiedLVSForm_FormClosed;
						try
						{
							this.OnPostButtonClick(sender, e);
						}
						finally
						{
							this.FormClosed -= SimplifiedLVSForm_FormClosed;
						}
					}
				}
			}
		}

		DialogResult ShowConfirmationForNewSupplierBuyerRelationship()
		{
			return Globals.Message.Show(Res.GetString("fe482f0f-d575-4a92-a79e-be09fbeeda4e", "Do you wish to save this Supplier/Buyer relationship?"), Res.GetString("63340755-d601-4a28-a808-69833a5ed2ad", "Save"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);
		}

		void SimplifiedLVSForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			MoveToNextLVSEntry();
		}

		void Cancel_Click(object sender, EventArgs e)
		{
			if (EntryMode != Mode.CargoListHeader)
			{
				EntryMode--;
			}
			else
			{
				this.Close();
			}
		}

		protected override void ZForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (EntryMode == Mode.InvoiceDetails)
			{
				base.ZForm_Closing(sender, e);
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			if (disposing)
			{
				saveButtonUserControl.Dispose();
			}
			base.Dispose(disposing);
		}

		void CalculateDutyButton_Click(object sender, EventArgs e)
		{
			if (SimplifiedLVS.Invoices.Count > 0)
			{
				var declaration = SimplifiedLVS.Invoices[0].JobDeclaration;
				declaration.ResumeApportionment();
			}
		}

		void LastJobButton_Click(object sender, EventArgs e)
		{
			if (LastJobPK.IsValid)
			{
				var declaration = SimplifiedLVS.Factory.Load<JobDeclaration>(LastJobPK);
				if (declaration != null && declaration.IsInDatabase)
				{
#if DEBUG
					LastFormShownForTest =
#endif
					ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration).ShowEditForm(declaration);
				}
			}
		}

		internal ZGuid LastJobPK;

		internal ZMenuItem continueAndSaveMenuItem;
		internal ZMenuItem quitMenuItem;
		protected override void AddAdornments()
		{
			base.AddAdornments();
			var fileMenuItem = this.Menu.MenuItems[ZFormMenuStrategy.FileMenuItemName];

			var fileSaveMenuItem = fileMenuItem.MenuItems[ZFormMenuStrategy.FileSaveMenuItemName];
			continueAndSaveMenuItem = new ZMenuItem(ResString.GetMultilingualString("FBF0A3B0-B5F4-4556-9E6F-A346BC1D06F4", "Continue"));
			continueAndSaveMenuItem.Shortcut = Shortcut.CtrlS;
			continueAndSaveMenuItem.Enabled = true;
			continueAndSaveMenuItem.Click += delegate
			{ this.ContinueAndSaveButton.PerformClick(); };
			fileMenuItem.MenuItems.Replace(fileSaveMenuItem, new[] { continueAndSaveMenuItem });

			var closeMenuItem = fileMenuItem.MenuItems[ZFormMenuStrategy.FileCloseMenuItemName];
			quitMenuItem = new ZMenuItem(ResString.GetMultilingualString("8C3E30F7-91CE-4456-B6D0-19E9EC025456", "Close"));
			quitMenuItem.Shortcut = Shortcut.CtrlQ;
			quitMenuItem.Enabled = true;
			quitMenuItem.Click += delegate
			{ this.GaveUpButton.PerformClick(); };
			fileMenuItem.MenuItems.Replace(closeMenuItem, new[] { quitMenuItem });
		}

		public enum Mode
		{
			CargoListHeader = 0,
			ShipmentHeader = 1,
			InvoiceDetails = 2
		}
	}
}
