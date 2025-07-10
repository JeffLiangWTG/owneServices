using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PersonMergePreviewItemRegistryDataType))]
	sealed class PersonMergePreviewItemRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<PersonMergePreviewItemRegistryDataType>
	{
		protected override PersonMergePreviewItemRegistryDataType GetNewDataType()
		{
			return new PersonMergePreviewItemRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var firstCollection = new PersonMergePreviewItemCollection();
			var bizoA = firstCollection.AddNew();
			bizoA.FriendlyName = "Full Name1";
			bizoA.ColumnName = "PER_FullName1";
			bizoA.Visibility = true;

			var secondCollection = new PersonMergePreviewItemCollection();
			var bizoB = secondCollection.AddNew();
			bizoB.FriendlyName = "Full Name2";
			bizoB.ColumnName = "PER_FullName2";
			bizoB.Visibility = true;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(firstCollection, new PersonMergePreviewItemRegistryDataType().Serialise(firstCollection)),
				new ValidSampleAndBinaryValueInDB(secondCollection, new PersonMergePreviewItemRegistryDataType().Serialise(secondCollection))
			};
		}

		protected override string ExpectedEditorName
		{
			get { return "PersonMergePreviewItemsRegistryItemEditor"; }
		}
	}
}
