using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM413AndIM415DeclarationTypeProvider : IIM413AndIM415DeclarationType
	{
		public IM413AndIM415DeclarationTypeProvider(EntryHeaderWrapper entryHeaderWrapper, bool generateNewLRN)
		{
			entryHeader = entryHeaderWrapper.EntryHeader;
			declaration = entryHeaderWrapper.Declaration;
			instruction = entryHeaderWrapper.Instruction;
			this.generateNewLRN = generateNewLRN;
		}
		protected readonly CusEntryHeader entryHeader;
		protected readonly JobDeclaration declaration;
		protected readonly CusEntryInstruction instruction;
		readonly bool generateNewLRN;

		public string MessageType => instruction.CEI_Style;

		public string DeclarationType => declaration.JE_EntryStyle;

		public string AdditionalDeclarationType => instruction.CEI_SubStyle;

		public string LRN => generateNewLRN ? AISOutboundEDIMessage.LRNPlaceHolder : entryHeader.CH_BGMReference;

		public IIM413AndIM415DeclarationTypeValuationInformation ValuationInformation => CachedValueHelper.GetValue(ref valuationInformationCached, () => new IM413AndIM415DeclarationTypeValuationInformationProvider(entryHeader));
		CachedValue<IIM413AndIM415DeclarationTypeValuationInformation> valuationInformationCached;

		public IIM413AndIM415DeclarationTypeGoodsInformation GoodsInformation => CachedValueHelper.GetValue(ref goodsInformationCached, () => new IM413AndIM415DeclarationTypeGoodsInformationProvider(declaration));
		CachedValue<IIM413AndIM415DeclarationTypeGoodsInformation> goodsInformationCached;

		public IIM413AndIM415DeclarationTypeTransportInformation TransportInformation => CachedValueHelper.GetValue(ref transportInformationCached, () => new IM413AndIM415DeclarationTypeTransportInformationProvider(declaration));
		CachedValue<IIM413AndIM415DeclarationTypeTransportInformation> transportInformationCached;

		public IDeclarationTypeCustomsOffices CustomsOffices => CachedValueHelper.GetValue(ref customsOfficesCached, () => new DeclarationTypeCustomsOfficesProvider(declaration));
		CachedValue<IDeclarationTypeCustomsOffices> customsOfficesCached;

		public IIM413AndIM415DeclarationTypeParties Parties => CachedValueHelper.GetValue(ref partiesCached, () => new IM413AndIM415DeclarationTypePartiesProvider(entryHeader));

		CachedValue<IIM413AndIM415DeclarationTypeParties> partiesCached;

		public string PaymentMethod => declaration.JE_PaymentMethod;

		public IAuth8F Auth8F => CachedValueHelper.GetValue(ref auth8FCached, () => new Authorisation8FProvider(entryHeader));
		CachedValue<IAuth8F> auth8FCached;
	}
}
