using System.Collections.Immutable;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class ArrivalCusTransportMeansLookups : EU.NCTS.Business.ArrivalCusTransportMeansLookups
	{
		public ArrivalCusTransportMeansLookups(ArrivalCusTransportMeans parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList TransportStateList => Factory.GetCachedValue($"DE.NCTS.ArrivalCusTransportMeansLookups.TransportStateList_TypeOfIdentification={Parent.TPM_TypeOfIdentification}_TransportState={Parent.TPM_TransportState}", () =>
		{
			var list = new NctsUnloadedStateList();
			list.RemoveCode(NctsUnloadedStateList.Codes.DAM);

			if (Parent.TPM_TransportState == NctsUnloadedStateList.Codes.DEC && typesOfIdentificationWhichMustNotBeReportedAsMissing.Contains(Parent.TPM_TypeOfIdentification))
			{
				list.RemoveCode(NctsUnloadedStateList.Codes.MIS);
				list.RemoveCode(NctsUnloadedStateList.Codes.NEW);
			}

			return list;
		});

		readonly static ImmutableHashSet<string> typesOfIdentificationWhichMustNotBeReportedAsMissing = ImmutableHashSet.Create(
			NctsTransportTypeOfIdList.Codes._10,
			NctsTransportTypeOfIdList.Codes._11,
			NctsTransportTypeOfIdList.Codes._30,
			NctsTransportTypeOfIdList.Codes._40,
			NctsTransportTypeOfIdList.Codes._41,
			NctsTransportTypeOfIdList.Codes._80,
			NctsTransportTypeOfIdList.Codes._81);

		protected override ZBool ShouldIncludeDIFInUnloadedStatesList => true;
	}
}
