using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.ApplicationLogging.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.ApplicationLogging
{
	internal class ApplicationLoggerController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID => ClientModuleRegistration.ApplicationLogger;

		public override ControllerID ID => ClientControllerRegistration.ApplicationLogger;

		public override Type TypeOfTopLevelBusinessObject => typeof(ApplicationLogger);

		protected override IZForm GetForm(IBusiness businessEntity) => new ApplicationLoggerForm((ApplicationLogger)businessEntity);

		protected override SecurityCheckpoint CheckPointForDelete => EDISecurityCheckpoints.ApplicationLoggingApplicationLoggerDelete;

		protected override SecurityCheckpoint CheckPointForEdit => EDISecurityCheckpoints.ApplicationLoggingApplicationLoggerEdit;

		protected override SecurityCheckpoint CheckPointForNew => EDISecurityCheckpoints.ApplicationLoggingApplicationLoggerNew;

		protected override SecurityCheckpoint CheckPointForView => EDISecurityCheckpoints.ApplicationLoggingApplicationLoggerView;
	}
}
