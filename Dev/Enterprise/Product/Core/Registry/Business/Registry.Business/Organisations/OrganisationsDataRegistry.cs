using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.IO;
using CargoWise.Organizations.CodeGeneration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.ComplianceWise;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	public sealed class OrganisationsDataRegistry : RegistryItemSet, IOrganisationsDataRegistry
	{
		OrganisationsDataRegistry()
		{
		}

		public override bool IsForProductivityWise => true;

		#region Organisations Form Tabs Status

		static MultilingualString OrganisationFormTabsStatusCategory
		{
			get { return ResString.GetMultilingualString("548707e1-feb3-4346-bc03-bbd726e593f1", "{0}/Tabs Status", Categories.Organizations); }
		}

		public BooleanRegistryItem GetShowEnabledOrganisationTabs(Guid userPK)
		{
			return new BooleanRegistryItem("GetShowEnabledOrganisationTabs" + userPK.ToString("N"), OrganisationFormTabsStatusCategory, ResString.GetMultilingualString("00be25e9-cf9f-48aa-b463-a4ac6c971bec", "Tabs Status of Organization Form"), ResString.GetMultilingualString("62a97f3a-4a47-429f-90f7-7bcde3298259", "Depends of Organization Types shows Tabs of Organization Form"), RegistryStorageFlags.Company, RegistryOptions.IsHidden, false);
		}

		#endregion

		#region Instance

		public static OrganisationsDataRegistry Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new OrganisationsDataRegistry();
				}
				return fInstance;
			}
		}

		[ThreadStatic]
		static OrganisationsDataRegistry fInstance;

		#endregion

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Organizations_CodeMappings { get { return CombineCategories(Organizations, ResString.GetMultilingualString("63a61f9d-ad7a-4577-ad4c-429c00e1efda", "Code Mappings")); } }
			public static MultilingualString Organizations_DefaultValues { get { return CombineCategories(Organizations, ResString.GetMultilingualString("f2859a78-fe18-49b6-840a-3ae11005675a", "Default Values")); } }
			public static MultilingualString Organizations_DefaultValues_INCOTerms { get { return CombineCategories(Organizations_DefaultValues, ResString.GetMultilingualString("b63d66c4-a244-848d-41c9-83ff95b73ecd", "Incoterms")); } }
			public static MultilingualString Organizations_DefaultValues_RequiredPortTransportEquipment { get { return CombineCategories(Organizations_DefaultValues, ResString.GetMultilingualString("3928625d-d71e-4a79-958f-1e69b3a21d57", "Required Port Transport Equipment")); } }
			public static MultilingualString Organizations_DefaultValues_UNLOCODefaultingRules { get { return CombineCategories(Organizations_DefaultValues, ResString.GetMultilingualString("8DCCBBD9-F0D8-4B21-8BB7-AD0F286C1450", "UNLOCO Defaulting Rules")); } }
			public static MultilingualString Organizations_DeniedPartyScreening { get { return CombineCategories(Organizations, ResString.GetMultilingualString("0ccdf0b5-a4ef-4418-bb42-c55332568fba", "Denied Party Screening")); } }
			public static MultilingualString Organizations_DeniedPartyScreening_Timeouts { get { return CombineCategories(Organizations_DeniedPartyScreening, ResString.GetMultilingualString("58ec97bc-a392-4864-8557-7a1aaa429da6", "Timeouts")); } }
			public static MultilingualString Organizations_DeniedPartyScreening_ComparisonSettings { get { return CombineCategories(Organizations_DeniedPartyScreening, ResString.GetMultilingualString("a56e6a70-a893-4922-b8b2-aa2524f624a4", "Comparison Settings")); } }
			public static MultilingualString Organizations_DeniedPartyScreening_WebService { get { return CombineCategories(Organizations_DeniedPartyScreening, ResString.GetMultilingualString("AFF738A1-6B3C-4C1F-963E-ED69E1F4BB4A", "Web Service")); } }
			public static MultilingualString Organizations_eTailDepot { get { return CombineCategories(Organizations, ResString.GetMultilingualString("e39e18dc-6f2b-41b2-a7cf-75892a00b4eb", "eTail Depot")); } }
			public static MultilingualString Organizations_AddressValidationService_Background { get { return CombineCategories(Organizations_AddressValidationService, ResString.GetMultilingualString("CBB9B661-7567-47CF-B7B8-95821A17FE7B", "Background")); } }
			public static MultilingualString Organizations_AddressValidationService_Suppressions { get { return CombineCategories(Organizations_AddressValidationService, ResString.GetMultilingualString("6AF0D2F6-F29F-430D-B3F2-FC516D40B0F2", "Suppressions")); } }
			public static MultilingualString Organizations_AddedInfoPanel { get { return CombineCategories(Organizations, ResString.GetMultilingualString("766F4503-ACB3-4A73-9718-61C4499DFDAA", "Added Info Panel")); } }
			public static MultilingualString Organizations_Sales { get { return CombineCategories(Organizations, ResString.GetMultilingualString("73239416-4221-4DE2-9B3A-7DE2B4DDF03F", "Sales")); } }
			public static MultilingualString SalesMarketing_Commission { get { return CombineCategories(SalesMarketing, ResString.GetMultilingualString("0f0d6f91-3abd-4994-96d9-ae0805f3736a", "Commission")); } }
			public static MultilingualString SalesMarketing_SalesRelations { get { return CombineCategories(SalesMarketing, ResString.GetMultilingualString("10a131c3-5226-4d69-a4e0-3f449fd4da7d", "Sales Relations")); } }
			public static MultilingualString SalesMarketing_InquiryManager { get { return CombineCategories(SalesMarketing, ResString.GetMultilingualString("93246ad5-536c-480c-b8c1-0285ca53d1aa", "Inquiry Manager")); } }
			public static MultilingualString SalesMarketing_CommunicationManager { get { return CombineCategories(SalesMarketing, ResString.GetMultilingualString("108ff111-dc8f-4f11-a1d5-1b9a6abf8fe4", "Communication Manager")); } }
			public static MultilingualString SalesMarketing_CommunicationManager_Purpose { get { return CombineCategories(SalesMarketing_CommunicationManager, ResString.GetMultilingualString("e679f698-a9d0-41b1-97c0-82f18e837be6", "Purpose")); } }
			public static MultilingualString SalesMarketing_OpportunityManagement { get { return CombineCategories(SalesMarketing, ResString.GetMultilingualString("50dcec6d-900e-4d1a-8ae3-5b5047bffae6", "Opportunity Management")); } }
			public static MultilingualString SalesMarketing_OpportunityManagement_ProductType { get { return CombineCategories(SalesMarketing_OpportunityManagement, ResString.GetMultilingualString("847c21c6-9b9f-43c2-9c45-67818de80f9e", "Product Type")); } }
			public static MultilingualString SalesMarketing_OpportunityManagement_CustomizableLabels { get { return CombineCategories(SalesMarketing_OpportunityManagement, ResString.GetMultilingualString("7da9b8e3-fd5d-4114-81b7-5475fe3f9f91", "Customizable Labels")); } }
			public static MultilingualString SalesMarketing_CampaignManagement { get { return CombineCategories(SalesMarketing, ResString.GetMultilingualString("d7231831-cfad-435b-bd02-7301743802a6", "Campaign Management")); } }
			public static MultilingualString SalesMarketing_CampaignManagement_ClientRelationshipManagement { get { return CombineCategories(SalesMarketing_CampaignManagement, ResString.GetMultilingualString("776A3EED-F512-4503-A889-7BC7A5584909", "Client Relationship Management")); } }
			public static MultilingualString SalesMarketing_CampaignManagement_ClientRelationshipManagement_Category1List { get { return CombineCategories(SalesMarketing_CampaignManagement_ClientRelationshipManagement, ResString.GetMultilingualString("7e4fa8a4-dd92-40a8-8ab5-b891933778db", "Category 1 List")); } }
			public static MultilingualString SalesMarketing_CampaignManagement_ClientRelationshipManagement_Category2List { get { return CombineCategories(SalesMarketing_CampaignManagement_ClientRelationshipManagement, ResString.GetMultilingualString("ce0f6bf9-06d3-43e1-ab72-fb87141216e7", "Category 2 List")); } }
			public static MultilingualString SalesMarketing_CampaignManagement_HumanResources { get { return CombineCategories(SalesMarketing_CampaignManagement, ResString.GetMultilingualString("EB113525-6E36-4E51-9F30-E5F5BB502A7A", "Human Resources")); } }
			public static MultilingualString SalesMarketing_CampaignManagement_HumanResources_Category1List { get { return CombineCategories(SalesMarketing_CampaignManagement_HumanResources, ResString.GetMultilingualString("4029D744-A974-461C-8CD2-483078B7CBC3", "Category 1 List")); } }
			public static MultilingualString SalesMarketing_CampaignManagement_HumanResources_Category2List { get { return CombineCategories(SalesMarketing_CampaignManagement_HumanResources, ResString.GetMultilingualString("D6023539-A757-434E-8ECB-0A06678B7F3D", "Category 2 List")); } }
			public static MultilingualString SalesMarketing_SalesProduct { get { return CombineCategories(SalesMarketing, ResString.GetMultilingualString("9d753e41-3951-4298-aa45-50836551088a", "Sales Product")); } }
			public static MultilingualString SalesMarketing_CampaignManagement_Unsubscribe => CombineCategories(SalesMarketing_CampaignManagement, ResString.GetMultilingualString("7b9c12f2-a62a-46e2-9d7d-125647844239", "Unsubscribe"));
			public static MultilingualString SalesMarketing_CampaignManagement_EmbeddedImages => CombineCategories(SalesMarketing_CampaignManagement, ResString.GetMultilingualString("e81f974d-a564-4954-a0c8-248d1eaa24d0", "Embedded Images"));
			public static MultilingualString SalesMarketing_CampaignManagement_SubscripitionPreference => CombineCategories(SalesMarketing_CampaignManagement, ResString.GetMultilingualString("8031afc7-9608-4f6d-bbff-77817d8fd61f", "Subscription Preference"));
			public static MultilingualString SalesMarketing_Glow { get { return CombineCategories(SalesMarketing, ResString.GetMultilingualString("8ea3f9ec-862d-4025-9540-e6c4d9b16e02", "GLOW")); } }
			public static MultilingualString SalesMarketing_Glow_CommunicationManagement { get { return CombineCategories(SalesMarketing_Glow, ResString.GetMultilingualString("ed74df6b-b997-4620-a0e8-4913f0bfd541", "Communication Management")); } }
			public static MultilingualString SalesMarketing_Glow_OpportunityManagement { get { return CombineCategories(SalesMarketing_Glow, ResString.GetMultilingualString("718745d6-87e3-4541-be8d-e194ad9f3849", "Opportunity Management")); } }
			public static MultilingualString SalesMarketing_Glow_OpportunityManagement_OppScopeConfigurableParameters { get { return CombineCategories(SalesMarketing_Glow_OpportunityManagement, ResString.GetMultilingualString("50D10F47-47B3-4E6F-918A-E61D8930AAF7", "Opportunity Scope Configurable Parameters")); } }
			public static MultilingualString SalesMarketing_Glow_OpportunityManagement_OppScopeConfigurableParameters_ConfigurableParameters { get { return CombineCategories(SalesMarketing_Glow_OpportunityManagement_OppScopeConfigurableParameters, ResString.GetMultilingualString("F7EBC0DF-8D7B-460F-890E-083DBA3A7814", "Configurable Parameters")); } }
			public static MultilingualString SalesMarketing_Glow_OpportunityManagement_OppScopeConfigurableParameters_PrioritySequence { get { return CombineCategories(SalesMarketing_Glow_OpportunityManagement_OppScopeConfigurableParameters, ResString.GetMultilingualString("07FAD315-FEB2-4602-96B0-9164BDCC5291", "Priority Sequence")); } }
			public static MultilingualString Organizations_Duplicate_Detection => CombineCategories(Organizations, ResString.GetMultilingualString("95ce3cd1-b805-498d-81b0-d800a8901198", "Duplicate Detection"));
			public static MultilingualString Organizations_DuplicateDetection_Configuration => CombineCategories(Organizations_Duplicate_Detection, ResString.GetMultilingualString("77C02CA7-8099-4645-A539-AF9C51A0E106", "Configuration"));
			public static MultilingualString Organizations_DuplicateDetection_Configuration_OrganisationForm => CombineCategories(Organizations_DuplicateDetection_Configuration, ResString.GetMultilingualString("7F8C8004-03A6-484A-8D62-7C78930E089D", "Organization Form"));
			public static MultilingualString Organizations_DuplicateDetection_Configuration_Global => CombineCategories(Organizations_DuplicateDetection_Configuration, ResString.GetMultilingualString("7995D956-0BE2-4620-A02D-349194513299", "Global"));
			public static MultilingualString Organizations_CreditReports => CombineCategories(Organizations, ResString.GetMultilingualString("F53A27F8-F0BD-4E56-9E53-469D6A3596AA", "Credit Reports"));
			public static MultilingualString Organizations_CreditReports_Timeouts => CombineCategories(Organizations_CreditReports, ResString.GetMultilingualString("07FC71A6-14C3-4013-B535-29F185A0DD62", "Timeouts"));
		}

		#endregion

		#region Code Lists

		#region Contact Job Categories

		public CodeDescriptionBoolRegistryItem ContactJobCategories
		{
			get
			{
				return GetItem(
					"OrganisationsContactJobTitles",
					() =>
					{
						var defaultValues = new CodeDescriptionBoolCollection(3);
						foreach (CodeDescriptionPair pair in new OrgContactJobCategories())
						{
							defaultValues.AddSystemDefined(pair.Code, pair.MultilingualDescription, true);
						}

						return new CodeDescriptionBoolRegistryItem(
							"OrganisationsContactJobTitles",
							Categories.Organizations_CodeLists,
							ResString.GetMultilingualString("b6b5b4b9-90f6-4df5-af52-ce118d5d67e2", "Contact Job Categories"),
							ResString.GetMultilingualString("a82adf89-cf57-4859-af65-fe957a917ee7", "A list of valid job categories for contacts on organizations."),
							RegistryStorageFlags.System,
							new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("2eeb3128-c7aa-4590-aac2-ad845ad1c022", "Not Used"), false),
							defaultValues);
					});
			}
		}

		#endregion

		#region Contact Source Types

		public CodeDescriptionPairListRegistryItem ContactSourceTypes
		{
			get
			{
				return GetItem("OrganisationsContactSourceTypes", delegate
				{
					CodeDescriptionPairList defaultValue = new CodeDescriptionPairList();

					defaultValue.AddPair(ResString.GetMultilingualString("b0964124-b045-43ee-a7ad-c2d73472200c", "Advertisement"));
					defaultValue.AddPair(ResString.GetMultilingualString("0b34f79d-7d2f-440b-8c39-4fede2585712", "Campaign"));
					defaultValue.AddPair(ResString.GetMultilingualString("66d46b20-35a4-434b-bf12-a7647270e449", "Event"));
					defaultValue.AddPair(ResString.GetMultilingualString("b59c40fc-0d74-4769-b1f0-6bfa49ab2132", "Existing Client"));
					defaultValue.AddPair(ResString.GetMultilingualString("f6a60f7a-f3ae-4e7e-a695-aadfa2db1130", "Explicit List Name"));
					defaultValue.AddPair(ResString.GetMultilingualString("41926a6b-dafc-4a65-88c2-0c8c530215de", "Referral"));
					defaultValue.AddPair(ResString.GetMultilingualString("999be0ad-44e2-4c46-a24e-818605ad9556", "Website"));

					CodeDescriptionPairListRegistryItem result = new CodeDescriptionPairListRegistryItem(
						"OrganisationsContactSourceTypes",
						Categories.Organizations_CodeLists,
						ResString.GetMultilingualString("c890e88a-d6ca-4e99-aca5-a16192c69008", "Contact Source List"),
						ResString.GetMultilingualString("4fb20d89-0762-40c8-87d1-98ad30b23f64", "A list of sources of how this contact was acquired."),
						OrgContactSchema.OC_ContactSource.MaxLength,
						RegistryStorageFlags.System,
						true,
						RegistryOptions.PreserveTestValue,
						defaultValue);

					result.EditorInfo = new CodeDescriptionPairListEditorInfo(true, false,
						CodeDescriptionPairListEditorInfo.CharacterCasing.Normal, CodeDescriptionPairListEditorInfo.CharacterCasing.Normal);
					return result;
				});
			}
		}

		#endregion

		#region User Defined Context

		public CodeDescriptionPairListRegistryItem UserDefinedContext
		{
			get
			{
				return GetItem("OrganisationsUserDefinedContexts", delegate
				{
					CodeDescriptionPairList defaultValue = new CodeDescriptionPairList();

					CodeDescriptionPairListRegistryItem result = new CodeDescriptionPairListRegistryItem(
						"OrganisationsUserDefinedContexts",
						Categories.Organizations_CodeMappings,
						ResString.GetMultilingualString("49d4de26-dcb5-46aa-a20a-4871ca9e313e", "User Defined Context"),
						ResString.GetMultilingualString("445fabf5-ef5e-4e78-b2c1-717a272b0b88", "Define your own contexts that are included with EDI code mappings when exported."),
						OrgPatternMatchOverrideSchema.OO_Context.MaxLength,
						RegistryStorageFlags.System,
						true,
						RegistryOptions.PreserveTestValue,
						defaultValue);

					result.EditorInfo = new CodeDescriptionPairListEditorInfo(true, true,
						CodeDescriptionPairListEditorInfo.CharacterCasing.Upper, CodeDescriptionPairListEditorInfo.CharacterCasing.Normal);
					return result;
				});
			}
		}

		#endregion

		#region Enable EDI Code Mapping Module

		public BooleanRegistryItem EnableEDICodeMappingModule
		{
			get
			{
				return GetItem("EnableEDICodeMappingModule", delegate
				{
					return new BooleanRegistryItem(
						"EnableEDICodeMappingModule",
						Categories.Organizations_CodeMappings,
						(NoResString)"Enable EDI Code Mapping Module",
						(NoResString)"When turned on, can find Maintain -> EDI Messaging -> EDI Code Mapping module",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#endregion

		#region Sales & Marketing

		#region Commission

		public CommissionPeriodListRegistryItem CommissionPeriodList
		{
			get
			{
				return GetItem("CommissionPeriodList", delegate
				{
					var defaultValue = new CommissionPeriodCollection();
					defaultValue.AddNew("NEW", ResString.GetMultilingualString("f106cb97-c1af-4c0c-aab6-dde1988f9c50", "New client"), 0, 12);
					defaultValue.AddNew("EXS", ResString.GetMultilingualString("c4e31007-7e54-481c-b21e-9c508603dfd7", "Existing client"), 12, 0);

					return new CommissionPeriodListRegistryItem(
						"CommissionPeriodList",
						Categories.SalesMarketing_Commission,
						ResString.GetMultilingualString("e15627cf-f04d-4d2a-8464-f198baa9e30b", "Commission Entitlement Periods List"),
						ResString.GetMultilingualString("d626355d-aa61-48fa-9c1e-2bd3348fc5ec", @"The list of commission entitlement period options that can be selected for sales rep commission calculation.

Start and End values are measured in number of months. A value of '0' for Start or End will make the respective bounds unconstrained.
E.g. Start value of '12' and End of '0', is an entitlement period of 'From second year onwards'"),
						RegistryStorageFlags.System,
						new CommissionPeriodListRegistryEditorInfo(),
						defaultValue);
				});
			}
		}

		public BooleanRegistryItem AutoApproveFutureCommissionAgreements
		{
			get
			{
				return GetItem("AutoApproveFutureCommissionAgreements", delegate
				{
					return new BooleanRegistryItem(
						"AutoApproveFutureCommissionAgreements",
						Categories.SalesMarketing_Commission,
						ResString.GetMultilingualString("bc36b19e-10bc-4968-a1b6-c41c48e25858", "Auto-Approve Future Commission Agreements"),
						ResString.GetMultilingualString("9b3c35e7-ebec-4b50-8e27-3cf303505d05", @"When this registry is set to 'Yes', new or modifications to commission agreements with an effective date in the future will be automatically approved."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		public CodePairRegistryItem CommissionRecognitionDate
		{
			get
			{
				return GetItem("CommissionRecognitionDate", delegate
				{
					return new CodePairRegistryItem(
						"CommissionRecognitionDate",
						Categories.SalesMarketing_Commission,
						ResString.GetMultilingualString("fe61e852-36d6-4213-926b-b87c80a1935a", "Commission Recognition Date"),
						ResString.GetMultilingualString("1a564dd5-ea3a-44bb-882c-24efb12f795d", @"The date to use for commission recognition."),
						new CodeDescriptionPairListProvider(() => new CommissionRecognitionDateTypeList()),
						RegistryStorageFlags.System,
						CommissionRecognitionDateTypeList.Codes.PostDateOfFirstArTransaction);
				});
			}
		}

		public CodePairRegistryItem CommissionTransactionJobTrigger
		{
			get
			{
				return GetItem("CommissionTransactionJobTrigger", delegate
				{
					return new CodePairRegistryItem(
						"CommissionTransactionJobTrigger",
						Categories.SalesMarketing_Commission,
						ResString.GetMultilingualString("129EE6FF-239E-4050-AC1B-7766F794E2B0", "Job Billing Triggers for Commission Transactions"),
						ResString.GetMultilingualString("67296D00-7058-4EF7-ABB2-7F08CE305E67", @"By default (CLS option), both Revenue and Profit based commission transactions are created at Billing Job = CLS status.

Overriding the value to 'REV' will allow Revenue based commission transactions to be created at Job Billing status = INV. Profit based commission calculations will continue to be created at Job Billing = CLS status.

Changes to the Job Billing Triggers will only apply to new commission transactions. Commission Agreements will have to be appended before existing unpaid transactions are rewarded."),
						new CodeDescriptionPairListProvider(() => new CommissionTransactionJobTriggerList()),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						CommissionTransactionJobTriggerList.Codes.RevenueProfitBasedCommissionCalculationsToBeCreatedAtClsStatus);
				});
			}
		}

		public BooleanRegistryItem OnlyShowCommissionsForCurrentLoginCompany
		{
			get
			{
				return GetItem("OnlyShowCommissionsForCurrentLoginCompany", delegate
				{
					return new BooleanRegistryItem(
						"OnlyShowCommissionsForCurrentLoginCompany",
						Categories.SalesMarketing_Commission,
						ResString.GetMultilingualString("f3fb1df2-024b-4931-8463-764c3ed9be8e", "Only Show Commissions for Current Login Company"),
						ResString.GetMultilingualString("3a3a59f4-689f-4b62-b0d4-abd1bf0c9398", @"When this registry is set to 'Yes', only the commissions for the current login company are shown in the Commission Management Module."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		public BooleanRegistryItem OverwriteOldValuesOnJobClosed
		{
			get
			{
				return GetItem("OverwriteOldValuesOnJobClosed", delegate
				{
					return new BooleanRegistryItem(
						"OverwriteOldValuesOnJobClosed",
						Categories.SalesMarketing_Commission,
						ResString.GetMultilingualString("186d2203-47cd-4e97-9dc8-934cc2eee9a5", "Overwrite Old Values On Job Closed"),
						ResString.GetMultilingualString("bc07a0c6-6113-45bc-8990-e0c9166d408f", @"When this registry is set to 'Yes', existing commissions for a job are recalculated when the job is closed. This is relevant when there has been multiple postings of charges against the job."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public IntRegistryItem CommissionApprovalLevelRequired
		{
			get
			{
				return GetItem("CommissionApprovalLevelRequired", delegate
				{
					const int defaultValue = 1;
					const int minValue = 0;
					const int maxValue = 2;

					return new IntRegistryItem(
						"CommissionApprovalLevelRequired",
						Categories.SalesMarketing_Commission,
						ResString.GetMultilingualString("0583f10c-53fd-430f-94df-8fcad9b71d2d", "Commission Approval Level Required"),
						ResString.GetMultilingualString("7f9cbab9-7962-4edf-8540-63a79c70b355", @"The number of staff required to approve entity commissions before payment can be processed."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						defaultValue,
						minValue,
						maxValue);
				});
			}
		}

		public CodeDescriptionBoolRegistryItem CustomCommissionTypes
		{
			get
			{
				return GetItem("CustomCommissionTypes", delegate
				{
					return new CodeDescriptionBoolRegistryItem(
						"CustomCommissionTypes",
						Categories.SalesMarketing_Commission,
						ResString.GetMultilingualString("b8cf3299-2b48-47f8-b216-afb9a7f3cc0e", "Custom Commission Types"),
						ResString.GetMultilingualString("8b0e693d-1e42-479e-a395-fc633ecbdc82", @"Additional Commission Types that can be selected when importing manual entity commissions."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						ResString.GetMultilingualString("d81c25c9-14af-4e75-aa12-314578b9180a", "Enabled"),
						true);
				});
			}
		}

		public BooleanRegistryItem DisallowCommissionPaymentIfARInvoiceNotFullyPaid
		{
			get
			{
				return GetItem("DisallowCommissionPaymentIfARInvoiceNotFullyPaid", delegate
				{
					return new BooleanRegistryItem(
						"DisallowCommissionPaymentIfARInvoiceNotFullyPaid",
						Categories.SalesMarketing_Commission,
						ResString.GetMultilingualString("10359398-F39D-442F-AFDB-4CC278568A8B", "Disallow Commission Payment If AR Invoice(s) Have Not Been Fully Paid"),
						ResString.GetMultilingualString("836AD0CA-F9F4-47D2-989D-36D3535CA122", @"When this registry is set to 'Yes', if there are any unpaid AR invoices, the related commissions can not be processed for payment."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public IntRegistryItem CommissionFinalizerMaxNumberOfRecordsToShowInDisplayGrids
		{
			get
			{
				return GetItem("CommissionFinalizerMaxNumberOfRecordsToShowInDisplayGrids", () =>
					new IntRegistryItem(
						"CommissionFinalizerMaxNumberOfRecordsToShowInDisplayGrids",
						Categories.SalesMarketing_Commission,
						ResString.GetMultilingualString("98E07652-26F6-40EE-98DE-A1E842685590", "Commission Finalizer Max. No. of Records to Show"),
						ResString.GetMultilingualString("7BEFF762-425F-4084-8E44-C67B680941C9", "An error icon will show on a Display Grid if a search returns more than the maximum number of results. Results will not be displayed if this happens."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						5000,
						1,
						40000)
				);
			}
		}

		public BooleanRegistryItem RunCommissionCreationInBackground
		{
			get
			{
				return GetItem("RunCommissionCreationInBackground", delegate
				{
					return new BooleanRegistryItem(
						"RunCommissionCreationInBackground",
						Categories.SalesMarketing_Commission,
						ResString.GetMultilingualString("9dc4a540-a985-4479-b855-2763cabaeb20", "Run Commission Creation from Saves In the Background"),
						ResString.GetMultilingualString("510724c8-e1c8-42fc-87c1-6602546f4aec", @"When this registry is set to 'Yes', then if a job is closed or an invoice is posted the commission creation will be done in the background by the CGN service task."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#region Sales Relations

		#region Lead Interests

		public SalesRelationDirectionRulesRegistryItem SalesRelationDirectionRules
		{
			get
			{
				return GetItem("SalesRelationDirectionRules", delegate
				{
					var defaultValue = SalesRelationDirectionRuleCollection.GetDefaultValues();

					return new SalesRelationDirectionRulesRegistryItem(
						"SalesRelationDirectionRules",
						Categories.SalesMarketing_SalesRelations,
						ResString.GetMultilingualString("a1aa728a-a8e3-4117-8299-9c68c330bc9d", "Sales Relation Direction Rules"),
						ResString.GetMultilingualString("bbbc1e1e-2d73-4361-9e44-e593adeddd0c", "The list of rules that restrict the sequence of linked Sales Relation Activities."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						defaultValue);
				});
			}
		}

		#endregion

		#endregion

		#region Enquiry

		#region Lead Interests

		public CodeDescriptionBoolRegistryItem SalesEnquiryLeadInterests
		{
			get
			{
				return GetItem("EnquiriesLeadInterests", delegate
				{
					var defaultValue = new CodeDescriptionBoolCollection(3);
					defaultValue.Add("HOT", ResString.GetMultilingualString("ac1fbda7-8d1f-4be7-8696-548918bd3704", "Hot"));
					defaultValue.Add("WRM", ResString.GetMultilingualString("336540a5-d9e4-446f-b0b5-e5216bcc80c8", "Warm"));
					defaultValue.Add("CLD", ResString.GetMultilingualString("21d75a43-a9b4-4189-ac04-8b39add024df", "Cold"));

					return new CodeDescriptionBoolRegistryItem(
						"EnquiriesLeadInterests",
						Categories.SalesMarketing_InquiryManager,
						ResString.GetMultilingualString("2F74A570-31BB-47EC-A547-D5B5CB1BBCE7", "Lead Interest"),
						ResString.GetMultilingualString("BDE663D2-7AC7-4CC9-898E-B304E56FEAFA", "The list of lead interests that can be used on a Sales Inquiry."),
						RegistryStorageFlags.System,
						ResString.GetMultilingualString("94c8d660-c51d-46fa-9342-837f90fb68ae", "Enabled"),
						defaultValue);
				});
			}
		}

		#endregion

		#region Sales Enquiry Type List

		public CodeDescriptionBoolRegistryItem SalesEnquiryTypeList
		{
			get
			{
				return GetItem("SalesEnquiryTypeList", delegate
				{
					var defaultValue = new CodeDescriptionBoolCollection(3);
					defaultValue.Add("CCR", ResString.GetMultilingualString("Sales|SalesInquiryTypeList|CCR", "Cold Call"));
					defaultValue.Add("INQ", ResString.GetMultilingualString("Sales|SalesInquiryTypeList|INQ", "General Inquiry"));
					defaultValue.Add("WEB", ResString.GetMultilingualString("Sales|SalesInquiryTypeList|WEB", "Web Site Inquiry"));
					defaultValue.Add("PHN", ResString.GetMultilingualString("Sales|SalesInquiryTypeList|PHN", "Phone Inquiry"));
					defaultValue.Add("EML", ResString.GetMultilingualString("Sales|SalesInquiryTypeList|EML", "Email Inquiry"));
					defaultValue.Add("CAM", ResString.GetMultilingualString("Sales|SalesInquiryTypeList|CAM", "Campaign Response"));
					defaultValue.Add("WBI", ResString.GetMultilingualString("Sales|SalesInquiryTypeList|WBI", "Webinar Response"));
					defaultValue.Add("PSO", ResString.GetMultilingualString("Sales|SalesInquiryTypeList|PSO", "Product/Service Opportunity"));

					var result = new CodeDescriptionBoolRegistryItem(
						"SalesEnquiryTypeList",
						Categories.SalesMarketing_InquiryManager,
						ResString.GetMultilingualString("67BCAD0B-E3D4-40AA-A7B1-AF175DC2710A", "Type"),
						ResString.GetMultilingualString("97337758-0063-4DBE-B84C-A7ABA23E0158", "A list of inquiry types which can be used on a Sales Inquiry."),
						RegistryStorageFlags.System,
						ResString.GetMultilingualString("94c8d660-c51d-46fa-9342-837f90fb68ae", "Enabled"),
						defaultValue);

					return result;
				});
			}
		}

		#endregion

		#region Sales Enquiry Close Reason List

		public CodeDescriptionBoolRegistryItem SalesEnquiryCloseReasonList
		{
			get
			{
				return GetItem("SalesEnquiryCloseReasonList", delegate
				{
					CodeDescriptionBoolCollection defaultValue = new CodeDescriptionBoolCollection();
					defaultValue.Add("UDF", ResString.GetMultilingualString("268E650B-633D-4C62-BE8B-63A8E4CDD1BB", "You can change this list in the registry at {0}/Close Reason", Categories.SalesMarketing_InquiryManager));

					return new CodeDescriptionBoolRegistryItem(
						"SalesEnquiryCloseReasonList",
						Categories.SalesMarketing_InquiryManager,
						ResString.GetMultilingualString("C2806EB1-A013-4F60-9C4A-3D656E67717E", "Close Reason"),
						ResString.GetMultilingualString("93B12375-55D9-4F56-8704-E638C9ECB643", "A list of inquiry close reasons which can be used on a Sales Inquiry."),
						RegistryStorageFlags.System,
						ResString.GetMultilingualString("94c8d660-c51d-46fa-9342-837f90fb68ae", "Enabled"),
						defaultValue);
				});
			}
		}

		#endregion

		#region Fields Mandatory

		public CodeDescriptionBoolDisallowNewRegistryItem SalesEnquiryFieldsMandatory
		{
			get
			{
				return GetItem("SalesEnquiryFieldsMandatory",
					delegate
					{
						var defaultValue = new CodeDescriptionBoolDisallowNewCollection();
						defaultValue.Add(50, OrgColdCallRegisterSchema.Constants.O1_CloseReason, ResString.GetMultilingualString("80A36E76-918E-48B9-8EA1-8BA492DE6E5B", "Close Reason"), true);

						return new CodeDescriptionBoolDisallowNewRegistryItem(
							"SalesEnquiryFieldsMandatory",
							Categories.SalesMarketing_InquiryManager,
							ResString.GetMultilingualString("CB94BD9D-BB66-4FB1-8C64-F5F070466E31", "Mandatory Fields"),
							ResString.GetMultilingualString("A0801371-659B-4019-BCAB-D068FAED3E0D", "Specifies whether Inquiry fields are mandatory."),
							RegistryStorageFlags.System,
							new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("6AAF84D0-BF36-42F0-B5B7-B5CB6E8AA885", "Is Mandatory"), true, true),
							defaultValue);
					});
			}
		}

		#endregion

		#region Sales Enquiry Default Assigned Sales Rep

		public BooleanRegistryItem SalesEnquiryDefaultAssignedSalesRep
		{
			get
			{
				return GetItem("SalesEnquiryDefaultAssignedSalesRep", delegate
				{
					return new BooleanRegistryItem(
						"SalesEnquiryDefaultAssignedSalesRep",
						Categories.SalesMarketing_InquiryManager,
						ResString.GetMultilingualString("C7A6680E-19FE-4544-BA5D-37351A469818", "Default Assigned Sales Rep"),
						ResString.GetMultilingualString("E702EF8D-BB42-4D59-A86E-73FD83DAB9FB", "If this registry is set to 'Yes', when the Organization is updated on an Inquiry, the 'Assigned Sales Rep' is automatically set to the Sales Representative assigned to the Organization for all departments."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#endregion

		#region Communication

		public const string CommunicationMandatoryFieldsClosedStatusRequiredCode = "ClosedStatusRequired";

		public CodeDescriptionBoolDisallowNewRegistryItem CommunicationMandatoryFields
		{
			get
			{
				return GetItem("CommunicationMandatoryFields", delegate
				{
					var defaultValue = new CodeDescriptionBoolDisallowNewCollection();
					defaultValue.Add(50, OrgSalesCallSchema.Constants.OQ_TypeOfCall, ResString.GetMultilingualString("336C25B7-3B75-4149-9515-9EF69176132C", "Communication Method"), true);
					defaultValue.Add(50, OrgSalesCallSchema.Constants.OQ_CallSummary, ResString.GetMultilingualString("76E0F1C7-73A2-4B0B-B515-274A0A7FC567", "Subject"), true);
					defaultValue.Add(50, OrgSalesCallSchema.Constants.OQ_Status, ResString.GetMultilingualString("DEEBD338-46ED-41FF-BDA2-E6CB46E78E4F", "Status"), true);
					defaultValue.Add(50, OrgSalesCallSchema.Constants.OQ_Category, ResString.GetMultilingualString("0535f7b8-8fb3-48a8-b1ed-8dd3f75acbbc", "Purpose"), false);
					defaultValue.Add(50, CommunicationMandatoryFieldsClosedStatusRequiredCode, ResString.GetMultilingualString("A0DC9F33-67D4-4346-A726-917492D9C9EE", "'Closed' Overall Disposition on Actual Date entry"), true);

					return new CodeDescriptionBoolDisallowNewRegistryItem(
						"CommunicationMandatoryFields",
						Categories.SalesMarketing_CommunicationManager,
						ResString.GetMultilingualString("BF59A4AC-AF0A-4257-BB3C-7C3A5A1D1CD4", "Mandatory Fields"),
						ResString.GetMultilingualString("05E24C74-5CDF-43D5-B60D-D2F521336E74", "Specifies whether communication fields are mandatory."),
						RegistryStorageFlags.System,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("B0D902EA-DBDF-4A2A-A3FC-4AEAC31B3D5B", "Is Mandatory"), true, true),
						defaultValue);
				});
			}
		}

		public CodeDescriptionBoolRegistryItem CommunicationType
		{
			get
			{
				return GetItem("CommunicationType", delegate
				{
					var defaultValue = new CodeDescriptionBoolCollection(3);
					defaultValue.AddSystemDefined(Constants.Sales.CommunicationType.PhoneCall, ResString.GetMultilingualString("A7792589-5BAF-40A6-B2E9-D81866236105", "Phone Call"), true);
					defaultValue.AddSystemDefined(Constants.Sales.CommunicationType.Meeting, ResString.GetMultilingualString("3122FE57-2F52-4798-95B5-7F5158789A47", "Meeting"), true);
					defaultValue.AddSystemDefined(Constants.Sales.CommunicationType.Email, ResString.GetMultilingualString("BB2FB4CB-6E84-4FCF-91C1-C9F94A053624", "Email"), true);
					defaultValue.AddSystemDefined(Constants.Sales.CommunicationType.OnlineConference, ResString.GetMultilingualString("AE36E644-F254-4BA5-94BA-F12537A020D6", "Online Conference"), true);
					defaultValue.Add(Constants.Sales.CommunicationType.FirstCall, ResString.GetMultilingualString("a03e32a0-2905-4bdd-912a-56eb72d9c5fa", "First call to prospective client."), true);
					defaultValue.Add(Constants.Sales.CommunicationType.FollowUp, ResString.GetMultilingualString("ca7ac6da-2b7d-41db-9b1c-06effdd56a2b", "Follow up call to prospective client."), true);
					defaultValue.Add(Constants.Sales.CommunicationType.Service, ResString.GetMultilingualString("a603284d-3455-4ffb-bcf2-b2b63412013b", "Service call on existing client."), true);

					return new CodeDescriptionBoolRegistryItem(
							"CommunicationType",
							Categories.SalesMarketing_CommunicationManager,
							ResString.GetMultilingualString("14b0fc26-ea55-4566-86e5-800211658fae", "Method"),
							ResString.GetMultilingualString("B9A72BFB-F78E-4984-8B68-6FE1B5BF5F20", "The list of valid entries for communication method."),
							RegistryStorageFlags.System,
							new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("acc43fe1-ce9d-4a30-b8d1-99ac9f39c2f2", "Enabled")),
							defaultValue);
				});
			}
		}

		#region Category List

		#region Label
		public MultilingualStringRegistryItem CategoryListLabel
		{
			get
			{
				return GetItem("PurposeCategoryLabel", delegate
				{
					return new MultilingualStringRegistryItem(
						"PurposeCategoryLabel",
						Categories.SalesMarketing_CommunicationManager_Purpose,
						ResString.GetMultilingualString("929d352b-788d-4bd2-8c02-c086ccd93877", "Label"),
						ResString.GetMultilingualString("1a55850b-9d92-4b70-89c9-0142ee857752", "Allows you to further categorize your communications, by purpose identifier."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						ResString.GetMultilingualString("9757889c-e09a-4297-8dff-24184e3bdf85", "Purpose"));
				});
			}
		}
		#endregion

		#region List

		public CodeDescriptionBoolRegistryItem CategoryList
		{
			get
			{
				return GetItem("PurposeCategoryList", delegate
				{
					CodeDescriptionBoolCollection defaultValue = new CodeDescriptionBoolCollection(OrgSalesCallSchema.OQ_Category.MaxLength);
					defaultValue.Add("UDF", ResString.GetMultilingualString("41bc5325-8812-47bc-b7cb-864cb6455f11", "You can change this list in the registry at {0}/List", Categories.SalesMarketing_CommunicationManager_Purpose));

					return new CodeDescriptionBoolRegistryItem(
						"PurposeCategoryList",
						Categories.SalesMarketing_CommunicationManager_Purpose,
						ResString.GetMultilingualString("fd8698f9-1104-4b25-afbf-da94b16c4ff8", "List"),
						ResString.GetMultilingualString("c38b7898-9e7d-483f-960d-ca81b7416888", "Allows you to nominate a purpose."),
						RegistryStorageFlags.System,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("aa6c5205-8289-4c48-bfc5-ee8f8dee30f7", "Enabled")),
						defaultValue);
				});
			}
		}

		#endregion

		#endregion

		public CommunicationStatusRegistryItem CommunicationStatusList
		{
			get
			{
				return GetItem("CommunicationStatus", delegate
				{
					var defaultValue = new CommunicationStatusCollection(false, true);
					defaultValue.AddSystemDefined(Constants.Sales.Status.Scheduled, ResString.GetMultilingualString("30649596-3C9F-4715-8CC3-DEBB6AF85806", "Scheduled"), false, true);
					defaultValue.AddSystemDefined(Constants.Sales.Status.Completed, ResString.GetMultilingualString("71060D9A-E651-4B07-8333-96662B96C790", "Completed"), true, true);
					defaultValue.AddSystemDefined(Constants.Sales.Status.Cancelled, ResString.GetMultilingualString("AE050F46-125F-4348-AD68-D33E7CA1E445", "Canceled"), true, true);
					defaultValue.Add(Constants.Sales.Status.CurrentClient, ResString.GetMultilingualString("791e3888-b49e-4e39-ada6-4e2f5e312c56", "Current client"), false, true);
					defaultValue.Add(Constants.Sales.Status.ConfirmedEntire, ResString.GetMultilingualString("eba4b44a-461d-498b-895a-7a2fad946183", "Confirmed entire business"), false, true);
					defaultValue.Add(Constants.Sales.Status.ConfirmedPartial, ResString.GetMultilingualString("59290a67-8b19-4d00-8d4e-ae81488cef9b", "Confirmed partial business"), false, true);
					defaultValue.Add(Constants.Sales.Status.HotProspect, ResString.GetMultilingualString("1a9042f7-380f-4196-afd9-c627106ba2f8", "Hot sales prospect"), false, true);
					defaultValue.Add(Constants.Sales.Status.WarmProspect, ResString.GetMultilingualString("d3df2c82-3064-4a6d-b519-53c0578806e0", "Warm sales prospect"), false, true);
					defaultValue.Add(Constants.Sales.Status.ColdProspect, ResString.GetMultilingualString("a9aee4b4-2ebc-4807-8941-7c92deecc54f", "Cold sales prospect"), false, true);
					defaultValue.Add(Constants.Sales.Status.MoreWork, ResString.GetMultilingualString("3e762682-7811-40ff-8b3b-5d2078e93eef", "More work to do on this client"), false, true);
					defaultValue.Add(Constants.Sales.Status.NoInterestNow, ResString.GetMultilingualString("11c34983-4408-43c6-a557-f516f2664e8d", "No interest at this time - follow up in time"), true, true);
					defaultValue.Add(Constants.Sales.Status.NoInterestAtAll, ResString.GetMultilingualString("0e124f1b-9d2c-4e4e-8ba3-5aad05dedb3f", "No interest in any further contact - please don't call"), true, true);

					return new CommunicationStatusRegistryItem(
							"CommunicationStatus",
							Categories.SalesMarketing_CommunicationManager,
							ResString.GetMultilingualString("749CA78C-C65F-4DD0-8CD3-03753D5FA722", "Status List"),
							ResString.GetMultilingualString("F70215A6-D82D-4527-9CE3-3FF131D9CD95", "The list of valid entries for communication statuses. Each status has a 'Closed' flag which determines whether a communication is closed when in that status."),
							RegistryStorageFlags.System,
							new CommunicationStatusRegistryEditorInfo(ResString.GetMultilingualString("94c8d660-c51d-46fa-9342-837f90fb68ae", "Enabled")),
							defaultValue);
				});
			}
		}

		public BooleanRegistryItem CommunicationAutoSendUponSave
		{
			get
			{
				return GetItem("CommunicationAutoSendUponSave", delegate
				{
					return new BooleanRegistryItem(
						"CommunicationAutoSendUponSave",
						Categories.SalesMarketing_CommunicationManager,
						ResString.GetMultilingualString("48F4A3A7-5167-4C10-985D-6A42F4C014A7", "Auto Send Upon Save"),
						ResString.GetMultilingualString("4F269031-EE9A-4CCC-9616-41C9E7C42B86", "When this registry is set to 'Yes', calendar invitations are automatically sent upon saving communication."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		public BooleanRegistryItem CreateCommunicationOnInquiryContactCall
		{
			get
			{
				return GetItem("CreateCommunicationOnInquiryContactCall", delegate
				{
					return new BooleanRegistryItem(
						"CreateCommunicationOnInquiryContactCall",
						Categories.SalesMarketing_CommunicationManager,
						ResString.GetMultilingualString("B80CE7C6-D674-4144-9DE0-1E2F94308447", "Create Communication on Inquiry Contact Call"),
						ResString.GetMultilingualString("E744E763-6435-4267-985A-7773CD610D70", "When this registry is set to 'Yes', a related communication is automatically created when a call is made to an inquiry contact."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		public BooleanRegistryItem CreateCommunicationOnOpportunityContactCall
		{
			get
			{
				return GetItem("CreateCommunicationOnOpportunityContactCall", delegate
				{
					return new BooleanRegistryItem(
						"CreateCommunicationOnOpportunityContactCall",
						Categories.SalesMarketing_CommunicationManager,
						ResString.GetMultilingualString("BFBBABC2-ACA8-4E1E-AF1C-7920341F2BAF", "Create Communication on Opportunity Contact Call"),
						ResString.GetMultilingualString("C74B0D47-765B-4BA8-A6B8-80CD97A926CD", "When this registry is set to 'Yes', a related communication is automatically created when a call is made to an opportunity contact."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		public BooleanRegistryItem CreateCommunicationOnProjectContactCall
		{
			get
			{
				return GetItem("CreateCommunicationOnProjectContactCall", delegate
				{
					return new BooleanRegistryItem(
						"CreateCommunicationOnProjectContactCall",
						Categories.SalesMarketing_CommunicationManager,
						ResString.GetMultilingualString("9E29CA86-FE9B-4CD7-B82E-A4CECDC61AB5", "Create Communication on Project Contact Call"),
						ResString.GetMultilingualString("94157122-88EE-4641-9A94-4EE1304F1948", "When this registry is set to 'Yes', a related communication is automatically created when a call is made to a project contact."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		public NotificationEmailTemplateRegistryItem CommunicationInternalCalendarReminderSubjectTemplate
		{
			get
			{
				return GetItem("CommunicationInternalCalendarReminderSubjectTemplate", delegate
				{
					NotificationEmailTemplateRegistryItem registryItem = new NotificationEmailTemplateRegistryItem(
						"CommunicationInternalCalendarReminderSubjectTemplate",
						Categories.SalesMarketing_CommunicationManager,
						ResString.GetMultilingualString("594CB8FC-512F-453C-8EF5-AA775A92031D", "Internal Calendar Reminder Subject"),
						ResString.GetMultilingualString("1ECABE57-08CD-4A3B-8CE4-98CD3C649366", "Configure the subject for Non Client Visible calendar reminders by highlighting and adding the desired macros."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						ObjectFactory.GetType<DocumentWrappers.IDocSalesCall>(),
						 @"[(*MethodCode*)] (*OrganizationFullName*) | (*CommunicationSubject*)",
						ZString.Empty, true);
					return registryItem;
				});
			}
		}

		public GuidRegistryItem CommunicationDocumentTypeParsedEmails
		{
			get
			{
				return GetItem("CommunicationDocumentTypeParsedEmails", delegate
				{
					var defaultEmailType = new BusinessObjectFactory().LoadTop1<IRefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, Core.Constants.RefDocTypes.MiscellaneousDocument));

					var result = new GuidRegistryItem(
						"CommunicationDocumentTypeParsedEmails",
						Categories.SalesMarketing_CommunicationManager,
						ResString.GetMultilingualString("21F17E24-3BBD-4CB7-88BF-BE466C38AE34", "Document Type Parsed Emails"),
						ResString.GetMultilingualString("87D32855-B7FF-4491-9732-C6A8D82219E0", "Document Type when parsing emails into Communication."),
						RegistryStorageFlags.System,
						defaultEmailType?.PK.ToGuid() ?? Guid.Empty);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.RefDocType, RegistryFindBoxFilter.RefDocTypeForCommunicationParsedEmail);
					return result;
				});
			}
		}

		#endregion

		#region Client Size List

		public CodeDescriptionPairListRegistryItem ClientSizeList
		{
			get
			{
				return GetItem("OrganisationsClientSizeList", delegate
				{
					CodeDescriptionPairList defaultValue = new CodeDescriptionPairList();
					defaultValue.AddPair("SML", ResString.GetMultilingualString("Organisation|ClientSizeList|Small", "Small"));
					defaultValue.AddPair("MED", ResString.GetMultilingualString("Organisation|ClientSizeList|Medium", "Medium"));
					defaultValue.AddPair("LRG", ResString.GetMultilingualString("Organisation|ClientSizeList|Large", "Large"));

					CodeDescriptionPairListRegistryItem result = new CodeDescriptionPairListRegistryItem(
						"OrganisationsClientSizeList",
						Categories.SalesMarketing_ClientIntelligence,
						ResString.GetMultilingualString("d68f0d4a-8ebb-4606-b657-e2e1c34b0240", "Sales Client Size List"),
						ResString.GetMultilingualString("df3803e9-7ec8-44bb-89ec-0775458e8bd8", "A list of possible client sizes which can be used on the Sales tab of an organization."),
						OrgMiscServSchema.OM_CMClientSize.MaxLength,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue);

					result.EditorInfo = new CodeDescriptionPairListEditorInfo(true, true,
						CodeDescriptionPairListEditorInfo.CharacterCasing.Normal, CodeDescriptionPairListEditorInfo.CharacterCasing.Normal);
					return result;
				});
			}
		}

		#endregion

		#region Client Intelligence

		#region Show Sales Activities

		public BooleanRegistryItem ShowSalesActivities
		{
			get
			{
				return GetItem("ShowSalesActivities", delegate
				{
					return new BooleanRegistryItem(
						"ShowSalesActivities",
						Categories.SalesMarketing_ClientIntelligence,
						ResString.GetMultilingualString("1124fff1-3d46-4851-a720-c50c5f2d2fc8", "Show Sales Activities"),
						ResString.GetMultilingualString("fb42331f-c781-4e6b-b924-0b0a9a00f724", "By default Sales Activity tab on Organization screen is shown. Disable this to hide it."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						true);
				});
			}
		}

		#endregion

		#endregion

		#region Default Values

		#region Buyers Consol Invoicing Style

		public CodePairRegistryItem BuyersConsolInvoicingStyle
		{
			get
			{
				return GetItem("BuyersConsolInvoicingStyle", delegate
				{
					return new CodePairRegistryItem(
						"BuyersConsolInvoicingStyle",
						Categories.Organizations_DefaultValues,
						ResString.GetMultilingualString("7605c2d5-e530-4bc8-a24b-4a8dc5d3e017", "Buyers Consol Invoicing Style"),
						ResString.GetMultilingualString("fe22e45f-15b9-4b5a-804e-efc473fe4c95", "Specify the default style for autorating and invoicing Buyers Consol Shipments."),
						OLookUpEditType.ConsolInvoicingStyles,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						"MAS");
				});
			}
		}

		#endregion

		#region Shippers Consol Invoicing Style

		public CodePairRegistryItem ShippersConsolInvoicingStyle
		{
			get
			{
				return GetItem("ShippersConsolInvoicingStyle", delegate
				{
					return new CodePairRegistryItem(
						"ShippersConsolInvoicingStyle",
						Categories.Organizations_DefaultValues,
						ResString.GetMultilingualString("b8faee79-3467-48cf-b83e-907cb1739f8c", "Shippers Consol Invoicing Style"),
						ResString.GetMultilingualString("fa142d53-78e9-4764-84a0-768477319627", "Specify the default style for autorating and invoicing Shippers Consol Shipments."),
						OLookUpEditType.ConsolInvoicingStyles,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						"MAS");
				});
			}
		}

		#endregion

		#region Consignee IncoTerm

		public CodePairRegistryItem ConsigneeIncoTerm
		{
			get
			{
				return GetItem("ConsigneeIncoTerm", delegate
				{
					return new CodePairRegistryItem(
						"ConsigneeIncoTerm",
						Categories.Organizations_DefaultValues_INCOTerms,
						ResString.GetMultilingualString("82370549-17bb-c491-4f3c-4d90c587a9a3", "Consignee Incoterm"),
						ResString.GetMultilingualString("651ab6ae-1083-d894-4550-5a665c2eda85", "Specify the default Incoterm for consignee organizations. These defaults will flow through to buyer and supplier relationships."),
						new IncoTermsCodeDescriptionPairListProvider(IncoTermsListType.ActiveIncoTerms),
						true,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						Constants.IncoTerms.FreeOnBoard);
				});
			}
		}

		#endregion

		#region Consignor IncoTerm

		public CodePairRegistryItem ConsignorIncoTerm
		{
			get
			{
				return GetItem("ConsignorIncoTerm", delegate
				{
					return new CodePairRegistryItem(
						"ConsignorIncoTerm",
						Categories.Organizations_DefaultValues_INCOTerms,
						ResString.GetMultilingualString("a7ce4101-6378-339d-4b8c-05eb8c41c891", "Consignor Incoterm"),
						ResString.GetMultilingualString("ed10f38c-d3a0-fb81-478f-fb89607b8936", "Specify the default Incoterm for consignor organizations. These defaults will flow through to buyer and supplier relationships."),
						new IncoTermsCodeDescriptionPairListProvider(IncoTermsListType.ActiveIncoTerms),
						true,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						Constants.IncoTerms.FreeOnBoard);
				});
			}
		}

		#endregion

		#region APPaymentMethod

		public APPaymentMethodRegistryItem APPaymentMethod
		{
			get
			{
				return GetItem("APPaymentMethod", delegate
				{
					return new APPaymentMethodRegistryItem(
						"APPaymentMethod",
						Categories.Organizations_DefaultValues,
						ResString.GetMultilingualString("f0b3d154-5178-4193-9ff6-af3b69594555", "AP Bank Account Payment Methods"),
						ResString.GetMultilingualString("44610836-4359-4559-a857-c9d1e845a109", "Specify the default payment method for Account Payables Organizations"),
						new APPaymentMethodListProvider(),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						ReceiptTypes.DirectDebit);
				});
			}
		}

		#endregion

		#region DefaultARCreditApproved

		public BooleanRegistryItem DefaultARCreditApproved
		{
			get
			{
				return GetItem("DefaultARCreditApproved", delegate
				{
					return new BooleanRegistryItem(
						"DefaultARCreditApproved",
						Categories.Organizations_DefaultValues,
						ResString.GetMultilingualString("cb454c1e-c4a6-48f8-abbf-3f27abafb34c", "AR Credit Approved"),
						ResString.GetMultilingualString("43e434d4-d58b-48c4-9a94-4f0bcb0884a0", "Specifies default value for the AR Credit Approved field."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		#endregion

		#region UseARSettlementGroupCreditLimit

		public BooleanRegistryItem UseARSettlementGroupCreditLimit
		{
			get
			{
				return GetItem("UseSettlementGroupCreditLimitOrgDefault", delegate
				{
					return new BooleanRegistryItem(
						"UseSettlementGroupCreditLimitOrgDefault",
						Categories.Organizations_DefaultValues,
						ResString.GetMultilingualString("f3563592-3144-4d3f-8a99-bcffb0ac8ffe", "Use AR Settlement Group Credit Limit"),
						ResString.GetMultilingualString("69d41492-a55e-4746-ac3f-07f92b24495d", @"This registry item controls default value of the 'Use Settlement Group Credit Limit' checkbox for a new Organization. This default value then can be changed by user.

The ‘Use Settlement Group Credit Limit’ setting on an Organization determines whether Credit Limit checking uses the Organization Settlement group when summing up transaction amounts to calculate the balance.  This includes Credit Limit checking performed by the Credit Limit and Balance Web Service.

When set to 'Yes', the 'Use Settlement Group Credit Limit' checkbox will be ticked for a new Organization and 'DEF' Invoice Term will be set for 'ALL' Invoice Type to provide a fall back to the Settlement Group's Invoice Terms settings.

When set to ‘No’ (default), it will make the mentioned above checkbox un-ticked for a new organization and Invoice Term will have its default ‘COD’ value."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#region Required Cartage Equipment

		#region AIR/FCL/LCL Lists

		#region LCLAirEquipmentList & Provider

		class LCLAirEquipmentListProvider : ICodeDescriptionPairListProvider
		{
			CodeDescriptionPairList ICodeDescriptionPairListProvider.CodeDescriptionPairList
			{
				get { return new LCLAIREquipmentNeededList(); }
			}
		}

		LCLAirEquipmentListProvider AirLCLListProvider
		{
			get
			{
				if (airLCLListProvider == null)
				{
					airLCLListProvider = new LCLAirEquipmentListProvider();
				}
				return airLCLListProvider;
			}
		}
		LCLAirEquipmentListProvider airLCLListProvider;

		#endregion

		#region FCLEquipmentList & Provider

		class FCLEquipmentListProvider : ICodeDescriptionPairListProvider
		{
			CodeDescriptionPairList ICodeDescriptionPairListProvider.CodeDescriptionPairList
			{
				get { return new FCLEquipmentNeededList(); }
			}
		}

		FCLEquipmentListProvider FCLListProvider
		{
			get
			{
				if (fclListProvider == null)
				{
					fclListProvider = new FCLEquipmentListProvider();
				}
				return fclListProvider;
			}
		}
		FCLEquipmentListProvider fclListProvider;

		#endregion

		#endregion

		#region RequiredCartageEquipmentAIR

		static MultilingualString RequiredCartageEquipmentAIRCaption
		{
			get { return ResString.GetMultilingualString("c3852ad9-92b4-4c15-98f3-fd25e4b4279b", "Air"); }
		}
		MultilingualString RequiredCartageEquipmentAIRHint
		{
			get { return ResString.GetMultilingualString("ac6d993b-2714-4687-bc25-92bb9aede398", "Select the default Port Transport equipment requirement for air freight. The list of air Port Transport equipment can be changed in the registry from the following location: {0}", new ModifiedMultilingualString(delegate { return ((IRegistryItemInternals)Env.Registry.RawRegistry.LCLAIREquipmentNeeded).Location; })); }
		}

		public CodePairRegistryItem RequiredCartageEquipmentAIR
		{
			get
			{
				return GetItem("RequiredCartageEquipmentAIR", delegate
				{
					return new CodePairRegistryItem(
						"RequiredCartageEquipmentAIR",
						Categories.Organizations_DefaultValues_RequiredPortTransportEquipment,
						RequiredCartageEquipmentAIRCaption,
						RequiredCartageEquipmentAIRHint,
						AirLCLListProvider,
						false,
						true,
						new ComboBoxRegistryEditorInfo(AirLCLListProvider, true),
						RegistryStorageFlags.All,
						RegistryOptions.PreserveTestValue,
						Constants.LCLAIREquipmentNeeded.Premise,
						false);
				});
			}
		}

		#endregion

		#region RequiredCartageEquipmentFCL

		static MultilingualString RequiredCartageEquipmentFCLCaption
		{
			get { return ResString.GetMultilingualString("0a8baf22-0b20-41dc-914d-8b5ae6c64451", "FCL"); }
		}
		MultilingualString RequiredCartageEquipmentFCLHint
		{
			get { return ResString.GetMultilingualString("56efd063-14cf-41fc-8279-d385f2b450c6", "Select the default Port Transport equipment requirement for FCL freight. The list of FCL Port Transport equipment can be changed in the registry from the following location: {0}", new ModifiedMultilingualString(delegate { return ((IRegistryItemInternals)Env.Registry.RawRegistry.FCLEquipmentNeeded).Location; })); }
		}

		public CodePairRegistryItem RequiredCartageEquipmentFCL
		{
			get
			{
				return GetItem("RequiredCartageEquipmentFCL", delegate
				{
					CodeDescriptionPairList fCLList = new FCLEquipmentNeededList();
					return new CodePairRegistryItem(
						"RequiredCartageEquipmentFCL",
						Categories.Organizations_DefaultValues_RequiredPortTransportEquipment,
						RequiredCartageEquipmentFCLCaption,
						RequiredCartageEquipmentFCLHint,
						FCLListProvider,
						false,
						true,
						new ComboBoxRegistryEditorInfo(FCLListProvider, true),
						RegistryStorageFlags.All,
						RegistryOptions.PreserveTestValue,
						Constants.FCLEquipmentNeeded.WaitForUnpack,
						false);
				});
			}
		}

		#endregion

		#region RequiredCartageEquipmentLCL

		static MultilingualString RequiredCartageEquipmentLCLCaption
		{
			get { return ResString.GetMultilingualString("7680a72f-848c-4dd4-9ad9-383eb891029e", "LCL"); }
		}
		MultilingualString RequiredCartageEquipmentLCLHint
		{
			get { return ResString.GetMultilingualString("ad159eaa-289e-4718-ac15-6f333b96604b", "Select the default Port Transport equipment requirement for LCL freight. The list of LCL Port Transport equipment can be changed in the registry from the following location: {0}", new ModifiedMultilingualString(delegate { return ((IRegistryItemInternals)Env.Registry.RawRegistry.LCLAIREquipmentNeeded).Location; })); }
		}

		public CodePairRegistryItem RequiredCartageEquipmentLCL
		{
			get
			{
				return GetItem("RequiredCartageEquipmentLCL", delegate
				{
					CodeDescriptionPairList lCLList = new LCLAIREquipmentNeededList();
					return new CodePairRegistryItem(
						"RequiredCartageEquipmentLCL",
						Categories.Organizations_DefaultValues_RequiredPortTransportEquipment,
						RequiredCartageEquipmentLCLCaption,
						RequiredCartageEquipmentLCLHint,
						AirLCLListProvider,
						false,
						true,
						new ComboBoxRegistryEditorInfo(AirLCLListProvider, true),
						RegistryStorageFlags.All,
						RegistryOptions.PreserveTestValue,
						Constants.LCLAIREquipmentNeeded.Premise,
						false);
				});
			}
		}

		#endregion

		#endregion

		#region Bank Accounts Based On Currency

		public BankAccountBasedOnCurrencyRegistryItem BankAccountsBasedOnCurrency
		{
			get
			{
				return GetItem("BankAccountsBasedOnCurrency", delegate
				{
					return new BankAccountBasedOnCurrencyRegistryItem(
						"BankAccountsBasedOnCurrency",
						Categories.Organizations_DefaultValues,
						ResString.GetMultilingualString("16a5de78-e406-4d82-aa5d-2a57da80c3fe", "Bank Accounts for AR Documents and Receipting"),
						ResString.GetMultilingualString("eda6557d-aa85-433a-a882-ea097b043a2d", "This registry item allows you to define the default bank accounts based on currency. This registry item is used in the following locations:\r\n - Invoice Remittance section\r\n - Statement of Account Remittance section\r\n - Batch Invoice Remittance section\r\n - Default bank account when receipting for a particular customer"),
						RegistryStorageFlags.Company);
				});
			}
		}

		#endregion

		#region Attachment Type

		public CodePairRegistryItem DefaultAttachmentType
		{
			get
			{
				return GetItem("DefaultAttachmentType", delegate
				{
					return new CodePairRegistryItem(
						"DefaultAttachmentType",
						Categories.Organizations_DefaultValues,
						ResString.GetMultilingualString("D4AA8516-9668-465F-950B-1F5C0675671D", "Attachment Type for Contact"),
						ResString.GetMultilingualString("E830F82C-102A-4DE1-8086-409A30DEEFA2", "Specify the default attachment type for Contact"),
						new CodeDescriptionPairListProvider(() => ContactAttachmentTypeList.AttachmentType_List),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						OrgConstants.AttachmentType.PDF);
				});
			}
		}

		#endregion

		#endregion

		#region OrgMatchThreshold

		static MultilingualString MatchThresholdCaption
		{
			get { return ResString.GetMultilingualString("5CB09A80-770E-4BC5-A3D1-038E6B1E51A4", "Legacy Organization Match Threshold"); }
		}

		static MultilingualString MatchThresholdHint
		{
			get
			{
				return ResString.GetMultilingualString("5677FBD7-F101-4C28-9198-E1EFAAF2CFA5", @"This registry setting is related to legacy organization matching functionality. If ""Use Match Engine to Match Organization"" is set to Yes, this will not be used.
This value determines how aggressive the system should be in finding similar organization matches.

 - A setting of EXTREME will be the most scrutinizing, showing the fewest matches.
 - A setting of LOW will be the most relaxed showing more possible matches.
 - The default setting of MEDIUM is generally appropriate for most situations.");
			}
		}

		public CodePairRegistryItem OrgMatchThreshold
		{
			get
			{
				return GetItem("OrgMatchThreshold", delegate
				{
					var orgMatchThresholdsListProvider = new CodeDescriptionPairListProvider(() => new OrgMatchThresholds());

					return new CodePairRegistryItem(
						"OrgMatchThreshold",
						Categories.Organizations_PatternMatch,
						MatchThresholdCaption,
						MatchThresholdHint,
						orgMatchThresholdsListProvider,
						false,
						true,
						new ComboBoxRegistryEditorInfo(orgMatchThresholdsListProvider, true),
						RegistryStorageFlags.All,
						RegistryOptions.PreserveTestValue,
						OrgMatchThresholds.Codes.Medium,
						false);
				});
			}
		}

		#endregion

		#region Opportunity Management

		#region Opportunity Types and Stages

		public CodeDescriptionBoolRegistryItem OpportunitySalesTypes
		{
			get
			{
				return GetItem("OpportunitySalesTypes", delegate
				{
					var caption = ResString.GetMultilingualString("69fbc097-14e7-4d2c-b4fa-437a8455818c", "Sales Types");
					CodeDescriptionBoolCollection defaultValue = new CodeDescriptionBoolCollection(OrgOpportunitySchema.P8_OpportunityType.MaxLength);
					defaultValue.Add("UDF", ResString.GetMultilingualString("1d20f83a-daa9-4be6-bb9b-594cd323e563", "Undefined - You can modify this in the System Registry, under {0}/{1}", Categories.SalesMarketing_OpportunityManagement, caption));

					return new CodeDescriptionBoolRegistryItem(
						"OpportunitySalesTypes",
						Categories.SalesMarketing_OpportunityManagement,
						caption,
						ResString.GetMultilingualString("74d88dce-b2a9-47b9-9cde-64c6d59a3cf4", "A list of sale types for each Opportunity."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("94c8d660-c51d-46fa-9342-837f90fb68ae", "Enabled")),
						defaultValue);
				});
			}
		}

		public CodeDescriptionBoolRegistryItem OpportunityStages
		{
			get
			{
				return GetItem("OpportunityStages", delegate
				{
					var caption = ResString.GetMultilingualString("9cb60d11-73be-40a2-bf56-382524b31f63", "Stages");
					CodeDescriptionBoolCollection defaultValue = new CodeDescriptionBoolCollection(OrgOpportunitySchema.P8_Stage.MaxLength);
					defaultValue.Add("UDF", ResString.GetMultilingualString("1d20f83a-daa9-4be6-bb9b-594cd323e563", "Undefined - You can modify this in the System Registry, under {0}/{1}", Categories.SalesMarketing_OpportunityManagement, caption));

					return new CodeDescriptionBoolRegistryItem(
						"OpportunityStages",
						Categories.SalesMarketing_OpportunityManagement,
						caption,
						ResString.GetMultilingualString("e7b17fbc-ba77-46bc-a223-b852ec2b6399", "A list of stages for each Opportunity."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("94c8d660-c51d-46fa-9342-837f90fb68ae", "Enabled")),
						defaultValue);
				});
			}
		}

		#endregion

		#region Opportunity Status

		public OpportunityStatusRegistryItem OpportunityStatus
		{
			get
			{
				return GetItem("OpportunityManagementOpportunityStatus", delegate
				{
					var defaultValue = new OpportunityStatusCollection(defaultBoolForNewChild: false, defaultEffectiveAgreementForNewChild: false);
					defaultValue.Add("CRT", ResString.GetMultilingualString("32391244-2c12-44a5-97bf-c56c9a7abe6b", "Current"), effectiveAgreement: false, booleanValue: false, enabled: true, tradeStatus: OpportunityTradeStatus.Codes.Active);
					defaultValue.Add("LOS", ResString.GetMultilingualString("b9ba0ef1-449f-416d-9b1a-1e091a59ab1f", "Lost"), effectiveAgreement: false, booleanValue: true, enabled: true, tradeStatus: OpportunityTradeStatus.Codes.Unsuccessful);
					defaultValue.Add("ABA", ResString.GetMultilingualString("4b348cd8-a8d3-47a3-9a0a-fde9d92c0485", "Abandoned"), effectiveAgreement: false, booleanValue: true, enabled: true, tradeStatus: OpportunityTradeStatus.Codes.Unsuccessful);
					defaultValue.Add("SUS", ResString.GetMultilingualString("4f1db81d-7900-44c1-927f-e375201d3d41", "Suspended"), effectiveAgreement: false, booleanValue: false, enabled: true, tradeStatus: OpportunityTradeStatus.Codes.Active);
					defaultValue.Add("WON", ResString.GetMultilingualString("5e433745-a0fb-4b00-8359-38396edc7a27", "Won"), effectiveAgreement: true, booleanValue: true, enabled: true, tradeStatus: OpportunityTradeStatus.Codes.Successful);

					return new OpportunityStatusRegistryItem(
						"OpportunityManagementOpportunityStatus",
						Categories.SalesMarketing_OpportunityManagement,
						ResString.GetMultilingualString("cb61d112-f055-4189-9be5-b90fd19394ea", "Opportunity Status"),
						ResString.GetMultilingualString("5e7efbe0-312c-4c26-8444-284f23d60c05", "The different statuses that can apply to an Opportunity."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						defaultValue);
				});
			}
		}

		#endregion

		#region Opportunity Outcome

		public CodeDescriptionBoolRegistryItem OpportunityOutcome
		{
			get
			{
				return GetItem("OpportunityManagementOpportunityOutcome", delegate
				{
					CodeDescriptionBoolCollection defaultValue = new CodeDescriptionBoolCollection();
					var undefined = GetUndefinedCodeDescriptionPairList(ResString.GetMultilingualString("67ec1954-c6f8-4abb-b508-3651a9381deb", "{0}/Opportunity Outcome", Categories.SalesMarketing_OpportunityManagement))[0] as CodeDescriptionPair;
					defaultValue.Add(undefined.Code, undefined.MultilingualDescription);

					return new CodeDescriptionBoolRegistryItem(
						"OpportunityManagementOpportunityOutcome",
						Categories.SalesMarketing_OpportunityManagement,
						ResString.GetMultilingualString("1948c613-d43f-4357-abc2-15270554048d", "Opportunity Outcome"),
						ResString.GetMultilingualString("b7dbab8c-7ea3-4c0e-abb1-1d81c686a52d", "The possible outcomes of a Sales Opportunity."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("fd6f6bd8-6d15-48b4-be81-764538a9b5eb", "Enabled")),
						defaultValue);
				});
			}
		}

		#endregion

		#region Opportunity Source

		public CodeDescriptionBoolRelatedItemRegistryItem OpportunitySource
		{
			get
			{
				return GetItem("OpportunityManagementOpportunitySource", delegate
				{
					CodeDescriptionBoolRelatedItemCollection defaultList = new CodeDescriptionBoolRelatedItemCollection(OpportunitySourceRelatedItemProvider.SecondaryList, 5);
					defaultList.Add(Constants.Sales.LeadType.Website, ResString.GetMultilingualString("4779767e-5126-4dc7-aec6-14f2a90cf5bb", "Website"), true);
					defaultList.Add(Constants.Sales.LeadType.TelemarketingOrg, ResString.GetMultilingualString("fb90a251-c081-4d93-bec7-f98413b4e84c", "Telemarketing Organization"), true);
					defaultList.Add(Constants.Sales.LeadType.WordOfMouth, ResString.GetMultilingualString("e710250c-a80a-4dd4-9875-234eaa190d77", "Word Of Mouth"), true);
					defaultList.Add(Constants.Sales.LeadType.Other, ResString.GetMultilingualString("c044a558-687e-4b70-8e1c-de9568e3e874", "Other"), true);

					return new CodeDescriptionBoolRelatedItemRegistryItem(
						"OpportunityManagementOpportunitySource",
						Categories.SalesMarketing_OpportunityManagement,
						ResString.GetMultilingualString("fc90f259-c882-4067-8a0c-36b451e12c83", "Opportunity Source"),
						ResString.GetMultilingualString("da6dc931-2dc5-4e56-b675-04727174c334", "A list of sources that generated this Sales Opportunity."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						5,
						new CodeDescriptionBoolRelatedItemRegistryEditorInfo(ResString.GetMultilingualString("f2f31a5c-eadd-4bf8-9369-481ae42f76a2", "Enabled"), true),
						defaultList,
						OpportunitySourceRelatedItemProvider.SecondaryList);
				});
			}
		}

		#endregion

		#region Closed Opportunity Reasons

		public OpportunityClosedReasonsRegistryItem ClosedOpportunityReasons
		{
			get
			{
				return GetItem("ClosedOpportunityReasons", delegate
				{
					var defaultValue = new OpportunityClosedReasonsCollection
					{
						{ "PRI", ResString.GetMultilingualString("0f653aff-4a7d-44af-8d13-69e962a9b816", "Lost due to Price") },
						{ "SVC", ResString.GetMultilingualString("9e80741f-10bd-40c9-b90c-753f4d09f4d9", "Lost due to Service Offering") },
						{ "PRO", ResString.GetMultilingualString("7bff2303-ac28-4331-9d01-f5840910d548", "Lost due to Product Offering") }
					};

					return new OpportunityClosedReasonsRegistryItem(
						"ClosedOpportunityReasons",
						Categories.SalesMarketing_OpportunityManagement,
						ResString.GetMultilingualString("dfb17b38-d48e-4c3b-ae47-f11fc23cb7ba", "Closed Opportunity Reasons"),
						ResString.GetMultilingualString("8ae33bbc-fc0f-40a4-a4ec-d43b20e2b78b", "Listing of possible reasons a Sales Opportunity was closed."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						defaultValue);
				});
			}
		}

		#endregion

		#region Product Type

		#region Label

		public MultilingualStringRegistryItem ProductTypeLabel
		{
			get
			{
				return GetItem("ExtraCategoryLabel", delegate
				{
					return new MultilingualStringRegistryItem(
						"ExtraCategoryLabel",
						Categories.SalesMarketing_OpportunityManagement_ProductType,
						ResString.GetMultilingualString("a08438bc-e87c-469d-a9ed-df97df8de0b1", "Label"),
						ResString.GetMultilingualString("b0dae952-69ac-4704-9fff-290b0caad4d3", "Allows you to further categorize your opportunities, by a product type identifier."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						ResString.GetMultilingualString("bc42578a-911d-4d63-bd68-5983099ce5e6", "Product Type"));
				});
			}
		}

		#endregion

		#region List

		public CodeDescriptionBoolRegistryItem ProductTypeList
		{
			get
			{
				return GetItem("ExtraCategoryList", delegate
				{
					CodeDescriptionBoolCollection defaultValue = new CodeDescriptionBoolCollection(OrgOpportunitySchema.P8_PackageType.MaxLength);
					defaultValue.Add("STD", ResString.GetMultilingualString("ebd17965-bc60-400c-b9a2-5cf4209e9495", "You can change this list in the registry at {0}/List", Categories.SalesMarketing_OpportunityManagement_ProductType));

					return new CodeDescriptionBoolRegistryItem(
						"ExtraCategoryList",
						Categories.SalesMarketing_OpportunityManagement_ProductType,
						ResString.GetMultilingualString("d10e0e69-1b03-4898-824b-b69e109fe2ed", "List"),
						ResString.GetMultilingualString("7614d948-b1d1-4649-bc54-2a203a6a291f", "Allows you to nominate your product type list."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("f2f31a5c-eadd-4bf8-9369-481ae42f76a2", "Enabled")),
						defaultValue);
				});
			}
		}

		#endregion

		#endregion

		#region Customizable Labels

		public MultilingualStringRegistryItem PotentialLabel
		{
			get
			{
				const string name = "RentalMultiplierLabel";
				return GetItem(name, delegate
				{
					return new MultilingualStringRegistryItem(
						name,
						Categories.SalesMarketing_OpportunityManagement_CustomizableLabels,
						ResString.GetMultilingualString("56ee8ecd-06cd-4099-b856-8911426a190c", "Potential Label"),
						ResString.GetMultilingualString("4A2879CB-F7D0-4406-BCFF-F58AE94BBA39", "Allows you to further categorize your opportunities, by a Potential identifier with customizable label."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						OpportunityRegistryCaption.GetPotentialLabelCaption());
				});
			}
		}

		public MultilingualStringRegistryItem CurrentLabel
		{
			get
			{
				const string name = "TotalDiscountLabel";
				return GetItem(name, delegate
				{
					return new MultilingualStringRegistryItem(
						name,
						Categories.SalesMarketing_OpportunityManagement_CustomizableLabels,
						ResString.GetMultilingualString("4e80eb16-5f10-408b-b18b-e7e3eaf6e1e5", "Current Label"),
						ResString.GetMultilingualString("11d2f8c7-18cf-4fd1-a8e7-7203f550d973", "Allows you to further categorize your opportunities, by a Current identifier with customizable label."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						OpportunityRegistryCaption.GetCurrentLabelCaption());
				});
			}
		}

		#endregion

		#region Fields Mandatory

		public CodeDescriptionBoolDisallowNewRegistryItem OpportunityManagementFieldsMandatory
		{
			get
			{
				return GetItem("OpportunityManagementFieldsMandatory",
					delegate
					{
						var defaultValue = new CodeDescriptionBoolDisallowNewCollection();
						defaultValue.Add(50, OrgOpportunitySchema.Constants.P8_OpportunityDescription, ResString.GetMultilingualString("4d9ba3ee-c50d-4449-8334-ff894a31899d", "Description"), true);
						defaultValue.Add(50, OrgOpportunitySchema.Constants.P8_RX_NKEstimatedValueCurrency, ResString.GetMultilingualString("ab26005f-ceb8-4b53-8d95-3299b4a7e652", "Estimated Value Currency"), true);
						defaultValue.Add(50, OrgOpportunitySchema.Constants.P8_PackageType, ResString.GetMultilingualString("4caca684-6fd8-4d98-800c-c4e8dab4ba50", "Product Type"), false);
						defaultValue.Add(50, OrgOpportunitySchema.Constants.P8_Outcome, ResString.GetMultilingualString("adaa1166-2958-4c04-aece-f1a37b55ebb6", "Outcome"), false);
						defaultValue.Add(50, OrgOpportunitySchema.Constants.P8_Source, ResString.GetMultilingualString("11b5e448-f6ef-40fe-b57a-49adafe4f136", "Opportunity Source"), false);
						defaultValue.Add(50, OrgOpportunitySchema.Constants.P8_GS_NKPrimarySalesPerson, ResString.GetMultilingualString("1B658283-CAB3-4E20-B0EC-C195F859231A", "Sales Person"), false);

						return new CodeDescriptionBoolDisallowNewRegistryItem(
							"OpportunityManagementFieldsMandatory",
							Categories.SalesMarketing_OpportunityManagement,
							ResString.GetMultilingualString("ada49776-cb68-4450-8a4f-45cea9a866e3", "Mandatory Fields"),
							ResString.GetMultilingualString("db5d4270-1132-4495-bcb9-9e1438c5cc1b", "Specifies whether Opportunity fields are mandatory"),
							RegistryStorageFlags.System | RegistryStorageFlags.Company,
							new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("3fe4f7b4-b0fb-4259-b8c8-5f7ae79ebd20", "Is Mandatory"), true, true),
							defaultValue);
					});
			}
		}

		#endregion

		#region Divide Opportunity Value By 12

		public BooleanRegistryItem DivideAllOpportunityValuesByTwelveHasRun
		{
			get
			{
				return GetItem("DivideAllOpportunityValuesByTwelveHasRun", delegate
				{
					return new BooleanRegistryItem(
						"DivideAllOpportunityValuesByTwelveHasRun",
						Categories.SalesMarketing_OpportunityManagement,
						(NoResString)"Divide All Opportunity Values By Twelve Has Run",
						(NoResString)"A flag indicating whether the one-off procedure to divide all opportunity values by twelve has been run.",
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden | RegistryOptions.NotCached,
						false);
				});
			}
		}

		#endregion

		#endregion

		#region Sales Product

		public CodeDescriptionWithEnabledAndDefaultsRegistryItem PeriodOfActivityTypes
		{
			get
			{
				return GetItem("PeriodOfActivityTypes", delegate
				{
					var category = Categories.SalesMarketing_ClientIntelligence;
					var caption = ResString.GetMultilingualString("16cc7cf9-2f7a-4f78-8817-8c36ff4ac9f3", "Period of Activity Types");
					var defaultValue = new CodeDescriptionWithEnabledAndDefaultCollection(OrgMiscServSchema.OM_CMPeriodOfActivity.MaxLength);
					defaultValue.AddNew("ALL", ResString.GetMultilingualString("d6352c10-a0c9-4e3d-a3ff-aa2e87f0f627", "All Year Round"), true, true);
					defaultValue.AddNew("SNL", ResString.GetMultilingualString("147bce7e-db2b-4cdf-9cc1-67115dfd228f", "Seasonal"), false, true);

					return new CodeDescriptionWithEnabledAndDefaultsRegistryItem(
						"PeriodOfActivityTypes",
						category,
						caption,
						ResString.GetMultilingualString("870a0c3a-289a-434a-8406-3c68f643077d", "The list of period of activity types."),
						new CodeDescriptionWithEnabledAndDefaultsRegistryEditorInfo(),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue,
						false);
				});
			}
		}

		public CodeDescriptionBoolRegistryItem IndustryVerticalTypes
		{
			get
			{
				return GetItem("IndustryVerticalTypes", delegate
				{
					var category = Categories.SalesMarketing_ClientIntelligence;
					var caption = ResString.GetMultilingualString("e6cd4766-eb63-4d44-b4b5-5d56858fa0f2", "Vertical Market Types");
					var defaultValue = new CodeDescriptionBoolCollection(OrgMiscServSchema.OM_CMIndustryVertical.MaxLength);
					defaultValue.Add("AUTO", ResString.GetMultilingualString("6246fec1-4831-4268-ac73-6ee4d949bcdf", "Automotive"));
					defaultValue.Add("AERO", ResString.GetMultilingualString("6e8ec976-ddc5-4f03-a407-30c754c8f771", "Aerospace"));
					defaultValue.Add("DEF", ResString.GetMultilingualString("5a463395-d65b-46d1-be55-68296d581ff0", "Defense"));
					defaultValue.Add("TECH", ResString.GetMultilingualString("8e356397-0006-46a7-8b50-5d6224ae5a85", "Technology"));
					defaultValue.Add("INDUS", ResString.GetMultilingualString("b7f4c64a-a6a4-4256-88e8-a418596a0408", "Industrial"));
					defaultValue.Add("HLTH", ResString.GetMultilingualString("a4ef372a-9d5d-456f-8798-5dfaffb13eee", "Pharmaceuticals & Health-care"));
					defaultValue.Add("FMCG", ResString.GetMultilingualString("c5d0e945-7873-4829-b2a0-cb1f256a24b8", "Fast-moving consumer goods"));
					defaultValue.Add("LIFE", ResString.GetMultilingualString("a33c16bb-ed11-4117-8cd1-280436e2e2f4", "Fashion & Lifestyle"));
					defaultValue.Add("PE", ResString.GetMultilingualString("00ee48d3-facc-4dd4-8f31-52d20d31f31e", "Household & Personal effects"));

					return new CodeDescriptionBoolRegistryItem(
						"IndustryVerticalTypes",
						category,
						caption,
						ResString.GetMultilingualString("3260574e-0d57-4b38-96dd-fb9d53ca321b", "The list of vertical market types."),
						RegistryStorageFlags.System,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("94c8d660-c51d-46fa-9342-837f90fb68ae", "Enabled")),
						defaultValue);
				});
			}
		}

		#region Estimate Expiry Reason

		public CodeDescriptionBoolRegistryItem EstimateExpiryReasons
		{
			get
			{
				return GetItem("EstimateExpiryReasons", delegate
				{
					var defaultValue = new CodeDescriptionBoolCollection
					{
						{ "MAN", ResString.GetMultilingualString("A341E0D4-EA85-4520-A0BF-763AAC25357D", "Manually Expired") }
					};

					return new CodeDescriptionBoolRegistryItem(
						"EstimateExpiryReasons",
						Categories.SalesMarketing_ClientIntelligence,
						ResString.GetMultilingualString("047185FD-0F57-408C-92CD-10E578D6B15C", "Estimate Expiry Reasons"),
						ResString.GetMultilingualString("2C6DD5E1-15FA-46D2-A18D-85FD296E73CF", "Listing of possible reasons of why estimate value is expired."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("E4295712-CC19-4201-A2B5-51FF8D1C849E", "Enabled")),
						defaultValue);
				});
			}
		}

		#endregion

		#endregion

		#region GLOW

		#region Enable Glow Sales & Marketing Registry Settings

		public BooleanRegistryItem EnableGlowSalesMarketingRegistrySettings
		{
			get => GetItem("EnableGlowSalesMarketingRegistrySettings", () => new BooleanRegistryItem(
				"EnableGlowSalesMarketingRegistrySettings",
				Categories.SalesMarketing,
				(NoResString)"Enable GLOW registry settings",
				(NoResString)"When enabled, the GLOW registry settings appear under Sales & Marketing.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false
				));
		}

		#endregion

		#region Communication Management

		#region Mandatory Fields

		public CodeDescriptionBoolDisallowNewRegistryItem GlowCommunicationFieldsMandatory
		{
			get
			{
				return GetItem("GlowCommunicationFieldsMandatory",
					delegate
					{
						var defaultValue = new CodeDescriptionBoolDisallowNewCollection() { CodeMaxLength = 50 };
						defaultValue.AddSystemDefined(OrgSalesCallSchema.Constants.OQ_CallDate, ResString.GetMultilingualString("d8486411-2ba6-4e83-ae52-8a443cccaa08", "Date of activity"), true);
						defaultValue.AddSystemDefined(OrgSalesCallSchema.Constants.OQ_CallSummary, ResString.GetMultilingualString("4748233e-8e77-4c92-9805-42599634a6c0", "Subject"), true);
						defaultValue.Add(OrgSalesCallSchema.Constants.OQ_Status, ResString.GetMultilingualString("c283203c-4ea0-43fb-ba59-91794b67ecb0", "Status"), false);
						defaultValue.Add(OrgSalesCallSchema.Constants.OQ_TypeOfCall, ResString.GetMultilingualString("2b5ee51e-2d7b-4247-9786-bf7ffc41db00", "Method"), false);
						defaultValue.Add(OrgSalesCallSchema.Constants.OQ_Category, ResString.GetMultilingualString("ae5e5133-da96-4101-9a26-f1c98a151441", "Purpose"), false);

						return new CodeDescriptionBoolDisallowNewRegistryItem(
							"GlowCommunicationFieldsMandatory",
							Categories.SalesMarketing_Glow_CommunicationManagement,
							ResString.GetMultilingualString("0a1a21ce-5c04-4bd6-a674-0608914026b2", "Mandatory Fields"),
							ResString.GetMultilingualString("478d7d70-c81f-4373-939c-727943aa16e8", "Specifies whether Communication fields are mandatory."),
							RegistryStorageFlags.System,
							EnableGlowSalesMarketingRegistrySettings.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
							new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("7fc5b108-d355-4fff-834c-e07fee7b5ccc", "Is Mandatory"), true, true),
							defaultValue);
					});
			}
		}

		#endregion

		#region Method

		public CodeDescriptionBoolRegistryItem GlowCommunicationMethod
		{
			get
			{
				return GetItem("GlowCommunicationMethod", delegate
				{
					var defaultValue = new OverrideImmuneCodeDescriptionBoolWithMandatoryDescriptionCollection(OrgSalesCallSchema.OQ_TypeOfCall.MaxLength);
					defaultValue.AddSystemDefined("PHN", ResString.GetMultilingualString("9e3e14db-fbfc-4590-821b-093dcce4447e", "Phone call"), true);
					defaultValue.AddSystemDefined("MTG", ResString.GetMultilingualString("10b0a9cb-ebda-415a-a72d-c0ea1cf12bc6", "Meeting"), true);
					defaultValue.AddSystemDefined("EML", ResString.GetMultilingualString("b0bf829c-1fd8-45ac-b711-ab904f160c96", "Email"), true);

					return new CodeDescriptionBoolRegistryItem(
						"GlowCommunicationMethod",
						Categories.SalesMarketing_Glow_CommunicationManagement,
						ResString.GetMultilingualString("0be9c823-2c63-44fe-b095-72d7cb607870", "Method"),
						ResString.GetMultilingualString("fe0d7720-51cd-4e0c-99ec-acfa2a044745", "The list of valid entries for communication method."),
						RegistryStorageFlags.System,
						EnableGlowSalesMarketingRegistrySettings.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("0e06829d-1c0e-4cdf-bab8-25cc82b01d63", "Enabled")),
						defaultValue);
				});
			}
		}

		#endregion

		#region Purpose

		public CodeDescriptionBoolRegistryItem GlowCommunicationPurpose
		{
			get
			{
				return GetItem("GlowCommunicationPurpose", delegate
				{
					var defaultValue = new CodeDescriptionBoolWithMandatoryDescriptionCollection(OrgSalesCallSchema.OQ_Category.MaxLength);
					defaultValue.Add("FOL", ResString.GetMultilingualString("9b7dcf95-2417-435d-9144-cf27534a7a30", "Follow-up"), true);
					defaultValue.Add("INI", ResString.GetMultilingualString("83545341-88e8-4367-94d0-c0a6a3be2f3e", "Initial contact"), true);

					return new CodeDescriptionBoolRegistryItem(
						"GlowCommunicationPurpose",
						Categories.SalesMarketing_Glow_CommunicationManagement,
						ResString.GetMultilingualString("afdd9fb8-1fbe-4d90-b775-999682b58686", "Purpose"),
						ResString.GetMultilingualString("59ad85f6-ce24-4068-bf7f-e34306eb90ac", "Allows you to nominate a purpose."),
						RegistryStorageFlags.System,
						EnableGlowSalesMarketingRegistrySettings.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("019ac8d4-baa2-4b55-b2df-0941d047d40f", "Enabled")),
						defaultValue);
				});
			}
		}

		#endregion

		#endregion

		#region Opportunity Management

		#region Contact Roles

		public CodeDescriptionWithMandatoryDescriptionRegistryItem GlowOpportunityContactRoles
		{
			get
			{
				return GetItem("GlowOpportunityContactRoles", delegate
				{
					var contactRolesList = new CodeDescriptionWithMandatoryDescriptionCollection()
					{
						{ "DCM", ResString.GetMultilingualString("e4724402-554e-469b-aab8-6199daccfd5b", "Decision Maker") }
					};

					return new CodeDescriptionWithMandatoryDescriptionRegistryItem(
						"GlowOpportunityContactRoles",
						Categories.SalesMarketing_Glow_OpportunityManagement,
						ResString.GetMultilingualString("9248f383-b5dd-4686-ba88-1626afdbf639", "Contact Roles"),
						ResString.GetMultilingualString("8f5856a7-0921-4c72-909b-eced99900391", "List of valid contact roles in Web Portal."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						EnableGlowSalesMarketingRegistrySettings.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						contactRolesList);
				});
			}
		}

		#endregion

		#region Lead Source

		public CodeDescriptionBoolRegistryItem GlowOpportunityLeadSource
		{
			get
			{
				return GetItem("GlowOpportunityLeadSource", delegate
				{
					var defaultValue = new CodeDescriptionBoolWithMandatoryDescriptionCollection(OrgOpportunitySchema.P8_Source.MaxLength);
					defaultValue.Add(Constants.Sales.LeadType.Website, ResString.GetMultilingualString("4779767e-5126-4dc7-aec6-14f2a90cf5bb", "Website"), true);
					defaultValue.Add(Constants.Sales.LeadType.TelemarketingOrg, ResString.GetMultilingualString("fb90a251-c081-4d93-bec7-f98413b4e84c", "Telemarketing Organization"), true);
					defaultValue.Add(Constants.Sales.LeadType.WordOfMouth, ResString.GetMultilingualString("e710250c-a80a-4dd4-9875-234eaa190d77", "Word Of Mouth"), true);
					defaultValue.Add(Constants.Sales.LeadType.Other, ResString.GetMultilingualString("c044a558-687e-4b70-8e1c-de9568e3e874", "Other"), true);

					return new CodeDescriptionBoolRegistryItem(
						"GlowOpportunityLeadSource",
						Categories.SalesMarketing_Glow_OpportunityManagement,
						ResString.GetMultilingualString("ec013403-d3e2-4a45-a026-674d076df5bd", "Lead Source"),
						ResString.GetMultilingualString("82afba60-48d2-42d2-9614-eed4630e4c63", "A list of sources that generated this Sales Opportunity."),
						new GlowOpportunityLeadSourceRegistryDataType(),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						EnableGlowSalesMarketingRegistrySettings.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("f2f31a5c-eadd-4bf8-9369-481ae42f76a2", "Enabled"), true),
						defaultValue);
				});
			}
		}

		#endregion

		#region Mandatory Fields

		public CodeDescriptionBoolDisallowNewRegistryItem GlowOpportunityFieldsMandatory
		{
			get
			{
				return GetItem("GlowOpportunityFieldsMandatory",
					delegate
					{
						var defaultValue = new CodeDescriptionBoolDisallowNewCollection() { CodeMaxLength = 50 };
						defaultValue.AddSystemDefined(CrmOpportunitySchema.Constants.COP_OH_Organization, ResString.GetMultilingualString("3174429e-9dd8-4142-98c6-ba4687a284a0", "Organization"), true);
						defaultValue.AddSystemDefined(CrmOpportunitySchema.Constants.COP_OpportunityName, ResString.GetMultilingualString("4102e09f-94e0-4b99-b48f-b3aed5d64595", "Opportunity name"), true);
						defaultValue.Add(CrmOpportunitySchema.Constants.COP_ProductType, ResString.GetMultilingualString("cd1ba07b-e11e-4b3a-8230-9f0200334687", "Product type"), false);
						defaultValue.Add(CrmOpportunitySchema.Constants.COP_GS_NKSalesPerson, ResString.GetMultilingualString("946ddcac-9b43-414d-8ba0-78bc513f7582", "Sales person"), false);
						defaultValue.Add(CrmOpportunitySchema.Constants.COP_SalesType, ResString.GetMultilingualString("3d60903c-2330-48d1-afd2-264045c9095c", "Sales type"), false);
						defaultValue.Add(CrmOpportunitySchema.Constants.COP_SourceType, ResString.GetMultilingualString("9e2f9776-1f66-4a19-acd9-fe1836ca0b4d", "Source type"), false);
						defaultValue.AddSystemDefined(CrmOpportunitySchema.Constants.COP_Stage, ResString.GetMultilingualString("51b92aa1-782f-4bc2-bdb5-dddcd4db78de", "Stage"), true);
						defaultValue.Add(CrmOpportunitySchema.Constants.COP_Status, ResString.GetMultilingualString("7520ea81-f1cb-4138-b640-1e4346dd7b76", "Status"), false);

						return new CodeDescriptionBoolDisallowNewRegistryItem(
							"GlowOpportunityFieldsMandatory",
							Categories.SalesMarketing_Glow_OpportunityManagement,
							ResString.GetMultilingualString("f3e7ce83-cf8f-4b34-a4e9-4854ab70c5a1", "Mandatory Fields"),
							ResString.GetMultilingualString("2f0dc539-b1df-4ef9-af27-61cd7deda5e2", "Specifies whether Opportunity fields are mandatory."),
							RegistryStorageFlags.System | RegistryStorageFlags.Company,
							(EnableGlowSalesMarketingRegistrySettings.Value) ? RegistryOptions.Default : RegistryOptions.IsHidden,
							new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("3fe4f7b4-b0fb-4259-b8c8-5f7ae79ebd20", "Is Mandatory"), true, true),
							defaultValue);
					});
			}
		}

		#endregion

		#region Product type

		public CodeDescriptionBoolRegistryItem GlowOpportunityProductType
		{
			get
			{
				return GetItem("GlowOpportunityProductType", delegate
				{
					var defaultValue = new CodeDescriptionBoolWithMandatoryDescriptionCollection(OrgOpportunitySchema.P8_PackageType.MaxLength);
					defaultValue.Add("FWD", ResString.GetMultilingualString("63A10A7B-E717-4AF4-BE64-65B06B354798", "Forwarding"), true);

					return new CodeDescriptionBoolRegistryItem(
						"GlowOpportunityProductType",
						Categories.SalesMarketing_Glow_OpportunityManagement,
						ResString.GetMultilingualString("6e647f41-00a5-4400-8050-8d64c0968639", "Product Type"),
						ResString.GetMultilingualString("c66f646f-324e-4002-8d4e-65cda1188913", "Allows you to nominate your product type list."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						EnableGlowSalesMarketingRegistrySettings.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("c7650708-0f1b-4975-aaaa-5d848d321719", "Enabled")),
						defaultValue);
				});
			}
		}

		#endregion

		#region Sales Types

		public CodeDescriptionBoolRegistryItem GlowOpportunitySalesTypes
		{
			get
			{
				return GetItem("GlowOpportunitySalesTypes", delegate
				{
					var defaultValue = new CodeDescriptionBoolWithMandatoryDescriptionCollection(OrgOpportunitySchema.P8_OpportunityType.MaxLength);
					defaultValue.Add("NEW", ResString.GetMultilingualString("6D3AF019-44F4-46C0-B897-99AD19B231F2", "New Business"), true);
					defaultValue.Add("EXS", ResString.GetMultilingualString("B7F184DD-A3B6-45A6-9AFA-F8C75A297688", "Develop existing business"), true);

					return new CodeDescriptionBoolRegistryItem(
						"GlowOpportunitySalesTypes",
						Categories.SalesMarketing_Glow_OpportunityManagement,
						ResString.GetMultilingualString("0bc0dd49-c2a1-4b4a-ad68-f11ef433a928", "Sales Types"),
						ResString.GetMultilingualString("88ae2186-daba-4db3-9de0-1246853d1868", "A list of sale types for each Opportunity."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						EnableGlowSalesMarketingRegistrySettings.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("94c8d660-c51d-46fa-9342-837f90fb68ae", "Enabled")),
						defaultValue);
				});
			}
		}

		#endregion

		#region Opportunity Status

		public GlowOpportunityStatusRegistryItem GlowOpportunityStatuses
		{
			get
			{
				return GetItem("GlowOpportunityStatuses", delegate
				{
					var defaultValues = new GlowOpportunityStatusCollection();
					defaultValues.Add("CRT", ResString.GetMultilingualString("32391244-2c12-44a5-97bf-c56c9a7abe6b", "Current"), enabled: true, tradeStatus: OpportunityTradeStatus.Codes.Active);
					defaultValues.Add("LOS", ResString.GetMultilingualString("b9ba0ef1-449f-416d-9b1a-1e091a59ab1f", "Lost"), enabled: true, tradeStatus: OpportunityTradeStatus.Codes.Unsuccessful);
					defaultValues.Add("ABA", ResString.GetMultilingualString("4b348cd8-a8d3-47a3-9a0a-fde9d92c0485", "Abandoned"), enabled: true, tradeStatus: OpportunityTradeStatus.Codes.Unsuccessful);
					defaultValues.Add("SUS", ResString.GetMultilingualString("4f1db81d-7900-44c1-927f-e375201d3d41", "Suspended"), enabled: true, tradeStatus: OpportunityTradeStatus.Codes.Active);
					defaultValues.Add("WON", ResString.GetMultilingualString("5e433745-a0fb-4b00-8359-38396edc7a27", "Won"), enabled: true, tradeStatus: OpportunityTradeStatus.Codes.Successful);

					return new GlowOpportunityStatusRegistryItem(
						"GlowOpportunityStatuses",
						Categories.SalesMarketing_Glow_OpportunityManagement,
						ResString.GetMultilingualString("82eb837c-9147-4786-b99e-05ecce6e7928", "Opportunity Status"),
						ResString.GetMultilingualString("df35c2f1-7237-4e9a-b6d1-99e38f719e7a", "The different statuses that can apply to an Opportunity."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						EnableGlowSalesMarketingRegistrySettings.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						defaultValues);
				});
			}
		}

		#endregion

		#region Opportunity Stages

		public GlowOpportunityStageRegistryItem GlowOpportunityStages
		{
			get
			{
				return GetItem("GlowOpportunityStages", delegate
				{
					var caption = ResString.GetMultilingualString("927c6fbf-0af4-4953-a6e3-a9977f31f433", "Stages");
					var defaultValues = new GlowOpportunityStageCollection();
					defaultValues.Add("NEW", ResString.GetMultilingualString("921657ff-1d28-490e-b8ad-a4ac24ad1c62", "New", 0));

					return new GlowOpportunityStageRegistryItem(
						"GlowOpportunityStages",
						Categories.SalesMarketing_Glow_OpportunityManagement,
						caption,
						ResString.GetMultilingualString("6c380ff4-2d56-4354-85de-626e893a4f81", @"A sequence of stages for each opportunity and its nominated Winning probability.
Acceptable values for Win Probabilities are integers between 0 and 100."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						(EnableGlowSalesMarketingRegistrySettings.Value) ? RegistryOptions.Default : RegistryOptions.IsHidden,
						defaultValues);
				});
			}
		}

		#endregion

		#region Allow Restricted Opportunities

		public BooleanRegistryItem AllowRestrictedOpportunities
		{
			get => GetItem("AllowRestrictedOpportunities", () => new BooleanRegistryItem(
				"AllowRestrictedOpportunities",
				OrganisationsDataRegistry.Categories.SalesMarketing_Glow_OpportunityManagement,
				(NoResString)"Allow Restricted Opportunities",
				(NoResString)@"When enabled, this allows users to create opportunities that can be accessed only by specific staff as defined within the opportunity.

If this setting is disabled later, such restricted opportunities can no longer be created, however the ones that were created previously shall continue to remain restricted. Users that had access to specific restricted opportunities can open them and remove restrictions if so desired.",
				RegistryStorageFlags.System,
				EnableGlowSalesMarketingRegistrySettings.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
				false
				));
		}

		#endregion

		#region Opportunity Scope

		#region Configurable Parameters

		#region Forwarding

		public CodeDescriptionBoolDisallowNewRegistryItem OpportunityScopeForwardingParameters
		{
			get
			{
				return GetItem("OpportunityScopeForwardingParameters",
					delegate
					{
						var defaultValue = new CodeDescriptionBoolDisallowNewCollection
						{
							{ 50, (NoResString)"PickupDeliveryAddress", ResString.GetMultilingualString("D21285EB-8033-4FED-8AA6-CC201A1CFD5C", "Pickup & Delivery address"), true },
							{ 50, (NoResString)"Container", ResString.GetMultilingualString("06E9BD1C-3977-4CA9-A0C9-1928F48AC689", "Container / ULD"), false },
							{ 50, (NoResString)"Commodity", ResString.GetMultilingualString("B299D329-4893-431B-B048-9FCB70095C0B", "Commodity"), false },
							{ 50, (NoResString)"Incoterm", ResString.GetMultilingualString("4FCA8F2D-0EEB-48AA-98A0-33B0214F8980", "Incoterm"), false },
							{ 50, (NoResString)"ServiceLevel", ResString.GetMultilingualString("8445FD05-5566-46B8-BD8C-B7B42CE557AA", "Service level"), false }
						};

						return new CodeDescriptionBoolDisallowNewRegistryItem(
							"OpportunityScopeForwardingParameters",
							Categories.SalesMarketing_Glow_OpportunityManagement_OppScopeConfigurableParameters_ConfigurableParameters,
							ResString.GetMultilingualString("925082CC-6246-451E-9668-04AAFFAB0DA2", "Forwarding"),
							ResString.GetMultilingualString("3DB91B00-D00C-41B3-A526-94BEBDF3C3D3", "This is a list of additional parameters that can be added to a Forwarding scope."),
							RegistryStorageFlags.System | RegistryStorageFlags.Company,
							(EnableGlowSalesMarketingRegistrySettings.Value) ? RegistryOptions.Default : RegistryOptions.IsHidden,
							new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("8F6E3BB4-D07B-4818-A9C5-E537061358C6", "Enabled"), true, true),
							defaultValue);
					});
			}
		}

		#endregion

		#endregion

		#region Priority Sequence

		#region Forwarding

		public GlowOpportunityScopePrioritySequenceRegistryItem OpportunityScopeForwardingPrioritySequence
		{
			get
			{
				return GetItem("OpportunityScopeForwardingPrioritySequence", delegate
				{
					var caption = ResString.GetMultilingualString("925082CC-6246-451E-9668-04AAFFAB0DA2", "Forwarding");
					var defaultValue = new GlowOpportunityScopePrioritySequenceCollection();
					defaultValue.AddNew(ResString.GetMultilingualString("B299D329-4893-431B-B048-9FCB70095C0B", "Commodity"));
					defaultValue.AddNew(ResString.GetMultilingualString("8445FD05-5566-46B8-BD8C-B7B42CE557AA", "Service level"));
					defaultValue.AddNew(ResString.GetMultilingualString("4FCA8F2D-0EEB-48AA-98A0-33B0214F8980", "Incoterm"));
					defaultValue.AddNew(ResString.GetMultilingualString("06E9BD1C-3977-4CA9-A0C9-1928F48AC689", "Container / ULD"));

					return new GlowOpportunityScopePrioritySequenceRegistryItem(
						"OpportunityScopeForwardingPrioritySequence",
						Categories.SalesMarketing_Glow_OpportunityManagement_OppScopeConfigurableParameters_PrioritySequence,
						caption,
						ResString.GetMultilingualString("2730E7C8-85D7-43EC-A6C3-1C0C4195BD5E", "This is the priority sequence in which the additional scoping parameters on Forwarding opportunity scopes are used to link quotes and jobs. When you alter the sequence the same shall take effect from subsequent linkages."),
						RegistryStorageFlags.System,
						(EnableGlowSalesMarketingRegistrySettings.Value) ? RegistryOptions.Default : RegistryOptions.IsHidden,
						defaultValue);
				});
			}
		}

		#endregion

		#endregion

		#endregion

		#region Auto-linking opportunity and quote scopes

		public BooleanRegistryItem EnableAutoLinkingOpportunityAndQuoteScopes
		{
			get => GetItem("EnableAutoLinkingOpportunityAndQuoteScopes", () => new BooleanRegistryItem(
				"EnableAutoLinkingOpportunityAndQuoteScopes",
				Categories.SalesMarketing_Glow_OpportunityManagement,
				ResString.GetMultilingualString("6b8b0df4-4a31-4f68-92cf-6580f0f5b3c7", "Auto-linking opportunity and quote scopes"),
				ResString.GetMultilingualString("51c81f70-ad54-4176-9523-788d8f8f2e8e", "When set to 'Yes' the system attempts to automatically link quote scopes to relevant opportunity scopes. Automatic linking is disabled when this setting is 'No'. Manual linking and unlinking is supported at all times."),
				RegistryStorageFlags.System,
				EnableGlowSalesMarketingRegistrySettings.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
				true
				));
		}

		#endregion

		#endregion

		#region Temporary Organisation

		public BooleanRegistryItem EnableCreationOfTemporaryOrganizations
		{
			get => GetItem("EnableCreationOfTemporaryOrganizations", () => new BooleanRegistryItem(
				"EnableCreationOfTemporaryOrganizations",
				OrganisationsDataRegistry.Categories.SalesMarketing_Glow_OpportunityManagement,
				(NoResString)"Enable Creation of Temporary Organizations",
				(NoResString)"When enabled, GLOW users are allowed to create temporary organizations.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				EnableGlowSalesMarketingRegistrySettings.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
				true
			));
		}

		public GlowTempOrgRegistryItem TempOrganisationRequiredFields
		{
			get
			{
				return GetItem("TempOrganisationRequiredFields",
					delegate
					{
						var defaultValue = new GlowTempOrgRequiredFieldCollection(10);
						defaultValue.AddSystemDefined((NoResString)"Name", ResString.GetMultilingualString("52085C33-53AA-4F6B-8CBF-D36576E4AFC1", "Name"), true, true);
						defaultValue.AddSystemDefined("Address1", ResString.GetMultilingualString("a506de0a-af8f-4da1-a9e6-518b05b54e84", "Address 1"), false, false);
						defaultValue.AddSystemDefined("Address2", ResString.GetMultilingualString("c56f3a51-9ede-4045-8095-5fca138a39f2", "Address 2"), false, false);
						defaultValue.AddSystemDefined((NoResString)"Country", ResString.GetMultilingualString("da389b33-751e-458c-ae2a-fc08f3d68b85", "Country/Region"), false, false);
						defaultValue.AddSystemDefined("UNLOCO", ResString.GetMultilingualString("66e8b9cc-ccf5-45f7-99da-52222f2ef838", "UNLOCO"), false, false);
						defaultValue.AddSystemDefined((NoResString)"City", ResString.GetMultilingualString("a30ddcc4-8c21-4396-8278-989c808a654e", "City"), false, false);
						defaultValue.AddSystemDefined((NoResString)"Postcode", ResString.GetMultilingualString("aa527797-4ae0-4e77-b0cd-f62a640ecb7c", "Postcode"), false, false);
						defaultValue.AddSystemDefined((NoResString)"State", ResString.GetMultilingualString("7267acfc-f447-4be0-9450-c3879fbc9023", "State"), false, false);
						defaultValue.AddSystemDefined((NoResString)"Branch", ResString.GetMultilingualString("d06b17ff-d290-41a6-8813-a19bc015b0db", "Branch"), false, false);
						defaultValue.AddSystemDefined((NoResString)"Phone", ResString.GetMultilingualString("eefea28f-e9a0-4a76-ad11-7b486568a6a2", "Phone"), false, false);
						defaultValue.AddSystemDefined((NoResString)"Mobile", ResString.GetMultilingualString("0e6a1eaf-da3e-404a-a11c-0d11c28dba6b", "Mobile"), false, false);
						defaultValue.AddSystemDefined((NoResString)"Email", ResString.GetMultilingualString("dbca8d61-8a78-4c9e-81de-9dd5639df5e7", "Email"), false, false);
						defaultValue.AddSystemDefined((NoResString)"Fax", ResString.GetMultilingualString("5b5b0e3c-6aee-446f-b16a-eb15227f3ca4", "Fax"), false, false);
						defaultValue.AddSystemDefined((NoResString)"Web", ResString.GetMultilingualString("2df45a1b-ed4b-4a20-8e55-4c58ecde6299", "Website URL"), false, false);

						return new GlowTempOrgRegistryItem(
							"TempOrganisationRequiredFields",
							Categories.SalesMarketing_Glow,
							ResString.GetMultilingualString("faf30ad4-0938-44c4-b150-2c5cfdcb8669", "Temporary Organization Required Fields"),
							ResString.GetMultilingualString("3ceb54df-ec7e-450e-9140-4b9b1f8a4153", "Required fields for temporary organizations."),
							RegistryStorageFlags.System,
							(EnableGlowSalesMarketingRegistrySettings.Value) ? RegistryOptions.Default : RegistryOptions.IsHidden,
							defaultValue
							);
					});
			}
		}

		#endregion

		#endregion

		#endregion

		#region Rating

		#region Rate Group Security
		#region SuppressResourceStringsCheckRegion

		public CodeDescriptionPairListRegistryItem RatesSecurity
		{
			get
			{
				return GetItem("RatesSecurity", delegate
				{
					var ratesSecurityList = new CodeDescriptionPairList();
					var hint = ResString.GetMultilingualString("D531EE8F-17FC-4921-9E62-219FE4C67097", "Override this registry to create security settings that will control staff’s access rights to the sell and buy rates of the organizations allocated.");

					return new CodeDescriptionPairListRegistryItem(
							"RatesSecurity",
							Categories.Organizations_Rating,
							ResString.GetMultilingualString("97CFCA46-D6C7-434D-8B01-2AA3CC79F4AD", "Rates' Security"),
							hint,
							3,
							RegistryStorageFlags.System | RegistryStorageFlags.Company,
							ratesSecurityList);
				});
			}
		}

		#endregion
		#endregion

		#region Fees and Charges - Levels

		public FeeChargeLevelsRegistryItem RateFeeChargeLevels
		{
			get
			{
				var hint = ResString.GetMultilingualString("23f2038d-51b2-4457-9372-a8105c6bbdc1", "Create a list that defines the types of services offered to customers as \r\n\r\nvalue added services and nominate the levels and default values applicable to each service type.");
				var captions = ResString.GetMultilingualString("d9345c52-b41b-4635-8353-1931caa42519", "Fees and Charges - Levels");

				return GetItem("RateFeeChargeLevels", delegate
				{
					return new FeeChargeLevelsRegistryItem(
							"RateFeeChargeLevels",
							Categories.Organizations_Rating,
							captions,
							hint,
							RegistryStorageFlags.System | RegistryStorageFlags.Company,
							FeeChargeLevelsSection.GetDefault());
				});
			}
		}

		#endregion

		#endregion

		#region Use Unmatched Organisation for Matching

		public SystemDefinedOrganisationRegistryItem UseUnmatchedOrganisationForMatching
		{
			get
			{
				return GetItem("UseUnmatchedOrganisationForMatching", delegate
				{
					return new SystemDefinedOrganisationRegistryItem(
						"UseUnmatchedOrganisationForMatching",
						Categories.Organizations_PatternMatch,
						ResString.GetMultilingualString("77eab892-7dcd-46a9-87ce-3a95da80b47f", "Use Unmatched Organization for Matching"),
						ResString.GetMultilingualString("04392b70-405c-4f54-9129-20579f33a7c9", "When enabled, this option will allow jobs to be assigned to a special \"default\" (UNMATCHED) organization in the system, if a Consignee, Consignor, Broker, etc cannot be found during a data import.\r\nNOTE: Specific module configuration for UXML imports can be found in the \"Unmatched Organization Configuration\" registry. The control of these modules is separate to this registry."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						typeof(UnmatchedOrganisation));
				});
			}
		}

		public static class JobTypeCodes
		{
			public const string Order = "ORD";
		}

		public CodeDescriptionBoolRegistryItem UnmatchedOrganisationConfiguration
		{
			get
			{
				return GetItem("UnmatchedOrganisationConfiguration", () => new CodeDescriptionBoolRegistryItem(
					"UnmatchedOrganisationConfiguration",
					Categories.Organizations_PatternMatch,
					ResString.GetMultilingualString("BC485EBC-81C6-4D79-A878-8DABC18E82BA",
						"Unmatched Organization Configuration"),
					ResString.GetMultilingualString("6E0F7D60-FA52-4AA1-A7BF-F0DA5DEFF263",
						"Allows the unmatched organization for UXML import to be switched on/off for certain modules."),
					RegistryStorageFlags.System,
					RegistryOptions.PreserveTestValue,
					new CodeDescriptionBoolRegistryEditorInfo(
						ResString.GetMultilingualString("11AEA5B5-B738-4BAE-B273-DBBEF18158DB",
							"Enable Unmatched Organization"), true, true),
					JobTypesDefaultValue));
			}
		}

		public static CodeDescriptionBoolCollection JobTypesDefaultValue = new CodeDescriptionBoolCollection
		{
			{ JobTypeCodes.Order, ResString.GetMultilingualString("70F62F1B-2BB4-4E3D-85A1-5EEA97417F2A", "Order (Forwarding)"), false }
		};

		#endregion

		#region Deduplication

		public BooleanRegistryItem EnableDeduplicationFinder
		{
			get
			{
				return GetItem("EnableDeduplicationFinder", delegate
				{
					return new BooleanRegistryItem(
						"EnableDeduplicationFinder",
						Categories.Organizations_Duplicate_Detection,
						ResString.GetMultilingualString("fca222cc-4279-e2a8-4d1b-d3b0931b188f", "Enable Duplicate Detection"),
						ResString.GetMultilingualString("b9a5c28a-fa02-4abb-4e91-53d446cda47c", "When this registry is set to 'Yes', the system will show a warning when it detects that a potential duplicate is being added."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public IntRegistryItem DuplicateDetectionTimeout
		{
			get
			{
				return GetItem("DuplicateDetectionTimeout", delegate
				{
					const int defaultValue = 10;
					const int minValue = 1;
					const int maxValue = 300;

					return new IntRegistryItem(
						"DuplicateDetectionTimeout",
						Categories.Organizations_DuplicateDetection_Configuration_OrganisationForm,
						ResString.GetMultilingualString("e3a398eb-64d1-42f9-bb96-d6517941bb6e", "Duplicate Detection Timeout"),
						ResString.GetMultilingualString("c32c082b-d03c-438c-a951-1395efece765", "Time in seconds representing timeout for duplicate detection (from 1 to 300)."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue,
						minValue,
						maxValue);
				});
			}
		}

		public IntRegistryItem MaximumPotentialTargets
		{
			get
			{
				return GetItem("MaximumPotentialTargets", delegate
				{
					const int defaultValue = 50;
					const int minValue = 10;
					const int maxValue = 200;

					return new IntRegistryItem(
						"MaximumPotentialTargets",
						Categories.Organizations_DuplicateDetection_Configuration_Global,
						ResString.GetMultilingualString("9e923f54-65cc-4ed3-b7f0-0cbe5abe325b", "Maximum Potential Targets"),
						ResString.GetMultilingualString("55ec9dee-5438-43a4-9432-bad6c1da0892", "The maximum number of potential targets that are loaded during duplicate detection (from 10 to 200)."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue,
						minValue,
						maxValue);
				});
			}
		}

		public IntRegistryItem QueueProcessingBatchSize
		{
			get
			{
				return GetItem("QueueProcessingBatchSize", delegate
				{
					const int defaultValue = 50;
					const int minValue = 2;
					const int maxValue = 200;

					return new IntRegistryItem(
						"QueueProcessingBatchSize",
						Categories.Organizations_Duplicate_Detection,
						ResString.GetMultilingualString("2FE81B03-D743-4DA0-8B9D-D4C50CF4FEC4", "Queue Processing Batch Size"),
						ResString.GetMultilingualString("B8664D08-5D21-44BD-BB23-A5FE83AEC920", "The batch size for De-duplication Queue Processing (DQP) service task."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue,
						minValue,
						maxValue);
				});
			}
		}

		public CodePairRegistryItem DeduplicationMinimumConfidenceResult
		{
			get
			{
				return GetItem("DeduplicationMinimumConfidenceResult", delegate
				{
					var deDuplicationMinimumConfidenceRatingListProvider = new CodeDescriptionPairListProvider(() => new DeDuplicationMinimumConfidenceRating());

					return new CodePairRegistryItem(
						"DeduplicationMinimumConfidenceResult",
						Categories.Organizations_DuplicateDetection_Configuration_OrganisationForm,
						ResString.GetMultilingualString("b0ddcea2-94f9-433b-b6e9-9d12d0b83972", "Minimum Confidence Rating"),
						ResString.GetMultilingualString("9583a490-da45-44a7-9b3f-8b2537603848", "Only potential duplicates where the confidence is higher than the selected value will be shown. If Medium is selected only High confidence results will be shown."),
						deDuplicationMinimumConfidenceRatingListProvider,
						false,
						true,
						new ComboBoxRegistryEditorInfo(deDuplicationMinimumConfidenceRatingListProvider, true),
						RegistryStorageFlags.All,
						RegistryOptions.PreserveTestValue,
						DeDuplicationMinimumConfidenceRating.Codes.Low,
						false);
				});
			}
		}

		public BooleanRegistryItem ExcludePotentialDuplicatesFromOtherCountries
		{
			get
			{
				return GetItem("ExcludePotentialDuplicatesFromOtherCountries", delegate
				{
					return new BooleanRegistryItem(
						"ExcludePotentialDuplicatesFromOtherCountries",
						Categories.Organizations_DuplicateDetection_Configuration_Global,
						ResString.GetMultilingualString("e0890d35-e098-454a-b7b3-afc4164783d8", "Exclude Potential Duplicate Records From Other Countries/Regions"),
						ResString.GetMultilingualString("abf33225-f405-40fd-ac89-050cd5a4d3cc", @"When this registry is set to 'Yes', the system will exclude records from other countries/regions in the list of potential duplicate results."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		public IntRegistryItem RepeatedValueLimit
		{
			get
			{
				return GetItem("RepeatedValueLimit", delegate
				{
					return new IntRegistryItem(
						"RepeatedValueLimit",
						Categories.Organizations_DuplicateDetection_Configuration_Global,
						(NoResString)"Repeated Pattern Limit",
						(NoResString)"Any pattern that is repeated more than the specified number of times will be excluded from duplicate detection. The value can be set between 1 and 200.",
						null,
						RegistryStorageFlags.System,
						RegistryOptions.Default | RegistryOptions.IsOnlyForSupport,
						100,
						1,
						200
						);
				});
			}
		}

		public BooleanRegistryItem ExcludeInactivePotentialDuplicates
		{
			get
			{
				return GetItem("ExcludeInactivePotentialDuplicates", delegate
				{
					return new BooleanRegistryItem(
						"ExcludeInactivePotentialDuplicates",
						Categories.Organizations_DuplicateDetection_Configuration_Global,
						ResString.GetMultilingualString("f9410118-ed7f-4c45-b65d-8d2e48173e29", "Exclude Inactive Potential Duplicate Records"),
						ResString.GetMultilingualString("74078e6c-afd7-4605-9023-37d0883f620e", @"When this registry is set to 'Yes', The system will exclude all inactive records from the list of potential duplicate results."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		#region Configuration

		#region Organisation Form

		public IntRegistryItem MaximumAddressCountForUserDrivenDeduplication
		{
			get
			{
				return GetItem("MaximumAddressCountForUserDrivenDeduplication", delegate
				{
					return new IntRegistryItem(
						"MaximumAddressCountForUserDrivenDeduplication",
						Categories.Organizations_DuplicateDetection_Configuration_OrganisationForm,
						ResString.GetMultilingualString("F3266E50-94C8-4936-862A-C04D1241D611", "Maximum Address Count for User Driven De-duplication"),
						ResString.GetMultilingualString("A8D14374-58DD-4466-A559-DFC006BAE366", "The maximum amount of addresses an organization can contain before it is excluded from De-duplication."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						100,
						20,
						2000);
				});
			}
		}

		public IntRegistryItem MaximumContactCountForUserDrivenDeduplication
		{
			get
			{
				return GetItem("MaximumContactCountForUserDrivenDeduplication", delegate
				{
					return new IntRegistryItem(
						"MaximumContactCountForUserDrivenDeduplication",
						Categories.Organizations_DuplicateDetection_Configuration_OrganisationForm,
						ResString.GetMultilingualString("E453C2ED-9CE1-4C4F-8BBA-0A1A660EAC8E", "Maximum Contact Count for User Driven De-duplication"),
						ResString.GetMultilingualString("3796B321-DF10-4AF4-8BE5-4CA467824F6F", "The maximum amount of contacts an organization can contain before it is excluded from De-duplication."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						100,
						20,
						2000);
				});
			}
		}

		#endregion

		#endregion

		#endregion

		#region Credit Reports

		public BinaryRegistryItem CreditReportsPublicCertificate
		{
			get
			{
				return GetItem("CreditReportsPublicCertificate", delegate
				{
					return new BinaryRegistryItem(
						"CreditReportsPublicCertificate",
						Categories.Organizations_CreditReports,
						(NoResString)"Credit Reports Public Certificate",
						(NoResString)"Specify the Credit Reports Public Certificate",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						Array.Empty<byte>())
					{
						EditorInfo = new FileUpLoaderX509CertificateRegistryEditorInfo()
					};
				});
			}
		}

		public BooleanRegistryItem EnableCreditReports
		{
			get
			{
				return GetItem("EnableCreditReports", () => new BooleanRegistryItem(
					"EnableCreditReports",
					Categories.Organizations_CreditReports,
					(NoResString)"Enable Credit Reports",
					(NoResString)"When this registry is set to 'Yes', Credit Reports are enabled. When this registry is set to 'No', Credit Reports are disabled.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					true));
			}
		}

		public CodeDescriptionBoolWithSingleTrueRegistryItem CreditCheckServiceURLs
		{
			get
			{
				var collection = new CodeDescriptionBoolWithSingleTrueCollection() { CodeMaxLength = 4 };
				collection.Add(new CodeDescriptionBoolWithSingleTrue() { CodeMaxLength = 4, Code = "SYD1", Description = (NoResString)"https://creditreport.wisegrid.net", Bool = true });

				return GetItem("CreditCheckServiceURLs", delegate
				{
					return new CodeDescriptionBoolWithSingleTrueRegistryItem
					(
						"CreditCheckServiceURLs",
						Categories.Organizations_CreditReports,
						(NoResString)"Service's URLs",
						(NoResString)"Specify the URLs that Credit Check Service will use when making requests.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Main", true, false),
						collection
					);
				});
			}
		}

		public CreditReportItemCollectionRegistryItem EnableCreditReportsPerCountryOrganisationAndCompany
		{
			get
			{
				return GetItem("EnableCreditReportsPerCountryOrganisationAndCompany", () => new CreditReportItemCollectionRegistryItem(
					"EnableCreditReportsPerCountryOrganisationAndCompany",
					Categories.Organizations_CreditReports,
					(NoResString)"Enable Credit Reports Per Country/Region (Organization and Company)",
					(NoResString)((NoResString)"When a country/region is ticked as 'Enabled' under either \"Enable Company Reports\" or \"Enable Organization Report\", " +
					(NoResString)"Company or Organization-level Credit Reports respectively for that Country/Region are enabled. When a Country/Region is not ticked as 'Enabled', Credit Reports for that Country/Region are disabled. The " +
					(NoResString)"last four columns are options which apply only to Organization reports."),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					GetCreditReportsCollection()));
			}
		}

		public CreditReportItemCollection GetCreditReportsCollection()
		{
			var collection = new CreditReportItemCollection();
			var countryCollection = (new BusinessObjectFactory()).Load<IRefCountry>(new ZQuery()).OrderBy(c => c.RN_Desc).ToArray();

			foreach (var country in countryCollection)
			{
				var item = SetDefaultCreditReportsRegistry(country.RN_Code, country.RN_Desc);
				item.Country = (NoResString)(country.RN_Desc);
				collection.Add(item);
			}

			return collection;
		}

		CreditReportItem SetDefaultCreditReportsRegistry(string countryCode, string country)
		{
			var isDefaultEnabledCountry = creditDefaultEnabledCountries.Contains(countryCode);
			return new CreditReportItem()
			{
				CountryCode = countryCode,
				Country = country,
				CountryEnabledForCompany = isDefaultEnabledCountry,
				CountryEnabledForOrganisation = isDefaultEnabledCountry,
				ComprehensiveReportEnabled = isDefaultEnabledCountry,
				FailureRiskEnabled = isDefaultEnabledCountry,
				LatePaymentRiskEnabled = isDefaultEnabledCountry,
				CommercialBureauEnquiryEnabled = isDefaultEnabledCountry
			};
		}

		public IntRegistryItem GetCreditReportTimeout
		{
			get
			{
				return GetItem("GetReport", () => new IntRegistryItem(
					"GetReport",
					Categories.Organizations_CreditReports_Timeouts,
					ResString.GetMultilingualString("110b8c98-107e-4f0d-9956-fcc26bc48921", "Get Report"),
					ResString.GetMultilingualString("5fa3f9bd-f2d4-4fca-bcd2-55b4e4c2d78d", "Sets the timeout (in seconds) for Get Report."),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					60,
					30,
					120));
			}
		}

		public IntRegistryItem GetRetrospectiveMonitorTimeOut
		{
			get
			{
				return GetItem("GetRetrospectiveMonitor", () => new IntRegistryItem(
					"GetRetrospectiveMonitor",
					Categories.Organizations_CreditReports_Timeouts,
					ResString.GetMultilingualString("0e20155d-1140-4143-8618-0c373c9fff06", "Get Retrospective Monitor"),
					ResString.GetMultilingualString("363aaf8a-bbe0-45bb-91f5-53dfc0fef90e", "Sets the timeout (in seconds) for Get Retrospective Monitor."),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					60,
					30,
					120));
			}
		}

		public IntRegistryItem CompanyLookupTimeout
		{
			get
			{
				return GetItem("CompanyLookup", () => new IntRegistryItem(
					"CompanyLookup",
					Categories.Organizations_CreditReports_Timeouts,
					ResString.GetMultilingualString("3D012521-F186-4270-8DA8-579FC0BE49AF", "Company Lookup"),
					ResString.GetMultilingualString("18B30F3D-EE5E-468A-A009-74E0E058CFA0", "Sets the timeout (in seconds) for Company Lookup."),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					60,
					30,
					120));
			}
		}

		public IntRegistryItem SaveTradeInformationTimeout
		{
			get
			{
				return GetItem("SaveTradeInformation", () => new IntRegistryItem(
					"SaveTradeInformation",
					Categories.Organizations_CreditReports_Timeouts,
					(NoResString)"Save Trade Information",
					(NoResString)"Sets the timeout (in seconds) for Save Trade Information.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					100,
					30,
					1000));
			}
		}

		readonly string[] creditDefaultEnabledCountries = new[] { Constants.CountryCodes.Australia, Constants.CountryCodes.NewZealand };

		public BooleanRegistryItem EnableRealCreditCheckServiceInTestingSystem
		{
			get
			{
				return GetItem("EnableRealCreditCheckServiceInTestingSystem", () => new BooleanRegistryItem(
					"EnableRealCreditCheckServiceInTestingSystem",
					Categories.Organizations_CreditReports,
					(NoResString)"Enable Real Credit Check Service In The Testing System",
					(NoResString)"When this registry is set to 'Yes', will call real Credit Check Service even though in the testing system.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false));
			}
		}

		public BooleanRegistryItem EnableTradeBalanceInformationSent
		{
			get
			{
				return GetItem("EnableTradeBalanceInformationSent", () => new BooleanRegistryItem(
					"EnableTradeBalanceInformationSent",
					Categories.Organizations_CreditReports,
					(NoResString)"Enable Trade Balance Information Sending",
					(NoResString)"When this registry is set to 'Yes', Trade Balance Information Sending is enabled. When this registry is set to 'No', Trade Balance Information Sending is disabled.",
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					true));
			}
		}

		public BooleanRegistryItem EnableTradeBalanceServiceTaskForTestingSystem
		{
			get
			{
				return GetItem("EnableTradeBalanceInformationServiceTaskForTestingSystem", () => new BooleanRegistryItem(
					"EnableTradeBalanceInformationServiceTaskForTestingSystem",
					Categories.Organizations_CreditReports,
					(NoResString)"Enable Trade Balance Information Service Task For Testing System",
					(NoResString)"When this registry is set to 'Yes', Trade Balance Information Service Task For Testing System is enabled. When this registry is set to 'No', Trade Balance Information Service Task For Testing System is disabled.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false));
			}
		}

		#endregion

		#region Allow Numeric Characters In Code Generation

		public BooleanRegistryItem AllowNumericCharactersInCodeGeneration
		{
			get
			{
				return GetItem("AllowNumericCharactersInCodeGeneration", delegate
				{
					return new BooleanRegistryItem(
						"AllowNumericCharactersInCodeGeneration",
						Categories.Organizations_Codes,
						ResString.GetMultilingualString("16c8d60a-33ed-4dd7-bd66-377469bda44e", "Allow Numeric Characters In Code Generation"),
						ResString.GetMultilingualString("cda38fb7-af31-4ffa-85d9-4fe851fcc12b", "When generating a new organization code from an organization name, numeric characters are ignored by default and only letters are used as part of the new code. E.g. an organization called \"7 Eleven\" in USLAX will be \"ELEVENLAX\". By enabling this registry, numeric characters will be retained and in the example, the code generated will be \"7ELEVELAX\"."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		#endregion

		#region Enable Controlling Agent Functionality and Validations

		public BooleanRegistryItem EnableControllingAgentFunctionalityAndValidations
		{
			get
			{
				return GetItem("EnableControllingAgentFunctionalityAndValidations", delegate
				{
					return new BooleanRegistryItem(
						"EnableControllingAgentFunctionalityAndValidations",
						Categories.Organizations,
						ResString.GetMultilingualString("be2edeb3-5398-41da-971a-15a6ce3c8d8c", "Enable Controlling Agent Functionality and Validations"),
						ResString.GetMultilingualString("e8a4b7d0-1215-4aa3-9151-cad65420e093", "When turned on, the Organization Type \"Controlling Agent\" will be visible on the Organization form and its validation rules will be enabled."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#region Enable OrgPatternMatchOverride

		public BooleanRegistryItem EnableOrgPatternMatchOverride
		{
			get
			{
				return GetItem("EnableOrgPatternMatchOverride", delegate
				{
					return new BooleanRegistryItem(
						"EnableOrgPatternMatchOverride",
						Categories.Organizations,
						(NoResString)"Enable OrgPatternMatchOverride",
						(NoResString)"Enabling this registry will allow the OrgPatternMatchOverride Collection to be exported in an organisation xml.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region Enable Controlling Customer Functionality and Validations

		public BooleanRegistryItem EnableControllingCustomerFunctionalityAndValidations
		{
			get
			{
				return GetItem("EnableControllingCustomerFunctionalityAndValidations", delegate
				{
					return new BooleanRegistryItem(
						"EnableControllingCustomerFunctionalityAndValidations",
						Categories.Organizations,
						ResString.GetMultilingualString("5401f8a0-33c0-4421-ad17-c4b30ef5427e", "Enable Controlling Customer Functionality and Validations"),
						ResString.GetMultilingualString("c4b02377-7a54-4f8a-97a2-164f732a2975", "When turned on, the Organization Type \"Controlling Customer\" will be visible on the Organization form and its validation rules will be enabled."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#region Enable Defaulting Closest Port on Address Validation

		public BooleanRegistryItem EnableDefaultingClosestPortOnAddressValidation
		{
			get
			{
				return GetItem("EnableDefaultingClosestPortOnAddressValidation", delegate
				{
					return new BooleanRegistryItem(
						"EnableDefaultingClosestPortOnAddressValidation",
						Categories.Organizations_DefaultValues_UNLOCODefaultingRules,
						ResString.GetMultilingualString("3771fb0a-ceec-48bd-97b0-6678e37f30a3", "Enable Defaulting Closest Port on Address Validation"),
						ResString.GetMultilingualString("00c53e7d-1f32-4eec-bfda-f3e38092675d", @"When enabled, this program will look for the closest UNLOCO to the validated address’s Geo-Location Coordinates that comply with UNLOCO Defaulting Rules.
IE:
Yes = Enabled = Closest to Longitude X, Latitude Y and has Airport AND/OR Seaport AND/OR no Identifier.
No = Disabled = Closest not applicable but has Airport AND/OR Seaport AND/OR no Identifier.
This registry item is dependent on the existence of Geo-Location Coordinates in the address.
If an address is manually verified, then Geo-Location Coordinates can be inserted manually.
This registry item works in conjunction with AND or OR logic and UNLOCO Defaulting Rules."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		#endregion

		#region UNLOCO Defaulting Rules

		public CodeDescriptionBoolDisallowNewRegistryItem UNLOCODefaultingRules
		{
			get
			{
				return GetItem("UNLOCODefaultingRules", delegate
				{
					var codeDescriptionValues = new CodeDescriptionPairList();

					codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasAirport, ResString.GetMultilingualString("60674bec-6b45-40e5-81f2-757c6fd5c903", "Has Airport"));
					codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasSeaport, ResString.GetMultilingualString("bbd1a05e-fe3c-40c4-85c3-61936dc3b333", "Has Seaport"));
					codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasPost, ResString.GetMultilingualString("c9996e3e-bd71-4b0f-be2b-ef71e61d2b45", "Has Post"));
					codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasCustomsLodge, ResString.GetMultilingualString("b07e7193-0a31-4530-84ff-e41a1a603957", "Has Customs"));
					codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasUnload, ResString.GetMultilingualString("f033249f-9c6c-4a00-89c3-b840cfe67d21", "Has Unload"));
					codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasRail, ResString.GetMultilingualString("620aee9c-54f4-49a9-af93-6121ce201f15", "Has Rail"));
					codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasRoad, ResString.GetMultilingualString("acde6a64-99a3-4b59-a07a-63142413ab62", "Has Road"));
					codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasStore, ResString.GetMultilingualString("7f0e24fe-7384-4426-ba5d-db6c88198b13", "Has Store"));
					codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasTerminal, ResString.GetMultilingualString("c8cc03f5-874f-4508-b008-5511f81daf97", "Has Terminal"));
					codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasDischarge, ResString.GetMultilingualString("6d275318-8db3-471e-8d15-b78bd9a24a20", "Has Discharge"));
					codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasOutport, ResString.GetMultilingualString("44d0ed63-3645-414f-800d-7b77d341ce10", "Has Outport"));

					var defaultValue = new CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection(new ReadOnlyCodeDescriptionPairList(codeDescriptionValues));

					defaultValue[0].Bool = true;
					defaultValue[1].Bool = true;

					return new CodeDescriptionBoolDisallowNewRegistryItem(
						"UNLOCODefaultingRules",
						Categories.Organizations_DefaultValues_UNLOCODefaultingRules,
						ResString.GetMultilingualString("fa593645-6b77-4211-b735-ba162842b022", "UNLOCO Defaulting Rules"),
						ResString.GetMultilingualString("7ba60109-9fcd-4ee5-aade-660e936bcc3d", @"Please turn off 'Ignore UNLOCO Defaulting Rules' registry item to make this registry item take effect. This list of UNLOCO Identifiers is used for selecting the most appropriate UNLOCO to be defaulted into a Validated Address when multiple UNLOCO possibilities are found.
This registry item works in conjunction with Closest Port rule and And or OR logic which means that it only defaults after an address has been validated (green envelope) and has Geo-Location Coordinates."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("ebdc13b9-96b9-4fb9-bdd5-e2e8344c9f6d", "Applied"), true, true),
						defaultValue);
				});
			}
		}

		#endregion

		#region Enable Predefined UNLOCO Filter In Organisations

		public BooleanRegistryItem EnablePredefinedUNLOCOFilterInOrganizations
		{
			get
			{
				return GetItem("EnablePredefinedUNLOCOFilterInOrganizations", delegate
				{
					return new BooleanRegistryItem(
						"EnablePredefinedUNLOCOFilterInOrganizations",
						Categories.Organizations_DefaultValues_UNLOCODefaultingRules,
						ResString.GetMultilingualString("e7a18146-2262-4582-a53c-fea6e7b1ae3f", "Enable Predefined UNLOCO Filter In Organizations"),
						ResString.GetMultilingualString("e2ba1e28-fecc-49f9-ab55-5a5a87b8b524",
						@"When enabled, a predefined set of filters is added to the UNLOCO Search button on the Organization > Details Tab.
This predefined set of filters are defined by the following Registry Settings:
-AND or OR Logic
-UNLOCO Defaulting Rules"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		#endregion

		#region Require All UNLOCO Conditions To Be Met

		public BooleanRegistryItem RequireAllUNLOCOConditionsToBeMet
		{
			get
			{
				return GetItem("RequireAllUNLOCOConditionsToBeMet", delegate
				{
					return new BooleanRegistryItem
					(
						"RequireAllUNLOCOConditionsToBeMet",
						Categories.Organizations_DefaultValues_UNLOCODefaultingRules,
						ResString.GetMultilingualString("AD8D2E75-3A6C-463F-B6DA-2577380A2E0B", "AND or OR Logic"),
						ResString.GetMultilingualString("8212F4C8-9E79-447D-99B0-F9EFF466B1E6", @"When set to YES this program will look for UNLOCOs with all matching Identifiers specified in the UNLOCO Defaulting Rules registry item. NO will search for UNLOCOs with any Identifiers.
IE:
YES = AND = all Identifiers.
NO = OR = any Identifiers.
This registry item works in conjunction with Closest Port rule and UNLOCO Defaulting Rules which means that it only defaults after an address has been validated (green envelope) and has Geo-Location Coordinates."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						true);
				});
			}
		}

		#endregion

		#region Ignore UNLOCO Defaulting Rules

		public BooleanRegistryItem IgnoreUNLOCODefaultingRules
		{
			get
			{
				return GetItem("IgnoreUNLOCODefaultingRules", delegate
				{
					return new BooleanRegistryItem
					(
						"IgnoreUNLOCODefaultingRules",
						Categories.Organizations_DefaultValues_UNLOCODefaultingRules,
						ResString.GetMultilingualString("D5670368-904F-4317-8F2F-C9C76045D49F", "Ignore UNLOCO Defaulting Rules"),
						ResString.GetMultilingualString("117B34D9-B75E-4168-8B7D-85892469EAE4", @"When set to YES. 'UNLOCO Defaulting Rules' registry item will NOT be applied when retrieving closest port and NOT displayed in default UNLOCO filter.
When set to NO. 'UNLOCO Defaulting Rules' registry item will be applied when retrieving closest port and be displayed in default UNLOCO filter."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false);
				});
			}
		}

		#endregion

		#region New Staff Assignments As Global

		public BooleanRegistryItem NewStaffAssignmentsAsGlobal
		{
			get
			{
				return GetItem("NewStaffAssignmentsAsGlobal", () => new BooleanRegistryItem(
																		"NewStaffAssignmentsAsGlobal",
																		Categories.Organizations,
																		ResString.GetMultilingualString("72ae29ee-4107-4860-8756-e1422df9566b", "New Staff Assignments as Global"),
																		ResString.GetMultilingualString("aca3a185-d092-44c0-a16d-8174e91bd968", "If this registry is turned on then newly created staff assignments will be Global, as opposed to Company specific.\r\nNote that only users with appropriate security rights will be allowed to create and maintain Global staff assignments."),
																		RegistryStorageFlags.System,
																		false));
			}
		}

		#endregion

		#region Make Sales Rep mandatory on Organisations

		public BooleanRegistryItem MakeSalesRepMandatory
		{
			get
			{
				return GetItem("MakeSalesRepMandatory", delegate
				{
					return new BooleanRegistryItem(
						"MakeSalesRepMandatory",
						Categories.Organizations_OrganizationRequiredFields,
						ResString.GetMultilingualString("9d173984-4175-4759-a0e3-735686f15a73", "Make Sales Rep Mandatory"),
						ResString.GetMultilingualString("8b0c0a66-d1fb-453f-993d-fb50ac2fc3fc", "If registry is turned on, when saving changes on organizations (new or edits), if it does not have a sales rep record in staff assignment, it will auto add in a Sales rep record but leaving Initials field blank with an error, so that it cannot be saved until it is entered. This only applies to Receivables or Sales organizations. This does not apply to temporary organizations."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#region Make External Debtor and Creditor Codes Mandatory

		public BooleanRegistryItem MakeExternalDebtorCodeMandatory
		{
			get
			{
				return GetItem("MakeExternalDebtorCodeMandatory", delegate
				{
					return new BooleanRegistryItem(
						"MakeExternalDebtorCodeMandatory",
						Categories.Organizations,
						ResString.GetMultilingualString("0f06ec74-51ac-4158-948d-9d1a441de29f", "Make External Debtor Code Mandatory"),
						ResString.GetMultilingualString("a20ae866-2a15-4d50-a458-566712ae0346", "When this registry is set to 'Yes', the 'External Debtor Code' field will become mandatory for organization records flagged as 'Receivables'."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						false);
				});
			}
		}

		public BooleanRegistryItem MakeExternalCreditorCodeMandatory
		{
			get
			{
				return GetItem("MakeExternalCreditorCodeMandatory", delegate
				{
					return new BooleanRegistryItem(
						"MakeExternalCreditorCodeMandatory",
						Categories.Organizations,
						ResString.GetMultilingualString("dcacc6d1-5e3a-4ed2-a0f2-fa3dc896b773", "Make External Creditor Code Mandatory"),
						ResString.GetMultilingualString("3657a20c-1ff6-40c7-8409-373d208f0f01", "When this registry is set to 'Yes', the 'External Creditor Code' field will become mandatory for organization records flagged as 'Payables'."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#region Dropdown Address List Count Threshold

		public IntRegistryItem DropdownAddressCountThreshold
		{
			get
			{
				return GetItem("DropdownAddressCountThreshold", delegate
				{
					return new IntRegistryItem(
						"DropdownAddressCountThreshold",
						Categories.Organizations_DefaultValues,
						ResString.GetMultilingualString("10ce7fcb-a6ad-4868-8b20-6b2dd11f2dec", "Drop-down Address List Count Threshold"),
						ResString.GetMultilingualString("3e9d73c3-dcc3-4c01-ac08-6d1dc7634f7f", "When selecting an address from an organization's address list in address controls, an Address Search Form will display instead of a Drop-down Address List if the count of the address list is more than the value set."), new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						30, 8, 100);
				});
			}
		}

		#endregion

		public BooleanRegistryItem UpdateEDICodeMappingwhenChargeCodeIsRenamed
		{
			get
			{
				return GetItem("UpdateEDICodeMappingwhenChargeCodeIsRenamed", delegate
				{
					return new BooleanRegistryItem(
						"UpdateEDICodeMappingwhenChargeCodeIsRenamed",
						Categories.Organizations,
						ResString.GetMultilingualString("20133B20-B581-450E-AB3A-FF86E2EAD93F", "Update EDI Code Mapping when charge code is renamed"),
						ResString.GetMultilingualString("9CA6795D-AB9B-475E-87BB-ACD372417DF3", @"When this registry is set to ‘Yes’ and a charge code is renamed, all matching EDI Charge Code Mappings will be updated.
When this registry is set to ‘No’, no update will occur.

It is recommended that when a charge code is renamed, you should review all existing EDI Code Mapping.
In particular, in a multi - companies system."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						false);
				});
			}
		}

		#region Misc Organisation
		#region SuppressResourceStringsCheckRegion

		public SystemDefinedOrganisationRegistryItem MiscOrganisation
		{
			get
			{
				return GetItem("MiscOrganisation", delegate
				{
					SystemDefinedOrganisationRegistryItem result = new SystemDefinedOrganisationRegistryItem(
						"MiscOrganisation",
						Categories.Organizations,
						(NoResString)"The system defined MISCELLANEOUS organization.",
						(NoResString)"This is the MISCELLANEOUS organization in the system (Org. Code: MISC)",
						RegistryStorageFlags.System,
						typeof(MiscOrganisation));
					result.Options = RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue;
					return result;
				});
			}
		}

		#endregion
		#endregion

		#region Campaign Management

		#region Link Tracking

		public StringRegistryItem LinkTrackingImageUrl
		{
			get
			{
				return GetItem("LinkTrackingImageUrl", delegate
				{
					return new StringRegistryItem(
						"LinkTrackingImageUrl",
						Categories.SalesMarketing_CampaignManagement,
						(NoResString)"Link Tracking Image URL",
						(NoResString)"URL for image which is inserted into every campaign email. It is used to track clicks on hyperlinks in campaigns including those that do not use the Link Activity functionality.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"http://ilul.me/cw1ver.png"
					);
				});
			}
		}

		public StringRegistryItem LinkTrackingUrl
		{
			get
			{
				return GetItem("LinkTrackingUrl", delegate
				{
					return new StringRegistryItem(
						"LinkTrackingUrl",
						Categories.SalesMarketing_CampaignManagement,
						(NoResString)"Link Tracking URL",
						(NoResString)"URL used to track clicks on hyperlinks in campaigns. This URL is added as a prefix to the actual hyperlink. A click on the modified link first goes to the tracking site for recording and redirecting to the final destination.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"http://ilul.me/"
						);
				});
			}
		}

		#endregion

		public GuidRegistryItem PermanentlySavedCampaignEmailDocType
		{
			get
			{
				return GetItem("PermanentlySavedCampaignEmailDocType", delegate
				{
					var miscDocType = new BusinessObjectFactory().LoadTop1<IRefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, Core.Constants.RefDocTypes.MiscellaneousDocument));

					var result = new GuidRegistryItem(
						"PermanentlySavedCampaignEmailDocType",
						Categories.SalesMarketing_CampaignManagement,
						ResString.GetMultilingualString("018823f5-6ff6-4168-a72a-5a11e293e47c", "Permanently Saved Campaign Email Doc Type"),
						ResString.GetMultilingualString("ed9050ae-e11d-43cf-b921-4624e972693d", "eDocs Doc Type for Campaign emails that are permanently saved against recipients."),
						RegistryStorageFlags.System,
						miscDocType?.PK.ToGuid() ?? Guid.Empty);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.RefDocType);
					return result;
				});
			}
		}

		public BooleanRegistryItem PermanentlySaveAgainstRecipientDefault
		{
			get
			{
				return GetItem("PermanentlySaveAgainstRecipientDefault", () => new BooleanRegistryItem(
					"PermanentlySaveAgainstRecipientDefault",
					Categories.SalesMarketing_CampaignManagement,
					ResString.GetMultilingualString("2A80E0B6-2676-482F-9A70-180702FEFF56", "Permanently Save Against Recipient Default"),
					ResString.GetMultilingualString("0A0661AC-8249-4141-8C83-EF4E361D96BF", "When this registry is set to 'Yes', it will enable the Permanently Save Against Recipient Default checkbox in the Campaign Sending Options."),
					RegistryStorageFlags.Company | RegistryStorageFlags.System,
					false));
			}
		}

		#region Max Records in Display Grid

		public IntRegistryItem DisplayGridMaxRecords
		{
			get
			{
				return GetItem("DisplayGridMaxRecords", delegate
				{
					return new IntRegistryItem("DisplayGridMaxRecords",
						Categories.SalesMarketing_CampaignManagement,
						ResString.GetMultilingualString("a028d9ec-c1be-47f8-834a-600f99b9a848", "Max Record in Display Grid"),
						ResString.GetMultilingualString("1a3db830-97a4-4c8f-bb1d-35cb476e87c2", "The maximum number of records to display in the display grid"),
						 RegistryStorageFlags.System,
						 3000);
				});
			}
		}

		#endregion

		#region Stage List

		public CodeDescriptionPairListRegistryItem CampaignStageList
		{
			get
			{
				return GetItem("CampaignStageList", delegate
				{
					CodeDescriptionPairList defaultValue = new CodeDescriptionPairList();
					defaultValue.AddPair("UDF", ResString.GetMultilingualString("Registry|OrganisationsDataRegistry|CampaignStageListUndefinedDescription", "Undefined - You can modify this in the System Registry, under {0}", RegistryConstants.GetCategory(Categories.SalesMarketing_CampaignManagement, ResString.GetMultilingualString("e0f888fb-5fdb-4709-b40e-5dcebd963c1b", "Campaign Stage List"))));

					return new CodeDescriptionPairListRegistryItem(
						"CampaignStageList",
						Categories.SalesMarketing_CampaignManagement,
						ResString.GetMultilingualString("e0f888fb-5fdb-4709-b40e-5dcebd963c1b", "Campaign Stage List"),
						ResString.GetMultilingualString("926bb0ba-a33a-4b42-a190-dcf8e6435ab9", "This is the stage list for Campaign Management."),
						GlbCompanyCampaignSchema.G0_Stage.MaxLength,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						defaultValue);
				});
			}
		}

		#endregion

		#region Campaign Manager Client Relationship Management Category 1 List

		#region Label

		public MultilingualStringRegistryItem CampaignCategory1Label
		{
			get
			{
				return GetItem("CampaignCategory1Label", delegate
				{
					return new MultilingualStringRegistryItem(
						"CampaignCategory1Label",
						Categories.SalesMarketing_CampaignManagement_ClientRelationshipManagement_Category1List,
						ResString.GetMultilingualString("a08438bc-e87c-469d-a9ed-df97df8de0b1", "Label"),
						ResString.GetMultilingualString("edca0354-cd1e-4a2a-ba48-7643baa8c422", "Allows you to further categorize your campaigns, by a media category identifier."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						ResString.GetMultilingualString("30320ed0-b197-48ca-b5f3-11c97e0f531c", "Media Category"));
				});
			}
		}

		#endregion

		#region List

		public CodeDescriptionBoolRegistryItem CampaignCategory1List
		{
			get
			{
				return GetItem("CampaignMediaCategoryList", delegate
				{
					CodeDescriptionBoolCollection defaultValue = new CodeDescriptionBoolCollection(GlbCompanyCampaignSchema.G0_Category.MaxLength);

					defaultValue.Add("PRINT", ResString.GetMultilingualString("88cfd255-ddfa-4c72-8d2a-7e09f1255fab", "Print"), true);
					defaultValue.Add("REFER", ResString.GetMultilingualString("99dab8eb-3adb-4ecf-b5b2-e4cb41ae93fd", "Referral"), true);
					defaultValue.Add("RADIO", ResString.GetMultilingualString("0d607c84-02ca-4198-81c2-f8c29ac643ff", "Radio"), true);
					defaultValue.Add("TELEV", ResString.GetMultilingualString("653eb75a-7439-435a-b90f-ebf40c1ea0dd", "Television"), true);

					return new CodeDescriptionBoolRegistryItem(
						"CampaignMediaCategoryList",
						Categories.SalesMarketing_CampaignManagement_ClientRelationshipManagement_Category1List,
						ResString.GetMultilingualString("d10e0e69-1b03-4898-824b-b69e109fe2ed", "List"),
						ResString.GetMultilingualString("a4ea8e83-b04d-4751-b99a-956196dded50", "This is a categorization list for Campaign Management, which could be used for Media Category."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("acc43fe1-ce9d-4a30-b8d1-99ac9f39c2f2", "Enabled")),
						defaultValue);
				});
			}
		}

		#endregion

		#endregion

		#region Campaign Manager Client Relationship Management Category 2 List

		#region Label

		public MultilingualStringRegistryItem CampaignCategory2Label
		{
			get
			{
				return GetItem("CampaignCategory2Label", delegate
				{
					return new MultilingualStringRegistryItem(
						"CampaignCategory2Label",
						Categories.SalesMarketing_CampaignManagement_ClientRelationshipManagement_Category2List,
						ResString.GetMultilingualString("a08438bc-e87c-469d-a9ed-df97df8de0b1", "Label"),
						ResString.GetMultilingualString("eb64ee20-d9e8-4cc4-ab90-f7b981c23497", "Allows you to further categorize your campaigns, by a media type identifier."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						ResString.GetMultilingualString("8e4e345b-585c-4c0c-93be-d304bc2b4c2c", "Media Type"));
				});
			}
		}

		#endregion

		#region List

		public CodeDescriptionBoolRegistryItem CampaignCategory2List
		{
			get
			{
				return GetItem("CampaignMediaTypeList", delegate
				{
					CodeDescriptionBoolCollection defaultValue = new CodeDescriptionBoolCollection(GlbCompanyCampaignSchema.G0_Type.MaxLength);
					defaultValue.Add("PREAP", ResString.GetMultilingualString("97cf09fc-e3db-4b91-bf73-3cf98d48f76f", "Pre-approach letter"));
					defaultValue.Add("EXIST", ResString.GetMultilingualString("37d92222-ab3b-4106-a9da-48b7841fff67", "Existing Client"));
					defaultValue.Add("SLT30", ResString.GetMultilingualString("0b8427d7-2c95-4243-8c12-9b2b5355155e", "30 second slot"));

					return new CodeDescriptionBoolRegistryItem(
						"CampaignMediaTypeList",
						Categories.SalesMarketing_CampaignManagement_ClientRelationshipManagement_Category2List,
						ResString.GetMultilingualString("d10e0e69-1b03-4898-824b-b69e109fe2ed", "List"),
						ResString.GetMultilingualString("26973b70-42b4-4f05-a7ff-b825d969d9a2", "This is a 2nd categorization list for Campaign Management, which could be used for Media Type."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("acc43fe1-ce9d-4a30-b8d1-99ac9f39c2f2", "Enabled")),
						defaultValue);
				});
			}
		}

		#endregion

		#endregion

		#region Campaign Manager Human Resources Category 1 List

		#region Label

		public MultilingualStringRegistryItem HRCampaignCategory1Label
		{
			get
			{
				return GetItem("HRCampaignCategory1Label", delegate
				{
					return new MultilingualStringRegistryItem(
						"HRCampaignCategory1Label",
						Categories.SalesMarketing_CampaignManagement_HumanResources_Category1List,
						ResString.GetMultilingualString("9756c1a6-1908-4e82-abf4-663602066a83", "Label"),
						ResString.GetMultilingualString("7a2ebe60-75b2-4776-bcf2-c185825bb02d", "Allows you to further categorize your campaigns, by a media category identifier."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						ResString.GetMultilingualString("c1b05540-2842-4d77-b9ac-c78f78669346", "Media Category"));
				});
			}
		}

		#endregion

		#region List

		public CodeDescriptionBoolRegistryItem HRCampaignCategory1List
		{
			get
			{
				return GetItem("HRCampaignMediaCategoryList", delegate
				{
					CodeDescriptionBoolCollection defaultValue = new CodeDescriptionBoolCollection(GlbCompanyCampaignSchema.G0_Category.MaxLength);

					defaultValue.Add("PRINT", ResString.GetMultilingualString("748a8e36-0af3-4ded-8748-78156a3eca1b", "Print"), true);
					defaultValue.Add("REFER", ResString.GetMultilingualString("64aa0610-b95c-4084-9adf-9f488166e573", "Referral"), true);
					defaultValue.Add("RADIO", ResString.GetMultilingualString("a5090dc0-facb-4b90-b02e-aaae541b2e61", "Radio"), true);
					defaultValue.Add("TELEV", ResString.GetMultilingualString("de700fd1-d86f-49cb-9dfc-b113e4b8d382", "Television"), true);

					return new CodeDescriptionBoolRegistryItem(
						"HRCampaignMediaCategoryList",
						Categories.SalesMarketing_CampaignManagement_HumanResources_Category1List,
						ResString.GetMultilingualString("c49b0d79-c9a2-46e4-bdcc-b76e19443d80", "List"),
						ResString.GetMultilingualString("53745bb6-b8a7-4cc0-9b71-2e76d6aedd08", "This is a categorization list for Campaign Management, which could be used for Media Category."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("432f29c3-f630-481a-9599-30298fb8b623", "Enabled")),
						defaultValue);
				});
			}
		}

		#endregion

		#endregion

		#region Campaign Manager Human Resources Category 2 List

		#region Label

		public MultilingualStringRegistryItem HRCampaignCategory2Label
		{
			get
			{
				return GetItem("HRCampaignCategory2Label", delegate
				{
					return new MultilingualStringRegistryItem(
						"HRCampaignCategory2Label",
						Categories.SalesMarketing_CampaignManagement_HumanResources_Category2List,
						ResString.GetMultilingualString("8d10f2fd-4606-4f6c-917e-8f1a185df977", "Label"),
						ResString.GetMultilingualString("f7681b1d-daf9-4ff3-a0cb-98f47b47169c", "Allows you to further categorize your campaigns, by a media type identifier."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						ResString.GetMultilingualString("d1765371-02d1-4371-b0c1-4a808d93bd13", "Media Type"));
				});
			}
		}

		#endregion

		#region List

		public CodeDescriptionBoolRegistryItem HRCampaignCategory2List
		{
			get
			{
				return GetItem("HRCampaignMediaTypeList", delegate
				{
					CodeDescriptionBoolCollection defaultValue = new CodeDescriptionBoolCollection(GlbCompanyCampaignSchema.G0_Type.MaxLength);
					defaultValue.Add("PREAP", ResString.GetMultilingualString("309729b6-8b9e-4c65-aa3d-96ad7a0d02ba", "Pre-approach letter"));
					defaultValue.Add("EXIST", ResString.GetMultilingualString("ac41dcfe-2c27-40da-b473-145db4851b8e", "Existing Client"));
					defaultValue.Add("SLT30", ResString.GetMultilingualString("11a39939-6929-4915-844e-d71de41cce27", "30 second slot"));

					return new CodeDescriptionBoolRegistryItem(
						"HRCampaignMediaTypeList",
						Categories.SalesMarketing_CampaignManagement_HumanResources_Category2List,
						ResString.GetMultilingualString("669fdcad-4638-4228-8d89-03180ffe1ec3", "List"),
						ResString.GetMultilingualString("6680060e-37e3-4cbe-91c0-d78673b0e0bc", "This is a 2nd categorization list for Campaign Management, which could be used for Media Type."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("84e10688-351c-4c79-937c-5833833f9d39", "Enabled")),
						defaultValue);
				});
			}
		}

		#endregion

		#endregion

		#region AutoSaveVoteSurveyExamAnswers

		public BooleanRegistryItem AutoSaveVoteSurveyExamAnswers
		{
			get
			{
				return GetItem("AutoSaveVoteSurveyExamAnswers", delegate
				{
					return new BooleanRegistryItem(
						"AutoSaveVoteSurveyExamAnswers",
						Categories.SalesMarketing_CampaignManagement,
						ResString.GetMultilingualString("728e75e7-fca0-405a-920b-12cf30c9ab61", "Auto Save Vote / Survey / Exam Answers"),
						ResString.GetMultilingualString("06f6ef31-7771-4321-b2b6-0b21717d6619", "When this flag is set to TRUE, vote nominations for vote campaigns and answers for survey / exam campaigns will be automatically stored in the database when populated."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		#endregion

		#region Subscription Settings

		public MultilingualStringRegistryItem UnsubscribedSuccessfully
		{
			get
			{
				return GetItem(nameof(UnsubscribedSuccessfully), () => new MultilingualStringRegistryItem(
					nameof(UnsubscribedSuccessfully),
					Categories.SalesMarketing_CampaignManagement_Unsubscribe,
					ResString.GetMultilingualString("2edeaa3d-5743-4d3e-b5fc-df9c60ef6eb7", "Successfully Unsubscribed"),
					ResString.GetMultilingualString("e2d38ce6-e342-446b-8c78-bba1c5dc8e0e", "Allows you to change the message appearing on the Auto Unsubscribe web page for a user to unsubscribe."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					ResString.GetMultilingualString("f31d54ca-61db-4d7c-8c36-b0b0c4aea5d1", "You have successfully unsubscribed.")));
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1048")]
		public MultilingualStringRegistryItem SubscripitionPreferenceSuccessfullyAdmin
		{
			get
			{
				return GetItem(nameof(SubscripitionPreferenceSuccessfullyAdmin), () => new MultilingualStringRegistryItem(
					nameof(SubscripitionPreferenceSuccessfullyAdmin),
					Categories.SalesMarketing_CampaignManagement_SubscripitionPreference,
					ResString.GetMultilingualString("84efaa65-f6ce-46c1-88fd-2793dc42f24f", "Successfully changed the preferences (Displayed in {0})", Constants.ProductName),
					ResString.GetMultilingualString("0a06325e-3f96-49dd-ae66-598006e5a94c", "Allows you to change the message appearing on the subscription preference form after administrator successfully changed the preferences."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					ResString.GetMultilingualString("cee84e6a-e177-462a-9b53-5a0e61a64bc4", "You have successfully changed the subscription preferences.")));
			}
		}

		public MultilingualStringRegistryItem SubscripitionPreferenceSuccessfullyUser
		{
			get
			{
				return GetItem(nameof(SubscripitionPreferenceSuccessfullyUser), () => new MultilingualStringRegistryItem(
					nameof(SubscripitionPreferenceSuccessfullyUser),
					Categories.SalesMarketing_CampaignManagement_SubscripitionPreference,
					ResString.GetMultilingualString("cf81ce93-8a83-4443-81ea-5bf7929d8377", "Successfully changed the preferences (Displayed on the web page)"),
					ResString.GetMultilingualString("004850e8-1748-4cb5-a36e-a6adddbb0fb7", "Allows you to change the message appearing on the subscription preference web page after user successfully changed the preferences."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					ResString.GetMultilingualString("a309eb8a-5593-4412-b167-745b1dab361b", "You have successfully changed your subscription preferences.")));
			}
		}

		public MultilingualStringRegistryItem SubscripitionPreferencePageTitle
		{
			get
			{
				return GetItem(nameof(SubscripitionPreferencePageTitle), () => new MultilingualStringRegistryItem(
					nameof(SubscripitionPreferencePageTitle),
					Categories.SalesMarketing_CampaignManagement_SubscripitionPreference,
					ResString.GetMultilingualString("f3611e86-5e25-49ef-a5b6-f604ef1e95e1", "Subscription preference page title"),
					ResString.GetMultilingualString("e4ddc2f0-01ae-4862-9ad5-c995bc884541", "Allows you to change the message appearing on the subscription preference web page title."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					ResString.GetMultilingualString("edff4d28-ba66-4f5d-beb8-5f3c406f0a38", "Email Subscription Preference")));
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		public MultilingualStringRegistryItem SubscripitionPreferencePageMessage
		{
			get
			{
				return GetItem(nameof(SubscripitionPreferencePageMessage), () => new MultilingualStringRegistryItem(
					nameof(SubscripitionPreferencePageMessage),
					Categories.SalesMarketing_CampaignManagement_SubscripitionPreference,
					ResString.GetMultilingualString("544378a3-c14c-4dd4-9c54-462f5a93e96c", "Subscription preference page message"),
					ResString.GetMultilingualString("00227d44-3804-45a6-8baf-934e99fec37c", "Allows you to change the message appearing on the subscription preference web page message."),
					new StringRegistryDataType(),
					new TextRegistryEditorInfo(TextEditorType.Memo),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.Default,
					ResString.GetMultilingualString("9be29c2d-30c5-4e1b-89c2-328dcdca5608",
						"Email (*ContactEmail*) is currently subscribed to the following emails and alerts.\r\n\r\nPlease select what you would like to hear about?")));
			}
		}

		public MultilingualStringRegistryItem AlreadyUnsubscribed
		{
			get
			{
				return GetItem(nameof(AlreadyUnsubscribed), () => new MultilingualStringRegistryItem(
					nameof(AlreadyUnsubscribed),
					Categories.SalesMarketing_CampaignManagement_Unsubscribe,
					ResString.GetMultilingualString("0C08D7E6-39D1-49A4-B9AA-E5ECB3D16DF4", "Already Unsubscribed"),
					ResString.GetMultilingualString("935055E0-A5DB-434B-901F-564EB84E45DA", "Allows you to change the message appearing on the Auto Unsubscribe web page when a user unsubscribes from the same campaign several times."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					ResString.GetMultilingualString("78B133F5-94C5-412C-B156-60B43EF8E786", "You have already unsubscribed from this campaign.")));
			}
		}

		public MultilingualStringRegistryItem ResubscribedSuccessfully
		{
			get
			{
				return GetItem(nameof(ResubscribedSuccessfully), () => new MultilingualStringRegistryItem(
					nameof(ResubscribedSuccessfully),
					Categories.SalesMarketing_CampaignManagement_Unsubscribe,
					ResString.GetMultilingualString("405F93AA-8F8C-45DF-881A-517E5B7ECC04", "Successfully Re-subscribed"),
					ResString.GetMultilingualString("95530784-6A74-4645-9AF5-933A8DC1E33D", "Allows you to change the message appearing on the Auto Unsubscribe web page when a user resubscribe."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					ResString.GetMultilingualString("CAF10BD6-FAD2-4F1C-B0AB-BEE574808225", "You have successfully re-subscribed.")));
			}
		}

		public MultilingualStringRegistryItem ResubscribeLabel
		{
			get
			{
				return GetItem(nameof(ResubscribeLabel), () => new MultilingualStringRegistryItem(
					nameof(ResubscribeLabel),
					Categories.SalesMarketing_CampaignManagement_Unsubscribe,
					ResString.GetMultilingualString("D7D4BC47-9F42-4761-BDB8-8F9C7BD11A28", "Re-subscribe Label"),
					ResString.GetMultilingualString("94A337CC-85A7-4CE4-BEFE-EFEF97F9AE36", "Allows you to change the message appearing on the Auto Unsubscribe web page for a user to resubscribe."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					ResString.GetMultilingualString("1AB98F51-4183-4BEA-A747-92C4D8D07A7B", "Re-subscribe.")));
			}
		}

		public SubscriptionRulesRegistryItem SubscriptionRules
		{
			get
			{
				return GetItem("SubscriptionRules", delegate
				{
					return new SubscriptionRulesRegistryItem(
						"SubscriptionRules",
						Categories.SalesMarketing_CampaignManagement_SubscripitionPreference,
						ResString.GetMultilingualString("c903e2b0-3052-419a-b366-a7d16f201091", "Published Subscription Lists"),
						ResString.GetMultilingualString("069b39c4-2ae9-45e5-8e11-e5e9e11a0086", "Create subscription list preferences which are to be published on the Email Subscription preference page. Lists are displayed per Campaign configuration."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company);
				});
			}
		}

		#endregion

		#endregion

		#region Contact Job Category Mandatory

		public bool ContactJobCategoryMandatory
		{
			get { return ContactJobCategoryMandatoryRaw.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { ContactJobCategoryMandatoryRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		internal BooleanRegistryItem ContactJobCategoryMandatoryRaw
		{
			get
			{
				return GetItem("ContactJobCategoryMandatory", delegate
				{
					return new BooleanRegistryItem(
						"ContactJobCategoryMandatory",
						Categories.Organizations_OrganizationRequiredFields,
						ResString.GetMultilingualString("73054a0d-2409-4237-a65f-16fa4e22798c", "Contact Job Category Mandatory"),
						ResString.GetMultilingualString("749593b3-9be4-411f-9e1c-0555b1dc66e2", "Specifies whether the Job Category field on each Contact is required to be filled in."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#endregion

		#region Org Code Algorithms

		public OrgCodeAlgorithmRegistryItem OrgCodeAlgorithmDefault
		{
			get
			{
				return GetItem("OrgCodeAlgorithmDefault", delegate
				{
					return new OrgCodeAlgorithmRegistryItem("OrgCodeAlgorithmDefault",
						Categories.Organizations_Codes,
						ResString.GetMultilingualString("2e451a0c-27bf-44b0-bbed-9b28b1987d2c", "Organization Code - Default Set"),
						ResString.GetMultilingualString("994ce7aa-e949-4cd9-862d-3d4d6c077fcc", "Specify the default algorithm used to generate the code for all organizations. This will apply to all organizations that are not nominated in 'Organization Code - Override Set'."),
						RegistryOptions.PreserveTestValue,
						GetDefaultOrgCodeAlgorithm(OrgCodeAlgorithmType.Default));
				});
			}
		}

		public OrgCodeAlgorithmRegistryItem OrgCodeAlgorithmOverride
		{
			get
			{
				return GetItem("OrgCodeAlgorithmOverride", delegate
				{
					return new OrgCodeAlgorithmRegistryItem("OrgCodeAlgorithmOverride",
						Categories.Organizations_Codes,
						ResString.GetMultilingualString("9229c214-2ee3-48a6-b2b5-e147954e0c73", "Organization Code - Override Set"),
						ResString.GetMultilingualString("9bb4e6e2-047c-4358-897f-bb8b879d41a9", "Specify the algorithm used to generate the code for all organizations that are of the types nominated."),
						RegistryOptions.PreserveTestValue,
						GetDefaultOrgCodeAlgorithm(OrgCodeAlgorithmType.Override));
				});
			}
		}

		OrgCodeAlgorithm GetDefaultOrgCodeAlgorithm(OrgCodeAlgorithmType algorithmType)
		{
			OrgCodeAlgorithm result = new OrgCodeAlgorithm();
			result.AlgorithmType = algorithmType;
			result.RegenerateOrgCodeOnChanges = true;
			result.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			result.Elements[OrgCodeElementDescription.FirstName].Length = 3;
			result.Elements[OrgCodeElementDescription.SecondName].Order = 2;
			result.Elements[OrgCodeElementDescription.SecondName].Length = 3;
			result.Elements[OrgCodeElementDescription.IataCode].Order = 3;
			return result;
		}

		#endregion

		#region Denied Party Screening

		#region SuppressResourceStringsCheckRegion

		public DpsWebServiceItemCollectionRegistryItem DeniedPartyScreeningWebService
		{
			get
			{
				return GetItem("DeniedPartyScreeningWebService", delegate
				{
					var webServiceUrl = new DpsWebServiceItemCollection();

					return new DpsWebServiceItemCollectionRegistryItem(
						"DeniedPartyScreeningWebService",
						Categories.Organizations_DeniedPartyScreening_WebService,
						(NoResString)"Web Service URLs",
						(NoResString)"Specify the URLs that Denied Party Screening will use when making requests. One URL must be marked for service task use.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						webServiceUrl.DefaultValue);
				});
			}
		}

		#endregion

		public GuidRegistryItem DeniedPartyRescreenErrorNotificationGroup
		{
			get
			{
				return GetItem("DeniedPartyRescreenErrorNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"DeniedPartyRescreenErrorNotificationGroup",
						Categories.Organizations_DeniedPartyScreening,
						ResString.GetMultilingualString("5E007612-F62C-4CCF-975C-B91BE3AE99BD", "Denied Party Error Notification Group"),
						ResString.GetMultilingualString("66082649-76BB-4E23-A87E-999EC5EC5184", "The staff group that will receive notifications about execution or configuration errors with DPS."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						Core.Constants.Groups.PostMastersGroupPK);
					return result;
				});
			}
		}

		public RequireReasonForCLRRegistryItem DeniedPartyScreeningRequireReasonForJobClearing
		{
			get
			{
				return GetItem("DeniedPartyScreeningRequireReasonForJobClearing", delegate
				{
					return new RequireReasonForCLRRegistryItem
					(
						"DeniedPartyScreeningRequireReasonForJobClearing",
						Categories.Organizations_DeniedPartyScreening,
						ResString.GetMultilingualString("B1237B12-81E7-4C6C-BEE3-34966E4FE6E9", "Require Reason For Job CLR"),
						ResString.GetMultilingualString("409D9E44-4D00-4479-B2AA-300F4E2042B2", "If this registry is set to YES the user will be forced to write a comment before marking a record to job clear."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						new RequireReasonForCLRWrapper(new RequireReasonForCLRItemCollection
						{
							new RequireReasonForCLRItem
							{
								Code = Constants.RequireReasonForCLRRegistryConstants.Code.Default,
								Title = Constants.RequireReasonForCLRRegistryConstants.Description.Default,
								ClearingReason = string.Empty,
								IsMandatory = true
							}
						})
						{
							RequireReasonForCLR = true
						});
				});
			}
		}

		public RequireReasonForCLRRegistryItem DeniedPartyScreeningRequireReasonForClearing
		{
			get
			{
				return GetItem("DeniedPartyScreeningRequireReasonForClearing", delegate
				{
					return new RequireReasonForCLRRegistryItem
					(
						"DeniedPartyScreeningRequireReasonForClearing",
						Categories.Organizations_DeniedPartyScreening,
						ResString.GetMultilingualString("0719e3d7-a85a-4ead-b8e6-969fbe5866c3", "Require Reason For CLR"),
						ResString.GetMultilingualString("10829bda-46d8-4aea-9836-f0d52a652283", "If this registry is set to YES the user will be forced to write a comment before clearing a record with potential Medium or High Confidence Denied Party Candidates."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						new RequireReasonForCLRWrapper
						{
							RequireReasonForCLR = false
						});
				});
			}
		}

		public BooleanRegistryItem DeniedPartyScreeningEnableLogWalkerServiceTaskToUpdateRelatedJobs
		{
			get
			{
				return GetItem("DeniedPartyScreeningEnableLogWalkerServiceTaskToUpdateRelatedJobs", delegate
				{
					return new BooleanRegistryItem
					(
						"DeniedPartyScreeningEnableLogWalkerServiceTaskToUpdateRelatedJobs",
						Categories.Organizations_DeniedPartyScreening,
						(NoResString)"Enable Log Walker Service Task to update related jobs' screening status",
						(NoResString)"When updating related jobs' screening status, if this registry is set to Yes, the process will be done by Log Walker Service Task in the background. If set to No, the process will occur in the front-end.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem DeniedpartyScreeningEnableJobClear
		{
			get
			{
				return GetItem("DeniedpartyScreeningEnableJobClear", delegate
				{
					return new BooleanRegistryItem
					(
						"DeniedpartyScreeningEnableJobClear",
						Categories.Organizations_DeniedPartyScreening,
						ResString.GetMultilingualString("ec74834e-9fda-ce97-434f-10ba2e573890", "Denied Party Screening Enable Job Clear"),
						ResString.GetMultilingualString("6a6c30ba-35c1-9ba4-45a0-aff895f6fde2", "When set to Yes, Job Clear Status become available."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		public IntRegistryItem DeniedPartyScreeningTimeoutForScreeningRequests
		{
			get
			{
				return GetItem("DeniedPartyScreeningTimeoutForScreeningRequests", delegate
				{
					return new IntRegistryItem
					(
						"DeniedPartyScreeningTimeoutForScreeningRequests",
						Categories.Organizations_DeniedPartyScreening_Timeouts,
						ResString.GetMultilingualString("CC6137C3-F948-4B0A-98D8-725443038784", "Screening Requests"),
						ResString.GetMultilingualString("DE4C4E23-4F50-4CA7-A716-415E630393B3", "Sets the timeout (in seconds) for a request, to the Denied Party Screening web service, to screen an individual record."),
						null,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						600,
						180,
						1200);
				});
			}
		}

		public IntRegistryItem DeniedPartyScreeningTimeoutForRescreeningServiceTask
		{
			get
			{
				return GetItem("DeniedPartyScreeningTimeoutForRescreeningServiceTask", delegate
				{
					return new IntRegistryItem
					(
						"DeniedPartyScreeningTimeoutForRescreeningServiceTask",
						Categories.Organizations_DeniedPartyScreening_Timeouts,
						ResString.GetMultilingualString("B0281E91-23DF-4332-A275-2AB79DF98A3E", "Re-screening Advice Manager"),
						ResString.GetMultilingualString("2F00943C-466C-4DE4-9603-CEA21D2AE787", "Sets the timeout (in seconds) for the Re-screening Advice Manager Service Task to get a response from the Denied Party Screening web service."),
						null,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						600,
						300,
						1800);
				});
			}
		}

		public IntRegistryItem DeniedPartySilentScreeningServiceTaskBatchSize
		{
			get
			{
				return GetItem("DeniedPartySilentScreeningServiceTaskBatchSize", delegate
				{
					return new IntRegistryItem
					(
						"DeniedPartySilentScreeningServiceTaskBatchSize",
						Categories.Organizations_DeniedPartyScreening,
						(NoResString)"Denied Party Silent Screening (DSS) Service Task Batch Size",
						(NoResString)"This registry item configures the batch size for the Denied Party Silent Screening (DSS) service task.",
						null,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						10,
						2,
						50);
				});
			}
		}

		public IntRegistryItem DeniedPartyRescreeningServiceTaskBatchSize
		{
			get
			{
				return GetItem("DeniedPartyRescreeningServiceTaskBatchSize", delegate
				{
					return new IntRegistryItem
					(
						"DeniedPartyRescreeningServiceTaskBatchSize",
						Categories.Organizations_DeniedPartyScreening,
						(NoResString)"Denied Party Rescreening (DPR) Service Task Batch Size",
						(NoResString)"This registry item configures the batch size for the Denied Party Rescreening (DPR) service task.",
						null,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						50,
						10,
						200);
				});
			}
		}

		public IntRegistryItem DPSSendScreeningDecisionServiceTaskBatchSize
		{
			get
			{
				return GetItem("DPSSendScreeningDecisionServiceTaskBatchSize", delegate
				{
					return new IntRegistryItem
					(
						"DPSSendScreeningDecisionServiceTaskBatchSize",
						Categories.Organizations_DeniedPartyScreening,
						(NoResString)"Denied Party Send Screening Decision (DPM) Service Task Batch Size",
						(NoResString)"This registry item configures the batch size for the Denied Party Send Screening Decision (DPM) service task.",
						null,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						50,
						10,
						200);
				});
			}
		}

		public IntRegistryItem DeniedPartyScreeningTimeoutForSQLCommandQuery
		{
			get
			{
				return GetItem("DeniedPartyScreeningTimeoutForSQLCommandQuery", delegate
				{
					return new IntRegistryItem
					(
						"DeniedPartyScreeningTimeoutForSQLCommandQuery",
						Categories.Organizations_DeniedPartyScreening_Timeouts,
						(NoResString)"SQL Command Query",
						(NoResString)"Sets the timeout (in seconds) for SQL command query execution.",
						null,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						600,
						180,
						3600);
				});
			}
		}

		ResourceString MatchingConfidenceThresholdsDescription => ResString.GetMultilingualString("73D9AAE0-A91B-493A-B1DB-711913E085A7", @"This registry sets the minimum thresholds for a match to be considered a Medium or High Confidence match when returned from Denied Party Screening.
In order to ensure that the compliance aspects of Denied Party Screening are not compromised, there are some limitations to the configurations that can be made:
- Medium threshold must be between {0}% and {1}%,
- High threshold must be between {2}% and {3}%,
- There must be a difference of at least {4}% between the Medium and High thresholds.",
		DpsConfidenceThresholdsBusinessObject.Constants.MediumThresholdMinimum, DpsConfidenceThresholdsBusinessObject.Constants.MediumThresholdMaximum,
		DpsConfidenceThresholdsBusinessObject.Constants.HighThresholdMinimum, DpsConfidenceThresholdsBusinessObject.Constants.HighThresholdMaximum,
		DpsConfidenceThresholdsBusinessObject.Constants.ThresholdDifferenceMinimum);

		public DpsConfidenceThresholdsRegistryItem MatchingConfidenceThresholdsForVessels
		{
			get
			{
				return GetItem("MatchingConfidenceThresholdsForVessels", delegate
				{
					return new DpsConfidenceThresholdsRegistryItem
					(
						"MatchingConfidenceThresholdsForVessels",
						Categories.Organizations_DeniedPartyScreening_ComparisonSettings,
						ResString.GetMultilingualString("0CF5FFBB-1BBD-4BBD-9A3A-D1F7CF0DBD04", "Matching Confidence Thresholds For Vessels"),
						MatchingConfidenceThresholdsDescription,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						new DpsConfidenceThresholdsBusinessObject()
					);
				});
			}
		}

		public DpsConfidenceThresholdsRegistryItem MatchingConfidenceThresholdsForPersons
		{
			get
			{
				return GetItem("MatchingConfidenceThresholdsForPersons", delegate
				{
					return new DpsConfidenceThresholdsRegistryItem
					(
						"MatchingConfidenceThresholdsForPersons",
						Categories.Organizations_DeniedPartyScreening_ComparisonSettings,
						ResString.GetMultilingualString("F40E2D2A-DC91-4E11-8506-0618587F5A9C", "Matching Confidence Thresholds For Persons"),
						MatchingConfidenceThresholdsDescription,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						new DpsConfidenceThresholdsBusinessObject()
					);
				});
			}
		}

		public DpsConfidenceThresholdsRegistryItem MatchingConfidenceThresholdsForOrganisations
		{
			get
			{
				return GetItem("MatchingConfidenceThresholdsForOrganisations", delegate
				{
					return new DpsConfidenceThresholdsRegistryItem
					(
						"MatchingConfidenceThresholdsForOrganisations",
						Categories.Organizations_DeniedPartyScreening_ComparisonSettings,
						ResString.GetMultilingualString("38A6FF84-0D6E-445A-88DE-D853C3DFE74B", "Matching Confidence Thresholds For Organizations"),
						MatchingConfidenceThresholdsDescription,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						new DpsConfidenceThresholdsBusinessObject()
					);
				});
			}
		}

		public DpsConfidenceThresholdsBusinessObject GetDPSMatchingConfidenceThreshold(string nameType)
		{
			DpsConfidenceThresholdsBusinessObject dpsConfidenceThresholdsBusinessObject;

			switch (nameType)
			{
				case DeniedPartyScreening.Common.DeniedPartyConstants.ScreeningNameTypes.Organization:
					dpsConfidenceThresholdsBusinessObject = MatchingConfidenceThresholdsForOrganisations.Value;
					break;
				case DeniedPartyScreening.Common.DeniedPartyConstants.ScreeningNameTypes.Person:
				case DeniedPartyScreening.Common.DeniedPartyConstants.ScreeningNameTypes.NaturalPerson:
					dpsConfidenceThresholdsBusinessObject = MatchingConfidenceThresholdsForPersons.Value;
					break;
				case DeniedPartyScreening.Common.DeniedPartyConstants.ScreeningNameTypes.Vessel:
					dpsConfidenceThresholdsBusinessObject = MatchingConfidenceThresholdsForVessels.Value;
					break;
				default:
					dpsConfidenceThresholdsBusinessObject = new DpsConfidenceThresholdsBusinessObject();
					break;
			}

			return dpsConfidenceThresholdsBusinessObject;
		}

		public CodePairRegistryItem DPSFreightMovementRestrictions
		{
			get
			{
				return GetItem(nameof(DPSFreightMovementRestrictions), delegate
				{
					var dpsFreightMovementRestrictionsOptionsListProvider = new CodeDescriptionPairListProvider(() => new DPSFreightMovementRestrictionsOptions());

					return new CodePairRegistryItem(
						nameof(DPSFreightMovementRestrictions),
						Categories.Organizations_DeniedPartyScreening,
						ResString.GetMultilingualString("4ad44925-4fba-48b7-8d4e-d67068b67524", "Enable DPS Freight Movement Restrictions"),
						ResString.GetMultilingualString("a1ff2274-8622-46aa-9ad9-0f8389f3266f", "Enable/disable the DPS-specific restrictions."),
						dpsFreightMovementRestrictionsOptionsListProvider,
						false,
						true,
						new ComboBoxRegistryEditorInfo(dpsFreightMovementRestrictionsOptionsListProvider, true),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						DPSFreightMovementRestrictionsOptions.Codes.No,
						false);
				});
			}
		}

		public StringArrayRegistryItem NameSeparators
		{
			get
			{
				return GetItem("OrganisationsNameSeparators", delegate
				{
					var item = new StringArrayRegistryItem(
						"OrganisationsNameSeparators",
						Categories.Organizations_DeniedPartyScreening,
						ResString.GetMultilingualString("B44CBD7B-9B7E-4261-9C12-DFB7F4F134A3", "Name Separators"),
						ResString.GetMultilingualString("8C950C7A-E800-4F46-90A5-2B0DEA1893A3", "This Registry defines the separators that Denied Party Screening is able to recognize in order to screen parties on both sides of it as separate names."),
						RegistryStorageFlags.System,
						new string[] { "C/O", "T/A", "DBA", "O/A", "C/" });
					item.DataType.MaximumLength = 20;
					item.DataType.CharacterCase = CharacterCase.Normal;
					return item;
				});
			}
		}

		public RequireReasonForCLRRegistryItem ComplianceRiskOverrideDecisionReason
		{
			get
			{
				return GetItem("ComplianceRiskOverrideDecisionReason", delegate
				{
					return new RequireReasonForCLRRegistryItem
					(
						"ComplianceRiskOverrideDecisionReason",
						Categories.Compliance,
						ResString.GetMultilingualString("F8C1AFCD-0EEB-4014-AE2A-69B7767C6F87", "Override Decision Reason"),
						ResString.GetMultilingualString("D3617D78-FBF5-4CCD-8164-71F28C2A82E5", "When set to Yes, users will be required to include a decision reason when Overriding a Job"),
						RegistryStorageFlags.System,
						RawDataRegistry.Instance.EnableComplianceRisk.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						GetDefaultValue());
				});
			}
		}

		RequireReasonForCLRWrapper GetDefaultValue()
		{
			var requireReasonForJobClearing = DeniedPartyScreeningRequireReasonForJobClearing.Value;

			if (!requireReasonForJobClearing.RequireReasonForCLR)
			{
				requireReasonForJobClearing = new RequireReasonForCLRWrapper(new RequireReasonForCLRItemCollection
				{
					new RequireReasonForCLRItem
					{
						Code = Constants.RequireReasonForCLRRegistryConstants.Code.Default,
						Title = Constants.RequireReasonForCLRRegistryConstants.Description.FreeText,
						ClearingReason = string.Empty,
						IsMandatory = true
					}
				})
				{
					RequireReasonForCLR = false
				};
			}

			return requireReasonForJobClearing;
		}

		public CodePairRegistryItem ComplianceRiskFreightMovementRestrictions
		{
			get
			{
				return GetItem(nameof(ComplianceRiskFreightMovementRestrictions), delegate
				{
					var freightMovementRestrictions = new CodeDescriptionPairListProvider(() => new DPSFreightMovementRestrictionsOptions());

					return new CodePairRegistryItem(
						"ComplianceRiskFreightMovementRestrictions",
						RawDataRegistry.Categories.Compliance,
						ResString.GetMultilingualString("99BF20E5-C758-48C5-A3E3-5363293F71B3", "Compliance Risk Freight Movement Restriction"),
						ResString.GetMultilingualString("C5A1B995-16AA-4A48-8A73-330D6C1CFADC", "Enable or disable Freight Movement Restrictions for Compliance Risk."),
						freightMovementRestrictions,
						false,
						true,
						new ComboBoxRegistryEditorInfo(freightMovementRestrictions, true),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RawDataRegistry.Instance.EnableComplianceRisk.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						DPSFreightMovementRestrictions.Value,
						false);
				});
			}
		}

		public BooleanRegistryItem EnableComplianceWarningMessage
		{
			get
			{
				return GetItem("EnableComplianceWarningMessage",
					() => new BooleanRegistryItem(
						"EnableComplianceWarningMessage",
						RawDataRegistry.Categories.Compliance,
						ResString.GetMultilingualString("E7303039-EBCC-4521-A12E-A409D571B336", "Enable Compliance Warning Message"),
						ResString.GetMultilingualString("5DF00D8B-F38F-422B-920D-8CDFD6D4BAD2", "Enable this registry to include a warning message on Jobs where the Job Compliance status is not Clear."),
						RegistryStorageFlags.System,
						RawDataRegistry.Instance.EnableComplianceRisk.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						false))
				;
			}
		}

		public IntRegistryItem ComplianceJobEndDateLimit
		{
			get
			{
				return GetItem("ComplianceJobEndDateLimit", delegate
				{
					return new IntRegistryItem(
						new RegistryItemImpl(
							"ComplianceJobEndDateLimit",
							RawDataRegistry.Categories.Compliance,
							ResString.GetMultilingualString("6015937F-E941-4721-8B4B-B074437E6191", "Job End Date"),
							ResString.GetMultilingualString("62F6366E-24EC-43E9-96C1-6002470089A1", "This registry specifies the number of days after a job is complete, that the compliance risk statuses will be automatically resynchronized when the job is opened. Manual resynchronization will still be possible."),
							new ComplianceJobEndDateLimitDateType(),
							new NumericRegistryEditorInfo(0),
							RegistryStorageFlags.System,
							RawDataRegistry.Instance.EnableComplianceRisk.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
							defaultValue: 7)
					);
				});
			}
		}

		public BooleanRegistryItem AllowComplianceCommodityRiskAssessment
		{
			get
			{
				var isCommodityScreeningEnable = ObjectFactory.Get<IComplianceWiseEnabledDetail>().IsComplianceCommodityScreeningEnable;

				return GetItem("AllowComplianceCommodityRiskAssessment",
						() => new BooleanRegistryItem(
							"AllowComplianceCommodityRiskAssessment",
							RawDataRegistry.Categories.Compliance,
							ResString.GetMultilingualString("458887F1-62CD-4985-BD53-CC52D07F337B", "Commodity Risk Assessment Decision"),
							ResString.GetMultilingualString("BD59AE1F-367D-4C0D-90F3-90B7AA548160", "When enabled, the commodity risk assessment is required on all jobs. When disabled, the commodity risk assessment is optional and unassessed commodities are non-blocking."),
							RegistryStorageFlags.System,
							isCommodityScreeningEnable ? RegistryOptions.Default : RegistryOptions.IsHidden,
							false))
					;
			}
		}

		public static class ComplianceWiseFeatureCodes
		{
			public const string GlobalCommercialInvoice = "CIV";
			public const string JobEntitiesCaching = "JEC";
		}

		public CodeDescriptionBoolRegistryItem ComplianceWiseFeatureDevelopment
		{
			get
			{
				return GetItem("ComplianceWiseFeatureDevelopment", delegate
				{
					return new CodeDescriptionBoolRegistryItem(
						"ComplianceWiseFeatureDevelopment",
						RawDataRegistry.Categories.Compliance,
						(NoResString)"ComplianceWise Feature Development",
						(NoResString)"This registry item should only be updated by the Master Data Product team.",
						RegistryStorageFlags.System,
						RawDataRegistry.Instance.EnableComplianceRisk.Value ? RegistryOptions.IsOnlyForSupport : (RegistryOptions.IsOnlyForSupport | RegistryOptions.IsHidden),
						new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Is Enabled", true, true),
						new CodeDescriptionBoolCollection
						{
							{ ComplianceWiseFeatureCodes.GlobalCommercialInvoice, (NoResString)"Global Commercial Invoice", false },
							{ ComplianceWiseFeatureCodes.JobEntitiesCaching, (NoResString)"Job Entities Caching", false }
						});
				});
			}
		}

		public bool ComplianceWiseFeatureDevelopmentEnabled(string code)
		{
			return RawDataRegistry.Instance.EnableComplianceRisk.Value && ComplianceWiseFeatureDevelopment.Value.GetBoolFromCode(code);
		}

		public IntRegistryItem ComplianceRiskAssessmentBatchSize =>
			GetItem("ComplianceRiskAssessmentBatchSize", () => new IntRegistryItem(
				"ComplianceRiskAssessmentBatchSize",
				RawDataRegistry.Categories.Compliance,
				(NoResString)"Compliance Risk Assessment (CRA) Batch Size",
				(NoResString)"This registry item configures the batch size for the Compliance Risk Assessment (CRA) Service Task.",
				RegistryStorageFlags.System,
				RawDataRegistry.Instance.EnableComplianceRisk.Value
					? RegistryOptions.IsOnlyForSupport
					: (RegistryOptions.IsOnlyForSupport | RegistryOptions.IsHidden),
				10, 1, 1000));

		public static bool ScreeningStatusNotClear(ZString screeningStatus)
		{
			return (
				screeningStatus != ScreeningStatusesList.Codes.Clear &&
				screeningStatus != ScreeningStatusesList.Codes.PermanentClear &&
				screeningStatus != ScreeningStatusesList.Codes.JobCleared &&
				screeningStatus != ScreeningStatusesList.Codes.Release &&
				screeningStatus != ScreeningStatusesList.Codes.JobClearedExternal
			);
		}

		public bool IsDPSFreightMovementRestricted(ZString screeningStatus, ZBool isExport, ZBool isImport)
		{
			bool result = false;
			var restriction = this.DPSFreightMovementRestrictions.Value;

			if (restriction == DPSFreightMovementRestrictionsOptions.Codes.All || (restriction == DPSFreightMovementRestrictionsOptions.Codes.Exp && isExport) || (restriction == DPSFreightMovementRestrictionsOptions.Codes.Int && (isImport || isExport)))
			{
				result = ScreeningStatusNotClear(screeningStatus);
			}

			return result;
		}

		public StringRegistryItem DeniedPartyScreeningUserTerms
		{
			get
			{
				return GetItem("DeniedPartyScreeningUserTerms", delegate
				{
					string localTerm = new EmbeddedResourceRetriever().GetString("Enterprise.Registry.Business.Organisations.DeniedPartyScreeningUserTerms.txt", Encoding.UTF8);
					return new StringRegistryItem
					(
						"DeniedPartyScreeningUserTerms",
						Categories.Organizations_DeniedPartyScreening,
						(NoResString)"User Terms",
						(NoResString)"WiseTech Denied Party Screening Service User Terms.",
						new StringRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.Memo),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
						localTerm);
				});
			}
		}

		public IntRegistryItem JobUpdatePeriod
		{
			get
			{
				return GetItem("JobUpdatePeriod", delegate
				{
					return new IntRegistryItem
					(
						new RegistryItemImpl(
							"JobUpdatePeriod",
							Categories.Organizations_DeniedPartyScreening,
							ResString.GetMultilingualString("8FC874FE-01CE-4970-B651-481928874C13", "Job Update Period"),
							ResString.GetMultilingualString("75952257-DA61-46BB-AA63-FF8F87434736", "Specify the job update period (months)."),
							new JobUpdatePeriodRegistryDataType(),
							new NumericRegistryEditorInfo(0),
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							3)
					);
				});
			}
		}

		public DpsMatchingConfigurationRegisrtyItem DeniedPartyScreeningMatchingConfiguration
		{
			get
			{
				return GetItem("DeniedPartyScreeningMatchingConfiguration", delegate
				{
					return new DpsMatchingConfigurationRegisrtyItem
					(
						"DeniedPartyScreeningMatchingConfiguration",
						Categories.Organizations_DeniedPartyScreening,
						ResString.GetMultilingualString("644C04A5-1731-449A-A282-6B7E1F448F9B", "Matching Configuration"),
						ResString.GetMultilingualString("EE495B23-DDD1-4B4B-89DE-F86AD3AE151A", "This setting controls the strictness of matching used by the Denied Party Screening Service. There are three settings available."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						new DpsMatchingConfigurationBusinessObject());
				});
			}
		}

		public AddressMatchingLevelRegisrtyItem AddressMatchingLevel
		{
			get
			{
				var featureData = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(CargoWise.Definitions.LicenceFeatureCodeList.Codes.DpsAddressMatchingLevelFeature);

				return GetItem("AddressMatchingLevel", delegate
				{
					return new AddressMatchingLevelRegisrtyItem
					(
						"AddressMatchingLevel",
						Categories.Organizations_DeniedPartyScreening,
						ResString.GetMultilingualString("CCD6944D-E610-4BE5-878D-49C437DE0666", "Address Matching Level"),
						ResString.GetMultilingualString("5ECD308F-00A5-4E9C-8B24-32DF3D5604F7", "This setting controls the strictness of address matching logic in Denied Party Screening service. \r\nThere are three levels available."),
						RegistryStorageFlags.System,
						featureData != null ? RegistryOptions.Default : RegistryOptions.IsHidden,
						new AddressMatchingLevelBusinessObject());
				});
			}
		}

		public MDMSupportCertificateRegistryItem MDMSupportCertificateForDPS
		{
			get
			{
				return GetItem("MDMSupportCertificateForDPS", delegate
				{
					return new MDMSupportCertificateRegistryItem(
					"MDMSupportCertificateForDPS",
					Categories.Organizations_DeniedPartyScreening,
					(NoResString)"MDM Support Certificate For DPS",
					(NoResString)"With the System To System Trust Authentication option turned on for Denied Party Screen Web Service uri, if a MDM Support Certificate exists, it takes precedence.",
					MDMProductCodes.DPS);
				});
			}
		}

		public BooleanRegistryItem EnableExternalOverrideJobScreeningStatuses
		{
			get
			{
				return GetItem("DeniedPartyScreeningEnableExternalOverrideJobScreeningStatuses", delegate
				{
					return new BooleanRegistryItem(
						"DeniedPartyScreeningEnableExternalOverrideJobScreeningStatuses",
						Categories.Organizations_DeniedPartyScreening,
						(NoResString)"Enable External Screening Status Override",
						(NoResString)"When enabled, a Standalone Custom Declaration job's screening status can be set to Job Cleared Externally (JCE) or Job Blocked Externally (JBE) through an XML Universal Event (XUE).",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region ComplianceWiseCustoms

		public BooleanRegistryItem CustomsShowImportAlertsOnExportDeclarations
		{
			get
			{
				var isVisible = ObjectFactory.Get<IComplianceWiseEnabledDetail>().IsCustomsEnabledComplianceWise;

				return GetItem("CustomsShowImportAlertsOnExportDeclarations",
					() => new BooleanRegistryItem(
						"CustomsShowImportAlertsOnExportDeclarations",
						RawDataRegistry.Categories.Compliance_Customs,
						ResString.GetMultilingualString("1E52DA0C-5717-4915-B019-CF527F1889A4", "Show Import Alerts on Export Declarations"),
						ResString.GetMultilingualString("4CAE3275-DC56-4136-AA00-399307EA6235", @"When enabled, import alerts will be visible on standalone customs export declarations jobs.
When disabled, import alerts will be not visible on standalone customs export declarations jobs."),
						RegistryStorageFlags.System,
						isVisible ? RegistryOptions.Default : RegistryOptions.IsHidden,
						true));
			}
		}

		#endregion

		#region CountryDefaultLanguage

		public CountryDefaultLanguageBusinessObjectCollectionRegistryItem CountryDefaultLanguage
		{
			get
			{
				return GetItem("CountryDefaultLanguage", delegate
				{
					return new CountryDefaultLanguageBusinessObjectCollectionRegistryItem(
						"CountryDefaultLanguage",
						Categories.Organizations_DefaultValues,
						ResString.GetMultilingualString("71D37619-7FAD-4119-91C7-C174CE63D27B", "Country/Region Default Language"),
						ResString.GetMultilingualString("E6557FD7-310A-4B00-AC06-D1ABDC4EA97A", "Specify the default language for countries/regions."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						new CountryDefaultLanguageBusinessObjectCollection());
				});
			}
		}

		public bool TryGetDefaultLanguage(ZGuid countryPK, out ZString defaultLanguage)
		{
			var result = false;
			defaultLanguage = ZString.Empty;

			var language = Instance.CountryDefaultLanguage.Value.FirstOrDefault(x => ((CountryDefaultLanguageBusinessObject)x).CountryPk == countryPK);
			if (language != null)
			{
				result = true;
				defaultLanguage = ((CountryDefaultLanguageBusinessObject)language).DefaultLanguage;
			}

			return result;
		}

		#endregion

		#region OrgPatternMatchCountLimit
		#region SuppressResourceStringsCheckRegion

		public IntRegistryItem OrgPatternMatchLimit
		{
			get
			{
				return GetItem("OrgPatternMatchLimit", delegate
				{
					return new IntRegistryItem(
						"OrgPatternMatchLimit",
						Categories.Organizations_PatternMatch,
						(NoResString)"Org. Pattern Match Limit",
						(NoResString)"The maximum number of organization pattern match records to examine during organization matching. Adjust this number lower if system is getting out of memory problems or is too slow. Adjust this number higher if system is not able to match certain organizations when there are large number of duplicates or similar organizations. Write 0 (zero) for unlimited maximum number.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						0);
				});
			}
		}

		#endregion
		#endregion

		#region Buyer Supplier Relationships

		public static class BuyerSupplierRelationshipFields
		{
			public const string ImportBroker = "IMP";
			public const string DefaultCurrency = "CUR";
			public const string Incoterm = "INC";
			public const string ContainerMode = "CMO";
			public const string OriginalBills = "OBL";
			public const string CopyBills = "CBL";
		}

		public CodeDescriptionBoolRegistryItem BuyerSupplierRelationshipFieldsToExclude
		{
			get
			{
				return GetItem("BuyerSupplierRelationshipFieldsToExclude", delegate
				{
					return new CodeDescriptionBoolRegistryItem(
						"BuyerSupplierRelationshipFieldsToExclude",
						Categories.Organizations_BuyerSupplierRelationships,
						ResString.GetMultilingualString("f7e3becb-1375-45a7-87ac-06e5b9bf9eeb", "Fields To Exclude"),
						ResString.GetMultilingualString("1a97b6c5-9337-4121-b7b3-535c121ea1f1", "Use this Registry setting to specify fields not to be added to the Buyer/Supplier Relationship when it’s first created from the job."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("1f79e1c7-f865-4f61-9008-069b8f9dbbd4", "Exclude?"), true, true),
						GetBuyerSupplierFieldsToExcludeDefaultValue());
				});
			}
		}

		CodeDescriptionBoolCollection GetBuyerSupplierFieldsToExcludeDefaultValue()
		{
			return new CodeDescriptionBoolCollection
			{
				{ BuyerSupplierRelationshipFields.ImportBroker, ResString.GetMultilingualString("6ed9c408-98b5-4086-a350-82d460014850", "Import Broker"), false },
				{ BuyerSupplierRelationshipFields.DefaultCurrency, ResString.GetMultilingualString("3282ede6-72ca-4a9e-aef4-512b46daca38", "Default Currency"), false },
				{ BuyerSupplierRelationshipFields.Incoterm, ResString.GetMultilingualString("a69c8cc2-7a5a-4787-9d40-d289e2996e7c", "Incoterm"), false },
				{ BuyerSupplierRelationshipFields.ContainerMode, ResString.GetMultilingualString("1961aeb9-79fa-43a4-99af-0eaefb3189d9", "Container Mode"), false },
				{ BuyerSupplierRelationshipFields.OriginalBills, ResString.GetMultilingualString("cb0aa885-2e4e-4ab2-a919-5a2580cd14ea", "Original Bills"), false },
				{ BuyerSupplierRelationshipFields.CopyBills, ResString.GetMultilingualString("1afd0f17-cd1f-4a86-9e54-83264e864326", "Copy Bills"), false },
			};
		}

		#endregion

		#region Contact Salutation Gender/Type

		public ContactSalutationRegistryItem ContactSalutation
		{
			get
			{
				return GetItem("ContactSalutation", delegate
				{
					var defaultValue = new ContactSalutationCollection();
					SalutationHelper.LoadDefaultSalutations(defaultValue);
					return new ContactSalutationRegistryItem(
						"ContactSalutation",
						Categories.Organizations,
						ResString.GetMultilingualString("1324c5a2-dbcc-4533-b5fd-0d215be60239", "Contact Salutation Gender/Type"),
						ResString.GetMultilingualString("95b21bc9-5bd8-4584-a479-f18f0181a738", "Additional Salutation Gender (Female/Male)/Relationship Type (Friendly or Formal) option has been added to organization Contact to identify the correct greeting for system defined documents, and can be set per specific language."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						defaultValue);
				});
			}
		}

		#endregion

		#region eTail

		#region Depot Address Color/Sound

		public DepotAddressColorSoundRegistryItem DepotAddressColorSound
		{
			get
			{
				return GetItem("DepotAddressColorSound", delegate
				{
					DepotAddressColorSoundRegistryItem result = new DepotAddressColorSoundRegistryItem(
						"DepotAddressColorSound",
						Categories.Organizations_eTailDepot,
						ResString.GetMultilingualString("678FFB6A-4E91-425B-81B8-768BAA0DBED4", "Depot Address Color/Sound Tags"),
						ResString.GetMultilingualString("C9691271-9662-4A07-92C7-3E0D97BF5F62", "Assign a color and .wav and .mp3 sound files to a depot's address."),
						RegistryStorageFlags.System);
					return result;
				});
			}
		}

		#endregion

		#region Organisation Barcode Mask

		public OrgBarcodeMaskRegistryItem OrgBarcodeMask
		{
			get
			{
				return GetItem("OrgBarcodeMask", delegate
				{
					OrgBarcodeMaskRegistryItem result = new OrgBarcodeMaskRegistryItem(
						"OrgBarcodeMask",
						Categories.Organizations_eTailDepot,
						ResString.GetMultilingualString("d3962c70-0220-4fa7-87a9-63f27514ad0b", "Organization Barcode Mask"),
						ResString.GetMultilingualString("b289bb30-c2dd-4f71-892c-4bfdde79cf16", @"For eTail Origin and Destination Depot scanning via eTail Portal, this mask can be applied to barcodes scanned in order to lift out actual consignment reference from the barcode. e.g. eParcel barcode in Australia can be 29 characters including postcode, however, the actual consignment reference is only 16 characters and need to be lifted from the 29 character barcode using this mask. Masks can be applied in an order of priority (1 - 99), such that a given barcode can be checked against each mask in order for a given Depot, until the first mask to return a proper consignment reference.

Mask entries are required to be regular expressions, containing no more than one group. For further information on regular expressions, please consult the Microsoft Developer Network page on the Regular Expression Language."),
						RegistryStorageFlags.System);
					return result;
				});
			}
		}

		#endregion

		#endregion

		#region Shipment

		public GuidRegistryItem ImportAirBroker
		{
			get
			{
				return GetItem("ImportAirBroker", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"ImportAirBroker",
						Categories.Organizations_Shipment,
						ResString.GetMultilingualString("afe22fc3-37e1-4541-b7a0-bb063d458354", "Import Air Broker"),
						ResString.GetMultilingualString("AE8C5A04-3E54-4BE0-9D6E-4CFFC629E9DE", "The Organization selected below will default as the Import Air Broker in the Related Parties section when creating a new Organization record."),
						RegistryStorageFlags.Branch, RegistryOptions.IsValueOptional);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.Broker);
					return result;
				});
			}
		}

		public GuidRegistryItem ImportSeaBroker
		{
			get
			{
				return GetItem("ImportSeaBroker", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"ImportSeaBroker",
						Categories.Organizations_Shipment,
						ResString.GetMultilingualString("062a40ad-efa4-40c1-b0c8-492888307785", "Import Sea Broker"),
						ResString.GetMultilingualString("40C418B9-4091-4AE8-B25D-09C1402B709A", "The Organization selected below will default as the Import Sea Broker in the Related Parties section when creating a new Organization record."),
						RegistryStorageFlags.Branch, RegistryOptions.IsValueOptional);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.Broker);
					return result;
				});
			}
		}

		public GuidRegistryItem ExportAirBroker
		{
			get
			{
				return GetItem("ExportAirBroker", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"ExportAirBroker",
						Categories.Organizations_Shipment,
						ResString.GetMultilingualString("0d530421-13f1-4372-88ed-88522509d40d", "Export Air Broker"),
						ResString.GetMultilingualString("11D98832-8F0C-464E-86E7-FC1BB255728D", "The Organization selected below will default as the Export Air Broker in the Related Parties section when creating a new Organization record."),
						RegistryStorageFlags.Branch, RegistryOptions.IsValueOptional);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.Broker);
					return result;
				});
			}
		}

		public GuidRegistryItem ExportSeaBroker
		{
			get
			{
				return GetItem("ExportSeaBroker", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"ExportSeaBroker",
						Categories.Organizations_Shipment,
						ResString.GetMultilingualString("4e9263ce-34cf-4bd1-97b9-9c788a383ec3", "Export Sea Broker"),
						ResString.GetMultilingualString("84273E89-4865-4D8B-918F-43B1962FAFD2", "The Organization selected below will default as the Export Sea Broker in the Related Parties section when creating a new Organization record."),
						RegistryStorageFlags.Branch, RegistryOptions.IsValueOptional);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.Broker);
					return result;
				});
			}
		}

		#endregion

		#region Address Validation

		public AddressValidationDisabledCountryItemCollectionRegistryItem DisabledAddressValidationCountries
		{
			get
			{
				return GetItem("DisabledAddressValidationCountries", delegate
				{
					return new AddressValidationDisabledCountryItemCollectionRegistryItem(
						"DisabledAddressValidationCountries",
						Categories.Organizations_AddressValidationService,
						(NoResString)"Disabled Countries/Regions",
						(NoResString)"Disable one or more countries/regions from address validation process in the specific section.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport);
				});
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Switch use is justifiable")]
		public bool ShouldUseAddressValidation(Guid countryPK, AddressValidationSection section)
		{
			var shouldValidation = countryPK != Guid.Empty && EnvProxy.Instance.CurrentCompany != null && Env.Instance.Registry.EnableAddressValidationWebService;

			if (shouldValidation)
			{
				var disabledCountryItems = DisabledAddressValidationCountries.Value.Cast<AddressValidationDisabledCountryItem>();

				switch (section)
				{
					case AddressValidationSection.AdminPanel:
						shouldValidation = !disabledCountryItems.Any(u => u.CountryPK == countryPK && u.DisabledForAdminPanel);
						break;
					case AddressValidationSection.OrganizationAddress:
						shouldValidation = !disabledCountryItems.Any(u => u.CountryPK == countryPK && u.DisabledForOrgAddress);
						break;
					case AddressValidationSection.OverrideAddress:
						shouldValidation = !disabledCountryItems.Any(u => u.CountryPK == countryPK && u.DisabledForOverrideAddress);
						break;
					case AddressValidationSection.Applicant:
						shouldValidation = !disabledCountryItems.Any(u => u.CountryPK == countryPK && u.DisabledForApplicant);
						break;
					case AddressValidationSection.Branch:
						shouldValidation = !disabledCountryItems.Any(u => u.CountryPK == countryPK && u.DisabledForBranch);
						break;
					case AddressValidationSection.Company:
						shouldValidation = !disabledCountryItems.Any(u => u.CountryPK == countryPK && u.DisabledForCompany);
						break;
					case AddressValidationSection.Person:
						shouldValidation = !disabledCountryItems.Any(u => u.CountryPK == countryPK && u.DisabledForPerson);
						break;
					case AddressValidationSection.SalesInquiry:
						shouldValidation = !disabledCountryItems.Any(u => u.CountryPK == countryPK && u.DisabledForSalesInquiry);
						break;
					case AddressValidationSection.Staff:
						shouldValidation = !disabledCountryItems.Any(u => u.CountryPK == countryPK && u.DisabledForStaff);
						break;
					case AddressValidationSection.HVLVConsignment:
						shouldValidation = !disabledCountryItems.Any(u => u.CountryPK == countryPK && u.DisabledForHVLVConsignment);
						break;
					case AddressValidationSection.SupplierBookingLine:
						shouldValidation = !disabledCountryItems.Any(u => u.CountryPK == countryPK && u.DisabledForSupplierBookingLine);
						break;
				}
			}

			return shouldValidation;
		}

		public BooleanRegistryItem EnableErrorSuppression => GetItem(
			"EnableErrorSuppression",
			() => new BooleanRegistryItem(
				"EnableErrorSuppression",
				Categories.Organizations_AddressValidationService,
				(NoResString)"Enable Error Suppression",
				(NoResString)"Suppress address validation error per individual address.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false));

		public bool IsErrorSuppressionEnabled
		{
			get => EnableErrorSuppression.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			set => EnableErrorSuppression.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		public BooleanRegistryItem PreventUpdateWhenGettingPointExact => GetItem(
			"PreventUpdateWhenGettingPointExact",
			() => new BooleanRegistryItem(
				"PreventUpdateWhenGettingPointExact",
				Categories.Organizations_AddressValidationService,
				ResString.GetMultilingualString("A610A2A7-4303-4C3E-AA01-7C2311192B1E", "Prevent Automatic Standardization"),
				ResString.GetMultilingualString("F67CEFBD-599A-46EC-BFB2-222A747DFD5E", "Prevent automatic standardization on validated address."),
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false));

		public bool ShouldPreventUpdateWhenGettingPointExact
		{
			get => PreventUpdateWhenGettingPointExact.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			set => PreventUpdateWhenGettingPointExact.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		#endregion

		#region Address Validation Service

		internal AvsWebServiceUriRegistryItem AddressValidationWebServiceURIs
		{
			get
			{
				return GetItem("AddressValidationWebServiceURIs", ()
					=> new AvsWebServiceUriRegistryItem("AddressValidationWebServiceURIs",
						Categories.Organizations_AddressValidationService,
						(NoResString)"Address Validation Web Service URIs",
						(NoResString)"Address Validation Web Service URIs",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport));
			}
		}

		/// <summary>
		/// Do not reference this registry item directly. Please use AddressValidationServiceUrisProvider instead.
		/// </summary>
		public AvsWebServiceUriRegistryBusinessObjectCollection AvsWebServiceURIs
		{
			get => AddressValidationWebServiceURIs.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
		}

		public IntRegistryItem BackgroundValidationServiceTaskBatchSize
		{
			get
			{
				return GetItem("BackgroundValidationServiceTaskBatchSize", delegate
				{
					return new IntRegistryItem
					(
						"BackgroundValidationServiceTaskBatchSize",
						Categories.Organizations_AddressValidationService_Background,
						(NoResString)"Background Addresss Validation (BAV) Service Task Batch Size",
						(NoResString)"This registry item configures the batch size for the Background Addresss Validation (BAV) Service Task.",
						null,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						50,
						2,
						100);
				});
			}
		}

		#endregion

		#region Address Validation Service (Suppressions)

		public AddressListRegistryItem AddressValidationServiceSuppression
		{
			get
			{
				return GetItem("AddressValidationServiceSuppression", delegate
				{
					return new AddressListRegistryItem(
						"AddressValidationServiceSuppression",
						Categories.Organizations_AddressValidationService_Suppressions,
						(NoResString)"Disable Address Types for Controller",
						(NoResString)"Disable one or more address types from the address validation process for a Controller",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						new AddressListCollection());
				});
			}
		}

		#endregion

		#region OrgMatchUseDeduplication

		public BooleanRegistryItem OrgMatchUseDeduplication
		{
			get
			{
				return GetItem("OrgMatchUseDeduplication", delegate
				{
					return new BooleanRegistryItem(
						"OrgMatchUseDeduplication",
						Categories.Organizations_PatternMatch,
						ResString.GetMultilingualString("A02E751F-1888-45ED-9F29-EAD805D98D15", "Use Match Engine to Match Organization"),
						ResString.GetMultilingualString("A52398B3-8D29-4D52-9BCE-075CDCFF4DBA", "Set True to Use Match Engine to Match Organization."),
						RegistryStorageFlags.All,
						RegistryOptions.Default,
						false);
				});
			}
		}

		#endregion

		#region Match Engine Confidence

		#region OrgAddressMinimumConfidence

		public IntRegistryItem OrgAddressMinimumConfidence
		{
			get
			{
				return GetItem("OrgAddressMinimumConfidence", delegate
				{
					const int defaultValue = 80;
					const int minValue = 0;
					const int maxValue = 100;

					return new IntRegistryItem(
						"OrgAddressMinimumConfidence",
						Categories.Organizations_PatternMatch,
						ResString.GetMultilingualString("d32f0b89-40c7-4ce2-8269-e21ddfc2400b", "UXML Address Match Threshold"),
						ResString.GetMultilingualString("5f6968b8-4933-4d42-949c-280479491ae5", "When the Match Engine is enabled, as well as reaching the overall confidence level with the organization, the potential matches must also have the address confidence higher than the selected value."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.All,
						RegistryOptions.PreserveTestValue,
						defaultValue,
						minValue,
						maxValue);
				});
			}
		}

		#endregion

		#region UXMLOrganisationMinimumConfidence

		public IntRegistryItem UXMLOrganisationMinimumConfidence
		{
			get
			{
				return GetItem("UXMLOrganisationMinimumConfidence", delegate
				{
					return new IntRegistryItem(
						"UXMLOrganisationMinimumConfidence",
						Categories.Organizations_PatternMatch,
						ResString.GetMultilingualString("9903f480-1320-4b93-bdcd-114a719bc781", "UXML Organization Match Threshold"),
						ResString.GetMultilingualString("8c886781-9e45-4577-a31d-91be0186492d", "When the Match Engine is enabled, potential organization matches must have a confidence level higher than the selected percentage value."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						50,
						0, 100);
				});
			}
		}

		#endregion

		#endregion

		#region Added Info Panel

		public BooleanRegistryItem EnableAddedInfoPanel
		{
			get
			{
				return GetItem("EnableAddedInfoPanel", delegate
				{
					return new BooleanRegistryItem(
						"EnableAddedInfoPanel",
						Categories.Organizations_AddedInfoPanel,
						(NoResString)"Enable Added Info Panel",
						(NoResString)"Set True to Show Added Info Panel on Organization Form.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}
		#endregion

		#region Bulk Branch Status Update On Company Deactivation And Activation

		public BooleanRegistryItem BulkBranchStatusUpdateOnCompanyDeactivationAndActivation
		{
			get
			{
				return GetItem("BulkBranchStatusUpdateOnCompanyDeactivationAndActivation", delegate
				{
					return new BooleanRegistryItem(
						"BulkBranchStatusUpdateOnCompanyDeactivationAndActivation",
						Categories.Organizations,
						ResString.GetMultilingualString("d5f16b1e-2dff-c2af-48a6-3ebf20c19581", "Bulk Branch Status Update On Company Deactivation And Activation"),
						ResString.GetMultilingualString("bfdca373-e486-a89a-49f3-12265dde952f", @"This registry enables users to bulk activate branches when a company is activated. Also, this will automatically deactivate all linked branches if the company is deactivated.
Yes - the automatic deactivation of all branches when a company is deactivated is enabled.
Yes - bulk activation of branches when a company is activated is enabled."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#region Enable Controlling Contact Filters

		public BooleanRegistryItem EnableControllingContactFilters
		{
			get
			{
				return GetItem("EnableControllingContactFilters", delegate
				{
					return new BooleanRegistryItem(
						"EnableControllingContactFilters",
						Categories.Organizations,
						ResString.GetMultilingualString("42e38450-241a-f2b6-4aca-6301a3d036cb", "Enable Controlling Contact Filters"),
						ResString.GetMultilingualString("18d88f89-133f-9084-4b41-cbd18063c796", "When turned on, the Contact filter will be visible on the Organization > Contact page and its criteria will be enabled."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#region Competitor Type
		public OverrideImmuneCodeDescriptionBoolRegistryItem CompetitorType
		{
			get
			{
				return GetItem("CompetitorType", delegate
				{
					var defaultValue = new OverrideImmuneCodeDescriptionBoolCollection();
					foreach (CodeDescriptionPair pair in new CompetitorTypeList())
					{
						defaultValue.AddSystemDefined(pair.Code, pair.MultilingualDescription, true);
					}

					return new OverrideImmuneCodeDescriptionBoolRegistryItem(
							"CompetitorType",
							Categories.Organizations_Sales,
							ResString.GetMultilingualString("AA17158E-A2BD-4338-969C-C5F70C2EFD80", "Competitor Type"),
							ResString.GetMultilingualString("FE7EA744-187C-4A5F-8821-8F7E6CA9648E", "The List of Competitor types that can be set on each Organization."),
							RegistryStorageFlags.System,
							new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("fd6f6bd8-6d15-48b4-be81-764538a9b5eb", "Enabled")),
							defaultValue);
				});
			}
		}

		#endregion

		public CodeDescriptionBoolRegistryItem ReceivablesCreditAgreedPaymentMethodsList
		{
			get
			{
				return GetItem("ReceivablesCreditAgreedPaymentMethods", () =>
				{
					var item = new CodeDescriptionBoolRegistryItem("ReceivablesCreditAgreedPaymentMethods",
						Categories.Organizations_CodeLists,
						ResString.GetMultilingualString("1a423a07-eebe-41e5-b6fd-9911fd6fb38a", "Receivables Credit Agreed Payment Methods"),
						ResString.GetMultilingualString("E9D44D82-E2BE-4D1B-912D-A69287174CBC", @"The list of valid payment methods that can be assigned against a Receivables Organization to record the a settlement method a customer has agreed as part of their credit arrangements.
By default, only transactions with payment method CRQ - Collection Request can be added to a collection batch. Use the 'Collection Batch Type' flag to indicate which payment methods can be added to a Collection Batch."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("01234567-89ab-cdef-0123-456789abcdef", "Collection Batch type"), isBoolColumnVisible: true),
						ReceivablesCreditAgreedPaymentMethodsDefaultValueGetter);
					item.OnBuildLogReference += RawDataRegistry.FindDeletedItemsForCodeDescriptionPairList;
					return item;
				});
			}
		}

		static object ReceivablesCreditAgreedPaymentMethodsDefaultValueGetter(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var result = new CodeDescriptionBoolCollection
			{
				{ OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck, OrgDescriptions.CreditAgreedPaymentMethods.BusinessCheck, false },
				{ OrgConstants.CreditAgreedPaymentMethods.Code.CreditCard, OrgDescriptions.CreditAgreedPaymentMethods.CreditCard, false },
				{ OrgConstants.CreditAgreedPaymentMethods.Code.BankTransfer, OrgDescriptions.CreditAgreedPaymentMethods.BankTransfer, false },
				{ OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck, OrgDescriptions.CreditAgreedPaymentMethods.CashAndBankCheck, false },
				{ OrgConstants.CreditAgreedPaymentMethods.Code.DebitCard, OrgDescriptions.CreditAgreedPaymentMethods.DebitCard, false },
				{ OrgConstants.CreditAgreedPaymentMethods.Code.CollectionRequest, OrgDescriptions.CreditAgreedPaymentMethods.CollectionRequest, true },
			};

			if (companyPK != Guid.Empty && ObjectFactory.Get<IAccounting>().IsEPaymentFunctionalityEnabledForAnyProvider(companyPK))
			{
				result.Add(OrgConstants.CreditAgreedPaymentMethods.Code.EPayment, OrgDescriptions.CreditAgreedPaymentMethods.EPayment, false);
			}

			return result;
		}
	}
}
