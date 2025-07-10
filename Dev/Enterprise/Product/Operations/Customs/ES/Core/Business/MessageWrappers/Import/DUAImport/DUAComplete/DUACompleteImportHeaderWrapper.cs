using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DUACompleteImportHeaderWrapper : DUAImportCommonHeaderWrapper, IDUACompleteImportHeader
	{
		public DUACompleteImportHeaderWrapper(CusEntryHeader cusEntryHeader) : base(cusEntryHeader)
		{
		}

		const string ProcedureY = "Y";

		protected override ZString ProcedureCore => entryInstruction.CEI_SubStyle == Declaration.EntrySubStyleList.Codes.C ? (ZString)ProcedureY : entryInstruction.CEI_SubStyle;

		public IDUACompleteImportExporterProvider Exporter => CachedValueHelper.GetValue(ref exporter, () => DUAImportExporterWrapper.New(entryHeader));
		CachedValue<IDUACompleteImportExporterProvider> exporter;

		public ZString DestinationCountry => declaration.JE_GoodsDestination;

		public ZString DestinationState => declaration.ZG_DestinationState;

		public ZString ArrivalTransportId => declaration.ZG_Box18TransportID;

		public IDUACompleteImportDeliveryConditions DeliveryConditions => CachedValueHelper.GetValue(ref deliveryConditions, () => new DUAImportDeliveryConditionsWrapper(entryHeader));
		CachedValue<IDUACompleteImportDeliveryConditions> deliveryConditions;

		public ZString FrontierTransportCountry => declaration.GetDefaultTerritory(declaration.JE_RN_NKTransportNationality);

		public ZDecimal InvoiceAmount
		{
			get
			{
				if (invoiceAmount == null)
				{
					invoiceAmount = new CachedProperty<ZDecimal>(entryHeader.Factory, () =>
					{
						var amount = ZDecimal.Zero;
						amount += entryHeader.InvoiceLines.Sum(line => line.JI_LinePrice);

						return amount;
					});
				}
				return invoiceAmount.Value;
			}
		}
		CachedProperty<ZDecimal> invoiceAmount;

		public ZString TransactionNature => entryHeader.RandomHeader.JZ_ValuationCode;

		public ZString FrontierTransportMode => declaration.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportMode, false);

		public ZString InteriorTransportMode => declaration.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportModeInland, false);

		public ZString CustomsOfficeOfEntry => declaration.GetCustomsOfficeFromList(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent);

		public ZString DepositId => !entryInstruction.FromWarehouseCode.IsEmpty ? entryInstruction.FromWarehouseCode : entryInstruction.ToWarehouseCode;
	}
}
