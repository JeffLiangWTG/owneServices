namespace Enterprise.Customs.CA.Business
{
	using System;
	using System.Collections.Generic;
	using CargoWise.EntityFramework;
	using Enterprise.Customs.Business;
	using Enterprise.MasterFiles.Business;

	public class DeliveryDocAddressSynchroniser : BusinessObjectSynchroniser
	{
		public DeliveryDocAddressSynchroniser(CusSCAHouse destination, JobDocAddress source)
			: base(destination, source)
		{
		}

		public new JobDocAddress Source
		{
			get { return (JobDocAddress)base.Source; }
		}

		public new CusSCAHouse Destination
		{
			get { return (CusSCAHouse)base.Destination; }
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_DeliveryAddress1Info,
				delegate
				{ return Source.E2_Address1.SubstringSafe(0, Destination.CA_DeliveryAddress1Info.MaxLength); },
				delegate
				{ return new List<ZPropertyInfo>() { Source.E2_Address1Info }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_DeliveryAddress2Info,
				delegate
				{ return Source.E2_Address2.SubstringSafe(0, Destination.CA_DeliveryAddress2Info.MaxLength); },
				delegate
				{ return new List<ZPropertyInfo>() { Source.E2_Address2Info }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_RN_NKDeliveryCountryCodeInfo, Source.E2_RN_NKCountryCodeInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_DeliveryContactNameInfo,
				delegate
				{ return Source.E2_Contact.SubstringSafe(0, Destination.CA_DeliveryContactNameInfo.MaxLength); },
				delegate
				{ return new List<ZPropertyInfo>() { Source.E2_ContactInfo }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_DeliveryNameInfo,
				delegate
				{ return Source.E2_CompanyName.SubstringSafe(0, Destination.CA_DeliveryNameInfo.MaxLength); },
				delegate
				{ return new List<ZPropertyInfo>() { Source.E2_CompanyNameInfo }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_DeliveryPhoneInfo,
				delegate
				{ return Source.E2_Phone.SubstringSafe(0, Destination.CA_DeliveryPhoneInfo.MaxLength); },
				delegate
				{ return new List<ZPropertyInfo>() { Source.E2_PhoneInfo }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_DeliveryPostcodeInfo,
				delegate
				{ return Source.E2_Postcode.SubstringSafe(0, Destination.CA_DeliveryPostcodeInfo.MaxLength); },
				delegate
				{ return new List<ZPropertyInfo>() { Source.E2_PostcodeInfo }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_DeliveryStateInfo,
				delegate
				{ return Source.E2_State.SubstringSafe(0, Destination.CA_DeliveryStateInfo.MaxLength); },
				delegate
				{ return new List<ZPropertyInfo>() { Source.E2_StateInfo }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_DeliverySuburbInfo,
				delegate
				{ return Source.E2_City.SubstringSafe(0, Destination.CA_DeliverySuburbInfo.MaxLength); },
				delegate
				{ return new List<ZPropertyInfo>() { Source.E2_CityInfo }; }));
			Source.DocAddressChanged += new EventHandler(DocAddress_Changed);
		}

		protected override void UnHookSynchronisers()
		{
			Source.DocAddressChanged -= new EventHandler(DocAddress_Changed);
		}

		void DocAddress_Changed(object sender, EventArgs e)
		{
			Synchronise();
		}
	}
}
