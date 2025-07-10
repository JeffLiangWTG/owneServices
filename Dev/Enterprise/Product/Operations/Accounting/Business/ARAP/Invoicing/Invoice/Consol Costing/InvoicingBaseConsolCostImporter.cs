using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class InvoicingBaseConsolCostImporter : NonPersistentBusinessObject, IObsoleteValidation
	{
		public InvoicingBaseConsolCostImporter(BusinessObjectFactory factory, JobConsolCost originatingCost, InvoicingBase parentInvoicingBase)
			: base(factory)
		{
			this.OriginatingCost = originatingCost;
			this.ParentInvoicingBase = parentInvoicingBase;
		}

		readonly JobConsolCost OriginatingCost;
		readonly InvoicingBase ParentInvoicingBase;

		ZQuery CostsFilter
		{
			get
			{
				var result = new ZQuery(JobConsolCostSchema.E6_GC, GlbCompany.CurrentCompany.PK);

				if (OriginatingCost.E6_ParentID.IsValid)
				{
					result.AddToFilter(JobConsolCostSchema.E6_ParentID, OriginatingCost.E6_ParentID);
				}

				if (OriginatingCost.E6_AC_ChargeCode.IsValid)
				{
					result.AddToFilter(JobConsolCostSchema.E6_AC_ChargeCode, OriginatingCost.E6_AC_ChargeCode);
				}

				result.AddToFilter(JobConsolCostSchema.E6_AH_APInvoice, null);

				var toExclude = new List<ZGuid>();
				foreach (JobConsolCost cost in ParentInvoicingBase.ConsolCosting.ConsolCosts)
				{
					if (cost.RelatedConsolCostPK.IsValid &&
						(!OriginatingCost.E6_ParentID.IsValid || OriginatingCost.E6_ParentID == cost.E6_ParentID) &&
						(!OriginatingCost.E6_AC_ChargeCode.IsValid || OriginatingCost.E6_AC_ChargeCode == cost.E6_AC_ChargeCode))
					{
						toExclude.Add(cost.RelatedConsolCostPK);
					}
				}
				if (toExclude.Count > 0)
				{
					result.AddToFilter(JobConsolCostSchema.PK, SQLComparisonOperator.NotEqual, toExclude.ToArray());
				}

				var costNotImportedIntoTheINIFilter = new ZDBOnlyQuery(typeof(JobConsolCost));
				var costAttribFilter = new ZDBOnlySubQuery(typeof(JobConsolCostAttrib), JobConsolCostAttribSchema.E6A_E6_JobConsolCost, true);
				costAttribFilter.AddToFilter(JobConsolCostAttribSchema.E6A_Name, JobChargeAttribTypeList.Codes.LinkedToIncompleteInvoice);
				costNotImportedIntoTheINIFilter.AddSubQuery(costAttribFilter, JoinCondition.And);
				result.AddToFilter(costNotImportedIntoTheINIFilter);

				return result;
			}
		}

		InvoicingBaseConsolCostCollectionForImporting fCostsCollection;
		public InvoicingBaseConsolCostCollectionForImporting CostsCollection
		{
			get
			{
				if (fCostsCollection == null)
				{
					fCostsCollection = new InvoicingBaseConsolCostCollectionForImporting(Factory);
					fCostsCollection.Load(CostsFilter);
					fCostsCollection.SetReadOnlyIncludingChildren(true);
				}
				return fCostsCollection;
			}
		}

		public void ImportCostsIntoCosting(BusinessObject[] costsToImport)
		{
			var consolCostImporter = ObjectFactory.Get<IConsolCostImporter>();
			consolCostImporter.ImportCostIntoCosting(ParentInvoicingBase, (JobConsolCost)costsToImport[0], OriginatingCost, syncParentInfo: false);
			consolCostImporter.ImportCostsToCollection(ParentInvoicingBase.ConsolCosting.ConsolCosts, costsToImport.Skip(1).Cast<JobConsolCost>(), syncParentInfo: false);
		}
	}
}
