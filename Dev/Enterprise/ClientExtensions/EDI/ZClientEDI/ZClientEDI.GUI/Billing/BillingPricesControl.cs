using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Cache;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.GenericCharge;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.Client.EDI.Popups;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.GUI
{
	public partial class BillingPricesControl : ZUserControl
	{
		public BillingPricesControl()
		{
			InitializeComponent();
			AddContextMenu();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			SplitterState.Persist(SplitContainer);
			SplitterState.Persist(itemSplitContainer);

			SplitterState.Persist(stlDiscountsSplitContainer);
			SplitterState.Persist(headerDiscountSplitContainer);
		}

		#region Context Menu

		MenuItem[] PriceListMenuItems
		{
			get
			{
				if (priceListMenuItems == null)
				{
					MenuItem cloneStdPriceListMenuItem = new ZMenuItem("Clone Standard Price List");
					cloneStdPriceListMenuItem.Click += delegate
					{
						if (PriceHeadersGrid.ListManager != null)
						{
							Cursor.Current = Cursors.WaitCursor;
							ClientLicencePriceHeader targetPriceHeader = LicCompany.CopyPriceList(PriceHeadersGrid.ListManager.GetCurrent() as ClientLicencePriceHeader);
							Cursor.Current = Cursors.Default;
							if (targetPriceHeader == null)
							{
								Globals.Message.ShowError("Invalid Price List");
							}
						}
					};

					var translateMenuItem = new ZMenuItem("Translate Text Elements");
					translateMenuItem.Click += delegate
					{
						var priceHeader = PriceHeadersGrid.ListManager?.GetCurrent() as ClientLicencePriceHeader;

						if (priceHeader != null && priceHeader.Items.Any())
						{
							var infos = new List<ZPropertyInfo>();
							infos.AddRange(priceHeader.Items.Select(x => x.L7_DescriptionInfo));
							infos.AddRange(priceHeader.Items.Select(x => x.L7_ChargeBasisInfo));

							var chargeCodes = priceHeader.Items.SelectMany(x => new[] { x.L7_ChargeCode, x.L7_DepositChargeCode, x.L7_DiscountChargeCode })
							.Where(x => !x.IsEmpty).Distinct().ToArray();

							var genericCharges = priceHeader.Factory.Load<GenericCharge>(new ZQuery(ViewGenericChargeSchema.VC_Code, chargeCodes));
							infos.AddRange(genericCharges.Select(x => x.VC_DescriptionInfo));

							var source = new MultipleDataCaptionSource(infos.ToArray(), priceHeader.L6_PricelistVersion);

							try
							{
								Cursor.Current = Cursors.WaitCursor;
								ObjectFactory.Get<ICustomizableDataTranslationEditor>().EditTranslations(
									new CustomizableDataResourceStrings(source),
									null, DataSource);
							}
							finally
							{
								Cursor.Current = Cursors.Default;
							}
						}
					};

					var exportDisbursementPricelistMenuItem = new ZMenuItem("Export Disbursement Pricelist");
					exportDisbursementPricelistMenuItem.Click += ExportDisbursementPricelist;

					priceListMenuItems =
					[
						cloneStdPriceListMenuItem,
						translateMenuItem,
						exportDisbursementPricelistMenuItem
					];
				}
				return priceListMenuItems;
			}
		}
		MenuItem[] priceListMenuItems;

		void AddContextMenu()
		{
			PriceHeadersGrid.ContextMenu.Popup += delegate (object sender, EventArgs e)
			{ SetupActionsMenuItems(); };

			PriceItemsGrid.ContextMenu.MenuItems.Add(new ZMenuItem("Selected Row #s", new MenuItem[]
			{
				new ZMenuItem("+1", (s, args) => IncrementRowNumbers(1)),
				new ZMenuItem("+2", (s, args) => IncrementRowNumbers(2)),
				new ZMenuItem("+3", (s, args) => IncrementRowNumbers(3)),
				new ZMenuItem("+4", (s, args) => IncrementRowNumbers(4)),
				new ZMenuItem("+5", (s, args) => IncrementRowNumbers(5)),
				new ZMenuItem("+6", (s, args) => IncrementRowNumbers(6)),
				new ZMenuItem("+7", (s, args) => IncrementRowNumbers(7)),
				new ZMenuItem("+8", (s, args) => IncrementRowNumbers(8)),
				new ZMenuItem("+9", (s, args) => IncrementRowNumbers(9)),
				new ZMenuItem("+10", (s, args) => IncrementRowNumbers(10)),
				new ZMenuItem("+20", (s, args) => IncrementRowNumbers(20)),
				new ZMenuItem("+30", (s, args) => IncrementRowNumbers(30)),
				new ZMenuItem("+40", (s, args) => IncrementRowNumbers(40)),
				new ZMenuItem("+50", (s, args) => IncrementRowNumbers(50)),
				new ZMenuItem("+100", (s, args) => IncrementRowNumbers(100)),
				new ZMenuItem("x 10", (s, args) => ScaleRowNumbers(10))
			}));

			PriceItemsGrid.ContextMenu.MenuItems.Add(new ZMenuItem("Download and convert Price Item", DownloadAndConvertPriceItem));
		}

		void IncrementRowNumbers(short offset)
		{
			foreach (ClientLicencePriceItem item in PriceItemsGrid.SelectedElements)
			{
				if (item.ReadOnly)
				{
					break;
				}
				item.L7_Order += offset;
			}
		}

		void ScaleRowNumbers(short factor)
		{
			foreach (ClientLicencePriceItem item in PriceItemsGrid.SelectedElements)
			{
				if (item.ReadOnly)
				{
					break;
				}
				item.L7_Order *= factor;
			}
		}

		void SetupActionsMenuItems()
		{
			PriceHeadersGrid.ContextMenu.MenuItems.AddRange(PriceListMenuItems);
			return;
		}

		void DownloadAndConvertPriceItem(object sender, EventArgs e)
		{
			if (PriceItemsGrid.SelectedRowCount == 0)
			{
				Globals.Message.ShowError(Res.GetString("667868d7-297f-4a2a-ac7d-f3bfeaf4697c", "Please select at least one row in the Price Items Grid."));
				return;
			}

			var currentItems = PriceItemsGrid.SelectedElements.Cast<ClientLicencePriceItem>();
			var priceHeader = currentItems.First().Parent;

			var validationErrorMessage = ExchangeRateSerializerHelper.RunValidation(currentItems);

			if (!string.IsNullOrEmpty(validationErrorMessage))
			{
				Globals.Message.ShowError(validationErrorMessage);
				return;
			}

			var priceItemsDisplayList = string.Join(", ", currentItems.Select(x => FormattableString.Invariant($"{x.L7_Category} - {x.L7_Code}")));

			if (Globals.Message.Show(
				Res.GetString("f3baa89a-1133-48c9-8987-7d6fd122e080", "You are going to convert Price Item(s) [{0}] from Pricelist [{1}]. This pricelist will be valid from [{2}]. Do you wish to proceed?", priceItemsDisplayList, priceHeader.L6_PricelistVersion, priceHeader.L6_ValidFrom.ToShortDateString()),
				"",
				MessageBoxButtons.YesNo,
				DialogResult.Yes) != DialogResult.Yes)
			{
				return;
			}

			using (var dialog = new ZSaveFileDialog())
			{
				dialog.DefaultExt = "xml"; // File extension
				dialog.AddExtension = true;
				dialog.Filter = "XML Files | *.xml"; // File extension filter
				dialog.InitialDirectory = "\\"; // File extension

				if (ZFormModaliser.ShowCommonDialogWithoutDispose(dialog) == DialogResult.OK)
				{
					var errorMessage = string.Empty;
					using (var stream = dialog.OpenFile())
					{
						var helper = new ExchangeRateSerializerHelper();
						errorMessage = helper.AddRates(currentItems);

						if (string.IsNullOrEmpty(errorMessage))
						{
							var result = helper.SerializeRates();
							if (!result.Success)
							{
								errorMessage = result.ErrorMessage;
							}
							else
							{
								result.SeralizedValue.Save(stream);
							}
						}
					}

					if (!string.IsNullOrEmpty(errorMessage))
					{
						if (File.Exists(dialog.UnmappedFileName))
						{
							File.Delete(dialog.UnmappedFileName);
						}

						Globals.Message.ShowError(errorMessage);
					}
					else
					{
						Globals.Message.Show(GetDocumentSavedMessage(dialog.UnmappedFileName));
					}
				}
			}
		}

		void ExportDisbursementPricelist(object sender, EventArgs e)
		{
			if (PriceHeadersGrid.SelectedRowCount != 1)
			{
				Globals.Message.ShowError(Res.GetString("6d062a96-921c-4a2f-ada8-339db669e653", "Please select a single row in the Price Headers Grid."));
				return;
			}

			var currentHeader = PriceHeadersGrid.ListManager.GetCurrent() as ClientLicencePriceHeader;

			var validationErrorMessage = DisbursementPricelistExportHelper.RunValidation(currentHeader);

			if (!string.IsNullOrEmpty(validationErrorMessage))
			{
				Globals.Message.ShowError(validationErrorMessage);
				return;
			}

			using (var dialog = new ZSaveFileDialog())
			{
				dialog.DefaultExt = "xml"; // File extension
				dialog.AddExtension = true;
				dialog.Filter = "XML Files | *.xml"; // File extension filter
				dialog.InitialDirectory = "\\"; // File extension

				if (ZFormModaliser.ShowCommonDialogWithoutDispose(dialog) == DialogResult.OK)
				{
					var errorMessage = string.Empty;
					using (var stream = dialog.OpenFile())
					{
						var helper = new DisbursementPricelistExportHelper();
						errorMessage = helper.AddPriceItems(currentHeader);

						if (string.IsNullOrEmpty(errorMessage))
						{
							var result = helper.SerializePricelist();
							if (!result.Success)
							{
								errorMessage = result.ErrorMessage;
							}
							else
							{
								result.SeralizedValue.Save(stream);
							}
						}
					}

					if (!string.IsNullOrEmpty(errorMessage))
					{
						if (File.Exists(dialog.UnmappedFileName))
						{
							File.Delete(dialog.UnmappedFileName);
						}

						Globals.Message.ShowError(errorMessage);
					}
					else
					{
						Globals.Message.Show(GetDocumentSavedMessage(dialog.UnmappedFileName));
					}
				}
			}
		}

		string GetDocumentSavedMessage(string fileName)
		{
			return Res.GetString("b780c4b6-7d26-40d2-8c20-84d329b0ff48", "The document was saved as {0}", fileName);
		}

		#endregion

		#region Bound Licence Company

		LicenceCompany LicCompany
		{
			get
			{
				if (licCompany == null)
				{
					licCompany = ((EDIOrgHeader)this.BindingSource.DataSource).LicCompany;
				}

				return licCompany;
			}
		}
		LicenceCompany licCompany;

		#endregion

		#region Pricelist Button Handlers

		void NewPriceListButton_Click(object sender, EventArgs e)
		{
			if (IsSecurityCheckpointAllowed())
			{
				var strip = new ContextMenuStrip();
				strip.Items.Add("Standard Pricelist", null, AddStandardPriceList);
				strip.Items.Add("Quick Transactional Pricelist", null, AddTransactionalPriceList);
				strip.Items.Add("Miscellaneous Pricelist", null, AddOtherPriceList);
				strip.Items.Add("eHub Pricelist", null, AddEHubPriceList);
				var button = sender as Control;
				strip.Show(button, 0, button.Height);
			}
		}

		void AddStandardPriceList(object sender, EventArgs e)
		{
			var newList = LicCompany.CreatePriceList(true);
			PriceHeadersGrid.SelectSingleElementByPK(newList.PK);
		}

		void AddTransactionalPriceList(object sender, EventArgs e)
		{
			LicCompany.QuickAddTransactionalPriceList();
		}

		void AddOtherPriceList(object sender, EventArgs e)
		{
			LicCompany.QuickAddOtherPriceList();
		}

		void AddEHubPriceList(object sender, EventArgs e)
		{
			LicCompany.QuickAddEHubPriceList();
		}

		bool IsSecurityCheckpointAllowed()
		{
			bool result = EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed;
			if (!result)
			{
				EDISecurityCheckpoints.OrgLicenceBilling.ShowError();
			}

			return result;
		}

		#endregion

		#region EDIOrganisationForm

		public void SelectPriceHeadersGrid(ClientLicencePriceItem priceItem)
		{
			var headerGrid = PriceHeadersGrid;
			var priceItemGrid = PriceItemsGrid;
			headerGrid.SelectSingleElementByPK(priceItem.L7_L6);
			priceItemGrid.SelectSingleElementByPK(priceItem.PK);
		}

		#endregion

		void addCurrencyButton_Click(object sender, EventArgs e)
		{
			var items = PriceItemsGrid.SelectedElements.Cast<ClientLicencePriceItem>().ToArray();
			if (items.Length > 0)
			{
				if (items.First().Parent.L6_HasExchangeRates)
				{
					Globals.Message.Show("Price Item Rate can only be created if Price Header does not have Exchange Rates");
					return;
				}

				var msg = string.Format(CultureInfo.InvariantCulture, "Updates {0} selected items(s)?\r\n\r\nRows with no price will be skipped.", items.Length);
				if (Globals.Message.Show(msg, addCurrencyButton.Text, MessageBoxButtons.OKCancel, DialogResult.OK) == DialogResult.OK)
				{
					var updater = new PriceCurrencyUpdater(new BusinessObjectFactory() { RefreshEnabled = false });
					using (var form = new AddPriceCurrencyForm(updater))
					{
						if (ZFormModaliser.ShowDialogWithoutDispose(form, null) == DialogResult.OK)
						{
							updater.ApplyTo(items);
						}
					}
				}
			}
			else
			{
				Globals.Message.Show("Please select items in the pricelist to update.");
			}
		}

		void FilterPriceListButton_Click(object sender, EventArgs e)
		{
			PriceHeaderFilterPopupForm ??= new FilterPopupForm(LicCompany.PriceHeaders.AsFilterable);

			ZFormModaliser.ShowDialogWithoutDispose(PriceHeaderFilterPopupForm);
		}

		FilterPopupForm PriceHeaderFilterPopupForm;

		void ClearPriceListFiltersButton_Click(object sender, EventArgs e)
		{
			PriceHeaderFilterPopupForm?.Dispose();
			PriceHeaderFilterPopupForm = null;
			LicCompany.PriceHeaders.AsFilterable.ClearFilter();
		}

		ClientLicencePriceHeader PreviousPriceListHeader;
		FilterPopupForm PriceItemFilterPopupForm;

		void FilterPriceItemButton_Click(object sender, EventArgs e)
		{
			var header = PriceHeadersGrid.SelectedElements
				.Cast<ClientLicencePriceHeader>().ToArray().FirstOrDefault();
			if (header != null)
			{
				if (PreviousPriceListHeader != header)
				{
					PreviousPriceListHeader = header;
					PriceItemFilterPopupForm = new FilterPopupForm(header?.Items.AsFilterable);
				}
				PriceItemFilterPopupForm ??= new FilterPopupForm(header?.Items.AsFilterable);

				ZFormModaliser.ShowDialogWithoutDispose(PriceItemFilterPopupForm);
			}
		}

		void ClearPriceItemFiltersButton_Click(object sender, EventArgs e)
		{
			PriceItemFilterPopupForm?.Dispose();
			PriceItemFilterPopupForm = null;
			PriceHeadersGrid.SelectedElements
				.Cast<ClientLicencePriceHeader>().ToArray().FirstOrDefault()?.Items.AsFilterable.ClearFilter();
		}

		public ClientLicencePriceHeader GetCurrentPriceHeader()
		{
			return PriceHeadersGrid.ListManager?.GetCurrent() as ClientLicencePriceHeader;
		}
	}
}

