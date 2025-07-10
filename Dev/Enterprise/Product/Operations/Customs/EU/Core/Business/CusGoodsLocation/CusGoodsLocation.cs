using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using CusGoodsLocationQualifierList = Enterprise.Customs.Business.CusGoodsLocationQualifierList;

namespace Enterprise.Customs.EU.Business
{
	public class CusGoodsLocation : Customs.Business.CusGoodsLocation
		, Integration.Customs.EU.ICusGoodsLocation
		, ISupportMultipleResourceStringData
	{
		public CusGoodsLocation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static new readonly CusGoodsLocationTypeDecider TypeDecider = new CusGoodsLocationTypeDecider();

		protected override TypeLoaderCollection GetParentLoaders()
		{
			var parentLoaders = base.GetParentLoaders();
			parentLoaders.Add(typeof(CusTempStorageJobHeader));
			parentLoaders.Add(typeof(TemporaryStorageHeader));
			return parentLoaders;
		}

		[ResourceStringData("EU.Business.Declaration.CusGoodsLocation|DisplayText", Caption = "Goods Location")]
		public virtual ZString DisplayText
		{
			get
			{
				var stringBuilder = new ZStringBuilder();
				stringBuilder.AppendIfNotEmpty(CGL_Qualifier);
				stringBuilder.AppendIfNotEmpty(CGL_Type);
				var latitude = Address.E2_Latitude;
				var longitude = Address.E2_Longitude;
				stringBuilder.AppendIfNotEmpty(latitude != ZDecimal.Zero || longitude != ZDecimal.Zero ? $"{latitude:0.0000000},{longitude:0.0000000}" : string.Empty);
				stringBuilder.AppendIfNotEmpty(Address.E2_GovRegNum);
				stringBuilder.AppendIfNotEmpty(Address.E2_Address1AndE2_Address2);
				stringBuilder.AppendIfNotEmpty(Address.E2_Postcode);
				stringBuilder.AppendIfNotEmpty(CGL_AdditionalIdentifier);
				stringBuilder.AppendIfNotEmpty(CGL_CustomsOffice);
				stringBuilder.AppendIfNotEmpty(Address.E2_City);
				stringBuilder.AppendIfNotEmpty(Address.E2_RN_NKCountryCode);
				var contact = Address.E2_Contact;
				stringBuilder.AppendIfNotEmpty(contact.IsEmpty ? string.Empty : $"Contact {contact}");
				var phone = Address.E2_Phone;
				stringBuilder.AppendIfNotEmpty(phone.IsEmpty ? string.Empty : $"Ph {phone}");
				stringBuilder.AppendIfNotEmpty(Address.E2_Email);

				return stringBuilder.ToStringWithDelimiterBetweenAppends(";");
			}
		}

		public ZPropertyInfo DisplayTextInfo => GetZPropertyInfo(nameof(DisplayText));

		[ReadOnlyMember(nameof(CGL_TypeReadonly))]
		public override ZString CGL_Type
		{
			get => base.CGL_Type;
			set
			{
				var oldValue = CGL_Type;
				base.CGL_Type = value;

				if (oldValue != CGL_Type && !IsCopying && !IsValidationSuspended)
				{
					Validation.ValidateCGL_Qualifier();
				}
			}
		}

		public bool CGL_TypeReadonly => CGL_TypeReadonlyCore;
		protected virtual bool CGL_TypeReadonlyCore => false;

		[ReadOnlyMember(nameof(CGL_QualifierReadonly))]
		public override ZString CGL_Qualifier
		{
			get => base.CGL_Qualifier;
			set
			{
				var oldValue = CGL_Qualifier;
				base.CGL_Qualifier = value;

				if (oldValue != CGL_Qualifier && !IsCopying)
				{
					ClearFields();
				}
			}
		}

		public bool CGL_QualifierReadonly => CGL_QualifierReadonlyCore;
		protected virtual bool CGL_QualifierReadonlyCore => false;

		[ResourceStringData("f4bdbe25-5731-4b89-b910-d07e469ce09d", Caption = "Additional Identifier")]
		[MaxLength(nameof(AdditionalIdentifierMaxLength))]
		public virtual ZString AdditionalIdentifier
		{
			get => CGL_AdditionalIdentifier;
			set => CGL_AdditionalIdentifier = value;
		}

		public ZPropertyInfo AdditionalIdentifierInfo => GetWrappedZPropertyInfo(nameof(AdditionalIdentifier), x => CGL_AdditionalIdentifierInfo);

		protected virtual int AdditionalIdentifierMaxLength => 4;

		public CusGoodsLocationAddress Address
		{
			get
			{
				if (address == null)
				{
					address = LoadOrCreateCusGoodsLocationAddress();
					address.MakePersistentEvenIfEmpty();
					address.IdentificationHolderPKChanged += AddressIdentificationHolderPKChanged;
					RegisterEditableChildObject(address);
					RegisterListChangedCalledRefreshBinding(address);
					address.IgnoreValidationStatusError = true;
				}
				return address;
			}
		}
		CusGoodsLocationAddress address;

		protected override bool SupportsCloneCore() => true;

		[ResourceStringData("F1344A07-AFD1-4FE8-B343-8FF7514BEDCD", Caption = "UNLOCODE")]
		[List(nameof(Lookups) + "." + nameof(CusGoodsLocationLookups.UnlocodeList))]
		public virtual ZString Unlocode
		{
			get => CGL_AdditionalIdentifier;
			set => CGL_AdditionalIdentifier = value;
		}

		public virtual ZPropertyInfo UnlocodeInfo => GetWrappedZPropertyInfo(nameof(Unlocode), x => CGL_AdditionalIdentifierInfo);

		[ResourceStringData("6A7CE3D2-A17C-47C9-B954-D77D4090CCF1", Caption = "Customs Office")]
		[List(nameof(Lookups) + "." + nameof(CusGoodsLocationLookups.CustomsOfficeList))]
		[MaxLength(10)]
		public override ZString CGL_CustomsOffice
		{
			get => base.CGL_CustomsOffice;
			set
			{
				var oldvalue = CGL_CustomsOffice;
				base.CGL_CustomsOffice = value;
				if (!IsCopying && oldvalue != base.CGL_CustomsOffice)
				{
					Parent?.MarkAsNeedingValidation();
				}
			}
		}

		public virtual bool ContactPersonDataVisible => !CGL_Qualifier.IsEmpty;

		public new CusGoodsLocationLookups Lookups => (CusGoodsLocationLookups)base.Lookups;

		protected override Customs.Business.CusGoodsLocationLookups GetNewLookups()
		{
			return new CusGoodsLocationLookups(this);
		}

		public new CusGoodsLocationValidation Validation => (CusGoodsLocationValidation)base.Validation;

		protected override Customs.Business.CusGoodsLocationValidation GetNewValidation() => new CusGoodsLocationValidation(this);

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteChildren<CusGoodsLocationAddress>(JobDocAddressSchema.E2_ParentID);
			}
			base.Delete();
		}

		protected virtual CusGoodsLocationAddress CreateCusGoodsLocationAddressCore()
		{
			return (CusGoodsLocationAddress)Factory.New(AddressType);
		}

		protected virtual void AddressIdentificationHolderPKChanged(object sender, EventArgs e)
		{
			var organisation = address.IdentificationHolder;
			switch (CGL_Qualifier)
			{
				case CusGoodsLocationQualifierList.Codes.EoriNumber:
					UpdateAddressEoriNumber(organisation);
					break;
				case CusGoodsLocationQualifierList.Codes.AuthorizationNumber:
					ClearAddressAuthorisationNumber();
					break;
			}
		}

		protected virtual void UpdateAddressEoriNumber(OrgHeader organisation)
		{
			if (organisation != null)
			{
				var orgCusCodes = organisation.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
				if (orgCusCodes.Length >= 1)
				{
					var eoriNumber = orgCusCodes.Length == 1
						? orgCusCodes[0].OK_CustomsRegNo
						: (orgCusCodes.FirstOrDefault(x => x.OK_RN_NKCodeCountry == GlbCompany.CurrentCompany.GC_RN_NKCountryCode)?.OK_CustomsRegNo ?? orgCusCodes[0].OK_CustomsRegNo);

					address.E2_GovRegNum = eoriNumber;
					address.E2_GovRegNumType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
					address.E2_GovRegNumReadOnly = true;
				}
				else
				{
					ClearAddressEoriNumber();
				}
			}
			else
			{
				ClearAddressEoriNumber();
			}

			void ClearAddressEoriNumber()
			{
				address.E2_GovRegNum = ZString.Empty;
				address.E2_GovRegNumType = ZString.Empty;
				address.E2_GovRegNumReadOnly = false;
			}
		}

		protected void ClearAddressAuthorisationNumber()
		{
			address.E2_GovRegNum = ZString.Empty;
		}

		protected virtual CusGoodsLocationAddress LoadOrCreateCusGoodsLocationAddress()
		{
			return LoadCusGoodsLocationAddress() ?? CreateCusGoodsLocationAddress();
		}

		CusGoodsLocationAddress LoadCusGoodsLocationAddress()
		{
			var filter = new ZQuery(JobDocAddressSchema.E2_ParentID, SQLComparisonOperator.Equal, PK);
			filter.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_ParentTableCode, SQLComparisonOperator.Equal, CusGoodsLocationSchema.Constants.Prefix);
			filter.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_AddressType, SQLComparisonOperator.Equal, DocAddressTypes.Codes.Location);
			return (CusGoodsLocationAddress)Factory.LoadTop1(AddressType, filter);
		}

		CusGoodsLocationAddress CreateCusGoodsLocationAddress()
		{
			var goodsLocationAddress = CreateCusGoodsLocationAddressCore();
			using (goodsLocationAddress.SuspendSettingHasChanges())
			{
				goodsLocationAddress.E2_ParentID = PK;
				goodsLocationAddress.E2_ParentTableCode = CusGoodsLocationSchema.Constants.Prefix;
				goodsLocationAddress.E2_AddressType = DocAddressTypes.Codes.Location;
				goodsLocationAddress.E2_AddressOverride = GoodsLocationAddressOverride;
				goodsLocationAddress.E2_RN_NKCountryCode = ZString.Empty;
			}
			return goodsLocationAddress;
		}

		void ClearFields()
		{
			CGL_AdditionalIdentifier = ZString.Empty;
			if (address != null)
			{
				ClearAddressFields(address);
			}
		}

		protected virtual void ClearAddressFields(CusGoodsLocationAddress address)
		{
			address.E2_RN_NKCountryCode = ZString.Empty;
			address.E2_GeoLocation = ZGeography.Empty;
			address.IdentificationHolderPK = ZGuid.Empty;
			address.OrganisationPK = ZGuid.Empty;
			address.E2_GovRegNum = ZString.Empty;
			address.E2_GovRegNumType = ZString.Empty;
			address.E2_GovRegNumReadOnly = false;
			address.E2_Address1AndE2_Address2 = ZString.Empty;
			address.E2_City = ZString.Empty;
			address.E2_Postcode = ZString.Empty;
			address.E2_Contact = ZString.Empty;
			address.E2_Phone = ZString.Empty;
			address.E2_Email = ZString.Empty;
		}

		protected virtual Type AddressType => typeof(CusGoodsLocationAddress);
		protected virtual ZBool GoodsLocationAddressOverride => ZBool.True;

		public IReadOnlyList<string> MultipleKeysToUse => Parent is ISupportMultipleResourceStringData multipleResStringParent
			? multipleResStringParent.MultipleKeysToUse
			: Array.Empty<string>();

		public void BeginEdit() => BeginEditCore();

		protected virtual void BeginEditCore() { }
	}
}
