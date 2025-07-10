using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.DocumentEngineCore.DocWrappers.Testing
{
	sealed class DocWrapperContextTest : TestCaseWithFactory
	{
		public void TestMergeConstants()
		{
			Dictionary<string, object> constants = new Dictionary<string, object>
													{
														{ Constants.TemplateDefined.ContactType, "CONTYPE" },
														{ Constants.TemplateDefined.DocumentDirection, "EXP" },
														{ Constants.TemplateDefined.MenuTitle, "A menu title" },
													};

			Dictionary<string, object> constants2 = new Dictionary<string, object>
													{
														{ Constants.TemplateDefined.ContactType, "CONTYPE" },
														{ Constants.TemplateDefined.ReportName, "A report name" },
														{ Constants.TemplateDefined.DocumentDirection, "IMP" },
													};

			DocWrapperContext docWrapperContext = new DocWrapperContext(constants);
			docWrapperContext.MergeConstants(constants2);

			AssertEquals("ContactType", "CONTYPE", docWrapperContext.DocumentContactTypeCode);
			AssertEquals("ReportName", "A report name", docWrapperContext.ReportName);
			AssertEquals("MenuTitle", "A menu title", docWrapperContext.MenuTitle);
			AssertEquals("DocumentDirection", "IMP", docWrapperContext.DocumentDirection);
		}

		public void TestDocumentContactTypeCode()
		{
			ZGuid contactOrganisationPK = ZGuid.NewZGuid();
			ZGuid menuItemPK = ZGuid.NewZGuid();
			Dictionary<string, object> constants = new Dictionary<string, object>
													{
														{ Constants.TemplateDefined.ContactType, "CONTYPE" },
														{ Constants.TemplateDefined.ContactOrganisationPK, contactOrganisationPK },
														{ Constants.TemplateDefined.ReportName, "A report name" },
														{ Constants.TemplateDefined.DocumentDirection, "EXP" },
														{ Constants.TemplateDefined.MenuTitle, "A menu title" },
														{ Constants.TemplateDefined.MenuItemPK, menuItemPK },
														{ Constants.TemplateDefined.DeliveryMode, "DLV" }
													};

			DocWrapperContext docWrapperContext = new DocWrapperContext(constants);

			AssertEquals("DocumentContactTypeCode", "CONTYPE", docWrapperContext.DocumentContactTypeCode);
			AssertEquals("ContactOrganisationPK", contactOrganisationPK, docWrapperContext.ContactOrganisationPK);
			AssertEquals("ReportName", "A report name", docWrapperContext.ReportName);
			AssertEquals("DocumentDirection", "EXP", docWrapperContext.DocumentDirection);
			AssertEquals("MenuTitle", "A menu title", docWrapperContext.MenuTitle);
			AssertEquals("MenuItemPK", menuItemPK, docWrapperContext.MenuItemPK);
			AssertEquals("DeliveryMode", "DLV", docWrapperContext.DocumentDeliveryMode);

			docWrapperContext.ReWriteConstants(new Dictionary<string, object>());

			Assert("DocumentContactTypeCode", docWrapperContext.DocumentContactTypeCode.IsEmpty);
			Assert("ContactOrganisationPK", docWrapperContext.ContactOrganisationPK.IsEmpty);
			Assert("ReportName", docWrapperContext.ReportName.IsEmpty);
			Assert("DocumentDirection", docWrapperContext.DocumentDirection.IsEmpty);
			Assert("MenuTitle", docWrapperContext.MenuTitle.IsEmpty);
			Assert("MenuItemPK", docWrapperContext.MenuItemPK.IsEmpty);
			Assert("DeliveryMode", docWrapperContext.DocumentDeliveryMode.IsEmpty);

			docWrapperContext.ReWriteConstants(new Dictionary<string, object> { { Constants.TemplateDefined.ContactType, "CONTYPE2" } });

			AssertEquals("DocumentContactTypeCode", "CONTYPE2", docWrapperContext.DocumentContactTypeCode);
			Assert("ContactOrganisationPK", docWrapperContext.ContactOrganisationPK.IsEmpty);
			Assert("ReportName", docWrapperContext.ReportName.IsEmpty);
			Assert("DocumentDirection", docWrapperContext.DocumentDirection.IsEmpty);
			Assert("MenuTitle", docWrapperContext.MenuTitle.IsEmpty);
			Assert("MenuItemPK", docWrapperContext.MenuItemPK.IsEmpty);
			Assert("DeliveryMode", docWrapperContext.DocumentDeliveryMode.IsEmpty);

			docWrapperContext.ReWriteConstants(null);

			Assert("DocumentContactTypeCode", docWrapperContext.DocumentContactTypeCode.IsEmpty);
			Assert("ContactOrganisationPK", docWrapperContext.ContactOrganisationPK.IsEmpty);
			Assert("ReportName", docWrapperContext.ReportName.IsEmpty);
			Assert("DocumentDirection", docWrapperContext.DocumentDirection.IsEmpty);
			Assert("MenuTitle", docWrapperContext.MenuTitle.IsEmpty);
			Assert("MenuItemPK", docWrapperContext.MenuItemPK.IsEmpty);
			Assert("DeliveryMode", docWrapperContext.DocumentDeliveryMode.IsEmpty);
		}

		public void TestGetTemplateConstantValue()
		{
			var constants = new Dictionary<string, object>
													{
														{ "BoolValue", "true" },
														{ "ZBoolValue", "Y" },
														{ "ZStringValue", "abcd" },
														{ "ZIntValue", "123" },
													};

			var docWrapperContext = new DocWrapperContext(constants);
			bool isFound;

			AssertEquals(true, docWrapperContext.GetTemplateConstantValue<bool>("BoolValue", out isFound));
			Assert(isFound);
			AssertEquals(true, docWrapperContext.GetTemplateConstantValue<ZBool>("ZBoolValue", out isFound));
			Assert(isFound);

			AssertEquals("abcd", docWrapperContext.GetTemplateConstantValue<string>("ZStringValue", out isFound));
			Assert(isFound);
			AssertEquals("abcd", docWrapperContext.GetTemplateConstantValue<ZString>("ZStringValue", out isFound));
			Assert(isFound);

			AssertEquals(123, docWrapperContext.GetTemplateConstantValue<int>("ZIntValue", out isFound));
			Assert(isFound);
			AssertEquals(123, docWrapperContext.GetTemplateConstantValue<ZInt>("ZIntValue", out isFound));
			Assert(isFound);

			AssertEquals(0, docWrapperContext.GetTemplateConstantValue<ZInt>("NotExistValue", out isFound));
			Assert(!isFound);
			AssertEquals(false, docWrapperContext.GetTemplateConstantValue<ZBool>("NotExistValue", out isFound));
			Assert(!isFound);
			AssertEquals(string.Empty, docWrapperContext.GetTemplateConstantValue<ZString>("NotExistValue", out isFound));
			Assert(!isFound);

			AssertExceptionThrown("ZBoolValue' template constant value cannot be converted to target type 'CargoWise.Types.ZInt'. Actual value is 'Y'", typeof(DocumentTypeConversionFailedException), () => docWrapperContext.GetTemplateConstantValue<ZInt>("ZBoolValue", out isFound));
			AssertExceptionThrown("'ZStringValue' template constant value cannot be converted to target type 'CargoWise.Types.ZBool'. Actual value is 'abcd'", typeof(DocumentTypeConversionFailedException), () => docWrapperContext.GetTemplateConstantValue<ZBool>("ZStringValue", out isFound));
		}
	}
}
