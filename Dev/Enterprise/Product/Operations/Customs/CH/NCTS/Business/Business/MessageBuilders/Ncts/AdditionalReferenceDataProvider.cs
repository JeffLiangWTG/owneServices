using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.CH.NCTS.Business;

public class AdditionalReferenceDataProvider : IAdditionalReference
{
	public static IEnumerable<IAdditionalReference> NewCollection(ICusSupportingInfoCollection<AdditionalInfo> additionalInfos)
		=> additionalInfos?.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference).Select((additionalReference, index) => new AdditionalReferenceDataProvider(additionalReference, index + 1));

	AdditionalReferenceDataProvider(AdditionalInfo additionalInfo, int sequenceNumber)
	{
		this.additionalInfo = additionalInfo;
		SequenceNumber = sequenceNumber;
	}

	readonly AdditionalInfo additionalInfo;

	public int SequenceNumber { get; }

	public string ReferenceNumber => additionalInfo.CSI_ReferenceNumber;

	public string Type => additionalInfo.CSI_Code;
}
