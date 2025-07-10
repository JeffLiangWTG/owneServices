using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Messaging;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS
{
	public abstract class NctsHeaderDocBaseWrapper : DocBaseWrapper
	{
		protected NctsHeaderDocBaseWrapper(NctsHeader nctsHeader, BusinessObjectFactory factory) : base(nctsHeader, factory)
		{
			NctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
			Argument.NotNull(factory, nameof(factory));
			MovementHeader = nctsHeader.MovementHeader;
		}

		protected NctsHeader NctsHeader { get; }

		protected NctsDepartureMovementHeader MovementHeader { get; }

		public ZBool IsPhase5 => NctsHeader.IsPhase5;

		protected ZBool IsAddressExtended => IsPhase5 && !NctsHeader.IsInPhase5TransitionPeriod;

		#region Cached Properties

		protected IGuarantee[] Guarantees => guarantees ??= (IsPhase5 ? NctsHeader.MovementHeader.Guarantees : NctsHeader.Guarantees).Select(x => new GuaranteeWrapper(x)).ToArray();
		IGuarantee[] guarantees;

		protected ITrader Consignor => CachedValueHelper.GetValue(ref consignor, () => TraderWrapper.New(NctsHeader.Consignor, false, IsAddressExtended));
		CachedValue<ITrader> consignor;

		protected ITrader Consignee => CachedValueHelper.GetValue(ref consignee, () => TraderWrapper.New(NctsHeader.Consignee, false, IsAddressExtended));
		CachedValue<ITrader> consignee;

		protected ITrader Principal => CachedValueHelper.GetValue(ref principal, () => TraderWrapper.New(NctsHeader.Principal, true, IsAddressExtended));
		CachedValue<ITrader> principal;

		protected ITrader Carrier => CachedValueHelper.GetValue(ref carrier, () => TraderWrapper.New(NctsHeader.MovementHeader?.Carrier, false, IsAddressExtended));
		CachedValue<ITrader> carrier;

		#endregion

		#region Document Properties

		public ZBool IsFallBackActive => NctsHeader.FallBackIsActive;

		public ZString Mrn => NctsHeader.MovementReferenceNumber;

		public ZString Pending_MRN => new ZString(Res.GetString("B47FE785-38C8-47B3-A26C-9B409541FC64", "Declaration pending MRN"));

		public ZString MOVEMENTREFERENCENUMBER => GetMovementReferenceNumber();

		public ZString EMAILSUBJECT => IsFallBackActive ? new ZString(Res.GetString("C6736BC6-209C-47C1-AE70-D7388E5C18C6", "Fallback")) : MOVEMENTREFERENCENUMBER;

		public ZString LOCALREFERENCENUMBER => GetLocalReferenceNumber();

		#endregion

		protected virtual ZString GetMovementReferenceNumber()
		{
			if (IsFallBackActive)
			{
				return ZString.Empty;
			}
			return Mrn.IsEmpty ? Pending_MRN : Mrn;
		}

		protected virtual ZString GetLocalReferenceNumber() => NctsHeader.LocalReferenceNumber;
	}
}
