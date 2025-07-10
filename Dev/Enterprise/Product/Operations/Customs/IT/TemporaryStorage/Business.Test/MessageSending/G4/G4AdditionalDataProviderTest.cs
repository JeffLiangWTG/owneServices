using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts.TemporaryStorage;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(G4AdditionalDataProvider))]
sealed class G4AdditionalDataProviderTest : TestCaseWithFactory
{
	public void TestGetContainer_WhenBillIsNull()
	{
		AssertNull(provider.GetContainer(businessObject: null));
	}

	public void TestGetContainer_WhenBillHasNoContainersEntered()
	{
		var result = provider.GetContainer(bill);
		AssertEquals(1, result.Count);

		var containerWrapper = result.ElementAt(0);
		AssertEquals(nameof(containerWrapper.IdentificativoContainer), "0", containerWrapper.IdentificativoContainer);
		AssertEquals(nameof(containerWrapper.Sigillo), 1, containerWrapper.Sigillo.Count);
		AssertSealWrapper(containerWrapper.Sigillo.ElementAt(0), "0000", "0");
	}

	public void TestGetContainer_WhenBillHasContainerWithoutSealsEntered()
	{
		var container = header.Containers.AddNew();
		container.ACN_ContainerNumber = "CONT1";

		var pack = bill.Packs.AddNew();
		pack.ContainerPK = container.PK;

		var packedItems = bill.PackedItems.AddNew();
		var linkPackage = packedItems.TemporaryStorageLinkPackages.AddNew();
		linkPackage.Package = pack;
		linkPackage.IsLinked = true;

		var result = provider.GetContainer(bill);
		AssertEquals(1, result.Count);

		var containerWrapper = result.ElementAt(0);
		AssertEquals(nameof(containerWrapper.IdentificativoContainer), "CONT1", containerWrapper.IdentificativoContainer);
		AssertEquals(nameof(containerWrapper.Sigillo), 1, containerWrapper.Sigillo.Count);
		AssertSealWrapper(containerWrapper.Sigillo.ElementAt(0), "0000", "0");
	}

	public void TestGetContainer_WhenBillHasContainerWithSealsEntered()
	{
		var container = header.Containers.AddNew();
		container.ACN_ContainerNumber = "CONT1";
		container.ACN_Seal1 = "S1";
		container.ACN_Seal2 = "S2";
		container.ACN_Seal3 = "S3";
		container.AdditionalSeals.AddNew().BK_SealNumber = "S4";
		container.AdditionalSeals.AddNew().BK_SealNumber = "S4";
		container.AdditionalSeals.AddNew().BK_SealNumber = "";

		var pack = bill.Packs.AddNew();
		pack.ContainerPK = container.PK;

		var packedItems = bill.PackedItems.AddNew();
		var linkPackage = packedItems.TemporaryStorageLinkPackages.AddNew();
		linkPackage.Package = pack;
		linkPackage.IsLinked = true;

		var result = provider.GetContainer(bill);
		AssertEquals(1, result.Count);

		var containerWrapper = result.ElementAt(0);
		AssertEquals(nameof(containerWrapper.IdentificativoContainer), "CONT1", containerWrapper.IdentificativoContainer);
		AssertEquals(nameof(containerWrapper.Sigillo), 4, containerWrapper.Sigillo.Count);

		AssertSealWrapper(containerWrapper.Sigillo.ElementAt(0), "0001", "S1");
		AssertSealWrapper(containerWrapper.Sigillo.ElementAt(1), "0002", "S2");
		AssertSealWrapper(containerWrapper.Sigillo.ElementAt(2), "0003", "S3");
		AssertSealWrapper(containerWrapper.Sigillo.ElementAt(3), "0004", "S4");
	}

