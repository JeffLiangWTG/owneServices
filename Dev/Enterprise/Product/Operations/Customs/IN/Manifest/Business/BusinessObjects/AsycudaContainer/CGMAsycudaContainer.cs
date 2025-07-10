using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IN.Manifest.Business;

public sealed class CGMAsycudaContainer : ASYCUDA.Business.AsycudaContainer, Integration.Customs.ASYCUDA.INManifest.ICGMAsycudaContainer, IDocAddresses
{
	public CGMAsycudaContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	#region Schema

	public new class Schema : ManifestBase.AutoAsycudaContainer.Schema
	{
		public const string ContainerAgentCode = "ContainerAgentCode";
		public const string ContainerAgentCodeOrgPK = "ContainerAgentCodeOrgPK";
		public const string ISOCode = "ISOCode";

		public const int ISOCodeMaxLength = 4;
	}

	#endregion

	#region Propertes

	public new CGMAsycudaManifestHeader Header => (CGMAsycudaManifestHeader)base.Header;

	[ResourceStringData("091A4C96-AC53-4022-A9D4-918069FDA01C", Caption = "Shipper's Own Container", MediumCaption = "Shipper's Own Cont.", ShortCaption = "SOC")]
	public override ZBool ACN_IsShipperOwned => base.ACN_IsShipperOwned;

	[ResourceStringData("B0D34B25-8A2B-4093-AD9E-E28003FEF13F", Caption = "Container Weight", MediumCaption = "Cont. Weight", ShortCaption = "Cont. Wt.")]
	public override ZDecimal ACN_GoodsWeight => base.ACN_GoodsWeight;

	public decimal ContainerWeightInTonnes => Weight.ConvertSafe(ACN_GoodsWeight, ACN_GoodsWeightUQ, Weight.Tonnes);

	public override ZGuid ACN_RC_ContainerType
	{
		get => base.ACN_RC_ContainerType;
		set
		{
			base.ACN_RC_ContainerType = value;
			if (!IsCopying)
			{
				var isoContainerCodeFromType = GetISOCodeFromContainerType();
				if(!isoContainerCodeFromType.IsEmpty)
				{
					ISOCode = GetISOCodeFromContainerType();
				}
			}
		}
	}

	[List(nameof(ContainerAgentCodeDocAddress) + "." + nameof(JobDocAddress.Lookups) + "." + nameof(JobDocAddressLookups.Address_List))]
	[ResourceStringData("ED0DBD2B-C6B2-4C84-A755-5CCC17829E9F", Caption = "Container Agent Code Address", MediumCaption = "Cont. Agent Code Address", ShortCaption = "Cont. Agt. Cd. Address")]
	public ZGuid ContainerAgentCode
	{
		get => ContainerAgentCodeDocAddress.E2_OA_Address;
		set => ContainerAgentCodeDocAddress.E2_OA_Address = value;
	}

	public ZString ContainerAgentPAN => ContainerAgentCodeDocAddress.Organisation?.CustomsCodes.GetCustomsRegNo(IndiaOrgCusCodeInfo.OrgCusCodes.PAN, CountryCodes.India) ?? ZString.Empty;

	public ZPropertyInfo ContainerAgentCodeInfo => GetWrappedZPropertyInfo(Schema.ContainerAgentCode, x => ContainerAgentCodeDocAddress.E2_OA_AddressInfo);

	[List(nameof(Lookups) + "." + nameof(CGMAsycudaContainerLookups.ContainerAgentCodeOrganisations))]
	[ResourceStringData("0F69E1BC-DB15-4CF4-B235-AEDBE49FB295", Caption = "Container Agent Code", MediumCaption = "Cont. Agent Code", ShortCaption = "Cont. Agt. Cd.")]
	public ZGuid ContainerAgentCodeOrgPK
	{
		get => ContainerAgentCodeDocAddress.OrganisationPK;
		set => ContainerAgentCodeDocAddress.OrganisationPK = value;
	}

	public ZPropertyInfo ContainerAgentCodeOrgPKInfo => GetWrappedZPropertyInfo(Schema.ContainerAgentCodeOrgPK, x => ContainerAgentCodeDocAddress.OrganisationPKInfo);

	public JobDocAddress ContainerAgentCodeDocAddress
	{
		get
		{
			if (containerAgentCodeDocAddress == null || containerAgentCodeDocAddress.IsDeleted)
			{
				containerAgentCodeDocAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ContainerAgentCodeAddress);
			}

			return containerAgentCodeDocAddress;
		}
	}
	JobDocAddress containerAgentCodeDocAddress;

	[MaxLength(Schema.ISOCodeMaxLength)]
	[ReadOnlyMember(nameof(ISOCode_ReadOnly))]
	[List(nameof(Lookups) + "." + nameof(CGMAsycudaContainerLookups.ISOCodeList))]
	[ResourceStringData("6F70236D-419D-413C-B5F7-033ADD6BF3C1", Caption = "Customs Container Code", MediumCaption = "Container Code", ShortCaption = "Cont. Code")]
	public ZString ISOCode
	{
		get => this.GetSystemDefinedValue<ZString>(Schema.ISOCode);
		set
		{
			var oldValue = ISOCode;
			CheckMaximumLength(ISOCodeInfo, value);
			this.SetSystemDefinedValue(Schema.ISOCode, value);
			ISOCodeInfo.RefreshBinding(oldValue);
		}
	}

	public ZPropertyInfo ISOCodeInfo => GetZPropertyInfo(Schema.ISOCode);

	public bool ISOCode_ReadOnly => !GetISOCodeFromContainerType().IsEmpty;

	ZString GetISOCodeFromContainerType()
	{
		var isoCode = ZString.Empty;
		if (ContainerType is RefContainer refContainer)
		{
			var cacheKey = FormattableString.Invariant($"Enterprise.Customs.IN.Manifest.Business|GetISOCodeFromContainerType_{refContainer.PK}");
			isoCode = Factory.GetCachedValue(cacheKey, () => refContainer.GetCountrySpecificContainerCode(CountryCodes.India),
				CacheStalenessPolicy.StaleWhenDataTableChanges(RefContainerCodeMap.Schema.TableName, Factory));
		}
		return isoCode;
	}

	#endregion

	#region DocAddresses

	[ChildEditable(true)]
	public JobDocAddressDependentCollection DocAddresses
	{
		get
		{
			if (fDocAddresses == null)
			{
				fDocAddresses = new JobDocAddressDependentCollection(this);
				fDocAddresses.Load();
				RegisterEditableChildObject(fDocAddresses);
			}

			return fDocAddresses;
		}
	}
	JobDocAddressDependentCollection fDocAddresses;

	#endregion

	#region IDocAddresses

	JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType) => null;

	IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes => new[] { DocAddressType.ContainerAgentCodeAddress };

	SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress) => Env.Security.None;

	void IDocAddresses.DocAddressChanged(JobDocAddress docAddress) { }

	void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress) { }

	void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress) { }

	void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress) { }

	void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress) { }

	bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress) => false;

	OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType) => null;

	ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => null;

	#endregion

	#region Lookups

	public new CGMAsycudaContainerLookups Lookups => (CGMAsycudaContainerLookups)base.Lookups;

	protected override ManifestBase.AsycudaContainerLookups GetNewLookups() => new CGMAsycudaContainerLookups(this);

	#endregion
}
