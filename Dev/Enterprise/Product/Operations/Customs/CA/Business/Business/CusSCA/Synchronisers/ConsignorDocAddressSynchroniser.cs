
namespace Enterprise.Customs.CA.Business
{
	using System;
	using System.Collections.Generic;
	using CargoWise.EntityFramework;
	using Enterprise.Customs.Business;
	using Enterprise.MasterFiles.Business;

	public class ConsignorDocAddressSynchroniser : BusinessObjectSynchroniser
	{
		public ConsignorDocAddressSynchroniser(CusSCAHouse destination, JobDocAddress source)
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
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_OH_ConsignorInfo,
				delegate
				{
					return Source.Organisation != null && Destination.CA_OH_Consignor != Source.OrganisationPK ? Source.OrganisationPK : Destination.CA_OH_Consignor;
				},
				delegate
				{ return new List<ZPropertyInfo>() { Source.OrganisationPKInfo }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_ConsignorAddress1Info,
				delegate
				{ return Source.E2_Address1.SubstringSafe(0, Destination.CA_ConsignorAddress1Info.MaxLength); },
				delegate
				{ return new List<ZPropertyInfo>() { Source.E2_Address1Info }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_ConsignorAddress2Info,
				delegate
				{ return Source.E2_Address2.SubstringSafe(0, Destination.CA_ConsignorAddress2Info.MaxLength); },
				delegate
				{ return new List<ZPropertyInfo>() { Source.E2_Address2Info }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_RN_NKConsignorCountryCodeInfo, Source.E2_RN_NKCountryCodeInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_ConsignorContactNameInfo,
				delegate
				{ return Source.E2_Contact.SubstringSafe(0, Destination.CA_ConsignorContactNameInfo.MaxLength); },
				delegate
				{ return new List<ZPropertyInfo>() { Source.E2_ContactInfo }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_ConsignorNameInfo,
				delegate
				{ return Source.E2_CompanyName.SubstringSafe(0, Destination.CA_ConsignorNameInfo.MaxLength); },
				delegate
				{ return new List<ZPropertyInfo>() { Source.E2_CompanyNameInfo }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_ConsignorPhoneInfo,
				delegate
				{ return Source.E2_Phone.SubstringSafe(0, Destination.CA_ConsignorPhoneInfo.MaxLength); },
				delegate
				{ return new List<ZPropertyInfo>() { Source.E2_PhoneInfo }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_ConsignorPostcodeInfo,
				delegate
				{ return Source.E2_Postcode.SubstringSafe(0, Destination.CA_ConsignorPostcodeInfo.MaxLength); },
				delegate
				{ return new List<ZPropertyInfo>() { Source.E2_PostcodeInfo }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_ConsignorStateInfo,
				delegate
				{ return Source.E2_State.SubstringSafe(0, Destination.CA_ConsignorStateInfo.MaxLength); },
				delegate
				{ return new List<ZPropertyInfo>() { Source.E2_StateInfo }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_ConsignorSuburbInfo,
				delegate
				{ return Source.E2_City.SubstringSafe(0, Destination.CA_ConsignorSuburbInfo.MaxLength); },
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
