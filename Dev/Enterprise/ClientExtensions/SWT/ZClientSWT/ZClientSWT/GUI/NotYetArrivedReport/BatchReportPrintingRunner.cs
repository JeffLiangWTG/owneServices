using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.SWT.GUI
{
	class BatchReportPrintingRunner : BatchReportRunner
	{
		protected override void RunReportWithDataCore(CancellationToken token)
		{
			DialogResult userChoice = ShowPrinterSelectionForm();

			if (userChoice == DialogResult.OK)
			{
				base.RunReportWithDataCore(token);
			}
		}
		protected virtual DialogResult ShowPrinterSelectionForm()
		{
			return ZFormModaliser.ShowDialogAndDispose(new SWTPrinterSelectionForm(PrintingInstructions));
		}

		protected override void SetInstructions(DeliveryInstructions instructions, OrgHeader consignee, IList<ZString> contactTypeCodeCollection)
		{
			instructions.PrinterDelivery = PrintingInstructions.PrinterDelivery;
			DocDeliveryContact docContact = (instructions.Recipients.Count > 0) ? instructions.Recipients[0] : instructions.Recipients.AddNew();
			docContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;

			foreach (OrgContact contact in consignee.Contacts)
			{
				OrgDocument doc = GetDocContactForAllOrDocType(contact, contactTypeCodeCollection);
				if (doc != null)
				{
					docContact.AttachmentType = doc.OD_AttachmentType;
					docContact.OrgHeaderPK = consignee.PK;
					docContact.Name = contact.OC_ContactName;
					break;
				}
			}
		}

		protected DeliveryInstructions PrintingInstructions
		{
			get { return printingInstructions ?? (printingInstructions = new DeliveryInstructions()); }
		}
		DeliveryInstructions printingInstructions;
	}
}
