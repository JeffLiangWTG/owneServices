using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using IAdditionalInformation = CargoWise.Customs.CH.MessageContracts.Passar.Outgoing.IAdditionalInformation;

namespace Enterprise.Customs.CH.Business;

public class AdditionalInformationDataProvider : IAdditionalInformation
{
	public static IEnumerable<AdditionalInformationDataProvider> NewCollection(ICusSupportingInfoCollection<CusSupportingInfo> additionalInfos, bool generateSequenceNumber = false)
		=> additionalInfos?.Select((additionalInformation, index) => new AdditionalInformationDataProvider(additionalInformation, generateSequenceNumber ? index + 1 : additionalInformation.CSI_LineNo))
		?? Enumerable.Empty<AdditionalInformationDataProvider>();

	public static IEnumerable<AdditionalInformationDataProvider> NewCollection(ICusSupportingInfoCollection<CusSupportingInfo> additionalInfos, string additionalInfoSubType)
		=> additionalInfos?.Where(x => x.CSI_SubType == additionalInfoSubType).Select((additionalInformation, index) => new AdditionalInformationDataProvider(additionalInformation, index + 1))
		?? Enumerable.Empty<AdditionalInformationDataProvider>();

	public static AdditionalInformationDataProvider New(CusSupportingInfo additionalInformation, int sequenceNumber) => additionalInformation == null ? null : new AdditionalInformationDataProvider(additionalInformation, sequenceNumber);

	public static AdditionalInformationDataProvider New(int sequenceNumber, string code, string text) => new AdditionalInformationDataProvider(sequenceNumber, code, text);

	AdditionalInformationDataProvider(CusSupportingInfo additionalInfo, int sequenceNumber)
		: this(sequenceNumber, additionalInfo.CSI_Code, additionalInfo.CSI_Description) { }

	AdditionalInformationDataProvider(int sequenceNumber, string code, string text)
	{
		SequenceNumber = sequenceNumber;
		Code = code;
		Text = text;
	}

	public int SequenceNumber { get; }

	public string Code { get; }

	public string Text { get; }
}
