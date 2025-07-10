using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.GUI
{
	public partial class PriceItemBulkCopyForm : ZChildForm
	{
		public PriceItemBulkCopyForm(IEnumerable<ZGuid> priceItemPks)
			: base()
		{
			PriceCodeTextBox.MaxLength = ClientLicencePriceItemSchema.L7_Code.MaxLength;
			PriceDescTextBox.MaxLength = ClientLicencePriceItemSchema.L7_Description.MaxLength;
			PriceItemPks = priceItemPks;
		}

		readonly IEnumerable<ZGuid> PriceItemPks;
		public override string FormVerb => string.Empty;

		void OKButton_Click(object sender, EventArgs e)
		{
			var priceCode = PriceCodeTextBox.Text.ToUpperInvariant().Trim();
			var priceDesc = PriceDescTextBox.Text;

			if (priceCode.Length != ClientLicencePriceItemSchema.L7_Code.MaxLength)
			{
				Globals.Message.ShowError("Please enter a valid price code.");
			}
			else if (string.IsNullOrWhiteSpace(priceDesc))
			{
				Globals.Message.ShowError("Please enter a valid description.");
			}
			else
			{
				var factory = new BusinessObjectFactory();
				var priceItems = factory.Load<ClientLicencePriceItem>(new ZQuery(ClientLicencePriceItemSchema.PK, PriceItemPks));
				if (priceItems.Length == 0 || priceItems.Skip(1).Any(x => x.L7_Category != priceItems[0].L7_Category))
				{
					Globals.Message.ShowError("Selected prices must all have the same category.");
					return;
				}

				var category = priceItems[0].L7_Category;
				var query = new ZQuery(ClientLicencePriceItemSchema.L7_L6, priceItems.Select(x => x.L7_L6));
				query.AddToFilter(ClientLicencePriceItemSchema.L7_Code, priceCode);
				query.AddToFilter(ClientLicencePriceItemSchema.L7_Category, category);
				var duplicatePrices = factory.Load<ClientLicencePriceItem>(query);

				if (duplicatePrices.Any())
				{
					var priceHeaders = duplicatePrices.Select(x => x.Parent).Distinct().Select(x => ZString.Format("{0} - [{1}] - {2}", x.L6_SystemCode, x.L6_PricelistVersion, x.L6_RX_NKCurrency));
					var errorMessage = ZString.Format("The following price lists contain the price category and code already.\r\n\r\n{0}",
						string.Join(System.Environment.NewLine, priceHeaders));
					Globals.Message.ShowError(errorMessage);
				}
				else if (Globals.Message.ShowConfirmation(
					"New prices will be created directly.\r\n\r\nThis action cannot be reversed.",
					"Bulk Copy",
					"Please type the following to continue:",
					"Yes",
					MessageBoxIcon.Warning,
					ConfirmationMessageLayout.AllInOneLine
				) == DialogResult.OK)
				{
					var cursor = Cursor;

					try
					{
						Cursor = Cursors.WaitCursor;
						OKButton.Enabled = false;
						CancelFormButton.Enabled = false;

						foreach (var item in priceItems)
						{
							var newCopy = item.CreateNewCopy(priceCode, priceDesc);
							newCopy.RunPreSaveValidation();

							if (newCopy.HasErrors)
							{
								var errorMessages = new ZStringBuilder();
								newCopy.Notifications.ToList().ForEach(x => errorMessages.AppendLine(x.Message));
								Globals.Message.ShowError(errorMessages.ToString());
								return;
							}
						}

						factory.Save();
						Globals.Message.ShowInformation("Prices have been created.");
						Close();
					}
					finally
					{
						Cursor = cursor;
						OKButton.Enabled = true;
						CancelFormButton.Enabled = true;
					}
				}
			}
		}

		void CancelFormButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
