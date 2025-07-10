using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class AdditionalInformationProvider : IAdditionalInformation
	{
		protected readonly CusSupportingInfo cusSupportingInfo;

		public AdditionalInformationProvider(CusSupportingInfo cusSupportingInfo)
		{
			this.cusSupportingInfo = Argument.NotNull(cusSupportingInfo, nameof(cusSupportingInfo));
		}

		public string Code => cusSupportingInfo.CSI_Code;

		public string Text => cusSupportingInfo.CSI_Description;

		public int SequenceNumber => cusSupportingInfo.CSI_LineNo;
	}
}
