using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	[TestedType(typeof(BarcodeRuleSet))]
	class BarcodeRuleSetTest : BarcodeParsingBusinessObjectTestCase
	{
		#region Related Entities

		#region TestRelatedEntity

		public void TestRelatedEntity()
		{
			var ruleSet = Helper.CreateRuleSet();
			ruleSet.BRS_Module = DummyBarcodeParsingConsumer.Module;
			AssertNull("No Related Entity foreign key set, should be no Related Entity.", ruleSet.RelatedEntity);

			var dummy = DummyBarcodeParsingConsumer.GetDummy(Factory);
			dummy.RelatedEntityList = new GlbCompanyCollection(Factory);
			ruleSet.BRS_RelatedEntityId = GlbCompany.CurrentCompany.PK;
			AssertEquals("Setting the Related Entity foreign key should set the table code, so 'RelatedEntity' should not be null.", GlbCompany.CurrentCompany.PK, ruleSet.RelatedEntity.PK);
		}

		#endregion

		#region TestRules

		public void TestRules()
		{
			var ruleSet = Helper.CreateRuleSet();
			AssertEquals(typeof(BarcodeRuleCollection), ruleSet.Rules.GetType());
			AssertEquals(true, ruleSet.IsRegisteredEditableChildObject(ruleSet.Rules));
		}

		#endregion

		#region TestValidationRules

		public void TestValidationRules()
		{
			var ruleSet = Helper.CreateRuleSet();
			AssertEquals(typeof(BarcodeValidationRuleCollection), ruleSet.ValidationRules.GetType());
			AssertEquals(true, ruleSet.IsRegisteredEditableChildObject(ruleSet.ValidationRules));
		}

		#endregion

		#endregion

		#region Properties

		// persistent

		#region TestBRS_IsSystemInfo

		public void TestBRS_IsSystemInfo()
		{
			AssertEquals(true, Helper.CreateRuleSet().BRS_IsSystemInfo.ReadOnly);
		}

		#endregion

		#region TestBRS_Module

		public void TestBRS_Module()
		{
			var ruleSet = Helper.CreateRuleSet();
			ruleSet.BRS_Module = DummyBarcodeParsingConsumer.Module;

			var dummy = DummyBarcodeParsingConsumer.GetDummy(Factory);
			dummy.RelatedEntityList = new GlbCompanyCollection(Factory);
			ruleSet.BRS_RelatedEntityId = GlbCompany.CurrentCompany.PK;
			AssertEquals("Precondition - Related Entity PK should be set.", GlbCompany.CurrentCompany.PK, ruleSet.BRS_RelatedEntityId);

			ruleSet.BRS_Module = "";
			AssertEquals("When changing module, Related Entity PK should be cleared.", ZGuid.Empty, ruleSet.BRS_RelatedEntityId);
		}

		#endregion

		#region TestBRS_OH_Buyer

		public void TestBRS_OH_Buyer()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var buyer2 = Helper.CreateOrg("Buyer2");
			var ruleSet = Helper.CreateRuleSet(buyer1);
			ruleSet.BRS_Module = DummyBarcodeParsingConsumer.Module;

			var dummy = DummyBarcodeParsingConsumer.GetDummy(Factory);
			dummy.IsRelatedEntityAvailable = true;
			dummy.RelatedEntityList = new GlbCompanyCollection(Factory);
			ruleSet.BRS_RelatedEntityId = GlbCompany.CurrentCompany.PK;
			AssertEquals("Precondition - Related Entity PK should be set.", GlbCompany.CurrentCompany.PK, ruleSet.BRS_RelatedEntityId);

			ruleSet.BRS_OH_Buyer = ZGuid.Empty;
			AssertEquals("The Related Entity is always editable if Buyer is not required for the Related Entity and should not be cleared.",
				GlbCompany.CurrentCompany.PK, ruleSet.BRS_RelatedEntityId);

			ruleSet.BRS_OH_Buyer = buyer1.PK;
			AssertEquals("The Related Entity is always editable if Buyer is not required for the Related Entity and should not be cleared.",
				GlbCompany.CurrentCompany.PK, ruleSet.BRS_RelatedEntityId);

			dummy.RelatedEntityRequirements = RelatedEntityRequirements.MustHaveBuyer;
			AssertEquals("If the Related Entity is only editable if Buyer is set, changing Buyer shouldn't clear the Related Entity.",
				GlbCompany.CurrentCompany.PK, ruleSet.BRS_RelatedEntityId);

			ruleSet.BRS_OH_Buyer = ZGuid.Empty;
			AssertEquals("If the Related Entity is only editable if Buyer is set, clearing out the Buyer should clear out the Related Entity.",
				ZGuid.Empty, ruleSet.BRS_RelatedEntityId);
		}

		#endregion

		#region TestBRS_OH_BuyerInfo

		public void TestBRS_OH_BuyerInfo()
		{
			var ruleSet = Helper.CreateRuleSet();
			ruleSet.BRS_Module = DummyBarcodeParsingConsumer.Module;
			AssertEquals("Dummy Buyer", ruleSet.BRS_OH_BuyerInfo.HumanReadableName);

			ruleSet.BRS_Module = "";
			AssertEquals("Buyer", ruleSet.BRS_OH_BuyerInfo.HumanReadableName);
		}

		#endregion

		#region TestBRS_OH_Supplier

		public void TestBRS_OH_Supplier()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var buyer2 = Helper.CreateOrg("Buyer2");
			var supplier = Helper.CreateOrg("Supplier");

			var ruleSet = Helper.CreateRuleSet(buyer1);
			ruleSet.BRS_Module = DummyBarcodeParsingConsumer.Module;

			var dummy = DummyBarcodeParsingConsumer.GetDummy(Factory);
			dummy.IsRelatedEntityAvailable = true;
			dummy.RelatedEntityRequirements = RelatedEntityRequirements.MustHaveBuyer;
			dummy.RelatedEntityList = new GlbCompanyCollection(Factory);
			ruleSet.BRS_RelatedEntityId = GlbCompany.CurrentCompany.PK;
			AssertEquals("Precondition - Related Entity PK should be set.", GlbCompany.CurrentCompany.PK, ruleSet.BRS_RelatedEntityId);

			ruleSet.BRS_OH_Buyer = buyer2.PK;
			ruleSet.BRS_OH_Supplier = supplier.PK;
			AssertEquals("If the Related Entity is only editable if Buyer is set, changing Supplier shouldn't clear the Related Entity.", GlbCompany.CurrentCompany.PK, ruleSet.BRS_RelatedEntityId);
			ruleSet.BRS_OH_Supplier = ZGuid.Empty; // clean up

			// don't use the business layer so as to avoid the buyer setter clearing the Related Entity.
			((IBusinessObjectInternals)ruleSet).Row[BarcodeRuleSetSchema.Constants.BRS_OH_Buyer] = Guid.Empty;
			AssertEquals("Precondition - Related Entity PK should be set.", GlbCompany.CurrentCompany.PK, ruleSet.BRS_RelatedEntityId);

			ruleSet.BRS_OH_Supplier = supplier.PK;
			AssertEquals("If the Related Entity is only editable if Buyer is set, if the Buyer is empty it should clear out the Related Entity.", ZGuid.Empty, ruleSet.BRS_RelatedEntityId);
		}

		#endregion

		#region TestBRS_OH_SupplierInfo

		public void TestBRS_OH_SupplierInfo()
		{
			var ruleSet = Helper.CreateRuleSet();
			ruleSet.BRS_Module = DummyBarcodeParsingConsumer.Module;
			AssertEquals("Dummy Supplier", ruleSet.BRS_OH_SupplierInfo.HumanReadableName);

			ruleSet.BRS_Module = "";
			AssertEquals("Supplier", ruleSet.BRS_OH_SupplierInfo.HumanReadableName);
		}

		#endregion

		#region TestBRS_RelatedEntity

		public void TestBRS_RelatedEntity()
		{
			bool wasTablePrefixSetBeforeValueSet = false;
			var ruleSet = Helper.CreateRuleSet();
			var dummy = DummyBarcodeParsingConsumer.GetDummy(Factory);
			dummy.IsRelatedEntityAvailable = true;
			dummy.RelatedEntityList = new GlbCompanyCollection(Factory);
			ruleSet.BRS_RelatedEntityIdInfo.ValueChanged += delegate
			{
				wasTablePrefixSetBeforeValueSet = ruleSet.BRS_RelatedEntityTableCode == GlbCompanySchema.Constants.Prefix;
			};

			ruleSet.BRS_RelatedEntityId = GlbCompany.CurrentCompany.PK;
			AssertEquals("Should have set the Table Code when setting the Related Entity.", GlbCompanySchema.Constants.Prefix, ruleSet.BRS_RelatedEntityTableCode);
			AssertEquals("Should have set the Table Code before setting the Related Entity PK.", true, wasTablePrefixSetBeforeValueSet);
		}

		#endregion

		#region TestBRS_RelatedEntityInfo

		public void TestBRS_RelatedEntityInfo()
		{
			var ruleSet = Helper.CreateRuleSet();
			ruleSet.BRS_Module = DummyBarcodeParsingConsumer.Module;
			AssertEquals("Dummy Entity", ruleSet.BRS_RelatedEntityIdInfo.HumanReadableName);

			ruleSet.BRS_Module = "";
			AssertEquals("Related Entity", ruleSet.BRS_RelatedEntityIdInfo.HumanReadableName);
		}

		#endregion

		#region TestBRS_RelatedEntityInfo_ReadOnly

		public void TestBRS_RelatedEntityInfo_ReadOnly()
		{
			var ruleSet = Helper.CreateRuleSet();
			ruleSet.BRS_Module = "";
			AssertEquals("No module set, means related Entity is not available and thus read only.", true, ruleSet.BRS_RelatedEntityIdInfo.ReadOnly);

			var dummy = DummyBarcodeParsingConsumer.GetDummy(Factory);
			dummy.IsRelatedEntityAvailable = false;
			ruleSet.BRS_Module = DummyBarcodeParsingConsumer.Module;
			AssertEquals("Related Entity is Read Only if Module does not support Related Entities.", true, ruleSet.BRS_RelatedEntityIdInfo.ReadOnly);

			dummy.IsRelatedEntityAvailable = true;
			AssertEquals("Related Entity should always be editable if Related Entity does not require Buyer.", false, ruleSet.BRS_RelatedEntityIdInfo.ReadOnly);

			dummy.RelatedEntityRequirements = RelatedEntityRequirements.MustHaveBuyer;
			AssertEquals("Related Entity should be Read Only if Related Entity requires Buyer and no Buyer is set.", true, ruleSet.BRS_RelatedEntityIdInfo.ReadOnly);

			ruleSet.BRS_OH_Buyer = Helper.CreateOrg("Buyer").PK;
			AssertEquals("Related Entity should be Editable if Buyer is set.", false, ruleSet.BRS_RelatedEntityIdInfo.ReadOnly);
		}

		#endregion

		// calculated

		#region TestBuyerCaption

		public void TestBuyerCaption()
		{
			var ruleSet = Helper.CreateRuleSet();
			ruleSet.BRS_Module = DummyBarcodeParsingConsumer.Module;
			AssertEquals("Dummy Buyer", ruleSet.BuyerCaption);

			ruleSet.BRS_Module = "";
			AssertEquals("Buyer", ruleSet.BuyerCaption);
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			AssertEquals("Barcode Rule Set", Helper.CreateRuleSet().HumanReadableName);
		}

		#endregion

		#region TestIsAutoLogged

		public void TestIsAutoLogged()
		{
			AssertEquals(true, Helper.CreateRuleSet().IsAutoAdminBusinessObjectLoggerEnabled);
		}

		#endregion

		#region TestReadOnly

		public void TestReadOnly()
		{
			var ruleSet = Helper.CreateRuleSet();
			AssertEquals("Not System Defined, should be editable.", false, ruleSet.ReadOnly);

			ruleSet.BRS_IsSystem = true;
			AssertEquals("System Defined, should be read-only.", true, ruleSet.ReadOnly);
		}

		#endregion

		#region TestRelatedEntityCaption

		public void TestRelatedEntityCaption()
		{
			var ruleSet = Helper.CreateRuleSet();
			ruleSet.BRS_Module = DummyBarcodeParsingConsumer.Module;
			AssertEquals("Dummy Entity", ruleSet.RelatedEntityCaption);

			ruleSet.BRS_Module = "";
			AssertEquals("Related Entity", ruleSet.RelatedEntityCaption);
		}

		#endregion

		#region TestSupplierCaption

		public void TestSupplierCaption()
		{
			var ruleSet = Helper.CreateRuleSet();
			ruleSet.BRS_Module = DummyBarcodeParsingConsumer.Module;
			AssertEquals("Dummy Supplier", ruleSet.SupplierCaption);

			ruleSet.BRS_Module = "";
			AssertEquals("Supplier", ruleSet.SupplierCaption);
		}

		#endregion

		#region TestTotalRules

		public void TestTotalRules()
		{
			var ruleSet = Helper.CreateRuleSet();
			Helper.CreateRule(ruleSet);
			AssertEquals(1, ruleSet.TotalRules);

			Helper.CreateValidationRule(ruleSet, DummyTargetFields.Codes.TargetField1);
			AssertEquals(2, ruleSet.TotalRules);

			Helper.CreateValidationRule(ruleSet, DummyTargetFields.Codes.TargetField2);
			AssertEquals(3, ruleSet.TotalRules);
		}

		#endregion

		#endregion

		#region Flags

		#region TestIsRelatedEntityAvailable

		public void TestIsRelatedEntityAvailable()
		{
			var ruleSet = Helper.CreateRuleSet();
			ruleSet.BRS_Module = DummyBarcodeParsingConsumer.Module;

			var dummy = DummyBarcodeParsingConsumer.GetDummy(Factory);
			dummy.IsRelatedEntityAvailable = true;
			AssertEquals("If Related Entity is supported by Module it should be available.", true, ruleSet.IsRelatedEntityAvailable);

			dummy.IsRelatedEntityAvailable = false;
			AssertEquals("If Related Entity is not supported by Module it should not be available.", false, ruleSet.IsRelatedEntityAvailable);

			ruleSet.BRS_Module = "";
			AssertEquals("When module is blank, Related Entity should not be available.", false, ruleSet.IsRelatedEntityAvailable);
		}

		#endregion

		#endregion

		#region SetDefaultValues

		public void TestSetDefaultValues()
		{
			AssertEquals(BarcodeModuleTypes.Codes.Warehouse, Factory.New<BarcodeRuleSet>().BRS_Module);
		}

		#endregion
	}
}
