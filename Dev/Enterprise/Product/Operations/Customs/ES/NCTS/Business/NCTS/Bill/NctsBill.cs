using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.ES.NCTS.Business;

public class NctsBill : EU.NCTS.Business.NctsBill
{
	public NctsBill(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new NctsHeader Header => (NctsHeader)base.Header;

	[ResourceStringData("F7323C60-A867-40ED-901D-13D5AB8A00CE", Caption = "Reference Number / UCR", FullDescription = "Indicate the Reference Number / Unique Consignment Reference (UCR)", MediumCaption = "Ref. No. / UCR", ShortCaption = "Ref. No. / UCR")]
	public override ZString B0_ReferenceID { get => base.B0_ReferenceID; set => base.B0_ReferenceID = value; }

	[LightValidationTestExempt]
	public override ZGuid B0_BH { get => base.B0_BH; set => base.B0_BH = value; }

	public override ZDecimal B0_Weight
	{
		get => base.B0_Weight;
		set
		{
			var oldValue = base.B0_Weight;
			base.B0_Weight = value;

			if (!IsCopying && oldValue != value)
			{
				Header.ModifyReleaseStatusInPredeclaration();
			}
		}
	}

	[ChildEditable]
	public new EU.NCTS.Business.INctsDepartureCargoDescCollection<NctsDepartureCargoDesc> GoodsItems
		=> (EU.NCTS.Business.INctsDepartureCargoDescCollection<NctsDepartureCargoDesc>)base.GoodsItems;

	protected override EU.NCTS.Business.INctsDepartureCargoDescCollection<EU.NCTS.Business.NctsDepartureCargoDesc> GetNewGoodsItems()
		=> new EU.NCTS.Business.NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>(this);

	[ChildEditable]
	public new EU.NCTS.Business.INctsArrivalCargoDescCollection<NctsArrivalCargoDesc> ArrivalGoodsItems
		=> (EU.NCTS.Business.INctsArrivalCargoDescCollection<NctsArrivalCargoDesc>)base.ArrivalGoodsItems;

	protected override EU.NCTS.Business.INctsArrivalCargoDescCollection<EU.NCTS.Business.NctsArrivalCargoDesc> GetNewArrivalGoodsItems()
		=> new NctsArrivalCargoDescCollection(this);

	public new EU.Business.Declaration.ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> CusSupplyChainActorReferences
		=> (EU.Business.Declaration.ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>)base.CusSupplyChainActorReferences;

	protected override EU.Business.Declaration.ICusSupplyChainActorReferenceCollection<EU.NCTS.Business.CusSupplyChainActorReference> GetCusSupplyChainActorReferencesCore()
		=> new EU.Business.Declaration.CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>(this);

	protected override Type SupplyChainActorType => typeof(CusSupplyChainActorReference);

	public new EU.NCTS.Business.IArrivalCusTransportMeansCollection<ArrivalCusTransportMeans> ArrivalTransportInfos
		=> (EU.NCTS.Business.IArrivalCusTransportMeansCollection<ArrivalCusTransportMeans>)base.ArrivalTransportInfos;

	protected override EU.NCTS.Business.IArrivalCusTransportMeansCollection<EU.NCTS.Business.ArrivalCusTransportMeans> GetNewArrivalTransportInfos()
		=> new EU.NCTS.Business.ArrivalCusTransportMeansCollection<ArrivalCusTransportMeans>(this);

	public new EU.NCTS.Business.INctsSupportingDocumentCollection<NctsSupportingDocument> SupportingDocuments
		=> (EU.NCTS.Business.INctsSupportingDocumentCollection<NctsSupportingDocument>)base.SupportingDocuments;

	protected override EU.NCTS.Business.INctsSupportingDocumentCollection<EU.NCTS.Business.NctsSupportingDocument> GetSupportingDocuments()
		=> new EU.NCTS.Business.NctsSupportingDocumentCollection<NctsSupportingDocument>(this);

	public new EU.NCTS.Business.INctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument> AdditionalDocuments
		=> (EU.NCTS.Business.INctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument>)base.AdditionalDocuments;

	protected override EU.NCTS.Business.INctsBillAdditionalDocumentCollection<EU.NCTS.Business.NctsBillAdditionalDocument> GetAdditionalDocuments()
		=> new EU.NCTS.Business.NctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument>(this);

	public new EU.NCTS.Business.ICommonPreviousDocumentCollection<CommonPreviousDocument> PreviousDocuments
		=> (EU.NCTS.Business.ICommonPreviousDocumentCollection<CommonPreviousDocument>)base.PreviousDocuments;

	protected override EU.NCTS.Business.ICommonPreviousDocumentCollection<EU.NCTS.Business.CommonPreviousDocument> GetPreviousDocuments()
		=> new EU.NCTS.Business.CommonPreviousDocumentCollection<CommonPreviousDocument>(this);

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		var result = base.GetCusSupportingInfoTypes();
		result[CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(NctsSupportingDocument);
		result[CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(NctsBillAdditionalDocument);
		result[CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(CommonPreviousDocument);
		return result;
	}

	protected override EU.NCTS.Business.INctsCustomsEntryIntegrator GetCustomsEntryIntegratorCore() => new NctsBillCustomsEntryIntegrator(this);

	internal ZDecimal GoodsItemTotalGrossMassFromBill => GoodsItems.Sum(item => item.BY_GrossWeight);

	public override void Delete()
	{
		if (IsInDatabase)
		{
			Header.ShouldResetGoodsItemNumbers = !Header.UpdatePreDeclaration;
		}
		base.Delete();
	}
}
