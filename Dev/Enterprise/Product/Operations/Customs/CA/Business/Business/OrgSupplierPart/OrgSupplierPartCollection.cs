using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
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

		public OrgSupplierPartCollection(BusinessObjectFactory factory, BaseJobComInvoiceLine invoiceLine, OrgHeader supplier, OrgHeader owner, bool isExport)
			: base(factory, invoiceLine, supplier, owner, isExport)
		{
		}

		public new OrgSupplierPart AddNew()
		{
			return (OrgSupplierPart)base.AddNew();
		}

		public new OrgSupplierPart this[int index]
		{
			get { return (OrgSupplierPart)Elements[index]; }
		}

		protected override void AddPivotWithAdditionalLineDetailsCore(Customs.Business.OrgSupplierPart basePart, BaseJobComInvoiceLine baseInvoiceLine)
		{
			var part = (OrgSupplierPart)basePart;
			var invoiceLine = (JobComInvoiceLine)baseInvoiceLine;
			if (part != null && invoiceLine != null && (invoiceLine.JI_CC.IsValid || !invoiceLine.JI_Tariff.IsEmpty))
			{
				var pivot = part.PivotsForBinding.AddNew();
				try
				{
					pivot.UpdatingPivotFromInvoiceLine = true;
					pivot.CI_ChildType = invoiceLine.GetPartPivotType();
					if (invoiceLine.JI_CC.IsValid)
					{
						pivot.CI_CC = invoiceLine.JI_CC;
					}
					else
					{
						pivot.CI_TariffNum = invoiceLine.JI_Tariff.Left(15);
					}
					if (invoiceLine.IsExport)
					{
						AddExportAdditionalDetails(invoiceLine, pivot);
					}
					else
					{
						AddImportAdditionalDetails(invoiceLine, pivot);
					}
				}
				finally
				{
					pivot.UpdatingPivotFromInvoiceLine = false;
				}
			}
		}

		void AddExportAdditionalDetails(JobComInvoiceLine invoiceLine, CusClassPartPivot pivot)
		{
			pivot.CCA_RN_NKOrigin = invoiceLine.JI_CountryOfOrigin;
			pivot.CCA_ProvinceOfOrigin = invoiceLine.JI_StateOrRegionOfOrigin.Left(2);
		}

		void AddImportAdditionalDetails(JobComInvoiceLine invoiceLine, CusClassPartPivot pivot)
		{
			#region From invoiceLine Addinfo

			pivot.CCA_ValueForDutyCode = invoiceLine.CA_ValueForDutyCode;
			pivot.CCA_TreatmentCode = invoiceLine.CA_TreatmentCode;
			pivot.CCA_99TariffCode = invoiceLine.CA_99TariffCode;
			pivot.CCA_AuthorityNumber = invoiceLine.CA_AuthorityNumber;
			pivot.CCA_TRSNumber = invoiceLine.CA_TRSNumber;
			pivot.CCA_RequirementID = invoiceLine.CA_RequirementID;
			pivot.CCA_RequirementVersion = invoiceLine.CA_RequirementVer;
			pivot.CCA_AirsCode = invoiceLine.CA_AirsCode;
			pivot.CCA_DestinationProvince = invoiceLine.CA_DestinationProvince;
			pivot.CCA_EndUse = invoiceLine.CA_EndUse;
			pivot.CCA_MiscID = invoiceLine.CA_MiscID;
			pivot.CCA_RN_NKCFIAOrigin = invoiceLine.CA_RN_NKCFIAOrigin;
			pivot.CCA_CFIAUSStateOfOrigin = invoiceLine.CA_CFIAUSStateOfOrigin;
			pivot.CCA_ImportReasonCode = invoiceLine.CA_ImportReasonCode;
			pivot.CCA_Model = invoiceLine.CA_Model;
			pivot.CCA_ModelNumber = invoiceLine.CA_ModelNumber;
			pivot.CCA_BrandName = invoiceLine.JI_BrandName.Left(pivot.CCA_BrandNameInfo.MaxLength);
			pivot.CCA_TypeSize = invoiceLine.CA_TypeSize;
			pivot.CCA_TIIN = invoiceLine.CA_TIIN;
			pivot.CCA_CompliantCompletion = invoiceLine.CA_CompliantCompletion;
			pivot.CCA_CompliantImportDateIndicator = invoiceLine.CA_CompliantImportDate;
			pivot.Details.CCA_AMMVPercentage = invoiceLine.CA_AMMVPercentage;
			pivot.Details.CCA_AMMVPerUnit = invoiceLine.CA_AMMVPerUnit;

			#endregion

			pivot.CCA_RN_NKOrigin = invoiceLine.JI_CountryOfOrigin;
			pivot.CCA_GSTStatusCode = invoiceLine.DutyAndTaxManager.GetExemptCode(DutyAndTaxTypes.Codes.GST);
			pivot.CCA_ETExemption = invoiceLine.DutyAndTaxManager.GetExemptCode(DutyAndTaxTypes.Codes.ExciseTax);
			pivot.CCA_ProvinceOfOrigin = invoiceLine.JI_StateOrRegionOfOrigin.Left(2);
			invoiceLine.CFIARegistrationNumbers.CloneElementsTo(pivot.CFIARegistrationNumbers);
			invoiceLine.SITTCertificationNumbers.CloneElementsTo(pivot.SITTCertificationNumbers);

			UpdateSIMADutiesFromInvoiceLine(invoiceLine, pivot);
		}

		void UpdateSIMADutiesFromInvoiceLine(JobComInvoiceLine invoiceLine, CusClassPartPivot pivot)
		{
			pivot.CCA_SIMADumpingNumber = invoiceLine.CA_SIMADumpingNum;

			foreach (var dutyInInvoiceLine in invoiceLine.DutiesAndTaxes.Where(x => DutyAndTaxTypes.IsSIMATaxCode(x.C1_TaxType)))
			{
				var shouldCreateNewDuty = false;
				var matchedDuty = pivot.DutiesAndTaxes.FirstOrDefault(x => x.C1_TaxType == dutyInInvoiceLine.C1_TaxType);
				if (matchedDuty == null)
				{
					shouldCreateNewDuty = true;
				}
				else if (!matchedDuty.C1_Override && dutyInInvoiceLine.C1_Override)
				{
					matchedDuty.Delete();
					shouldCreateNewDuty = true;
				}

				if (shouldCreateNewDuty)
				{
					var duty = pivot.DutiesAndTaxes.AddNew(dutyInInvoiceLine.C1_TaxType);
					duty.C1_ExemptCode = dutyInInvoiceLine.C1_ExemptCode;
					duty.C1_Override = dutyInInvoiceLine.C1_Override;
					duty.C1_RateType = dutyInInvoiceLine.C1_RateType;
					duty.C1_Rate = dutyInInvoiceLine.C1_Rate;
					duty.C1_UnitOfMeasure = dutyInInvoiceLine.C1_UnitOfMeasure;
					duty.C1_NormalValuePerUnit = dutyInInvoiceLine.C1_NormalValuePerUnit;
					duty.C1_NormalValueCurrency = dutyInInvoiceLine.C1_NormalValueCurrency;
					duty.C1_ForeignRate = dutyInInvoiceLine.C1_ForeignRate;
					duty.C1_ForeignCurrency = dutyInInvoiceLine.C1_ForeignCurrency;
				}
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var orgSupplierPart = (OrgSupplierPart)child;
			var unit = orgSupplierPart.OP_StockKeepingUnit;
			if (!unit.IsEmpty)
			{
				orgSupplierPart.OP_StockKeepingUnit = CustomsUnitOfMeasureList.ConvertCustomsUnitsToStockUnits(unit, Factory);
			}
		}
	}
}
