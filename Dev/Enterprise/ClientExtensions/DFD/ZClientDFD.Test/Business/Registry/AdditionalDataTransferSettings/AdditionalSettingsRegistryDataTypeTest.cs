using System;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.DFD.Registry.Testing
{
	[TestedType(typeof(AdditionalSettingsRegistryDataType))]
	internal class AdditionalSettingsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<AdditionalSettingsRegistryDataType>
	{
		protected override AdditionalSettingsRegistryDataType GetNewDataType()
		{
			return new AdditionalSettingsRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get
			{
				return "AdditionalSettingsRegistryItemEditor";
			}
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			AdditionalSettingsRegistryBusinessObject bizObj = new AdditionalSettingsRegistryBusinessObject();
			bizObj.ExportFileName = "FILENAME";
			bizObj.Directory = TempForTest.TempPath;
			bizObj.Interval = 2;
			bizObj.LastRunDateTime = new DateTime(2006, 1, 1, 1, 1, 50);
			bizObj.NextRunDateTime = new DateTime(2006, 1, 2, 1, 1, 50);
			bizObj.GroupPK = Core.Constants.Groups.PostMastersGroupPK;
			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB(bizObj, DataType.Serialise(bizObj)) };
		}
	}
}
