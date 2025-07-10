using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.CH.Business;

public class Restriction : CusSupportingInfo, ICusCodeDataTypeSupporter, IHugeSequenceNumberLine, IDocAddresses
{
	new public class Schema : AutoCusSupportingInfo.Schema
	{
		new public const int CSI_DescriptionMaxLength = 35;
		new public const int CSI_ReferenceNumberMaxLength = 35;
		new public const int CSI_ReferenceNumber2MaxLength = 17;
	}

	public Restriction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new IRestrictionParent Parent => (IRestrictionParent)base.Parent;

	#region ICusCodeDataTypeSupporter Members

	IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
	{
		return new Dictionary<ZString, Type>()
			{
				{ CusCodeDataTypeList.Codes.RestrictionAdditionalInformation, typeof(RestrictionAdditionalInformation) }
			};
	}

	IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
	{
		yield return new CusCodeDataTypeSupporterFetchStrategy(this);
	}

	#endregion

	protected override CusSupportingInfoLookups GetNewLookups() => new RestrictionLookups(this);

	public new RestrictionLookups Lookups => (RestrictionLookups)base.Lookups;

	protected override CusSupportingInfoValidation GetNewValidation() => Parent is JobComInvoiceLine ? new ExportRestrictionValidation(this) : new RestrictionValidation(this);

	[ResourceStringData("Enterprise.Customs.CH.Business.Restriction|CSI_LineNo", Caption = "Item Number")]
	public override ZInt CSI_LineNo
	{
		get => base.CSI_LineNo;
		set
		{
			var oldValue = CSI_LineNo;
			base.CSI_LineNo = value;

			if (oldValue != value && !IsCopying && Parent != null)
			{
				Parent.RestrictionsLineNumberGenerator.RecalculateWhenRenumbered(this, oldValue);
			}
		}
	}

	[ResourceStringData("Enterprise.Customs.CH.Business.Restriction|CSI_Code", Caption = "Code")]
	[MaxLength(3)]
	public override ZString CSI_Code
	{
		get => base.CSI_Code;
		set
		{
			var oldValue = CSI_Code;
			base.CSI_Code = value;
			if (!IsCopying && oldValue != CSI_Code)
			{
				Validation.ValidateCSI_Description();
				AdditionalInformations?.MarkAsNeedingValidation();
			}
		}
	}

