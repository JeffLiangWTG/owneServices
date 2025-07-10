using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using static Enterprise.Core.Constants.Customs.Universal;
using RefCusCodeListTypesEU = Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsBillAdditionalDocument : EU.NCTS.Business.NctsBillAdditionalDocument
{
	public NctsBillAdditionalDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	internal new NctsBill Parent => base.Parent as NctsBill;

	public new NctsHeader Header => Parent?.Header;

	public new NctsBillAdditionalDocumentLookups Lookups => (NctsBillAdditionalDocumentLookups)base.Lookups;

	protected override CusSupportingInfoLookups GetNewLookups() => new NctsBillAdditionalDocumentLookups(this);

	protected override ZString GetCodeTypeBySubTypeCore()
	{
		var isNationalTransit = Parent?.Header?.IsNationalTransitSwitzerland ?? false;

		return CSI_SubType.ToString() switch
		{
			AdditionalInfoSubTypeList.Codes.AdditionalReference => isNationalTransit ? ZString.Empty : RefCusCodeListTypesEU.Codes.Code_AR44N,
			AdditionalInfoSubTypeList.Codes.AdditionalInformation => isNationalTransit ? RefCusCodeListTypes.Codes.ExportAddDocAdditionalInformation : RefCusCodeListTypesEU.Codes.Code_AI44N,
			AdditionalInfoSubTypeList.Codes.TransportDocument => RefCusCodeListTypesEU.Codes.Code_TD44N,
			_ => ZString.Empty
		};
	}
}
