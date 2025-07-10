using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class OrgSupplierPartCollection : Customs.Business.OrgSupplierPartCollection
	{
		public OrgSupplierPartCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgSupplierPartCollection(BusinessObjectFactory factory, JobComInvoiceLine invoiceLine, bool isExport)
			: base(factory, invoiceLine, isExport)
		{
		}

		public new OrgSupplierPart AddNew()
		{
			return (OrgSupplierPart)base.AddNew();
		}

		public new OrgSupplierPart this[int index] => (OrgSupplierPart)Elements[index];

		protected override void AddPivotWithAdditionalLineDetailsCore(Customs.Business.OrgSupplierPart basePart, BaseJobComInvoiceLine baseInvoiceLine)
		{
			var part = (OrgSupplierPart)basePart;
			var invoiceLine = (JobComInvoiceLine)baseInvoiceLine;
			if (part != null && invoiceLine != null && !invoiceLine.JI_Tariff.IsEmpty)
			{
				var pivot = part.PivotsForBinding.AddNew();

				CopyValuesFromInvoiceLine(pivot, invoiceLine);
				CopyAdditionalInformationsFromInvoiceLine(pivot, invoiceLine);
			}
		}

		void CopyAdditionalInformationsFromInvoiceLine(CusClassPartPivot pivot, JobComInvoiceLine invoiceLine)
		{
			pivot.CNC_NameOfGoods = invoiceLine.JI_NameOfGoods;
			pivot.CNC_GoodsSpecModel = invoiceLine.XC_GoodsSpecModel;
		}

		void CopyValuesFromInvoiceLine(CusClassPartPivot pivot, JobComInvoiceLine invoiceLine)
		{
			var childType = invoiceLine.GetPartPivotType();
			pivot.CI_ChildType = childType;
			pivot.CI_TariffNum = invoiceLine.JI_Tariff.Left(10);

			pivot.CI_RN_NKCountryOfOrigin = invoiceLine.JI_CountryOfOrigin;
			pivot.CI_RN_NKCountryOfExport = invoiceLine.JI_RN_NKCountryOfExport;

			if (pivot.IsExportClassification)
			{
				pivot.CNC_OriginDistrict = invoiceLine.JI_OriginDistrict;
				pivot.CNC_OriginRegion = invoiceLine.JI_OriginRegion;
			}

			if (pivot.IsImportClassification)
			{
				pivot.CI_RW_NKOriginState = invoiceLine.JI_StateOrRegionOfOrigin;
				pivot.CNC_OriginState = invoiceLine.JI_CIQOriginState;
				pivot.CNC_DestinationDistrict = invoiceLine.JI_DestinationDistrict;
				pivot.CNC_DestinationRegion = invoiceLine.JI_DestinationRegion;
			}
			pivot.CNC_CIQTariff = invoiceLine.JI_CIQTariff;

			if (!invoiceLine.CargoAttributesAsString.IsEmpty)
			{
				invoiceLine.CargoAttributes.Cast<CargoAttribute>().ForEach(att => pivot.CargoAttributes.AddNew(att.CY_Code));
			}

			pivot.CNC_OA_ManufacturerAddress = invoiceLine.JI_OA_ManufacturerAddress;
			pivot.CI_NDescription = invoiceLine.JI_NDescription;
			pivot.CNC_Model = invoiceLine.JI_Model;
			pivot.CNC_Brand = invoiceLine.JI_BrandName;
			pivot.CNC_EndUse = invoiceLine.JI_CIQEndUse;

			pivot.CIQIngredient = invoiceLine.CIQIngredient;

			pivot.CNC_UNPackageMarking = invoiceLine.JI_PackageTypeOfUNDG.Left(pivot.CNC_UNPackageMarkingInfo.MaxLength);
			pivot.CNC_QualityGuaranteePeriod = invoiceLine.JI_CIQQualityGuaranteePeriod;
			pivot.CNC_NonDangerousChemicalFlag = invoiceLine.JI_NonDangerousChemicalFlag;
			pivot.CNC_TradeUnitQty = invoiceLine.JI_TradeUnitQty;

			if (CNCustomsDataRegistry.Instance.DefaultTradeUnitPriceOnProduct.Value)
			{
				pivot.CNC_TradeUnitPrice = invoiceLine.TradeUnitPrice;
				pivot.CNC_RX_NKTradeUnitPriceCurrency = invoiceLine.JI_RX_NKLinePriceCurr;
			}
		}
	}
}
