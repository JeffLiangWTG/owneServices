using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.SeaManifest;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalPackLine = Enterprise.UniversalDataBuss.DataObjects.Universal.PackingLine;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCusSCAPivotDataObjectReader : CusSCAPivotDataObjectReader<CusSCAPivot>
	{
		public CMRCusSCAPivotDataObjectReader(ZInt lineNo, IColumnIndexer houseBill, HVLVShipmentDataObjectWrapper hvlvShipmentDataObjectWrapper, Shipment shipmentDataObject, UniversalPackLine data, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(lineNo, houseBill, hvlvShipmentDataObjectWrapper, data, logger, factory)
		{
			this.shipmentDataObject = shipmentDataObject;
		}
		readonly Shipment shipmentDataObject;

		protected override void PopulateCountrySpecificData(CusSCAPivot targetBO)
		{
			base.PopulateCountrySpecificData(targetBO);
			var packageBO = GetColumnIndexer(targetBO);
			SetValue(packageBO, CusSCAPivotSchema.CV_CB, houseBill.GetValue(CusSCAHouseSchema.CA_CB));
			if (dataObject.Volume.HasValue && !dataObject.VolumeUnit.GetCodeAsUpperCase().IsEmpty)
			{
				ZDecimal volumnInM3 = Core.Constants.Volume.ConvertSafe(dataObject.Volume.Value, dataObject.VolumeUnit.GetCodeAsUpperCase(), Core.Constants.Volume.CubicMetres);
				if (!volumnInM3.IsEmpty)
				{
					SetValue(packageBO, CusSCAPivotSchema.CV_Volume, volumnInM3);
				}
			}
			if (packageBO.GetValue(CusSCAPivotSchema.CV_PackageType).IsEmpty || !targetBO.Lookups.PackageTypes.ContainsCode(targetBO.CV_PackageType))
			{
				var convertedPackageType = SeaCargoUtilities.ConvertPkgUnitToCMRPackageType(dataObject.PackType.GetCodeAsUpperCase());
				if (!convertedPackageType.IsEmpty)
				{
					SetValue(packageBO, CusSCAPivotSchema.CV_PackageType, convertedPackageType);
				}
			}
			SetValue(packageBO, CusSCAPivotSchema.CV_FumigationCert, dataObject.RequiresFumigationCertificate);
			SetValue(packageBO, CusSCAPivotSchema.CV_PersonalEffects, dataObject.IsPersonalEffects);
			SetValue(packageBO, CusSCAPivotSchema.CV_Timber, dataObject.IsTimber);
			SetValue(packageBO, CusSCAPivotSchema.CV_PerishableGoods, dataObject.IsPerishable);
			SetValue(packageBO, CusSCAPivotSchema.CV_IsSAC, dataObject.IsHVLVClearance);
			SetValue(packageBO, CusSCAPivotSchema.CV_Flammable, dataObject.IsFlammable);

			if (hvlvConsolidatorShipmentWrapper != null)
			{
				var marksAndNumbers = ZString.Empty;
				if (shipmentDataObject.NoteCollection != null)
				{
					marksAndNumbers = shipmentDataObject.NoteCollection.Where(x => x.Description.GetValueOrDefault() == PredefinedNoteTypes.Instance.MarksAndNumbers.Description)
						.Select(x => x.NoteText.GetValueOrDefault()).FirstOrDefault();
				}
				if (marksAndNumbers.IsEmpty)
				{
					marksAndNumbers = shipmentDataObject.WayBillNumber.GetValueOrDefault();
				}
				if (!marksAndNumbers.IsEmpty)
				{
					SetValue(packageBO, CusSCAPivotSchema.CV_MarksAndNumbers, marksAndNumbers);
				}
				if (shipmentDataObject.GoodsDescription.HasValue && !shipmentDataObject.GoodsDescription.Value.IsEmpty)
				{
					SetValue(packageBO, CusSCAPivotSchema.CV_GoodsDescription, shipmentDataObject.GoodsDescription);
				}
				else
				{
					SetValue(packageBO, CusSCAPivotSchema.CV_GoodsDescription, dataObject.GoodsDescription);
				}
				if (!shipmentDataObject.GoodsValue.GetValueOrDefault().IsEmpty)
				{
					var currency = RefCurrency.LoadFromCurrencyCode(factory.BOFactory, shipmentDataObject.GoodsValueCurrency.GetCodeAsUpperCase());
					var goodsValueInLocalCurrency = new CurrencyConverterWithDataProvider(factory.BOFactory, hvlvConsolidatorShipmentWrapper).ConvertExact(new Money(shipmentDataObject.GoodsValue.GetValueOrDefault(), currency), JobDeclaration.GetLocalCurrency()).Amount;
					if (!goodsValueInLocalCurrency.IsEmpty)
					{
						var sacDecider = new SACDecider(factory.BOFactory, goodsValueInLocalCurrency, packageBO.GetValue(CusSCAPivotSchema.CV_GoodsDescription));
						SetValue(packageBO, CusSCAPivotSchema.CV_IsSAC, sacDecider.IsValidForSAC);
					}
				}
			}
		}

		protected override ZString GetContainerNumber()
		{
			var result = base.GetContainerNumber();
			if (result.IsEmpty && hvlvConsolidatorShipmentWrapper != null)
			{
				result = hvlvConsolidatorShipmentWrapper.ContainerNumber;
			}
			return result;
		}

		protected override void ReadUNDGCollection(CusSCAPivot targetBO)
		{
			var packageBO = GetColumnIndexer(targetBO);
			var dangerousGoods = dataObject.UNDGCollection != null && dataObject.UNDGCollection.Any() && dataObject.UNDGCollection.First().UNDGCode == (ZString?)Enterprise.Customs.Business.YesNoList.Codes.Yes;
			SetValue(packageBO, CusSCAPivotSchema.CV_HazardousGoods, dangerousGoods);
		}
	}
}
