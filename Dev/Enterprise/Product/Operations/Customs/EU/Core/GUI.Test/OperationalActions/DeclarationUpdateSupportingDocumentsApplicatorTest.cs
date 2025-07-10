using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.OperationalActions;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing.OperationalActions;

[TestedType(typeof(DeclarationUpdateSupportingDocumentsApplicator))]
public class DeclarationUpdateSupportingDocumentsApplicatorTest : OperationalActionMethodApplicatorTest
{
	public void TestDefaults()
	{
		AssertEquals(ZString.Empty, applicator.DocumentCode);
		AssertEquals(ZString.Empty, applicator.ReferenceNumber);
		AssertEquals(ZDateTime.Empty, applicator.Date);
		AssertEquals(false, applicator.OverrideExisitingDocument);
	}

	public void TestDocumentCodeList()
	{
		var documentCodeList = applicator.DocumentCodeList as ZZRefCusCodeListCombinedCollection;
		documentCodeList.Load();
		AssertEquals(0, applicator.DocumentCodeList.Count);

		var factory = new BusinessObjectFactory();
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "DC44I", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "DC44E", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		var cusCode1 = helper.CreateCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "TST1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		var cusCode2 = helper.CreateCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "TST2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		var cusCode3 = helper.CreateCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "TST3", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		var cusCode4 = helper.CreateCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "TST4", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateCusCodeListAttribute(cusCode1.PK, RefCusCodeListAttributeTypes.Codes.Level, UniversalReferenceConstants.RefCusCodeListAttributes.Values.Item);
		helper.CreateCusCodeListAttribute(cusCode2.PK, RefCusCodeListAttributeTypes.Codes.Level, UniversalReferenceConstants.RefCusCodeListAttributes.Values.Header);
		helper.CreateCusCodeListAttribute(cusCode3.PK, RefCusCodeListAttributeTypes.Codes.Level, UniversalReferenceConstants.RefCusCodeListAttributes.Values.Item);
		helper.CreateCusCodeListAttribute(cusCode4.PK, RefCusCodeListAttributeTypes.Codes.Level, UniversalReferenceConstants.RefCusCodeListAttributes.Values.Header);
		factory.Save();

		var applicator2 = new DeclarationUpdateSupportingDocumentsApplicator(factory);
		documentCodeList = applicator2.DocumentCodeList as ZZRefCusCodeListCombinedCollection;
		documentCodeList.Load();
		AssertEquals(2, documentCodeList.Count);
		AssertContainsExactElementsInAnyOrder(new ZString[] { "TST2", "TST4" }, documentCodeList.Select(x => x.ZZD_Code));
	}

	public void TestApply()
	{
		applicator.DocumentCode = "DC1";
		applicator.ReferenceNumber = "Reference1";
		applicator.Date = ZDateTime.BrettsBirthday;
		applicator.OverrideExisitingDocument = true;

		var log = new DummyOperationalActionSectionLog();
		var declaration1 = Factory.New<JobDeclaration>();
		applicator.Apply(log, new BusinessObject[] { declaration1 });

		AssertEquals(1, declaration1.SupportingDocuments.Count);
		var supportingDocument1 = declaration1.SupportingDocuments.Cast<SupportingDocument>().FirstOrDefault();
		AssertEquals("DC1", supportingDocument1.CSI_Code);
		AssertEquals("Reference1", supportingDocument1.CSI_ReferenceNumber);
		AssertEquals(ZDateTime.BrettsBirthday, supportingDocument1.CSI_DateOfIssue);

		applicator.ReferenceNumber = "Reference2";
		var declaration2 = Factory.New<JobDeclaration>();
		var supportingDocument2 = declaration2.SupportingDocuments.AddNew();
		supportingDocument2.CSI_Code = "DC1";

		var declaration3 = Factory.New<JobDeclaration>();
		var supportingDocument3 = declaration3.SupportingDocuments.AddNew();
		supportingDocument3.CSI_Code = "DC2";
		applicator.Apply(log, new BusinessObject[] { declaration1, declaration2, declaration3 });
		AssertEquals("Reference2", supportingDocument1.CSI_ReferenceNumber);
		AssertEquals(ZDateTime.BrettsBirthday, supportingDocument1.CSI_DateOfIssue);
		AssertEquals("Reference2", supportingDocument2.CSI_ReferenceNumber);
		AssertEquals(ZDateTime.BrettsBirthday, supportingDocument2.CSI_DateOfIssue);
		AssertEquals(ZString.Empty, supportingDocument3.CSI_ReferenceNumber);
		AssertEquals(ZDateTime.Empty, supportingDocument3.CSI_DateOfIssue);
	}

	protected override void SetUp()
	{
		base.SetUp();
		applicator = new DeclarationUpdateSupportingDocumentsApplicator(Factory);
	}
	DeclarationUpdateSupportingDocumentsApplicator applicator;
}
