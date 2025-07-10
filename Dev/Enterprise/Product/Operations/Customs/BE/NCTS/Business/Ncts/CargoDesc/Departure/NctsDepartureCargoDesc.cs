using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.BE.NCTS.Business;

public class NctsDepartureCargoDesc : EU.NCTS.Business.NctsDepartureCargoDesc, Integration.Customs.BE.IDepartureCargoDesc
{
	public NctsDepartureCargoDesc(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	[ChildEditable]
	public new EU.NCTS.Business.INctsPreviousDocumentCollection<NctsPreviousDocument> PreviousDocuments => (EU.NCTS.Business.INctsPreviousDocumentCollection<NctsPreviousDocument>)base.PreviousDocuments;

	[ChildEditable]
	public new EU.NCTS.Business.INctsAdditionalInfoCollection<NctsAdditionalInfo> AdditionalInfos => (EU.NCTS.Business.INctsAdditionalInfoCollection<NctsAdditionalInfo>)base.AdditionalInfos;

	protected override EU.NCTS.Business.INctsPreviousDocumentCollection<EU.NCTS.Business.NctsPreviousDocument> GetPreviousDocuments() => new EU.NCTS.Business.NctsPreviousDocumentCollection<NctsPreviousDocument>(this);

	protected override EU.NCTS.Business.INctsAdditionalInfoCollection<EU.NCTS.Business.NctsAdditionalInfo> GetNctsAdditionalInfoCollection() => new EU.NCTS.Business.NctsAdditionalInfoCollection<NctsAdditionalInfo>(this);

	protected override Type AdditionalInfoType => typeof(NctsAdditionalInfo);
	protected override Type PreviousDocumentType => typeof(NctsPreviousDocument);

	public new NctsDepartureCargoDescValidation Validation => (NctsDepartureCargoDescValidation)base.Validation;

	protected override EU.NCTS.Business.NctsDepartureCargoDescPhase5Validation GetNewPhase5Validation() => new NctsDepartureCargoDescValidation(this);

	protected override ZDecimal VATRateForEmptyTaxType => ZDecimal.Zero;
}
