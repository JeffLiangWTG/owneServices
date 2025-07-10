using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUOrgSupplierPartCollection : OrgSupplierPartCollection
	{
		public AUOrgSupplierPartCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public AUOrgSupplierPartCollection(BusinessObjectFactory factory, JobComInvoiceLine invoiceLine, bool isExport)
			: base(factory, invoiceLine, isExport)
		{
		}

		public new AUOrgSupplierPart AddNew()
		{
			return (AUOrgSupplierPart)base.AddNew();
		}

		public new AUOrgSupplierPart this[int index]
		{
			get { return (AUOrgSupplierPart)Elements[index]; }
		}

		protected override void AddPivotWithAdditionalLineDetailsCore(OrgSupplierPart part1, BaseJobComInvoiceLine invoiceLine1)
		{
			AUOrgSupplierPart part = (AUOrgSupplierPart)part1;
			JobComInvoiceLine invoiceLine = (JobComInvoiceLine)invoiceLine1;
			if (part != null && invoiceLine != null)
			{
				Classification cusClass = invoiceLine.Classification;
				if (cusClass != null)
				{
					if (cusClass.CC_ClassificationType == Enterprise.Customs.Business.BaseCusClassification.ClassificationType.IMP)
					{
						var pivot = part.PivotsForBinding.AddNew();
						pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
						pivot.CI_CC = cusClass.PK;
						var customsAddinfo = BaseAddInfo.AddinfoWithNonProductItemsRemoved(invoiceLine.JI_AddInfo, addinfoItemsThatMayBeAddedToProduct, AUAddInfo.SeperationCharacter);
						var pivotAddInfo = pivot.AddInfo;
						pivotAddInfo.LoadPropertiesFromString(customsAddinfo);

						var cusClassAddInfo = cusClass.AddInfo;
						if (pivotAddInfo.AddInfoLine.EqualsIgnoringCase(cusClassAddInfo.AddInfoLine))
						{
							pivotAddInfo.AddInfoLine = ZString.Empty;
						}
						if (pivotAddInfo.ZA_TreatmentCode_Hidden.EqualsIgnoringCase(cusClassAddInfo.ZA_TreatmentCode_Hidden))
						{
							pivotAddInfo.ZA_TreatmentCode_Hidden = ZString.Empty;
						}
						if (pivotAddInfo.ZA_InstrumentType_Hidden.EqualsIgnoringCase(cusClassAddInfo.ZA_InstrumentType_Hidden))
						{
							pivotAddInfo.ZA_InstrumentType_Hidden = ZString.Empty;
						}
						if (pivotAddInfo.ZA_InstrumentCode_Hidden.EqualsIgnoringCase(cusClassAddInfo.ZA_InstrumentCode_Hidden))
						{
							pivotAddInfo.ZA_InstrumentCode_Hidden = ZString.Empty;
						}
					}
					else if (cusClass.CC_ClassificationType == Enterprise.Customs.Business.BaseCusClassification.ClassificationType.EXP)
					{
						var pivot = part.PivotsForBinding.AddNew();
						pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
						pivot.CI_CC = cusClass.PK;
						AddInfoItemsAddedToProductWhenHTE(invoiceLine, pivot);
					}
				}
				else if (!invoiceLine.JI_Tariff.IsEmpty)
				{
					var pivot = part.PivotsForBinding.AddNew();
					pivot.CI_ChildType = invoiceLine.GetPartPivotType();
					pivot.CI_TariffNum = invoiceLine.JI_Tariff.Left(BaseCusClassPartPivot.Schema.CI_TariffNumMaxLength);

					AddInfoItemsAddedToProductWhenHTE(invoiceLine, pivot);
				}
			}
		}

		void AddInfoItemsAddedToProductWhenHTE(JobComInvoiceLine invoiceLine, CusClassPartPivot pivot)
		{
			if (invoiceLine.IsQuarantine)
			{
				var pivotAddinfo = pivot.AddInfo;
				var quarantineLine = invoiceLine.QuarantineExDocLine;
				if (pivotAddinfo != null && quarantineLine != null)
				{
					pivotAddinfo.ZA_AQISProduceType_Hidden = quarantineLine.QuarantineExDocHeader?.QH_ProduceType ?? ZString.Empty;
					pivotAddinfo.ZA_AQISProduct_Hidden = quarantineLine.QL_ProductType;
					pivotAddinfo.ZA_AQISCategoryCode_Hidden = quarantineLine.QL_Category;
					pivotAddinfo.ZA_AQISSupplementaryCode_Hidden = quarantineLine.QL_SupplimentaryCode;
					pivotAddinfo.ZA_AQISPackType_Hidden = quarantineLine.QL_PackType;
					pivotAddinfo.ZA_AQISPreservation_Hidden = quarantineLine.QL_PreservationType;
					pivotAddinfo.ZA_AQISCutCode_Hidden = quarantineLine.QL_CutCode;
				}
			}
		}

		readonly string[] addinfoItemsThatMayBeAddedToProduct = new string[]
		{
			AUAddInfoSchema.ZA_DCX.Name.Substring(3),
			AUAddInfoSchema.ZA_DXT.Name.Substring(3),
			AUAddInfoSchema.ZA_GSTE.Name.Substring(3),
			AUAddInfoSchema.ZA_ICN.Name.Substring(3),
			AUAddInfoSchema.ZA_ISC.Name.Substring(3),
			AUAddInfoSchema.ZA_LCTE.Name.Substring(3),
			AUAddInfoSchema.ZA_ORG.Name.Substring(3),
			AUAddInfoSchema.ZA_PRI.Name.Substring(3),
			AUAddInfoSchema.ZA_PRT.Name.Substring(3),
			AUAddInfoSchema.ZA_PST.Name.Substring(3),
			AUAddInfoSchema.ZA_MD2.Name.Substring(3),
			AUAddInfoSchema.ZA_TC2.Name.Substring(3),
			AUAddInfoSchema.ZA_CL2.Name.Substring(3),
			AUAddInfoSchema.ZA_TR2.Name.Substring(3),
			AUAddInfoSchema.ZA_TI2.Name.Substring(3),
			AUAddInfoSchema.ZA_UQ2.Name.Substring(3),
			AUAddInfoSchema.ZA_TAN.Name.Substring(3),
			AUAddInfoSchema.ZA_RNO.Name.Substring(3),
			AUAddInfoSchema.ZA_TRN.Name.Substring(3),
			AUAddInfoSchema.ZA_VAN.Name.Substring(3),
			AUAddInfoSchema.ZA_WETE.Name.Substring(3),
			AUAddInfoSchema.ZA_ELA.Name.Substring(3),
			AUAddInfoSchema.ZA_TCI.Name.Substring(3),
			AUAddInfoSchema.ZA_InstrumentCode_Hidden.Name.Substring(3),
			AUAddInfoSchema.ZA_InstrumentType_Hidden.Name.Substring(3),
			AUAddInfoSchema.ZA_TreatmentCode_Hidden.Name.Substring(3),
			AUAddInfoSchema.ZA_AQISPermitIds_Hidden.Name.Substring(3),
			AUAddInfoSchema.ZA_AQISEntityIds_Hidden.Name.Substring(3),
			AUAddInfoSchema.ZA_AQISProducerCodes_Hidden.Name.Substring(3),
			AUAddInfoSchema.ZA_AQISCommCodes_Hidden.Name.Substring(3),
			AUAddInfoSchema.ZA_AQISDocuments_Hidden.Name.Substring(3),
			AUAddInfoSchema.ZA_AQISPremIdProcessType_Hidden.Name.Substring(3)
		};
	}
}
