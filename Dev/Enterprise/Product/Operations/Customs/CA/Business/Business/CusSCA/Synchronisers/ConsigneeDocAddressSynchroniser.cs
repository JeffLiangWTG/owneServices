namespace Enterprise.Customs.CA.Business
{
	using System;
	using System.Collections.Generic;
	using CargoWise.EntityFramework;
	using Enterprise.Customs.Business;
	using Enterprise.MasterFiles.Business;

	public class ConsigneeDocAddressSynchroniser : BusinessObjectSynchroniser
	{
		public ConsigneeDocAddressSynchroniser(CusSCAHouse destination, JobDocAddress source)
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
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_OH_ConsigneeInfo,
				delegate
				{ return Source.Organisation != null && Destination.CA_OH_Consignee != Source.OrganisationPK ? Source.OrganisationPK : Destination.CA_OH_Consignee; },
				delegate
				{ return new List<ZPropertyInfo>() { Source.OrganisationPKInfo }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_ConsigneeAddress1Info,
				delegate
				{ return Source.E2_Address1.SubstringSafe(0, Destination.CA_ConsigneeAddress1Info.MaxLength); },
				delegate
				{ return new List<ZPropertyInfo>() { Source.E2_Address1Info }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_ConsigneeAddress2Info,
				delegate
				{ return Source.E2_Address2.SubstringSafe(0, Destination.CA_ConsigneeAddress2Info.MaxLength); },
				delegate
				{ return new List<ZPropertyInfo>() { Source.E2_Address2Info }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_RN_NKConsigneeCountryCodeInfo, Source.E2_RN_NKCountryCodeInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_ConsigneeContactNameInfo,
				delegate
				{ return Source.E2_Contact.SubstringSafe(0, Destination.CA_ConsigneeContactNameInfo.MaxLength); },
				delegate
				{ return new List<ZPropertyInfo>() { Source.E2_ContactInfo }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_ConsigneeNameInfo,
				delegate
				{ return Source.E2_CompanyName.SubstringSafe(0, Destination.CA_ConsigneeNameInfo.MaxLength); },
				delegate
				{ return new List<ZPropertyInfo>() { Source.E2_CompanyNameInfo }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_ConsigneePhoneInfo,
				delegate
				{ return Source.E2_Phone.SubstringSafe(0, Destination.CA_ConsigneePhoneInfo.MaxLength); },
				delegate
				{ return new List<ZPropertyInfo>() { Source.E2_PhoneInfo }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_ConsigneePostcodeInfo,
				delegate
				{ return Source.E2_Postcode.SubstringSafe(0, Destination.CA_ConsigneePostcodeInfo.MaxLength); },
				delegate
				{ return new List<ZPropertyInfo>() { Source.E2_PostcodeInfo }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_ConsigneeStateInfo,
				delegate
				{ return Source.E2_State.SubstringSafe(0, Destination.CA_ConsigneeStateInfo.MaxLength); },
				delegate
				{ return new List<ZPropertyInfo>() { Source.E2_StateInfo }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_ConsigneeSuburbInfo,
				delegate
				{ return Source.E2_City.SubstringSafe(0, Destination.CA_ConsigneeSuburbInfo.MaxLength); },
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
