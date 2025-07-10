using System.Linq;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HVLVDetailsPreScreeningConfiguration))]
	sealed class HVLVDetailsPreScreeningConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestGetMostMatchedPreScreeningRule_MatchingLogic()
		{
			var validationConfiguration = new HVLVDetailsPreScreeningConfiguration();
			var rule1 = validationConfiguration.Rules.AddNew();
			rule1.TransportMode = Core.Constants.TransportModes.Sea;
			rule1.ETailer = "ARJIMP";
			rule1.OriginCountryCode = "CN";
			rule1.DestinationCountryCode = "NZ";
			var field1 = rule1.Fields.AddNew();
			field1.FieldDescription = "Shipper";
			field1.ValidationRule = "ERR";

			var rule2 = validationConfiguration.Rules.AddNew();
			rule2.TransportMode = Core.Constants.TransportModes.Sea;
			var field2 = rule2.Fields.AddNew();
			field2.FieldDescription = "Consignee";
			field2.ValidationRule = "ERR";

			var rule3 = validationConfiguration.Rules.AddNew();
			rule3.TransportMode = Core.Constants.TransportModes.Sea;
			rule3.DestinationCountryCode = "AU";
			var field3 = rule3.Fields.AddNew();
			field3.FieldDescription = "Consignee Address1";
			field3.ValidationRule = "ERR";

			var rule4 = validationConfiguration.Rules.AddNew();
			rule4.TransportMode = Core.Constants.TransportModes.Sea;
			rule4.OriginCountryCode = "US";
			var field4 = rule4.Fields.AddNew();
			field4.FieldDescription = "Consignee Address2";
			field4.ValidationRule = "ERR";

			var rule5 = validationConfiguration.Rules.AddNew();
			rule5.TransportMode = Core.Constants.TransportModes.Sea;
			rule5.ETailer = "ARTNRO";
			var field5 = rule5.Fields.AddNew();
			field5.FieldDescription = "Consignee City";
			field5.ValidationRule = "ERR";

			var rule6 = validationConfiguration.Rules.AddNew();
			rule6.ETailer = "AMEAEF";
			var field6 = rule6.Fields.AddNew();
			field6.FieldDescription = "Shipper";
			field6.ValidationRule = "ERR";

			var matchedRuleCombine1 = validationConfiguration.GetMostMatchedPreScreeningRules(JobShipmentSchema.Constants.Prefix, Core.Constants.TransportModes.Sea, "ARJIMP", "AU", "US");
			AssertEquals(1, matchedRuleCombine1.Length);
			Assert(matchedRuleCombine1[0].Fields.Cast<HVLVPreScreeningField>().Any(x => x.FieldDescription == "Consignee"));

			var matchedRuleCombine2 = validationConfiguration.GetMostMatchedPreScreeningRules(JobShipmentSchema.Constants.Prefix, Core.Constants.TransportModes.Sea, "ATSENT", "NZ", "AU");
			AssertEquals(1, matchedRuleCombine2.Length);
			Assert(matchedRuleCombine2.Any(x => x.DestinationCountryCode == "AU"));

			var matchedRuleCombine3 = validationConfiguration.GetMostMatchedPreScreeningRules(JobShipmentSchema.Constants.Prefix, Core.Constants.TransportModes.Sea, "", "US", "AU");
			AssertEquals(2, matchedRuleCombine3.Length);
			Assert(matchedRuleCombine3.Any(x => x.DestinationCountryCode == "AU"));
			Assert(matchedRuleCombine3.Any(x => x.OriginCountryCode == "US"));

			var matchedRuleCombine4 = validationConfiguration.GetMostMatchedPreScreeningRules(JobShipmentSchema.Constants.Prefix, Core.Constants.TransportModes.Sea, "ARJIMP", "CN", "NZ");
			AssertEquals(1, matchedRuleCombine4.Length);
			Assert(matchedRuleCombine4.Any(x => x.OriginCountryCode == "CN" && x.DestinationCountryCode == "NZ" && x.ETailer == "ARJIMP"));

			var matchedRuleCombine5 = validationConfiguration.GetMostMatchedPreScreeningRules(JobShipmentSchema.Constants.Prefix, Core.Constants.TransportModes.Air, "ARJIMP", "CN", "NZ");
			AssertNull(matchedRuleCombine5);

			var matchedRuleCombine6 = validationConfiguration.GetMostMatchedPreScreeningRules(JobShipmentSchema.Constants.Prefix, Core.Constants.TransportModes.Sea, "", "CN", "");
			AssertEquals(1, matchedRuleCombine6.Length);
			Assert(matchedRuleCombine6[0].Fields.Cast<HVLVPreScreeningField>().Any(x => x.FieldDescription == "Consignee"));

			var matchedRuleCombine7 = validationConfiguration.GetMostMatchedPreScreeningRules(JobShipmentSchema.Constants.Prefix, Core.Constants.TransportModes.Sea, "", "", "AU");
			AssertEquals(1, matchedRuleCombine7.Length);
			Assert(matchedRuleCombine7.Any(x => x.DestinationCountryCode == "AU"));

			var matchedRuleCombine8 = validationConfiguration.GetMostMatchedPreScreeningRules(JobShipmentSchema.Constants.Prefix, Core.Constants.TransportModes.Sea, "", "", "");
			AssertEquals(1, matchedRuleCombine8.Length);
			Assert(matchedRuleCombine8[0].Fields.Cast<HVLVPreScreeningField>().Any(x => x.FieldDescription == "Consignee"));

			var matchedRuleCombine9 = validationConfiguration.GetMostMatchedPreScreeningRules(JobShipmentSchema.Constants.Prefix, "", "ARTNRO", "US", "AU");
			AssertEquals(3, matchedRuleCombine9.Length);
			Assert(matchedRuleCombine9.Any(x => x.DestinationCountryCode == "AU"));
			Assert(matchedRuleCombine9.Any(x => x.OriginCountryCode == "US"));
			Assert(matchedRuleCombine9.Any(x => x.ETailer == "ARTNRO"));

			var matchedRuleCombine10 = validationConfiguration.GetMostMatchedPreScreeningRules(JobShipmentSchema.Constants.Prefix, "", "ARTNRO", "", "");
			AssertEquals(1, matchedRuleCombine10.Length);
			Assert(matchedRuleCombine10.Any(x => x.ETailer == "ARTNRO"));

			var matchedRuleCombine11 = validationConfiguration.GetMostMatchedPreScreeningRules(JobShipmentSchema.Constants.Prefix, Core.Constants.TransportModes.Air, "AMEAEF", "", "");
			AssertEquals(1, matchedRuleCombine11.Length);
			Assert(matchedRuleCombine11.Any(x => x.ETailer == "AMEAEF"));
		}

		public void TestGetMostMatchedPreScreeningRule_DifferentModule()
		{
			var validationConfiguration = new HVLVDetailsPreScreeningConfiguration();

			var rule1 = validationConfiguration.Rules.AddNew();
			rule1.ModuleType = HVLVPreScreeningRule.ModuleTypeCodes.HVLVBookingHeader;
			rule1.OriginCountryCode = "AU";
			var field1 = rule1.Fields.AddNew();
			field1.FieldDescription = "Shipper";
			field1.ValidationRule = "ERR";

			var rule2 = validationConfiguration.Rules.AddNew();
			rule2.ModuleType = HVLVPreScreeningRule.ModuleTypeCodes.HVLVBookingHeader;
			rule2.OriginCountryCode = "US";
			var field2 = rule2.Fields.AddNew();
			field2.FieldDescription = "Consignee";
			field2.ValidationRule = "ERR";

			var rule3 = validationConfiguration.Rules.AddNew();
			rule3.ModuleType = HVLVPreScreeningRule.ModuleTypeCodes.HVLVShipment;
			rule3.TransportMode = Core.Constants.TransportModes.Sea;
			rule3.OriginCountryCode = "AU";
			var field3 = rule3.Fields.AddNew();
			field3.FieldDescription = "ConsigneeAddress1";
			field3.ValidationRule = "ERR";

			var rule4 = validationConfiguration.Rules.AddNew();
			rule4.ModuleType = HVLVPreScreeningRule.ModuleTypeCodes.HVLVShipment;
			rule4.TransportMode = Core.Constants.TransportModes.Air;
			rule4.OriginCountryCode = "US";
			var field4 = rule4.Fields.AddNew();
			field4.FieldDescription = "ConsigneeAddress2";
			field4.ValidationRule = "ERR";

			CombineAssertions("booking header match HVH rules", () =>
			{
				var matchedRule1 = validationConfiguration.GetMostMatchedPreScreeningRules(HVLVPreScreeningRule.ModuleTypeCodes.HVLVBookingHeader, "", "", "AU", "");
				AssertEquals(1, matchedRule1.Length);
				AssertEquals(1, matchedRule1[0].Fields.Count);
				AssertEquals("Shipper", matchedRule1[0].Fields[0].FieldDescription);

				var matchedRule2 = validationConfiguration.GetMostMatchedPreScreeningRules(HVLVPreScreeningRule.ModuleTypeCodes.HVLVBookingHeader, "", "", "US", "");
				AssertEquals(1, matchedRule2.Length);
				AssertEquals(1, matchedRule2[0].Fields.Count);
				AssertEquals("Consignee", matchedRule2[0].Fields[0].FieldDescription);
			});

			CombineAssertions("shipment match SHP rules", () =>
			{
				var matchedRule3 = validationConfiguration.GetMostMatchedPreScreeningRules(JobShipmentSchema.Constants.Prefix, Core.Constants.TransportModes.Sea, "", "AU", "");
				AssertEquals(1, matchedRule3.Length);
				AssertEquals(1, matchedRule3[0].Fields.Count);
				AssertEquals("ConsigneeAddress1", matchedRule3[0].Fields[0].FieldDescription);

				var matchedRule4 = validationConfiguration.GetMostMatchedPreScreeningRules(JobShipmentSchema.Constants.Prefix, Core.Constants.TransportModes.Air, "", "US", "");
				AssertEquals(1, matchedRule4.Length);
				AssertEquals(1, matchedRule4[0].Fields.Count);
				AssertEquals("ConsigneeAddress2", matchedRule4[0].Fields[0].FieldDescription);
			});
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			BizObj.IsEnabled = true;
			return BizObj;
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		new HVLVDetailsPreScreeningConfiguration BizObj
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (HVLVDetailsPreScreeningConfiguration)base.BizObj; }
		}

		#endregion
	}
}
