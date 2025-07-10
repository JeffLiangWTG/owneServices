using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business
{
	public partial class CusEntryLine : ICIQDataLine
	{
		public ZString Ingredient => RandomLine.CIQIngredient;

		public ZDateTime ExpiryDate => InvoiceLines.Cast<JobComInvoiceLine>().Min(line => line.JI_CIQExpiryDate);

		public ZInt QGPByDays => RandomLine.JI_CIQQualityGuaranteePeriod;

		public ZString Specification => RandomLine.JI_NDescription;

		public ZString Model => RandomLine.JI_Model;

		public ZString Brand => RandomLine.JI_BrandName;

		public OrgAddress Manufacturer => RandomLine.ManufacturerAddress;

		public ZString ManufacturerCIQ => Header.IsEntering ? ZString.Empty : Manufacturer?.Header?.GetCIQ() ?? ZString.Empty;

		public ZString ManufacturerName => Manufacturer?.GetChineseCompanyName() ?? ZString.Empty;

		public ZDateTime ManufactureDate => ProductionBatches.Where(batch => !batch.CY_Date.IsEmpty).MinOrDefault(batch => batch.CY_Date);

		public ZString BatchNumber => JoinAllProductBatchValues(batch => batch.CY_Data);

		public IEnumerable<ZString> CargoAttributes => RandomLine.CargoAttributes.AllCodes;

		public ZString EndUse => RandomLine.JI_CIQEndUse;

		public IEnumerable<ICIQProductQualification> ProductQualifications
		{
			get
			{
				if (fProductQualifications == null)
				{
					fProductQualifications = new EntryLineProductQualificationCollection(this);
					fProductQualifications.Load();
				}
				return fProductQualifications.Cast<ICIQProductQualification>();
			}
		}
		EntryLineProductQualificationCollection fProductQualifications;

		public void ClearCachedMergedData() => fProductQualifications = null;

		public ZBool NonDangerousChemical => RandomLine.JI_NonDangerousChemicalFlag;

		public ZString UNDGNumber => RandomLine.DangerousGoods?.UNDGSubstance?.DG_Code ?? ZString.Empty;

		public ZString UNDGClass
		{
			get
			{
				ZString result = ZString.Empty;
				var dg = RandomLine.DangerousGoods?.UNDGSubstance;
				if (dg != null)
				{
					var classParts = new List<ZString> { dg.DG_Class, dg.DG_SubLabel1, dg.DG_SubLabel2 }
					.Where(x => !x.IsEmpty)
					.Select(x => x.KeepChars("0123456789.")).ToArray();
					result = ZString.Join("+", classParts);
				}

				return result;
			}
		}

		public ZString UNDGPackageType => RandomLine.JI_PackageTypeOfUNDG;

		public ZString UNDGPackingGroup => RandomLine.DangerousGoods?.UNDGSubstance?.DG_PG ?? ZString.Empty;

		IEnumerable<ProductionBatch> ProductionBatches => InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(line => line.ProductionBatch.Cast<ProductionBatch>());

		string JoinAllProductBatchValues(Func<ProductionBatch, ZString> fieldFunc)
		{
			return ProductionBatches.Select(batch => fieldFunc(batch)).DistinctSortAndJoinForDisplay();
		}
	}
}
