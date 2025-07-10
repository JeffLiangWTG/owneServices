using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.BE.Business.Declaration;

public class CusContainer : EU.Business.Declaration.CusContainer, Integration.Customs.BE.ICusContainer
{
	public CusContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new CusContainerLookups Lookups => (CusContainerLookups)base.Lookups;

	protected override Customs.Business.CusContainerLookups GetNewLookups() => new CusContainerLookups(this);

	public new CusContainerValidation Validation => (CusContainerValidation)base.Validation;

	protected override Customs.Business.CusContainerValidation GetNewValidation() => new CusContainerValidation(this);

	[List(nameof(Lookups) + "." + nameof(CusContainerLookups.SealParty_List))]
	[MaxLength(3)]
	[ResourceStringData("Enterprise.Customs.BE.Business.Declaration.CusContainer|SealPartyForBinding", Caption = "Sealed By")]
	public ZString SealPartyForBinding
	{
		get { return JobContainer.JC_SealParty; }
		set
		{
			JobContainer.JC_SealParty = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateSealPartyForBinding();
			}
			SealPartyForBindingInfo.RefreshBinding();
		}
	}

	public ZPropertyInfo SealPartyForBindingInfo
	{
		get { return GetZPropertyInfo(nameof(SealPartyForBinding)); }
	}

	[List(nameof(Lookups) + "." + nameof(CusContainerLookups.SealParty_List))]
	[MaxLength(3)]
	[ResourceStringData("Enterprise.Customs.BE.Business.Declaration.CusContainer|AdditionalSealPartyForBinding", Caption = "Sealed By", FullDescription = "2nd Sealed By")]
	public ZString AdditionalSealPartyForBinding
	{
		get { return JobContainer.JC_AdditionalSealParty; }
		set
		{
			JobContainer.JC_AdditionalSealParty = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateAdditionalSealPartyForBinding();
			}
			AdditionalSealPartyForBindingInfo.RefreshBinding();
		}
	}

	public ZPropertyInfo AdditionalSealPartyForBindingInfo
	{
		get { return GetZPropertyInfo(nameof(AdditionalSealPartyForBinding)); }
	}
}