	public void TestGetContainerForBillAndItem()
	{
		var header = Factory.New<TemporaryStorageHeader>();

		var c1 = header.Containers.AddNew();
		c1.ACN_ContainerNumber = "C1";
		c1.ACN_Seal1 = "C1.S1";
		c1.ACN_Seal2 = "C1.S2";
		c1.ACN_Seal3 = "C1.S3";
		c1.AdditionalSeals.AddNew().BK_SealNumber = "C1.S4";

		var c2 = header.Containers.AddNew();
		c2.ACN_ContainerNumber = "C2";
		c2.ACN_Seal1 = "C2.S1";
		c2.ACN_Seal2 = "C2.S2";
		c2.ACN_Seal3 = "C2.S3";
		c2.AdditionalSeals.AddNew().BK_SealNumber = "C2.S4";

		var b1 = header.Bills.AddNew();

		var b1p1 = b1.Packs.AddNew();
		var b1p2 = b1.Packs.AddNew();
		var b1p3 = b1.Packs.AddNew();

		b1p1.ContainerPK = c1.PK;
		b1p2.ContainerPK = c2.PK;
		b1p3.ContainerPK = c1.PK;

		var b1pi1 = b1.PackedItems.AddNew();
		var b1pi2 = b1.PackedItems.AddNew();

		var b1pi1l1 = b1pi1.TemporaryStorageLinkPackages.AddNew();
		var b1pi1l2 = b1pi1.TemporaryStorageLinkPackages.AddNew();
		var b1pi1l3 = b1pi1.TemporaryStorageLinkPackages.AddNew();

		var b1pi2l1 = b1pi2.TemporaryStorageLinkPackages.AddNew();
		var b1pi2l2 = b1pi2.TemporaryStorageLinkPackages.AddNew();
		var b1pi2l3 = b1pi2.TemporaryStorageLinkPackages.AddNew();

		b1pi1l1.Package = b1p1;
		b1pi1l1.IsLinked = true;
		b1pi1l2.Package = b1p2;
		b1pi1l2.IsLinked = false;
		b1pi1l3.Package = b1p3;
		b1pi1l3.IsLinked = true;

		b1pi2l1.Package = b1p1;
		b1pi2l1.IsLinked = true;
		b1pi2l2.Package = b1p2;
		b1pi2l2.IsLinked = true;
		b1pi2l3.Package = b1p3;
		b1pi2l3.IsLinked = true;

		var containerDettaglioWrapper = provider.GetContainerDettaglio(b1pi1);
		AssertEquals(nameof(containerDettaglioWrapper.Count), 1, containerDettaglioWrapper.Count);
		AssertContainerDettaglioWrapper(containerDettaglioWrapper.ElementAt(0), "C1", [("0001", "C1.S1"), ("0002", "C1.S2"), ("0003", "C1.S3"), ("0004", "C1.S4")]);

		containerDettaglioWrapper = provider.GetContainerDettaglio(b1pi2);
		AssertEquals(nameof(containerDettaglioWrapper.Count), 2, containerDettaglioWrapper.Count);
		AssertContainerDettaglioWrapper(containerDettaglioWrapper.ElementAt(0), "C1", [("0001", "C1.S1"), ("0002", "C1.S2"), ("0003", "C1.S3"), ("0004", "C1.S4")]);
		AssertContainerDettaglioWrapper(containerDettaglioWrapper.ElementAt(1), "C2", [("0001", "C2.S1"), ("0002", "C2.S2"), ("0003", "C2.S3"), ("0004", "C2.S4")]);

		var containerWrapper = provider.GetContainer(b1);
		AssertEquals(nameof(containerWrapper.Count), 2, containerWrapper.Count);
		AssertContainerWrapper(containerWrapper.ElementAt(0), "C1", [("0001", "C1.S1"), ("0002", "C1.S2"), ("0003", "C1.S3"), ("0004", "C1.S4")]);
		AssertContainerWrapper(containerWrapper.ElementAt(1), "C2", [("0001", "C2.S1"), ("0002", "C2.S2"), ("0003", "C2.S3"), ("0004", "C2.S4")]);
	}

