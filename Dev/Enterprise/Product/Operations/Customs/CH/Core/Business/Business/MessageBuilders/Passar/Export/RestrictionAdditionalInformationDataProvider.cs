using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class RestrictionAdditionalInformationDataProvider : IAdditionalInformation
{
	public static IEnumerable<RestrictionAdditionalInformationDataProvider> NewCollection(CusCodeDataCollection<RestrictionAdditionalInformation> restrictionAdditionalInfos)
=> restrictionAdditionalInfos?.Select(addInfo => new RestrictionAdditionalInformationDataProvider(addInfo)) ?? Enumerable.Empty<RestrictionAdditionalInformationDataProvider>();

	public RestrictionAdditionalInformationDataProvider(CusCodeData restrictionAdditionalInfo)
	{
		this.restrictionAdditionalInformation = restrictionAdditionalInfo;
	}

	readonly CusCodeData restrictionAdditionalInformation;

	public int SequenceNumber => restrictionAdditionalInformation.CY_Order;

	public string Code => restrictionAdditionalInformation.CY_Code;

	public string Text => restrictionAdditionalInformation.CY_Data.ReturnNullIfEmpty();
}
