using System;
using System.Collections.Generic;
using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappersCore.Testing
{
	public class DocImageSupportWrapperTests : TestCaseWithDummy
	{
		public void TestPrincipalBrandingRegistryItemDoesntOverrideWhenFlagIsSet()
		{
			DocumentsDataRegistry.Instance.EnableAgentBranding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, true);

			wrapper.SetTemplateConstants(new Dictionary<string, object>
			{
				{ Constants.TemplateDefined.ContactType, "FWD" },
				{ Constants.TemplateDefined.BrandedOrganisationPK, brandedOrg.PK.ToString() },
			});

			brandedOrg.MiscServ.OM_FWAgentCategory = "GNR";

			GlbStaff.CurrentUser.GS_EmailAddress = "bob@notgeneric.com";
			GlbStaff.CurrentUser.GS_PublishEmailAddress = ZBool.True;

			AssertEquals("Brand EmailAddress from registry", "bob@generic.com", wrapper.BrandEmailAddress);
			wrapper.DocumentBrandingObjectExposed.ReplaceDomainNames = false;
			AssertEquals("Brand EmailAddress from registry", "bob@notgeneric.com", wrapper.BrandEmailAddress);
		}

		public void TestConstantsGetReWritten()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DocBaseWrapperBaseWithImageSupportTestClass testWrapper = new DocBaseWrapperBaseWithImageSupportTestClass(factory.NewWithValidTestData<DummyBusinessObject>(), factory);

			((IBODocDataProvider)testWrapper).SetDocWrapperContext(new Dictionary<string, object>
																	   {
																		   { Constants.TemplateDefined.ContactType, "CNR" },
																		   { Constants.TemplateDefined.ReportName, "First report" },
																		   { Constants.TemplateDefined.DocumentDirection, "EXP" },
																		   { Constants.TemplateDefined.MenuTitle, "First Menu Title" },
																		   { Constants.TemplateDefined.DeliveryMode, "DLV" }
																	   });

			AssertEquals(testWrapper.DocumentContactType.Code, "CNR");
			AssertEquals(testWrapper.ReportName, "First report");
			AssertEquals(testWrapper.DocumentDirection, "EXP");
			AssertEquals(testWrapper.MenuTitle, "First Menu Title");
			AssertEquals(testWrapper.DocumentDeliveryMode, "DLV");

			factory.GetDocWrapperContextManager().SetupDocWrapperContextFromDocumentPack("", ZGuid.Empty, "", "CNE", null, null);

			((IBODocDataProvider)testWrapper).SetDocWrapperContext(new Dictionary<string, object>
																	   {
																		   { Constants.TemplateDefined.ReportName, "Second report" },
																	   });

			AssertEquals(testWrapper.DocumentContactType.Code, "CNE");
			AssertEquals(testWrapper.ReportName, "Second report");
			Assert(testWrapper.DocumentDirection.IsEmpty);
			Assert(testWrapper.MenuTitle.IsEmpty);
			Assert(testWrapper.DocumentDeliveryMode.IsEmpty);
		}

		public void TestConstantsGetReWrittenForMultipleWrappers()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			DocBaseWrapperBaseWithImageSupportTestClass testWrapper1 = new DocBaseWrapperBaseWithImageSupportTestClass(factory.NewWithValidTestData<DummyBusinessObject>(), factory);
			ZGuid contactOrganisationPK1 = ZGuid.NewZGuid();

			DocBaseWrapperBaseWithImageSupportTestClass testWrapper2 = new DocBaseWrapperBaseWithImageSupportTestClass(factory.NewWithValidTestData<DummyBusinessObject>(), factory);
			ZGuid contactOrganisationPK2 = ZGuid.NewZGuid();

			((IBODocDataProvider)testWrapper1).SetDocWrapperContext(new Dictionary<string, object>
																	   {
																		{ Constants.TemplateDefined.ContactOrganisationPK, contactOrganisationPK1 }
																	   });

			((IBODocDataProvider)testWrapper2).SetDocWrapperContext(new Dictionary<string, object>
																	   {
																		{ Constants.TemplateDefined.ContactOrganisationPK, contactOrganisationPK2 }
																	   });

			CombineAssertions(delegate
			{
				AssertEquals(contactOrganisationPK1, testWrapper1.DocWrapperContextContactOrganisationPK_Exposed());
				AssertEquals(contactOrganisationPK2, testWrapper2.DocWrapperContextContactOrganisationPK_Exposed());
			});
		}

		public void TestChildDocumentWrapperInASameFactoryGetsSameDocumentContext()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			DocBaseWrapperBaseWithImageSupportTestClass testWrapper1 = new DocBaseWrapperBaseWithImageSupportTestClass(factory1.NewWithValidTestData<DummyBusinessObject>(), factory1);
			DocBaseWrapperBaseWithImageSupportTestClass childWrapper1 = new DocBaseWrapperBaseWithImageSupportTestClass(factory1.NewWithValidTestData<DummyBusinessObject>(), factory1);

			DocBaseWrapperBaseWithImageSupportTestClass testWrapper2 = new DocBaseWrapperBaseWithImageSupportTestClass(factory2.NewWithValidTestData<DummyBusinessObject>(), factory2);
			DocBaseWrapperBaseWithImageSupportTestClass childWrapper2 = new DocBaseWrapperBaseWithImageSupportTestClass(factory2.NewWithValidTestData<DummyBusinessObject>(), factory2);

			factory1.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);
			factory2.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);

			AssertNull(testWrapper1.DocumentContactType);
			Assert(testWrapper1.ReportName.IsEmpty);
			Assert(testWrapper1.DocumentDirection.IsEmpty);
			Assert(testWrapper1.MenuTitle.IsEmpty);
			Assert(testWrapper1.DocumentDeliveryMode.IsEmpty);

			AssertNull(childWrapper1.DocumentContactType);
			Assert(childWrapper1.ReportName.IsEmpty);
			Assert(childWrapper1.DocumentDirection.IsEmpty);
			Assert(childWrapper1.MenuTitle.IsEmpty);
			Assert(childWrapper1.DocumentDeliveryMode.IsEmpty);

			AssertNull(testWrapper2.DocumentContactType);
			Assert(testWrapper2.ReportName.IsEmpty);
			Assert(testWrapper2.DocumentDirection.IsEmpty);
			Assert(testWrapper2.MenuTitle.IsEmpty);
			Assert(testWrapper2.DocumentDeliveryMode.IsEmpty);

			AssertNull(childWrapper2.DocumentContactType);
			Assert(childWrapper2.ReportName.IsEmpty);
			Assert(childWrapper2.DocumentDirection.IsEmpty);
			Assert(childWrapper2.MenuTitle.IsEmpty);
			Assert(childWrapper2.DocumentDeliveryMode.IsEmpty);

			testWrapper1.SetTemplateConstants(new Dictionary<string, object>(new Dictionary<string, object>
																					 {
																						 { Constants.TemplateDefined.ContactType, "CNR" },
																						 { Constants.TemplateDefined.ReportName, "First report" },
																						 { Constants.TemplateDefined.DocumentDirection, "EXP" },
																						 { Constants.TemplateDefined.MenuTitle, "First Menu Title" },
																						 { Constants.TemplateDefined.DeliveryMode, "DLV" }
																					 }));

			AssertEquals(testWrapper1.DocumentContactType.Code, "CNR");
			AssertEquals(testWrapper1.ReportName, "First report");
			AssertEquals(testWrapper1.DocumentDirection, "EXP");
			AssertEquals(testWrapper1.MenuTitle, "First Menu Title");
			AssertEquals(testWrapper1.DocumentDeliveryMode, "DLV");

			AssertEquals(childWrapper1.DocumentContactType.Code, "CNR");
			AssertEquals(childWrapper1.ReportName, "First report");
			AssertEquals(childWrapper1.DocumentDirection, "EXP");
			AssertEquals(childWrapper1.MenuTitle, "First Menu Title");
			AssertEquals(childWrapper1.DocumentDeliveryMode, "DLV");

			AssertNull(testWrapper2.DocumentContactType);
			Assert(testWrapper2.ReportName.IsEmpty);
			Assert(testWrapper2.DocumentDirection.IsEmpty);
			Assert(testWrapper2.MenuTitle.IsEmpty);
			Assert(testWrapper2.DocumentDeliveryMode.IsEmpty);

			AssertNull(childWrapper2.DocumentContactType);
			Assert(childWrapper2.ReportName.IsEmpty);
			Assert(childWrapper2.DocumentDirection.IsEmpty);
			Assert(childWrapper2.MenuTitle.IsEmpty);
			Assert(childWrapper2.DocumentDeliveryMode.IsEmpty);

			testWrapper2.SetTemplateConstants(new Dictionary<string, object>(new Dictionary<string, object>
																					 {
																						 { Constants.TemplateDefined.ContactType, "CNE" },
																						 { Constants.TemplateDefined.ReportName, "Second report" },
																						 { Constants.TemplateDefined.DocumentDirection, "IMP" },
																						 { Constants.TemplateDefined.MenuTitle, "Second Menu Title" },
																						 { Constants.TemplateDefined.DeliveryMode, "DLV" }
																					 }));

			AssertEquals(testWrapper1.DocumentContactType.Code, "CNR");
			AssertEquals(testWrapper1.ReportName, "First report");
			AssertEquals(testWrapper1.DocumentDirection, "EXP");
			AssertEquals(testWrapper1.MenuTitle, "First Menu Title");
			AssertEquals(testWrapper1.DocumentDeliveryMode, "DLV");

			AssertEquals(childWrapper1.DocumentContactType.Code, "CNR");
			AssertEquals(childWrapper1.ReportName, "First report");
			AssertEquals(childWrapper1.DocumentDirection, "EXP");
			AssertEquals(childWrapper1.MenuTitle, "First Menu Title");
			AssertEquals(childWrapper1.DocumentDeliveryMode, "DLV");

			AssertEquals(testWrapper2.DocumentContactType.Code, "CNE");
			AssertEquals(testWrapper2.ReportName, "Second report");
			AssertEquals(testWrapper2.DocumentDirection, "IMP");
			AssertEquals(testWrapper2.MenuTitle, "Second Menu Title");
			AssertEquals(testWrapper2.DocumentDeliveryMode, "DLV");

			AssertEquals(childWrapper2.DocumentContactType.Code, "CNE");
			AssertEquals(childWrapper2.ReportName, "Second report");
			AssertEquals(childWrapper2.DocumentDirection, "IMP");
			AssertEquals(childWrapper2.MenuTitle, "Second Menu Title");
			AssertEquals(childWrapper2.DocumentDeliveryMode, "DLV");

			testWrapper1.SetTemplateConstants(null);

			AssertEquals(testWrapper1.DocumentContactType.Code, "CNR");
			AssertEquals(testWrapper1.ReportName, "First report");
			AssertEquals(testWrapper1.DocumentDirection, "EXP");
			AssertEquals(testWrapper1.MenuTitle, "First Menu Title");
			AssertEquals(testWrapper1.DocumentDeliveryMode, "DLV");

			AssertEquals(childWrapper1.DocumentContactType.Code, "CNR");
			AssertEquals(childWrapper1.ReportName, "First report");
			AssertEquals(childWrapper1.DocumentDirection, "EXP");
			AssertEquals(childWrapper1.MenuTitle, "First Menu Title");
			AssertEquals(childWrapper1.DocumentDeliveryMode, "DLV");

			AssertEquals(testWrapper2.DocumentContactType.Code, "CNE");
			AssertEquals(testWrapper2.ReportName, "Second report");
			AssertEquals(testWrapper2.DocumentDirection, "IMP");
			AssertEquals(testWrapper2.MenuTitle, "Second Menu Title");
			AssertEquals(testWrapper2.DocumentDeliveryMode, "DLV");

			AssertEquals(childWrapper2.DocumentContactType.Code, "CNE");
			AssertEquals(childWrapper2.ReportName, "Second report");
			AssertEquals(childWrapper2.DocumentDirection, "IMP");
			AssertEquals(childWrapper2.MenuTitle, "Second Menu Title");
			AssertEquals(childWrapper2.DocumentDeliveryMode, "DLV");

			testWrapper2.SetTemplateConstants(new Dictionary<string, object>());

			AssertEquals(testWrapper1.DocumentContactType.Code, "CNR");
			AssertEquals(testWrapper1.ReportName, "First report");
			AssertEquals(testWrapper1.DocumentDirection, "EXP");
			AssertEquals(testWrapper1.MenuTitle, "First Menu Title");
			AssertEquals(testWrapper1.DocumentDeliveryMode, "DLV");

			AssertEquals(childWrapper1.DocumentContactType.Code, "CNR");
			AssertEquals(childWrapper1.ReportName, "First report");
			AssertEquals(childWrapper1.DocumentDirection, "EXP");
			AssertEquals(childWrapper1.MenuTitle, "First Menu Title");
			AssertEquals(childWrapper1.DocumentDeliveryMode, "DLV");

			AssertEquals(testWrapper2.DocumentContactType.Code, "CNE");
			AssertEquals(testWrapper2.ReportName, "Second report");
			AssertEquals(testWrapper2.DocumentDirection, "IMP");
			AssertEquals(testWrapper2.MenuTitle, "Second Menu Title");
			AssertEquals(testWrapper2.DocumentDeliveryMode, "DLV");

			AssertEquals(childWrapper2.DocumentContactType.Code, "CNE");
			AssertEquals(childWrapper2.ReportName, "Second report");
			AssertEquals(childWrapper2.DocumentDirection, "IMP");
			AssertEquals(childWrapper2.MenuTitle, "Second Menu Title");
			AssertEquals(childWrapper2.DocumentDeliveryMode, "DLV");
		}

		public void TestBrandName_NullUserContext()
		{
			AssertEquals("Default brand name with UserContext.", GlbCompany.CurrentCompany.GC_Name.ToUpper(), wrapper.BrandName);
			using (Env.SetTemporaryUserContext(null))
			{
				var result = "should be empty";
				AssertNoExceptionThrown(() => result = wrapper.BrandName);
				AssertEquals("Default brand name without UserContext.", "", result);
			}
		}

		#region Images

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCompanyLogoWithClientBranding()
		{
			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestImage);
			AssertEquals("Default is SystemDataRegistry.Instance.CompanyLogo.Value", TestImage.Size, wrapper.CompanyLogo.Size);
			AssertEquals("CompanyLogo(Disposed: False)", wrapper.CompanyLogo.Tag);

			contactOrg.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 11);
			constants.Add(Constants.TemplateDefined.ContactType, "CNE");
			constants.Add(Constants.TemplateDefined.ContactOrganisationPK, contactOrg.PK.ToString());
			wrapper.SetTemplateConstants(constants);
			DocumentsDataRegistry.Instance.EnableClientBranding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, true);
			AssertEquals("Brand image from client's tariff level", new Size(1, 1), wrapper.CompanyLogo.Size);
			AssertEquals("TariffAndLevelRegistry(Disposed: False)", wrapper.CompanyLogo.Tag);

			brandedOrg.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 22);
			constants.Add(Constants.TemplateDefined.BrandedOrganisationPK, brandedOrg.PK.ToString());
			wrapper.SetTemplateConstants(constants);
			AssertEquals("Brand image from contact's tariff level", new Size(2, 2), wrapper.CompanyLogo.Size);

			DocumentsDataRegistry.Instance.EnableClientBranding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, false);
			wrapper.SetBranchImage(new Bitmap(3, 3));
			AssertEquals("Brand image from branch logo", new Size(3, 3), wrapper.CompanyLogo.Size);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCompanyLogoWithAgentBranding()
		{
			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestImage);
			AssertEquals("Default is SystemDataRegistry.Instance.CompanyLogo", TestImage.Size, wrapper.CompanyLogo.Size);

			contactOrg.MiscServ.OM_FWAgentCategory = "STD";
			constants.Add(Constants.TemplateDefined.ContactType, "FWD");
			constants.Add(Constants.TemplateDefined.ContactOrganisationPK, contactOrg.PK.ToString());
			wrapper.SetTemplateConstants(constants);
			DocumentsDataRegistry.Instance.EnableAgentBranding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, true);

			AssertEquals("Brand image from registry", new Size(5, 5), wrapper.CompanyLogo.Size);
			AssertEquals("AgentBrand(Disposed: False)", wrapper.CompanyLogo.Tag);

			contactOrg.MiscServ.OM_FWAgentCategory = "ZZZ";
			AssertEquals("Agent code doesnt have brand image. Get Brand image from default logo", TestImage.Size, wrapper.CompanyLogo.Size);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCompanyLogoWithAdditionalBranding()
		{
			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestImage);
			AssertEquals("Default company logo without branding.", TestImage.Size, wrapper.CompanyLogo.Size);

			var branding = new PrincipalBranding();
			branding.Image = new Bitmap(3, 3);

			wrapper.AlternativeBrandingExposed = branding;
			AssertNotEquals("Precondition: alternate image size does not equal to default image size.", TestImage.Size, branding.Image.Size);
			AssertEquals("Alternative company logo.", branding.Image.Size, wrapper.CompanyLogo.Size);
			AssertEquals("AlternativeBranding(Disposed: False)", wrapper.CompanyLogo.Tag);
		}

		#endregion

		#region Brand Name

		public void TestBrandNameAndEmailAddressForAdditionalBranding()
		{
			AssertEquals("Default brand name without branding.", GlbCompany.CurrentCompany.GC_Name.ToUpper(), wrapper.BrandName);
			AssertEquals("Default brand email without branding.", GlbStaff.CurrentUser.GS_EmailAddress, wrapper.BrandEmailAddress);

			var branding = new PrincipalBranding();
			branding.BrandName = "My Brand";
			branding.BrandEmailAddress = "info@mybrand.com";

			wrapper.AlternativeBrandingExposed = branding;
			AssertNotEquals("Precondition: alternate brand name does not equal to default brand name.", GlbCompany.CurrentCompany.GC_Name.ToUpper(), branding.BrandName);
			AssertNotEquals("Precondition: alternate brand email does not equal to default brand email.", GlbStaff.CurrentUser.GS_EmailAddress, branding.BrandEmailAddress);
			AssertEquals("Alternative brand name.", branding.BrandName.ToUpper(), wrapper.BrandName);
			AssertEquals("Alternative brand email.", branding.BrandEmailAddress, wrapper.BrandEmailAddress);
		}

		public void TestEmptyBrandingRegistryItemReturnStaffDefaultEmailAddress()
		{
			GlbStaff.CurrentUser.GS_EmailAddress = "bob@notgeneric.com";
			GlbStaff.CurrentUser.GS_PublishEmailAddress = ZBool.True;

			var branding = new PrincipalBranding();
			branding.BrandName = "My Brand";
			branding.BrandEmailAddress = "";

			wrapper.AlternativeBrandingExposed = branding;

			AssertEquals("If brand email address empty, current staff email shoudl be return.", GlbStaff.CurrentUser.GS_EmailAddress, wrapper.BrandEmailAddress);
		}

		public void TestBrandNameAndEmailAddressForAgent()
		{
			DocumentsDataRegistry.Instance.EnableAgentBranding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, false);
			ZString expectedBrandName = GlbCompany.CurrentCompany.GC_Name.ToString();
			AssertEquals("Default Brand Name", expectedBrandName.ToUpper(), wrapper.BrandName.ToUpper());
			AssertEquals("Default Brand EmailAddress", GlbStaff.CurrentUser.GS_EmailAddress, wrapper.BrandEmailAddress);

			contactOrg.MiscServ.OM_FWAgentCategory = "STD";
			constants.Add(Constants.TemplateDefined.ContactType, "FWD");
			constants.Add(Constants.TemplateDefined.ContactOrganisationPK, contactOrg.PK.ToString());
			wrapper.SetTemplateConstants(constants);
			DocumentsDataRegistry.Instance.EnableAgentBranding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, true);
			AssertEquals("Brand Name from registry", "BLAH1", wrapper.BrandName);
			AssertEquals("Brand EmailAddress from registry", "blah1@blah.com", wrapper.BrandEmailAddress);

			brandedOrg.MiscServ.OM_FWAgentCategory = "11";
			constants.Add(Constants.TemplateDefined.BrandedOrganisationPK, brandedOrg.PK.ToString());
			wrapper.SetTemplateConstants(constants);
			AssertEquals("Brand Name from registry", "BLAH2", wrapper.BrandName);
			AssertEquals("Brand EmailAddress from registry", "blah2@blah.com", wrapper.BrandEmailAddress);

			brandedOrg.MiscServ.OM_FWAgentCategory = "GNR";
			GlbStaff.CurrentUser.GS_EmailAddress = "bob@blah.com";
			GlbStaff.CurrentUser.GS_PublishEmailAddress = ZBool.True;
			AssertEquals("Brand EmailAddress from registry", "bob@generic.com", wrapper.BrandEmailAddress);
		}

		public void TestBrandNameForTariffAndLevel()
		{
			DocumentsDataRegistry.Instance.EnableClientBranding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, false);
			ZString expectedBrandName = GlbCompany.CurrentCompany.GC_Name.ToString();
			AssertEquals("Default Brand Name", expectedBrandName.ToUpper(), wrapper.BrandName.ToUpper());
			AssertEquals("Default Brand EmailAddress", GlbStaff.CurrentUser.GS_EmailAddress, wrapper.BrandEmailAddress);

			contactOrg.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 22);
			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(Constants.TemplateDefined.ContactType, "CNE");
			constants.Add(Constants.TemplateDefined.ContactOrganisationPK, contactOrg.PK.ToString());
			wrapper.SetTemplateConstants(constants);
			DocumentsDataRegistry.Instance.EnableClientBranding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, true);
			AssertEquals("Brand Name from registry", "BLAH2", wrapper.BrandName);
			AssertEquals("Brand EmailAddress from registry", "blah2@blah.com", wrapper.BrandEmailAddress);

			brandedOrg.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 11);
			constants.Add(Constants.TemplateDefined.BrandedOrganisationPK, brandedOrg.PK.ToString());
			wrapper.SetTemplateConstants(constants);
			AssertEquals("Brand Name from registry", "BLAH1", wrapper.BrandName);
			AssertEquals("Brand EmailAddress from registry", "blah1@blah.com", wrapper.BrandEmailAddress);

			brandedOrg.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 33);
			GlbStaff.CurrentUser.GS_EmailAddress = "bob@blah.com";
			GlbStaff.CurrentUser.GS_PublishEmailAddress = ZBool.True;
			AssertEquals("Brand EmailAddress from registry", "bob@generic.com", wrapper.BrandEmailAddress);
		}

		#endregion

		#region TestDocumentBrandingBusinessObjectWithAliveData

		public void TestDocumentBrandingBusinessObjectWithAliveData()
		{
			DocumentsDataRegistry.Instance.EnableClientBranding.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			contactOrg.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 11);
			constants.Add(Constants.TemplateDefined.ContactType, "CNE");
			constants.Add(Constants.TemplateDefined.ContactOrganisationPK, contactOrg.PK.ToString());
			wrapper.SetTemplateConstants(constants);

			ClientAndAgentBrandingBusinessObject doc1 = wrapper.DocumentBrandingObjectExposed;
			AssertNotNull(doc1);
			AssertEquals(1, doc1.Image.Width);

			ClientAndAgentBrandingBusinessObject doc2 = wrapper.DocumentBrandingObjectExposed;
			AssertNotNull(doc2);
			AssertEquals(1, doc2.Image.Width);
			AssertEquals("Should return cached object", doc1.PK, doc2.PK);

			doc1.Image.Dispose();

			ClientAndAgentBrandingBusinessObject doc3 = wrapper.DocumentBrandingObjectExposed;
			AssertNotNull(doc3);
			AssertEquals(1, doc3.Image.Width);
			AssertNotEquals("Should return new object", doc1.PK, doc3.PK);
		}

		#endregion

		#region Implementation

		Dictionary<string, object> constants;
		DocBaseWrapperBaseWithImageSupportTestClass wrapper;
		Image testImage;
		OrgHeader contactOrg;
		OrgHeader brandedOrg;

		protected override void SetUp()
		{
			base.SetUp();
			contactOrg = Factory.NewWithValidTestData<OrgHeader>();
			brandedOrg = Factory.NewWithValidTestData<OrgHeader>();

			DummyBusinessObject dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			wrapper = new DocBaseWrapperBaseWithImageSupportTestClass(dummy, Factory);
			constants = new Dictionary<string, object>();

			BrandingTestHelperClass.SetClientBrandRegistryImage();
			BrandingTestHelperClass.SetAgentBrandRegistryImage();

			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);
		}

		Image TestImage
		{
			get
			{
				if (testImage == null)
				{
					var resourceRetriever = new EmbeddedResourceRetriever(typeof(DocImageSupportWrapperTests).Assembly);
					testImage = Image.FromStream(resourceRetriever.GetStream("Enterprise.DocumentWrappersCore.Test.TestResources.TransparentPNG.png"));
				}
				return testImage;
			}
		}

		#region Test Classes

		public class DocBaseWrapperBaseWithImageSupportTestClass : DocBaseWrapperBaseWithImageSupport
		{
			public DocBaseWrapperBaseWithImageSupportTestClass(BusinessObject bizObj, BusinessObjectFactory factoryToWrap)
				: base(bizObj, factoryToWrap)
			{
			}

			public override string ToString()
			{
				return ZString.Empty;
			}

			protected override Image GetCompanyLogoFallback()
			{
				if (fBranchImage != null)
				{
					return fBranchImage;
				}
				else
				{
					return base.GetCompanyLogoFallback();
				}
			}

			protected Image fBranchImage;
			public void SetBranchImage(Image imageToSet)
			{
				fBranchImage = imageToSet;
			}

			public ClientAndAgentBrandingBusinessObject DocumentBrandingObjectExposed
			{
				get { return DocumentBrandingObject; }
			}

			protected override ClientAndAgentBrandingBusinessObject AlternativeBranding
			{
				get { return AlternativeBrandingExposed; }
			}

			public DocumentBrandingBusinessObject AlternativeBrandingExposed { get; set; }
		}

		#endregion

		#endregion
	}
}