	[ResourceStringData("Enterprise.Customs.CH.Business.Restriction|CSI_Description", Caption = "Permit Exception Reason")]
	[MaxLength(Schema.CSI_DescriptionMaxLength)]
	[List($"{nameof(Lookups)}.{nameof(RestrictionLookups.ExceptionReasonList)}")]
	public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value; }

	[ResourceStringData("Enterprise.Customs.CH.Business.Restriction|CSI_ReferenceNumber", Caption = "Permit Number")]
	[MaxLength(Schema.CSI_ReferenceNumberMaxLength)]
	public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

	[ResourceStringData("Enterprise.Customs.CH.Business.Restriction|CSI_ReferenceNumber2", Caption = "Identification")]
	[MaxLength(Schema.CSI_ReferenceNumber2MaxLength)]
	[ReadOnlyMember(nameof(CSI_ReferenceNumber2_ReadOnly))]
	public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }

	public ZBool CSI_ReferenceNumber2_ReadOnly => !OverrideIdentification;

	[ResourceStringData("Enterprise.Customs.CH.Business.Restriction|OverrideIdentification", Caption = "Override", ShortCaption = "Override")]
	[ReadOnlyMember(nameof(OverrideIdentification_ReadOnly))]
	public ZBool OverrideIdentification
	{
		get { return overrideIdentification; }
		set
		{
			var oldValue = overrideIdentification;
			overrideIdentification = value;

			if (oldValue != overrideIdentification && !overrideIdentification)
			{
				CSI_ReferenceNumber2 = ZString.Empty;
			}

			OverrideIdentificationInfo.RefreshBinding();
		}
	}
	ZBool overrideIdentification;

	public ZBool OverrideIdentification_ReadOnly => PermitOwnerDocAddress?.Address?.PK.IsEmpty ?? true;

	public ZPropertyInfo OverrideIdentificationInfo => GetZPropertyInfo(nameof(OverrideIdentification));

	[ResourceStringData("Enterprise.Customs.CH.Business.Restriction|PermitOwnerIdentification", Caption = "Identification")]
	public ZString PermitOwnerIdentification => Factory.GetCached(ref permitOwnerIdentification, () =>
	{
		return Parent is JobComInvoiceLine ? PermitOwnerDocAddress.Organisation.GetIdentificationNumberForCH() : ZString.Empty;
	});
	CachedProperty<ZString> permitOwnerIdentification;

	public ZPropertyInfo PermitOwnerIdentificationInfo => GetZPropertyInfo(nameof(PermitOwnerIdentification));

	public JobDocAddress PermitOwnerDocAddress
	{
		get
		{
			if (permitOwnerDocAddress == null || permitOwnerDocAddress.IsDeleted)
			{
				permitOwnerDocAddress = DocAddresses.FindOrCreateWithRequirement(PermitOwnerDocAddressRequirement);
				permitOwnerDocAddress.AdditionalValidation = PiggyBackedDocAddressValidation(permitOwnerDocAddress);
			}
			return permitOwnerDocAddress;
		}
	}
	JobDocAddress permitOwnerDocAddress;

	JobDocAddressRequirement PermitOwnerDocAddressRequirement
	{
		get
		{
			if (permitOwnerDocAddressRequirement == null)
			{
				permitOwnerDocAddressRequirement = GetDocAddressRequirement(DocAddressType.PermitOwner);
				DocAddressManager.AddRequirement(permitOwnerDocAddressRequirement);
			}
			return permitOwnerDocAddressRequirement;
		}
	}
	JobDocAddressRequirement permitOwnerDocAddressRequirement;

	ZInt ISequenceNumberLine<ZInt>.SequenceNumber { get => CSI_LineNo; set => CSI_LineNo = value; }
	ZGuid ISequenceNumberLine.FKToHeader => CSI_ParentID;

	internal HugeSequenceNumberGenerator AdditionalInformationLineNumberGenerator =>
		additionalInformationLineNumberGenerator ??= new HugeSequenceNumberGenerator(() =>
			new TypedEnumerable<IHugeSequenceNumberLine>(AdditionalInformations));

	HugeSequenceNumberGenerator additionalInformationLineNumberGenerator;

	[ChildEditable(true)]
	[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentID,
		CusCodeDataSchema.Constants.CY_ParentTableCode)]
	public RestrictionAdditionalInformationCollection AdditionalInformations
	{
		get
		{
			if (additionalInformations == null)
			{
				additionalInformations = new RestrictionAdditionalInformationCollection(this);
				RegisterEditableChildObject(additionalInformations);
				additionalInformations.Load();
			}

			return additionalInformations;
		}
	}

	RestrictionAdditionalInformationCollection additionalInformations;

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CSI_Type = Common.CH.CusSupportingInfoTypeList.Codes.Restriction;
	}

	public override ZGuid CSI_ParentID
	{
		get => base.CSI_ParentID;
		set
		{
			var oldParent = Parent;
			base.CSI_ParentID = value;
			if (oldParent?.PK != value && !IsCopying)
			{
				SetLineNoOnSettingParent(oldParent);
			}
		}
	}

	public override ZString CSI_ParentTableCode
	{
		get => base.CSI_ParentTableCode;
		set
		{
			var oldParent = Parent;
			base.CSI_ParentTableCode = value;
			if (oldParent?.TablePrefix != value && !IsCopying)
			{
				SetLineNoOnSettingParent(oldParent);
			}
		}
	}

	#region DocAddresses

	[ChildEditable(false)]
	public JobDocAddressDependentCollection DocAddresses
	{
		get
		{
			if (docAddresses == null)
			{
				docAddresses = new JobDocAddressDependentCollection(this);
				docAddresses.Load();
				RegisterEditableChildObject(docAddresses);
			}

			return docAddresses;
		}
	}
	JobDocAddressDependentCollection docAddresses;

	IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes => new DocAddressType[] { DocAddressType.PermitOwner };

	ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => null;

	SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress) => Env.Security.None;

	public JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType) => addressType switch
	{
		DocAddressType.PermitOwner => new JobDocAddressRequirement(DocAddressType.PermitOwner),
		_ => null
	};

	void IDocAddresses.DocAddressChanged(JobDocAddress docAddress) { }

	void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress) { }

	void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress) { }

	void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress) { }

	void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress) { }

	bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress) => false;

	OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType) => addressType switch
	{
		DocAddressType.PermitOwner => Lookups.PermitOwnerList,
		_ => null
	};

	public JobDocAddressManager DocAddressManager => docAddressManager ??= new JobDocAddressManager();
	JobDocAddressManager docAddressManager;

	public ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
	{
		if (addressToValidate.E2_AddressType == DocAddressTypes.Codes.PermitOwner)
		{
			return new PermitOwnerJobDocAddressValidation(addressToValidate, this);
		}
		return null;
	}

	#endregion DocAddresses

	public bool IsPermitNumberAllowed => Factory.GetCachedValue($"Enterprise.Customs.CH.Business.Restriction_{CSI_Code}_PermitNumberAllowed", () =>
	{
		var additionalCode = ZZRefCusCodeListCombined.Loader.LoadByCode(Factory, Core.Constants.CountryCodes.Switzerland, new ZString[] { UniversalReferenceConstants.RefCusCodeList.PassarTypes.Restrictions }, CSI_Code, ZDateTime.Today).FirstOrDefault();
		return additionalCode?.GetAttribute(UniversalReferenceConstants.RefCusCodeList.Attributes.PermitNumberAllowed).Equals(UniversalReferenceConstants.RefCusCodeList.AttributeValues.Yes) ?? true;
	});

	public bool IsPermitExceptionReasonAllowed => Factory.GetCachedValue($"Enterprise.Customs.CH.Business.Restriction_{CSI_Code}_PermitExceptionReasonAllowed", () =>
	{
		var additionalCode = ZZRefCusCodeListCombined.Loader.LoadByCode(Factory, Core.Constants.CountryCodes.Switzerland, new ZString[] { UniversalReferenceConstants.RefCusCodeList.PassarTypes.Restrictions }, CSI_Code, ZDateTime.Today).FirstOrDefault();
		return additionalCode?.GetAttribute(UniversalReferenceConstants.RefCusCodeList.Attributes.PermitExceptionReasonAllowed).Equals(UniversalReferenceConstants.RefCusCodeList.AttributeValues.Yes) ?? false;
	});

	public bool IsAdditionalInformation => Factory.GetCachedValue($"Enterprise.Customs.CH.Business.Restriction_{CSI_Code}_AdditionalInformation", () =>
	{
		var additionalCode = ZZRefCusCodeListCombined.Loader.LoadByCode(Factory, Core.Constants.CountryCodes.Switzerland, new ZString[] { UniversalReferenceConstants.RefCusCodeList.PassarTypes.Restrictions }, CSI_Code, ZDateTime.Today).FirstOrDefault();
		return additionalCode?.GetAttribute(UniversalReferenceConstants.RefCusCodeList.Attributes.AdditionalInformation).Equals(UniversalReferenceConstants.RefCusCodeList.AttributeValues.Yes) ?? false;
	});

	void SetLineNoOnSettingParent(IRestrictionParent oldParent)
	{
		oldParent?.RestrictionsLineNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
		Parent?.RestrictionsLineNumberGenerator.RecalculateWhenAdded(this);
	}

	public override void Delete()
	{
		if (Parent != null)
		{
			Parent.RestrictionsLineNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
		}

		using (AdditionalInformationLineNumberGenerator.GetLineNumberSuspender())
		{
			base.Delete();
		}
	}

	public override void OnLoaded()
	{
		base.OnLoaded();

		OverrideIdentification = !CSI_ReferenceNumber2.IsEmpty;
	}
}
