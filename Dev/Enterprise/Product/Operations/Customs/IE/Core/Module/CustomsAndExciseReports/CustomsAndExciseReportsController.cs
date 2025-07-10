using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.IE.Module
{
	public class CustomsAndExciseReportsController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.IE.CustomsAndExciseReports;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.EU.IE.CustomsAndExciseReports;

		public override Type TypeOfTopLevelBusinessObject => typeof(CustomsAndExciseReportOutboundMessage);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.CustomsFiles;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.CustomsFiles;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.CustomsFiles;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.CustomsFiles;

		protected override IZForm GetForm(IBusiness businessEntity) => new CustomsAndExciseReportsForm((CustomsAndExciseReportOutboundMessage)businessEntity);
	}
}
