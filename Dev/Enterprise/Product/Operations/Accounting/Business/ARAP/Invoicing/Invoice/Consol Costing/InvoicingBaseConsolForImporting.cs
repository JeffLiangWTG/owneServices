using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.ZArchitecture.Schema;
using AccGenericConsol = Enterprise.Accounting.Business.GenericConsol.GenericConsol;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	[TestedAsNonPersistentBusinessObject]
	public class InvoicingBaseConsolForImporting : AccGenericConsol
	{
		public InvoicingBaseConsolForImporting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		internal InvoicingBaseBulkConsolCostImporter Importer
		{
			get { return importer_cached; }
			set
			{
				importer_cached = value;
				ConsolCosts.Importer = importer_cached;
			}
		}
		InvoicingBaseBulkConsolCostImporter importer_cached;

		#region IsSelectedForImport

		ZBool fIsSelectedForImport;
		public ZBool IsSelectedForImport
		{
			get { return fIsSelectedForImport; }
			set
			{
				SetNonPersistentPropertyValue(IsSelectedForImportInfo, ref fIsSelectedForImport, value);
				foreach (InvoicingBaseConsolCostForImporting cost in ConsolCostsFilteredByViewingPermission)
				{
					cost.IsSelectedForImport = value;
				}
				ConsolCosts.RefreshBinding();
				ConsolCostsFilteredByViewingPermission.RefreshBinding();
			}
		}

		public ZPropertyInfo IsSelectedForImportInfo
		{
			get { return GetZPropertyInfo(nameof(IsSelectedForImport)); }
		}

		#endregion

		#region ConsolCosts

		InvoicingBaseConsolCostCollectionForImporting fConsolCosts;

		[ChildEditable(true)]
		public InvoicingBaseConsolCostCollectionForImporting ConsolCosts
		{
			get
			{
				if (fConsolCosts == null)
				{
					fConsolCosts = new InvoicingBaseConsolCostCollectionForImporting(Factory) { Importer = Importer };
					ZQuery filter = new ZQuery(JobConsolCostSchema.E6_ParentID, PK);
					filter.AddToFilter(JobConsolCostSchema.E6_AH_APInvoice, null);
					fConsolCosts.Load(filter);
					RegisterEditableChildObject(fConsolCosts);
				}
				return fConsolCosts;
			}
		}

		[ChildEditable(true)]
		public FilteredInvoicingBaseConsolCostCollectionForImportingView ConsolCostsFilteredByViewingPermission
		{
			get
			{
				if (fConsolCostsFilteredByViewingPermission == null && ConsolCosts != null)
				{
					fConsolCostsFilteredByViewingPermission = new FilteredInvoicingBaseConsolCostCollectionForImportingView(ConsolCosts);
					RegisterEditableChildObject(fConsolCostsFilteredByViewingPermission);
				}
				return fConsolCostsFilteredByViewingPermission;
			}
		}

		FilteredInvoicingBaseConsolCostCollectionForImportingView fConsolCostsFilteredByViewingPermission;

		public List<JobConsolCost> GetConsolCostsSelectedForImport()
		{
			List<JobConsolCost> result = new List<JobConsolCost>();
			foreach (InvoicingBaseConsolCostForImporting consolCost in ConsolCostsFilteredByViewingPermission)
			{
				if (consolCost.IsSelectedForImport)
				{
					result.Add(consolCost);
				}
			}
			return result;
		}

		#endregion

		#region ConsolCostsAdditionalFilter

		ZQuery fConsolCostsAdditionalFilter;
		public ZQuery ConsolCostsAdditionalFilter
		{
			get
			{
				if (fConsolCostsAdditionalFilter == null)
				{
					fConsolCostsAdditionalFilter = new ZQuery(JobConsolCostSchema.E6_ParentID, PK);
					fConsolCostsAdditionalFilter.AddToFilter(JobConsolCostSchema.E6_AH_APInvoice, null);
				}
				return fConsolCostsAdditionalFilter;
			}
			set
			{
				fConsolCostsAdditionalFilter = value;
				value.AddToFilter(JobConsolCostSchema.E6_ParentID, PK);
				fConsolCosts.Load(value);
			}
		}

		#endregion
	}
}
