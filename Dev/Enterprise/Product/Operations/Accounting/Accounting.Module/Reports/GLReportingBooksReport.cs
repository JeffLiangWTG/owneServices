using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Module;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class GLReportingBooksReport : ZReportModule
	{
		public GLReportingBooksReport()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.GLReportingBooksReport; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.GLReportingBooksReport; }
		}

		protected override Control GetNewEmbeddedControl()
		{
			var companyPK = GlbCompany.CurrentCompany.PK;
			var factory = new BusinessObjectFactory();
			var lastProcessedDate =
				AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.GetValueWithoutFallback(companyPK.ToGuid(), Guid.Empty, Guid.Empty);

			var sqlQuery = new ZDBOnlyQuery(typeof(AccPeriodManagement));
			sqlQuery.AddToFilter(AccPeriodManagementSchema.AM_GC_Company, GlbCompany.CurrentCompany.PK);
			sqlQuery.OrderBy = AccPeriodManagementSchema.Constants.AM_StartDate;

			var period = factory.LoadTop1<AccPeriodManagement>(sqlQuery);
			if (period == null || lastProcessedDate == DateTime.MinValue || period.AM_StartDate < lastProcessedDate || !Business.AccountingUtils.IsEDWEnabled())
			{
				var label = new ZLabel();
				label.Text = period == null || lastProcessedDate == DateTime.MinValue || period.AM_StartDate < lastProcessedDate
					? Res.GetString("FD6857B2-23C3-48B6-B793-3617425162F9", "Reporting book reports cannot be generated as journal entries for backlog accounting transactions have not been completely processed. Please check the Journal Entries Last Processed Date registry.")
					: Res.GetString("9F4A6E4F-5EA2-4B4C-BE76-BFC57EACE462", "Reporting book reports cannot be generated as the EET service task has not been run. Please check the Data Warehouse Server via System->BI->Data Warehouse Server.");
				label.TextAlign = ContentAlignment.MiddleCenter;
				return label;
			}
			return base.GetNewEmbeddedControl();
		}
	}
}
