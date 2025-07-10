using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.GUI;
using Enterprise.Environment;
using Enterprise.Messaging.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Module
{
	public class K84ReportsController : EDIMessageController
	{
		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.CA.K84Reports; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.CA.K84Reports; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CAK84ReportsView; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new EDIMessageWithDocumentsForm((EDIMessage)businessEntity);
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(EDIMessage); }
		}
	}
}
