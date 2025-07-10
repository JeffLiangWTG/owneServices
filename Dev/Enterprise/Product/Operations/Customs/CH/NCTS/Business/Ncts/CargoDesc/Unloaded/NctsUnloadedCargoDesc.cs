using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsUnloadedCargoDesc : EU.NCTS.Business.NctsUnloadedCargoDesc
{
	public NctsUnloadedCargoDesc(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new NctsHeader Header => (NctsHeader)base.Header;

	public new NctsUnloadedCargoDescValidation Validation => (NctsUnloadedCargoDescValidation)base.Validation;
	protected override CusInBondCargoDescValidation GetNewValidation() => new NctsUnloadedCargoDescValidation(this);
	protected override CusInBondCargoDescLookups GetNewLookups() => new NctsUnloadedCargoDescLookups(this);

	public new EU.NCTS.Business.NctsAdditionalInfoCollection<NctsAdditionalInfo> AdditionalInfos => (EU.NCTS.Business.NctsAdditionalInfoCollection<NctsAdditionalInfo>)base.AdditionalInfos;
	protected override EU.NCTS.Business.INctsAdditionalInfoCollection<EU.NCTS.Business.NctsAdditionalInfo> GetNctsAdditionalInfoCollection() => new EU.NCTS.Business.NctsAdditionalInfoCollection<NctsAdditionalInfo>(this);
	protected override Type AdditionalInfoType => typeof(NctsAdditionalInfo);

	protected override TariffFormatter GetNewTariffFormatter() => new TariffFormatterCH();
}
