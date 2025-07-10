using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Module.Testing;

[TestedType(typeof(ImportFromTemporaryStorageRegisterFilterStripBusinessObject))]
sealed class ImportFromTemporaryStorageRegisterFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
{
	public void TestRegistrationNumberFilterProperties()
	{
		var registrationNumberFilter = (ModuleTextFilter)filter[ImportFromTemporaryStorageRegisterFilterStripBusinessObject.Schema.RegistrationNumber];
		CombineAssertions(() =>
		{
			AssertEquals("Description", "Registration Number", registrationNumberFilter.Description);
			AssertEquals("Category", FilterCategories.NumbersAndReferences, registrationNumberFilter.Category);
			AssertEquals("MaxLength", CusTempStorageRegHeaderSchema.SRH_Reference.MaxLength, registrationNumberFilter.MaxLength);
			AssertEquals("Visibility", FilterVisibility.AlwaysVisible, registrationNumberFilter.Visibility);
		});
	}

	public void TestRegistrationNumberFilter()
	{
		var (_, regLine) = CreateBusinessObject("REF123");
		var (_, regLine2) = CreateBusinessObject("REF456");
		Factory.Save();

		var registrationNumberFilter = (ModuleTextFilter)filter[ImportFromTemporaryStorageRegisterFilterStripBusinessObject.Schema.RegistrationNumber];
		registrationNumberFilter.IsActive = true;
		registrationNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;

		CombineAssertions(() =>
		{
			registrationNumberFilter.Property = "REF123";
			AssertEquals("regLine", expected: true, regLine.MatchesFilter(filter.Filter));
			AssertEquals("regLine2", expected: false, regLine2.MatchesFilter(filter.Filter));
		});
	}

	public void TestCustomerReferenceFilterProperties()
	{
		var customerReferenceFilter = (ModuleTextFilter)filter[ImportFromTemporaryStorageRegisterFilterStripBusinessObject.Schema.CustomerReference];
		CombineAssertions(() =>
		{
			AssertEquals("Description", "Customer Reference", customerReferenceFilter.Description);
			AssertEquals("Category", FilterCategories.NumbersAndReferences, customerReferenceFilter.Category);
			AssertEquals("MaxLength", CusTempStorageRegHeaderSchema.SRH_InternalReference.MaxLength, customerReferenceFilter.MaxLength);
			AssertEquals("Visibility", FilterVisibility.AlwaysVisible, customerReferenceFilter.Visibility);
		});
	}

	public void TestCustomerReferenceFilter()
	{
		var (regHeader, regLine) = CreateBusinessObject("REF123");
		regHeader.SRH_InternalReference = "INT123";
		var (regHeader2, regLine2) = CreateBusinessObject("REF456");
		regHeader2.SRH_InternalReference = "INT456";
		Factory.Save();

		var customerReferenceFilter = (ModuleTextFilter)filter[ImportFromTemporaryStorageRegisterFilterStripBusinessObject.Schema.CustomerReference];
		customerReferenceFilter.IsActive = true;
		customerReferenceFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;

		CombineAssertions(() =>
		{
			customerReferenceFilter.Property = "INT123";
			AssertEquals("regLine", expected: true, regLine.MatchesFilter(filter.Filter));
			AssertEquals("regLine2", expected: false, regLine2.MatchesFilter(filter.Filter));
		});
	}

	public void TestLineNumberFilterProperties()
	{
		var lineNumberFilter = (ModuleNumberRangeFilter)filter[ImportFromTemporaryStorageRegisterFilterStripBusinessObject.Schema.LineNumber];
		CombineAssertions(() =>
		{
			AssertEquals("Description", "Line #", lineNumberFilter.Description);
			AssertEquals("Category", FilterCategories.NumbersAndReferences, lineNumberFilter.Category);
			AssertEquals("MaxLength", CusTempStorageRegLineSchema.SRL_LineNumber.MaxLength, lineNumberFilter.MaxLength);
			AssertEquals("PropertySearch", ModuleNumberRangeFilter.SearchTexts.GreaterThanOrEqualTo, lineNumberFilter.PropertySearch);
		});
	}

	public void TestLineNumberFilter()
	{
		var (_, regLine) = CreateBusinessObject("REF123");
		regLine.SRL_LineNumber = 1;
		var (_, regLine2) = CreateBusinessObject("REF456");
		regLine2.SRL_LineNumber = 3;
		Factory.Save();

		var lineNumberFilter = (ModuleNumberRangeFilter)filter[ImportFromTemporaryStorageRegisterFilterStripBusinessObject.Schema.LineNumber];
		lineNumberFilter.IsActive = true;
		lineNumberFilter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.LessThanOrEqualTo;

		CombineAssertions(() =>
		{
			lineNumberFilter.Property2 = 2;
			AssertEquals("regLine", expected: true, regLine.MatchesFilter(filter.Filter));
			AssertEquals("regLine2", expected: false, regLine2.MatchesFilter(filter.Filter));
		});
	}

	public void TestOwnerReferenceNumberFilterProperties()
	{
		var ownerReferenceNumberFilter = (ModuleTextFilter)filter[ImportFromTemporaryStorageRegisterFilterStripBusinessObject.Schema.LineOwnerReferenceNumber];
		CombineAssertions(() =>
		{
			AssertEquals("Description", "Owner Reference Number", ownerReferenceNumberFilter.Description);
			AssertEquals("Category", FilterCategories.NumbersAndReferences, ownerReferenceNumberFilter.Category);
			AssertEquals("MaxLength", CusTempStorageRegLineSchema.SRL_OwnerReference.MaxLength, ownerReferenceNumberFilter.MaxLength);
			AssertEquals("Visibility", FilterVisibility.AlwaysVisible, ownerReferenceNumberFilter.Visibility);
		});
	}

