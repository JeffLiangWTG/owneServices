namespace Enterprise.Customs.CA.Business
{
	using System;
	using System.Collections.Generic;
	using CargoWise.EntityFramework;
	using Enterprise.Customs.Business;
	using Enterprise.MasterFiles.Business;

	public class NotifyDocAddressSynchroniser : BusinessObjectSynchroniser
	{
		public NotifyDocAddressSynchroniser(CusSCAHouse destination, JobDocAddress source)
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
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_OH_NotifyInfo,
				delegate
				{ return Source.Organisation != null && Destination.CA_OH_Notify != Source.OrganisationPK ? Source.OrganisationPK : Destination.CA_OH_Notify; },
				delegate
				{ return new List<ZPropertyInfo>() { Source.OrganisationPKInfo }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_NotifyAddress1Info,
				delegate
				{ return Source.E2_Address1.SubstringSafe(0, Destination.CA_NotifyAddress1Info.MaxLength); },
				delegate
				{ return new List<ZPropertyInfo>() { Source.E2_Address1Info }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_NotifyAddress2Info,
				delegate
				{ return Source.E2_Address2.SubstringSafe(0, Destination.CA_NotifyAddress2Info.MaxLength); },
				delegate
				{ return new List<ZPropertyInfo>() { Source.E2_Address2Info }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_RN_NKNotifyCountryCodeInfo, Source.E2_RN_NKCountryCodeInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_NotifyContactNameInfo,
				delegate
				{ return Source.E2_Contact.SubstringSafe(0, Destination.CA_NotifyContactNameInfo.MaxLength); },
				delegate
				{ return new List<ZPropertyInfo>() { Source.E2_ContactInfo }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_NotifyNameInfo,
				delegate
				{ return Source.E2_CompanyName.SubstringSafe(0, Destination.CA_NotifyNameInfo.MaxLength); },
				delegate
				{ return new List<ZPropertyInfo>() { Source.E2_CompanyNameInfo }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_NotifyPhoneInfo,
				delegate
				{ return Source.E2_Phone.SubstringSafe(0, Destination.CA_NotifyPhoneInfo.MaxLength); },
				delegate
				{ return new List<ZPropertyInfo>() { Source.E2_PhoneInfo }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_NotifyPostcodeInfo,
				delegate
				{ return Source.E2_Postcode.SubstringSafe(0, Destination.CA_NotifyPostcodeInfo.MaxLength); },
				delegate
				{ return new List<ZPropertyInfo>() { Source.E2_PostcodeInfo }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_NotifyStateInfo,
				delegate
				{ return Source.E2_State.SubstringSafe(0, Destination.CA_NotifyStateInfo.MaxLength); },
				delegate
				{ return new List<ZPropertyInfo>() { Source.E2_StateInfo }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_NotifySuburbInfo,
				delegate
				{ return Source.E2_City.SubstringSafe(0, Destination.CA_NotifySuburbInfo.MaxLength); },
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
