using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public abstract class OrgSupplierPartDataSave : MasterFiles.Business.OrgSupplierPartDataSave
	{
		protected OrgSupplierPartDataSave() { }

		protected override void MapCountrySpecificTariff(MasterFiles.Business.OrgSupplierPart bizo, string classType, PartsDataToSave record)
		{
			if (classType == "IMP")
			{
				MapCountrySpecificTariffCore(classType, record.PartClassification, out record.ImportTariff);
			}
			else if (classType == "EXP")
			{
				MapCountrySpecificTariffCore(classType, record.PartExportClassification, out record.ExportTariff);
			}
			else
			{
				record.ImportTariff = ZString.Empty;
				record.ExportTariff = ZString.Empty;
			}
		}

		void MapCountrySpecificTariffCore(string classType, ZString classificationCode, out ZString recordField)
		{
			ZQuery filter = new ZQuery(CusClassificationSchema.CC_ClassificationType, classType);
			filter.AddToFilter(CusClassificationSchema.CC_LookupCode, classificationCode);
			filter.AddToFilter(CusClassificationSchema.CC_IsActive, true);
			filter.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			BaseCusClassification cusClassification = Factory.LoadTop1<BaseCusClassification>(filter);
			if (cusClassification != null)
			{
				recordField = cusClassification.CC_TariffNum;
			}
			else
			{
				recordField = ZString.Empty;
			}
		}
	}
}
