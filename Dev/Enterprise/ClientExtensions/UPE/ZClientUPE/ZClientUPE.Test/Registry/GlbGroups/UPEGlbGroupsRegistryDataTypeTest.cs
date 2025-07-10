using Enterprise.Client.UPE.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Registry.Testing
{
	[TestedType(typeof(UPEGlbGroupsRegistryDataType))]
	class UPEGlbGroupsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<UPEGlbGroupsRegistryDataType>
	{
		#region Implementation
		protected override UPEGlbGroupsRegistryDataType GetNewDataType()
		{
			return new UPEGlbGroupsRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get
			{
				return "UPEGlbGroupsRegistryItemEditor";
			}
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			UPEGlbGroupsRegistryObjectCollection collection = new UPEGlbGroupsRegistryObjectCollection();
			collection.AddNew().Group = Core.Constants.Groups.AllPK;
			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB(collection, DataType.Serialise(collection)) };
		}
		#endregion
	}
}
