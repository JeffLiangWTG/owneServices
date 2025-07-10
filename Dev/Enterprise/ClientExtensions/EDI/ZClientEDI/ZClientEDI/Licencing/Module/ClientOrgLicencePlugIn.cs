using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Licencing.Module;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.PlugIn;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI
{
	public class ClientOrgLicencePlugIn : ZPlugIn
	{
		public ClientOrgLicencePlugIn(IClientOrgLicenceProvider hostBusinessEntity)
			: this(hostBusinessEntity, false)
		{
		}

		public ClientOrgLicencePlugIn(IClientOrgLicenceProvider hostBusinessEntity, bool isPluginReadonly)
			: base(hostBusinessEntity)
		{
			this.isPluginReadonly = isPluginReadonly;
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			EDIOrgHeader org = HostBusinessEntity.LicenceOrganisation;

			if (isPluginReadonly)
			{
				org.SuspendValidation();
				if (org.LicCompany != null)
				{
					org.LicCompany.SetReadOnlyIncludingChildren(true);
				}
			}

			return org;
		}

		public override string Name
		{
			get { return Res.GetString("41172814-c864-4d57-adcd-a06c239eb81b", "License"); }
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Core; }
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override Control GetNewUserControl()
		{
			LicenceKeyBuilderControl result = new LicenceKeyBuilderControl(new LicenceViewController());
			result.ContextBusinessEntity = HostBusinessEntity;
			return result;
		}

		new IClientOrgLicenceProvider HostBusinessEntity
		{
			get { return (IClientOrgLicenceProvider)base.HostBusinessEntity; }
		}

		public override ZString PlugInNotDisplayedMessage
		{
			get
			{
				if (HostBusinessEntity.LicenceOrganisation == null)
				{
					return Res.GetString("66d01688-ca7a-4ad3-8a7f-8cda9eb2bd23", "You need to specify the Client Organization");
				}
				else
				{
					return base.PlugInNotDisplayedMessage;
				}
			}
		}

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			return base.ShouldPlugInGUIAndBusinessEntityBeCreatedCore() && HostBusinessEntity.LicenceOrganisation != null;
		}

		protected override bool RegisterPlugInBusinessEntityAsEditable
		{
			get { return !isPluginReadonly; }
		}

		readonly bool isPluginReadonly;
	}
}

