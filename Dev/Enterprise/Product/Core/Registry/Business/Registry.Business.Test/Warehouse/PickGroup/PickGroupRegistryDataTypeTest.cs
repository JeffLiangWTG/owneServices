using System.Text;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PickGroupRegistryDataType))]
	sealed class PickGroupRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<PickGroupRegistryDataType>
	{
		protected override PickGroupRegistryDataType GetNewDataType()
		{
			return new PickGroupRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "PickGroupRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new PickGroupCollection();
			var pickGroup = collection.AddNew();
			pickGroup.PickSequence = 2;
			pickGroup.Description = (NoResString)"Desc";

			byte[] byteArrayValue = Encoding.Unicode.GetBytes("<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfPickGroup xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><PickGroup><PickSequence>2</PickSequence><Description>Desc</Description></PickGroup></ArrayOfPickGroup>");

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}
	}
}
