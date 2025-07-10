using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class DepartureGoodsItemWrapper : CommonGoodsItemWrapper, IDepartureGoodsItem
	{
		public DepartureGoodsItemWrapper(NctsDepartureCargoDesc line) : base(line)
		{
		}

		public ZString CountryOfDispatchExportCode => line.BY_RN_NKCountryOfDispatch;

		public ZString CountryOfDestinationCode => line.BY_RN_NKCountryOfDestination;

		public ZString TransportChargesMethodOfPayment => line.BY_TransportChargesMethodOfPayment;

		public ZString CommercialReferenceNumber => line.BY_CommercialReferenceNumber;

		public ZString UNDangerousGoodsCode => line.UNDangerousGoodsCode;

		public ZDecimal BillValue => line.BY_MonetaryValue;

		public IReadOnlyCollection<IPreviousAdministrativeReference> PreviousAdministrativeReferences => previousAdministrativeReferences ?? (previousAdministrativeReferences = line.PreviousDocuments.OfType<NctsPreviousDocument>().Select(p => new PreviousDocumentWrapper(p)).ToArray());
		IReadOnlyCollection<PreviousDocumentWrapper> previousAdministrativeReferences;

		public IReadOnlyCollection<IStatement> SpecialMentions => specialMentions ?? (specialMentions = line.SpecialMentions.ToArray());
		IReadOnlyCollection<IStatement> specialMentions;

		public ITrader Consignor => CachedValueHelper.GetValue(ref consignor, () => TraderWrapper.New(IsAddressExtended ? line.Bill?.Consignor : line.Consignor, false, IsAddressExtended));
		CachedValue<ITrader> consignor;

		public ITrader Consignee => CachedValueHelper.GetValue(ref consignee, () => TraderWrapper.New(IsAddressExtended ? line.Bill?.Consignee : line.Consignee, false, IsAddressExtended));
		CachedValue<ITrader> consignee;

		IReadOnlyCollection<ZString> ICommonGoodsItem.Containers => containers ?? (containers = line.ContainersSelected.ToArray());
		IReadOnlyCollection<ZString> containers;

		public ITrader ConsignorSecurity => CachedValueHelper.GetValue(ref consignorSecurity, () => TraderWrapper.New(line.SecurityConsignor, false, IsAddressExtended));
		CachedValue<ITrader> consignorSecurity;

		public ITrader ConsigneeSecurity => CachedValueHelper.GetValue(ref consigneeSecurity, () => TraderWrapper.New(line.SecurityConsignee, false, IsAddressExtended));
		CachedValue<ITrader> consigneeSecurity;

		protected new NctsDepartureCargoDesc line => (NctsDepartureCargoDesc)base.line;

		protected ZBool IsAddressExtended => line.Header.IsPhase5 && !line.Header.IsInPhase5TransitionPeriod;
	}
}
