using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SeaUnderbondSelectorLine : UnderbondSelectorLine
	{
		public SeaUnderbondSelectorLine(CusUnderbond underbond)
			: base(underbond)
		{ }

		protected internal override void UpdateStatuses()
		{
			var isWaitingResponse = false;
			var outturnHeader = Underbond.OutturnHeader;
			if (outturnHeader != null)
			{
				switch (outturnHeader.C6_MessageStatus)
				{
					case CMRBaseStatuses.Codes.AwaitingResponseToAmendment:
					case CMRBaseStatuses.Codes.AwaitingResponseToOriginal:
					case CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal:
						isWaitingResponse = true;
						break;
				}
			}
			OutturnStatus = isWaitingResponse ? OutturnStatus.AwaitingResponseFromCustoms : OutturnStatus.ReadyForScanning;
		}

		public OutturnStatus OutturnStatus { get; private set; }

		public override ZString OutturnStatusText
		{
			get { return OutturnStatus.ToString(); }
		}

		public override ZString UnderbondStatusText
		{
			get { return ZString.Empty; }
		}
	}
}
