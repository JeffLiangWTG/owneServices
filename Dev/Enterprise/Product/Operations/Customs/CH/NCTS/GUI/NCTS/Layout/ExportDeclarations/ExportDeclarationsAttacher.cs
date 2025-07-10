using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CH.NCTS.GUI;

public class ExportDeclarationsAttacher : ZRecordAttacher
{
	public ExportDeclarationsAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID, NctsHeader nctsHeader)
		: base(destinationCollection, findBoxList, moduleID)
	{
	}

	protected override bool AttachCore(BusinessObject bizO, List<BusinessObject> listToBulkAdd)
	{
		var result = false;
		if (bizO is CusEntryHeader cusEntryHeader)
		{
			var collection = (RelatedExportEntryHeaderGenPivotCollection)DestinationCollection;
			collection.AddPivotFor(cusEntryHeader);
			result = true;
		}
		return result;
	}

	protected override ZGuid GetSelectedPKFromDestinationCollection(BusinessObject bizObj)
	{
		return bizObj is RelatedExportEntryHeaderGenPivot relatedExportEntryHeaderGenPivot ? relatedExportEntryHeaderGenPivot.XX_Relation2ID : ZGuid.Empty;
	}
}
