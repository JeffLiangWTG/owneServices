using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class OperationalActionRunnerValidation : AutoOperationalActionRunnerValidation
	{
		public OperationalActionRunnerValidation(AutoOperationalActionRunner parent)
			: base(parent) { }

		protected override void CheckPrinter()
		{
			base.CheckPrinter();

			if (Parent.Action.DocumentPivots.Count > 0 &&
				Parent.Lookups.BulkDeliveryMethod_List.UsesPrinter(Parent.BulkDeliveryMethod))
			{
				if (!Parent.Lookups.Printer_List.Cast<CodeElement>().Any(element => (ZGuid)element.PK == Parent.Printer))
				{
					Parent.PrinterInfo.AddError(Res.GetString("8ddd36fa-e1cb-49a3-9865-b24876b6f303", "Please enter a valid printer"));
				}
			}
		}

		protected override void CheckRunOnAllMatchingRecords()
		{
			if (Parent.RunOnAllMatchingRecords && !Parent.Action.Context.Supporter.AllowRunOnAllMatchingRecordsCheckpoint.IsAllowed)
			{
				Parent.RunOnAllMatchingRecordsInfo.AddError(Res.GetString("8E7744FD-9E6F-43D6-8700-046B8E8FEEBC", "You do not have the appropriate security rights to select this option."));
			}
			base.CheckRunOnAllMatchingRecords();
		}
		protected override void CheckBulkDeliveryMethod()
		{
			base.CheckBulkDeliveryMethod();
			MandatoryValidation.CheckEntered(Parent.BulkDeliveryMethodInfo);
			ListValidation.ErrorIfInvalidCode(Parent.BulkDeliveryMethodInfo, Parent.Lookups.BulkDeliveryMethod_List);
		}

		protected override void CheckAttachmentOptions()
		{
			base.CheckAttachmentOptions();
			if (Parent.DeliverDocumentsInOneEmail)
			{
				MandatoryValidation.CheckEntered(Parent.AttachmentOptionsInfo);

				if (!Parent.AttachmentOptions.IsEmpty)
				{
					ListValidation.ErrorIfInvalidCode(Parent.AttachmentOptionsInfo, Parent.Lookups.AttachmentOptions_List);
				}
			}
		}

		protected override void CheckRecipientEmail()
		{
			base.CheckRecipientEmail();

			if (Parent.OverrideRecipientEmail)
			{
				MandatoryValidation.CheckEntered(Parent.RecipientEmailInfo);
				EmailAddressValidation.ValidateEmailAddress(Parent.RecipientEmailInfo);
			}
		}

		#region Implementation

		public new OperationalActionRunner Parent
		{
			get { return (OperationalActionRunner)base.Parent; }
		}

		#endregion
	}
}
