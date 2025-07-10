using System.Text;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OpportunityStatusRegistryDataType))]
	sealed class OpportunityStatusRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<OpportunityStatusRegistryDataType>
	{
		protected override OpportunityStatusRegistryDataType GetNewDataType()
		{
			var defaultStatuses = new OpportunityStatusCollection();
			defaultStatuses.Add("AAA", (NoResString)"AAA Description", true, false, true, OpportunityTradeStatus.Codes.Active);
			defaultStatuses.Add("ZZZ", (NoResString)"ZZZ Description", false, false, true, OpportunityTradeStatus.Codes.Successful);
			return new OpportunityStatusRegistryDataType(defaultStatuses);
		}

		protected override string ExpectedEditorName
		{
			get { return "OpportunityStatusRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var defaultStatuses = new OpportunityStatusCollection();
			var status1 = defaultStatuses.AddNew();
			status1.Code = "AAA";
			status1.EnglishDescription = "AAA Description";
			status1.Bool = true;
			status1.EffectiveAgreement = false;
			status1.Enabled = false;
			status1.TradeStatus = OpportunityTradeStatus.Codes.Active;

			var status2 = defaultStatuses.AddNew();
			status2.Code = "ZZZ";
			status2.EnglishDescription = "ZZZ Description";
			status2.Bool = false;
			status2.EffectiveAgreement = false;
			status2.Enabled = true;
			status2.TradeStatus = OpportunityTradeStatus.Codes.Successful;

			byte[] byteArrayvalue = Encoding.Unicode.GetBytes("<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfOpportunityStatus xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><OpportunityStatus><CodeMaxLength>3</CodeMaxLength><Code>AAA</Code><Description>AAA Description</Description><Bool>Y</Bool><EffectiveAgreement>N</EffectiveAgreement><Enabled>N</Enabled><TradeStatus>ACT</TradeStatus></OpportunityStatus><OpportunityStatus><CodeMaxLength>3</CodeMaxLength><Code>ZZZ</Code><Description>ZZZ Description</Description><Bool>N</Bool><EffectiveAgreement>N</EffectiveAgreement><Enabled>Y</Enabled><TradeStatus>SUC</TradeStatus></OpportunityStatus></ArrayOfOpportunityStatus>");

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(defaultStatuses, byteArrayvalue)
			};
		}
	}
}
