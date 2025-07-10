using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ES.NCTS.Business;

public class NctsArrivalCargoDesc(BusinessObjectFactory factory, DataRow row)
	: EU.NCTS.Business.NctsArrivalCargoDesc(factory, row)
	, Integration.Customs.ES.IArrivalCargoDesc
{
	public new NctsHeader Header => (NctsHeader)base.Header;

	public new EU.NCTS.Business.NctsPackageCollection<NctsPackage, NctsArrivalCargoDesc> Packages
		=> (NctsPackageCollection<NctsArrivalCargoDesc>)base.Packages;

	protected override EU.NCTS.Business.INctsPackageCollection<EU.NCTS.Business.NctsPackage, EU.NCTS.Business.NctsCommonCargoDesc> GetNctsPackageCollection()
		=> new NctsPackageCollection<NctsArrivalCargoDesc>(this);

	public new NctsArrivalCargoDescValidation Validation => (NctsArrivalCargoDescValidation)base.Validation;

	protected override Customs.Business.CusInBondCargoDescValidation GetNewValidation() => new NctsArrivalCargoDescValidation(this);

	[ChildEditable(true)]
	public new EU.NCTS.Business.INctsAdditionalInfoCollection<NctsAdditionalInfo> AdditionalInfos => (EU.NCTS.Business.INctsAdditionalInfoCollection<NctsAdditionalInfo>)base.AdditionalInfos;

	protected override EU.NCTS.Business.INctsAdditionalInfoCollection<EU.NCTS.Business.NctsAdditionalInfo> GetNctsAdditionalInfoCollection() => new EU.NCTS.Business.NctsAdditionalInfoCollection<NctsAdditionalInfo>(this);

	public new EU.NCTS.Business.INctsSupportingDocumentCollection<NctsSupportingDocument> SupportingDocuments => (EU.NCTS.Business.INctsSupportingDocumentCollection<NctsSupportingDocument>)base.SupportingDocuments;
	protected override EU.NCTS.Business.INctsSupportingDocumentCollection<EU.NCTS.Business.NctsSupportingDocument> GetNewNctsSupportingDocumentCollection() => new EU.NCTS.Business.NctsSupportingDocumentCollection<NctsSupportingDocument>(this);

	protected override Customs.Business.ICusInBondFeeCollection<EU.NCTS.Business.NctsCargoDescFee> GetNctsCargoDescFeeCollection() => new Customs.Business.CusInBondFeeCollection<NctsCargoDescFee>(this);

	protected override Type AdditionalInfoType => typeof(NctsAdditionalInfo);
	protected override Type SupportingDocumentType => typeof(NctsSupportingDocument);

	[LightValidationTestExempt]
	public override ZGuid BY_ParentID { get => base.BY_ParentID; set => base.BY_ParentID = value; }

	[LightValidationTestExempt]
	public override ZString BY_ParentTableCode { get => base.BY_ParentTableCode; set => base.BY_ParentTableCode = value; }

	public HashSet<ZString> ContainersSelected => Factory.GetValue(ref containersSelectedCached, () => IsPhase5 ?
		Packages.Cast<NctsPackage>()
		.SelectMany(x => x.ContainersSelected)
		.Distinct()
		.ToHashSet()
		:
		ContainersPivots.Cast<EU.NCTS.Business.NonPersistentDepartureContainerPivot>()
		.Where(container => container.ContainerSelected && !container.ContainerNumber.IsEmpty)
		.Select(container => container.ContainerNumber)
		.ToHashSet());
	CachedProperty<HashSet<ZString>> containersSelectedCached;

	protected override ZString TariffTypeCore => Constants.TariffTypes.Import;

	public new NctsUnloadedCargoDesc UnloadedGoodsItem => (NctsUnloadedCargoDesc)base.UnloadedGoodsItem;

	protected override Type NctsUnloadedCargoDescType => typeof(NctsUnloadedCargoDesc);

	[ReadOnlyMember(nameof(IsUnloadedStateReadOnly))]
	public override ZString BY_UnloadedState { get => base.BY_UnloadedState; set => base.BY_UnloadedState = value; }

	protected bool IsUnloadedStateReadOnly
	{
		get
		{
			var arrivalMovement = Header.ArrivalMovementHeader;
			return arrivalMovement.UnloadingDifferenceDataReadOnly
				|| arrivalMovement.IsUnloadingRemarksReadOnlySpain
				|| (StatusIsNew && EU.NCTS.Business.NctsHelper.UnloadedStateInitiallyNew(BY_UnloadedStateInfo));
		}
	}

	protected override ZString LiabilityFormattedTariffCore
	{
		get => base.LiabilityFormattedTariffCore;
		set
		{
			var valuedFormatted = TariffFormatter.Format(value);
			var oldValue = LiabilityFormattedTariffCore;
			LiabilityTariff = valuedFormatted;
			LiabilityFormattedTariffInfo.RefreshBinding(oldValue);
		}
	}
}