	public void TestGetContainerForBillAndItemWithIdenticalContainerNumber()
	{
		var header = Factory.New<TemporaryStorageHeader>();

		var c1 = header.Containers.AddNew();
		c1.ACN_ContainerNumber = "MERU1";
		c1.ACN_Seal1 = "Seal1";

		var c2 = header.Containers.AddNew();
		c2.ACN_ContainerNumber = "MERU1";
		c2.ACN_Seal1 = "Seal1";
		c2.ACN_Seal2 = "Seal2";

		var b1 = header.Bills.AddNew();

		var b1p1 = b1.Packs.AddNew();
		var b1p2 = b1.Packs.AddNew();

		b1p1.ContainerPK = c1.PK;
		b1p2.ContainerPK = c2.PK;

		var b1pi1 = b1.PackedItems.AddNew();
		var b1pi2 = b1.PackedItems.AddNew();

		var b1pi1l1 = b1pi1.TemporaryStorageLinkPackages.AddNew();
		var b1pi1l2 = b1pi1.TemporaryStorageLinkPackages.AddNew();

		var b1pi2l1 = b1pi2.TemporaryStorageLinkPackages.AddNew();
		var b1pi2l2 = b1pi2.TemporaryStorageLinkPackages.AddNew();

		b1pi1l1.Package = b1p1;
		b1pi1l1.IsLinked = true;
		b1pi1l2.Package = b1p2;
		b1pi1l2.IsLinked = true;

		b1pi2l1.Package = b1p1;
		b1pi2l1.IsLinked = true;
		b1pi2l2.Package = b1p2;
		b1pi2l2.IsLinked = false;

		var containerDettaglioWrapper = provider.GetContainerDettaglio(b1pi1);
		AssertEquals(nameof(containerDettaglioWrapper.Count), 1, containerDettaglioWrapper.Count);
		AssertContainerDettaglioWrapper(containerDettaglioWrapper.ElementAt(0), "MERU1", [("0001", "Seal1"), ("0002", "Seal2")]);

		containerDettaglioWrapper = provider.GetContainerDettaglio(b1pi2);
		AssertEquals(nameof(containerDettaglioWrapper.Count), 1, containerDettaglioWrapper.Count);
		AssertContainerDettaglioWrapper(containerDettaglioWrapper.ElementAt(0), "MERU1", [("0001", "Seal1")]);

		var containerWrapper = provider.GetContainer(b1);
		AssertEquals(nameof(containerWrapper.Count), 1, containerWrapper.Count);
		AssertContainerWrapper(containerWrapper.ElementAt(0), "MERU1", [("0001", "Seal1"), ("0002", "Seal2")]);

		c2.ACN_Seal1 = "Seal2";
		c2.ACN_Seal2 = "Seal3";

		b1pi2l1.IsLinked = false;
		b1pi2l2.IsLinked = true;

		containerDettaglioWrapper = provider.GetContainerDettaglio(b1pi1);
		AssertEquals(nameof(containerDettaglioWrapper.Count), 1, containerDettaglioWrapper.Count);
		AssertContainerDettaglioWrapper(containerDettaglioWrapper.ElementAt(0), "MERU1", [("0001", "Seal1"), ("0002", "Seal2"), ("0003", "Seal3")]);

		containerDettaglioWrapper = provider.GetContainerDettaglio(b1pi2);
		AssertEquals(nameof(containerDettaglioWrapper.Count), 1, containerDettaglioWrapper.Count);
		AssertContainerDettaglioWrapper(containerDettaglioWrapper.ElementAt(0), "MERU1", [("0001", "Seal2"), ("0002", "Seal3")]);

		containerWrapper = provider.GetContainer(b1);
		AssertEquals(nameof(containerWrapper.Count), 1, containerWrapper.Count);
		AssertContainerWrapper(containerWrapper.ElementAt(0), "MERU1", [("0001", "Seal1"), ("0002", "Seal2"), ("0003", "Seal3")]);
	}

	public void TestGetDeposito_WhenAdditionalIdentifierIsNotEntered()
	{
		var goodsLocation = header.GoodsLocation;
		goodsLocation.CGL_AdditionalIdentifier = "";
		AssertNull(provider.GetDeposito(bill));
	}

