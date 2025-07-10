using System;
using System.Windows.Forms;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.AuditDataServices.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI
{
	partial class ZAuditLogsForm : ZChildForm, IZAuditLogsForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public ZAuditLogsForm()
		{
			InitializeComponent();
		}

		public ZAuditLogsForm(IDataVersionLoggingSupported auditParentBizObj) : base(ZAuditLogsForm.GetAudit(auditParentBizObj))
		{
			InitializeComponent();
			if (DataSource != null)
			{
				IsAuditServerValid = true;
			}

			AuditParentBizObj = auditParentBizObj;
		}

		public readonly IDataVersionLoggingSupported AuditParentBizObj;

		public bool IsAuditServerValid { get; private set; }

		static IBusiness GetAudit(IDataVersionLoggingSupported auditParentBizObj)
		{
#if DEBUG
			BiServers.ClearBiServersCache();
#endif
			var auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(Db.Connection);
			if (!AuditServerValidator.AuditHasValidServerHost(auditServer))
			{
				return null;
			}
			return new Audit(auditParentBizObj as BusinessObject, auditServer);
		}

		void DataAuditLogsForm_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)Keys.Escape)
			{
				BeginInvoke(new Action(() => { Close(); }));
			}
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}
	}
}
