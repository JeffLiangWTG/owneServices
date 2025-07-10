using System.Text;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SubscriptionRulesRegistryDataType))]
	sealed class SubscriptionRulesRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<SubscriptionRulesRegistryDataType>
	{
		#region Implementation

		protected override SubscriptionRulesRegistryDataType GetNewDataType()
		{
			return new SubscriptionRulesRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "SubscriptionRulesRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var rules1 = new SubscriptionRuleCollection();
			var rule1 = rules1.AddNewRule("CD1", (NoResString)"Default Pubished List AAA", false, false, new string[] { "PRINT;SLT30;DESC1;SUM1", "RADIO;PREAP;DESC2;SUM2" });
			rule1.IsDefault = true;

			byte[] byteArrayvalue1 = Encoding.Unicode.GetBytes("<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfSubscriptionRule xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><SubscriptionRule><CodeMaxLength>3</CodeMaxLength><Code>CD1</Code><Description>Default Pubished List AAA</Description><IsDefault>Y</IsDefault><IsEnabled>Y</IsEnabled><SubscriptionNodesAsString>PRINT;SLT30;DESC1;SUM1|RADIO;PREAP;DESC2;SUM2</SubscriptionNodesAsString><CampaignType>CRM</CampaignType></SubscriptionRule></ArrayOfSubscriptionRule>");

			var rules2 = new SubscriptionRuleCollection();
			var rule2 = rules2.AddNewRule("CD2", (NoResString)"Default Pubished List YYY", false, false, new string[] { "TELEV;EXIST;DESC1;SUM1", "PRINT;EXIST;DESC2;SUM2" });
			rule2.IsDefault = true;

			byte[] byteArrayvalue2 = Encoding.Unicode.GetBytes("<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfSubscriptionRule xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><SubscriptionRule><CodeMaxLength>3</CodeMaxLength><Code>CD2</Code><Description>Default Pubished List YYY</Description><IsDefault>Y</IsDefault><IsEnabled>Y</IsEnabled><SubscriptionNodesAsString>TELEV;EXIST;DESC1;SUM1|PRINT;EXIST;DESC2;SUM2</SubscriptionNodesAsString><CampaignType>CRM</CampaignType></SubscriptionRule></ArrayOfSubscriptionRule>");

			var rules3 = new SubscriptionRuleCollection();
			var rule3 = rules3.AddNewRule("CD3", (NoResString)"Default Pubished List ZZZ", false, true, new string[] { "TELEV;EXIST;DESC1;SUM1", "RADIO;PREAP;DESC2;SUM2" });
			rule3.IsDefault = true;

			byte[] byteArrayvalue3 = Encoding.Unicode.GetBytes("<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfSubscriptionRule xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><SubscriptionRule><CodeMaxLength>3</CodeMaxLength><Code>CD3</Code><Description>Default Pubished List ZZZ</Description><IsDefault>Y</IsDefault><IsEnabled>Y</IsEnabled><SubscriptionNodesAsString>TELEV;EXIST;DESC1;SUM1|RADIO;PREAP;DESC2;SUM2</SubscriptionNodesAsString><CampaignType>HRM</CampaignType></SubscriptionRule></ArrayOfSubscriptionRule>");

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(rules1, byteArrayvalue1),
				new ValidSampleAndBinaryValueInDB(rules2, byteArrayvalue2),
				new ValidSampleAndBinaryValueInDB(rules3, byteArrayvalue3),
			};
		}
		#endregion
	}
}