	public void TestGetDeposito_WhenAdditionalIdentifierIsEntered()
	{
		var goodsLocation = header.GoodsLocation;
		goodsLocation.CGL_AdditionalIdentifier = "1234567";

		var result = provider.GetDeposito(bill);
		AssertNotNull(result);
		AssertEquals(nameof(result.TipoDep), "V", result.TipoDep);
		AssertEquals(nameof(result.IdentificativoDep), "123456", result.IdentificativoDep);
	}

	public void TestGetDeposito_WhenAdditionalIdentifierIsEnteredLessThanSixChars()
	{
		var goodsLocation = header.GoodsLocation;
		goodsLocation.CGL_AdditionalIdentifier = "123";

		var result = provider.GetDeposito(bill);
		AssertNotNull(result);
		AssertEquals(nameof(result.TipoDep), "V", result.TipoDep);
		AssertEquals(nameof(result.IdentificativoDep), "123", result.IdentificativoDep);
	}

	public void TestGetMenzioniSpeciali_WhenPackedItemIsNull()
	{
		AssertNull(provider.GetMenzioniSpeciali(businessObject: null));
	}

	public void TestGetMenzioniSpeciali_WhenNoAdditionalInfoAreEntered()
	{
		var result = provider.GetMenzioniSpeciali(packedItem).ToArray();
		AssertEquals(1, result.Length);

		var item = result[0];
		AssertNull(nameof(item.Codice), item.Codice);
		AssertEquals(nameof(item.Descrizione), "Nessuna delle precedenti.", result[0].Descrizione);
	}

	public void TestGetMenzioniSpeciali_WhenAdditionalInfoAreEntered()
	{
		var additionalInfo = packedItem.AdditionalInfos.AddNew();
		additionalInfo.CSI_SubType = "INF";
		additionalInfo.CSI_Code = "00500";
		additionalInfo.CSI_Description = "Descr";
		var additionalReference = packedItem.AdditionalInfos.AddNew();
		additionalReference.CSI_SubType = "REF";
		additionalReference.CSI_Code = "99999";
		additionalReference.CSI_Description = "Descr";

		var result = provider.GetMenzioniSpeciali(packedItem).ToArray();
		AssertEquals(1, result.Length);

		var item = result[0];
		AssertEquals(nameof(item.Codice), "00500", item.Codice);
		AssertEquals(nameof(item.Descrizione), "Descr", result[0].Descrizione);
	}

	public void TestGetContainerDettaglio_WhenNoContainersAreEntered()
	{
		var containerDettaglioWrapper = provider.GetContainerDettaglio(packedItem);
		AssertEquals(1, containerDettaglioWrapper.Count);
		AssertContainerDettaglioWrapper(containerDettaglioWrapper.ElementAt(0), "0", [("0000", "0")]);
	}

	public void TestGetContainerDettaglio_WhenContainersAreEntered()
	{
		var container1 = header.Containers.AddNew();
		container1.ACN_ContainerNumber = "C1";

		var container2 = header.Containers.AddNew();
		container2.ACN_ContainerNumber = "C2";

		var container3 = header.Containers.AddNew();
		container3.ACN_ContainerNumber = "C3";
		container3.ACN_Seal1 = "S1";
		container3.ACN_Seal2 = "S2";
		container3.ACN_Seal3 = "S3";
		var additionalSeal = container3.AdditionalSeals.AddNew();
		additionalSeal.BK_SequenceNumber = 1;
		additionalSeal.BK_SealNumber = "S4";

		var pack1 = bill.Packs.AddNew();
		pack1.ContainerPK = container1.PK;
		packedItem.TemporaryStorageLinkPackages[0].IsLinked = false;

		var pack2 = bill.Packs.AddNew();
		pack2.ContainerPK = container2.PK;
		packedItem.TemporaryStorageLinkPackages[1].IsLinked = true;

		var pack3 = bill.Packs.AddNew();
		pack3.ContainerPK = container3.PK;
		packedItem.TemporaryStorageLinkPackages[2].IsLinked = true;

		var containerDettaglioWrapper = provider.GetContainerDettaglio(packedItem);
		AssertEquals(nameof(containerDettaglioWrapper.Count), 2, containerDettaglioWrapper.Count);
		AssertContainerDettaglioWrapper(containerDettaglioWrapper.ElementAt(0), "C2", [("0000", "0")]);
		AssertContainerDettaglioWrapper(containerDettaglioWrapper.ElementAt(1), "C3", [("0001", "S1"), ("0002", "S2"), ("0003", "S3"), ("0004", "S4")]);
	}

