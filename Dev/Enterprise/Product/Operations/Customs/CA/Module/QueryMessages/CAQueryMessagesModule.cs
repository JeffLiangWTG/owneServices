using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Messaging.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public class CAQueryMessagesModule : EDIMessageModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.CA.CAQueryMessages; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.CA.CAQueryMessages);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CAQueryMessageCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CAQueryMessagesFilterBusinessObject();
		}

		protected override bool ShowRequeuingMenu
		{
			get { return false; }
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CAQueryMessagesFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.ImportBroker; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CAQueryMessages; }
		}
	}
}
