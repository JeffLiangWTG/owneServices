using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CusUnderbondOutturnManager : CMRMessageManager
	{
		public CusUnderbondOutturnManager(CusUnderbond underbond, ZString ownerCompanyABN)
		{
			Underbond = underbond;
			OwnerCompanyABN = ownerCompanyABN;
		}
		public ZString OwnerCompanyABN { get; }

		internal override ICalculatedCusStatusCalculator[] StatusCalculators => new ICalculatedCusStatusCalculator[] { Underbond.OutturnStatusCalculator };

		internal override string GetStatus() => Underbond.OutturnStatus.Code;

		public override BusinessObject BusinessObject => Underbond;

		public override string MessageFriendlyName => "Outturn Report for: " + Underbond.C4_SendersMessageReference;

		internal override EDIMessageCollection GetMessages(BusinessObject bizo) => (bizo as CusUnderbond).Messages;

		protected bool HasSplitMessageFailedLog(BusinessObject bizo)
		{
			var result = false;
			if (Underbond != null)
			{
				result = Underbond.HasSplitMessageFailedLog;
			}
			else
			{
				var outturnHeader = bizo as CusOutturnHeader;
				if (outturnHeader != null)
				{
					result = outturnHeader.HasSplitMessageFailedLog;
				}
			}

			return result;
		}

		protected override void ResetToOriginalCore()
		{
			base.ResetToOriginalCore();
			if (Underbond != null)
			{
				Underbond.C4_Outurned = ZDate.Empty;    // Outurn has been reset to original - reset Outurned date
			}
		}

		protected readonly CusUnderbond Underbond;
	}
}
