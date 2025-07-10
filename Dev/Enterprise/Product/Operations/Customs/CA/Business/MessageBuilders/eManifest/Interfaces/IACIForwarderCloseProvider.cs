namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	using System.Collections.Generic;
	using CargoWise.Types;

	public interface IACIForwarderCloseProvider : IACIForwarderMessageProvider
	{
		ZString PreviousCCN { get; }
		ZString PreviousCCNForWithdraw { get; }
		ZString CarrierCode { get; }
		ZString CarrierCodeForWithdraw { get; }
		IEnumerable<ZString> RelatedCCNs { get; }
		List<HouseCCNInstruction> AllCCNs { get; }
		ZBool ReadyToClose { get; }
		ZString AmendReasonCode { get; }
		ZDateTime ATA { get; }
	}

	public class HouseCCNInstruction
	{
		public ZString CCN;
		public ZBool IsShouldSend;
	}
}
