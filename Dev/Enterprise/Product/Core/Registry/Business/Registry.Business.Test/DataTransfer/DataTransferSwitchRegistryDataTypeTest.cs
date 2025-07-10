using System;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(Enterprise.Registry.Business.DataTransferSwitchRegistryItem.DataTransferSwitchRegistryDataType))]
	sealed class DataTransferSwitchRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<Enterprise.Registry.Business.DataTransferSwitchRegistryItem.DataTransferSwitchRegistryDataType>
	{
		protected override Enterprise.Registry.Business.DataTransferSwitchRegistryItem.DataTransferSwitchRegistryDataType GetNewDataType()
		{
			return new Enterprise.Registry.Business.DataTransferSwitchRegistryItem.DataTransferSwitchRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get
			{
				return "DataTransferSwitchRegistryItemEditor";
			}
		}

		[TestDate(2006, 1, 1, 1, 1, 50)]
		public override void TestGetSetValidValues()
		{
			base.TestGetSetValidValues();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			DataTransferSwitchRegistryBusinessObject bizObj = new DataTransferSwitchRegistryBusinessObject();
			bizObj.Directory = TempForTest.TempPath;
			bizObj.Interval = 2;
			bizObj.LastRunDateTime = new DateTime(2006, 1, 1, 1, 1, 50);
			bizObj.NextRunDateTime = new DateTime(2006, 1, 2, 1, 1, 50);
			bizObj.GroupPK = Core.Constants.Groups.PostMastersGroupPK;
			bizObj.EnableInterface = true;

			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB(bizObj, DataType.Serialise(bizObj)) };
		}
	}
}
