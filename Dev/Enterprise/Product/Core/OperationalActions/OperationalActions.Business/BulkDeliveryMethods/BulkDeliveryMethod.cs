using System;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.Business
{
	[System.Diagnostics.DebuggerDisplay("Code = {Code}")]
	public abstract class BulkDeliveryMethod : CodeDescriptionPair
	{
		internal BulkDeliveryMethod(string code, string description)
			: base(code, description) { }

		public void SetRecipients(DeliveryInstructions instructions, DocDeliveryContactCollection contacts)
		{
			if (UsesPrinter)
			{
				if (!instructions.PrinterDelivery.PrintQueuePK.IsValid)
				{
					throw new InvalidOperationException("No printer set");
				}
			}

			SetRecipientsCore(instructions, contacts);
		}

		public virtual bool UsesPrinter => false;

		public virtual bool AllowCoverNote => false;

		public virtual bool AllowOverridePrintDetails => false;

		public virtual bool AllowDeliverDocumentsInOneEmail => false;

		protected abstract void SetRecipientsCore(DeliveryInstructions instructions, DocDeliveryContactCollection contacts);
	}
}
