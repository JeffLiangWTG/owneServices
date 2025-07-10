using CargoWise.EntityFramework;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CH.NCTS.GUI;

public class ExportDeclarationsModuleButtonGrid : ZModuleButtonGrid
{
	protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
	{
		return new ExportDeclarationsAttacher(destinationCollection, findBoxList, moduleID, NctsHeader);
	}
	NctsHeader NctsHeader => DataSource as NctsHeader;

	protected override void Detach(BusinessObject selected)
	{
		base.Detach(selected);

		if (selected is RelatedExportEntryHeaderGenPivot relatedExportEntryHeaderGenPivot && !Collection.Contains(relatedExportEntryHeaderGenPivot))
		{
			relatedExportEntryHeaderGenPivot.Delete();
		}
	}

	protected override BusinessObject GetObjectToEdit(BusinessObject selected) => (selected as RelatedExportEntryHeaderGenPivot)?.EntryHeader;

	protected override ZController GetNewControllerCore(BusinessObject selected) => EntryHeaderController;

	protected ZController EntryHeaderController => entryHeaderController ?? (entryHeaderController = ZControllerFactory.Create(ControllerIDs.Customs.EntryHeader));
	ZController entryHeaderController;
}
