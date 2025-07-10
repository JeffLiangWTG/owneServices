using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.ApplicationLogging.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.ApplicationLogging
{
	internal class ApplicationActiveLoggerController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID => ClientModuleRegistration.ApplicationActiveLogger;

		public override ControllerID ID => ClientControllerRegistration.ApplicationActiveLogger;

		public override Type TypeOfTopLevelBusinessObject => typeof(ApplicationActiveLogger);

		protected override IZForm GetForm(IBusiness businessEntity) => new ApplicationActiveLoggerForm((ApplicationActiveLogger)businessEntity);

		protected override SecurityCheckpoint CheckPointForDelete => EDISecurityCheckpoints.ApplicationLoggingApplicationActiveLoggerDelete;

		protected override SecurityCheckpoint CheckPointForEdit => EDISecurityCheckpoints.ApplicationLoggingApplicationActiveLoggerEdit;

		protected override SecurityCheckpoint CheckPointForNew => EDISecurityCheckpoints.ApplicationLoggingApplicationActiveLoggerNew;

		protected override SecurityCheckpoint CheckPointForView => EDISecurityCheckpoints.ApplicationLoggingApplicationActiveLoggerView;
	}
}
