using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CH.Module;

public sealed class CustomsSummaryModule : ZFilterGridModule
{
	public override ModuleIdentifier ID => ModuleIDs.Customs.CH.CustomsSummary;

	public override bool AllowNew => false;

	public override bool AllowEdit => true;

	public override bool AllowDelete => false;

	public override bool AllowView => true;

	public override bool AllowUniversalCopy => false;

	public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

	protected override SortInfo DefaultSortOrder => new SortInfo(CustomsSummaryLine.Schema.ProcessDate, System.ComponentModel.ListSortDirection.Descending);

	protected override IFilterControl GetNewFilterControl() => new CustomsSummaryFilterControl(GridCollection, FilterBusinessObject);

	protected override IBusinessObjectCollection GetNewGridCollection() => new CustomsSummaryLineCollection(Factory);

	protected override FilterBusinessObject GetNewFilterBusinessObject() => new CustomsSummaryFilterStripBusinessObject();

	protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.CH.CustomsSummary);

	protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ImportBroker;

	public override Security.SecurityCheckpoint SecurityCheckpoint => Env.Security.CustomsDeclarationEnquiryEdit;
}
