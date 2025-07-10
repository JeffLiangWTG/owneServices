using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.MasterFiles;
using EuMf = Enterprise.Customs.EU.Business.MasterFiles;
using MF = Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business
{
	public class GBOrgSupplierPartDataLoad : OrgSupplierPartDataLoad, Integration.Customs.GB.IGBOrgSupplierPartDataLoad
	{
		protected override IEnumerable<string> GetFieldNames()
		{
			return new List<string>() { GbPartsDataToLoad.Schema.ECSUPPLEMENT1, GbPartsDataToLoad.Schema.ECSUPPLEMENT2, GbPartsDataToLoad.Schema.ECSUPPLEMENT, GbPartsDataToLoad.Schema.CPC, GbPartsDataToLoad.Schema.THIRDQTY };
		}

		protected override MF.PartsDataToLoad GetPartsDataToLoad()
		{
			return new GbPartsDataToLoad();
		}

		protected override void UnlinkPreviousExistingClassificationsFromProduct(MF.PartsDataToLoad partData, MF.OrgSupplierPart product)
		{
			var needsSave = false;
			var customsProduct = product as EuMf.OrgSupplierPart;
			if (customsProduct != null)
			{
				foreach (var pivot in customsProduct.GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.UnitedKingdom))
				{
					if (pivot.Classification != null)
					{
						var classificationCode = pivot.Classification.CC_LookupCode;
						if (classificationCode != partData.PartClassification && classificationCode != partData.PartExportClassification)
						{
							pivot.Delete();
							needsSave = true;
						}
					}
				}
			}
			if (needsSave)
			{
				product.Factory.Save();
			}
		}

		protected override bool ClassificationLookupExistsOrIsNotRequired(ZString impLookup, ZString expLookup)
		{
			return true;
		}

		protected override ZGuid GetClassificationPK(string partClassificationCode, string classType)
		{
			return ZGuid.Empty;
		}

		protected override void LoadCountrySpecificDataForTariffNum(MF.OrgSupplierPart enterprisePart, ZString tariffNumber, ZString importOrExport, MF.PartsDataToLoad dataToLoad)
		{
			if (!tariffNumber.IsEmpty)
			{
				var gbPartsDataToLoad = dataToLoad as GbPartsDataToLoad;
				if (gbPartsDataToLoad != null)
				{
					var part = (EuMf.OrgSupplierPart)enterprisePart;
					foreach (var pivotToKill in (from CusClassPartPivot p in part.GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.UnitedKingdom) where new ZString[] { "BTH", importOrExport }.Contains(p.CI_ChildType) select p))
					{
						pivotToKill.Delete();
					}
					var cusClassPartPivot = part.PivotsForBinding.AddNew();
					cusClassPartPivot.CI_RN_NKCountry = Core.Constants.CountryCodes.UnitedKingdom;
					cusClassPartPivot.CI_TariffNum = tariffNumber.Left(cusClassPartPivot.CI_TariffNumInfo.MaxLength);
					cusClassPartPivot.CI_CC = ZGuid.Empty;
					cusClassPartPivot.CI_CPC = gbPartsDataToLoad.CPC.Left(cusClassPartPivot.CI_CPCInfo.MaxLength);

					var supplementCodes = (new[] { gbPartsDataToLoad.ECSUPPLEMENT1.Left(cusClassPartPivot.CI_Supplement1Info.MaxLength), gbPartsDataToLoad.ECSUPPLEMENT2.Left(cusClassPartPivot.CI_Supplement2Info.MaxLength) })
											.Union(gbPartsDataToLoad.ECSUPPLEMENT.Split(';'));
					PartDataLoadHelper.SetSupplementCodes(cusClassPartPivot, supplementCodes.ToArray());

					cusClassPartPivot.CI_ThirdQty = ZDecimal.ParseSafe(gbPartsDataToLoad.THIRDQTY, 0m);
					cusClassPartPivot.CI_RN_NKCountryOfOrigin = gbPartsDataToLoad.PartOrigin.Left(cusClassPartPivot.CI_RN_NKCountryOfOriginInfo.MaxLength);
					cusClassPartPivot.CI_UsageComment = gbPartsDataToLoad.UsageComment;
					cusClassPartPivot.CI_Description = gbPartsDataToLoad.ClassificationDescription;
				}
			}
		}

		protected override void SetTariffAndClassificationDetails(MF.OrgSupplierPart enterprisePart, MF.PartsDataToLoad dataToLoad)
		{
			if (!dataToLoad.ImportTariff.IsEmpty)
			{
				LoadCountrySpecificDataForTariffNum(enterprisePart, dataToLoad.ImportTariff, "IMP", dataToLoad);
			}

			if (!dataToLoad.ExportTariff.IsEmpty)
			{
				LoadCountrySpecificDataForTariffNum(enterprisePart, dataToLoad.ExportTariff, "EXP", dataToLoad);
			}

			if (!dataToLoad.PartFullDesc.IsEmpty)
			{
				AddDescriptionNote(enterprisePart, dataToLoad.PartFullDesc, "Full Product Description"); // Hard-coded constant
			}
		}
	}
}
