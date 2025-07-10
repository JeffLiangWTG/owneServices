using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.GUI;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.Module
{
	/// <summary>
	/// Module Controller for ZReport.
	/// </summary>
	public abstract class ZReportModule : ZEmbeddedModule
	{
		public ZReportModule()
		{
			if (ID.ToString().Length > 17)
			{
				throw new Exception("ModuleID: " + ID.ToString() + " for ZReportModule must be less than 18 characters, as 'Rep' is prefixed to it and saved into the SU_BusinessContext Field");
			}
		}

		protected override Control GetNewEmbeddedControl()
		{
			ReportCommandCollection collection = new ReportCommandCollection(new BusinessObjectFactory(), BusinessContext);
			collection.SetReadOnlyIncludingChildren(true);
			collection.LoadApplicableReports();
			ReportUserControl reportControl = NewReportUserControl(collection);
			reportControl.Dock = DockStyle.Fill;

			return reportControl;
		}

		protected virtual ReportUserControl NewReportUserControl(ReportCommandCollection collection)
		{
			return new ReportUserControl(ID, collection, this.SecurityCheckpoint);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.AlwaysAllow; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		public string BusinessContext
		{
			get { return (NoResString)"Rep" + ID.ToString(); }
		}
	}
}
