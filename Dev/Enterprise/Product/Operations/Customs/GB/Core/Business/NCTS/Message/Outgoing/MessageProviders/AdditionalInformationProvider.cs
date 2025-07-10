using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class AdditionalInformationProvider : IAdditionalInformation
	{
		protected readonly CusSupportingInfo cusSupportingInfo;

		public AdditionalInformationProvider(CusSupportingInfo cusSupportingInfo)
		{
			this.cusSupportingInfo = Argument.NotNull(cusSupportingInfo, nameof(cusSupportingInfo));
		}

		public string Code => cusSupportingInfo.CSI_Code;

		public virtual string Text => cusSupportingInfo.CSI_ReferenceNumber;

		public int SequenceNumber => cusSupportingInfo.CSI_LineNo;
	}
}
