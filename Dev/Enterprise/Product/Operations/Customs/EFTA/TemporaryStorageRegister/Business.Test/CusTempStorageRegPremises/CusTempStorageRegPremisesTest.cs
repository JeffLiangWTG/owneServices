using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(CusTempStorageRegPremises))]
sealed class CusTempStorageRegPremisesTest : EnterpriseBusinessObjectTestCase
{
	public void TestValidation_Type()
	{
		AssertType<CusTempStorageRegPremisesValidation>(premises.Validation);
	}

	public void TestLookup_Type()
	{
		AssertType<CusTempStorageRegPremisesLookups>(premises.Lookups);
	}

	public void TestTypeDescription()
	{
		premises.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility;
		AssertEquals(CusTempStorageRegPremisesTypeList.Descriptions.ExportStorageFacility, premises.TypeDescription);
	}

	public void TestCaptionSRP_Code_Attributes() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegPremises>()
			.HasProperty(p => p.SRP_Code)
			.WithCaption("Code");
	});

	public void TestCaptionSRP_Type_Attributes() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegPremises>()
			.HasProperty(p => p.SRP_Type)
			.WithCaption("Type")
			.WithList($"{nameof(CusTempStorageRegPremises.Lookups)}.{nameof(CusTempStorageRegPremisesLookups.TypeList)}");
	});

	public void TestSRP_Description_Attributes() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegPremises>()
			.HasProperty(p => p.SRP_Description)
			.WithCaption("Description")
			.WithShortCaption("Desc.")
			.WithMediumCaption("Description")
			.WithFullDescription("Description for Premises");
	});

	public void TestSRP_OA_PremisesAddress_Attributes() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegPremises>()
			.HasProperty(p => p.SRP_OA_PremisesAddress)
			.WithCaption("Address")
			.WithShortCaption("Addr.")
			.WithMediumCaption("Address")
			.WithFullDescription("Address for Premises");
	});

	public void TestTypeDescription_Attributes() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegPremises>()
			.HasProperty(p => p.TypeDescription)
			.WithCaption("Type Description");
	});

	public void TestCaptionSRP_CustomsLocation_Attributes() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegPremises>()
			.HasProperty(p => p.SRP_CustomsLocation)
			.WithCaption("Location");
	});

	public void TestHumanReadableName()
	{
		var header = (CusTempStorageRegPremises)GetNewBusinessObject();
		header.SRP_Code = "X";
		AssertEquals("Code X", header.HumanReadableName);
	}

	public void TestCustomsNumberProvider()
	{
		var provider = ((ICustomsNumberViewStmNumsParent)premises).CustomsNumberProvider;
		AssertType<DefaultTSCustomsNumberViewStmNumsBusinessProvider>("Default Provider",provider);
	}

	public void TestGetEDocsProviderSupporter()
	{
		AssertType<EDocsProviderSupporter>(premises.GetEDocsProviderSupporter());
	}

	public void TestDocumentSupporter()
	{
		var supporter = premises.DocumentSupporter;
		CombineAssertions(() =>
		{
			AssertType<CusTempStorageRegPremisesDocumentSupporter>("Type", supporter);
			AssertSame("Cached", supporter, premises.DocumentSupporter);
		});
	}

	public void TestDocManagerInfo()
	{
		var manager = premises.DocManagerInfo;
		CombineAssertions(() =>
		{
			AssertType<DocManagerInfo>("Type", manager);
			AssertSame("Cached", manager, premises.DocManagerInfo);
		});
	}

	public void TestCodeProperty()
	{
		premises.SRP_Code = "X";
		premises.SRP_Description = "DESC";
		premises.SRP_CustomsLocation = "LOCATION";
		AssertEquals("CodeProperty(CusTempStorageRegPremises.Schema.SRP_CustomsLocation) - should be Location", "LOCATION", CodePropertyAttribute.CodeFromBusinessObject(premises));
		AssertEquals("DescriptionProperty(CusTempStorageRegPremises.Schema.SRP_Code) - should be Code", "X", DescriptionPropertyAttribute.DescriptionFromBusinessObject(premises));
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetPremises(factory);

	static CusTempStorageRegPremises GetPremises(BusinessObjectFactory factory)
	{
		var cusTempStorageRegPremises = factory.New<CusTempStorageRegPremises>();
		cusTempStorageRegPremises.SRP_Code = "X";
		cusTempStorageRegPremises.SRP_Description = "DESC";

		var orgHeader = factory.New<OrgHeader>();
		orgHeader.OH_Code = "AAA";
		var orgAddress = factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		cusTempStorageRegPremises.SRP_OA_PremisesAddress = orgAddress.PK;

		return cusTempStorageRegPremises;
	}

	protected override void SetUp()
	{
		premises = Factory.New<CusTempStorageRegPremises>();
	}

	CusTempStorageRegPremises premises;
}
