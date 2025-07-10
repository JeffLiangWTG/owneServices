using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core.Writing;

namespace Enterprise.Customs.IT.NCTS.DataTransfer.Testing;

sealed class NctsMoveHeaderDataObjectWriterTest : DataObjectWriterTest
{
	public void TestPopulateElectronicFolderCustomsReference()
	{
		movementHeader.UseElectronicFolder = false;
		var dataObject = writer.GetDataObject(nctsHeader);
		var electronicDocumentsCusRefList = dataObject.CustomsReferenceCollection.Where(x => x.Type.Code.GetValueOrDefault() == "EDO").ToArray();
		AssertEquals("'AUT' CustomsReferenceCollection Length", 1, electronicDocumentsCusRefList.Length);
		CombineAssertions("When UseElectronicFolder = false", () => AssertElectronicDocumentsCustomsReference(electronicDocumentsCusRefList[0], "N"));

		movementHeader.UseElectronicFolder = true;
		dataObject = writer.GetDataObject(nctsHeader);
		electronicDocumentsCusRefList = dataObject.CustomsReferenceCollection.Where(x => x.Type.Code.GetValueOrDefault() == "EDO").ToArray();
		AssertEquals("'AUT' CustomsReferenceCollection Length", 1, electronicDocumentsCusRefList.Length);
		CombineAssertions("When UseElectronicFolder = true", () => AssertElectronicDocumentsCustomsReference(electronicDocumentsCusRefList[0], "Y"));

		void AssertElectronicDocumentsCustomsReference(CustomsReference customsReference, ZString expectedReference)
		{
			AssertEquals("Type.Description", "Electronic Documents", customsReference.Type.Description);
			AssertEquals("Reference", expectedReference, customsReference.Reference);
		}
	}

	public void TestPopulateDefermentAccountNumber()
	{
		movementHeader.DefermentAccountNumber = "ABC";
		var dataObject = writer.GetDataObject(nctsHeader);
		AssertEquals("DefermentAccountNumber", "ABC", dataObject.DefermentAccountNumber);
	}

	public void TestGetNewDepartureGoodsItemDataObjectWriter()
	{
		var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
		shipment.SetAddInfoCollection(() => new List<AddInfo>());
		shipment.SetCustomsReferenceCollection(() => new List<CustomsReference>());
		var helper = new UniversalDataObjectWriterHelper(Factory, Core.Constants.CountryCodes.Italy);
		var manager = new DataWritingManager(new ActionInfo(null, movementHeader));
		var departureGoodsItemDataObjectWriterForTest = new NctsMoveHeaderDataObjectWriterForTest(manager, helper, shipment);
		AssertType<DepartureGoodsItemDataObjectWriter>(departureGoodsItemDataObjectWriterForTest.GetNewDepartureGoodsItemDataObjectWriterExposed());
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeader();
		movementHeader = nctsHeader.MovementHeader;
		writer = new NctsHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(null, nctsHeader)));
	}

	NctsHeader nctsHeader;
	NctsDepartureMovementHeader movementHeader;
	NctsHeaderDataObjectWriter writer;

	class NctsMoveHeaderDataObjectWriterForTest : NctsMoveHeaderDataObjectWriter
	{
		public NctsMoveHeaderDataObjectWriterForTest(IDataWritingManager manager, UniversalDataObjectWriterHelper helper, Shipment headerData) : base(manager, helper, headerData)
		{
		}

		public EU.NCTS.DataTransfer.Phase4.DepartureGoodsItemDataObjectWriter GetNewDepartureGoodsItemDataObjectWriterExposed()
		{
			var helper = new UniversalDataObjectWriterHelper(new BusinessObjectFactory(), Core.Constants.CountryCodes.Italy);
			return GetNewDepartureGoodsItemDataObjectWriter(writeManager, helper, headerData: null, commercialInvoiceHeaderData: null);
		}
	}
}
