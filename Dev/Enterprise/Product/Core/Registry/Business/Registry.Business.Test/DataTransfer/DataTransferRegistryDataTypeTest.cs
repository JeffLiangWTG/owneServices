using System;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DataTransferRegistryDataType))]
	sealed class DataTransferRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DataTransferRegistryDataType>
	{
		protected override DataTransferRegistryDataType GetNewDataType()
		{
			return new DataTransferRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get
			{
				return "DataTransferRegistryItemEditor";
			}
		}

		[TestDate(2006, 1, 1, 1, 1, 50)]
		public override void TestGetSetValidValues()
		{
			base.TestGetSetValidValues();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			DataTransferRegistryBusinessObject bizObj = new DataTransferRegistryBusinessObject();
			bizObj.Directory = TempForTest.TempPath;
			bizObj.Interval = 2;
			bizObj.LastRunDateTime = new DateTime(2006, 1, 1, 1, 1, 50);
			bizObj.NextRunDateTime = new DateTime(2006, 1, 2, 1, 1, 50);
			bizObj.GroupPK = Core.Constants.Groups.PostMastersGroupPK;

			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB(bizObj, DataType.Serialise(bizObj)) };
		}
	}
}
