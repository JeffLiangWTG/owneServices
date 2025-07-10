using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Services.OperationalActions.Business
{
	sealed class AutoBulkDeliveryMethod : BulkDeliveryMethod
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is used as a code.")]
		public const string CodeText = "Auto";

		public AutoBulkDeliveryMethod()
			: base(CodeText, Res.GetString("4acf1e89-7ac2-43d6-85e6-d6d871841bec", "Use the normal auto-delivery settings for each contact")) { }

		public override bool UsesPrinter
		{
			get { return true; }
		}

		public override bool AllowCoverNote
		{
			get { return true; }
		}

		protected override void SetRecipientsCore(DeliveryInstructions instructions, DocDeliveryContactCollection contacts)
		{
			foreach (DocDeliveryContact contact in contacts)
			{
				instructions.Recipients.Add(contact);
			}
		}
	}
}
