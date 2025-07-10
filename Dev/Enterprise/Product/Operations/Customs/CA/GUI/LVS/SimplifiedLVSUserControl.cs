using System;
using System.Windows.Forms;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class SimplifiedLVSUserControl : ZUserControl
	{
		public SimplifiedLVSUserControl()
		{
			InitializeComponent();
			this.LvsLinesUserControl.SetSimplifiedLVSMode();
		}

		SimplifiedLVS SimLVS
		{
			get { return (SimplifiedLVS)base.DataSource; }
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			if (SimLVS != null)
			{
				SimLVS.CA_PortOfClearanceInfo.ValueChanged += CA_PortOfClearance_ValueChanged;
			}
		}

		void CA_PortOfClearance_ValueChanged(object sender, EventArgs e)
		{
			this.PortCodeFindBox.CurrentCode = SimLVS.CA_PortOfClearance;
		}

		internal bool LVSEntryVisible
		{
			get { return this.DetailsPanel.Visible; }
			set
			{
				var oldValue = LVSEntryVisible;
				if (oldValue != value)
				{
					if (value)
					{
						if (SimLVS != null)
						{
							if (SimLVS.Invoices.Count == 0)
							{
								SimLVS.LoadInvoiceHeader(
								() =>
								{
									var importerAddInfo = OrgImpAddInfo.Get(SimLVS.Importer);
									if (importerAddInfo != null && importerAddInfo.HasAccountSecurityNumber)
									{
										Globals.Message.ShowWarning(Res.GetString("8a7d51af-cd99-4961-8120-59698014697f", "The system could not find a matched Type F Consolidation declaration, and the entered Importer Organization has an Importer Account Security Code specified. Please create the Consolidation by Importer LVS job for this Importer using the normal LVS Data entry."));
										return false;
									}

									return Globals.Message.Show(
										ResString.GetMultilingualString("B759881E-5A9A-4C6A-BB38-7259A4BDEFF1", "The system could not find a matched Type F Consolidation declaration. Do you want to create a new Type F Consolidation declaration now?"),
										ResString.GetMultilingualString("7799B8B6-C9ED-4FDA-8B6A-3C6C6D0F5AE7", "Create a new Type F Consolidation declaration?"), MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == System.Windows.Forms.DialogResult.OK;
								});
							}
							if (SimLVS.Invoices.Count > 0)
							{
								this.DetailsPanel.Visible = true;
								var declaration = SimLVS.Declaration;
								if (declaration != null)
								{
									if (declaration.IsLVX)
									{
										this.HeaderDetailsGroupBox.Text = Res.GetString("fca3e3dd-35db-4fe2-8912-561465110a48", "Courier LVS Declaration");
									}
									else
									{
										if (declaration.IsInDatabase)
										{
											this.HeaderDetailsGroupBox.Text = Res.GetString("552ca06e-71a6-44a3-9122-9d9ae50a20de", "Existing Type F Consolidation Selected")
												+ string.Format(" ({0})", declaration.JE_DeclarationReference);
										}
										else
										{
											this.HeaderDetailsGroupBox.Text = Res.GetString("ca338288-2263-4339-9713-beb93e45d207", "New Type F Consolidation Created");
										}
									}
									ChangeHeaderDetailControlVisiblity(declaration.IsLVX);
								}
								SimLVS.RefreshBinding();
							}
						}
					}
					else
					{
						this.DetailsPanel.Visible = false;
						this.HeaderDetailsGroupBox.Text = Res.GetString("229f16fe-5561-46bc-9ffa-460fcf96e02d", "Header Details");
						if (SimLVS != null && SimLVS.Invoices.Count > 0)
						{
							var invoice = SimLVS.Invoices[0];
							SimLVS.Invoices.Remove(invoice);
							if (!invoice.JobDeclaration.IsInDatabase)
							{
								invoice.JobDeclaration.Delete();
							}
							else if (!invoice.IsInDatabase)
							{
								invoice.Delete();
							}
							SimLVS.RefreshBinding();
						}
					}
				}
			}
		}

		internal bool ShipmentHeaderVisible
		{
			get { return this.ShipmentHeaderGroupBox.Visible; }
			set
			{
				var oldValue = ShipmentHeaderVisible;
				if (oldValue != value)
				{
					this.ShipmentHeaderGroupBox.Visible = value;
				}
			}
		}

		void ChangeHeaderDetailControlVisiblity(bool isLVX)
		{
			this.DeliveryAddressTabPage.TabVisible = isLVX;
			this.SimplifiedLVSHeaderDetailsUserControl.ShipperDocAddressControl.Visible = isLVX;
		}
	}
}
