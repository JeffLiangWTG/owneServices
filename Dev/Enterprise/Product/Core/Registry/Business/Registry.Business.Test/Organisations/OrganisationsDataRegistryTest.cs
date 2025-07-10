using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Organizations.CodeGeneration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OrganisationsDataRegistry))]
	sealed class OrganisationsDataRegistryTest : RegistryItemSetTestCaseWithFactory<OrganisationsDataRegistry>
	{
		public void TestReceivablesCreditAgreedPaymentMethodsRegistryItem()
		{
			var item = ItemSet.ReceivablesCreditAgreedPaymentMethodsList;
			AssertEquals("Name", "ReceivablesCreditAgreedPaymentMethods", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.Organizations_CodeLists, item.Category);
			AssertEquals("Caption", "Receivables Credit Agreed Payment Methods", item.Caption);
			AssertEquals("Hint", @"The list of valid payment methods that can be assigned against a Receivables Organization to record the a settlement method a customer has agreed as part of their credit arrangements.
By default, only transactions with payment method CRQ - Collection Request can be added to a collection batch. Use the 'Collection Batch Type' flag to indicate which payment methods can be added to a Collection Batch.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
		}

		public void TestReceivablesCreditAgreedPaymentMethodDefaultValue()
		{
			AssertReceivablesCreditAgreedPaymentMethodDefaultValues(isOFXEPaymentEnabled: true);
			AssertReceivablesCreditAgreedPaymentMethodDefaultValues(isOFXEPaymentEnabled: false);
		}

		void AssertReceivablesCreditAgreedPaymentMethodDefaultValues(bool isOFXEPaymentEnabled)
		{
			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(EnvProxy.Instance.CurrentCompany.PK, isOFXEPaymentEnabled))
			{
				var systemLevelDefaultValue = ItemSet.ReceivablesCreditAgreedPaymentMethodsList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				AssertEquals("DefaultValue.Count", 6, systemLevelDefaultValue.Count);
				AssertEquals("DefaultValue.GetDescriptionFromCode(\"CHK\")", "Business Check", systemLevelDefaultValue.GetDescriptionFromCode("CHK"));
				AssertEquals("DefaultValue.GetDescriptionFromCode(\"CCD\")", "Credit Card", systemLevelDefaultValue.GetDescriptionFromCode("CCD"));
				AssertEquals("DefaultValue.GetDescriptionFromCode(\"TRF\")", "Bank Transfer", systemLevelDefaultValue.GetDescriptionFromCode("TRF"));
				AssertEquals("DefaultValue.GetDescriptionFromCode(\"CBC\")", "Cash and/or Bank Check", systemLevelDefaultValue.GetDescriptionFromCode("CBC"));
				AssertEquals("DefaultValue.GetDescriptionFromCode(\"DBC\")", "Debit Card", systemLevelDefaultValue.GetDescriptionFromCode("DBC"));
				AssertEquals("DefaultValue.GetDescriptionFromCode(\"CRQ\")", "Collection Request", systemLevelDefaultValue.GetDescriptionFromCode("CRQ"));

				var companyLevelDefaultValue = ItemSet.ReceivablesCreditAgreedPaymentMethodsList.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				var expectedPaymentMethodCount = isOFXEPaymentEnabled ? 7 : 6;
				AssertEquals("DefaultValue.Count", expectedPaymentMethodCount, companyLevelDefaultValue.Count);
				AssertEquals("DefaultValue.GetDescriptionFromCode(\"CHK\")", "Business Check", companyLevelDefaultValue.GetDescriptionFromCode("CHK"));
				AssertEquals("DefaultValue.GetDescriptionFromCode(\"CCD\")", "Credit Card", companyLevelDefaultValue.GetDescriptionFromCode("CCD"));
				AssertEquals("DefaultValue.GetDescriptionFromCode(\"TRF\")", "Bank Transfer", companyLevelDefaultValue.GetDescriptionFromCode("TRF"));
				AssertEquals("DefaultValue.GetDescriptionFromCode(\"CBC\")", "Cash and/or Bank Check", companyLevelDefaultValue.GetDescriptionFromCode("CBC"));
				AssertEquals("DefaultValue.GetDescriptionFromCode(\"DBC\")", "Debit Card", companyLevelDefaultValue.GetDescriptionFromCode("DBC"));
				AssertEquals("DefaultValue.GetDescriptionFromCode(\"CRQ\")", "Collection Request", companyLevelDefaultValue.GetDescriptionFromCode("CRQ"));
				if (isOFXEPaymentEnabled)
				{
					AssertEquals("DefaultValue.GetDescriptionFromCode(\"EPA\")", "E-Payment", companyLevelDefaultValue.GetDescriptionFromCode("EPA"));
				}
				else
				{
					AssertNullOrEmpty(companyLevelDefaultValue.GetDescriptionFromCode("EPA"));
				}
			}
		}

		public void TestDropdownAddressCountThresholdItem()
		{
			TestRegistryItem(ItemSet.DropdownAddressCountThreshold,
				"DropdownAddressCountThreshold",
				OrganisationsDataRegistry.Categories.Organizations_DefaultValues,
				"Drop-down Address List Count Threshold",
				"When selecting an address from an organization's address list in address controls, an Address Search Form will display instead of a Drop-down Address List if the count of the address list is more than the value set.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.PreserveTestValue,
				30,
				8,
				100);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public void TestCampaignManagementItems()
		{
			string category = OrganisationsDataRegistry.Categories.SalesMarketing_CampaignManagement;
			RegistryStorageFlags storage = RegistryStorageFlags.System | RegistryStorageFlags.Company;
			TestRegistryItem(ItemSet.CampaignStageList, "CampaignStageList", category, "Campaign Stage List", "This is the stage list for Campaign Management.", storage, RegistryOptions.Default, 1, new CodeDescriptionPair("UDF", $"Undefined - You can modify this in the System Registry, under {OrganisationsDataRegistry.Categories.SalesMarketing_CampaignManagement + "/Campaign Stage List"}"));
			TestRegistryItem(ItemSet.CampaignCategory1List, "CampaignMediaCategoryList", OrganisationsDataRegistry.Categories.SalesMarketing_CampaignManagement_ClientRelationshipManagement_Category1List, "List", "This is a categorization list for Campaign Management, which could be used for Media Category.", storage, RegistryOptions.Default);
			TestRegistryItem(ItemSet.CampaignCategory2List, "CampaignMediaTypeList", OrganisationsDataRegistry.Categories.SalesMarketing_CampaignManagement_ClientRelationshipManagement_Category2List, "List", "This is a 2nd categorization list for Campaign Management, which could be used for Media Type.", storage, RegistryOptions.Default);
			TestRegistryItem(ItemSet.CampaignCategory1Label, "CampaignCategory1Label", OrganisationsDataRegistry.Categories.SalesMarketing_CampaignManagement_ClientRelationshipManagement_Category1List, "Label", "Allows you to further categorize your campaigns, by a media category identifier.", storage, TextEditorType.TextBox, "Media Category");
			TestRegistryItem(ItemSet.CampaignCategory2Label, "CampaignCategory2Label", OrganisationsDataRegistry.Categories.SalesMarketing_CampaignManagement_ClientRelationshipManagement_Category2List, "Label", "Allows you to further categorize your campaigns, by a media type identifier.", storage, TextEditorType.TextBox, "Media Type");
			TestRegistryItem(ItemSet.HRCampaignCategory1List, "HRCampaignMediaCategoryList", OrganisationsDataRegistry.Categories.SalesMarketing_CampaignManagement_HumanResources_Category1List, "List", "This is a categorization list for Campaign Management, which could be used for Media Category.", storage, RegistryOptions.PreserveTestValue);
			TestRegistryItem(ItemSet.HRCampaignCategory2List, "HRCampaignMediaTypeList", OrganisationsDataRegistry.Categories.SalesMarketing_CampaignManagement_HumanResources_Category2List, "List", "This is a 2nd categorization list for Campaign Management, which could be used for Media Type.", storage, RegistryOptions.PreserveTestValue);
			TestRegistryItem(ItemSet.HRCampaignCategory1Label, "HRCampaignCategory1Label", OrganisationsDataRegistry.Categories.SalesMarketing_CampaignManagement_HumanResources_Category1List, "Label", "Allows you to further categorize your campaigns, by a media category identifier.", storage, TextEditorType.TextBox, "Media Category");
			TestRegistryItem(ItemSet.HRCampaignCategory2Label, "HRCampaignCategory2Label", OrganisationsDataRegistry.Categories.SalesMarketing_CampaignManagement_HumanResources_Category2List, "Label", "Allows you to further categorize your campaigns, by a media type identifier.", storage, TextEditorType.TextBox, "Media Type");
			TestRegistryItem(ItemSet.AutoSaveVoteSurveyExamAnswers, "AutoSaveVoteSurveyExamAnswers", category, "Auto Save Vote / Survey / Exam Answers",
				"When this flag is set to TRUE, vote nominations for vote campaigns and answers for survey / exam campaigns will be automatically stored in the database when populated.", RegistryStorageFlags.System, RegistryOptions.Default, false);

			TestRegistryItem(ItemSet.LinkTrackingImageUrl, "LinkTrackingImageUrl", category, "Link Tracking Image URL",
				"URL for image which is inserted into every campaign email. It is used to track clicks on hyperlinks in campaigns including those that do not use the Link Activity functionality.",
				RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, TextEditorType.TextBox, "http://ilul.me/cw1ver.png");

			TestRegistryItem(ItemSet.LinkTrackingUrl, "LinkTrackingUrl", category, "Link Tracking URL",
				"URL used to track clicks on hyperlinks in campaigns. This URL is added as a prefix to the actual hyperlink. A click on the modified link first goes to the tracking site for recording and redirecting to the final destination.",
				RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, TextEditorType.TextBox, "http://ilul.me/");

			TestRegistryItem(ItemSet.PermanentlySavedCampaignEmailDocType, "PermanentlySavedCampaignEmailDocType",
				category, "Permanently Saved Campaign Email Doc Type", "eDocs Doc Type for Campaign emails that are permanently saved against recipients.", RegistryStorageFlags.System, RegistryFindBoxCollection.RefDocType, GetMSCDocTypePK());

			TestRegistryItem(ItemSet.DisplayGridMaxRecords, "DisplayGridMaxRecords", category, "Max Record in Display Grid", "The maximum number of records to display in the display grid", RegistryStorageFlags.System, 3000);

			TestRegistryItem(ItemSet.UnsubscribedSuccessfully, "UnsubscribedSuccessfully", OrganisationsDataRegistry.Categories.SalesMarketing_CampaignManagement_Unsubscribe, "Successfully Unsubscribed", "Allows you to change the message appearing on the Auto Unsubscribe web page for a user to unsubscribe.", storage, TextEditorType.TextBox, "You have successfully unsubscribed.");

			TestRegistryItem(ItemSet.SubscripitionPreferenceSuccessfullyAdmin, "SubscripitionPreferenceSuccessfullyAdmin",
				OrganisationsDataRegistry.Categories.SalesMarketing_CampaignManagement_SubscripitionPreference, $"Successfully changed the preferences (Displayed in {Core.Constants.ProductName})",
				"Allows you to change the message appearing on the subscription preference form after administrator successfully changed the preferences.",
				storage, TextEditorType.TextBox, "You have successfully changed the subscription preferences.");
			TestRegistryItem(ItemSet.SubscripitionPreferenceSuccessfullyUser, "SubscripitionPreferenceSuccessfullyUser",
				OrganisationsDataRegistry.Categories.SalesMarketing_CampaignManagement_SubscripitionPreference, "Successfully changed the preferences (Displayed on the web page)",
				"Allows you to change the message appearing on the subscription preference web page after user successfully changed the preferences.",
				storage, TextEditorType.TextBox, "You have successfully changed your subscription preferences.");
			TestRegistryItem(ItemSet.SubscripitionPreferencePageTitle, "SubscripitionPreferencePageTitle",
				OrganisationsDataRegistry.Categories.SalesMarketing_CampaignManagement_SubscripitionPreference, "Subscription preference page title",
				"Allows you to change the message appearing on the subscription preference web page title.",
				storage, TextEditorType.TextBox, "Email Subscription Preference");
			TestRegistryItem(ItemSet.SubscripitionPreferencePageMessage, "SubscripitionPreferencePageMessage",
				OrganisationsDataRegistry.Categories.SalesMarketing_CampaignManagement_SubscripitionPreference, "Subscription preference page message",
				"Allows you to change the message appearing on the subscription preference web page message.",
				storage, TextEditorType.Memo, "Email (*ContactEmail*) is currently subscribed to the following emails and alerts.\r\n\r\nPlease select what you would like to hear about?");
			TestRegistryItem(ItemSet.AlreadyUnsubscribed, "AlreadyUnsubscribed", OrganisationsDataRegistry.Categories.SalesMarketing_CampaignManagement_Unsubscribe, "Already Unsubscribed", "Allows you to change the message appearing on the Auto Unsubscribe web page when a user unsubscribes from the same campaign several times.", storage, TextEditorType.TextBox, "You have already unsubscribed from this campaign.");
			TestRegistryItem(ItemSet.ResubscribedSuccessfully, "ResubscribedSuccessfully", OrganisationsDataRegistry.Categories.SalesMarketing_CampaignManagement_Unsubscribe, "Successfully Re-subscribed", "Allows you to change the message appearing on the Auto Unsubscribe web page when a user resubscribe.", storage, TextEditorType.TextBox, "You have successfully re-subscribed.");
			TestRegistryItem(ItemSet.ResubscribeLabel, "ResubscribeLabel", OrganisationsDataRegistry.Categories.SalesMarketing_CampaignManagement_Unsubscribe, "Re-subscribe Label", "Allows you to change the message appearing on the Auto Unsubscribe web page for a user to resubscribe.", storage, TextEditorType.TextBox, "Re-subscribe.");
		}

		Guid GetMSCDocTypePK()
		{
			return Factory.LoadTop1<IRefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, Core.Constants.RefDocTypes.MiscellaneousDocument)).PK.ToGuid();
		}

		public void TestRateFeeChargeLevels()
		{
			var level = ItemSet.RateFeeChargeLevels;

			AssertEquals("RateFeeChargeLevels", level.Name);
			AssertEquals("Master Data/Organizations/Rating", level.Category);
			AssertEquals("Fees and Charges - Levels", level.Caption);

			var expectedHint = "Create a list that defines the types of services offered to customers as \r\n\r\nvalue added services and nominate the levels and default values applicable to each service type.";
			AssertEquals(expectedHint, level.Hint);

			var storage = RegistryStorageFlags.System | RegistryStorageFlags.Company;
			AssertEquals(storage, level.Storage);

			AssertEquals(3, level.DefaultValue.FeeChargeTypes.Count);
		}

		public void TestConsolInvoicingStyles()
		{
			CodeDescriptionPairList expectedLookUpList = new CodeDescriptionPairList(OLookUpEditType.ConsolInvoicingStyles);
			TestRegistryItem(ItemSet.BuyersConsolInvoicingStyle, "BuyersConsolInvoicingStyle", OrganisationsDataRegistry.Categories.Organizations_DefaultValues, "Buyers Consol Invoicing Style", "Specify the default style for autorating and invoicing Buyers Consol Shipments.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, expectedLookUpList, "MAS");
			TestRegistryItem(ItemSet.ShippersConsolInvoicingStyle, "ShippersConsolInvoicingStyle", OrganisationsDataRegistry.Categories.Organizations_DefaultValues, "Shippers Consol Invoicing Style", "Specify the default style for autorating and invoicing Shippers Consol Shipments.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, expectedLookUpList, "MAS");
		}

		public void TestIncoTerms()
		{
			CodeDescriptionPairList expectedLookUpList = new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms);
			TestRegistryItem(ItemSet.ConsigneeIncoTerm, "ConsigneeIncoTerm", $"{OrganisationsDataRegistry.Categories.Organizations_DefaultValues}/Incoterms", "Consignee Incoterm", "Specify the default Incoterm for consignee organizations. These defaults will flow through to buyer and supplier relationships.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, expectedLookUpList, Constants.IncoTerms.FreeOnBoard);
			TestRegistryItem(ItemSet.ConsignorIncoTerm, "ConsignorIncoTerm", $"{OrganisationsDataRegistry.Categories.Organizations_DefaultValues}/Incoterms", "Consignor Incoterm", "Specify the default Incoterm for consignor organizations. These defaults will flow through to buyer and supplier relationships.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, expectedLookUpList, Constants.IncoTerms.FreeOnBoard);
		}

		public void TestAPPaymentMethod()
		{
			AssertAPPaymentMethod(true);
			AssertAPPaymentMethod(false);
		}

		void AssertAPPaymentMethod(bool isOFXEPaymentEnabled)
		{
			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, isOFXEPaymentEnabled))
			{
				var expectedLookUpListForSystemLevel = APPaymentMethodListProvider.GetAPPaymentMethods();
				expectedLookUpListForSystemLevel.RemoveCode(EPaymentMethods.EPaymentViaOFX);
				((IRegistryItemInternals)ItemSet.APPaymentMethod).GetRegistryItemPK(Guid.Empty, Guid.Empty, Guid.Empty);
				TestRegistryItem(ItemSet.APPaymentMethod, "APPaymentMethod", OrganisationsDataRegistry.Categories.Organizations_DefaultValues, "AP Bank Account Payment Methods", "Specify the default payment method for Account Payables Organizations", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, expectedLookUpListForSystemLevel, ReceiptTypes.DirectDebit);

				var expectedLookUpListForCompanyLevel = APPaymentMethodListProvider.GetAPPaymentMethods();
				if (!isOFXEPaymentEnabled)
				{
					expectedLookUpListForCompanyLevel.RemoveCode(EPaymentMethods.EPaymentViaOFX);
				}

				((IRegistryItemInternals)ItemSet.APPaymentMethod).GetRegistryItemPK(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty);
				TestRegistryItem(ItemSet.APPaymentMethod, "APPaymentMethod", OrganisationsDataRegistry.Categories.Organizations_DefaultValues, "AP Bank Account Payment Methods", "Specify the default payment method for Account Payables Organizations", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, expectedLookUpListForCompanyLevel, ReceiptTypes.DirectDebit);
			}
		}

		public void TestDefaultAttachmentType()
		{
			CodeDescriptionPairList expectedLookUpList = new CodeDescriptionPairList(ContactAttachmentTypeList.AttachmentType_List);
			TestRegistryItem(ItemSet.DefaultAttachmentType, "DefaultAttachmentType", OrganisationsDataRegistry.Categories.Organizations_DefaultValues, "Attachment Type for Contact", "Specify the default attachment type for Contact", RegistryStorageFlags.System, RegistryOptions.PreserveTestValue, expectedLookUpList, OrgConstants.AttachmentType.PDF);
		}

		public void TestDefaultARCreditApproved()
		{
			TestRegistryItem(ItemSet.DefaultARCreditApproved, "DefaultARCreditApproved", OrganisationsDataRegistry.Categories.Organizations_DefaultValues, "AR Credit Approved", "Specifies default value for the AR Credit Approved field.", RegistryStorageFlags.System | RegistryStorageFlags.Company, true);
		}

		public void TestUseARSettlementGroupCreditLimit()
		{
			TestRegistryItem(ItemSet.UseARSettlementGroupCreditLimit, "UseSettlementGroupCreditLimitOrgDefault", OrganisationsDataRegistry.Categories.Organizations_DefaultValues, "Use AR Settlement Group Credit Limit", @"This registry item controls default value of the 'Use Settlement Group Credit Limit' checkbox for a new Organization. This default value then can be changed by user.

The ‘Use Settlement Group Credit Limit’ setting on an Organization determines whether Credit Limit checking uses the Organization Settlement group when summing up transaction amounts to calculate the balance.  This includes Credit Limit checking performed by the Credit Limit and Balance Web Service.

When set to 'Yes', the 'Use Settlement Group Credit Limit' checkbox will be ticked for a new Organization and 'DEF' Invoice Term will be set for 'ALL' Invoice Type to provide a fall back to the Settlement Group's Invoice Terms settings.

When set to ‘No’ (default), it will make the mentioned above checkbox un-ticked for a new organization and Invoice Term will have its default ‘COD’ value.", RegistryStorageFlags.System | RegistryStorageFlags.Company, false);
		}

		public void TestOrgMatchThreshold()
		{
			CodeDescriptionPairList expectedLookUpList = new OrgMatchThresholds();

			TestRegistryItem(item: ItemSet.OrgMatchThreshold,
				expectedName: "OrgMatchThreshold",
				expectedCategory: OrganisationsDataRegistry.Categories.Organizations_PatternMatch,
				expectedCaption: "Legacy Organization Match Threshold",
				expectedHint: @"This registry setting is related to legacy organization matching functionality. If ""Use Match Engine to Match Organization"" is set to Yes, this will not be used.
This value determines how aggressive the system should be in finding similar organization matches.

 - A setting of EXTREME will be the most scrutinizing, showing the fewest matches.
 - A setting of LOW will be the most relaxed showing more possible matches.
 - The default setting of MEDIUM is generally appropriate for most situations.",
				expectedStorage: RegistryStorageFlags.All,
				expectedOptions: RegistryOptions.PreserveTestValue,
				expectedLookUpList: expectedLookUpList,
				expectedDefaultValue: OrgMatchThresholds.Codes.Medium);
		}

		public void TestUseUnmatchedOrganisationForMatching()
		{
			var unmatchedOrg = new UnmatchedOrganisation(Factory);
			unmatchedOrg.IsEnabled = true;
			ItemSet.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unmatchedOrg);
			AssertEquals("Value.IsEnabled", true, ItemSet.UseUnmatchedOrganisationForMatching.Value.IsEnabled);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.Organizations_PatternMatch, ItemSet.UseUnmatchedOrganisationForMatching.Category);
			AssertEquals("Caption", "Use Unmatched Organization for Matching", ItemSet.UseUnmatchedOrganisationForMatching.Caption);
			AssertEquals("Hint", "When enabled, this option will allow jobs to be assigned to a special \"default\" (UNMATCHED) organization in the system, if a Consignee, Consignor, Broker, etc cannot be found during a data import.\r\nNOTE: Specific module configuration for UXML imports can be found in the \"Unmatched Organization Configuration\" registry. The control of these modules is separate to this registry.", ItemSet.UseUnmatchedOrganisationForMatching.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, ItemSet.UseUnmatchedOrganisationForMatching.Storage);
			AssertEquals("RegistryOptions", RegistryOptions.PreserveTestValue, ItemSet.UseUnmatchedOrganisationForMatching.Options);
			AssertEquals("RegistryStorageFlags", RegistryStorageFlags.System, ItemSet.UseUnmatchedOrganisationForMatching.Storage);
		}

		public void TestUnmatchedOrganisationConfiguration()
		{
			TestRegistryItem(ItemSet.UnmatchedOrganisationConfiguration,
				"UnmatchedOrganisationConfiguration",
				OrganisationsDataRegistry.Categories.Organizations_PatternMatch,
				"Unmatched Organization Configuration",
				"Allows the unmatched organization for UXML import to be switched on/off for certain modules.",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue);

			var collection = ItemSet.UnmatchedOrganisationConfiguration.Value;
			CombineAssertions(() =>
			{
				AssertEquals(1, collection.Count);

				Assert(!collection.GetBoolFromCode(OrganisationsDataRegistry.JobTypeCodes.Order));
				AssertEquals("Order (Forwarding)", collection.GetDescriptionFromCode(OrganisationsDataRegistry.JobTypeCodes.Order));
			});
		}

		public void TestContactJobCategories_DefaultsAreCorrect()
		{
			var defaultValues = ItemSet.ContactJobCategories.DefaultValue;

			AssertEquals("DefaultValue.Count", 17, defaultValues.Count);

			AssertEquals("DefaultValue.ContainsCode(\"LEA\")", true, defaultValues.ContainsCode("LEA"));
			AssertEquals("DefaultValue.ContainsCode(\"EMA\")", true, defaultValues.ContainsCode("EMA"));
			AssertEquals("DefaultValue.ContainsCode(\"MAS\")", true, defaultValues.ContainsCode("MAS"));
			AssertEquals("DefaultValue.ContainsCode(\"SMO\")", true, defaultValues.ContainsCode("SMO"));
			AssertEquals("DefaultValue.ContainsCode(\"EMS\")", true, defaultValues.ContainsCode("EMS"));
			AssertEquals("DefaultValue.ContainsCode(\"MAA\")", true, defaultValues.ContainsCode("MAA"));

			AssertEquals("DefaultValue does not contain AC1)", false, defaultValues.ContainsCode("AC1"));
			AssertEquals("DefaultValue does not contain EXE)", false, defaultValues.ContainsCode("EXA"));
		}

		public void TestContactJobCategories_DefaultsCodeAndDescriptionAreReadOnly()
		{
			CombineAssertions(() =>
			{
				foreach (CodeDescriptionBool cdb in ItemSet.ContactJobCategories.DefaultValue)
				{
					AssertEquals($"Default value code & description should be readonly - {cdb.Description}", true, cdb.SystemDefined);
				}
			});
		}

		public void TestContactSourceTypes()
		{
			ReadOnlyCodeDescriptionPairList defaultValue = ItemSet.ContactSourceTypes.DefaultValue;

			AssertEquals("DefaultValue.Count", 7, defaultValue.Count);
			AssertEquals("DefaultValue.ContainsCode(\"Website\")", true, defaultValue.ContainsCode("Website"));

			CodeDescriptionPairList newValue = new CodeDescriptionPairList();
			newValue.AddPair("Janitor");

			ItemSet.ContactSourceTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
			AssertEquals("Value.Count", 1, ItemSet.ContactSourceTypes.Value.Count);
			AssertEquals("Value.ContainsCode(\"Janitor\")", true, ItemSet.ContactSourceTypes.Value.ContainsCode("Janitor"));
		}

		public void TestRatesSecurity()
		{
			AssertEquals(ItemSet.RatesSecurity.Storage, RegistryStorageFlags.System | RegistryStorageFlags.Company);

			var defaultValue = ItemSet.RatesSecurity.DefaultValue;

			AssertEquals("DefaultValue.Count", 0, defaultValue.Count);

			var newValue = new CodeDescriptionPairList();
			newValue.AddPair("TES", "TEST");

			ItemSet.RatesSecurity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
			AssertEquals("Value.Count", 1, ItemSet.RatesSecurity.Value.Count);
			AssertEquals("Value.ContainsCode(\"TES\")", true, ItemSet.RatesSecurity.Value.ContainsCode("TES"));
		}

		public void TestClientSizeList()
		{
			ReadOnlyCodeDescriptionPairList defaultValue = ItemSet.ClientSizeList.DefaultValue;

			AssertEquals("DefaultValue.Count", 3, defaultValue.Count);
			AssertEquals("DefaultValue.ContainsCode(\"SML\")", true, defaultValue.ContainsCode("SML"));

			CodeDescriptionPairList newValue = new CodeDescriptionPairList();
			newValue.AddPair("BIG", "Very Big");

			ItemSet.ClientSizeList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
			AssertEquals("Value.Count", 1, ItemSet.ClientSizeList.Value.Count);
			AssertEquals("Value.ContainsCode(\"BIG\")", true, ItemSet.ClientSizeList.Value.ContainsCode("BIG"));
			AssertEquals(RegistryOptions.Default, ItemSet.ClientSizeList.Options);
		}

		public void TestContactJobCategoryMandatory()
		{
			TestRegistryItem(ItemSet.ContactJobCategoryMandatoryRaw, "ContactJobCategoryMandatory", $"{OrganisationsDataRegistry.Categories.Organizations}/Organization Required Fields", "Contact Job Category Mandatory", "Specifies whether the Job Category field on each Contact is required to be filled in.", RegistryStorageFlags.System, RegistryOptions.PreserveTestValue, false);
		}

		public void TestOrgCodeAlgorithms()
		{
			CombineAssertions(() =>
			{
				AssertEquals("OrgCodeAlgorithmDefault.Value.AlgorithmType", OrgCodeAlgorithmType.Default, ItemSet.OrgCodeAlgorithmDefault.Value.AlgorithmType);
				AssertEquals("OrgCodeAlgorithmDefault.Value.AllowRecalculatedOrgCodeByUser", false, ItemSet.OrgCodeAlgorithmDefault.Value.AllowRecalculatedOrgCodeByUser);
				AssertEquals("OrgCodeAlgorithmOverride.Value.AlgorithmType", OrgCodeAlgorithmType.Override, ItemSet.OrgCodeAlgorithmOverride.Value.AlgorithmType);
				AssertEquals("OrgCodeAlgorithmOverride.Value.AllowRecalculatedOrgCodeByUser", false, ItemSet.OrgCodeAlgorithmOverride.Value.AllowRecalculatedOrgCodeByUser);
			});
		}

		public void TestTabsStatus()
		{
			TestRegistryItem(ItemSet.GetShowEnabledOrganisationTabs(Env.CurrentUser.PK), ItemSet.GetShowEnabledOrganisationTabs(Env.CurrentUser.PK).Name, string.Format("{0}/Tabs Status", OrganisationsDataRegistry.Categories.Organizations), "Tabs Status of Organization Form", "Depends of Organization Types shows Tabs of Organization Form", RegistryStorageFlags.Company, RegistryOptions.IsHidden, false);
		}

		public void TestAllowNumericCharactersInCodeGeneration()
		{
			TestRegistryItem(ItemSet.AllowNumericCharactersInCodeGeneration,
				"AllowNumericCharactersInCodeGeneration",
				OrganisationsDataRegistry.Categories.Organizations_Codes,
				"Allow Numeric Characters In Code Generation",
				"When generating a new organization code from an organization name, numeric characters are ignored by default and only letters are used as part of the new code. E.g. an organization called \"7 Eleven\" in USLAX will be \"ELEVENLAX\". By enabling this registry, numeric characters will be retained and in the example, the code generated will be \"7ELEVELAX\".",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue,
				true);
		}

		public void TestEnableControllingAgentFunctionalityAndValidations()
		{
			TestRegistryItem(ItemSet.EnableControllingAgentFunctionalityAndValidations,
				"EnableControllingAgentFunctionalityAndValidations",
				OrganisationsDataRegistry.Categories.Organizations,
				"Enable Controlling Agent Functionality and Validations",
				"When turned on, the Organization Type \"Controlling Agent\" will be visible on the Organization form and its validation rules will be enabled.",
				RegistryStorageFlags.System,
				false);
		}

		public void TestEnableOrgPatternMatchOverride()
		{
			TestRegistryItem(ItemSet.EnableOrgPatternMatchOverride,
				"EnableOrgPatternMatchOverride",
				OrganisationsDataRegistry.Categories.Organizations,
				"Enable OrgPatternMatchOverride",
				"Enabling this registry will allow the OrgPatternMatchOverride Collection to be exported in an organisation xml.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestEnableControllingCustomerFunctionalityAndValidations()
		{
			TestRegistryItem(ItemSet.EnableControllingCustomerFunctionalityAndValidations,
				"EnableControllingCustomerFunctionalityAndValidations",
				OrganisationsDataRegistry.Categories.Organizations,
				"Enable Controlling Customer Functionality and Validations",
				"When turned on, the Organization Type \"Controlling Customer\" will be visible on the Organization form and its validation rules will be enabled.",
				RegistryStorageFlags.System,
				false);
		}

		public void TestEnableDefaultingClosestPortOnAddressValidation()
		{
			TestRegistryItem(ItemSet.EnableDefaultingClosestPortOnAddressValidation,
				"EnableDefaultingClosestPortOnAddressValidation",
				OrganisationsDataRegistry.Categories.Organizations_DefaultValues_UNLOCODefaultingRules,
				"Enable Defaulting Closest Port on Address Validation", @"When enabled, this program will look for the closest UNLOCO to the validated address’s Geo-Location Coordinates that comply with UNLOCO Defaulting Rules.
IE:
Yes = Enabled = Closest to Longitude X, Latitude Y and has Airport AND/OR Seaport AND/OR no Identifier.
No = Disabled = Closest not applicable but has Airport AND/OR Seaport AND/OR no Identifier.
This registry item is dependent on the existence of Geo-Location Coordinates in the address.
If an address is manually verified, then Geo-Location Coordinates can be inserted manually.
This registry item works in conjunction with AND or OR logic and UNLOCO Defaulting Rules.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				true);
		}

		public void TestNewStaffAssignmentsAsGlobal()
		{
			TestRegistryItem(ItemSet.NewStaffAssignmentsAsGlobal, "NewStaffAssignmentsAsGlobal", OrganisationsDataRegistry.Categories.Organizations, "New Staff Assignments as Global", "If this registry is turned on then newly created staff assignments will be Global, as opposed to Company specific.\r\nNote that only users with appropriate security rights will be allowed to create and maintain Global staff assignments.", RegistryStorageFlags.System, false);
		}

		public void TestMakeSalesRepMandatory()
		{
			TestRegistryItem(ItemSet.MakeSalesRepMandatory, "MakeSalesRepMandatory", OrganisationsDataRegistry.Categories.Organizations_OrganizationRequiredFields, "Make Sales Rep Mandatory", "If registry is turned on, when saving changes on organizations (new or edits), if it does not have a sales rep record in staff assignment, it will auto add in a Sales rep record but leaving Initials field blank with an error, so that it cannot be saved until it is entered. This only applies to Receivables or Sales organizations. This does not apply to temporary organizations.", RegistryStorageFlags.System, false);
		}

		public void TestSetMakeExternalDebtorCodeMandatory()
		{
			TestRegistryItem(ItemSet.MakeExternalDebtorCodeMandatory, "MakeExternalDebtorCodeMandatory", OrganisationsDataRegistry.Categories.Organizations, "Make External Debtor Code Mandatory", "When this registry is set to 'Yes', the 'External Debtor Code' field will become mandatory for organization records flagged as 'Receivables'.", RegistryStorageFlags.Company | RegistryStorageFlags.System, false);
		}

		public void TestSetMakeExternalCreditorCodeMandatory()
		{
			TestRegistryItem(ItemSet.MakeExternalCreditorCodeMandatory, "MakeExternalCreditorCodeMandatory", OrganisationsDataRegistry.Categories.Organizations, "Make External Creditor Code Mandatory", "When this registry is set to 'Yes', the 'External Creditor Code' field will become mandatory for organization records flagged as 'Payables'.", RegistryStorageFlags.Company | RegistryStorageFlags.System, false);
		}

		public void TestUpdateEDICodeMappingwhenChargeCodeIsRenamed()
		{
			TestRegistryItem(ItemSet.UpdateEDICodeMappingwhenChargeCodeIsRenamed, "UpdateEDICodeMappingwhenChargeCodeIsRenamed", OrganisationsDataRegistry.Categories.Organizations, "Update EDI Code Mapping when charge code is renamed", @"When this registry is set to ‘Yes’ and a charge code is renamed, all matching EDI Charge Code Mappings will be updated.
When this registry is set to ‘No’, no update will occur.

It is recommended that when a charge code is renamed, you should review all existing EDI Code Mapping.
In particular, in a multi - companies system.", RegistryStorageFlags.Company | RegistryStorageFlags.System, false);
		}

		public void TestPermanentlySaveAgainstRecipientDefault()
		{
			TestRegistryItem(ItemSet.PermanentlySaveAgainstRecipientDefault,
				"PermanentlySaveAgainstRecipientDefault",
				OrganisationsDataRegistry.Categories.SalesMarketing_CampaignManagement,
				"Permanently Save Against Recipient Default",
				"When this registry is set to 'Yes', it will enable the Permanently Save Against Recipient Default checkbox in the Campaign Sending Options.",
				RegistryStorageFlags.Company | RegistryStorageFlags.System,
				false);
		}

		public void TestOrgPatternMatchLimit()
		{
			TestRegistryItem(ItemSet.OrgPatternMatchLimit,
				"OrgPatternMatchLimit",
				OrganisationsDataRegistry.Categories.Organizations_PatternMatch,
				"Org. Pattern Match Limit",
				"The maximum number of organization pattern match records to examine during organization matching. Adjust this number lower if system is getting out of memory problems or is too slow. Adjust this number higher if system is not able to match certain organizations when there are large number of duplicates or similar organizations. Write 0 (zero) for unlimited maximum number.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers, 0);
		}

		public void TestUXMLOrganisationMinimumConfidence()
		{
			TestRegistryItem(ItemSet.UXMLOrganisationMinimumConfidence,
				"UXMLOrganisationMinimumConfidence",
				OrganisationsDataRegistry.Categories.Organizations_PatternMatch,
				"UXML Organization Match Threshold",
				"When the Match Engine is enabled, potential organization matches must have a confidence level higher than the selected percentage value.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				50,
				0, 100);
		}

		public void TestOrgMatchUseDeduplication()
		{
			TestRegistryItem(ItemSet.OrgMatchUseDeduplication,
				"OrgMatchUseDeduplication",
				OrganisationsDataRegistry.Categories.Organizations_PatternMatch,
						"Use Match Engine to Match Organization",
						"Set True to Use Match Engine to Match Organization.",
				RegistryStorageFlags.All,
				RegistryOptions.Default,
				false);
		}

		public void TestContactSalutation()
		{
			TestGenericRegistryItem(ItemSet.ContactSalutation, "ContactSalutation", OrganisationsDataRegistry.Categories.Organizations,
						"Contact Salutation Gender/Type",
						"Additional Salutation Gender (Female/Male)/Relationship Type (Friendly or Formal) option has been added to organization Contact to identify the correct greeting for system defined documents, and can be set per specific language.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company);
		}

		public void TestImportAirBroker()
		{
			TestRegistryItem(ItemSet.ImportAirBroker, "ImportAirBroker", OrganisationsDataRegistry.Categories.Organizations_Shipment,
						"Import Air Broker",
						"The Organization selected below will default as the Import Air Broker in the Related Parties section when creating a new Organization record.",
						RegistryStorageFlags.Branch, RegistryOptions.IsValueOptional, RegistryFindBoxCollection.Broker, Guid.Empty);
		}

		public void TestImportSeaBroker()
		{
			TestRegistryItem(ItemSet.ImportSeaBroker, "ImportSeaBroker", OrganisationsDataRegistry.Categories.Organizations_Shipment,
						"Import Sea Broker",
						"The Organization selected below will default as the Import Sea Broker in the Related Parties section when creating a new Organization record.",
						RegistryStorageFlags.Branch, RegistryOptions.IsValueOptional, RegistryFindBoxCollection.Broker, Guid.Empty);
		}

		public void TestExportAirBroker()
		{
			TestRegistryItem(ItemSet.ExportAirBroker, "ExportAirBroker", OrganisationsDataRegistry.Categories.Organizations_Shipment,
						"Export Air Broker",
						"The Organization selected below will default as the Export Air Broker in the Related Parties section when creating a new Organization record.",
						RegistryStorageFlags.Branch, RegistryOptions.IsValueOptional, RegistryFindBoxCollection.Broker, Guid.Empty);
		}

		public void TestExportSeaBroker()
		{
			TestRegistryItem(ItemSet.ExportSeaBroker, "ExportSeaBroker", OrganisationsDataRegistry.Categories.Organizations_Shipment,
						"Export Sea Broker",
						"The Organization selected below will default as the Export Sea Broker in the Related Parties section when creating a new Organization record.",
						RegistryStorageFlags.Branch, RegistryOptions.IsValueOptional, RegistryFindBoxCollection.Broker, Guid.Empty);
		}

		public void TestOrgShipmentBrokerRegistryOptional()
		{
			AssertEquals("ImportAirBroker optional", RegistryOptions.IsValueOptional, ItemSet.ImportAirBroker.Options & RegistryOptions.IsValueOptional);
			AssertEquals("ImportSeaBroker optional", RegistryOptions.IsValueOptional, ItemSet.ImportSeaBroker.Options & RegistryOptions.IsValueOptional);
			AssertEquals("ExportAirBroker optional", RegistryOptions.IsValueOptional, ItemSet.ExportAirBroker.Options & RegistryOptions.IsValueOptional);
			AssertEquals("ExportSeaBroker optional", RegistryOptions.IsValueOptional, ItemSet.ExportSeaBroker.Options & RegistryOptions.IsValueOptional);
		}

		public void TestBankAccountsBasedOnCurrency()
		{
			AssertEquals("Value", 0, ItemSet.BankAccountsBasedOnCurrency.Value.Count);

			BankAccountBasedOnCurrencyCollection collection = new BankAccountBasedOnCurrencyCollection();
			BankAccountBasedOnCurrency bankAccountBasedOnCurrency = collection.AddNew();
			bankAccountBasedOnCurrency.Currency = "AUD";
			bankAccountBasedOnCurrency.DoNotPerformListValidationOnBankAccount = true;
			bankAccountBasedOnCurrency.BankAccount = new ZGuid("9a2b1218-2998-4390-9f85-1d42fe69352e");

			ItemSet.BankAccountsBasedOnCurrency.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			AssertEquals("Value", 1, ItemSet.BankAccountsBasedOnCurrency.Value.Count);
			AssertEquals("Value", collection[0].Currency, ItemSet.BankAccountsBasedOnCurrency.Value[0].Currency);
			AssertEquals("Value", collection[0].CurrencyDescription, ItemSet.BankAccountsBasedOnCurrency.Value[0].CurrencyDescription);
			AssertEquals("Value", collection[0].BankAccount, ItemSet.BankAccountsBasedOnCurrency.Value[0].BankAccount);
		}

		public void TestBuyerSupplierRelationshipFieldsToExclude()
		{
			var defaultValues = new CodeDescriptionBoolCollection
			{
				{ OrganisationsDataRegistry.BuyerSupplierRelationshipFields.ImportBroker, (NoResString)"Import Broker", false },
				{ OrganisationsDataRegistry.BuyerSupplierRelationshipFields.DefaultCurrency, (NoResString)"Default Currency", false },
				{ OrganisationsDataRegistry.BuyerSupplierRelationshipFields.Incoterm, (NoResString)"Incoterm", false },
				{ OrganisationsDataRegistry.BuyerSupplierRelationshipFields.ContainerMode, (NoResString)"Container Mode", false },
				{ OrganisationsDataRegistry.BuyerSupplierRelationshipFields.OriginalBills, (NoResString)"Original Bills", false },
				{ OrganisationsDataRegistry.BuyerSupplierRelationshipFields.CopyBills, (NoResString)"Copy Bills", false },
			};

			TestRegistryItem(
				ItemSet.BuyerSupplierRelationshipFieldsToExclude,
				"BuyerSupplierRelationshipFieldsToExclude",
				OrganisationsDataRegistry.Categories.Organizations_BuyerSupplierRelationships,
				(NoResString)"Fields To Exclude",
				(NoResString)"Use this Registry setting to specify fields not to be added to the Buyer/Supplier Relationship when it’s first created from the job.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment,
				RegistryOptions.Default,
				"Exclude?",
				false,
				6,
				defaultValues.ToArray());
		}

		public void TestShowSalesActivities()
		{
			TestRegistryItem(
				ItemSet.ShowSalesActivities,
				"ShowSalesActivities",
				OrganisationsDataRegistry.Categories.SalesMarketing_ClientIntelligence,
				"Show Sales Activities",
				"By default Sales Activity tab on Organization screen is shown. Disable this to hide it.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				true);
		}

		public void TestDepotColorSound()
		{
			DepotAddressColorSoundRegistryItem item = ItemSet.DepotAddressColorSound;
			AssertEquals("Name", "DepotAddressColorSound", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.Organizations_eTailDepot, item.Category);
			AssertEquals("Caption", "Depot Address Color/Sound Tags", item.Caption);
			AssertEquals("Hint", "Assign a color and .wav and .mp3 sound files to a depot's address.", item.Hint);
			AssertEquals("Flgs", RegistryStorageFlags.System, item.Storage);
		}

		public void TestOrgBarcodeMask()
		{
			OrgBarcodeMaskRegistryItem item = ItemSet.OrgBarcodeMask;
			AssertEquals("Name", "OrgBarcodeMask", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.Organizations_eTailDepot, item.Category);
			AssertEquals("Caption", "Organization Barcode Mask", item.Caption);
			AssertEquals("Hint", @"For eTail Origin and Destination Depot scanning via eTail Portal, this mask can be applied to barcodes scanned in order to lift out actual consignment reference from the barcode. e.g. eParcel barcode in Australia can be 29 characters including postcode, however, the actual consignment reference is only 16 characters and need to be lifted from the 29 character barcode using this mask. Masks can be applied in an order of priority (1 - 99), such that a given barcode can be checked against each mask in order for a given Depot, until the first mask to return a proper consignment reference.

Mask entries are required to be regular expressions, containing no more than one group. For further information on regular expressions, please consult the Microsoft Developer Network page on the Regular Expression Language.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
		}

		public void TestEnableDeduplicationFinder()
		{
			var item = ItemSet.EnableDeduplicationFinder;
			AssertEquals("Name", "EnableDeduplicationFinder", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.Organizations_Duplicate_Detection, item.Category);
			AssertEquals("Caption", "Enable Duplicate Detection", item.Caption);
			AssertEquals("Hint", "When this registry is set to 'Yes', the system will show a warning when it detects that a potential duplicate is being added.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.Default, item.Options);
			AssertEquals("Default Value", true, item.DefaultValue);
		}

		public void TestUserDefinedContextLists()
		{
			var item = ItemSet.UserDefinedContext;
			AssertEquals("Name", "OrganisationsUserDefinedContexts", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.Organizations_CodeMappings, item.Category);
			AssertEquals("Caption", "User Defined Context", item.Caption);
			AssertEquals("Hint", "Define your own contexts that are included with EDI code mappings when exported.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.PreserveTestValue, item.Options);
			AssertEquals("Default Value", new CodeDescriptionPairList(), item.DefaultValue);

			var invalidCode = new CodeDescriptionPairList();
			invalidCode.AddPair("AAAAA");
			AssertExceptionThrown<RegistryValidationException>("Exceeded length", () => ItemSet.UserDefinedContext.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, invalidCode));

			var validCodeDescription = new CodeDescriptionPairList();
			validCodeDescription.AddPair("AAA", "My Test Description");
			AssertNoExceptionThrown(() => ItemSet.UserDefinedContext.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, validCodeDescription));

			var blankDescription = new CodeDescriptionPairList();
			blankDescription.AddPair("AAA", "");
			AssertNoExceptionThrown(() => ItemSet.UserDefinedContext.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, blankDescription));
		}

		public void TestDuplicateDetectionTimeout()
		{
			TestRegistryItem(ItemSet.DuplicateDetectionTimeout,
				"DuplicateDetectionTimeout",
				OrganisationsDataRegistry.Categories.Organizations_DuplicateDetection_Configuration_OrganisationForm,
				"Duplicate Detection Timeout",
				"Time in seconds representing timeout for duplicate detection (from 1 to 300).",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				10,
				1,
				300);
		}

		public void TestMaximumPotentialTargets()
		{
			var item = ItemSet.MaximumPotentialTargets;
			AssertEquals("Name", "MaximumPotentialTargets", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.Organizations_DuplicateDetection_Configuration_Global, item.Category);
			AssertEquals("Caption", "Maximum Potential Targets", item.Caption);
			AssertEquals("Hint", "The maximum number of potential targets that are loaded during duplicate detection (from 10 to 200).", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.Default, item.Options);
			AssertEquals("Default Value", 50, item.DefaultValue);
			AssertExceptionThrown<RegistryValidationException>("Maximum value should be 200", () => ItemSet.MaximumPotentialTargets.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 201));
			AssertExceptionThrown<RegistryValidationException>("Minimum value should be 10", () => ItemSet.MaximumPotentialTargets.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 9));
		}

		public void TestQueueProcessingBatchSize()
		{
			var item = ItemSet.QueueProcessingBatchSize;
			AssertEquals("Name", "QueueProcessingBatchSize", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.Organizations_Duplicate_Detection, item.Category);
			AssertEquals("Caption", "Queue Processing Batch Size", item.Caption);
			AssertEquals("Hint", "The batch size for De-duplication Queue Processing (DQP) service task.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.Default, item.Options);
			AssertEquals("Default Value", 50, item.DefaultValue);
			AssertExceptionThrown<RegistryValidationException>("Maximum value should be 200", () => ItemSet.MaximumPotentialTargets.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 201));
			AssertExceptionThrown<RegistryValidationException>("Minimum value should be 2", () => ItemSet.MaximumPotentialTargets.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1));
		}

		public void TestExcludePotentialDuplicatesFromOtherCountries()
		{
			var item = ItemSet.ExcludePotentialDuplicatesFromOtherCountries;
			AssertEquals("Name", "ExcludePotentialDuplicatesFromOtherCountries", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.Organizations_DuplicateDetection_Configuration_Global, item.Category);
			AssertEquals("Caption", "Exclude Potential Duplicate Records From Other Countries/Regions", item.Caption);
			AssertEquals("Hint", "When this registry is set to 'Yes', the system will exclude records from other countries/regions in the list of potential duplicate results.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Default Value", false, item.DefaultValue);
		}

		public void TestRepeatedValueLimit()
		{
			var item = ItemSet.RepeatedValueLimit;
			AssertEquals("Name", "RepeatedValueLimit", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.Organizations_DuplicateDetection_Configuration_Global, item.Category);
			AssertEquals("Caption", "Repeated Pattern Limit", item.Caption);
			AssertEquals("Hint", "Any pattern that is repeated more than the specified number of times will be excluded from duplicate detection. The value can be set between 1 and 200.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, item.Options);
			AssertEquals("Default Value", 100, item.DefaultValue);
		}

		public void TestExcludeInactivePotentialDuplicates()
		{
			var item = ItemSet.ExcludeInactivePotentialDuplicates;
			AssertEquals("Name", "ExcludeInactivePotentialDuplicates", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.Organizations_DuplicateDetection_Configuration_Global, item.Category);
			AssertEquals("Caption", "Exclude Inactive Potential Duplicate Records", item.Caption);
			AssertEquals("Hint", "When this registry is set to 'Yes', The system will exclude all inactive records from the list of potential duplicate results.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Default Value", true, item.DefaultValue);
		}

		public void TestMaximumAddressCountForUserDrivenDeduplication()
		{
			TestRegistryItem(ItemSet.MaximumAddressCountForUserDrivenDeduplication,
				"MaximumAddressCountForUserDrivenDeduplication",
				OrganisationsDataRegistry.Categories.Organizations_DuplicateDetection_Configuration_OrganisationForm,
				"Maximum Address Count for User Driven De-duplication",
				"The maximum amount of addresses an organization can contain before it is excluded from De-duplication.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				100,
				20,
				2000);
		}

		public void TestMaximumContactCountForUserDrivenDeduplication()
		{
			TestRegistryItem(ItemSet.MaximumContactCountForUserDrivenDeduplication,
				"MaximumContactCountForUserDrivenDeduplication",
				OrganisationsDataRegistry.Categories.Organizations_DuplicateDetection_Configuration_OrganisationForm,
				"Maximum Contact Count for User Driven De-duplication",
				"The maximum amount of contacts an organization can contain before it is excluded from De-duplication.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				100,
				20,
				2000);
		}

		public void TestCreditReportsPublicCertificate()
		{
			TestRegistryItem(ItemSet.CreditReportsPublicCertificate,
				"CreditReportsPublicCertificate",
				OrganisationsDataRegistry.Categories.Organizations_CreditReports,
				"Credit Reports Public Certificate",
				"Specify the Credit Reports Public Certificate",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				Array.Empty<byte>());
		}

		public void TestEnableCreditReports()
		{
			TestRegistryItem(ItemSet.EnableCreditReports,
				"EnableCreditReports",
				OrganisationsDataRegistry.Categories.Organizations_CreditReports,
				"Enable Credit Reports",
				"When this registry is set to 'Yes', Credit Reports are enabled. When this registry is set to 'No', Credit Reports are disabled.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				true);
		}

		public void TestCreditCheckServiceURLs()
		{
			var item = ItemSet.CreditCheckServiceURLs;

			TestGenericRegistryItem(
				item,
				"CreditCheckServiceURLs",
				OrganisationsDataRegistry.Categories.Organizations_CreditReports,
				"Service's URLs",
				"Specify the URLs that Credit Check Service will use when making requests.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport
				);

			AssertEquals("BoolColumnCaption", "Main", ((CodeDescriptionBoolRegistryEditorInfo)item.EditorInfo).BoolColumnCaption);

			var defaultCollection = item.DefaultValue;
			AssertEquals("DefaultValue.Count", 1, defaultCollection.Count);

			CombineAssertions(() =>
			{
				AssertEquals(4, defaultCollection[0].CodeMaxLength);
				AssertEquals("Default Bool Column Value", true, defaultCollection[0].Bool);
				AssertEquals("SYD1", defaultCollection[0].Code);
				AssertEquals("https://creditreport.wisegrid.net", defaultCollection[0].Description);
			});
		}

		public void TestEnableCreditReportsPerCountry()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Name", "EnableCreditReportsPerCountryOrganisationAndCompany", ItemSet.EnableCreditReportsPerCountryOrganisationAndCompany.Name);
				AssertEquals("Category", OrganisationsDataRegistry.Categories.Organizations_CreditReports, ItemSet.EnableCreditReportsPerCountryOrganisationAndCompany.Category);
				AssertEquals("Caption", "Enable Credit Reports Per Country/Region (Organization and Company)", ItemSet.EnableCreditReportsPerCountryOrganisationAndCompany.Caption);
				AssertEquals("Hint", "When a country/region is ticked as 'Enabled' under either \"Enable Company Reports\" or \"Enable Organization Report\", " +
					"Company or Organization-level Credit Reports respectively for that Country/Region are enabled. When a Country/Region is not ticked as 'Enabled', Credit Reports for that Country/Region are disabled. The " +
					"last four columns are options which apply only to Organization reports.", ItemSet.EnableCreditReportsPerCountryOrganisationAndCompany.Hint);
				AssertEquals("Flags", RegistryStorageFlags.System, ItemSet.EnableCreditReportsPerCountryOrganisationAndCompany.Storage);
				AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.EnableCreditReportsPerCountryOrganisationAndCompany.Options);

				AssertCountryAndCodeOfCreditReportsPerCountryRegistryItem(ItemSet.EnableCreditReportsPerCountryOrganisationAndCompany.DefaultValue);
			}
			);
		}

		void AssertCountryAndCodeOfCreditReportsPerCountryRegistryItem(CreditReportItemCollection creditReportItemCollection)
		{
			var countrysCollection = Factory.Load<IRefCountry>(new ZQuery()).OrderBy(c => c.RN_Desc).ToArray();

			AssertEquals("Expect equal number of countries in registry as from ref", creditReportItemCollection.Count, countrysCollection.Length);
			for (int i = 0; i < creditReportItemCollection.Count; i++)
			{
				var currentCountry = countrysCollection[i];

				var currentCreditReportRegistryItem = creditReportItemCollection[i];
				AssertEquals(currentCountry.RN_Code, currentCreditReportRegistryItem.CountryCode);
				AssertEquals(currentCountry.RN_Desc, currentCreditReportRegistryItem.Country);

				if (currentCreditReportRegistryItem.CountryCode.Equals(Constants.CountryCodes.Australia)
					|| currentCreditReportRegistryItem.CountryCode.Equals(Constants.CountryCodes.NewZealand))
				{
					AssertEquals($"CountryEnabledForCompany for {currentCountry.RN_Desc}", true, currentCreditReportRegistryItem.CountryEnabledForCompany);
					AssertEquals($"CountryEnabledForOrganisation for {currentCountry.RN_Desc}", true, currentCreditReportRegistryItem.CountryEnabledForOrganisation);
					AssertEquals($"CommercialBureauEnquiryEnabled for {currentCountry.RN_Desc}", true, currentCreditReportRegistryItem.CommercialBureauEnquiryEnabled);
					AssertEquals($"ComprehensiveReportEnabled for {currentCountry.RN_Desc}", true, currentCreditReportRegistryItem.ComprehensiveReportEnabled);
					AssertEquals($"FailureRiskEnabled for {currentCountry.RN_Desc}", true, currentCreditReportRegistryItem.FailureRiskEnabled);
					AssertEquals($"LatePaymentRiskEnabled for {currentCountry.RN_Desc}", true, currentCreditReportRegistryItem.LatePaymentRiskEnabled);
				}
				else
				{
					AssertEquals($"CountryEnabledForCompany for {currentCountry.RN_Desc}", false, currentCreditReportRegistryItem.CountryEnabledForCompany);
					AssertEquals($"CountryEnabledForOrganisation for {currentCountry.RN_Desc}", false, currentCreditReportRegistryItem.CountryEnabledForOrganisation);
					AssertEquals($"CommercialBureauEnquiryEnabled for {currentCountry.RN_Desc}", false, currentCreditReportRegistryItem.CommercialBureauEnquiryEnabled);
					AssertEquals($"ComprehensiveReportEnabled for {currentCountry.RN_Desc}", false, currentCreditReportRegistryItem.ComprehensiveReportEnabled);
					AssertEquals($"FailureRiskEnabled for {currentCountry.RN_Desc}", false, currentCreditReportRegistryItem.FailureRiskEnabled);
					AssertEquals($"LatePaymentRiskEnabled for {currentCountry.RN_Desc}", false, currentCreditReportRegistryItem.LatePaymentRiskEnabled);
				}
			}
		}

		public void TestGetCreditReportTimeout()
		{
			TestRegistryItem(ItemSet.GetCreditReportTimeout,
				"GetReport",
				"Master Data/Organizations/Credit Reports/Timeouts",
				"Get Report",
				"Sets the timeout (in seconds) for Get Report.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				60,
				30,
				120);
		}

		public void TestEnableTradeBalanceInformationSent()
		{
			TestRegistryItem(ItemSet.EnableTradeBalanceInformationSent,
				"EnableTradeBalanceInformationSent",
				OrganisationsDataRegistry.Categories.Organizations_CreditReports,
				"Enable Trade Balance Information Sending",
				"When this registry is set to 'Yes', Trade Balance Information Sending is enabled. When this registry is set to 'No', Trade Balance Information Sending is disabled.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				true);
		}

		public void TestEnableTradeBalanceServiceTaskForTestingSystem()
		{
			TestRegistryItem(ItemSet.EnableTradeBalanceServiceTaskForTestingSystem,
				"EnableTradeBalanceInformationServiceTaskForTestingSystem",
				OrganisationsDataRegistry.Categories.Organizations_CreditReports,
				"Enable Trade Balance Information Service Task For Testing System",
				"When this registry is set to 'Yes', Trade Balance Information Service Task For Testing System is enabled. When this registry is set to 'No', Trade Balance Information Service Task For Testing System is disabled.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestGetRetrospectiveMonitorTimeOut()
		{
			TestRegistryItem(ItemSet.GetRetrospectiveMonitorTimeOut,
				"GetRetrospectiveMonitor",
				"Master Data/Organizations/Credit Reports/Timeouts",
				"Get Retrospective Monitor",
				"Sets the timeout (in seconds) for Get Retrospective Monitor.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				60,
				30,
				120);
		}

		public void TestCompanyLookupTimeout()
		{
			TestRegistryItem(ItemSet.CompanyLookupTimeout,
				"CompanyLookup",
				"Master Data/Organizations/Credit Reports/Timeouts",
				"Company Lookup",
				"Sets the timeout (in seconds) for Company Lookup.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				60,
				30,
				120);
		}

		public void TestSaveTradeInformationTimeout()
		{
			TestRegistryItem(ItemSet.SaveTradeInformationTimeout,
				"SaveTradeInformation",
				"Master Data/Organizations/Credit Reports/Timeouts",
				"Save Trade Information",
				"Sets the timeout (in seconds) for Save Trade Information.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				100,
				30,
				1000);
		}

		public void TestEnableCallRealCreditReportServiceInTestingSystem()
		{
			TestRegistryItem(ItemSet.EnableRealCreditCheckServiceInTestingSystem,
				"EnableRealCreditCheckServiceInTestingSystem",
				OrganisationsDataRegistry.Categories.Organizations_CreditReports,
				"Enable Real Credit Check Service In The Testing System",
				"When this registry is set to 'Yes', will call real Credit Check Service even though in the testing system.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestDeduplicationMinimumConfidenceResult()
		{
			TestGenericRegistryItem(ItemSet.DeduplicationMinimumConfidenceResult,
				"DeduplicationMinimumConfidenceResult",
				OrganisationsDataRegistry.Categories.Organizations_DuplicateDetection_Configuration_OrganisationForm,
				"Minimum Confidence Rating",
				"Only potential duplicates where the confidence is higher than the selected value will be shown. If Medium is selected only High confidence results will be shown.",
				RegistryStorageFlags.All,
				RegistryOptions.PreserveTestValue,
				DeDuplicationMinimumConfidenceRating.Codes.Low);
		}

		public void TestOrgAddressMinimumConfidence()
		{
			var item = ItemSet.OrgAddressMinimumConfidence;
			AssertEquals("Name", "OrgAddressMinimumConfidence", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.Organizations_PatternMatch, item.Category);
			AssertEquals("Caption", "UXML Address Match Threshold", item.Caption);
			AssertEquals("Hint", "When the Match Engine is enabled, as well as reaching the overall confidence level with the organization, the potential matches must also have the address confidence higher than the selected value.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.All, item.Storage);
			AssertEquals("Default Value", 80, item.DefaultValue);
			AssertEquals("Options", RegistryOptions.PreserveTestValue, item.Options);
		}

		#region Address Validation

		public void TestAddressValidationWebServiceURIs()
		{
			var item = ItemSet.AddressValidationWebServiceURIs;
			AssertEquals("Name", "AddressValidationWebServiceURIs", item.Name);
			AssertEquals("Category", "Master Data/Organizations/Address Validation Service", item.Category);
			AssertEquals("Caption", "Address Validation Web Service URIs", item.Caption);
			AssertEquals("Hint", "Address Validation Web Service URIs", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, item.Options);
		}

		public void AssertAvsWebServiceUriRegistryBusinessObject(AvsWebServiceUriRegistryBusinessObject avsUri, string expectedType, string expectedUri, bool enabled)
		{
			AssertEquals(expectedType, avsUri.Type);
			AssertEquals(expectedUri, avsUri.ServiceUri);
			AssertEquals(enabled, avsUri.EnableSystemToSystemTrustAuthentication);
		}

		public void TestBackgroundValidationServiceTaskBatchSize()
		{
			TestRegistryItem(ItemSet.BackgroundValidationServiceTaskBatchSize,
				"BackgroundValidationServiceTaskBatchSize",
				"Master Data/Organizations/Address Validation Service/Background",
				"Background Addresss Validation (BAV) Service Task Batch Size",
				"This registry item configures the batch size for the Background Addresss Validation (BAV) Service Task.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				50,
				2,
				100);
		}

		#endregion

		#region Address Validation (Supressions)

		public void TestAddressValidationServiceSuppression()
		{
			AssertEquals("Name", "AddressValidationServiceSuppression", ItemSet.AddressValidationServiceSuppression.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.Organizations_AddressValidationService_Suppressions, ItemSet.AddressValidationServiceSuppression.Category);
			AssertEquals("Caption", "Disable Address Types for Controller", ItemSet.AddressValidationServiceSuppression.Caption);
			AssertEquals("Hint", "Disable one or more address types from the address validation process for a Controller", ItemSet.AddressValidationServiceSuppression.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.AddressValidationServiceSuppression.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.AddressValidationServiceSuppression.Options);
			AssertType(typeof(AddressListCollection), ItemSet.AddressValidationServiceSuppression.DefaultValue);
		}

		#endregion

		#region Sales & Marketing

		#region Commission

		public void TestCommissionPeriodList()
		{
			var item = ItemSet.CommissionPeriodList;
			AssertEquals("Name", "CommissionPeriodList", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_Commission, item.Category);
			AssertEquals("Caption", "Commission Entitlement Periods List", item.Caption);
			AssertEquals("Hint", @"The list of commission entitlement period options that can be selected for sales rep commission calculation.

Start and End values are measured in number of months. A value of '0' for Start or End will make the respective bounds unconstrained.
E.g. Start value of '12' and End of '0', is an entitlement period of 'From second year onwards'", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
		}

		public void TestAutoApproveFutureCommissionAgreements()
		{
			var item = ItemSet.AutoApproveFutureCommissionAgreements;
			AssertEquals("Name", "AutoApproveFutureCommissionAgreements", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_Commission, item.Category);
			AssertEquals("Caption", "Auto-Approve Future Commission Agreements", item.Caption);
			AssertEquals("Hint", @"When this registry is set to 'Yes', new or modifications to commission agreements with an effective date in the future will be automatically approved.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
		}

		public void TestCommissionRecognitionDate()
		{
			var item = ItemSet.CommissionRecognitionDate;
			AssertEquals("Name", "CommissionRecognitionDate", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_Commission, item.Category);
			AssertEquals("Caption", "Commission Recognition Date", item.Caption);
			AssertEquals("Hint", @"The date to use for commission recognition.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Default Value", CommissionRecognitionDateTypeList.Codes.PostDateOfFirstArTransaction, item.DefaultValue);
		}

		public void TestCommissionTransactionJobTrigger()
		{
			var item = ItemSet.CommissionTransactionJobTrigger;
			AssertEquals("Name", "CommissionTransactionJobTrigger", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_Commission, item.Category);
			AssertEquals("Caption", "Job Billing Triggers for Commission Transactions", item.Caption);
			AssertEquals("Hint", @"By default (CLS option), both Revenue and Profit based commission transactions are created at Billing Job = CLS status.

Overriding the value to 'REV' will allow Revenue based commission transactions to be created at Job Billing status = INV. Profit based commission calculations will continue to be created at Job Billing = CLS status.

Changes to the Job Billing Triggers will only apply to new commission transactions. Commission Agreements will have to be appended before existing unpaid transactions are rewarded.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Default Value", CommissionTransactionJobTriggerList.Codes.RevenueProfitBasedCommissionCalculationsToBeCreatedAtClsStatus, item.DefaultValue);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.ClientSizeList.Options);
		}

		public void TestOnlyShowCommissionsForCurrentLoginCompany()
		{
			TestRegistryItem(ItemSet.OnlyShowCommissionsForCurrentLoginCompany,
				"OnlyShowCommissionsForCurrentLoginCompany",
				OrganisationsDataRegistry.Categories.SalesMarketing_Commission,
				"Only Show Commissions for Current Login Company",
				"When this registry is set to 'Yes', only the commissions for the current login company are shown in the Commission Management Module.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				true);
		}

		public void TestOverwriteOldValuesOnJobClosed()
		{
			TestRegistryItem(ItemSet.OverwriteOldValuesOnJobClosed,
				"OverwriteOldValuesOnJobClosed",
				OrganisationsDataRegistry.Categories.SalesMarketing_Commission,
				"Overwrite Old Values On Job Closed",
				"When this registry is set to 'Yes', existing commissions for a job are recalculated when the job is closed. This is relevant when there has been multiple postings of charges against the job.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				false);
		}

		public void TestCommissionApprovalLevelRequired()
		{
			TestRegistryItem(ItemSet.CommissionApprovalLevelRequired,
				"CommissionApprovalLevelRequired",
				OrganisationsDataRegistry.Categories.SalesMarketing_Commission,
				"Commission Approval Level Required",
				"The number of staff required to approve entity commissions before payment can be processed.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				1,
				0,
				2);
		}

		public void TestCustomCommissionTypes()
		{
			TestRegistryItem(ItemSet.CustomCommissionTypes,
				"CustomCommissionTypes",
				OrganisationsDataRegistry.Categories.SalesMarketing_Commission,
				"Custom Commission Types",
				"Additional Commission Types that can be selected when importing manual entity commissions.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				"Enabled",
				true,
				0);
		}

		public void TestDisallowCommissionPaymentIfInvoiceNotFullyPaid()
		{
			var item = ItemSet.DisallowCommissionPaymentIfARInvoiceNotFullyPaid;
			AssertEquals("Name", "DisallowCommissionPaymentIfARInvoiceNotFullyPaid", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_Commission, item.Category);
			AssertEquals("Caption", "Disallow Commission Payment If AR Invoice(s) Have Not Been Fully Paid", item.Caption);
			AssertEquals("Hint", @"When this registry is set to 'Yes', if there are any unpaid AR invoices, the related commissions can not be processed for payment.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
		}

		public void TestCommissionFinalizerMaxNumberOfRecordsToShowInDisplayGrids()
		{
			TestRegistryItem(ItemSet.CommissionFinalizerMaxNumberOfRecordsToShowInDisplayGrids,
				"CommissionFinalizerMaxNumberOfRecordsToShowInDisplayGrids",
				OrganisationsDataRegistry.Categories.SalesMarketing_Commission,
				"Commission Finalizer Max. No. of Records to Show",
				"An error icon will show on a Display Grid if a search returns more than the maximum number of results. Results will not be displayed if this happens.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				5000,
				1,
				40000);
		}

		public void TestRunCommissionCreationInBackground()
		{
			TestRegistryItem(ItemSet.RunCommissionCreationInBackground,
				"RunCommissionCreationInBackground",
				OrganisationsDataRegistry.Categories.SalesMarketing_Commission,
				"Run Commission Creation from Saves In the Background",
				"When this registry is set to 'Yes', then if a job is closed or an invoice is posted the commission creation will be done in the background by the CGN service task.",
				RegistryStorageFlags.System,
				false);
		}

		#endregion

		#region Inquiry

		public void TestSalesEnquiryLeadInterests()
		{
			var item = ItemSet.SalesEnquiryLeadInterests;
			AssertEquals("Name", "EnquiriesLeadInterests", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_InquiryManager, item.Category);
			AssertEquals("Caption", "Lead Interest", item.Caption);
			AssertEquals("Hint", "The list of lead interests that can be used on a Sales Inquiry.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Bool Caption", "Enabled", ((CodeDescriptionBoolRegistryEditorInfo)item.EditorInfo).BoolColumnCaption);
			AssertEquals("Default Value", 3, item.DefaultValue.Count);
		}

		public void TestSalesEnquiryMandatoryFields()
		{
			var item = ItemSet.SalesEnquiryFieldsMandatory;
			AssertEquals("Name", "SalesEnquiryFieldsMandatory", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_InquiryManager, item.Category);
			AssertEquals("Caption", "Mandatory Fields", item.Caption);
			AssertEquals("Hint", "Specifies whether Inquiry fields are mandatory.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Bool Caption", "Is Mandatory", ((CodeDescriptionBoolRegistryEditorInfo)item.EditorInfo).BoolColumnCaption);
			AssertEquals("Default Value", 1, item.DefaultValue.Count);
		}

		public void TestSalesEnquiryDefaultAssignedSalesRep()
		{
			TestRegistryItem(ItemSet.SalesEnquiryDefaultAssignedSalesRep,
				"SalesEnquiryDefaultAssignedSalesRep",
				OrganisationsDataRegistry.Categories.SalesMarketing_InquiryManager,
				"Default Assigned Sales Rep",
				"If this registry is set to 'Yes', when the Organization is updated on an Inquiry, the 'Assigned Sales Rep' is automatically set to the Sales Representative assigned to the Organization for all departments.",
				RegistryStorageFlags.System,
				false);
		}

		#endregion

		#region Communication

		public void TestCommunicationMandatoryFields()
		{
			var item = ItemSet.CommunicationMandatoryFields;
			AssertEquals("Name", "CommunicationMandatoryFields", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_CommunicationManager, item.Category);
			AssertEquals("Caption", "Mandatory Fields", item.Caption);
			AssertEquals("Hint", "Specifies whether communication fields are mandatory.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Bool Caption", "Is Mandatory", ((CodeDescriptionBoolRegistryEditorInfo)item.EditorInfo).BoolColumnCaption);
			AssertEquals("Default Value", 5, item.DefaultValue.Count);
		}

		public void TestCommunicationType()
		{
			var item = ItemSet.CommunicationType;
			AssertEquals("Name", "CommunicationType", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_CommunicationManager, item.Category);
			AssertEquals("Caption", "Method", item.Caption);
			AssertEquals("Hint", "The list of valid entries for communication method.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Bool Caption", "Enabled", ((CodeDescriptionBoolRegistryEditorInfo)item.EditorInfo).BoolColumnCaption);
			AssertEquals("Default Value", 7, item.DefaultValue.Count);
		}

		public void TestCommunicationStatusList()
		{
			var item = ItemSet.CommunicationStatusList;
			AssertEquals("Name", "CommunicationStatus", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_CommunicationManager, item.Category);
			AssertEquals("Caption", "Status List", item.Caption);
			AssertEquals("Hint", "The list of valid entries for communication statuses. Each status has a 'Closed' flag which determines whether a communication is closed when in that status.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Bool Caption", "Enabled", ((CommunicationStatusRegistryEditorInfo)item.EditorInfo).BoolColumnCaption);
			AssertEquals("Default Value", 12, item.DefaultValue.Count);
		}

		public void TestCategoryList()
		{
			var item = ItemSet.CategoryList;
			AssertEquals("Name", "PurposeCategoryList", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_CommunicationManager_Purpose, item.Category);
			AssertEquals("Caption", "List", item.Caption);
			AssertEquals("Hint", "Allows you to nominate a purpose.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Bool Caption", "Enabled", ((CodeDescriptionBoolRegistryEditorInfo)item.EditorInfo).boolColumnCaption);
			AssertEquals("Default Value", 1, item.DefaultValue.Count);
			AssertEquals("Default Value", "You can change this list in the registry at " + OrganisationsDataRegistry.Categories.SalesMarketing_CommunicationManager_Purpose + "/List", item.DefaultValue[0].Description);
		}

		public void TestCommunicationAutoSendUponSave()
		{
			var item = ItemSet.CommunicationAutoSendUponSave;
			AssertEquals("Name", "CommunicationAutoSendUponSave", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_CommunicationManager, item.Category);
			AssertEquals("Caption", "Auto Send Upon Save", item.Caption);
			AssertEquals("Hint", "When this registry is set to 'Yes', calendar invitations are automatically sent upon saving communication.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Default Value", false, item.DefaultValue);
		}

		public void TestCreateCommunicationOnInquiryContactCall()
		{
			var item = ItemSet.CreateCommunicationOnInquiryContactCall;
			AssertEquals("Name", "CreateCommunicationOnInquiryContactCall", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_CommunicationManager, item.Category);
			AssertEquals("Caption", "Create Communication on Inquiry Contact Call", item.Caption);
			AssertEquals("Hint", "When this registry is set to 'Yes', a related communication is automatically created when a call is made to an inquiry contact.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Default Value", true, item.DefaultValue);
		}

		public void TestCreateCommunicationOnOpportunityContactCall()
		{
			var item = ItemSet.CreateCommunicationOnOpportunityContactCall;
			AssertEquals("Name", "CreateCommunicationOnOpportunityContactCall", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_CommunicationManager, item.Category);
			AssertEquals("Caption", "Create Communication on Opportunity Contact Call", item.Caption);
			AssertEquals("Hint", "When this registry is set to 'Yes', a related communication is automatically created when a call is made to an opportunity contact.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Default Value", true, item.DefaultValue);
		}

		public void TestCreateCommunicationOnProjectContactCall()
		{
			var item = ItemSet.CreateCommunicationOnProjectContactCall;
			AssertEquals("Name", "CreateCommunicationOnProjectContactCall", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_CommunicationManager, item.Category);
			AssertEquals("Caption", "Create Communication on Project Contact Call", item.Caption);
			AssertEquals("Hint", "When this registry is set to 'Yes', a related communication is automatically created when a call is made to a project contact.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Default Value", true, item.DefaultValue);
		}

		public void TestCommunicationInternalCalendarReminderSubjectTemplate()
		{
			var item = ItemSet.CommunicationInternalCalendarReminderSubjectTemplate;
			AssertEquals("Name", "CommunicationInternalCalendarReminderSubjectTemplate", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_CommunicationManager, item.Category);
			AssertEquals("Caption", "Internal Calendar Reminder Subject", item.Caption);
			AssertEquals("Hint", "Configure the subject for Non Client Visible calendar reminders by highlighting and adding the desired macros.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
		}

		public void TestCommunicationDocumentTypeParsedEmails()
		{
			var item = ItemSet.CommunicationDocumentTypeParsedEmails;
			AssertEquals("Name", "CommunicationDocumentTypeParsedEmails", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_CommunicationManager, item.Category);
			AssertEquals("Caption", "Document Type Parsed Emails", item.Caption);
			AssertEquals("Hint", "Document Type when parsing emails into Communication.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			IRefDocType docType = Factory.LoadTop1<IRefDocType>(new ZQuery(RefDocTypeSchema.PK, OrganisationsDataRegistry.Instance.CommunicationDocumentTypeParsedEmails.Value));
			AssertEquals("Default Value", Core.Constants.RefDocTypes.MiscellaneousDocument, docType.RT_DocType);
			var editorInfo = (GuidFindBoxRegistryEditorInfo)item.EditorInfo;
			AssertEquals("Find Box Filter", RegistryFindBoxFilter.RefDocTypeForCommunicationParsedEmail, editorInfo.FindBoxFilter);
		}

		#endregion

		#region Opportunity Management

		public void TestOpportunitySalesTypes()
		{
			CodeDescriptionBoolRegistryItem item = ItemSet.OpportunitySalesTypes;
			AssertEquals("Name", "OpportunitySalesTypes", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_OpportunityManagement, item.Category);
			AssertEquals("Caption", "Sales Types", item.Caption);
			AssertEquals("Hint", "A list of sale types for each Opportunity.", item.Hint);
			AssertEquals("Flgs", RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
		}

		public void TestOpportunityStages()
		{
			CodeDescriptionBoolRegistryItem item = ItemSet.OpportunityStages;
			AssertEquals("Name", "OpportunityStages", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_OpportunityManagement, item.Category);
			AssertEquals("Caption", "Stages", item.Caption);
			AssertEquals("Hint", "A list of stages for each Opportunity.", item.Hint);
			AssertEquals("Flgs", RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
		}

		public void TestOpportunityStatus()
		{
			var item = ItemSet.OpportunityStatus;
			AssertEquals("Name", "OpportunityManagementOpportunityStatus", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_OpportunityManagement, item.Category);
			AssertEquals("Caption", "Opportunity Status", item.Caption);
			AssertEquals("Hint", "The different statuses that can apply to an Opportunity.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.ClientSizeList.Options);

			var defaultValue = ItemSet.OpportunityStatus.DefaultValue;

			AssertEquals("DefaultValue.Count", 5, defaultValue.Count);

			AssertCodeDescriptionBool(defaultValue[0], "CRT", "Current", false);
			AssertCodeDescriptionBool(defaultValue[1], "LOS", "Lost", true);
			AssertCodeDescriptionBool(defaultValue[2], "ABA", "Abandoned", true);
			AssertCodeDescriptionBool(defaultValue[3], "SUS", "Suspended", false);
			AssertCodeDescriptionBool(defaultValue[4], "WON", "Won", true);

			AssertEquals("DefaultValue[0].EffectiveAgreement", false, defaultValue[0].EffectiveAgreement);
			AssertEquals("DefaultValue[1].EffectiveAgreement", false, defaultValue[1].EffectiveAgreement);
			AssertEquals("DefaultValue[2].EffectiveAgreement", false, defaultValue[2].EffectiveAgreement);
			AssertEquals("DefaultValue[3].EffectiveAgreement", false, defaultValue[3].EffectiveAgreement);
			AssertEquals("DefaultValue[4].EffectiveAgreement", true, defaultValue[4].EffectiveAgreement);
		}

		public void TestEstimateExpiryReasons()
		{
			TestRegistryItem(ItemSet.EstimateExpiryReasons,
				"EstimateExpiryReasons",
				OrganisationsDataRegistry.Categories.SalesMarketing_ClientIntelligence,
				"Estimate Expiry Reasons",
				"Listing of possible reasons of why estimate value is expired.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default);
		}

		public void TestDivideAllOpportunityValuesByTwelveHasRun()
		{
			TestRegistryItem(ItemSet.DivideAllOpportunityValuesByTwelveHasRun,
				"DivideAllOpportunityValuesByTwelveHasRun",
				OrganisationsDataRegistry.Categories.SalesMarketing_OpportunityManagement,
				"Divide All Opportunity Values By Twelve Has Run",
				"A flag indicating whether the one-off procedure to divide all opportunity values by twelve has been run.",
				RegistryStorageFlags.System,
				RegistryOptions.IsHidden | RegistryOptions.NotCached,
				false);
		}

		public void TestCartageEquipmentRequired()
		{
			string category = OrganisationsDataRegistry.Categories.Organizations_DefaultValues_RequiredPortTransportEquipment;
			TestRegistryItem(ItemSet.RequiredCartageEquipmentAIR, "RequiredCartageEquipmentAIR", category, "Air", "Select the default Port Transport equipment requirement for air freight. The list of air Port Transport equipment can be changed in the registry from the following location: Freight -> Shipment -> LCL/AIR Port Transport Drop Modes", RegistryStorageFlags.All, RegistryOptions.PreserveTestValue, new LCLAIREquipmentNeededList(), Constants.LCLAIREquipmentNeeded.Premise);
			TestRegistryItem(ItemSet.RequiredCartageEquipmentFCL, "RequiredCartageEquipmentFCL", category, "FCL", "Select the default Port Transport equipment requirement for FCL freight. The list of FCL Port Transport equipment can be changed in the registry from the following location: Freight -> Shipment -> FCL Port Transport Drop Modes", RegistryStorageFlags.All, RegistryOptions.PreserveTestValue, new FCLEquipmentNeededList(), Constants.FCLEquipmentNeeded.WaitForUnpack);
			TestRegistryItem(ItemSet.RequiredCartageEquipmentLCL, "RequiredCartageEquipmentLCL", category, "LCL", "Select the default Port Transport equipment requirement for LCL freight. The list of LCL Port Transport equipment can be changed in the registry from the following location: Freight -> Shipment -> LCL/AIR Port Transport Drop Modes", RegistryStorageFlags.All, RegistryOptions.PreserveTestValue, new LCLAIREquipmentNeededList(), Constants.LCLAIREquipmentNeeded.Premise);

			CodePairRegistryDataType dataTypeAIR = (CodePairRegistryDataType)ItemSet.RequiredCartageEquipmentAIR.DataType;
			CodePairRegistryDataType dataTypeFCL = (CodePairRegistryDataType)ItemSet.RequiredCartageEquipmentFCL.DataType;
			CodePairRegistryDataType dataTypeLCL = (CodePairRegistryDataType)ItemSet.RequiredCartageEquipmentLCL.DataType;

			Assert("Custom Code 1 not in AIR list", !dataTypeAIR.LookUpList.ContainsCode("CC1"));
			Assert("Custom Code 2 not in AIR list", !dataTypeAIR.LookUpList.ContainsCode("CC2"));
			Assert("Custom Code 3 not in AIR list", !dataTypeAIR.LookUpList.ContainsCode("CC3"));

			Assert("Custom Code 1 not in FCL list", !dataTypeFCL.LookUpList.ContainsCode("CC1"));
			Assert("Custom Code 2 not in FCL list", !dataTypeFCL.LookUpList.ContainsCode("CC2"));
			Assert("Custom Code 3 not in FCL list", !dataTypeFCL.LookUpList.ContainsCode("CC3"));

			Assert("Custom Code 1 not in LCL list", !dataTypeLCL.LookUpList.ContainsCode("CC1"));
			Assert("Custom Code 2 not in LCL list", !dataTypeLCL.LookUpList.ContainsCode("CC2"));
			Assert("Custom Code 3 not in LCL list", !dataTypeLCL.LookUpList.ContainsCode("CC3"));

			CodeDescriptionPairList listForAirLCL = new CodeDescriptionPairList();
			listForAirLCL.AddPair("CC1", "Custom Code 1");

			CodeDescriptionPairList listForFCL = new CodeDescriptionPairList();
			listForFCL.AddPair("CC2", "Custom Code 2");
			listForFCL.AddPair("CC3", "Custom Code 3");

			ReadOnlyCodeDescriptionPairList fCLListOriginalValue = Env.Registry.RawRegistry.FCLEquipmentNeeded.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			ReadOnlyCodeDescriptionPairList lCLAirListOriginalValue = Env.Registry.RawRegistry.LCLAIREquipmentNeeded.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			try
			{
				Env.Registry.RawRegistry.FCLEquipmentNeeded.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, listForFCL);
				Env.Registry.RawRegistry.LCLAIREquipmentNeeded.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, listForAirLCL);

				Assert("List refreshed using provider - Custom Code 1 IS in AIR list", dataTypeAIR.LookUpList.ContainsCode("CC1"));
				Assert("List refreshed using provider - Custom Code 2 not in AIR list", !dataTypeAIR.LookUpList.ContainsCode("CC2"));
				Assert("List refreshed using provider - Custom Code 3 not in AIR list", !dataTypeAIR.LookUpList.ContainsCode("CC3"));

				Assert("List refreshed using provider - Custom Code 1 not in FCL list", !dataTypeFCL.LookUpList.ContainsCode("CC1"));
				Assert("List refreshed using provider - Custom Code 2 IS in FCL list", dataTypeFCL.LookUpList.ContainsCode("CC2"));
				Assert("List refreshed using provider - Custom Code 3 IS in FCL list", dataTypeFCL.LookUpList.ContainsCode("CC3"));

				Assert("List refreshed using provider - Custom Code 1 IS not in LCL list", dataTypeLCL.LookUpList.ContainsCode("CC1"));
				Assert("List refreshed using provider - Custom Code 2 NOT in LCL list", !dataTypeLCL.LookUpList.ContainsCode("CC2"));
				Assert("List refreshed using provider - Custom Code 3 NOT in LCL list", !dataTypeLCL.LookUpList.ContainsCode("CC3"));
			}
			finally
			{
				Env.Registry.RawRegistry.FCLEquipmentNeeded.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fCLListOriginalValue);
				Env.Registry.RawRegistry.LCLAIREquipmentNeeded.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, lCLAirListOriginalValue);
			}
		}

		public void TestOpportunityManagementRegistryItems()
		{
			TestRegistryItem(ItemSet.ProductTypeLabel, "ExtraCategoryLabel", OrganisationsDataRegistry.Categories.SalesMarketing_OpportunityManagement + "/Product Type", "Label", "Allows you to further categorize your opportunities, by a product type identifier.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default, TextEditorType.TextBox, "Product Type");
			TestRegistryItem(ItemSet.PotentialLabel, "RentalMultiplierLabel", OrganisationsDataRegistry.Categories.SalesMarketing_OpportunityManagement + "/Customizable Labels", "Potential Label", "Allows you to further categorize your opportunities, by a Potential identifier with customizable label.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default, TextEditorType.TextBox, "Potential");
			TestRegistryItem(ItemSet.CurrentLabel, "TotalDiscountLabel", OrganisationsDataRegistry.Categories.SalesMarketing_OpportunityManagement + "/Customizable Labels", "Current Label", "Allows you to further categorize your opportunities, by a Current identifier with customizable label.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default, TextEditorType.TextBox, "Current");
		}

		public void TestOpportunityOutcomeRegistryItem()
		{
			var item = ItemSet.OpportunityOutcome;
			AssertEquals("Name", "OpportunityManagementOpportunityOutcome", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_OpportunityManagement, item.Category);
			AssertEquals("Caption", "Opportunity Outcome", item.Caption);
			AssertEquals("Hint", "The possible outcomes of a Sales Opportunity.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Options", RegistryOptions.Default, item.Options);
		}

		public void TestClosedOpportunityReasonsRegistryItem()
		{
			var item = ItemSet.ClosedOpportunityReasons;
			AssertEquals("Name", "ClosedOpportunityReasons", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_OpportunityManagement, item.Category);
			AssertEquals("Caption", "Closed Opportunity Reasons", item.Caption);
			AssertEquals("Hint", "Listing of possible reasons a Sales Opportunity was closed.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Options", RegistryOptions.Default, item.Options);
			AssertType(typeof(OpportunityClosedReasonsCollection), item.Value);
			AssertType(typeof(CodeSelectionCollection), item.DefaultValue[0].StatusRules);
		}

		public void TestOpportunitySourceListRegistryItem()
		{
			CodeDescriptionBoolRelatedItemRegistryItem item = ItemSet.OpportunitySource;
			AssertEquals("Name", "OpportunityManagementOpportunitySource", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_OpportunityManagement, item.Category);
			AssertEquals("Caption", "Opportunity Source", item.Caption);
			AssertEquals("Hint", "A list of sources that generated this Sales Opportunity.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Options", RegistryOptions.Default, item.Options);
		}

		public void TestOpportunityProductTypeListRegistryItem()
		{
			CodeDescriptionBoolRegistryItem item = ItemSet.ProductTypeList;
			AssertEquals("Name", "ExtraCategoryList", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_OpportunityManagement + "/Product Type", item.Category);
			AssertEquals("Caption", "List", item.Caption);
			AssertEquals("Hint", "Allows you to nominate your product type list.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Options", RegistryOptions.Default, item.Options);
		}

		public void TestOpportunityManagementFieldsMandatoryRegistryItem()
		{
			var item = ItemSet.OpportunityManagementFieldsMandatory;
			AssertEquals("Name", "OpportunityManagementFieldsMandatory", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_OpportunityManagement, item.Category);
			AssertEquals("Caption", "Mandatory Fields", item.Caption);
			AssertEquals("Hint", "Specifies whether Opportunity fields are mandatory", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Default Value", 6, item.DefaultValue.Count);

			var entries = item.DefaultValue;
			AssertEquals("Description Default Value", true, entries[0].Bool);
			AssertEquals("Estimated Value Currency Default Value", true, entries[1].Bool);
			AssertEquals("Package Type Default Value", false, entries[2].Bool);
			AssertEquals("Outcome Default Value", false, entries[3].Bool);
			AssertEquals("Source Default Value", false, entries[4].Bool);
			AssertEquals("Sales Person Default Value", false, entries[5].Bool);
		}

		public void TestCompetitorTypeRegistryItem()
		{
			var item = ItemSet.CompetitorType;
			AssertEquals("Name", "CompetitorType", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.Organizations_Sales, item.Category);
			AssertEquals("Caption", "Competitor Type", item.Caption);
			AssertEquals("Hint", "The List of Competitor types that can be set on each Organization.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);

			var defaultValue = item.DefaultValue;

			AssertEquals("DefaultValue.Count", 4, defaultValue.Count);

			CombineAssertions(() =>
			{
				AssertCodeDescriptionBool(defaultValue[0], "CMB", "Customs", true);
				AssertCodeDescriptionBool(defaultValue[1], "CMF", "Forwarding", true);
				AssertCodeDescriptionBool(defaultValue[2], "CMD", "Land Transport", true);
				AssertCodeDescriptionBool(defaultValue[3], "CMW", "Warehouse", true);

				Assert("DefaultValue[0].SystemDefined", defaultValue[0].SystemDefined);
				Assert("DefaultValue[1].SystemDefined", defaultValue[1].SystemDefined);
				Assert("DefaultValue[2].SystemDefined", defaultValue[2].SystemDefined);
				Assert("DefaultValue[3].SystemDefined", defaultValue[3].SystemDefined);
			});
		}

		#endregion

		#region Sales Product

		public void TestPeriodOfActivityTypes()
		{
			var item = ItemSet.PeriodOfActivityTypes;
			AssertEquals("Name", "PeriodOfActivityTypes", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_ClientIntelligence, item.Category);
			AssertEquals("Caption", "Period of Activity Types", item.Caption);
			AssertEquals("Hint", @"The list of period of activity types.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
		}

		public void TestIndustryVerticalTypes()
		{
			var item = ItemSet.IndustryVerticalTypes;
			AssertEquals("Name", "IndustryVerticalTypes", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_ClientIntelligence, item.Category);
			AssertEquals("Caption", "Vertical Market Types", item.Caption);
			AssertEquals("Hint", @"The list of vertical market types.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
		}

		#endregion

		#region GLOW

		#region Enable Glow Opportunity Registry Settings

		public void TestEnableGlowSalesMarketingRegistrySettings()
		{
			TestRegistryItem(
					ItemSet.EnableGlowSalesMarketingRegistrySettings,
					"EnableGlowSalesMarketingRegistrySettings",
					OrganisationsDataRegistry.Categories.SalesMarketing,
					"Enable GLOW registry settings",
					"When enabled, the GLOW registry settings appear under Sales & Marketing.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false);
		}

		#endregion

		#region Communication Management

		#region Mandatory Fields

		public void TestGlowCommunicationFieldsMandatory()
		{
			var item = ItemSet.GlowCommunicationFieldsMandatory;
			AssertEquals("Name", "GlowCommunicationFieldsMandatory", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_Glow_CommunicationManagement, item.Category);
			AssertEquals("Caption", "Mandatory Fields", item.Caption);
			AssertEquals("Hint", "Specifies whether Communication fields are mandatory.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Option is set to hidden by default", RegistryOptions.IsHidden, item.Options);

			using (ItemSet.EnableGlowSalesMarketingRegistrySettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ItemSet.RemoveItemFromCacheIfOlderThan("GlowCommunicationFieldsMandatory", TimeSpan.FromMilliseconds(-1));

				item = ItemSet.GlowCommunicationFieldsMandatory;
				Assert("Precondition: EnableGlowOpportunityRegistrySettings is set to true", ItemSet.EnableGlowSalesMarketingRegistrySettings.Value);
				AssertEquals("Option is set to default when EnableGlowOpportunityRegistrySettings is set to true", RegistryOptions.Default, item.Options);
			}

			var defaultValue = item.DefaultValue;

			AssertEquals("DefaultValue.Count", 5, defaultValue.Count);
			AssertEquals("DefaultValue Allow New", false, defaultValue.AllowNew);

			CombineAssertions(() =>
			{
				AssertCodeDescriptionBool(defaultValue[0], OrgSalesCallSchema.Constants.OQ_CallDate, "Date of activity", true);
				AssertCodeDescriptionBool(defaultValue[1], OrgSalesCallSchema.Constants.OQ_CallSummary, "Subject", true);
				AssertCodeDescriptionBool(defaultValue[2], OrgSalesCallSchema.Constants.OQ_Status, "Status", false);
				AssertCodeDescriptionBool(defaultValue[3], OrgSalesCallSchema.Constants.OQ_TypeOfCall, "Method", false);
				AssertCodeDescriptionBool(defaultValue[4], OrgSalesCallSchema.Constants.OQ_Category, "Purpose", false);

				AssertEquals("DefaultValue[0].SystemDefined", true, defaultValue[0].SystemDefined);
				AssertEquals("DefaultValue[1].SystemDefined", true, defaultValue[1].SystemDefined);
				AssertEquals("DefaultValue[2].SystemDefined", false, defaultValue[2].SystemDefined);
				AssertEquals("DefaultValue[3].SystemDefined", false, defaultValue[3].SystemDefined);
				AssertEquals("DefaultValue[4].SystemDefined", false, defaultValue[4].SystemDefined);
			});
		}

		#endregion

		#region Method

		public void TestGlowCommunicationMethod()
		{
			var item = ItemSet.GlowCommunicationMethod;
			AssertEquals("Name", "GlowCommunicationMethod", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_Glow_CommunicationManagement, item.Category);
			AssertEquals("Caption", "Method", item.Caption);
			AssertEquals("Hint", "The list of valid entries for communication method.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Option is set to hidden by default", RegistryOptions.IsHidden, item.Options);

			using (ItemSet.EnableGlowSalesMarketingRegistrySettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ItemSet.RemoveItemFromCacheIfOlderThan("GlowCommunicationMethod", TimeSpan.FromMilliseconds(-1));

				item = ItemSet.GlowCommunicationMethod;
				Assert("Precondition: EnableGlowOpportunityRegistrySettings is set to true", ItemSet.EnableGlowSalesMarketingRegistrySettings.Value);
				AssertEquals("Option is set to default when EnableGlowOpportunityRegistrySettings is set to true", RegistryOptions.Default, item.Options);
			}

			var defaultValues = ItemSet.GlowCommunicationMethod.DefaultValue;
			AssertEquals("Should not be able to change system defined bool and description should be mandatory", typeof(OverrideImmuneCodeDescriptionBoolWithMandatoryDescriptionCollection), defaultValues.GetType());
			AssertEquals("DefaultValue.Count", 3, defaultValues.Count);

			AssertCodeDescriptionBool(defaultValues[0], "PHN", "Phone call", true);
			AssertCodeDescriptionBool(defaultValues[1], "MTG", "Meeting", true);
			AssertCodeDescriptionBool(defaultValues[2], "EML", "Email", true);

			AssertEquals("DefaultValue[0].SystemDefined", true, defaultValues[0].SystemDefined);
			AssertEquals("DefaultValue[1].SystemDefined", true, defaultValues[1].SystemDefined);
			AssertEquals("DefaultValue[2].SystemDefined", true, defaultValues[2].SystemDefined);
		}

		#endregion

		#region Purpose

		public void TestGlowCommunicationPurpose()
		{
			var item = ItemSet.GlowCommunicationPurpose;
			AssertEquals("Name", "GlowCommunicationPurpose", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_Glow_CommunicationManagement, item.Category);
			AssertEquals("Caption", "Purpose", item.Caption);
			AssertEquals("Hint", "Allows you to nominate a purpose.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Option is set to hidden by default", RegistryOptions.IsHidden, item.Options);

			using (ItemSet.EnableGlowSalesMarketingRegistrySettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ItemSet.RemoveItemFromCacheIfOlderThan("GlowCommunicationPurpose", TimeSpan.FromMilliseconds(-1));

				item = ItemSet.GlowCommunicationPurpose;
				Assert("Precondition: EnableGlowOpportunityRegistrySettings is set to true", ItemSet.EnableGlowSalesMarketingRegistrySettings.Value);
				AssertEquals("Option is set to default when EnableGlowOpportunityRegistrySettings is set to true", RegistryOptions.Default, item.Options);
			}

			var defaultValues = ItemSet.GlowCommunicationPurpose.DefaultValue;
			AssertEquals("Description should be mandatory", typeof(CodeDescriptionBoolWithMandatoryDescriptionCollection), defaultValues.GetType());
			AssertEquals("DefaultValue.Count", 2, defaultValues.Count);

			AssertCodeDescriptionBool(defaultValues[0], "FOL", "Follow-up", true);
			AssertCodeDescriptionBool(defaultValues[1], "INI", "Initial contact", true);

			AssertEquals("DefaultValue[0].SystemDefined", false, defaultValues[0].SystemDefined);
			AssertEquals("DefaultValue[1].SystemDefined", false, defaultValues[1].SystemDefined);
		}

		#endregion

		#endregion

		#region Opportunity Management

		#region Contact Roles

		public void TestGlowContactRolesRegistryItem()
		{
			var item = ItemSet.GlowOpportunityContactRoles;
			AssertEquals("Name", "GlowOpportunityContactRoles", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_Glow_OpportunityManagement, item.Category);
			AssertEquals("Caption", "Contact Roles", item.Caption);
			AssertEquals("Hint", "List of valid contact roles in Web Portal.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Option is set to hidden by default", RegistryOptions.IsHidden, item.Options);

			using (ItemSet.EnableGlowSalesMarketingRegistrySettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ItemSet.RemoveItemFromCacheIfOlderThan("GlowOpportunityContactRoles", TimeSpan.FromMilliseconds(-1));

				item = ItemSet.GlowOpportunityContactRoles;
				Assert("Precondition: EnableGlowOpportunityRegistrySettings is set to true", ItemSet.EnableGlowSalesMarketingRegistrySettings.Value);
				AssertEquals("Option is set to default when EnableGlowOpportunityRegistrySettings is set to true", RegistryOptions.Default, item.Options);
			}
		}

		#endregion

		#region Lead Source

		public void TestGlowOpportunityLeadSourceRegistryItem()
		{
			var item = ItemSet.GlowOpportunityLeadSource;
			AssertEquals("Name", "GlowOpportunityLeadSource", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_Glow_OpportunityManagement, item.Category);
			AssertEquals("Caption", "Lead Source", item.Caption);
			AssertEquals("Hint", "A list of sources that generated this Sales Opportunity.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Option is set to hidden by default", RegistryOptions.IsHidden, item.Options);

			using (ItemSet.EnableGlowSalesMarketingRegistrySettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ItemSet.RemoveItemFromCacheIfOlderThan("GlowOpportunityLeadSource", TimeSpan.FromMilliseconds(-1));

				item = ItemSet.GlowOpportunityLeadSource;
				Assert("Precondition: EnableGlowOpportunityRegistrySettings is set to true", ItemSet.EnableGlowSalesMarketingRegistrySettings.Value);
				AssertEquals("Option is set to default when EnableGlowOpportunityRegistrySettings is set to true", RegistryOptions.Default, item.Options);
			}
		}

		#endregion

		#region Mandatory Fields

		public void TestGlowOpportunityFieldsMandatoryRegistryItem()
		{
			var item = ItemSet.GlowOpportunityFieldsMandatory;
			AssertEquals("Name", "GlowOpportunityFieldsMandatory", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_Glow_OpportunityManagement, item.Category);
			AssertEquals("Caption", "Mandatory Fields", item.Caption);
			AssertEquals("Hint", "Specifies whether Opportunity fields are mandatory.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Option is set to hidden by default", RegistryOptions.IsHidden, item.Options);

			using (ItemSet.EnableGlowSalesMarketingRegistrySettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ItemSet.RemoveItemFromCacheIfOlderThan("GlowOpportunityFieldsMandatory", TimeSpan.FromMilliseconds(-1));

				item = ItemSet.GlowOpportunityFieldsMandatory;
				Assert("Precondition: EnableGlowOpportunityRegistrySettings is set to true", ItemSet.EnableGlowSalesMarketingRegistrySettings.Value);
				AssertEquals("Option is set to default when EnableGlowOpportunityRegistrySettings is set to true", RegistryOptions.Default, item.Options);
			}

			var defaultValue = item.DefaultValue;

			AssertEquals("DefaultValue.Count", 8, defaultValue.Count);
			AssertEquals("DefaultValue Allow New", false, defaultValue.AllowNew);

			CombineAssertions(() =>
			{
				AssertCodeDescriptionBool(defaultValue[0], CrmOpportunitySchema.Constants.COP_OH_Organization, "Organization", true);
				AssertCodeDescriptionBool(defaultValue[1], CrmOpportunitySchema.Constants.COP_OpportunityName, "Opportunity name", true);
				AssertCodeDescriptionBool(defaultValue[2], CrmOpportunitySchema.Constants.COP_ProductType, "Product type", false);
				AssertCodeDescriptionBool(defaultValue[3], CrmOpportunitySchema.Constants.COP_GS_NKSalesPerson, "Sales person", false);
				AssertCodeDescriptionBool(defaultValue[4], CrmOpportunitySchema.Constants.COP_SalesType, "Sales type", false);
				AssertCodeDescriptionBool(defaultValue[5], CrmOpportunitySchema.Constants.COP_SourceType, "Source type", false);
				AssertCodeDescriptionBool(defaultValue[6], CrmOpportunitySchema.Constants.COP_Stage, "Stage", true);
				AssertCodeDescriptionBool(defaultValue[7], CrmOpportunitySchema.Constants.COP_Status, "Status", false);

				AssertEquals("DefaultValue[0].SystemDefined", true, defaultValue[0].SystemDefined);
				AssertEquals("DefaultValue[1].SystemDefined", true, defaultValue[1].SystemDefined);
				AssertEquals("DefaultValue[2].SystemDefined", false, defaultValue[2].SystemDefined);
				AssertEquals("DefaultValue[3].SystemDefined", false, defaultValue[3].SystemDefined);
				AssertEquals("DefaultValue[4].SystemDefined", false, defaultValue[4].SystemDefined);
				AssertEquals("DefaultValue[5].SystemDefined", false, defaultValue[5].SystemDefined);
				AssertEquals("DefaultValue[6].SystemDefined", true, defaultValue[6].SystemDefined);
				AssertEquals("DefaultValue[7].SystemDefined", false, defaultValue[7].SystemDefined);
			});
		}

		#endregion

		#region Product type

		public void TestGlowOpportunityProductType()
		{
			CodeDescriptionBoolRegistryItem item = ItemSet.GlowOpportunityProductType;
			AssertEquals("Name", "GlowOpportunityProductType", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_Glow_OpportunityManagement, item.Category);
			AssertEquals("Caption", "Product Type", item.Caption);
			AssertEquals("Hint", "Allows you to nominate your product type list.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Option is set to hidden by default", RegistryOptions.IsHidden, item.Options);

			using (ItemSet.EnableGlowSalesMarketingRegistrySettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ItemSet.RemoveItemFromCacheIfOlderThan("GlowOpportunityProductType", TimeSpan.FromMilliseconds(-1));

				item = ItemSet.GlowOpportunityProductType;
				Assert("Precondition: EnableGlowOpportunityRegistrySettings is set to true", ItemSet.EnableGlowSalesMarketingRegistrySettings.Value);
				AssertEquals("Option is set to default when EnableGlowOpportunityRegistrySettings is set to true", RegistryOptions.Default, item.Options);
			}

			var defaultValue = ItemSet.GlowOpportunityProductType.DefaultValue;
			AssertEquals("DefaultValue.Count", 1, defaultValue.Count);
			AssertCodeDescriptionBool(defaultValue[0], "FWD", "Forwarding", true);
		}

		#endregion

		#region Sales Types

		public void TestGlowOpportunitySalesTypesRegistryItem()
		{
			var item = ItemSet.GlowOpportunitySalesTypes;
			AssertEquals("Name", "GlowOpportunitySalesTypes", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_Glow_OpportunityManagement, item.Category);
			AssertEquals("Caption", "Sales Types", item.Caption);
			AssertEquals("Hint", "A list of sale types for each Opportunity.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Option is set to hidden by default", RegistryOptions.IsHidden, item.Options);

			using (ItemSet.EnableGlowSalesMarketingRegistrySettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ItemSet.RemoveItemFromCacheIfOlderThan("GlowOpportunitySalesTypes", TimeSpan.FromMilliseconds(-1));

				item = ItemSet.GlowOpportunitySalesTypes;
				Assert("Precondition: EnableGlowOpportunityRegistrySettings is set to true", ItemSet.EnableGlowSalesMarketingRegistrySettings.Value);
				AssertEquals("Option is set to default when EnableGlowOpportunityRegistrySettings is set to true", RegistryOptions.Default, item.Options);
			}

			var defaultValues = ItemSet.GlowOpportunitySalesTypes.DefaultValue;

			AssertEquals("DefaultValues.Count", 2, defaultValues.Count);
			AssertCodeDescriptionBool(defaultValues[0], "NEW", "New Business", true);
			AssertCodeDescriptionBool(defaultValues[1], "EXS", "Develop existing business", true);
		}

		#endregion

		#region Opportunity Stages

		public void TestGlowOpportunityStages()
		{
			GlowOpportunityStageRegistryItem item = ItemSet.GlowOpportunityStages;
			AssertEquals("Name", "GlowOpportunityStages", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_Glow_OpportunityManagement, item.Category);
			AssertEquals("Caption", "Stages", item.Caption);
			AssertEquals("Hint", @"A sequence of stages for each opportunity and its nominated Winning probability.
Acceptable values for Win Probabilities are integers between 0 and 100.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Option is set to hidden by default", RegistryOptions.IsHidden, item.Options);

			using (ItemSet.EnableGlowSalesMarketingRegistrySettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ItemSet.RemoveItemFromCacheIfOlderThan("GlowOpportunityStages", TimeSpan.FromMilliseconds(-1));

				item = ItemSet.GlowOpportunityStages;
				Assert("Precondition: EnableGlowOpportunityRegistrySettings is set to true", ItemSet.EnableGlowSalesMarketingRegistrySettings.Value);
				AssertEquals("Option is set to default when EnableGlowOpportunityRegistrySettings is set to true", RegistryOptions.Default, item.Options);
			}

			var defaultValue = ItemSet.GlowOpportunityStages.DefaultValue;

			AssertEquals("DefaultValue.Count", 1, defaultValue.Count);
			AssertEquals("DefaultValue[0].Code", "NEW", defaultValue[0].Code);
			AssertEquals("DefaultValue[0].Description", "New", defaultValue[0].Description);
			AssertEquals("DefaultValue[0].WinProbability", 0, defaultValue[0].WinProbability);
		}

		#endregion

		#region Opportunity Status

		public void TestGlowOpportunityStatus()
		{
			var item = ItemSet.GlowOpportunityStatuses;
			AssertEquals("Name", "GlowOpportunityStatuses", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_Glow_OpportunityManagement, item.Category);
			AssertEquals("Caption", "Opportunity Status", item.Caption);
			AssertEquals("Hint", "The different statuses that can apply to an Opportunity.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Option is set to hidden by default", RegistryOptions.IsHidden, item.Options);

			using (ItemSet.EnableGlowSalesMarketingRegistrySettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ItemSet.RemoveItemFromCacheIfOlderThan("GlowOpportunityStatuses", TimeSpan.FromMilliseconds(-1));

				item = ItemSet.GlowOpportunityStatuses;
				Assert("Precondition: EnableGlowOpportunityRegistrySettings is set to true", ItemSet.EnableGlowSalesMarketingRegistrySettings.Value);
				AssertEquals("Option is set to default when EnableGlowOpportunityRegistrySettings is set to true", RegistryOptions.Default, item.Options);
			}

			var defaultValues = ItemSet.GlowOpportunityStatuses.DefaultValue;

			AssertEquals("DefaultValue.Count", 5, defaultValues.Count);

			AssertEquals("DefaultValue[0].Code", "CRT", defaultValues[0].Code);
			AssertEquals("DefaultValue[1].Code", "LOS", defaultValues[1].Code);
			AssertEquals("DefaultValue[2].Code", "ABA", defaultValues[2].Code);
			AssertEquals("DefaultValue[3].Code", "SUS", defaultValues[3].Code);
			AssertEquals("DefaultValue[4].Code", "WON", defaultValues[4].Code);

			AssertEquals("DefaultValue[0].Description", "Current", defaultValues[0].Description);
			AssertEquals("DefaultValue[1].Description", "Lost", defaultValues[1].Description);
			AssertEquals("DefaultValue[2].Description", "Abandoned", defaultValues[2].Description);
			AssertEquals("DefaultValue[3].Description", "Suspended", defaultValues[3].Description);
			AssertEquals("DefaultValue[4].Description", "Won", defaultValues[4].Description);

			AssertEquals("DefaultValue[0].Bool", true, defaultValues[0].Bool);
			AssertEquals("DefaultValue[1].Bool", true, defaultValues[1].Bool);
			AssertEquals("DefaultValue[2].Bool", true, defaultValues[2].Bool);
			AssertEquals("DefaultValue[3].Bool", true, defaultValues[3].Bool);
			AssertEquals("DefaultValue[4].Bool", true, defaultValues[4].Bool);

			AssertEquals("DefaultValue[0].TradeStatus", OpportunityTradeStatus.Codes.Active, defaultValues[0].TradeStatus);
			AssertEquals("DefaultValue[1].TradeStatus", OpportunityTradeStatus.Codes.Unsuccessful, defaultValues[1].TradeStatus);
			AssertEquals("DefaultValue[2].TradeStatus", OpportunityTradeStatus.Codes.Unsuccessful, defaultValues[2].TradeStatus);
			AssertEquals("DefaultValue[3].TradeStatus", OpportunityTradeStatus.Codes.Active, defaultValues[3].TradeStatus);
			AssertEquals("DefaultValue[4].TradeStatus", OpportunityTradeStatus.Codes.Successful, defaultValues[4].TradeStatus);
		}

		#endregion

		#region Allow Restricted Opportunities

		public void TestAllowRestrictedOpportunities()
		{
			var item = ItemSet.AllowRestrictedOpportunities;
			AssertEquals("Name", "AllowRestrictedOpportunities", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_Glow_OpportunityManagement, item.Category);
			AssertEquals("Caption", "Allow Restricted Opportunities", item.Caption);
			AssertEquals("Hint", @"When enabled, this allows users to create opportunities that can be accessed only by specific staff as defined within the opportunity.

If this setting is disabled later, such restricted opportunities can no longer be created, however the ones that were created previously shall continue to remain restricted. Users that had access to specific restricted opportunities can open them and remove restrictions if so desired.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Option is set to hidden by default", RegistryOptions.IsHidden, item.Options);
			AssertEquals("Default Value", false, item.DefaultValue);

			using (ItemSet.EnableGlowSalesMarketingRegistrySettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ItemSet.RemoveItemFromCacheIfOlderThan("AllowRestrictedOpportunities", TimeSpan.FromMilliseconds(-1));

				item = ItemSet.AllowRestrictedOpportunities;
				Assert("Precondition: EnableGlowSalesMarketingRegistrySettings is set to true", ItemSet.EnableGlowSalesMarketingRegistrySettings.Value);
				AssertEquals("Option is set to default when EnableGlowOpportunityRegistrySettings is set to true", RegistryOptions.Default, item.Options);
			}
		}

		#endregion

		#region Opportunity Scope

		#region Configurable Parameters

		#region Forwarding

		public void TestOpportunityScopeForwardingParameters()
		{
			var item = ItemSet.OpportunityScopeForwardingParameters;
			AssertEquals("Name", "OpportunityScopeForwardingParameters", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_Glow_OpportunityManagement_OppScopeConfigurableParameters_ConfigurableParameters, item.Category);
			AssertEquals("Caption", "Forwarding", item.Caption);
			AssertEquals("Hint", "This is a list of additional parameters that can be added to a Forwarding scope.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Option is set to hidden by default", RegistryOptions.IsHidden, item.Options);

			using (ItemSet.EnableGlowSalesMarketingRegistrySettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ItemSet.RemoveItemFromCacheIfOlderThan("OpportunityScopeForwardingParameters", TimeSpan.FromMilliseconds(-1));

				item = ItemSet.OpportunityScopeForwardingParameters;
				Assert("Precondition: EnableGlowSalesMarketingRegistrySettings is set to true", ItemSet.EnableGlowSalesMarketingRegistrySettings.Value);
				AssertEquals("Option is set to default when EnableGlowOpportunityRegistrySettings is set to true", RegistryOptions.Default, item.Options);
			}

			var defaultValue = item.DefaultValue;

			AssertEquals("DefaultValue.Count", 5, defaultValue.Count);
			AssertEquals("DefaultValue Allow New", false, defaultValue.AllowNew);

			CombineAssertions(() =>
			{
				AssertCodeDescriptionBool(defaultValue[0], "PickupDeliveryAddress", "Pickup & Delivery address", true);
				AssertCodeDescriptionBool(defaultValue[1], "Container", "Container / ULD", false);
				AssertCodeDescriptionBool(defaultValue[2], "Commodity", "Commodity", false);
				AssertCodeDescriptionBool(defaultValue[3], "Incoterm", "Incoterm", false);
				AssertCodeDescriptionBool(defaultValue[4], "ServiceLevel", "Service level", false);
			});
		}

		#endregion

		#endregion

		#region Priority Sequence

		#region Forwarding

		public void TestOpportunityScopeForwardingPrioritySequence()
		{
			var item = ItemSet.OpportunityScopeForwardingPrioritySequence;
			AssertEquals("Name", "OpportunityScopeForwardingPrioritySequence", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_Glow_OpportunityManagement_OppScopeConfigurableParameters_PrioritySequence, item.Category);
			AssertEquals("Caption", "Forwarding", item.Caption);
			AssertEquals("Hint", "This is the priority sequence in which the additional scoping parameters on Forwarding opportunity scopes are used to link quotes and jobs. When you alter the sequence the same shall take effect from subsequent linkages.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Option is set to hidden by default", RegistryOptions.IsHidden, item.Options);

			using (ItemSet.EnableGlowSalesMarketingRegistrySettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ItemSet.RemoveItemFromCacheIfOlderThan("OpportunityScopeForwardingPrioritySequence", TimeSpan.FromMilliseconds(-1));

				item = ItemSet.OpportunityScopeForwardingPrioritySequence;
				Assert("Precondition: EnableGlowSalesMarketingRegistrySettings is set to true", ItemSet.EnableGlowSalesMarketingRegistrySettings.Value);
				AssertEquals("Option is set to default when EnableGlowOpportunityRegistrySettings is set to true", RegistryOptions.Default, item.Options);
			}

			var defaultValue = item.DefaultValue;

			AssertEquals("DefaultValue.Count", 4, defaultValue.Count);

			CombineAssertions(() =>
			{
				AssertEquals("Commodity", defaultValue[0].Description);
				AssertEquals("Service level", defaultValue[1].Description);
				AssertEquals("Incoterm", defaultValue[2].Description);
				AssertEquals("Container / ULD", defaultValue[3].Description);
			});
		}

		#endregion

		#endregion

		#endregion

		#region Auto-linking opportunity and quote scopes

		public void TestEnableAutoLinkingOpportunityAndQuoteScopes()
		{
			var item = ItemSet.EnableAutoLinkingOpportunityAndQuoteScopes;
			AssertEquals("Name", "EnableAutoLinkingOpportunityAndQuoteScopes", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_Glow_OpportunityManagement, item.Category);
			AssertEquals("Caption", "Auto-linking opportunity and quote scopes", item.Caption);
			AssertEquals("Hint", "When set to 'Yes' the system attempts to automatically link quote scopes to relevant opportunity scopes. Automatic linking is disabled when this setting is 'No'. Manual linking and unlinking is supported at all times.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Option is set to hidden by default", RegistryOptions.IsHidden, item.Options);
			AssertEquals("Default Value", true, item.DefaultValue);

			using (ItemSet.EnableGlowSalesMarketingRegistrySettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ItemSet.RemoveItemFromCacheIfOlderThan("EnableAutoLinkingOpportunityAndQuoteScopes", TimeSpan.FromMilliseconds(-1));

				item = ItemSet.EnableAutoLinkingOpportunityAndQuoteScopes;
				Assert("Precondition: EnableGlowSalesMarketingRegistrySettings is set to true", ItemSet.EnableGlowSalesMarketingRegistrySettings.Value);
				AssertEquals("Option is set to default when EnableGlowOpportunityRegistrySettings is set to true", RegistryOptions.Default, item.Options);
			}
		}

		#endregion

		#endregion

		#region Temporary Organisation

		public void TestEnableCreationOfTemporaryOrganizations()
		{
			var item = ItemSet.EnableCreationOfTemporaryOrganizations;
			AssertEquals("Name", "EnableCreationOfTemporaryOrganizations", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_Glow_OpportunityManagement, item.Category);
			AssertEquals("Caption", "Enable Creation of Temporary Organizations", item.Caption);
			AssertEquals("Hint", "When enabled, GLOW users are allowed to create temporary organizations.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Option is set to hidden by default", RegistryOptions.IsHidden, item.Options);
			AssertEquals("Default Value", true, item.DefaultValue);

			using (ItemSet.EnableGlowSalesMarketingRegistrySettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ItemSet.RemoveItemFromCacheIfOlderThan("EnableCreationOfTemporaryOrganizations", TimeSpan.FromMilliseconds(-1));

				item = ItemSet.EnableCreationOfTemporaryOrganizations;
				Assert("Precondition: EnableGlowOpportunityRegistrySettings is set to true", ItemSet.EnableGlowSalesMarketingRegistrySettings.Value);
				AssertEquals("Option is set to default when EnableGlowOpportunityRegistrySettings is set to true", RegistryOptions.Default, item.Options);
			}

			var defaultValues = ItemSet.TempOrganisationRequiredFields.DefaultValue;

			AssertEquals("DefaultValue.Count", 14, defaultValues.Count);

			AssertEquals("DefaultValue[0].Code", "Name", defaultValues[0].Code);
			AssertEquals("DefaultValue[1].Code", "Address1", defaultValues[1].Code);
			AssertEquals("DefaultValue[2].Code", "Address2", defaultValues[2].Code);
			AssertEquals("DefaultValue[3].Code", "Country", defaultValues[3].Code);
			AssertEquals("DefaultValue[4].Code", "UNLOCO", defaultValues[4].Code);
			AssertEquals("DefaultValue[5].Code", "City", defaultValues[5].Code);
			AssertEquals("DefaultValue[6].Code", "Postcode", defaultValues[6].Code);
			AssertEquals("DefaultValue[7].Code", "State", defaultValues[7].Code);
			AssertEquals("DefaultValue[8].Code", "Branch", defaultValues[8].Code);
			AssertEquals("DefaultValue[9].Code", "Phone", defaultValues[9].Code);
			AssertEquals("DefaultValue[10].Code", "Mobile", defaultValues[10].Code);
			AssertEquals("DefaultValue[11].Code", "Email", defaultValues[11].Code);
			AssertEquals("DefaultValue[12].Code", "Fax", defaultValues[12].Code);
			AssertEquals("DefaultValue[13].Code", "Web", defaultValues[13].Code);

			AssertEquals("DefaultValue[0].Description", "Name", defaultValues[0].Description);
			AssertEquals("DefaultValue[1].Description", "Address 1", defaultValues[1].Description);
			AssertEquals("DefaultValue[2].Description", "Address 2", defaultValues[2].Description);
			AssertEquals("DefaultValue[3].Description", "Country/Region", defaultValues[3].Description);
			AssertEquals("DefaultValue[4].Description", "UNLOCO", defaultValues[4].Description);
			AssertEquals("DefaultValue[5].Description", "City", defaultValues[5].Description);
			AssertEquals("DefaultValue[6].Description", "Postcode", defaultValues[6].Description);
			AssertEquals("DefaultValue[7].Description", "State", defaultValues[7].Description);
			AssertEquals("DefaultValue[8].Description", "Branch", defaultValues[8].Description);
			AssertEquals("DefaultValue[9].Description", "Phone", defaultValues[9].Description);
			AssertEquals("DefaultValue[10].Description", "Mobile", defaultValues[10].Description);
			AssertEquals("DefaultValue[11].Description", "Email", defaultValues[11].Description);
			AssertEquals("DefaultValue[12].Description", "Fax", defaultValues[12].Description);
			AssertEquals("DefaultValue[13].Description", "Website URL", defaultValues[13].Description);

			AssertEquals("DefaultValue[0].Bool", true, defaultValues[0].Bool);
			AssertEquals("DefaultValue[1].Bool", false, defaultValues[1].Bool);
			AssertEquals("DefaultValue[2].Bool", false, defaultValues[2].Bool);
			AssertEquals("DefaultValue[3].Bool", false, defaultValues[3].Bool);
			AssertEquals("DefaultValue[4].Bool", false, defaultValues[4].Bool);
			AssertEquals("DefaultValue[5].Bool", false, defaultValues[5].Bool);
			AssertEquals("DefaultValue[6].Bool", false, defaultValues[6].Bool);
			AssertEquals("DefaultValue[7].Bool", false, defaultValues[7].Bool);
			AssertEquals("DefaultValue[8].Bool", false, defaultValues[8].Bool);
			AssertEquals("DefaultValue[9].Bool", false, defaultValues[9].Bool);
			AssertEquals("DefaultValue[10].Bool", false, defaultValues[10].Bool);
			AssertEquals("DefaultValue[11].Bool", false, defaultValues[11].Bool);
			AssertEquals("DefaultValue[12].Bool", false, defaultValues[12].Bool);
			AssertEquals("DefaultValue[13].Bool", false, defaultValues[13].Bool);

			AssertEquals("DefaultValue[0].IsMandatory", true, defaultValues[0].IsMandatory);
			AssertEquals("DefaultValue[1].IsMandatory", false, defaultValues[1].IsMandatory);
			AssertEquals("DefaultValue[2].IsMandatory", false, defaultValues[2].IsMandatory);
			AssertEquals("DefaultValue[3].IsMandatory", false, defaultValues[3].IsMandatory);
			AssertEquals("DefaultValue[4].IsMandatory", false, defaultValues[4].IsMandatory);
			AssertEquals("DefaultValue[5].IsMandatory", false, defaultValues[5].IsMandatory);
			AssertEquals("DefaultValue[6].IsMandatory", false, defaultValues[6].IsMandatory);
			AssertEquals("DefaultValue[7].IsMandatory", false, defaultValues[7].IsMandatory);
			AssertEquals("DefaultValue[8].IsMandatory", false, defaultValues[8].IsMandatory);
			AssertEquals("DefaultValue[9].IsMandatory", false, defaultValues[9].IsMandatory);
			AssertEquals("DefaultValue[10].IsMandatory", false, defaultValues[10].IsMandatory);
			AssertEquals("DefaultValue[11].IsMandatory", false, defaultValues[11].IsMandatory);
			AssertEquals("DefaultValue[12].IsMandatory", false, defaultValues[12].IsMandatory);
			AssertEquals("DefaultValue[13].IsMandatory", false, defaultValues[13].IsMandatory);
		}

		public void TestTempOrganisationRequiredFields()
		{
			var item = ItemSet.TempOrganisationRequiredFields;
			AssertEquals("Name", "TempOrganisationRequiredFields", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.SalesMarketing_Glow, item.Category);
			AssertEquals("Caption", "Temporary Organization Required Fields", item.Caption);
			AssertEquals("Hint", "Required fields for temporary organizations.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Option is set to hidden by default", RegistryOptions.IsHidden, item.Options);

			using (ItemSet.EnableGlowSalesMarketingRegistrySettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ItemSet.RemoveItemFromCacheIfOlderThan("TempOrganisationRequiredFields", TimeSpan.FromMilliseconds(-1));

				item = ItemSet.TempOrganisationRequiredFields;
				Assert("Precondition: EnableGlowOpportunityRegistrySettings is set to true", ItemSet.EnableGlowSalesMarketingRegistrySettings.Value);
				AssertEquals("Option is set to default when EnableGlowOpportunityRegistrySettings is set to true", RegistryOptions.Default, item.Options);
			}
		}

		#endregion

		#endregion

		void AssertCodeDescriptionBool(CodeDescriptionBool value, ZString code, ZString description, bool boolValue)
		{
			AssertEquals($"{code} Code", code, value.Code);
			AssertEquals($"{code} Description", description, value.Description);
			AssertEquals($"{code} Bool", boolValue, value.Bool);
		}

		#endregion

		#region Denied Party Screening

		public void TestDeniedPartyCategory()
		{
			AssertEquals(string.Format("{0}/Denied Party Screening", OrganisationsDataRegistry.Categories.Organizations), OrganisationsDataRegistry.Categories.Organizations_DeniedPartyScreening);
		}

		public void TestDeniedPartyRescreenErrorNotificationGroup()
		{
			TestGenericRegistryItem(ItemSet.DeniedPartyRescreenErrorNotificationGroup,
				"DeniedPartyRescreenErrorNotificationGroup",
				"Master Data/Organizations/Denied Party Screening",
				"Denied Party Error Notification Group",
				"The staff group that will receive notifications about execution or configuration errors with DPS.",
				RegistryStorageFlags.System,
				RegistryOptions.IsValueMandatory,
				Core.Constants.Groups.PostMastersGroupPK);
		}

		public void TestCountryDefaultLanguage()
		{
			TestGenericRegistryItem(ItemSet.CountryDefaultLanguage,
				"CountryDefaultLanguage",
				"Master Data/Organizations/Default Values",
				"Country/Region Default Language",
				"Specify the default language for countries/regions.",
				RegistryStorageFlags.System,
				RegistryOptions.Default);
		}

		public void TestTryGetDefaultLanguage()
		{
			var countryPk = Constants.CountryGuids.China;
			AssertEquals(false, OrganisationsDataRegistry.Instance.TryGetDefaultLanguage(countryPk, out _));

			var defaultLanguages = new CountryDefaultLanguageBusinessObjectCollection()
			{
				new CountryDefaultLanguageBusinessObject()
				{
					CountryPk = countryPk,
					DefaultLanguage = Constants.Languages.ChineseSimplified,
				}
			};

			using (OrganisationsDataRegistry.Instance.CountryDefaultLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultLanguages))
			{
				var result = OrganisationsDataRegistry.Instance.TryGetDefaultLanguage(countryPk, out var defaultLanguage);
				AssertEquals(true, result);
				AssertEquals(Constants.Languages.ChineseSimplified, defaultLanguage);
			}
		}

		public void TestDPSFreightMovementRestrictions()
		{
			ItemSet.DPSFreightMovementRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.Exp);
			AssertEquals("Value set should also be retrieved", DPSFreightMovementRestrictionsOptions.Codes.Exp, ItemSet.DPSFreightMovementRestrictions.Value);
		}

		public void TestDPSFreightMovementRestrictionListItems()
		{
			var expectedLookUpList = new CodeDescriptionPairList();
			expectedLookUpList.AddPair("ALL", "All Jobs");
			expectedLookUpList.AddPair("NO", "Do not apply any Movement Restrictions");
			expectedLookUpList.AddPair("EXP", "Export Jobs");
			expectedLookUpList.AddPair("INT", "Import and Export Jobs");

			TestRegistryItem(item: ItemSet.DPSFreightMovementRestrictions,
				expectedName: "DPSFreightMovementRestrictions",
				expectedCategory: OrganisationsDataRegistry.Categories.Organizations_DeniedPartyScreening,
				expectedCaption: "Enable DPS Freight Movement Restrictions",
				expectedHint: "Enable/disable the DPS-specific restrictions.",
				expectedStorage: RegistryStorageFlags.System | RegistryStorageFlags.Company,
				expectedOptions: RegistryOptions.Default,
				expectedLookUpList: expectedLookUpList,
				expectedDefaultValue: DPSFreightMovementRestrictionsOptions.Codes.No);
		}

		public void TestScreeningStatusNotClear()
		{
			var statusList = new ScreeningStatusesList();
			foreach (CodeDescriptionPair status in statusList)
			{
				switch (status.Code)
				{
					case ScreeningStatusesList.Codes.Clear:
					case ScreeningStatusesList.Codes.PermanentClear:
					case ScreeningStatusesList.Codes.JobCleared:
					case ScreeningStatusesList.Codes.Release:
					case ScreeningStatusesList.Codes.JobClearedExternal:
						AssertEquals(false, OrganisationsDataRegistry.ScreeningStatusNotClear(status.Code));
						break;
					default:
						AssertEquals(true, OrganisationsDataRegistry.ScreeningStatusNotClear(status.Code));
						break;
				}
			}
		}

		public void TestIsDPSFreightMovementRestrictedForInt()
		{
			var registryInstance = OrganisationsDataRegistry.Instance;
			var registryRestriction = registryInstance.DPSFreightMovementRestrictions;

			var registryValue = DPSFreightMovementRestrictionsOptions.Codes.Int;
			registryRestriction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			var screeningStatus = ScreeningStatusesList.Codes.Matched;

			CombineAssertions("Reg option(Int) + Screening Status(Matched) => IsDPSFreightMovementRestricted is", () =>
			{
				AssertEquals(false, registryInstance.IsDPSFreightMovementRestricted(screeningStatus, false, false));
				AssertEquals(true, registryInstance.IsDPSFreightMovementRestricted(screeningStatus, true, false));
				AssertEquals(true, registryInstance.IsDPSFreightMovementRestricted(screeningStatus, false, true));
			});

			screeningStatus = ScreeningStatusesList.Codes.Clear;
			CombineAssertions("Reg option(Int) + Screening Status(Clear) => IsDPSFreightMovementRestricted is", () =>
			{
				AssertEquals(false, registryInstance.IsDPSFreightMovementRestricted(screeningStatus, false, false));
				AssertEquals(false, registryInstance.IsDPSFreightMovementRestricted(screeningStatus, true, false));
				AssertEquals(false, registryInstance.IsDPSFreightMovementRestricted(screeningStatus, true, true));
			});
		}

		public void TestIsDPSFreightMovementRestrictedForAll_Exp_No()
		{
			var registryInstance = OrganisationsDataRegistry.Instance;
			var registryRestriction = registryInstance.DPSFreightMovementRestrictions;

			var registryValue = DPSFreightMovementRestrictionsOptions.Codes.All;
			registryRestriction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			var screeningStatus = ScreeningStatusesList.Codes.Matched;
			var isExport = false;
			var isImport = false;
			AssertEquals("Reg option(All) + Screening Status(Matched) => IsDPSFreightMovementRestricted is", true, registryInstance.IsDPSFreightMovementRestricted(screeningStatus, isExport, isImport));

			registryValue = DPSFreightMovementRestrictionsOptions.Codes.All;
			registryRestriction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			screeningStatus = ScreeningStatusesList.Codes.Clear;
			isExport = false;
			AssertEquals("Reg option(All) + Screening Status(Clear) => IsDPSFreightMovementRestricted is", false, registryInstance.IsDPSFreightMovementRestricted(screeningStatus, isExport, isImport));

			screeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			AssertEquals("Reg option(All) + Screening Status(PermanentClear) => IsDPSFreightMovementRestricted is", false, registryInstance.IsDPSFreightMovementRestricted(screeningStatus, isExport, isImport));

			screeningStatus = ScreeningStatusesList.Codes.Release;
			AssertEquals("Reg option(All) + Screening Status(Release) => IsDPSFreightMovementRestricted is", false, registryInstance.IsDPSFreightMovementRestricted(screeningStatus, isExport, isImport));

			screeningStatus = ScreeningStatusesList.Codes.Block;
			AssertEquals("Reg option(All) + Screening Status(Block) => IsDPSFreightMovementRestricted is", true, registryInstance.IsDPSFreightMovementRestricted(screeningStatus, isExport, isImport));

			registryValue = DPSFreightMovementRestrictionsOptions.Codes.Exp;
			registryRestriction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			screeningStatus = ScreeningStatusesList.Codes.Matched;
			isExport = true;
			AssertEquals("Reg option(Exp) + Screening Status(Matched) + Export => IsDPSFreightMovementRestricted is", true, registryInstance.IsDPSFreightMovementRestricted(screeningStatus, isExport, isImport));

			registryValue = DPSFreightMovementRestrictionsOptions.Codes.Exp;
			registryRestriction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			screeningStatus = ScreeningStatusesList.Codes.Matched;
			isExport = false;
			AssertEquals("Reg option(Exp) + Screening Status(Matched) + Not Export => IsDPSFreightMovementRestricted is", false, registryInstance.IsDPSFreightMovementRestricted(screeningStatus, isExport, isImport));

			registryValue = DPSFreightMovementRestrictionsOptions.Codes.No;
			registryRestriction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			screeningStatus = ScreeningStatusesList.Codes.Matched;
			isExport = false;
			AssertEquals("Reg option(No) => IsDPSFreightMovementRestricted is always", false, registryInstance.IsDPSFreightMovementRestricted(screeningStatus, isExport, isImport));
		}

		public void TestNameSeparators()
		{
			TestGenericRegistryItem(
				ItemSet.NameSeparators,
				"OrganisationsNameSeparators",
				OrganisationsDataRegistry.Categories.Organizations_DeniedPartyScreening,
				"Name Separators",
				"This Registry defines the separators that Denied Party Screening is able to recognize in order to screen parties on both sides of it as separate names.",
				RegistryStorageFlags.System,
				RegistryOptions.Default);

			AssertContainsExactElementsInAnyOrder(new string[] { "C/O", "T/A", "DBA", "O/A", "C/" }, ItemSet.NameSeparators.DefaultValue);
		}

		public void TestComplianceRiskFreightMovementRestrictions()
		{
			TestGenericRegistryItem(ItemSet.ComplianceRiskFreightMovementRestrictions,
				"ComplianceRiskFreightMovementRestrictions",
				"Compliance",
				"Compliance Risk Freight Movement Restriction",
				"Enable or disable Freight Movement Restrictions for Compliance Risk.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RawDataRegistry.Instance.EnableComplianceRisk.Value ? RegistryOptions.Default : RegistryOptions.IsHidden);

			ItemSet.ComplianceRiskFreightMovementRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.Exp);
			AssertEquals("Value set should also be retrieved", DPSFreightMovementRestrictionsOptions.Codes.Exp, ItemSet.ComplianceRiskFreightMovementRestrictions.Value);
		}

		public void TestComplianceRiskFreightMovementRestrictionsDefaultValueShouldBeDPSFreightMovementRestrictionsValue()
		{
			var enableComplianceRisk = RawDataRegistry.Instance.EnableComplianceRisk.Value;
			var dpsFreightMovement = ItemSet.DPSFreightMovementRestrictions.Value;
			ItemSet.DPSFreightMovementRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All);
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertEquals(DPSFreightMovementRestrictionsOptions.Codes.All, ItemSet.ComplianceRiskFreightMovementRestrictions.Value);

			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enableComplianceRisk);
			ItemSet.DPSFreightMovementRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dpsFreightMovement);
		}

		public void TestEnableCompliancePotentialRiskWarningMessage()
		{
			TestGenericRegistryItem(ItemSet.EnableComplianceWarningMessage,
				"EnableComplianceWarningMessage",
				"Compliance",
				"Enable Compliance Warning Message",
				"Enable this registry to include a warning message on Jobs where the Job Compliance status is not Clear.",
				RegistryStorageFlags.System,
				RawDataRegistry.Instance.EnableComplianceRisk.Value ? RegistryOptions.Default : RegistryOptions.IsHidden);
		}

		public void TestComplianceJobEndDateLimit()
		{
			TestRegistryItem(ItemSet.ComplianceJobEndDateLimit,
				"ComplianceJobEndDateLimit",
				RawDataRegistry.Categories.Compliance,
				"Job End Date",
				"This registry specifies the number of days after a job is complete, that the compliance risk statuses will be automatically resynchronized when the job is opened. Manual resynchronization will still be possible.",
				RegistryStorageFlags.System,
				RawDataRegistry.Instance.EnableComplianceRisk.Value ? RegistryOptions.Default : RegistryOptions.IsHidden, 7, 1, 14);
		}

		public void TestAllowComplianceCommodityRiskAssessmentVisible()
		{
			AssertAllowComplianceCommodityRiskAssessment(true);
		}

		public void TestAllowComplianceCommodityRiskAssessmentInvisible()
		{
			AssertAllowComplianceCommodityRiskAssessment(false);
		}

		void AssertAllowComplianceCommodityRiskAssessment(bool enableCommodityScreening)
		{
			var complianceWiseFeatureRule = new ComplianceRiskFeatureControlRule { Enabled = enableCommodityScreening };
			var featureDataMock = new Mock<IFeatureData>();
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out complianceWiseFeatureRule)).Returns(true);
			featureControlMock.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.ComplianceWiseCommodityScreening, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));
			using (ComplianceRiskFeatureControlHelper.GetIngoreComplianceWiseCommodityScreeningEnableForTest())
			using (ObjectFactory.Substitute(featureControlMock.Object))
			{
				TestGenericRegistryItem(ItemSet.AllowComplianceCommodityRiskAssessment,
					"AllowComplianceCommodityRiskAssessment",
					"Compliance",
					"Commodity Risk Assessment Decision",
					"When enabled, the commodity risk assessment is required on all jobs. When disabled, the commodity risk assessment is optional and unassessed commodities are non-blocking.",
					RegistryStorageFlags.System,
					enableCommodityScreening ? RegistryOptions.Default : RegistryOptions.IsHidden,
					false);
			}
		}

		public void TestComplianceWiseFeatureDevelopment()
		{
			TestRegistryItem(ItemSet.ComplianceWiseFeatureDevelopment,
				"ComplianceWiseFeatureDevelopment",
				"Compliance",
				"ComplianceWise Feature Development",
				"This registry item should only be updated by the Master Data Product team.",
				RegistryStorageFlags.System,
				RawDataRegistry.Instance.EnableComplianceRisk.Value ? RegistryOptions.IsOnlyForSupport : (RegistryOptions.IsOnlyForSupport | RegistryOptions.IsHidden));

			var collection = ItemSet.ComplianceWiseFeatureDevelopment.Value;
			CombineAssertions(() =>
			{
				AssertEquals(2, collection.Count);

				Assert(!collection.GetBoolFromCode(OrganisationsDataRegistry.ComplianceWiseFeatureCodes.GlobalCommercialInvoice));
				AssertEquals("Global Commercial Invoice", collection.GetDescriptionFromCode(OrganisationsDataRegistry.ComplianceWiseFeatureCodes.GlobalCommercialInvoice));

				Assert(!collection.GetBoolFromCode(OrganisationsDataRegistry.ComplianceWiseFeatureCodes.JobEntitiesCaching));
				AssertEquals("Job Entities Caching", collection.GetDescriptionFromCode(OrganisationsDataRegistry.ComplianceWiseFeatureCodes.JobEntitiesCaching));
			});
		}

		public void TestComplianceWiseFeatureDevelopmentEnabled()
		{
			var collection = new CodeDescriptionBoolCollection
			{
				{ OrganisationsDataRegistry.ComplianceWiseFeatureCodes.GlobalCommercialInvoice, (NoResString)"Global Commercial Invoice", false },
			};

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (ItemSet.ComplianceWiseFeatureDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				AssertEquals(false, ItemSet.ComplianceWiseFeatureDevelopmentEnabled(OrganisationsDataRegistry.ComplianceWiseFeatureCodes.GlobalCommercialInvoice));
			}

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				collection[0].Bool = false;
				using (ItemSet.ComplianceWiseFeatureDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
				{
					AssertEquals(false, ItemSet.ComplianceWiseFeatureDevelopmentEnabled(OrganisationsDataRegistry.ComplianceWiseFeatureCodes.GlobalCommercialInvoice));
				}

				collection[0].Bool = true;
				using (ItemSet.ComplianceWiseFeatureDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
				{
					AssertEquals(true, ItemSet.ComplianceWiseFeatureDevelopmentEnabled(OrganisationsDataRegistry.ComplianceWiseFeatureCodes.GlobalCommercialInvoice));
				}
			}
		}

		public void TestComplianceRiskAssessmentBatchSize()
		{
			TestRegistryItem(ItemSet.ComplianceRiskAssessmentBatchSize,
				"ComplianceRiskAssessmentBatchSize",
				"Compliance",
				"Compliance Risk Assessment (CRA) Batch Size",
				"This registry item configures the batch size for the Compliance Risk Assessment (CRA) Service Task.",
				RegistryStorageFlags.System,
				RawDataRegistry.Instance.EnableComplianceRisk.Value ? RegistryOptions.IsOnlyForSupport : (RegistryOptions.IsOnlyForSupport | RegistryOptions.IsHidden),
				10, 1, 1000);
		}

		public void TestDeniedPartyScreeningRequireReasonForClearing()
		{
			TestGenericRegistryItem(ItemSet.DeniedPartyScreeningRequireReasonForClearing,
				"DeniedPartyScreeningRequireReasonForClearing",
				"Master Data/Organizations/Denied Party Screening",
				"Require Reason For CLR",
				"If this registry is set to YES the user will be forced to write a comment before clearing a record with potential Medium or High Confidence Denied Party Candidates.",
				RegistryStorageFlags.System,
				RegistryOptions.Default);

			var wrapper = ItemSet.DeniedPartyScreeningRequireReasonForClearing.DefaultValue;
			AssertEquals(false, wrapper.RequireReasonForCLR);
			AssertEquals(0, wrapper.ItemCollection.Count);
		}

		public void TestDeniedPartyScreeningRequireReasonForJobClearing()
		{
			TestGenericRegistryItem(ItemSet.DeniedPartyScreeningRequireReasonForJobClearing,
				"DeniedPartyScreeningRequireReasonForJobClearing",
				"Master Data/Organizations/Denied Party Screening",
				"Require Reason For Job CLR",
				"If this registry is set to YES the user will be forced to write a comment before marking a record to job clear.",
				RegistryStorageFlags.System,
				RegistryOptions.Default);

			var wrapper = ItemSet.DeniedPartyScreeningRequireReasonForJobClearing.DefaultValue;
			AssertEquals(true, wrapper.RequireReasonForCLR);
			AssertEquals(1, wrapper.ItemCollection.Count);

			var defaultItem = wrapper.ItemCollection.Cast<RequireReasonForCLRItem>().FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertEquals("DEF", defaultItem.Code);
				AssertEquals("Default", defaultItem.Title);
				AssertEquals(true, defaultItem.IsMandatory);
				AssertEquals("", defaultItem.ClearingReason);
			});
		}

		public void TestComplianceRiskOverrideDecisionReason()
		{
			using (ItemSet.DeniedPartyScreeningRequireReasonForJobClearing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new RequireReasonForCLRWrapper() { RequireReasonForCLR = false }))
			{
				TestGenericRegistryItem(ItemSet.ComplianceRiskOverrideDecisionReason,
					"ComplianceRiskOverrideDecisionReason",
					"Compliance",
					"Override Decision Reason",
					"When set to Yes, users will be required to include a decision reason when Overriding a Job",
					RegistryStorageFlags.System,
					RawDataRegistry.Instance.EnableComplianceRisk.Value ? RegistryOptions.Default : RegistryOptions.IsHidden);

				var wrapper = ItemSet.ComplianceRiskOverrideDecisionReason.DefaultValue;
				AssertEquals(false, wrapper.RequireReasonForCLR);
				AssertEquals(1, wrapper.ItemCollection.Count);

				var defaultItem = wrapper.ItemCollection.Cast<RequireReasonForCLRItem>().Single();
				CombineAssertions(() =>
				{
					AssertEquals("DEF", defaultItem.Code);
					AssertEquals("Free Text", defaultItem.Title);
					AssertEquals(true, defaultItem.IsMandatory);
					AssertEquals(string.Empty, defaultItem.ClearingReason);
				});
			}
		}

		public void TestComplianceRiskOverrideDecisionReason_DefaultFromDeniedPartyScreeningRequireReasonForJobClearing()
		{
			var dpsValue = ItemSet.DeniedPartyScreeningRequireReasonForJobClearing.Value;
			var cpwDefaultValue = ItemSet.ComplianceRiskOverrideDecisionReason.DefaultValue;
			AssertEquals(dpsValue.RequireReasonForCLR, cpwDefaultValue.RequireReasonForCLR);
			AssertEquals(1, dpsValue.ItemCollection.Count);
			AssertEquals(1, cpwDefaultValue.ItemCollection.Count);

			var dpsDefaultItem = dpsValue.ItemCollection.Cast<RequireReasonForCLRItem>().Single();
			var defaultItem = cpwDefaultValue.ItemCollection.Cast<RequireReasonForCLRItem>().Single();
			CombineAssertions(() =>
			{
				AssertEquals(dpsDefaultItem.Code, defaultItem.Code);
				AssertEquals(dpsDefaultItem.Title, defaultItem.Title);
				AssertEquals(dpsDefaultItem.IsMandatory, defaultItem.IsMandatory);
				AssertEquals(dpsDefaultItem.ClearingReason, defaultItem.ClearingReason);
			});
		}

		public void TestEnableEDICodeMappingModule()
		{
			TestGenericRegistryItem(ItemSet.EnableEDICodeMappingModule,
				"EnableEDICodeMappingModule",
				"Master Data/Organizations/Code Mappings",
				"Enable EDI Code Mapping Module",
				"When turned on, can find Maintain -> EDI Messaging -> EDI Code Mapping module",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestDeniedpartyScreeningEnableJobClear()
		{
			TestGenericRegistryItem(ItemSet.DeniedpartyScreeningEnableJobClear,
				"DeniedpartyScreeningEnableJobClear",
				"Master Data/Organizations/Denied Party Screening",
				"Denied Party Screening Enable Job Clear",
				"When set to Yes, Job Clear Status become available.",
				RegistryStorageFlags.System,
				false);
		}

		public void TestDeniedPartyScreeningTimeoutForScreeningRequests()
		{
			TestRegistryItem(ItemSet.DeniedPartyScreeningTimeoutForScreeningRequests,
				"DeniedPartyScreeningTimeoutForScreeningRequests",
				"Master Data/Organizations/Denied Party Screening/Timeouts",
				"Screening Requests",
				"Sets the timeout (in seconds) for a request, to the Denied Party Screening web service, to screen an individual record.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				600,
				180,
				1200);
		}

		public void TestDeniedPartyScreeningTimeoutForRescreeningServiceTask()
		{
			TestRegistryItem(ItemSet.DeniedPartyScreeningTimeoutForRescreeningServiceTask,
				"DeniedPartyScreeningTimeoutForRescreeningServiceTask",
				"Master Data/Organizations/Denied Party Screening/Timeouts",
				"Re-screening Advice Manager",
				"Sets the timeout (in seconds) for the Re-screening Advice Manager Service Task to get a response from the Denied Party Screening web service.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				600,
				300,
				1800);
		}

		public void TestDeniedPartySilentScreeningServiceTaskBatchSize()
		{
			TestRegistryItem(ItemSet.DeniedPartySilentScreeningServiceTaskBatchSize,
				"DeniedPartySilentScreeningServiceTaskBatchSize",
				"Master Data/Organizations/Denied Party Screening",
				"Denied Party Silent Screening (DSS) Service Task Batch Size",
				"This registry item configures the batch size for the Denied Party Silent Screening (DSS) service task.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				10,
				2,
				50);
		}

		public void TestDeniedPartyRescreeningServiceTaskBatchSize()
		{
			TestRegistryItem(ItemSet.DeniedPartyRescreeningServiceTaskBatchSize,
				"DeniedPartyRescreeningServiceTaskBatchSize",
				"Master Data/Organizations/Denied Party Screening",
				"Denied Party Rescreening (DPR) Service Task Batch Size",
				"This registry item configures the batch size for the Denied Party Rescreening (DPR) service task.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				50,
				10,
				200);
		}

		public void DPSSendScreeningDecisionServiceTaskBatchSize()
		{
			TestRegistryItem(ItemSet.DeniedPartyRescreeningServiceTaskBatchSize,
				"DPSSendScreeningDecisionServiceTaskBatchSize",
				"Master Data/Organizations/Denied Party Screening",
				"Denied Party Send Screening Decision (DPM) Service Task Batch Size",
				"This registry item configures the batch size for the Denied Party Send Screening Decision (DPM) service task.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				50,
				10,
				200);
		}

		public void TestDeniedPartyScreeningTimeoutForSQLCommandQuery()
		{
			TestRegistryItem(ItemSet.DeniedPartyScreeningTimeoutForSQLCommandQuery,
				"DeniedPartyScreeningTimeoutForSQLCommandQuery",
				"Master Data/Organizations/Denied Party Screening/Timeouts",
				"SQL Command Query",
				"Sets the timeout (in seconds) for SQL command query execution.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				600,
				180,
				3600);
		}

		public void TestMatchingConfidenceThresholdsForVessels()
		{
			var expectedDescription = $@"This registry sets the minimum thresholds for a match to be considered a Medium or High Confidence match when returned from Denied Party Screening.
In order to ensure that the compliance aspects of Denied Party Screening are not compromised, there are some limitations to the configurations that can be made:
- Medium threshold must be between {DpsConfidenceThresholdsBusinessObject.Constants.MediumThresholdMinimum}% and {DpsConfidenceThresholdsBusinessObject.Constants.MediumThresholdMaximum}%,
- High threshold must be between {DpsConfidenceThresholdsBusinessObject.Constants.HighThresholdMinimum}% and {DpsConfidenceThresholdsBusinessObject.Constants.HighThresholdMaximum}%,
- There must be a difference of at least {DpsConfidenceThresholdsBusinessObject.Constants.ThresholdDifferenceMinimum}% between the Medium and High thresholds.";
			var item = ItemSet.MatchingConfidenceThresholdsForVessels;
			TestGenericRegistryItem(item,
				"MatchingConfidenceThresholdsForVessels",
				"Master Data/Organizations/Denied Party Screening/Comparison Settings",
				"Matching Confidence Thresholds For Vessels",
				expectedDescription,
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				new DpsConfidenceThresholdsBusinessObject()
			);
		}

		public void TestMatchingConfidenceThresholdsForPersons()
		{
			var expectedDescription = $@"This registry sets the minimum thresholds for a match to be considered a Medium or High Confidence match when returned from Denied Party Screening.
In order to ensure that the compliance aspects of Denied Party Screening are not compromised, there are some limitations to the configurations that can be made:
- Medium threshold must be between {DpsConfidenceThresholdsBusinessObject.Constants.MediumThresholdMinimum}% and {DpsConfidenceThresholdsBusinessObject.Constants.MediumThresholdMaximum}%,
- High threshold must be between {DpsConfidenceThresholdsBusinessObject.Constants.HighThresholdMinimum}% and {DpsConfidenceThresholdsBusinessObject.Constants.HighThresholdMaximum}%,
- There must be a difference of at least {DpsConfidenceThresholdsBusinessObject.Constants.ThresholdDifferenceMinimum}% between the Medium and High thresholds.";
			var item = ItemSet.MatchingConfidenceThresholdsForPersons;
			TestGenericRegistryItem(item,
				"MatchingConfidenceThresholdsForPersons",
				"Master Data/Organizations/Denied Party Screening/Comparison Settings",
				"Matching Confidence Thresholds For Persons",
				expectedDescription,
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				new DpsConfidenceThresholdsBusinessObject()
			);
		}

		public void TestDeniedPartyScreeningMatchingConfidenceThresholds()
		{
			var expectedDescription = $@"This registry sets the minimum thresholds for a match to be considered a Medium or High Confidence match when returned from Denied Party Screening.
In order to ensure that the compliance aspects of Denied Party Screening are not compromised, there are some limitations to the configurations that can be made:
- Medium threshold must be between {DpsConfidenceThresholdsBusinessObject.Constants.MediumThresholdMinimum}% and {DpsConfidenceThresholdsBusinessObject.Constants.MediumThresholdMaximum}%,
- High threshold must be between {DpsConfidenceThresholdsBusinessObject.Constants.HighThresholdMinimum}% and {DpsConfidenceThresholdsBusinessObject.Constants.HighThresholdMaximum}%,
- There must be a difference of at least {DpsConfidenceThresholdsBusinessObject.Constants.ThresholdDifferenceMinimum}% between the Medium and High thresholds.";
			var item = ItemSet.MatchingConfidenceThresholdsForOrganisations;
			TestGenericRegistryItem(item,
				"MatchingConfidenceThresholdsForOrganisations",
				"Master Data/Organizations/Denied Party Screening/Comparison Settings",
				"Matching Confidence Thresholds For Organizations",
				expectedDescription,
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				new DpsConfidenceThresholdsBusinessObject()
			);
		}

		public void TestGetDPSMatchingConfidenceThreshold()
		{
			using (ItemSet.MatchingConfidenceThresholdsForOrganisations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DpsConfidenceThresholdsBusinessObject(70, 80)))
			using (ItemSet.MatchingConfidenceThresholdsForVessels.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DpsConfidenceThresholdsBusinessObject(71, 81)))
			using (ItemSet.MatchingConfidenceThresholdsForPersons.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DpsConfidenceThresholdsBusinessObject(72, 82)))
			{
				var forOrg = ItemSet.GetDPSMatchingConfidenceThreshold(DeniedPartyScreening.Common.DeniedPartyConstants.ScreeningNameTypes.Organization);
				var forVessel = ItemSet.GetDPSMatchingConfidenceThreshold(DeniedPartyScreening.Common.DeniedPartyConstants.ScreeningNameTypes.Vessel);
				var forPerson = ItemSet.GetDPSMatchingConfidenceThreshold(DeniedPartyScreening.Common.DeniedPartyConstants.ScreeningNameTypes.Person);
				var forNaturalPerson = ItemSet.GetDPSMatchingConfidenceThreshold(DeniedPartyScreening.Common.DeniedPartyConstants.ScreeningNameTypes.NaturalPerson);
				var dummy = ItemSet.GetDPSMatchingConfidenceThreshold("DUMMY");

				CombineAssertions(() =>
				{
					AssertEquals(70, forOrg.MediumThreshold);
					AssertEquals(80, forOrg.HighThreshold);

					AssertEquals(71, forVessel.MediumThreshold);
					AssertEquals(81, forVessel.HighThreshold);

					AssertEquals(72, forPerson.MediumThreshold);
					AssertEquals(82, forPerson.HighThreshold);

					AssertEquals(72, forNaturalPerson.MediumThreshold);
					AssertEquals(82, forNaturalPerson.HighThreshold);

					AssertEquals(DeniedPartyScreening.Common.DeniedPartyConstants.MatchScores.Medium, dummy.MediumThreshold);
					AssertEquals(DeniedPartyScreening.Common.DeniedPartyConstants.MatchScores.High, dummy.HighThreshold);
				});
			}
		}

		public void TestDeniedPartyScreeningWebService()
		{
			TestGenericRegistryItem(
				ItemSet.DeniedPartyScreeningWebService,
				"DeniedPartyScreeningWebService",
				OrganisationsDataRegistry.Categories.Organizations_DeniedPartyScreening_WebService,
				"Web Service URLs",
				"Specify the URLs that Denied Party Screening will use when making requests. One URL must be marked for service task use.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport
			);

			CombineAssertions(() =>
			{
				var webService = ItemSet.DeniedPartyScreeningWebService.DefaultValue;

				AssertEquals(3, webService.Count);
				AssertEquals(true, webService.Cast<DpsWebServiceItem>().Any(x => x.Code == "SYD1" && x.WebServiceUrl == "https://dpsv4.wisegrid.net" && x.Role == RoleHelper.Code.Production));
				AssertEquals(true, webService.Cast<DpsWebServiceItem>().Any(x => x.Code == "ORD1" && x.WebServiceUrl == "https://dpsv4-usord.wisegrid.net" && x.Role == RoleHelper.Code.ProductionFailover));
				AssertEquals(true, webService.Cast<DpsWebServiceItem>().Any(x => x.Code == "STG1" && x.WebServiceUrl == "https://dpsv4-test.wisegrid.net" && x.Role == RoleHelper.Code.Staging));
			});
		}

		public void TestDeniedPartyScreeningMatchingConfiguration()
		{
			TestGenericRegistryItem(
				ItemSet.DeniedPartyScreeningMatchingConfiguration,
				"DeniedPartyScreeningMatchingConfiguration",
				OrganisationsDataRegistry.Categories.Organizations_DeniedPartyScreening,
				"Matching Configuration",
				"This setting controls the strictness of matching used by the Denied Party Screening Service. There are three settings available.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport
			);

			var defaultValue = ItemSet.DeniedPartyScreeningMatchingConfiguration.DefaultValue;

			AssertEquals("Strict", defaultValue.MatchingConfiguration);
		}

		public void TestAddressMatchingLevel()
		{
			TestGenericRegistryItem(
				ItemSet.AddressMatchingLevel,
				"AddressMatchingLevel",
				OrganisationsDataRegistry.Categories.Organizations_DeniedPartyScreening,
				"Address Matching Level",
				"This setting controls the strictness of address matching logic in Denied Party Screening service. \r\nThere are three levels available.",
				RegistryStorageFlags.System,
				RegistryOptions.IsHidden
			);

			var defaultValue = ItemSet.AddressMatchingLevel.DefaultValue;

			AssertEquals("Balanced", defaultValue.MatchingLevel);
		}

		public void TestDeniedPartyScreeningUserTerms()
		{
			var expectedResult = @"WiseTech Denied Party Screening Service
The WTG Denied Party Screening Service facilitates the screening of your record against government and authority sanctioned and denied party lists.
In addition to government and authority sanctioned lists, denied party screening also screens against research derived data provided to WTG by providers who research the entities and vessels associated with sanctioned persons. These providers are currently 1nteger, LLC (trading as Kharon), and Factiva Limited (trading as Dow Jones). This data is provided to facilitate better adherence to OFAC’s and the EU’s 50% rules.
End User Agreement
Before using the denied party screening service (“Service”), you must accept the following terms and conditions (“Terms”). These Terms are a legal contract between you and WTG so it is important that you review them carefully before using the Service. Your use of the Service indicates that you agree to follow and be bound by the Terms. If you do not agree to the Terms, you may not use the Service.
These Terms must be read in conjunction with the obligations imposed in the separate Maintenance and Licence Agreement you have separately entered into with WTG (“MLA”). In the event of any inconsistency between the Terms and the MLA, these Terms will prevail.
1.	Definitions
	In these Terms, capitalised terms have the meanings as set out below:
		“Applicable Data Protection Law” means all applicable laws and regulations governing the protection of individuals with regard to the processing of Personal Data (including without limitation, security requirements for and the free movement of such Personal Data), including, but not limited to: (a) the General Data Protection Regulation (EU) No. 2016/679 of the European Parliament and of the Council of 27 April 2016 on the protection of natural persons with regard to the processing of personal data and on the free movement of such data (“GDPR”) and any national law implementing or supplementing the GDPR; and (b) in the event that the UK withdraws from the European Union, any UK laws or regulations replacing, succeeding or re-enacting the GDPR;
		“Criminal Data” means Personal Data relating to criminal convictions and offences or related security measures, including relating to the alleged commission of offences by a data subject or proceedings for an offence committed or alleged to have been committed by the data subject or the disposal of such proceedings, including sentencing;
		“Dow Jones Content” means the search results identified under “Denied Party Name” and “Other Info” where the words “Dow Jones” are contained in the Source List Name;
		“EEA” means the European Economic Area, consisting of all Member States of the European Union, plus Norway, Iceland and Liechtenstein, and shall also include Switzerland and the United Kingdom after its withdrawal from the European Union;
		“Kharon Content” means the search results identified under “Denied Party Name” and “Other Info” where the word “Kharon” is contained in the Source List Name;
		“Personal Data” means any information relating to an identified or identifiable individual;
		“Permitted User” means an individual authorised to access and use the Service and who is either an individual performing the functions of an employee, independent contractor or consultant, in each case who is performing work for you;
		“Process,” “Processing,” or “Processed” means any operation or set of operations that is or may be performed upon personal data in relation to or as a result of the Terms, whether or not by automatic means, including, but not limited to, collection, recording, organization, storage, access, transmission, adaptation, alteration, retrieval, consultation, use, disclosure, dissemination or otherwise making available, alignment, combination, blocking, disposal, deleting, erasure, or destruction;
		“Screening Data” means any and all Personal Data and/or other proprietary data Processed or transferred by a Third Party Provider (including the Kharon Content and Dow Jones Content) for the purposes of identifying sanctioned parties and their associates, and may include Special Categories of Data and Criminal Data.
		“Special Categories of Data” means Personal Data revealing racial or ethnic origin, political opinions, religious or philosophical beliefs, trade-union membership, genetic data, biometric data for the purpose of uniquely identifying a natural person, and data concerning health, a natural person’s sex life or sexual orientation;
		“Third Party Provider” means Factiva Limited acting on behalf of Dow Jones & Company, Inc. and 1nteger, LLC;
		“UK” means England, Wales, Scotland and Northern Ireland; and
		“WTG” means WiseTech Global Limited or any of its affiliated companies.
2.	Licence
	2.1	WTG grants you a non-exclusive, non-transferable, non-sub licensable, and non-assignable licence to use the Screening Data subject to the Terms and the MLA.
	2.2	Your use of the Kharon Content is also subject to the Terms of Use and Privacy Policy found at https://kharon.com/ and you understand that 1nteger, LLC is a beneficiary of your agreement to these terms and conditions and shall be entitled to enforce these terms and conditions against you.
3.	Terms of use
	3.1	You and your Permitted Users must use the Screening Data in compliance with these Terms and the MLA.
	3.2	You may only use the Screening Data for the following purposes, and you must ensure that the Screening Data is not used for any other purpose:
		(a)	performing customer or counterparty due diligence and other screening and risk management activities carried out to comply with legal or regulatory obligations to which you are subject, in particular “know your customer and counterparty” requirements under anti-money laundering, anti-bribery, corruption and economic sanctions regulation which apply to any member of your company group;
		(b)	performing a statutory role as a Governmental organisation;
		(c)	performing law enforcement duties;
		(d)	any establishment, exercise or defense of legal claims relating to clauses 3.2(a) to (c) above,
each to the extent permitted under, and subject always to, Applicable Data Protection Law.
	3.3	Except to the extent permitted or required for your permitted use under clause 3.2, you and your Permitted Users must not:
		3.3.1	reproduce, sub-licence, disseminate, distribute, display, sell, publish, broadcast or otherwise transfer the Screening Data to any third party, nor make the Screening Data available for any such use;
		3.3.2	create or store in electronic form any library or archive of the Screening Data save that, and notwithstanding anything to the contrary, you shall be entitled to retain copies of the Screening Data necessary for archival, regulatory and/or compliance purposes; or
		3.3.3	display or use in any manner, any trademarks, logos or other intellectual property of a Third Party Provider.
	3.4	You acknowledge that each Third-Party Provider retains control and ownership of the form and content of the Screening Data provided by it. You and your Permitted Users will not at any time acquire any ownership rights in the Screening Data.
4.	Disclaimer
	4.1	You acknowledge that the Screening Data may be sourced from a Third Party Provider and is not verified nor endorsed by WTG. Neither WTG nor any Third Party Provider provides any guarantees about the accuracy, adequacy, completeness, timeliness or availability of any Screening Data, and none of them shall be responsible for any errors or omissions (negligent or otherwise), regardless of the cause, or for the results obtained from the use of the Screening Data. Except as specified in the Terms, all express or implied representations, warranties, conditions and undertakings in relation to the provision of the Screening Data are excluded.
	4.2	In no event shall WTG or any Third Party Provider be liable for any direct, indirect or consequential loss, damages, costs, expenses, legal fees, or losses (including lost income or lost profit or opportunity costs) in connection with any use of the Service.
5.	Fair Credit Reporting Act
	5.1	You acknowledge that none of the Third Party Providers is a “consumer reporting agency” and that the Screening Data does not constitute a “consumer report” or “investigative consumer report” as such terms are defined in Fair Credit Reporting Act, 15 U.S.C. §1681, et seq. (FCRA), or any applicable state or national fair credit reporting laws.
	5.2	You represent and warrant that you will not use the Screening Data in whole or part as a factor in establishing a consumer’s eligibility for credit or insurance to be used primarily for personal, family or household purposes, for employment purposes, or for any other purpose authorized under section 604 of the Fair Credit Reporting Act (15 U.S.C. § 1681b) or applicable state or national fair credit reporting laws.
	5.3	Notwithstanding anything to the contrary in the Terms, you agree to fully indemnify, defend and hold harmless each Third Party Provider for any loss or damage suffered arising out of any breach by you of the representation and warranty given by you in clause 5.2.
6	Usage Information
	6.1	To enable WTG to measure usage of the Screening Data, WTG may provide your organisation’s name and the number of queries you have screened against the Screening Data to the Third Party Providers. This information will only be used to enable the Third Party Provider to determine amounts payable by WTG to the Third Party Provider. Under no circumstances will the Third Party Provider disclose such information to any third party, other than to members of its group companies or advisors on a need-to-know basis or use them for any other purpose whatsoever. By accepting these Terms, you consent to the collection and processing of your usage data for the purposes set out in this clause.
7	Data Privacy Obligations
	7.1	The Service involves the Processing of Personal Data, which may include Special Categories of Data and Criminal Data. You acknowledge that under certain Applicable Data Protection Laws, and in particular those of the EEA, the Processing of Special Categories of Data and Criminal Data is strictly regulated and is only permitted on specific legal bases.
	7.2	You acknowledge that, depending on the country or countries in which you are established and are using the Service, the laws of such country or countries may apply. You further acknowledge that in some jurisdictions (in particular some EEA countries), the legal bases for Processing Special Categories of Data and Criminal Data may be limited to performing due diligence and other screening activities in order to comply with your legal or regulatory obligations in the relevant country only. In those cases, you must ensure that the Processing of the Screening Data is limited to those purposes as authorized under relevant applicable law only (including Applicable Data Protection Law).
	7.3	Nothing in the Terms are to be taken to limit your obligation to comply with Applicable Data Protection Law.
	7.4	You must provide timely notice to WTG if you (a) receive an inquiry, a subpoena or a request for inspection or audit from a competent public authority relating to your use of the Screening Data; or (b) intend to disclose any part of the Screening Data to any competent public authority, to the extent legally possible.
	7.5	You will indemnify, defend and hold harmless WTG for any loss or damage suffered by it arising out of any breach by you of these Terms, and in particular this clause 7.
8	Suspension and Termination
	8.1	If at any time, WTG knows or reasonably suspects that you have breached any of the Terms, or becomes aware of any circumstance or change in Applicable Data Protection Law that is likely to have a substantial adverse impact on your ability to comply with the Terms,  WTG may suspend your access to the Screening Data, until such time that the non-compliance is remedied.
9	General
	9.1	These Terms are governed by the laws of New South Wales, Australia.
	9.2	We may revise these Terms at any time. Any changes to these Terms will be effective as of the date they are posted on the Service. You are expected to check this page from time to time to take notice of any changes we have made, as they are binding on you. These Terms were last updated on 18 February 2025.
";

			TestGenericRegistryItem(ItemSet.DeniedPartyScreeningUserTerms,
				"DeniedPartyScreeningUserTerms",
				"Master Data/Organizations/Denied Party Screening",
				"User Terms",
				"WiseTech Denied Party Screening Service User Terms.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
				expectedResult);
		}

		public void TestJobUpdatePeriod()
		{
			TestRegistryItem(ItemSet.JobUpdatePeriod,
				"JobUpdatePeriod",
				"Master Data/Organizations/Denied Party Screening",
				"Job Update Period",
				"Specify the job update period (months).",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				3,
				1,
				24);
		}

		public void TestJobUpdatePeriodExceptions()
		{
			var registryItem = ItemSet.JobUpdatePeriod;

			AssertNoExceptionThrown(() => registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1));
			AssertNoExceptionThrown(() => registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 24));
			AssertNoExceptionThrown(() => registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5));

			AssertExceptionThrown<RegistryValidationException>("The minimum fallback period is 1 month.", () => registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0));
			AssertExceptionThrown<RegistryValidationException>("The maximum fallback period is 24 months.", () => registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 25));
			AssertExceptionThrown<RegistryValidationException>("The minimum fallback period is 1 month.", () => registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, -5));
		}

		public void TestMDMSupportCertificateForDPS()
		{
			var registryItem = ItemSet.MDMSupportCertificateForDPS;

			AssertEquals("Name", "MDMSupportCertificateForDPS", registryItem.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.Organizations_DeniedPartyScreening, registryItem.Category);
			AssertEquals("Caption", "MDM Support Certificate For DPS", registryItem.Caption);
			AssertEquals("Hint", "With the System To System Trust Authentication option turned on for Denied Party Screen Web Service uri, if a MDM Support Certificate exists, it takes precedence.", registryItem.Hint);
			AssertEquals("Flgs", RegistryStorageFlags.System, registryItem.Storage);
			AssertEquals("Opts", RegistryOptions.IsOnlyForSupport, registryItem.Options);

			var trustedInfo = new SystemToSystemTrustInfo();
			trustedInfo.ClientId = "C3843230-0C84-4228-962D-349D8D8874F0";
			trustedInfo.TenantId = "985F5153-C738-4C55-9469-CF0C45542C53";
			trustedInfo.OperationId = "10992770-FC66-4B1E-BB21-5D435363430A";
#pragma warning disable CS0618 // To be replaced with S2ST library once WI00771920 is implemented
			trustedInfo.PrivateKey = "DUMMY";
			trustedInfo.Certificate = ZBlob.FromUTF8("DUMMY");

			using (ItemSet.MDMSupportCertificateForDPS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, trustedInfo))
			{
				var registryValue = registryItem.Value;

				AssertEquals(trustedInfo.ClientId, registryValue.ClientId);
				AssertEquals(trustedInfo.TenantId, registryValue.TenantId);
				AssertEquals(trustedInfo.OperationId, registryValue.OperationId);
				AssertEquals(trustedInfo.PrivateKey, registryValue.PrivateKey);
				AssertEquals(trustedInfo.Certificate, registryValue.Certificate);
			}
#pragma warning restore CS0618 // To be replaced with S2ST library once WI00771920 is implemented
		}

		public void TestEnableExternalOverrideJobScreeningStatuses()
		{
			TestRegistryItem(ItemSet.EnableExternalOverrideJobScreeningStatuses,
				"DeniedPartyScreeningEnableExternalOverrideJobScreeningStatuses",
				"Master Data/Organizations/Denied Party Screening",
				"Enable External Screening Status Override",
				"When enabled, a Standalone Custom Declaration job's screening status can be set to Job Cleared Externally (JCE) or Job Blocked Externally (JBE) through an XML Universal Event (XUE).",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#endregion

		#region ComplianceWiseCustoms

		public void TestCustomsShowImportAlertsOnExportDeclarationsVisible()
		{
			AssertCustomsShowImportAlertsOnExportDeclarations(true);
		}

		public void TestCustomsShowImportAlertsOnExportDeclarationsNotVisible()
		{
			AssertCustomsShowImportAlertsOnExportDeclarations(false);
		}

		void AssertCustomsShowImportAlertsOnExportDeclarations(bool enable)
		{
			var complianceWiseFeatureRule = new ComplianceRiskFeatureControlRule { Enabled = true };
			var featureDataMock = new Mock<IFeatureData>();
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out complianceWiseFeatureRule)).Returns(enable);
			featureControlMock.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.ComplianceWiseCustomsDeclarationModule, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

			using (ObjectFactory.Substitute(featureControlMock.Object))
			{
				CombineAssertions(() =>
				{
					AssertEquals("Name", "CustomsShowImportAlertsOnExportDeclarations", ItemSet.CustomsShowImportAlertsOnExportDeclarations.Name);
					AssertEquals("Categories.Length", 1, ItemSet.CustomsShowImportAlertsOnExportDeclarations.Categories.Length);
					AssertEquals("Category", "Compliance/Customs", ItemSet.CustomsShowImportAlertsOnExportDeclarations.Category);
					AssertEquals("Caption", "Show Import Alerts on Export Declarations", ItemSet.CustomsShowImportAlertsOnExportDeclarations.Caption);
					AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.CustomsShowImportAlertsOnExportDeclarations.Storage);
					AssertEquals("Hint", @"When enabled, import alerts will be visible on standalone customs export declarations jobs.
When disabled, import alerts will be not visible on standalone customs export declarations jobs.", ItemSet.CustomsShowImportAlertsOnExportDeclarations.Hint);
				});
				AssertEquals("Registry options", enable ? RegistryOptions.Default : RegistryOptions.IsHidden, ItemSet.CustomsShowImportAlertsOnExportDeclarations.Options);
				AssertEquals("Registry visible", enable, ItemSet.CustomsShowImportAlertsOnExportDeclarations.IsVisible(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			}
		}

		#endregion

		#region Address Validation

		public void TestDisabledAddressValidationCountries()
		{
			var item = ItemSet.DisabledAddressValidationCountries;

			CombineAssertions(() =>
			{
				TestGenericRegistryItem(ItemSet.DisabledAddressValidationCountries,
					"DisabledAddressValidationCountries",
					RawDataRegistry.Categories.Organizations_AddressValidationService,
					"Disabled Countries/Regions",
					"Disable one or more countries/regions from address validation process in the specific section.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport);
				AssertEquals(typeof(AddressValidationDisabledCountryItemCollection), item.DefaultValue.GetType());
				AssertEquals("DefaultValue", 0, item.DefaultValue.Count);
			});
		}

		public void TestShouldUseAddressValidation()
		{
			var country1PK = new Guid("8BE306AF-7C47-4AEC-A0DC-CB76D4F80615");
			var country2PK = new Guid("6C5D9394-CCB8-421D-9FDA-56E2D087A374");
			var rawRegistryValue = Env.Instance.Registry.EnableAddressValidationWebService;

			try
			{
				Env.Instance.Registry.EnableAddressValidationWebService = true;
				OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GetCollection());

				CombineAssertions(() =>
				{
					AssertEquals(true, OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(country1PK, AddressValidationSection.AdminPanel));
					AssertEquals(true, OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(country2PK, AddressValidationSection.AdminPanel));
					AssertEquals(false, OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(Guid.Empty, AddressValidationSection.AdminPanel));
				});

				OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GetCollection(country1PK, new[] { AddressValidationSection.AdminPanel, AddressValidationSection.OrganizationAddress }));

				CombineAssertions(() =>
				{
					AssertEquals(false, OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(country1PK, AddressValidationSection.AdminPanel));
					AssertEquals(false, OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(country1PK, AddressValidationSection.OrganizationAddress));
					AssertEquals(true, OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(country1PK, AddressValidationSection.OverrideAddress));
					AssertEquals(true, OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(country2PK, AddressValidationSection.AdminPanel));
					AssertEquals(false, OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(Guid.Empty, AddressValidationSection.AdminPanel));
				});

				OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GetCollection(country1PK, new[] { AddressValidationSection.OverrideAddress }));

				CombineAssertions(() =>
				{
					AssertEquals(true, OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(country1PK, AddressValidationSection.AdminPanel));
					AssertEquals(true, OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(country1PK, AddressValidationSection.OrganizationAddress));
					AssertEquals(false, OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(country1PK, AddressValidationSection.OverrideAddress));
				});

				foreach (var section in new[] { AddressValidationSection.Applicant, AddressValidationSection.Branch, AddressValidationSection.Company, AddressValidationSection.Staff, AddressValidationSection.Person, AddressValidationSection.SalesInquiry, AddressValidationSection.HVLVConsignment, AddressValidationSection.SupplierBookingLine })
				{
					OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GetCollection(country1PK, new[] { section }));
					AssertEquals(false, OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(country1PK, section));
					AssertEquals(true, OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(country1PK, AddressValidationSection.OverrideAddress));
				}

				Env.Instance.Registry.EnableAddressValidationWebService = false;
				OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GetCollection());

				CombineAssertions(() =>
				{
					AssertEquals(false, OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(country1PK, AddressValidationSection.AdminPanel));
					AssertEquals(false, OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(country2PK, AddressValidationSection.OrganizationAddress));
					AssertEquals(false, OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(Guid.Empty, AddressValidationSection.OverrideAddress));
				});
			}
			finally
			{
				Env.Instance.Registry.EnableAddressValidationWebService = rawRegistryValue;
			}

			AddressValidationDisabledCountryItemCollection GetCollection(Guid? countryPK = null, AddressValidationSection[] sections = null)
			{
				var collection = new AddressValidationDisabledCountryItemCollection();
				if (countryPK != null && sections != null)
				{
					var item = collection.AddNew();
					item.CountryPK = countryPK.Value;
					foreach (var section in sections)
					{
						switch (section)
						{
							case AddressValidationSection.AdminPanel:
								item.DisabledForAdminPanel = true;
								break;
							case AddressValidationSection.OrganizationAddress:
								item.DisabledForOrgAddress = true;
								break;
							case AddressValidationSection.OverrideAddress:
								item.DisabledForOverrideAddress = true;
								break;
							case AddressValidationSection.Branch:
								item.DisabledForBranch = true;
								break;
							case AddressValidationSection.Applicant:
								item.DisabledForApplicant = true;
								break;
							case AddressValidationSection.Company:
								item.DisabledForCompany = true;
								break;
							case AddressValidationSection.SalesInquiry:
								item.DisabledForSalesInquiry = true;
								break;
							case AddressValidationSection.Person:
								item.DisabledForPerson = true;
								break;
							case AddressValidationSection.Staff:
								item.DisabledForStaff = true;
								break;
							case AddressValidationSection.HVLVConsignment:
								item.DisabledForHVLVConsignment = true;
								break;
							case AddressValidationSection.SupplierBookingLine:
								item.DisabledForSupplierBookingLine = true;
								break;
						}
					}
				}

				return collection;
			}
		}

		public void TestShouldUseAddressValidation_ReturnFalseWhenCurrentCompanyIsNull()
		{
			var countryPK = Guid.NewGuid();

			using (Env.Instance.Registry.RawRegistry.EnableAddressValidationWebService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection()))
			{
				AssertEquals(true, OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(countryPK, AddressValidationSection.AdminPanel));

				using (EnvProxy.Instance.SetTemporaryUserContext(null))
				{
					AssertEquals(false, OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(countryPK, AddressValidationSection.AdminPanel));
				}
			}
		}

		public void TestEnableErrorSuppression()
		{
			var item = ItemSet.EnableErrorSuppression;

			AssertEquals("Name", "EnableErrorSuppression", item.Name);
			AssertEquals("Category", RawDataRegistry.Categories.Organizations_AddressValidationService, item.Category);
			AssertEquals("Caption", "Enable Error Suppression", item.Caption);
			AssertEquals("Hint", "Suppress address validation error per individual address.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, item.Options);
			AssertEquals("Default Value", false, item.DefaultValue);
		}

		public void TestPreventUpdateWhenGettingPointExact()
		{
			var item = ItemSet.PreventUpdateWhenGettingPointExact;

			AssertEquals("Name", "PreventUpdateWhenGettingPointExact", item.Name);
			AssertEquals("Category", RawDataRegistry.Categories.Organizations_AddressValidationService, item.Category);
			AssertEquals("Caption", "Prevent Automatic Standardization", item.Caption);
			AssertEquals("Hint", "Prevent automatic standardization on validated address.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.Default, item.Options);
			AssertEquals("Default Value", false, item.DefaultValue);
		}

		public void TestRequireAllUNLOCOConditionsToBeMet()
		{
			TestGenericRegistryItem(ItemSet.RequireAllUNLOCOConditionsToBeMet,
				"RequireAllUNLOCOConditionsToBeMet",
				"Master Data/Organizations/Default Values/UNLOCO Defaulting Rules",
				"AND or OR Logic", @"When set to YES this program will look for UNLOCOs with all matching Identifiers specified in the UNLOCO Defaulting Rules registry item. NO will search for UNLOCOs with any Identifiers.
IE:
YES = AND = all Identifiers.
NO = OR = any Identifiers.
This registry item works in conjunction with Closest Port rule and UNLOCO Defaulting Rules which means that it only defaults after an address has been validated (green envelope) and has Geo-Location Coordinates.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				true);
		}

		public void TestUNLOCODefaultingRule()
		{
			var item = ItemSet.UNLOCODefaultingRules;

			AssertEquals("Name", "UNLOCODefaultingRules", item.Name);
			AssertEquals("Category", OrganisationsDataRegistry.Categories.Organizations_DefaultValues_UNLOCODefaultingRules, item.Category);
			AssertEquals("Caption", "UNLOCO Defaulting Rules", item.Caption);
			AssertEquals("Hint", @"Please turn off 'Ignore UNLOCO Defaulting Rules' registry item to make this registry item take effect. This list of UNLOCO Identifiers is used for selecting the most appropriate UNLOCO to be defaulted into a Validated Address when multiple UNLOCO possibilities are found.
This registry item works in conjunction with Closest Port rule and And or OR logic which means that it only defaults after an address has been validated (green envelope) and has Geo-Location Coordinates.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Bool Caption", "Applied", ((CodeDescriptionBoolRegistryEditorInfo)item.EditorInfo).BoolColumnCaption);
			AssertEquals("Default Value", 11, item.DefaultValue.Count);
		}

		public void TestIgnoreUNLOCODefaultingRules()
		{
			TestGenericRegistryItem(ItemSet.IgnoreUNLOCODefaultingRules,
				"IgnoreUNLOCODefaultingRules",
				"Master Data/Organizations/Default Values/UNLOCO Defaulting Rules",
				"Ignore UNLOCO Defaulting Rules", @"When set to YES. 'UNLOCO Defaulting Rules' registry item will NOT be applied when retrieving closest port and NOT displayed in default UNLOCO filter.
When set to NO. 'UNLOCO Defaulting Rules' registry item will be applied when retrieving closest port and be displayed in default UNLOCO filter.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				false);
		}

		public void TestUnlocoDefaultingRule_DefaultsAreCorrect()
		{
			var defaultValues = ItemSet.UNLOCODefaultingRules.DefaultValue;

			AssertEquals("DefaultValue.Count", 11, defaultValues.Cast<CodeDescriptionBoolDisallowNewWithDefaultDisabled>().Count());

			CombineAssertions(() =>
			{
				AssertEquals("DefaultValue.ContainsCode(\"RL_HasAirport\")", true, defaultValues.ContainsCode("RL_HasAirport"));
				AssertEquals("DefaultValue.ContainsCode(\"RL_HasSeaport\")", true, defaultValues.ContainsCode("RL_HasSeaport"));
				AssertEquals("DefaultValue.ContainsCode(\"RL_HasRail\")", true, defaultValues.ContainsCode("RL_HasRail"));
				AssertEquals("DefaultValue.ContainsCode(\"RL_HasRoad\")", true, defaultValues.ContainsCode("RL_HasRoad"));
				AssertEquals("DefaultValue.ContainsCode(\"RL_HasPost\")", true, defaultValues.ContainsCode("RL_HasPost"));
				AssertEquals("DefaultValue.ContainsCode(\"RL_HasCustomsLodge\")", true, defaultValues.ContainsCode("RL_HasCustomsLodge"));
				AssertEquals("DefaultValue.ContainsCode(\"RL_HasUnload\")", true, defaultValues.ContainsCode("RL_HasUnload"));
				AssertEquals("DefaultValue.ContainsCode(\"RL_HasStore\")", true, defaultValues.ContainsCode("RL_HasStore"));
				AssertEquals("DefaultValue.ContainsCode(\"RL_HasTerminal\")", true, defaultValues.ContainsCode("RL_HasTerminal"));
				AssertEquals("DefaultValue.ContainsCode(\"RL_HasDischarge\")", true, defaultValues.ContainsCode("RL_HasDischarge"));
				AssertEquals("DefaultValue.ContainsCode(\"RL_HasOutport\")", true, defaultValues.ContainsCode("RL_HasOutport"));
			});

			AssertEquals("DefaultEnabled.Count", 2, defaultValues.Count(x => ((CodeDescriptionBoolDisallowNewWithDefaultDisabled)x).Bool));

			CombineAssertions(() =>
			{
				AssertNotNull("RL_HasAirport is Enabled by default", defaultValues.Cast<CodeDescriptionBoolDisallowNewWithDefaultDisabled>().Single(x => x.Code == "RL_HasAirport"));
				AssertNotNull("RL_HasSeaport is Enabled by default", defaultValues.Cast<CodeDescriptionBoolDisallowNewWithDefaultDisabled>().Single(x => x.Code == "RL_HasSeaport"));
			});
		}

		public void TestEnablePredefinedUNLOCOFilterInOrganisations()
		{
			TestRegistryItem(ItemSet.EnablePredefinedUNLOCOFilterInOrganizations,
				"EnablePredefinedUNLOCOFilterInOrganizations",
				OrganisationsDataRegistry.Categories.Organizations_DefaultValues_UNLOCODefaultingRules,
				"Enable Predefined UNLOCO Filter In Organizations",
						@"When enabled, a predefined set of filters is added to the UNLOCO Search button on the Organization > Details Tab.
This predefined set of filters are defined by the following Registry Settings:
-AND or OR Logic
-UNLOCO Defaulting Rules",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				true);
		}

		#endregion

		#region Added Info Panel

		public void TestEnableAddedInfoPanel()
		{
			TestGenericRegistryItem(ItemSet.EnableAddedInfoPanel,
				"EnableAddedInfoPanel",
				OrganisationsDataRegistry.Categories.Organizations_AddedInfoPanel,
				(NoResString)"Enable Added Info Panel",
				(NoResString)"Set True to Show Added Info Panel on Organization Form.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#endregion

		#region Bulk Branch Status Update On Company Deactivation And Activation

		public void TestBulkBranchStatusUpdateOnCompanyDeactivationAndActivation()
		{
			CombineAssertions(() =>
			{
				TestGenericRegistryItem(ItemSet.BulkBranchStatusUpdateOnCompanyDeactivationAndActivation,
					"BulkBranchStatusUpdateOnCompanyDeactivationAndActivation",
					OrganisationsDataRegistry.Categories.Organizations,
					"Bulk Branch Status Update On Company Deactivation And Activation",
					@"This registry enables users to bulk activate branches when a company is activated. Also, this will automatically deactivate all linked branches if the company is deactivated.
Yes - the automatic deactivation of all branches when a company is deactivated is enabled.
Yes - bulk activation of branches when a company is activated is enabled.",
					RegistryStorageFlags.System,
					false);
			});
		}

		#endregion

		#region Enable Controlling Contact Filters

		public void TestEnableControllingContactFilters()
		{
			TestRegistryItem(ItemSet.EnableControllingContactFilters,
				"EnableControllingContactFilters",
				OrganisationsDataRegistry.Categories.Organizations,
				"Enable Controlling Contact Filters", "When turned on, the Contact filter will be visible on the Organization > Contact page and its criteria will be enabled.",
				RegistryStorageFlags.System,
				false);
		}

		#endregion

		protected override IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				yield return nameof(ItemSet.EnableComplianceWarningMessage);
				yield return nameof(ItemSet.ComplianceRiskOverrideDecisionReason);
				yield return nameof(ItemSet.ComplianceRiskFreightMovementRestrictions);
				yield return nameof(ItemSet.CustomsShowImportAlertsOnExportDeclarations);
				yield return nameof(ItemSet.CompetitorType);
				yield return nameof(ItemSet.EnableAutoLinkingOpportunityAndQuoteScopes);
				yield return nameof(ItemSet.GlowCommunicationFieldsMandatory);
				yield return nameof(ItemSet.GlowCommunicationMethod);
				yield return nameof(ItemSet.GlowCommunicationPurpose);
				yield return nameof(ItemSet.GlowOpportunityContactRoles);
				yield return nameof(ItemSet.GlowOpportunityFieldsMandatory);
				yield return nameof(ItemSet.GlowOpportunityLeadSource);
				yield return nameof(ItemSet.GlowOpportunityProductType);
				yield return nameof(ItemSet.GlowOpportunitySalesTypes);
				yield return nameof(ItemSet.GlowOpportunityStages);
				yield return nameof(ItemSet.GlowOpportunityStatuses);
				yield return nameof(ItemSet.OpportunityScopeForwardingParameters);
				yield return nameof(ItemSet.OpportunityScopeForwardingPrioritySequence);
				yield return nameof(ItemSet.TempOrganisationRequiredFields);
				yield return nameof(ItemSet.AddressMatchingLevel);

				foreach (var registryItem in base.ConditionallyVisibleRegistryItems)
				{
					yield return registryItem;
				}
			}
		}
	}
}
