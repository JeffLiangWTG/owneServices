using System.Text;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(ScavengingPurgeSettingsRegistryDataType))]
	class ScavengingPurgeSettingsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ScavengingPurgeSettingsRegistryDataType>
	{
		protected override ScavengingPurgeSettingsRegistryDataType GetNewDataType()
		{
			return new ScavengingPurgeSettingsRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var settings = new ScavengingPurgeSettings();
			var clientStatistics = settings.AddNew();
			clientStatistics.Code = ScavengingPurgeSettings.ClientStatisticsArchiveCode;
			clientStatistics.Description = (NoResString)"Client Statistics XML Archive";
			clientStatistics.PurgeTime = 6;
			clientStatistics.PurgeTimeUnit = ScavengingPurgeItem.TimeUnit.Month;

			return new[] { new ValidSampleAndBinaryValueInDB(
				settings,
				Encoding.Unicode.GetBytes("<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfScavengingPurgeItem xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><ScavengingPurgeItem><CodeMaxLength>3</CodeMaxLength><Code>CSA</Code><Description>Client Statistics XML Archive</Description><PurgeTime>6</PurgeTime><PurgeTimeUnit>a25a1a69-19e8-499b-bd74-053cccd00f7f</PurgeTimeUnit></ScavengingPurgeItem></ArrayOfScavengingPurgeItem>")) };
		}

		protected override string ExpectedEditorName
		{
			get { return "ScavengingPurgeSettingsRegistryEditor"; }
		}
	}
}
