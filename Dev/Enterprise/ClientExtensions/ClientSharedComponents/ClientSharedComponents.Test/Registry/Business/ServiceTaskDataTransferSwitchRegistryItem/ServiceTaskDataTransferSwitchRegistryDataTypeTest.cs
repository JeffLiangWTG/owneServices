using System;
using CargoWise.IO;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(ServiceTaskDataTransferSwitchRegistryDataType))]
	class ServiceTaskDataTransferSwitchRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ServiceTaskDataTransferSwitchRegistryDataType>
	{
		protected override ServiceTaskDataTransferSwitchRegistryDataType GetNewDataType()
		{
			return new ServiceTaskDataTransferSwitchRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "ServiceTaskDataTransferSwitchRegistryItemEditor"; }
		}

		[TestDate(2006, 1, 1, 1, 1, 50)]
		public override void TestGetSetValidValues()
		{
			base.TestGetSetValidValues();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			DataTransferSwitchRegistryBusinessObject bizObj = new DataTransferSwitchRegistryBusinessObject();
			bizObj.Directory = Temp.TempPath;
			bizObj.Interval = 2;
			bizObj.LastRunDateTime = new DateTime(2006, 1, 1, 1, 1, 50);
			bizObj.NextRunDateTime = new DateTime(2006, 1, 2, 1, 1, 50);
			bizObj.GroupPK = Core.Constants.Groups.PostMastersGroupPK;

			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB(bizObj, DataType.Serialise(bizObj)) };
		}
	}
}
