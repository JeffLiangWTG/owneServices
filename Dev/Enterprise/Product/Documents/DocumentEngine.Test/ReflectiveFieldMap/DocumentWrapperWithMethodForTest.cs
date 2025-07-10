using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;

namespace Enterprise.DocumentEngine.ReflectiveFieldMap.Testing
{
	abstract class DocumentWrapperWithMethodForTest : DocumentWrapperForTest
	{
		public abstract DocumentWrapperWithMethodForTest RelationWithMethod { get; }

		[DocumentField("Test Method 1")]
		public abstract DocumentWrapperWithMethodForTest GetRelationWithMethod(ZString param1, ZInt param2);

		[DocumentField("Test Method 2")]
		public abstract ZString GetZStringIsIn();

		[DocumentField("Test Method 3")]
		public abstract DocumentWrapperCollectionForTest GetCollection();

		[DocumentField("Test Method 4")]
		public abstract ZString[] GetCollectionIsIn();
	}
}
