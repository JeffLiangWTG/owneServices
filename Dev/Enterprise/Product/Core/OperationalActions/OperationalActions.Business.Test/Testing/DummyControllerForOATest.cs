using System;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestExcludeZControllersAllHaveSecurityCheckpoints]
	public class DummyControllerForOATest : DummyController
	{
		public override Type TypeOfTopLevelBusinessObject
		{
			get
			{
				return typeof(DummyBusinessObjectWithDocumentSupport);
			}
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get
			{
				return Env.Security.MaintainShipmentEdit;
			}
		}
	}
}
