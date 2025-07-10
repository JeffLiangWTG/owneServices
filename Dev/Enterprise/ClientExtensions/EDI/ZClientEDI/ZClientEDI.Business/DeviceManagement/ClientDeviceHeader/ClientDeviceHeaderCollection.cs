using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DeviceManagement.Business
{
	[ModuleID("ClientDevice")]
	public class ClientDeviceHeaderCollection : ActiveBusinessObjectCollection<ClientDeviceHeader>
	{
		public ClientDeviceHeaderCollection(BusinessObjectFactory factory)
			: base(factory, new ZQuery(DmgDeviceHeaderSchema.CDH_IsTemplate, false))
		{
		}

		public ClientDeviceHeaderCollection(LicenceDatabase licenceDatabase)
			: base(licenceDatabase.Factory, licenceDatabase, null, DmgDeviceHeaderSchema.CDH_EnterpriseCode)
		{
		}

		public ClientDeviceHeaderCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
		}

		protected override bool AllowNew
		{
			get { return Relationship.Master == null; }
		}

		protected override void SetDefaultsForNewElementCore(ClientDeviceHeader newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			var master = Relationship.Master as LicenceDatabase;
			if (master != null)
			{
				newElement.CDH_EnterpriseCode = master.EnterpriseCode;
				newElement.CDH_ServerCode = master.LD_ServerCode;
			}
		}
	}
}

