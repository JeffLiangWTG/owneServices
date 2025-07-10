using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Module
{
	public class AirCTOExportFilterBusinessObject : ExportCustomsManifestFilterBusinessObject, IAccountingFilterStripHolder
	{
		public AirCTOExportFilterBusinessObject(bool allowAir, bool allowSea)
			: base(allowAir, allowSea)
		{
		}

		public AirCTOExportFilterBusinessObject()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = base.GetModuleFiltersCore();

			AccountingFilterStrip.AddJobManagementFilters(filters, Env.Security.AUCustomsAirCTOExportJobInvoicing);

			return filters;
		}

		#region IAccountingFilterStripHolder Members

		IAccountingFilterStrip AccountingFilterStrip
		{
			get
			{
				return accountingFilterStrip_innerValue ?? (accountingFilterStrip_innerValue =
					(IAccountingFilterStrip)Activator.CreateInstance(ObjectFactory.GetType<IAccountingFilterStrip>(), this));
			}
		}
		IAccountingFilterStrip accountingFilterStrip_innerValue;

		ZQuery IAccountingFilterStripHolder.TopLevelBusinessObjectQuery(ZDBOnlySubQuery billingPKSubQuery)
		{
			ZDBOnlySubQuery linesQuery = new ZDBOnlySubQuery(typeof(ExportCustomsManifestLines), ExportCustomsManifestLinesSchema.EL_ED);
			linesQuery.AddSubQuery(billingPKSubQuery, JoinCondition.And);

			ZDBOnlyQuery headerQuery = new ZDBOnlyQuery(typeof(ExportCustomsManifestHeader));
			headerQuery.AddSubQuery(linesQuery, JoinCondition.And);

			return headerQuery;
		}

		ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => true;

		Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration => null;

		MultilingualString IAccountingFilterStripHolder.AmountFiltersCategoryNameOveride
		{
			get { return null; }
		}

		MultilingualString IAccountingFilterStripHolder.BillingFiltersCategoryNameOveride
		{
			get { return null; }
		}

		MultilingualString IAccountingFilterStripHolder.FilterNameSuffixInOtherCategories
		{
			get { return null; }
		}

		#endregion
	}
}
