using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.EU.Business
{
	public abstract class MessageChangedStatusDeterminerToDictateWhetherSavingAllowed : Customs.Business.SingleMessageManager
	{
		public MessageChangedStatusDeterminerToDictateWhetherSavingAllowed(JobDeclaration dec)
		{
			this.declaration = dec;
		}

		readonly JobDeclaration declaration;

		public bool AllowSave
		{
			get
			{
				return !this.RequiresAmendmentCore();
			}
		}

		public override bool IsWaitingForResponse
		{
			get
			{
				return declaration.CustomsEntryHeaders != null && declaration.CustomsEntryHeaders.Count > 0 && declaration.CustomsEntryHeaders.AreAnyHeadersWaitingForAResponse;
			}
		}

		public abstract EDIMessage[] MakeMessagesOnThisBizoForComparison(BusinessObject bizo);

		protected override EDIMessage[] GenerateOriginalMessagesCore(BusinessObject bizo)
		{
			return MakeMessagesOnThisBizoForComparison(bizo); // <-- this done in countries
		}

		#region unused overrides
		public override string MessageFriendlyName
		{
			get { return ""; }
		}

		protected override EDIMessage[] GenerateWithdrawalMessagesCore(BusinessObject bizo)
		{
			return System.Array.Empty<EDIMessage>();
		}

		protected override EDIMessage[] GenerateAmendmentMessagesCore(BusinessObject bizo)
		{
			return System.Array.Empty<EDIMessage>();
		}

		public override bool CanSendWithdrawal
		{
			get { return false; }
		}

		public override BusinessObject BusinessObject
		{
			get { return this.declaration; }
		}

		public override bool CanSendOriginal
		{
			get { return false; }
		}
		#endregion
	}
}
