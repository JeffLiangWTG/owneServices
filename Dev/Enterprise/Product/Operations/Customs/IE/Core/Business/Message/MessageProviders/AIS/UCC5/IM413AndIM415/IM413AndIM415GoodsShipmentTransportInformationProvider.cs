using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Business.Declaration;
using CusEntryHeader = Enterprise.Customs.IE.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM413AndIM415GoodsShipmentTransportInformationProvider : IGoodsShipmentTypeTransportInformation
	{
		public IM413AndIM415GoodsShipmentTransportInformationProvider(EntryHeaderWrapper entryHeaderWrapper)
		{
			entryHeader = entryHeaderWrapper.EntryHeader;
			declaration = entryHeaderWrapper.Declaration;
		}
		readonly CusEntryHeader entryHeader;
		readonly JobDeclaration declaration;

		public string Container => entryHeader.MergedLines.Select(x => x.Containers).Any(c => !c.IsNullOrEmpty()) ? Yes : No;

		public string InlandBorderTransportMode => declaration.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportModeInland);

		public IIdType ArrivalTransportMeansId => CachedValueHelper.GetValue(ref arrivalTransportMeansCached, () => ArrivalTransportMeansProvider.New(declaration));
		CachedValue<IIdType> arrivalTransportMeansCached;

		public IReadOnlyCollection<string> ContainerIdentificationNumbers { get; private set; }
		internal void SetContainerIDs(string[] input) => ContainerIdentificationNumbers = input;

		const string Yes = "1";
		const string No = "0";
	}
}
