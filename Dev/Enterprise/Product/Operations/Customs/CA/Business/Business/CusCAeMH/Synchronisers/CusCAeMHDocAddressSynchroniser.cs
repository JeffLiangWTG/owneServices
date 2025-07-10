using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHDocAddressSynchroniser : BusinessObjectSynchroniser
	{
		public CusCAeMHDocAddressSynchroniser(CAeMHDocAddress destination, JobDocAddress source)
			: base(destination, source)
		{ }

		protected new JobDocAddress Source
		{
			get { return (JobDocAddress)base.Source; }
		}

		protected new CAeMHDocAddress Destination
		{
			get { return (CAeMHDocAddress)base.Destination; }
		}

		protected override void ForceSynchroniseCore()
		{
			base.ForceSynchroniseCore();
			Destination.SynchroniseWithParent(Source);
			Destination.ReadOnly = true;
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			if (Source != null && !Source.IsDeleted && Destination != null && !Destination.IsDeleted)
			{
				if (Destination.E2_AddressType.IsEmpty || Destination.E2_AddressType == "?")
				{
					Destination.E2_AddressType = GetDestinationAddressType(Source.E2_AddressType);
				}
				Destination.SynchroniseWithParent(Source, false);
				Destination.ReadOnly = true;
			}
		}

		protected override void UnHookSynchronisers()
		{
			base.UnHookSynchronisers();
			Destination.DeSynchroniseWithParent();
			Destination.ReadOnly = false;
		}

		internal static ZString GetDestinationAddressType(ZString sourceAddressType)
		{
			switch (sourceAddressType)
			{
				case DocAddressTypes.Codes.ConsigneeDocumentaryAddress:
				case DocAddressTypes.Codes.ConsignorDocumentaryAddress:
				case DocAddressTypes.Codes.ConsigneePickupDeliveryAddress:
				case DocAddressTypes.Codes.NotifyParty:
					return sourceAddressType;
				default:
					return ZString.Empty;
			}
		}
	}
}
