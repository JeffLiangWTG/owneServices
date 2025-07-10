using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.Invoices
{
	public class ImportSingleInvoiceXmlDataTransferDirector : XmlDataTransferDirector
	{
		public ImportSingleInvoiceXmlDataTransferDirector(bool checkLicence, ZString ledger)
			: base(new FinancialInvoiceDataAdapter(false), checkLicence)
		{
			((FinancialInvoiceDataAdapter)Adapter).RunExtraValidation = false;
			this.Ledger = ledger;
		}

		protected override void PromptUserAndImportCore(BillingInterfaceName interfaceName)
		{
			using (var dialog = new ZOpenFileDialog())
			{
				dialog.Title = (NoResString)"Import Invoice Xml"; // May be used as constant

				if (ZFormModaliser.ShowCommonDialogWithoutDispose(dialog) == DialogResult.OK)
				{
					try
					{
						using (Stream xmlFileStream = dialog.OpenFile())
						{
							var notifications = new NotificationBufferWithUserDialogs();
							var factory = new BusinessObjectFactory();
							factory.SetContext(BusinessContext.AllowReopenJobWhenImporting);
							Import(xmlFileStream, factory, notifications);

							if (notifications.HasErrors)
							{
								var messageToDisplay = Res.GetString("821315d6-561e-4d45-af98-781c4a2072b9", "The following errors occurred while trying to import this transaction:");
								messageToDisplay += System.Environment.NewLine + notifications.AsString;
								Globals.Message.ShowError(messageToDisplay, Res.GetString("0881ac3a-34bc-4a43-ac7a-775ab978a347", "Transaction Import Error"));
							}
						}
					}
					catch (UnauthorizedAccessException ex)
					{
						Globals.Message.ShowError(ex.Message);
					}
					catch (IOException ex)
					{
						Globals.Message.ShowError(ex.Message);
					}
				}
			}
		}

		protected void Import(Stream xmlFileStream, BusinessObjectFactory factory, INotifications notifications)
		{
			var collection = new InvoicingBaseCollection(factory);
			factory.SetContext(BusinessContext.LegacyXMLImport);

			try
			{
				Serializer.ImportXmlData(xmlFileStream, Adapter, collection, null, notifications);
				if (collection.Count > 1)
				{
					notifications.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("3ac9158d-3db1-436a-8008-abddcbfee91f", "There is more than one Invoice transaction in the XML file.")));
				}
				else
				{
					fImportedInvoice = (collection.Count == 0) ? null : collection[0];
				}
			}
			catch (ArgumentNullException e)
			{
				if (e.ParamName == "BizObj")
				{
					notifications.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("5c246859-567d-455b-9cf3-777aba818833", "This transaction type cannot be imported")));
				}
			}
			catch (InvalidOperationException e)
			{
				notifications.Notify(new ErrorNotification(ErrorType.Error, e.Message));
			}
			finally
			{
				collection.RemoveAll();
				factory.RemoveContext(BusinessContext.LegacyXMLImport);
			}
		}

		public override XmlValueObjectSerializer Serializer
		{
			get
			{
				return new InvoiceXmlValueObjectSerializer(Ledger, importInSingleFactory: base.OnlySaveDataWhenNoRecordsHaveErrors);
			}
		}

		public InvoicingBase ImportedInvoice
		{
			get { return fImportedInvoice; }
		}

		InvoicingBase fImportedInvoice;

		#region Helper Classes

		class NotificationBufferWithUserDialogs : NotificationBuffer
		{
			protected override void QueryUser(IQueryUserEventArgs e)
			{
				GuiHelper.QueryUser(e);
			}

			readonly NotificationSubscriberGuiHelper GuiHelper = new NotificationSubscriberGuiHelper();
		}

		#endregion

		readonly ZString Ledger;
	}
}
