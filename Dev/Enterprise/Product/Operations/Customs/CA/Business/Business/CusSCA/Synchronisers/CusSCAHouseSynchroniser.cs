using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CusSCAHouseSynchroniser : BusinessObjectSynchroniser
	{
		public CusSCAHouseSynchroniser(CusSCAHouse destination, ForwardingShipment source)
			: base(destination, source)
		{
		}

		public new ForwardingShipment Source
		{
			get { return (ForwardingShipment)base.Source; }
		}

		public new CusSCAHouse Destination
		{
			get { return (CusSCAHouse)base.Destination; }
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			if (Source != null && !Source.IsDeleted && Destination != null && !Destination.IsDeleted)
			{
				if (Destination.CA_JS.IsEmpty)
				{
					Destination.CA_JS = Source.PK;
				}

				SetReferenceNumbers();
				Synchronisers.Add(new FieldSynchroniser(Destination.CA_RL_NK_PortOfDestinationInfo,
								delegate
								{ return Source.Destination != null ? Source.Destination.Code : ZString.Empty; },
								delegate
								{ return new List<ZPropertyInfo>() { Source.JS_RL_NKDestinationInfo }; }));
				Synchronisers.Add(new FieldSynchroniser(Destination.CA_HouseBillInfo, Source.JS_HouseBillInfo));
				Synchronisers.Add(new ConsigneeDocAddressSynchroniser(Destination, Source.ConsigneeDocumentaryAddress));
				Synchronisers.Add(new ConsignorDocAddressSynchroniser(Destination, Source.ConsignorDocumentaryAddress));
				Synchronisers.Add(new NotifyDocAddressSynchroniser(Destination, Source.NotifyPartyDocumentaryAddress));
				Synchronisers.Add(new DeliveryDocAddressSynchroniser(Destination, Source.ConsigneeDeliveryAddress));
				Synchronisers.Add(new CusSCAPivotCollectionSynchroniser(Destination, Source));
				Source.JS_UniqueConsignRefInfo.ValueChanged += JS_UniqueConsignRef_ValueChanged;
				Destination.SetReadOnlyWhereRequired();
			}
		}

		protected override void UnHookSynchronisers()
		{
			base.UnHookSynchronisers();
			Source.JS_UniqueConsignRefInfo.ValueChanged -= JS_UniqueConsignRef_ValueChanged;
		}

		void JS_UniqueConsignRef_ValueChanged(object sender, EventArgs e)
		{
			SetReferenceNumbers();
		}

		void SetReferenceNumbers()
		{
			if (Source != null && !Source.JS_UniqueConsignRef.IsEmpty)
			{
				if (Destination.SupplementaryReferenceNumber.IsEmpty)
				{
					ZString srn = Source.Factory.CanadianCarrierCode() + Source.JS_UniqueConsignRef.Trim() + CACustomsDataRegistry.Instance.SupplementaryNumberSuffixAppliesAllCountries.Value;
					Destination.SupplementaryReferenceNumber = srn.Left(Destination.SupplementaryReferenceNumberInfo.MaxLength);
				}
				if (Destination.CA_BGMReference.IsEmpty)
				{
					Destination.CA_BGMReference = Source.JS_UniqueConsignRef.Trim().Right(Destination.CA_BGMReferenceInfo.MaxLength);
				}
			}
		}
	}
}
