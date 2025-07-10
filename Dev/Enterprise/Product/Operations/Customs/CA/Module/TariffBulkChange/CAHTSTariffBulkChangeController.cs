using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Module
{
	public class CAHTSTariffBulkChangeController : ZSingletonController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			CAHTSTariffBulkChange topTariffBulkChange = new CAHTSTariffBulkChange(factory);
			return new HTSTariffBulkChangeStartForm(topTariffBulkChange);
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.TariffBulkChange; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.CA.HTSTariffBulkChange; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CAHTSTariffBulkChange); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return null;
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}
	}
}
