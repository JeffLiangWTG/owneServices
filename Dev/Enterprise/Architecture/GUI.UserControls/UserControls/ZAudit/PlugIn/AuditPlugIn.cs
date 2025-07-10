using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.AuditDataServices.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.ZArchitecture.GUI.ZAudit.PlugIn
{
	public class AuditPlugIn : ZPlugIn
	{
		public AuditPlugIn(Audit hostBusinessEntity)
			: base(hostBusinessEntity)
		{
			if (hostBusinessEntity == null)
			{
				throw new ArgumentNullException(nameof(hostBusinessEntity));
			}

			auditUserControlObject = new ZAuditUserControl();
			this.TabPage.ShouldBeReadOnlyInViewMode = false;
		}

		public AuditPlugIn(ResourceStringData auditUnavailableMessage)
			: base(null)
		{
			if (auditUnavailableMessage == null)
			{
				throw new ArgumentNullException(nameof(auditUnavailableMessage));
			}

			auditUserControlObject = new ZAuditUserControl(auditUnavailableMessage);
		}

		readonly ZAuditUserControl auditUserControlObject;

		public override string Name
		{
			get { return Enterprise.ZArchitecture.GUI.UserControls.Res.GetString("D82AE799-183F-46BD-B481-8ABCE24E1E64", "Audit"); }
		}

		protected override ZBool HasUserControl
		{
			get { return auditUserControlObject != null; }
		}

		protected override System.Windows.Forms.Control GetNewUserControl()
		{
			return auditUserControlObject;
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return (Audit)base.HostBusinessEntity;
		}

		protected override ZTabPagePlugIn GetTabPage()
		{
			return new ZAutoSizedTabPagePlugIn(this);
		}

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.AlwaysAllow; }
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (auditUserControlObject != null)
				{
					auditUserControlObject.Dispose();
				}
			}

			base.Dispose(disposing);
		}
	}
}
