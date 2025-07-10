using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.NL.Business.Declaration;

public class CusContainer : EU.Business.Declaration.CusContainer, Integration.Customs.NL.ICusContainer
{
	public CusContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new CusContainerLookups Lookups => (CusContainerLookups)base.Lookups;

	protected override Customs.Business.CusContainerLookups GetNewLookups() => new CusContainerLookups(this);
	public new CusContainerValidation Validation => (CusContainerValidation)base.Validation;

	protected override Customs.Business.CusContainerValidation GetNewValidation() => new CusContainerValidation(this);

	[List(nameof(Lookups) + "." + nameof(CusContainerLookups.SealPartyList))]
	[MaxLength(3)]
	[ResourceStringData("Enterprise.Customs.NL.Business.Declaration.CusContainer|SealPartyForBinding", Caption = "Sealed By")]
	public ZString SealPartyForBinding
	{
		get => JobContainer.JC_SealParty;
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

	public ZPropertyInfo SealPartyForBindingInfo => GetZPropertyInfo(nameof(SealPartyForBinding));

	[List(nameof(Lookups) + "." + nameof(CusContainerLookups.SealPartyList))]
	[MaxLength(3)]
	[ResourceStringData("Enterprise.Customs.NL.Business.Declaration.CusContainer|AdditionalSealPartyForBinding", Caption = "Sealed By", FullDescription = "2nd Sealed By")]
	public ZString AdditionalSealPartyForBinding
	{
		get => JobContainer.JC_AdditionalSealParty;
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

	public ZPropertyInfo AdditionalSealPartyForBindingInfo => GetZPropertyInfo(nameof(AdditionalSealPartyForBinding));
}