	public void TestGetPaese_WhenAdditionalIdentifierIsNotEntered()
	{
		var goodsLocation = header.GoodsLocation;
		goodsLocation.CGL_AdditionalIdentifier = "";
		AssertNull(provider.GetPaese(goodsLocation));
	}

	public void TestGetPaese_WhenAdditionalIdentifierIsEntered()
	{
		var goodsLocation = header.GoodsLocation;
		goodsLocation.CGL_AdditionalIdentifier = "1234567";
		AssertEquals("IT", provider.GetPaese(goodsLocation));
	}

	public void TestGetAmendmentPaese_WhenAdditionalIdentifierIsNotEntered()
	{
		var goodsLocation = header.GoodsLocation;
		goodsLocation.CGL_AdditionalIdentifier = "";
		AssertNull(provider.GetAmendmentPaese(goodsLocation));
	}

	public void TestGetAmendmentPaese_WhenAdditionalIdentifierIsEntered()
	{
		var goodsLocation = header.GoodsLocation;
		goodsLocation.CGL_AdditionalIdentifier = "1234567";
		AssertEquals("IT", provider.GetAmendmentPaese(goodsLocation));
	}

	public void TestGetQualificaRappresentante()
	{
		AssertNull("When business object is null", provider.GetQualificaRappresentante(null));

		header.AMA_AgentType = ZString.Empty;
		AssertNull("When AMA_AgentType isn't set", provider.GetQualificaRappresentante(header));

		header.AMA_AgentType = "SEL";
		AssertNull("When AMA_AgentType is SEL", provider.GetQualificaRappresentante(header));

		header.AMA_AgentType = "DIR";
		AssertEquals("When AMA_AgentType is DIR", "2", provider.GetQualificaRappresentante(header));

		header.AMA_AgentType = "IND";
		AssertEquals("When AMA_AgentType is IND", "3", provider.GetQualificaRappresentante(header));
	}

	public void TestGetAmendmentQualificaRappresentante()
	{
		AssertNull("When business object is null", provider.GetAmendmentQualificaRappresentante(null));

		header.AMA_AgentType = ZString.Empty;
		AssertNull("When AMA_AgentType isn't set", provider.GetAmendmentQualificaRappresentante(header));

		header.AMA_AgentType = "SEL";
		AssertNull("When AMA_AgentType is SEL", provider.GetAmendmentQualificaRappresentante(header));

		header.AMA_AgentType = "DIR";
		AssertEquals("When AMA_AgentType is DIR", "2", provider.GetAmendmentQualificaRappresentante(header));

		header.AMA_AgentType = "IND";
		AssertEquals("When AMA_AgentType is IND", "3", provider.GetAmendmentQualificaRappresentante(header));
	}

