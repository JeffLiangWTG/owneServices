using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class MConsignment04Provider : IMConsignment04
	{
		public MConsignment04Provider(EntryHeaderWrapper entryHeaderWrapper)
		{
			this.entryHeaderWrapper = entryHeaderWrapper;
			entryHeader = entryHeaderWrapper.EntryHeader;
			declaration = entryHeaderWrapper.Declaration;
			instruction = entryHeaderWrapper.Instruction;
		}
		readonly EntryHeaderWrapper entryHeaderWrapper;
		readonly CusEntryHeader entryHeader;
		readonly JobDeclaration declaration;
		readonly CusEntryInstruction instruction;

		public string ContainerIndicator => declaration.JE_ContainerMode;

		public string InlandModeOfTransport => declaration.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportModeInland);

		public string ModeOfTransportAtTheBorder => declaration.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportMode);

		public decimal GrossMass => entryHeader.TotalGrossWeightInKG;

		public int TotalPackageNumber => entryHeader.PackagesCount;

		public string ReferenceNumberUCR => declaration.JE_UCR;

		public IReadOnlyCollection<IMTransportEquipment> TransportEquipments => transportEquipments ?? (transportEquipments = AISMessageProviderHelper.GetIM415TransportEquipments(entryHeader));
		public IReadOnlyCollection<IMTransportEquipment> transportEquipments;

		public IGoodsLocation LocationOfGoods => CachedValueHelper.GetValue(ref locationOfGoodsCached, () => new GoodsLocationProvider(instruction.GoodsLocation));
		CachedValue<IGoodsLocation> locationOfGoodsCached;

		public IIdType ArrivalTransportMeans => CachedValueHelper.GetValue(ref arrivalTransportMeansCached, () =>
		{
			var transportMeans = declaration.JE_TransportMeans;
			var transportMode = declaration.JE_TransportMode;
			if (string.IsNullOrEmpty(transportMeans) && string.IsNullOrEmpty(transportMode))
			{
				return null;
			}
			return IdTypeProvider.New(transportMeans, transportMode);
		});
		CachedValue<IIdType> arrivalTransportMeansCached;

		public string ActiveBorderTransportMeansNationality => declaration.JE_RN_NKTransportNationalityInland;

		public IReadOnlyCollection<IDocument> TransportDocuments => transportDocumentsCached ?? (transportDocumentsCached = entryHeaderWrapper.EntryHeader.GetTransportDocuments<DocumentProvider>());
		IReadOnlyCollection<IDocument> transportDocumentsCached;

		public ITransportCosts TransportAndInsuranceCostsToTheDestination => null;
	}
}
