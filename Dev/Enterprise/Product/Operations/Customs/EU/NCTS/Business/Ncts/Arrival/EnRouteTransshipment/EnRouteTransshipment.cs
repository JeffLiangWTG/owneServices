using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class EnRouteTransshipment : CusInBondEvent, Integration.Customs.EU.NCTS.IEnRouteTransshipment, ICusInBondContainerTypeSupporter
	{
		public EnRouteTransshipment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new NctsHeader Header => (NctsHeader)base.Header;

		[List(nameof(Lookups) + "." + nameof(EnRouteTransshipmentLookups.EventCountries))]
		[ResourceStringData("31A54AE6-0F6F-4CAD-914D-900EA6DCBE45", Caption = "Event Country/Region", ShortCaption = "Event Ctry./Rgn.")]
		public override ZString BN_EventCountryCode { get => base.BN_EventCountryCode; set => base.BN_EventCountryCode = value; }

		[List(nameof(Lookups) + "." + nameof(EnRouteTransshipmentLookups.NewTransportCountries))]
		[ResourceStringData("54C56913-C084-48FB-9752-69C69B9C43D4", Caption = "New Transport Nationality")]
		public override ZString BN_TransportCountryCode { get => base.BN_TransportCountryCode; set => base.BN_TransportCountryCode = value; }

		[ResourceStringData("83C8B3E8-D68A-4219-8D1E-512D632E8A3C", Caption = "Transhipment Date")]
		public override ZDateTime BN_EndorsementDate { get => base.BN_EndorsementDate; set => base.BN_EndorsementDate = value; }

		[ResourceStringData("65d758cb-06fc-4f80-98bb-fec224100c7d", Caption = "Place Reported")]
		public override ZString BN_EndorsementPlace { get => base.BN_EndorsementPlace; set => base.BN_EndorsementPlace = value; }

		[List(nameof(Lookups) + "." + nameof(EnRouteTransshipmentLookups.IncidentEndorsementCountries))]
		[ResourceStringData("112380f1-e06c-4bf8-ada5-257587b3af7e", Caption = "Country/Region Reported", ShortCaption = "Ctry./Rgn. Reported")]
		public override ZString BN_EndorsementCountryCode { get => base.BN_EndorsementCountryCode; set => base.BN_EndorsementCountryCode = value; }

		[ResourceStringData("EF0655FC-47BD-478A-B4F3-95036B6C4ABB", Caption = "In NCTS?")]
		public ZBool IsInNCTS
		{
			get => BN_CustomsStatus == SubmittedToNCTSCustomsStatus;
			set
			{
				BN_CustomsStatus = value ? SubmittedToNCTSCustomsStatus : string.Empty;
			}
		}
		const string SubmittedToNCTSCustomsStatus = "SUB";

		public ZWrappedPropertyInfo IsInNCTSInfo => GetWrappedZPropertyInfo(nameof(IsInNCTS), x => BN_CustomsStatusInfo);

		[ChildEditable]
		public EnRouteTransshipmentNctsContainerCollection Containers
		{
			get
			{
				if (containers == null)
				{
					containers = new EnRouteTransshipmentNctsContainerCollection(this);
					RegisterEditableChildObject(containers);
				}
				return containers;
			}
		}
		EnRouteTransshipmentNctsContainerCollection containers;

		Type ICusInBondContainerTypeSupporter.ContainerType => typeof(NctsContainer);

		public IEnumerable<ZString> ContainersNumbers
		{
			get
			{
				foreach (var container in Containers.Where(x => !x.BC_ContainerNum.IsEmpty))
				{
					yield return container.BC_ContainerNum;
				}
			}
		}

		public new EnRouteTransshipmentLookups Lookups => (EnRouteTransshipmentLookups)base.Lookups;

		protected override CusInBondEventLookups GetNewLookups() => new EnRouteTransshipmentLookups(this);

		protected override CusInBondEventValidation GetNewValidation() => new EnRouteTransshipmentValidation(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BN_Type = CusInBondEventTypeList.Codes.Transshipment;
		}
	}
}
