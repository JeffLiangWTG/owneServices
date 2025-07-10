using System;
using System.Text;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AutomaticProcessRegistryDataType<AutomaticProcessRegistryBusinessObject>))]
	sealed class AutomaticProcessRegistryDataTypeGenericTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<AutomaticProcessRegistryDataType<AutomaticProcessRegistryBusinessObject>>
	{
		protected override AutomaticProcessRegistryDataType<AutomaticProcessRegistryBusinessObject> GetNewDataType()
		{
			return new AutomaticProcessRegistryDataType<AutomaticProcessRegistryBusinessObject>();
		}

		protected override string ExpectedEditorName
		{
			get { return "AutomaticProcessRegistryItemEditor"; }
		}

		[TestDate(2006, 1, 1, 1, 1, 50)]
		public override void TestGetSetValidValues()
		{
			base.TestGetSetValidValues();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			AutomaticProcessRegistryBusinessObject bizObj = new AutomaticProcessRegistryBusinessObject();
			bizObj.Interval = 2;
			bizObj.LastRunDateTime = new DateTime(2006, 1, 1, 1, 1, 50);
			bizObj.NextRunDateTime = new DateTime(2006, 1, 2, 1, 1, 50);
			AutomaticProcessRegistryBusinessObject bizObj2 = new AutomaticProcessRegistryBusinessObject();
			bizObj2.Interval = 1;
			bizObj2.LastRunDateTime = new DateTime(2016, 1, 1, 1, 1, 50);
			bizObj2.NextRunDateTime = new DateTime(2016, 1, 2, 1, 1, 50);

			string xml = @"<?xml version=""1.0"" encoding=""utf-16""?>
				<AutomaticProcessRegistryBusinessObject>
					<NextRunDateTime>2006-01-02 01:01:50.000</NextRunDateTime>
					<LastRunDateTime>2006-01-01 01:01:50.000</LastRunDateTime>
					<Interval>2</Interval>
					<IntervalType>DAYS</IntervalType>
				</AutomaticProcessRegistryBusinessObject>";
			string xml2 = @"<?xml version=""1.0"" encoding=""utf-16""?>
				<AutomaticProcessRegistryBusinessObject>
					<NextRunDateTime>2016-01-02 01:01:50.000</NextRunDateTime>
					<LastRunDateTime>2016-01-01 01:01:50.000</LastRunDateTime>
					<Interval>1</Interval>
					<IntervalType>DAYS</IntervalType>
				</AutomaticProcessRegistryBusinessObject>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(bizObj, Encoding.Unicode.GetBytes(xml)),
				new ValidSampleAndBinaryValueInDB(bizObj2, Encoding.Unicode.GetBytes(xml2))
			};
		}
	}
}