	public void TestOwnerReferenceNumberFilter()
	{
		var (_, regLine) = CreateBusinessObject("REF123");
		regLine.SRL_OwnerReference = "OWNERREF123";
		var (_, regLine2) = CreateBusinessObject("REF456");
		regLine2.SRL_OwnerReference = "OWNERREF456";
		Factory.Save();

		var ownerReferenceNumberFilter = (ModuleTextFilter)filter[ImportFromTemporaryStorageRegisterFilterStripBusinessObject.Schema.LineOwnerReferenceNumber];
		ownerReferenceNumberFilter.IsActive = true;
		ownerReferenceNumberFilter.ComparisonOperator = ModuleNumberFilter.ComparisonConstants.Exact;

		CombineAssertions(() =>
		{
			ownerReferenceNumberFilter.Property = "OWNERREF123";
			AssertEquals("regLine", expected: true, regLine.MatchesFilter(filter.Filter));
			AssertEquals("regLine2", expected: false, regLine2.MatchesFilter(filter.Filter));
		});
	}

	public void TestOwnerReferenceTypeFilterProperties()
	{
		var ownerReferenceTypeFilter = (ModuleTextFilter)filter[ImportFromTemporaryStorageRegisterFilterStripBusinessObject.Schema.LineOwnerReferenceType];
		CombineAssertions(() =>
		{
			AssertEquals("Description", "Owner Reference Type", ownerReferenceTypeFilter.Description);
			AssertEquals("Category", FilterCategories.ModesAndTypes, ownerReferenceTypeFilter.Category);
			AssertEquals("MaxLength", CusTempStorageRegLineSchema.SRL_OwnerReferenceType.MaxLength, ownerReferenceTypeFilter.MaxLength);
		});
	}

	public void TestOwnerReferenceTypeFilter()
	{
		var (_, regLine) = CreateBusinessObject("REF123");
		regLine.SRL_OwnerReferenceType = "TPA";
		var (_, regLine2) = CreateBusinessObject("REF456");
		regLine2.SRL_OwnerReference = "TPB";
		Factory.Save();

		var ownerReferenceTypeFilter = (ModuleTextFilter)filter[ImportFromTemporaryStorageRegisterFilterStripBusinessObject.Schema.LineOwnerReferenceType];
		ownerReferenceTypeFilter.IsActive = true;
		ownerReferenceTypeFilter.ComparisonOperator = ModuleNumberFilter.ComparisonConstants.Exact;

		CombineAssertions(() =>
		{
			ownerReferenceTypeFilter.Property = "TPA";
			AssertEquals("regLine", expected: true, regLine.MatchesFilter(filter.Filter));
			AssertEquals("regLine2", expected: false, regLine2.MatchesFilter(filter.Filter));
		});
	}

	public void TestGoodsDescriptionFilterProperties()
	{
		var goodsDescriptionFilter = (ModuleTextFilter)filter[ImportFromTemporaryStorageRegisterFilterStripBusinessObject.Schema.LineGoodsDescription];
		CombineAssertions(() =>
		{
			AssertEquals("Description", "Goods Description", goodsDescriptionFilter.Description);
			AssertEquals("Category", FilterCategories.TextSearch, goodsDescriptionFilter.Category);
			AssertEquals("MaxLength", CusTempStorageRegLineSchema.SRL_GoodsDescription.MaxLength, goodsDescriptionFilter.MaxLength);
		});
	}

	public void TestGoodsDescriptionFilter()
	{
		var (_, regLine) = CreateBusinessObject("REF123");
		regLine.SRL_GoodsDescription = "DESCRIPTION1";
		var (_, regLine2) = CreateBusinessObject("REF456");
		regLine2.SRL_GoodsDescription = "DESCRIPTION2";
		Factory.Save();

		var ownerReferenceTypeFilter = (ModuleTextFilter)filter[ImportFromTemporaryStorageRegisterFilterStripBusinessObject.Schema.LineGoodsDescription];
		ownerReferenceTypeFilter.IsActive = true;
		ownerReferenceTypeFilter.ComparisonOperator = ModuleNumberFilter.ComparisonConstants.Exact;

		CombineAssertions(() =>
		{
			ownerReferenceTypeFilter.Property = "DESCRIPTION1";
			AssertEquals("regLine", expected: true, regLine.MatchesFilter(filter.Filter));
			AssertEquals("regLine2", expected: false, regLine2.MatchesFilter(filter.Filter));
		});
	}

	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new ImportFromTemporaryStorageRegisterFilterStripBusinessObject();

	protected override void SetUp()
	{
		base.SetUp();
		filter = new ();
	}
	ImportFromTemporaryStorageRegisterFilterStripBusinessObject filter;

	(CusTempStorageRegHeader, CusTempStorageRegLine) CreateBusinessObject(ZString reference)
	{
		var regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_AppCode = "SUM";
		regHeader.SRH_Reference = reference;
		var regLine = regHeader.CusTempStorageRegLines.AddNew();
		regLine.SRL_LineNumber = 1;
		return (regHeader, regLine);
	}
}
