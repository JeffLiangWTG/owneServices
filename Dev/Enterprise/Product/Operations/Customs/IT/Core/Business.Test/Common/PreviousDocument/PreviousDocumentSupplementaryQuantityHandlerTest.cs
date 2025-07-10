using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class PreviousDocumentSupplementaryQuantityHandlerTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when previousDocument parameter is null", () => new PreviousDocumentSupplementaryQuantityHandler(null, null));
	}

	public void TestUniversalTariff()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.Italy, "IMP");
		Factory.Save();
		helper.CreateTariff(Core.Constants.CountryCodes.Italy, tariffType.PK, "08091998", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();
		previousDocument.FormattedTariff = "";

		var previousDocumentSupplementaryQuantityHandler = new PreviousDocumentSupplementaryQuantityHandlerForTest(previousDocument, previousDocument.Factory);

		AssertNull("UniversalTariff should be null when formatted tariff is empty", previousDocumentSupplementaryQuantityHandler.UniversalTariffExposed);

		previousDocument.FormattedTariff = "1234";
		AssertNull("UniversalTariff should be null when formatted tariff is not a valid tariff", previousDocumentSupplementaryQuantityHandler.UniversalTariffExposed);

		previousDocument.FormattedTariff = "08091998";
		AssertNotNull("UniversalTariff should be not null when formatted tariff is a valid tariff", previousDocumentSupplementaryQuantityHandler.UniversalTariffExposed);
	}

	public void TestSupplementaryQuantityUOMs()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Italy, Universal.Constants.TariffTypes.Import);
		Factory.Save();

		var tariffWithOneUnit = helper.CreateTariff(Core.Constants.CountryCodes.Italy, tariffType.PK, "1111111111", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateTariffUOM(tariffWithOneUnit, UnitOfMeasureTypes.AdditionalUOMType, "NAR");
		Factory.Save();

		previousDocument.FormattedTariff = "1234";
		AssertEquals("UniversalTariff should be null when formatted tariff is not a valid tariff", 0, previousDocument.SupplementaryQuantityHandler.SupplementaryQuantityUOMs.Count());

		previousDocument.FormattedTariff = "1111111111";
		AssertNotEquals("UniversalTariff should be not null when formatted tariff is a valid tariff", 0, previousDocument.SupplementaryQuantityHandler.SupplementaryQuantityUOMs.Count());
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		previousDocument = invoiceLine.PreviousDocuments.AddNew();
	}
	JobDeclaration declaration;
	PreviousDocument previousDocument;

	class PreviousDocumentSupplementaryQuantityHandlerForTest : PreviousDocumentSupplementaryQuantityHandler
	{
		public PreviousDocumentSupplementaryQuantityHandlerForTest(IPreviousDocumentUniversalTariffProvider previousDocumentUniversalTariffProvider, BusinessObjectFactory factory)
			: base(previousDocumentUniversalTariffProvider, factory)
		{
		}

		public TariffView UniversalTariffExposed => UniversalTariff;
	}
}
