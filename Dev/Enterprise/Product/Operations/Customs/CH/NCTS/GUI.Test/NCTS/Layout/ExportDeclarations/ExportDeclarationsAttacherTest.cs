using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NctsHeader = Enterprise.Customs.CH.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

class ExportDeclarationsAttacherTest : TestCaseForAttachGUI
{
	public void TestAttachCore() => CombineAssertions(() =>
	{
		var cusEntryHeader = CreateCusEntryHeader();

		AssertEquals("AttachCore success", true, Attacher.AttachCoreExposed(cusEntryHeader, null));
		AssertEquals("CusEntryHeader added", true, RelatedExportEntryHeaderGenPivotCollection.Contains(cusEntryHeader));
	});

	public void TestGetSelectedPKFromDestinationCollection()
	{
		var cusEntryHeader = CreateCusEntryHeader();
		var genPivot = RelatedExportEntryHeaderGenPivotCollection.AddPivotFor(cusEntryHeader);
		AssertEquals("Should provide XX_Relation2ID", cusEntryHeader.PK, Attacher.GetSelectedPKFromDestinationCollectionExposed(genPivot));
	}

	NctsHeader CreateNctsHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return nctsHeader;
	}

	NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = CreateNctsHeader());
	NctsHeader nctsHeader;

	RelatedExportEntryHeaderGenPivotCollection RelatedExportEntryHeaderGenPivotCollection => NctsHeader.MovementHeader.RelatedExportEntryHeaders;

	CusEntryHeader CreateCusEntryHeader()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		return jobDeclaration.CustomsEntryHeaders.AddNew();
	}

	ExportDeclarationsAttacherForTesting Attacher => attacher ?? (attacher = new ExportDeclarationsAttacherForTesting(NctsHeader.MovementHeader.RelatedExportEntryHeaders, NctsHeader.MovementHeader.Lookups.ExportEntryHeaderCollection, ModuleIDs.Customs.EntryHeader, NctsHeader));
	ExportDeclarationsAttacherForTesting attacher;

	class ExportDeclarationsAttacherForTesting : ExportDeclarationsAttacher
	{
		internal ExportDeclarationsAttacherForTesting(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID, NctsHeader nctsHeader) : base(destinationCollection, findBoxList, moduleID, nctsHeader)
		{
		}

		internal bool AttachCoreExposed(BusinessObject bizO, List<BusinessObject> listToBulkAdd) => base.AttachCore(bizO, listToBulkAdd);

		internal ZGuid GetSelectedPKFromDestinationCollectionExposed(BusinessObject bizObj) => base.GetSelectedPKFromDestinationCollection(bizObj);
	}
}
