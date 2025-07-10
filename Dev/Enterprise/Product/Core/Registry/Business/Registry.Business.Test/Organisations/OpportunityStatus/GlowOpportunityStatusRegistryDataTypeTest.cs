using System.Text;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(GlowOpportunityStatusRegistryDataType))]
	sealed class GlowOpportunityStatusRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeWithEnabledItemTest<GlowOpportunityStatusRegistryDataType>
	{
		protected override GlowOpportunityStatusRegistryDataType GetNewDataType()
		{
			var defaultStatuses = new GlowOpportunityStatusCollection();
			defaultStatuses.Add("AAA", (NoResString)"AAA Description", enabled: true, OpportunityTradeStatus.Codes.Active);
			defaultStatuses.Add("ZZZ", (NoResString)"ZZZ Description", enabled: true, OpportunityTradeStatus.Codes.Successful);
			return new GlowOpportunityStatusRegistryDataType(defaultStatuses);
		}

		protected override string ExpectedEditorName
		{
			get { return "GlowOpportunityStatusRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var defaultStatuses = new GlowOpportunityStatusCollection();
			var status1 = defaultStatuses.AddNew();
			status1.Code = "AAA";
			status1.EnglishDescription = "AAA Description";
			status1.Bool = true;
			status1.TradeStatus = OpportunityTradeStatus.Codes.Active;

			var status2 = defaultStatuses.AddNew();
			status2.Code = "ZZZ";
			status2.EnglishDescription = "ZZZ Description";
			status2.Bool = true;
			status2.TradeStatus = OpportunityTradeStatus.Codes.Successful;

			var xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<ArrayOfGlowOpportunityStatus xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
				"<GlowOpportunityStatus><CodeMaxLength>3</CodeMaxLength><Code>AAA</Code><Description>AAA Description</Description><Bool>Y</Bool><SystemDefined>False</SystemDefined><TradeStatus>ACT</TradeStatus></GlowOpportunityStatus>" +
				"<GlowOpportunityStatus><CodeMaxLength>3</CodeMaxLength><Code>ZZZ</Code><Description>ZZZ Description</Description><Bool>Y</Bool><SystemDefined>False</SystemDefined><TradeStatus>SUC</TradeStatus></GlowOpportunityStatus>" +
				"</ArrayOfGlowOpportunityStatus>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(defaultStatuses, Encoding.Unicode.GetBytes(xml))
			};
		}

		protected override IRegistryItem GetRegistryItem() => OrganisationsDataRegistry.Instance.GlowOpportunityStatuses;

		protected override string ExpectedExceptionMessage => "At least one each of ACT, UNS and SUC must be available and enabled.";

		protected override RegistryBusinessObjectCollection[] RegistriesWithEnabledItem()
		{
			var defaultValues = new GlowOpportunityStatusCollection();
			defaultValues.Add("NEW", (NoResString)"New", true, OpportunityTradeStatus.Codes.Successful);
			defaultValues.Add("AAA", (NoResString)"aaa", true, OpportunityTradeStatus.Codes.Unsuccessful);
			defaultValues.Add("BBB", (NoResString)"bbb", true, OpportunityTradeStatus.Codes.Active);

			return [defaultValues];
		}

		protected override RegistryBusinessObjectCollection[] RegistriesWithoutEnabledItem()
		{
			var defaultValues1 = new GlowOpportunityStatusCollection();

			var defaultValues2 = new GlowOpportunityStatusCollection();
			defaultValues2.Add("NEW", (NoResString)"New", true, OpportunityTradeStatus.Codes.Successful);

			var defaultValues3 = new GlowOpportunityStatusCollection();
			defaultValues3.Add("NEW", (NoResString)"New", true, OpportunityTradeStatus.Codes.Successful);
			defaultValues3.Add("AAA", (NoResString)"aaa", true, OpportunityTradeStatus.Codes.Unsuccessful);

			var defaultValues4 = new GlowOpportunityStatusCollection();
			defaultValues4.Add("NEW", (NoResString)"New", false, OpportunityTradeStatus.Codes.Successful);
			defaultValues4.Add("AAA", (NoResString)"aaa", true, OpportunityTradeStatus.Codes.Unsuccessful);
			defaultValues4.Add("BBB", (NoResString)"bbb", true, OpportunityTradeStatus.Codes.Active);

			return [defaultValues1, defaultValues2, defaultValues3, defaultValues4];
		}
	}
}
