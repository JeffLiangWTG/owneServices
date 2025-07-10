using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common.CH;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class ExportEntryDocumentDataProviderTest : BaseDepartureDataProviderTest<ExportEntryDocumentDataProvider, NctsHeaderDepartureMessageSendingObject>
{
	public void TestNewCollection()
	{
		AssertNull("null argument", ExportEntryDocumentDataProvider.NewCollection(null));
	}

	public void TestSequenceNumber()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		DepartureMovementHeader.RelatedExportEntryHeaders.AddPivotFor(jobDeclaration.CustomsEntryHeaders.AddNew()).XX_Sequence = 13;
		DepartureMovementHeader.RelatedExportEntryHeaders.AddPivotFor(jobDeclaration.CustomsEntryHeaders.AddNew()).XX_Sequence = 11;

		var dataProviders = ExportEntryDocumentDataProvider.NewCollection(DepartureMovementHeader.RelatedExportEntryHeaders).ToArray();
		AssertEquals("Count", 2, dataProviders.Length);
		AssertEquals("SequenceNumber[0]", 11, dataProviders[0].SequenceNumber);
		AssertEquals("SequenceNumber[1]", 13, dataProviders[1].SequenceNumber);
	}

	public void TestProperties() => CombineAssertions(() =>
	{
		RelatedExportEntryHeaderGenPivot.XX_Sequence = 23;
		RelatedExportEntryHeaderGenPivot.EntryHeader.MovementReferenceNumberSetter("MRN123.4");

		AssertEquals("SequenceNumber", 23, DataProvider.SequenceNumber);
		AssertEquals("Type", "EXPO", DataProvider.Type);
		AssertEquals("ReferenceNumber", "MRN123", DataProvider.ReferenceNumber);
		AssertNull("ComplementOfInformation", DataProvider.ComplementOfInformation);
		AssertNull("TransportEquipments", DataProvider.TransportEquipments);
	});

	public void TestUnusedProperties() => CombineAssertions(() =>
	{
		_ = RelatedExportEntryHeaderGenPivot;
		AssertNull("LineItemNumber", DataProvider.LineItemNumber);
		AssertEquals("GoodsItemNumber", 0, DataProvider.GoodsItemNumber);
	});

	public void TestTransportEquipments() => CombineAssertions(() =>
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		var container1 = jobDeclaration.CusContainers.AddNew();
		container1.CO_ContainerNumber = "C1";
		var container2 = jobDeclaration.CusContainers.AddNew();
		container2.CO_ContainerNumber = "C2";
		Factory.Save();

		DepartureMovementHeader.RelatedExportEntryHeaders.AddPivotFor(jobDeclaration.CustomsEntryHeaders.First());

		var transportEquipments = DataProvider.TransportEquipments.ToArray();

		AssertEquals($"Count", 2, transportEquipments.Length);
		Assert("C1", transportEquipments.Any(x => x.ContainerIdentificationNumber == "C1"));
		Assert("C2", transportEquipments.Any(x => x.ContainerIdentificationNumber == "C2"));

		AssertSame("Cached", DataProvider.TransportEquipments, DataProvider.TransportEquipments);

		ResetDataProvider();
		jobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		AssertNull("Not EDA", DataProvider.TransportEquipments);
	});

	protected override ExportEntryDocumentDataProvider CreateDataProvider() => ExportEntryDocumentDataProvider.NewCollection(DepartureMovementHeader.RelatedExportEntryHeaders).First();
}
