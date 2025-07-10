using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BarcodeParsingEngine;
using Enterprise.BarcodeParsingEngine.Warehouse;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	[TestedType(typeof(BarcodeRuleComponent))]
	class BarcodeRuleComponentTest : BarcodeParsingBusinessObjectTestCase
	{
		#region Related Entities

		#region TestRule

		public void TestRule()
		{
			var ruleComponent = Factory.New<BarcodeRuleComponent>();
			AssertNull(ruleComponent.Rule);

			var rule = Helper.CreateRule();
			ruleComponent.BRC_BRU_Rule = rule.PK;
			AssertEquals(rule, ruleComponent.Rule);
		}

		#endregion

		#endregion

		#region Properties

		// persistent

		#region TestBRC_Delimiter

		public void TestBRC_Delimiter()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);

			int delimiterInfoRefreshBindingHitCount = 0;
			component.DelimiterForBindingInfo.ValueChanged += (sender, e) => delimiterInfoRefreshBindingHitCount++;
			component.BRC_Delimiter = 44;
			AssertEquals("Setting DB Delimiter property should refresh the non Persistent Delimiter property.", 1, delimiterInfoRefreshBindingHitCount);
		}

		#endregion

		#region TestBRC_ApplicationID_ForGS1Rule

		public void TestBRC_ApplicationID_ForGS1Rule()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);
			rule.IsGS1 = true;

			var regItem = WarehouseDataRegistry.Instance.ApplicationIdentifiers.Value["21"];
			component.BRC_ApplicationID = "21"; // just for example - serial number	appId
			AssertNotEquals("Precondition - value in registry is not empty.", 0, regItem.MaxFieldLength);
			AssertNotEquals("Precondition - value in registry is not empty.", "", regItem.DataType);

			AssertEquals("component.BRC_MinLength", (ZShort)1, component.BRC_MinLength); // it is 0 in registry but system should put 1 instead
			AssertEquals((ZShort)regItem.MaxFieldLength, component.BRC_MaxLength);
			AssertEquals(regItem.DataType, component.BRC_Format);
			AssertEquals(LengthTypes.Descriptions.Range, component.LengthTypeForBinding);
		}

		#endregion

		#region TestBRC_ApplicationID_ForDateRelatedIdentifiers

		public void TestBRC_ApplicationID_ForDateRelatedIdentifiers()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);
			rule.IsGS1 = true;

			var dateRelatedAppIds = new[] { "11", "12", "13", "15", "17" };

			foreach (var appId in dateRelatedAppIds)
			{
				component.BRC_ApplicationID = appId;
				AssertEquals(GS1DataFormatTypes.Codes.YYMMDD, component.BRC_Format);
			}
		}

		#endregion

		#region TestBRC_ApplicationID_ForDateRelatedIdentifiersOverriden

		public void TestBRC_ApplicationID_ForDateRelatedIdentifiersOverriden()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);
			rule.IsGS1 = true;
			var appIds = WarehouseDataRegistry.Instance.ApplicationIdentifiers.Value;
			var regItem = appIds["11"];
			regItem.MaxFieldLength = 7;
			using (WarehouseDataRegistry.Instance.ApplicationIdentifiers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, appIds))
			{
				component.BRC_ApplicationID = "11";
				AssertEquals(GS1DataFormatTypes.Codes.Digit, component.BRC_Format);
			}

			regItem = appIds["12"];
			regItem.MinFieldLength = 7;
			using (WarehouseDataRegistry.Instance.ApplicationIdentifiers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, appIds))
			{
				component.BRC_ApplicationID = "12";
				AssertEquals(GS1DataFormatTypes.Codes.Digit, component.BRC_Format);
			}

			regItem = appIds["13"];
			regItem.DataType = "ANY";
			using (WarehouseDataRegistry.Instance.ApplicationIdentifiers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, appIds))
			{
				component.BRC_ApplicationID = "13";
				AssertEquals(GS1DataFormatTypes.Codes.AlphaNumericWithSymbols, component.BRC_Format);
			}
		}

		#endregion

		#region TestBRC_ApplicationID_Change

		public void TestBRC_ApplicationID_Change()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);
			rule.IsGS1 = true;

			var regItem1 = WarehouseDataRegistry.Instance.ApplicationIdentifiers.Value["20"];
			var regItem2 = WarehouseDataRegistry.Instance.ApplicationIdentifiers.Value["240"];

			component.BRC_ApplicationID = "20";
			AssertNotEquals("Precondition - value in registry is not empty.", 0, regItem1.MaxFieldLength);
			AssertNotEquals("Precondition - value in registry is not empty.", "", regItem1.DataType);

			AssertEquals(regItem1.MinFieldLength, component.BRC_MinLength);
			AssertEquals((ZShort)regItem1.MaxFieldLength, component.BRC_MaxLength);
			AssertEquals(regItem1.DataType, component.BRC_Format);
			AssertEquals(LengthTypes.Descriptions.Fixed, component.LengthTypeForBinding);

			component.BRC_ApplicationID = "240";
			AssertEquals("component.BRC_MinLength", (ZShort)1, component.BRC_MinLength); // it is 0 in registry but system should put 1 instead
			AssertEquals((ZShort)regItem2.MaxFieldLength, component.BRC_MaxLength);
			AssertEquals(regItem2.DataType, component.BRC_Format);
			AssertEquals(LengthTypes.Descriptions.Range, component.LengthTypeForBinding);
		}

		#endregion

		// calculated

		#region TestApplicationIDDataFieldType

		public void TestApplicationIDDataFieldType()
		{
			var rule = Helper.CreateRule();
			var ruleComponent = Helper.CreateRuleComponent(rule);
			AssertEquals(nameof(FieldType.Text), ruleComponent.ApplicationIDDataFieldType);

			rule.TerminatorType = TerminatorTypes.Codes.GS1;
			AssertEquals(nameof(FieldType.TextDropEdit), ruleComponent.ApplicationIDDataFieldType);
		}

		#endregion

		#region TestDelimiterForBinding

		public void TestDelimiterForBinding()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);
			AssertEquals("DelimiterForBinding should be empty by default.", "", component.DelimiterForBinding);

			component.BRC_Delimiter = 44; // comma
			AssertEquals("DelimiterForBinding should get its value from the BRC_Delimiter property.", "\",\"", component.DelimiterForBinding);

			component.DelimiterForBinding = "";
			AssertEquals("Setting DelimiterForBinding should set BRC_Delimiter property.", (byte)0, component.BRC_Delimiter);
			AssertEquals("", component.DelimiterForBinding);

			component.DelimiterForBinding = " ";
			AssertEquals("Setting DelimiterForBinding to a space should retain the space.", (byte)' ', component.BRC_Delimiter);
			AssertEquals("\" \"", component.DelimiterForBinding);

			component.DelimiterForBinding = "-";
			AssertEquals("Setting DelimiterForBinding should set BRC_Delimiter property.", (byte)'-', component.BRC_Delimiter);
			AssertEquals("\"-\"", component.DelimiterForBinding);

			component.DelimiterForBinding = "\"";
			AssertEquals("Setting DelimiterForBinding to one Double Quote should wrap correctly.", "\"\"\"", component.DelimiterForBinding);
			AssertEquals("Setting DelimiterForBinding should set BRC_Delimiter property.", (byte)'\"', component.BRC_Delimiter);

			component.DelimiterForBinding = "\"\"";
			AssertEquals("Setting DelimiterForBinding to two Double Quotes should wrap correctly.", "\"\"\"", component.DelimiterForBinding);
			AssertEquals("Setting DelimiterForBinding should set BRC_Delimiter property.", (byte)'\"', component.BRC_Delimiter);

			component.DelimiterForBinding = "\"\"\"";
			AssertEquals("Setting DelimiterForBinding to three Double Quotes should wrap correctly.", "\"\"\"", component.DelimiterForBinding);
			AssertEquals("Setting DelimiterForBinding should set BRC_Delimiter property.", (byte)'\"', component.BRC_Delimiter);
		}

		#endregion

		#region TestDateFormatSetsMinLengthAndMaxLengthAutomatically

		public void TestDateFormatSetsMinLengthAndMaxLengthAutomatically()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);

			component.BRC_Format = GS1DataFormatTypes.Codes.Alpha;
			AssertEquals((short)0, component.BRC_MaxLength);
			AssertEquals((short)0, component.BRC_MinLength);
			AssertEquals(false, component.BRC_MaxLengthInfo.ReadOnly);
			AssertEquals(false, component.BRC_MinLengthInfo.ReadOnly);
			AssertEquals(false, component.LengthTypeForBindingInfo.ReadOnly);

			component.BRC_Format = OtherDataFormatTypes.Codes.MMDDYY;
			AssertEquals((short)6, component.BRC_MaxLength);
			AssertEquals((short)6, component.BRC_MinLength);
			AssertEquals(true, component.BRC_MaxLengthInfo.ReadOnly);
			AssertEquals(true, component.BRC_MinLengthInfo.ReadOnly);
			AssertEquals(true, component.LengthTypeForBindingInfo.ReadOnly);

			component.BRC_Format = OtherDataFormatTypes.Codes.MMDDYYYY;
			AssertEquals((short)8, component.BRC_MaxLength);
			AssertEquals((short)8, component.BRC_MinLength);
			AssertEquals(true, component.BRC_MaxLengthInfo.ReadOnly);
			AssertEquals(true, component.BRC_MinLengthInfo.ReadOnly);
			AssertEquals(true, component.LengthTypeForBindingInfo.ReadOnly);
		}

		#endregion

		#region TestDelimiterForBindingInfo

		public void TestDelimiterForBindingInfo()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);
			AssertEquals("Delimiter should be Read Only by default.", true, component.DelimiterForBindingInfo.ReadOnly);

			component.DelimiterForBinding = " ";
			AssertEquals("Delimiter should not be Read Only when Delimiter has a value.", false, component.DelimiterForBindingInfo.ReadOnly);

			component.DelimiterForBinding = "";
			AssertEquals("Delimiter should be Read Only when Delimiter has no value.", true, component.DelimiterForBindingInfo.ReadOnly);
		}

		#endregion

		#region TestIsDelimiterMultiComponent

		public void TestIsDelimiterMultiComponent_NonGS1Rule_FullRule()
		{
			var ruleSet = Helper.CreateRuleSet();
			var fullRule = Helper.CreateRule(ruleSet, "Full Rule", false, false);
			var component1WithoutDelimiter = Helper.CreateRuleComponent(fullRule);
			var component2WithoutDelimiter = Helper.CreateRuleComponent(fullRule);
			var component1WithDelimiter = Helper.CreateRuleComponent(fullRule, true);
			var component2WithDelimiter = Helper.CreateRuleComponent(fullRule, true);
			var component3WithDelimiter = Helper.CreateRuleComponent(fullRule, true);
			AssertEquals("Precondition", false, fullRule.BRU_IsDelimiterMultiComponent);
			AssertEquals("Precondition", false, component1WithoutDelimiter.IsDelimiterMultiComponent);
			AssertEquals("Precondition", false, component2WithoutDelimiter.IsDelimiterMultiComponent);
			AssertEquals("Precondition", false, component1WithDelimiter.IsDelimiterMultiComponent);
			AssertEquals("Precondition", false, component2WithDelimiter.IsDelimiterMultiComponent);

			// Without delimiter
			component1WithoutDelimiter.IsDelimiterMultiComponent = true;
			AssertEquals("Current component does not have a delimiter, however On the rule we always set BRU_IsDelimiterMultiComponent.",
				true, fullRule.BRU_IsDelimiterMultiComponent);
			AssertEquals("Current component does not have a delimiter, however checking this option will give default delimiter.",
				BarcodeRuleComponent.DefaultDelimiter, component1WithoutDelimiter.BRC_Delimiter);
			AssertEquals("Since the Rule was set to support multi component delimiters, IsDelimiterMultiComponent should be true.",
				true, component1WithoutDelimiter.IsDelimiterMultiComponent);
			AssertEquals("Since component does not have a Delimiter, IsDelimiterMultiComponent should be false.",
				false, component2WithoutDelimiter.IsDelimiterMultiComponent);
			AssertEquals("Only one Component can have a Multi-Component Delimiter, so the Delimiter should be removed from other Components.",
				false, component1WithDelimiter.IsDelimiterMultiComponent);
			AssertEquals("Only one Component can have a Multi-Component Delimiter, so the Delimiter should be removed from other Components.",
				false, component1WithDelimiter.HasDelimiter);
			AssertEquals("Only one Component can have a Multi-Component Delimiter, so the Delimiter should be removed from other Components.",
				false, component2WithDelimiter.IsDelimiterMultiComponent);

			component1WithoutDelimiter.IsDelimiterMultiComponent = false;
			AssertEquals("Current component's IsDelimiterMultiComponent is false, therefore should not be a multi component delimiter rule.",
				false, fullRule.BRU_IsDelimiterMultiComponent);
			AssertEquals("Current component has a delimiter therefore it should not be changed.",
				BarcodeRuleComponent.DefaultDelimiter, component1WithoutDelimiter.BRC_Delimiter);

			// With delimiter - other than last component
			component1WithoutDelimiter.BRC_Delimiter = (byte)'#'; // Changing delimiter on the first component so that it should be that delimiter when defaulting
			component1WithDelimiter.IsDelimiterMultiComponent = true;
			Assert("When setting IsDelimiterMultiComponent, we set the value on the rule as well.", fullRule.BRU_IsDelimiterMultiComponent);
			AssertEquals("Delimiter should be copied from the first component.", (byte)'#', component1WithDelimiter.BRC_Delimiter);
			AssertEquals("Only one component can have delimiter and delimiter should be removed from the components that already have a delimiter.",
				false, component2WithDelimiter.IsDelimiterMultiComponent || component2WithDelimiter.HasDelimiter);
			AssertEquals("Only one component can have delimiter and delimiter should be removed from the components that already have a delimiter.",
				false, component3WithDelimiter.IsDelimiterMultiComponent || component3WithDelimiter.HasDelimiter);
			AssertEquals("Only one component can have delimiter and delimiter should be removed from the components that already have a delimiter.",
				false, component1WithoutDelimiter.IsDelimiterMultiComponent || component1WithoutDelimiter.HasDelimiter);
			AssertEquals("Delimiter multi component doesn't apply to components without delimiter, therefore IsDelimiterMultiComponent should be false",
				false, component2WithoutDelimiter.IsDelimiterMultiComponent || component2WithoutDelimiter.HasDelimiter);

			component1WithDelimiter.IsDelimiterMultiComponent = false;
			AssertEquals("Since multi component delimiter is false it should mark multi component delimiter in rule as false.", false, fullRule.BRU_IsDelimiterMultiComponent);
			AssertEquals("Current component has a delimiter however multi component delimiter is turned off. therefore component should not be a multi component delimiter.", false, component1WithDelimiter.IsDelimiterMultiComponent);
			AssertEquals("Delimiter should not be removed.", (byte)'#', component1WithDelimiter.BRC_Delimiter);
		}

		#endregion

		#region TestIsGroupedWithDelimiterMultiComponent

		public void TestIsGroupedWithDelimiterMultiComponent()
		{
			var rule = Helper.CreateRule();
			var component1 = Helper.CreateRuleComponent(rule);
			var component2 = Helper.CreateRuleComponent(rule);
			var component3 = Helper.CreateRuleComponent(rule);
			component2.IsDelimiterMultiComponent = true;
			AssertEquals(false, component1.IsGroupedWithMultiComponentDelimiter);
			AssertEquals(true, component2.IsGroupedWithMultiComponentDelimiter);
			AssertEquals(true, component3.IsGroupedWithMultiComponentDelimiter);
		}

		#endregion

		#region TestIsLastComponent

		public void TestIsLastComponent()
		{
			var componentWithoutRule = Factory.New<BarcodeRuleComponent>();
			AssertEquals(false, componentWithoutRule.IsLastComponent);

			var rule = Helper.CreateRule();
			var component1 = Helper.CreateRuleComponent(rule);
			var lastComponent = Helper.CreateRuleComponent(rule);
			AssertEquals(false, component1.IsLastComponent);
			AssertEquals(true, lastComponent.IsLastComponent);
		}

		#endregion

		#region TestLengthTypeForBinding

		public void TestLengthTypeForBinding()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);

			AssertEquals("New Component should have Length Type of Range by default.", true, component.IsLengthTypeRange());
			AssertEquals("New Component should have Length Type of Range by default.", LengthTypes.Descriptions.Range, component.LengthTypeForBinding);
			AssertEquals(false, component.BRC_MinLengthInfo.ReadOnly);
			AssertEquals(false, component.BRC_MaxLengthInfo.ReadOnly);

			component.BRC_MaxLength = 5;
			AssertEquals("Precondition", (short)0, component.BRC_MinLength);

			component.LengthTypeForBinding = LengthTypes.Codes.Fixed;
			AssertEquals("Setting LengthTypeForBinding should make the getter return the correct description.",
				LengthTypes.Descriptions.Fixed, component.LengthTypeForBinding);
			AssertEquals((short)5, component.BRC_MinLength);

			component.BRC_MaxLength = 8;
			AssertEquals((short)8, component.BRC_MinLength);

			component.BRC_MinLength = 4;
			AssertEquals((short)4, component.BRC_MaxLength);

			component.LengthTypeForBinding = LengthTypes.Codes.Any;
			AssertEquals("Setting LengthTypeForBinding should make the getter return the correct description.",
				LengthTypes.Descriptions.Any, component.LengthTypeForBinding);
			AssertEquals((short)0, component.BRC_MaxLength);
			AssertEquals((short)0, component.BRC_MinLength);
			AssertEquals(true, component.BRC_MinLengthInfo.ReadOnly);
			AssertEquals(true, component.BRC_MaxLengthInfo.ReadOnly);
		}

		#endregion

		#region TestLengthTypeForBinding_DoesNotAcceptInvalidInput

		public void TestLengthTypeForBinding_DoesNotAcceptInvalidInput()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);

			AssertEquals("New Component should have Length Type of Range by default.", true, component.IsLengthTypeRange());
			AssertEquals("New Component should have Length Type of Range by default.", LengthTypes.Descriptions.Range, component.LengthTypeForBinding);

			component.LengthTypeForBinding = "X";
			AssertEquals("Invalid input should be ignored.", true, component.IsLengthTypeRange());
			AssertEquals("Invalid input should be ignored.", LengthTypes.Descriptions.Range, component.LengthTypeForBinding);

			component.LengthTypeForBinding = LengthTypes.Codes.Any;
			AssertEquals("Invalid input should be ignored.", true, component.IsLengthTypeAny());
			AssertEquals("Invalid input should be ignored.", LengthTypes.Descriptions.Any, component.LengthTypeForBinding);
		}

		#endregion

		#region TestLengthTypeForBinding_GetLengthTypeBasedOnMinAndMaxLength

		public void TestLengthTypeForBinding_GetLengthTypeBasedOnMinAndMaxLength()
		{
			var rule = Helper.CreateRule();
			Helper.CreateRuleComponent(rule, minLength: 1, maxLength: 1);
			Helper.CreateRuleComponent(rule, fixedLength: 0);
			Helper.CreateRuleComponent(rule, minLength: 1, maxLength: 2);

			Factory.Save();

			var ruleInOtherFactory = new BusinessObjectFactory().Load<BarcodeRule>(rule.PK);
			var component1 = ruleInOtherFactory.Components.Single(c => c.BRC_MinLength == 1 && c.BRC_MaxLength == 1);
			var component2 = ruleInOtherFactory.Components.Single(c => c.BRC_MinLength == 0 && c.BRC_MaxLength == 0);
			var component3 = ruleInOtherFactory.Components.Single(c => c.BRC_MinLength == 1 && c.BRC_MaxLength == 2);

			AssertEquals(LengthTypes.Descriptions.Fixed, component1.LengthTypeForBinding);
			AssertEquals(LengthTypes.Descriptions.Any, component2.LengthTypeForBinding);
			AssertEquals(LengthTypes.Descriptions.Range, component3.LengthTypeForBinding);
		}

		#endregion

		#region TestModule

		public void TestModule()
		{
			var ruleComponent = Factory.New<BarcodeRuleComponent>();
			AssertEquals("", ruleComponent.Module);

			var rule = Helper.CreateRule();
			ruleComponent.BRC_BRU_Rule = rule.PK;
			AssertEquals("DUM", ruleComponent.Module);
		}

		#endregion

		#region TestReadOnly

		public void TestReadOnly()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);
			AssertEquals("RuleSet is not system defined, should be editable.", false, component.ReadOnly);

			rule.RuleSet.BRS_IsSystem = true;
			AssertEquals("RuleSet is system defined, should be read-only.", true, component.ReadOnly);
		}

		#endregion

		#region TestSequenceNoReadOnlyIfRuleIsPartialRule

		public void TestSequenceNoReadOnlyIfRuleIsPartialRule()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);
			AssertEquals(false, component.BRC_SequenceInfo.ReadOnly);

			rule.IsPartialRule = true;
			AssertEquals(true, component.BRC_SequenceInfo.ReadOnly);
		}

		#endregion

		#region TestTargetFieldDescription

		public void TestTargetFieldDescription()
		{
			var rule = Helper.CreateRule();
			var ruleComponent = Helper.CreateRuleComponent(rule);
			ruleComponent.BRC_TargetField = DummyTargetFields.Codes.TargetField1;
			AssertEquals(DummyTargetFields.Descriptions.TargetField1, ruleComponent.TargetFieldDescription);
		}

		#endregion

		#endregion

		#region Flags

		#region TestHasDelimiter

		public void TestHasDelimiter()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);
			AssertEquals("Should have no Delimiter by default.", false, component.HasDelimiter);

			component.DelimiterForBinding = "|";
			AssertEquals("Should have Delimiter if Delimiter is set.", true, component.HasDelimiter);

			int hasDelimiterChangedHitCount = 0;
			component.HasDelimiterInfo.ValueChanged += (sender, e) => hasDelimiterChangedHitCount++;
			component.HasDelimiter = true;
			AssertEquals("Setting HasDelimiter to true should not override existing Delimiter.", "\"|\"", component.DelimiterForBinding);
			AssertEquals("Setting HasDelimiter should refresh binding.", 1, hasDelimiterChangedHitCount);

			component.HasDelimiter = false;
			AssertEquals("Setting HasDelimiter to false should Set Delimiter to have no value.", "", component.DelimiterForBinding);
			AssertEquals("Setting HasDelimiter should refresh binding.", 2, hasDelimiterChangedHitCount);

			component.HasDelimiter = true;
			AssertEquals("Setting HasDelimiter to true should set Delimiter to ',' if Delimiter has no value.", "\",\"", component.DelimiterForBinding);
			AssertEquals("Setting HasDelimiter should refresh binding.", 3, hasDelimiterChangedHitCount);
		}

		#endregion

		#region TestIsApplicationIdentifierBoundToList

		public void TestIsApplicationIdentifierBoundToList()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);
			AssertEquals("By default application identifier is not bound to list.", false, component.IsApplicationIdentifierBoundToList);

			rule.TerminatorType = TerminatorTypes.Codes.GS1;
			AssertEquals("If Rule is GS1 application identifier should be bound to list.", true, component.IsApplicationIdentifierBoundToList);
		}

		#endregion

		#region TestIsGS1

		public void TestIsGS1()
		{
			var component = Factory.New<BarcodeRuleComponent>();
			AssertEquals("Should not be GS1 Rule Component by default.", false, component.IsGS1);

			var rule = Helper.CreateRule();
			component.BRC_BRU_Rule = rule.PK;
			AssertEquals("Should not be GS1 Rule Component if Rule is not GS1.", false, component.IsGS1);

			rule.TerminatorType = TerminatorTypes.Codes.GS1;
			AssertEquals("Should be GS1 Rule Component if Rule is GS1.", true, component.IsGS1);
		}

		#endregion

		#region TestIsLengthTypeAny

		public void TestIsLengthTypeAny()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);
			AssertEquals("New Component should have Length Type of Range by default.", true, component.IsLengthTypeRange());
			AssertEquals("New Component should have Length Type of Range by default.", LengthTypes.Descriptions.Range, component.LengthTypeForBinding);
			AssertEquals(false, component.IsLengthTypeAny());

			component.LengthTypeForBinding = LengthTypes.Codes.Any;
			AssertEquals(true, component.IsLengthTypeAny());
		}

		#endregion

		#region TestIsLengthTypeRange

		public void TestIsLengthTypeRange()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);
			AssertEquals("New Component should have Length Type of Range by default.", true, component.IsLengthTypeRange());
			AssertEquals("New Component should have Length Type of Range by default.", LengthTypes.Descriptions.Range, component.LengthTypeForBinding);

			component.LengthTypeForBinding = LengthTypes.Codes.Any;
			AssertEquals(false, component.IsLengthTypeRange());
		}

		#endregion

		#endregion

		#region TestCloningSupport

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Prevent change to base class as we would lose context of the actual schema class")]
		public void TestCloningSupport()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);
			component.BRC_MinLength = 5;
			component.BRC_MaxLength = 13;
			component.LengthTypeForBinding = LengthTypes.Codes.Range;
			Assert("BarcodeRuleComponent should support cloninig", component.SupportsClone());

			var clone = (BarcodeRuleComponent)component.Clone();
			AssertEquals(BarcodeRuleComponent.Schema.BRC_BRU_Rule + " must be empty", ZGuid.Empty, clone.BRC_BRU_Rule);
			AssertEquals(BarcodeRuleComponent.Schema.BRC_MinLength, clone.BRC_MinLength, component.BRC_MinLength);
			AssertEquals(BarcodeRuleComponent.Schema.BRC_MaxLength, clone.BRC_MaxLength, component.BRC_MaxLength);
			AssertEquals("LengthTypeForBinding", clone.LengthTypeForBinding, component.LengthTypeForBinding);
		}

		#endregion

		// interfaces

		#region IBarcodeRuleComponent Members

		public void TestIBarcodeRuleComponent()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);
			component.BRC_ApplicationID = "(P)";
			component.BRC_Delimiter = 44; // comma
			component.BRC_Format = nameof(FormatType.AN);
			component.BRC_MinLength = 1;
			component.BRC_MaxLength = 2;
			component.BRC_Sequence = 3;
			component.BRC_TargetField = nameof(WarehouseTargetField.PA1);

			IBarcodeRuleComponent iComponent = component;
			AssertEquals("iComponent.ApplicationIdentifier", "(P)", iComponent.ApplicationIdentifier);
			AssertEquals("iComponent.Format", FormatType.AN, iComponent.Format);
			AssertEquals("iComponent.Delimiter", ',', iComponent.Delimiter);
			AssertEquals("iComponent.MaxLength", (short)2, iComponent.MaxLength);
			AssertEquals("iComponent.MinLength", (short)1, iComponent.MinLength);
			AssertEquals("iComponent.Sequence", (short)3, iComponent.Sequence);
			AssertEquals("iComponent.TargetField", "PA1", iComponent.TargetField);

			component.BRC_Format = "XX";
			AssertEquals("Incorrect Format value should return Undefined Format Type.", FormatType.Undefined, iComponent.Format);

			component.BRC_Delimiter = 0;
			AssertEquals("iComponent.Delimiter", '\0', iComponent.Delimiter);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var rule = Helper.CreateRule();
			return Helper.CreateRuleComponent(rule);
		}

		#endregion
	}
}
