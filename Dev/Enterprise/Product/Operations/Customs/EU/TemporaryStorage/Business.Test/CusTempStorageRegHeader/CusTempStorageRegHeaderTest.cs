using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Business.Testing;

[TestedType(typeof(CusTempStorageRegHeader))]
sealed class CusTempStorageRegHeaderTest : EnterpriseBusinessObjectTestCase
{
	public void TestLookupType() => AssertType<CusTempStorageRegHeaderLookups>(header.Lookups);

	public void TestCusTempStorageRegLines() => AssertType<CusTempStorageRegLineCollection<CusTempStorageRegLine>>(header.CusTempStorageRegLines);

	public void TestPremises()
	{
		var premises = Factory.New<CusTempStorageRegPremises>();
		premises.SRP_Code = "AAA";
		header.SRH_SRP_Premises = premises.PK;
		AssertType<CusTempStorageRegPremises>(header.Premises);
	}

	public void TestStorageRegLine() => AssertEquals(typeof(CusTempStorageRegLine), header.GetStorageRegLineType());

	public void TestGuarantee()
	{
		var guarantee = header.Guarantee;
		CombineAssertions("", () =>
		{
			AssertEquals("Guarantee empty when it does not join.", ZString.Empty, guarantee.PW_BondNumber);

			guarantee.PW_BondNumber = "AAA";
			AssertEquals("Guarantee AAA when it is joined.", "AAA", guarantee.PW_BondNumber);

			AssertEquals("Guarantee PW_BondNumber is Readonly", true, guarantee.PW_BondNumberInfo.ReadOnly);
			AssertEquals("Guarantee PW_BondAmount is Readonly", true, guarantee.PW_BondAmountInfo.ReadOnly);
		});
	}

	public void TestPackageQty()
	{
		var registerHeader = Factory.New<CusTempStorageRegHeader>();
		registerHeader.SRH_InternalReference = "DDT123456";

		CombineAssertions("", () =>
		{
			AssertEquals("PackageQty should be 0 when register is not bound to any temporary storage header.", 0, registerHeader.PackageQty);

			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			storageHeader.SJH_JobReference = "DDT123456";
			storageHeader.SJH_AppCode = "TES";

			var desc = Factory.New<CusTempStorageDec>();
			desc.STH_DeclarationType = "TES";
			desc.STH_SJH = storageHeader.PK;

			var line1 = desc.CusTempStorageLines.AddNew();
			line1.TSL_PackageQty = 10;
			line1.TSL_PackageType = "SB";
			var line2 = desc.CusTempStorageLines.AddNew();
			line2.TSL_PackageQty = 20;
			line2.TSL_PackageType = "NB";
			AssertEquals("PackageQty should return storage Header PackageCount.", 30, registerHeader.PackageQty);
		});
	}

	public void TestPackageType()
	{
		var registerHeader = Factory.New<CusTempStorageRegHeader>();
		registerHeader.SRH_InternalReference = "DDT123456";

		CombineAssertions("", () =>
		{
			AssertEquals("PackageType should be empty when register is not bound to any temporary storage header.", ZString.Empty, registerHeader.PackageType);

			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			storageHeader.SJH_JobReference = "DDT123456";
			storageHeader.SJH_AppCode = "TES";

			var desc = Factory.New<CusTempStorageDec>();
			desc.STH_DeclarationType = "TES";
			desc.STH_SJH = storageHeader.PK;

			var line1 = desc.CusTempStorageLines.AddNew();
			line1.TSL_PackageType = "SB";
			var line2 = desc.CusTempStorageLines.AddNew();
			line2.TSL_PackageType = "NB";
			AssertEquals("PackageType should return storage Header PackageType.", "Multiple", registerHeader.PackageType);

			line2.TSL_PackageType = "SB";
			AssertEquals("PackageType should return storage Header PackageType.", "SB", registerHeader.PackageType);
		});
	}

	public void TestLineCount()
	{
		var registerHeader = Factory.New<CusTempStorageRegHeader>();
		registerHeader.SRH_InternalReference = "DDT123456";
		CombineAssertions("", () =>
		{
			AssertEquals("PackageQty should be 0 when register is not bound to any temporary storage header.", 0, registerHeader.LineCount);

			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			storageHeader.SJH_JobReference = "DDT123456";
			storageHeader.SJH_AppCode = "TES";

			var desc = Factory.New<CusTempStorageDec>();
			desc.STH_DeclarationType = "TES";
			desc.STH_SJH = storageHeader.PK;

			var line1 = desc.CusTempStorageLines.AddNew();
			var line2 = desc.CusTempStorageLines.AddNew();
			AssertEquals("LineCount should return storage Header LineCount.", 2, registerHeader.LineCount);
		});
	}

	public void TestPackageQtyCaption() => CombineAssertions(() =>
	{
		AssertEntity<CusTempStorageRegHeader>()
			.HasProperty(h => h.PackageQty)
			.WithCaption("Package Qty");
	});

	public void TestPackageTypeCaption() => CombineAssertions(() =>
	{
		AssertEntity<CusTempStorageRegHeader>()
			.HasProperty(h => h.PackageType)
			.WithCaption("Package Type");
	});

	public void TestLineCountCaption() => CombineAssertions(() =>
	{
		AssertEntity<CusTempStorageRegHeader>()
			.HasProperty(h => h.LineCount)
			.WithCaption("Number of Lines");
	});

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

	BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var cusTempStorageRegHeader = factory.New<CusTempStorageRegHeader>();
		cusTempStorageRegHeader.SRH_ArrivalDate = ZDate.Today;
		cusTempStorageRegHeader.SRH_PresentationDate = ZDate.Today;
		cusTempStorageRegHeader.SRH_AppCode = "123";
		cusTempStorageRegHeader.SRH_Status = "OK";
		cusTempStorageRegHeader.SRH_Reference = "TEST";

		return cusTempStorageRegHeader;
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<CusTempStorageRegHeader>();
		header.SRH_AppCode = "123";
		header.SRH_Reference = "Reference";
	}
	CusTempStorageRegHeader header;
}
