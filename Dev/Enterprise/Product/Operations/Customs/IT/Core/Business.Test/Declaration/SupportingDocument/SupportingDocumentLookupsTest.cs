using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class SupportingDocumentLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestStatusList()
	{
		AssertEquals(true, object.ReferenceEquals(Factory.GetCachedValue<AvailabilityTypeList>(), supportingDocument.Lookups.StatusList));
	}

	public void TestCodeList()
	{
		var collection = (ZZRefCusCodeListCombinedCollection)supportingDocument.Lookups.CodeList;
		collection.Load();
		AssertEquals(1, collection.Count);
		AssertEquals("9001", collection[0].ZZD_Code);

		var support = invoiceHeader.SupportingDocuments.AddNew();
		collection = (ZZRefCusCodeListCombinedCollection)support.Lookups.CodeList;
		collection.Load();
		AssertEquals(1, collection.Count);
		AssertEquals("9001", collection[0].ZZD_Code);
	}

	public void TestUnitOfQuantityList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "ASVX", "Hectolitre", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeUtc);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "CTM", "Carats", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeUtc);
		Factory.Save();

		AssertEquals("ASVX, CTM", supportingDocument.Lookups.UnitOfQuantityList.CodesAsString);
	}

	#region Implementation

	protected override void SetUp()
	{
		base.SetUp();
		PrepareCusCodeListData();
		declaration = Factory.New<JobDeclaration>();
		invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		supportingDocument = invoiceLine.SupportingDocuments.AddNew();
	}

	void PrepareCusCodeListData()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
		var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
		helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.Italy, new string[] { importCodeType, exportCodeType }, "9001", "9001 DES", new Dictionary<string, string[]>(), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();
	}

	SupportingDocument supportingDocument;
	JobComInvoiceLine invoiceLine;
	JobComInvoiceHeader invoiceHeader;
	JobDeclaration declaration;

	#endregion
}
