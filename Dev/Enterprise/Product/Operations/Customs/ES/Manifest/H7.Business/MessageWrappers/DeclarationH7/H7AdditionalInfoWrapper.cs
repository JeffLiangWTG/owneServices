using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Manifest.H7.Business;

public class H7AdditionalInfoWrapper(CusSupportingInfo supportingInfo) : IH7AdditionalInfo
{
	public ZString Code => supportingInfo.CSI_Code;
	public ZString Description => supportingInfo.CSI_Description;
}