	public void TestGetDichiarazioneSemplificata_WhenBillAndPackedItemHavePreviousDocuments()
	{
		var billPreviousDocument = bill.PreviousDocuments.AddNew();
		billPreviousDocument.CSI_Code = "CODE1";
		billPreviousDocument.CSI_ReferenceNumber = "REF1";
		billPreviousDocument.CSI_PackType = "P1";
		billPreviousDocument.CSI_PackQty = 1;
		billPreviousDocument.CSI_UnitOfQuantity = "KG";
		billPreviousDocument.CSI_Quantity = 100;

		var packedItemPreviousDocument1 = packedItem.PreviousDocuments.AddNew();
		packedItemPreviousDocument1.CSI_Code = "CODE2";
		packedItemPreviousDocument1.CSI_ReferenceNumber = "REF2";
		packedItemPreviousDocument1.CSI_PackType = "P2";
		packedItemPreviousDocument1.CSI_PackQty = 2;
		packedItemPreviousDocument1.CSI_UnitOfQuantity = "L";
		packedItemPreviousDocument1.CSI_Quantity = 200;

		var packedItemPreviousDocument2 = packedItem.PreviousDocuments.AddNew();
		packedItemPreviousDocument2.CSI_Code = "CODE1";
		packedItemPreviousDocument2.CSI_ReferenceNumber = "REF1";
		packedItemPreviousDocument2.CSI_PackType = "P1";
		packedItemPreviousDocument2.CSI_PackQty = 1;
		packedItemPreviousDocument2.CSI_UnitOfQuantity = "KG";
		packedItemPreviousDocument2.CSI_Quantity = 100;

		var previousDocuments = provider.GetDichiarazioneSemplificata(bill);

		CombineAssertions("Lists distinct previousDocuments", () =>
		{
			AssertEquals(2, previousDocuments.Count);

			var item1 = previousDocuments.ToArray()[0];
			AssertEquals("CODE1", item1.DichSempTipoDocumento);
			AssertEquals("REF1", item1.DichSempDocumentoPrecedente);
			AssertEquals("P1", item1.DichSempTipoImballaggi);
			AssertEquals("2", item1.DichSempNumeroImballaggi);
			AssertEquals("KG", item1.DichSempUnitaMisura);
			AssertEquals(200m, item1.DichSempQuantita);

			var item2 = previousDocuments.ToArray()[1];
			AssertEquals("CODE2", item2.DichSempTipoDocumento);
			AssertEquals("REF2", item2.DichSempDocumentoPrecedente);
			AssertEquals("P2", item2.DichSempTipoImballaggi);
			AssertEquals("2", item2.DichSempNumeroImballaggi);
			AssertEquals("L", item2.DichSempUnitaMisura);
			AssertEquals(200m, item2.DichSempQuantita);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		provider = new G4AdditionalDataProvider();
		header = Factory.New<TemporaryStorageHeader>();
		bill = header.Bills.AddNew();
		packedItem = bill.PackedItems.AddNew();
	}

	TemporaryStorageHeader header;
	TemporaryStorageBill bill;
	TemporaryStoragePackedItem packedItem;
	ITemporaneaCustodiaG4AdditionalDataProvider provider;

	void AssertSealWrapper(SigilloTypeDataProviderAbstractClass sealWrapper, string sequenceNumber, string sealNumber)
	{
		AssertEquals(nameof(sealWrapper.NumeroSigilli), sequenceNumber, sealWrapper.NumeroSigilli);
		AssertEquals(nameof(sealWrapper.IdentificativoSigillo), sealNumber, sealWrapper.IdentificativoSigillo);
	}

	void AssertContainerDettaglioWrapper(ContainerDettaglioTypeDataProviderAbstractClass containerWrapper, string containerNumber, (string SequenceNumber, string SealNumber)[] seals)
	{
		AssertEquals(nameof(containerWrapper.IdentificativoContainerDettaglio), containerNumber, containerWrapper.IdentificativoContainerDettaglio);
		AssertEquals(nameof(containerWrapper.SigilloDettaglio.Count), seals.Length, containerWrapper.SigilloDettaglio.Count);

		for (var i = 0; i < seals.Length; i++)
		{
			var (sequenceNumber, sealNumber) = seals[i];
			AssertSealWrapper(containerWrapper.SigilloDettaglio.ElementAt(i), sequenceNumber, sealNumber);
		}
	}

	void AssertContainerWrapper(ContainerTypeDataProviderAbstractClass containerWrapper, string containerNumber, (string SequenceNumber, string SealNumber)[] seals)
	{
		AssertEquals(nameof(containerWrapper.IdentificativoContainer), containerNumber, containerWrapper.IdentificativoContainer);
		AssertEquals(nameof(containerWrapper.Sigillo), seals.Length, containerWrapper.Sigillo.Count);

		Enumerable.Range(0, seals.Length)
			.ForEach(i => AssertSealWrapper(containerWrapper.Sigillo.ElementAt(i), seals[i].SequenceNumber, seals[i].SealNumber));
	}
}
