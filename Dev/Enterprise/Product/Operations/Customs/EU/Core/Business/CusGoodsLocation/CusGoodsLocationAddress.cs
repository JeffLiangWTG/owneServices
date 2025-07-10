using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business
{
	public class CusGoodsLocationAddress : JobDocAddress
	{
		public CusGoodsLocationAddress(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("499C04A9-A05C-4E9C-BAF9-235AB5C51C00", Caption = "Postcode")]
		public override ZString E2_Postcode
		{
			get => base.E2_Postcode;
			set => base.E2_Postcode = value;
		}

		[ResourceStringData("6E0CAFA8-B0C4-4B91-A689-1538910713B7", Caption = "City")]
		public override ZString E2_City
		{
			get => base.E2_City;
			set => base.E2_City = value;
		}

		[ResourceStringData("2A1C6798-1CF5-41A3-BA67-10B04C6A8D5A", Caption = "Country")]
		[ReadOnlyMember(nameof(E2_RN_NKCountryCodeReadonly))]
		public override ZString E2_RN_NKCountryCode
		{
			get => base.E2_RN_NKCountryCode;
			set => base.E2_RN_NKCountryCode = value;
		}

		public bool E2_RN_NKCountryCodeReadonly => E2_RN_NKCountryCodeReadonlyCore;
		protected virtual bool E2_RN_NKCountryCodeReadonlyCore => false;

		[ResourceStringData("22C98281-0D8A-40B7-A120-D225D607CACA", Caption = "Street + Number")]
		public override ZString E2_Address1
		{
			get => base.E2_Address1;
			set => base.E2_Address1 = value;
		}

		[ResourceStringData("9E72A67C-845E-4FAA-87C2-B8812359A6A9", Caption = "Name")]
		public override ZString E2_Contact
		{
			get => base.E2_Contact;
			set => base.E2_Contact = value;
		}

		[ResourceStringData("599E8E8A-1045-4492-AEED-C580A313E666", Caption = "Phone Number")]
		public override ZString E2_Phone
		{
			get => base.E2_Phone;
			set => base.E2_Phone = value;
		}

		[ResourceStringData("BEBFFE4D-A674-4825-96E5-CFBE0FB16420", Caption = "Email")]
		public override ZString E2_Email
		{
			get => base.E2_Email;
			set => base.E2_Email = value;
		}

		[ReadOnlyMember(nameof(E2_GovRegNumReadOnly))]
		[ResourceStringData("6A4A61C8-DA9D-477E-94BD-D0C21CD0CB5B", Caption = "EORI Number")]
		public override ZString E2_GovRegNum
		{
			get => base.E2_GovRegNum;
			set => base.E2_GovRegNum = value;
		}

		public bool E2_GovRegNumReadOnly { get; set; }

		[ReadOnlyMember(nameof(AuthorisationNumberReadOnly))]
		[ResourceStringData("60C4F890-8A87-45F7-A4DE-97C132048D46", Caption = "Authorization Number", ShortCaption = "Authorization No.")]
		[List(nameof(Lookups) + "." + nameof(CusGoodsLocationAddressLookups.AuthorisationNumberList))]
		public virtual ZString AuthorisationNumber
		{
			get => base.E2_GovRegNum;
			set => base.E2_GovRegNum = value;
		}

		protected virtual bool AuthorisationNumberReadOnly => !IdentificationHolderPK.IsValid;

		public ZWrappedPropertyInfo AuthorisationNumberInfo => GetWrappedZPropertyInfo(nameof(AuthorisationNumber), x => E2_GovRegNumInfo);

		[ResourceStringData("4CA4B728-4791-4B09-85B1-3C4D3AFC4C4B", Caption = "Organization")]
		[List(nameof(Lookups) + "." + nameof(CusGoodsLocationAddressLookups.OrganisationList))]
		public virtual ZGuid IdentificationHolderPK
		{
			get
			{
				ZGuid parsedGuid;
				if (ZGuid.TryParse(E2_AdditionalAddressInformation, out parsedGuid))
				{
					return parsedGuid;
				}

				return ZGuid.Empty;
			}
			set
			{
				var oldValue = IdentificationHolderPK;
				E2_AdditionalAddressInformation = value.ToString();
				if (!IsCopying && oldValue != IdentificationHolderPK && IdentificationHolderPKChanged != null)
				{
					IdentificationHolderPKChanged(this, EventArgs.Empty);
				}
			}
		}

		public ZWrappedPropertyInfo IdentificationHolderPKInfo => GetWrappedZPropertyInfo(nameof(IdentificationHolderPK), x => E2_AdditionalAddressInformationInfo);

		public event EventHandler IdentificationHolderPKChanged;

		public OrgHeader IdentificationHolder => Factory.Load<OrgHeader>(IdentificationHolderPK);

		[ResourceStringData("208E0700-CD26-4ED4-9559-D721F3603268", Caption = "GNSS Latitude")]
		public new ZDecimal E2_Latitude
		{
			get => base.E2_Latitude;
			set => base.E2_Latitude = value;
		}

		[ResourceStringData("A86D8A6A-F80F-4931-913B-7A65E1232769", Caption = "GNSS Longitude")]
		public new ZDecimal E2_Longitude
		{
			get => base.E2_Longitude;
			set => base.E2_Longitude = value;
		}

		[ResourceStringData("EF60EC9D-749A-45BD-9E38-CE1A61EBD650", Caption = "Street + Number")]
		public override ZString E2_Address1AndE2_Address2
		{
			get => base.E2_Address1AndE2_Address2;
			set => base.E2_Address1AndE2_Address2 = value;
		}

		public new CusGoodsLocationAddressLookups Lookups => (CusGoodsLocationAddressLookups)base.Lookups;

		protected override JobDocAddressLookups GetNewLookups()
		{
			return new CusGoodsLocationAddressLookups(this);
		}

		public new CusGoodsLocationAddressValidation Validation => (CusGoodsLocationAddressValidation)base.Validation;

		protected override JobDocAddressValidation GetNewValidation()
		{
			return new CusGoodsLocationAddressValidation(this);
		}

		protected override bool SupportsCloneCore() => true;

		public CusGoodsLocation GoodsLocation => (CusGoodsLocation)ParentLoaders.LoadBusinessObject(Factory, E2_ParentTableCode, E2_ParentID);

		TypeLoaderCollection parentLoaders;
		protected virtual TypeLoaderCollection ParentLoaders => parentLoaders ?? (parentLoaders = new TypeLoaderCollection(typeof(CusGoodsLocation)));
	}
}
