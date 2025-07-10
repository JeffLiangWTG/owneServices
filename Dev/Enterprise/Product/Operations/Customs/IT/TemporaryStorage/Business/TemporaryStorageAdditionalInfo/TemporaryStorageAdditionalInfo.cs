using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public sealed class TemporaryStorageAdditionalInfo : EU.Business.CusTempStorage.TemporaryStorageAdditionalInfo
{
	public TemporaryStorageAdditionalInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	[MaxLength(CusSupportingInfo.Schema.CSI_DescriptionMaxLength)]
	public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value; }

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
	}

	protected override CusSupportingInfoLookups GetNewLookups() => new TemporaryStorageAdditionalInfoLookups(this);

	protected override CusSupportingInfoValidation GetNewValidation() => new TemporaryStorageAdditionalInfoValidation(this);
}
