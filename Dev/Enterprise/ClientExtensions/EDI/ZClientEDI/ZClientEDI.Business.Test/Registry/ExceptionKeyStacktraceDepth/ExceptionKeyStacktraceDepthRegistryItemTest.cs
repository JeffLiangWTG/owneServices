using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Client.EDI.Registry.Business.ExceptionKeyStacktraceDepthRegistryItem;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(ExceptionKeyStacktraceDepthRegistryItem))]
	class ExceptionKeyStacktraceDepthRegistryItemTest : StronglyTypedRegistryItemTestCase<ExceptionKeyStacktraceDepthCollection>
	{
		protected override StronglyTypedRegistryItem<ExceptionKeyStacktraceDepthCollection, ExceptionKeyStacktraceDepthCollection> GetNewRegistryItem()
		{
			return new ExceptionKeyStacktraceDepthRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.NotCached);
		}
	}

	[TestedType(typeof(ExceptionKeyStacktraceDepthRegistryDataType))]
	class ExceptionKeyStacktraceDepthRegistryDataTypeNonPersistentTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ExceptionKeyStacktraceDepthRegistryDataType>
	{
		protected override string ExpectedEditorName => "ExceptionKeyStacktraceDepthRegistryEditor";
		protected override ExceptionKeyStacktraceDepthRegistryDataType GetNewDataType()
		{
			return new ExceptionKeyStacktraceDepthRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new ExceptionKeyStacktraceDepthCollection();
			var item = collection.AddNew();
			item.ReadOnly = false;
			item.StackDepth = 2;

			var collection2 = new ExceptionKeyStacktraceDepthCollection();
			var item2 = collection.AddNew();
			item2.ReadOnly = true;
			item2.StackDepth = 3;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new ExceptionKeyStacktraceDepthRegistryDataType().Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, new ExceptionKeyStacktraceDepthRegistryDataType().Serialise(collection2))
			};
		}
	}
}
