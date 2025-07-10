using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using ContactNotifyModes = Enterprise.Core.Constants.ContactNotifyModes;

namespace Enterprise.Services.OperationalActions.Business
{
	sealed class ForcePrinterBulkDeliveryMethod : BulkDeliveryMethod
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is used as a code.")]
		public const string CodeText = "Force Printer";

		public ForcePrinterBulkDeliveryMethod()
			: base(CodeText, Res.GetString("cc475184-9bb2-4860-9fff-c1241f7ffd8a", "Send all documents to the printer regardless of the contacts auto-delivery settings")) { }

		public override bool UsesPrinter => true;

		public override bool AllowOverridePrintDetails => true;

		protected override void SetRecipientsCore(DeliveryInstructions instructions, DocDeliveryContactCollection contacts)
		{
			foreach (DocDeliveryContact contact in contacts)
			{
				contact.DeliveryMethod = ContactNotifyModes.Print;
				instructions.Recipients.Add(contact);
			}
		}
	}
}
