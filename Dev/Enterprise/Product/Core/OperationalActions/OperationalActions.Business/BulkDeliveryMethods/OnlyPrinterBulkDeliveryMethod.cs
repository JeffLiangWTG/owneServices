using System;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using ContactNotifyModes = Enterprise.Core.Constants.ContactNotifyModes;

namespace Enterprise.Services.OperationalActions.Business
{
	sealed class OnlyPrinterBulkDeliveryMethod : BulkDeliveryMethod
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is used as a code.")]
		public const string CodeText = "Only Printer";

		public OnlyPrinterBulkDeliveryMethod()
			: base(CodeText, Res.GetString("2591e668-1005-4cc7-a235-aeb869994451", "Only deliver documents destined for the printer")) { }

		public override bool UsesPrinter => true;

		public override bool AllowOverridePrintDetails => true;

		protected override void SetRecipientsCore(DeliveryInstructions instructions, DocDeliveryContactCollection contacts)
		{
			if (!instructions.PrinterDelivery.PrintQueuePK.IsValid)
			{
				throw new InvalidOperationException("No printer set");
			}

			instructions.Recipients.RemoveAll();

			foreach (DocDeliveryContact contact in contacts)
			{
				if (contact.DeliveryMethod == ContactNotifyModes.Print)
				{
					instructions.Recipients.Add(contact);
				}
			}
		}
	}
}
