using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public class UpdateImportEntryStatusObjectValidation : ZValidation
	{
		public UpdateImportEntryStatusObjectValidation(UpdateImportEntryStatusObject updateImportEntryStatus) : base(updateImportEntryStatus)
		{
			parent = updateImportEntryStatus;
		}
		readonly UpdateImportEntryStatusObject parent;

		public override Type AutoValidationType => typeof(UpdateImportEntryStatusObjectValidation);

		public override void ValidateAll()
		{
			ValidateEntryStatus();
			ValidateEventDate();
			ValidateRiskChannel();
		}

		public void ValidateEntryStatus()
		{
			ValidateCalculatedProperty(parent.EntryStatusInfo);
		}

		protected void CheckEntryStatus()
		{
			ListValidation.ErrorIfInvalidCode(parent.EntryStatusInfo);
			ValidateEventDate();
		}

		public void ValidateEventDate()
		{
			ValidateCalculatedProperty(parent.EventDateInfo);
		}

		protected void CheckEventDate()
		{
			TypeValidation.CheckValidSmallDateTime(parent.EventDateInfo);

			if (parent.EntryHeader != null && parent.EntryStatus != parent.EntryHeader.CH_EntryStatus)
			{
				MandatoryValidation.CheckEntered(parent.EventDateInfo);
			}
		}

		public void ValidateRiskChannel()
		{
			ValidateCalculatedProperty(parent.RiskChannelInfo);
		}

		protected void CheckRiskChannel()
		{
			ListValidation.ErrorIfInvalidCode(parent.RiskChannelInfo);
		}
	}
}
