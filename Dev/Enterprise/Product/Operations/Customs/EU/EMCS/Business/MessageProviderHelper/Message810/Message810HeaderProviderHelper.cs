using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public sealed class Message810HeaderProviderHelper : HeaderProviderHelper
	{
		public Message810HeaderProviderHelper(EMCSJobDeclaration emcsJobDeclaration, IEMCSCancellation cancellationOfEad) : base(emcsJobDeclaration)
		{
			this.cancellationOfEad = cancellationOfEad;
		}
		readonly IEMCSCancellation cancellationOfEad;

		public int CancellationReasonCode => ZInt.ParseSafe(cancellationOfEad.Reason, ZInt.Zero);
	}
}
