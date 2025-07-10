using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class AccReportingBookController : ZController
	{
		public override ControllerID ID => ControllerIDs.AccReportingBook;

		public override ModuleIdentifier ModuleID => ModuleIDs.AccReportingBook;

		public override Type TypeOfTopLevelBusinessObject => typeof(AccReportingBook);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.ReportingBooksView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.ReportingBooksNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.ReportingBooksModify;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new AccReportingBookForm((AccReportingBook)businessEntity);
		}
	}
}
