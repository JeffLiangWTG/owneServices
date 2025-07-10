using System;
using Enterprise.Metadata.Integration;
using NUnit.Framework;

namespace Enterprise.Metadata.Business.Tests
{
	public class MetadataProviderTest : TestCase
	{
		public void TestGetMetadataFromBO()
		{
			var dummyBO = new DummyBO11() { IsDummy = true };
			var actual = metadataProvider.GetMetadataFromBO(dummyBO);
			AssertType(typeof(DummyBO), actual);
			Assert(((IDummyBOShareProperty)actual).IsDummy);

			var linkedMetadata = ((IDummyBOLinkedMetadata)actual).LinkedBO;
			AssertType(typeof(Business.DummyLinkedBO), linkedMetadata);
			var linkedMetadatas = ((IDummyBOLinkedMetadata)actual).LinkedBOs;
			AssertType(typeof(IMetadata[]), linkedMetadatas);
			AssertEquals(2, linkedMetadatas.Length);
			AssertType(typeof(Business.DummyLinkedBO), linkedMetadatas[0]);
			AssertType(typeof(Business.DummyLinkedBO), linkedMetadatas[1]);

			actual = metadataProvider.GetMetadataFromBO(new DummyBO12());
			AssertEquals(null, actual);

			actual = metadataProvider.GetMetadataFromBO(null);
			AssertEquals(null, actual);
		}

		public void TestGetMetadataFromBO_Inheritance()
		{
			var dummyBO = new DummyBO11Child() { IsDummy = true };
			var actual = metadataProvider.GetMetadataFromBO(dummyBO);
			AssertType(typeof(DummyBOChild), actual);
			Assert(((IDummyBOShareProperty)actual).IsDummy);

			var linkedMetadata = ((IDummyBOLinkedMetadata)actual).LinkedBO;
			AssertType(typeof(Business.DummyLinkedBO), linkedMetadata);
			var linkedMetadatas = ((IDummyBOLinkedMetadata)actual).LinkedBOs;
			AssertType(typeof(IMetadata[]), linkedMetadatas);
			AssertEquals(2, linkedMetadatas.Length);
			AssertType(typeof(Business.DummyLinkedBO), linkedMetadatas[0]);
			AssertType(typeof(Business.DummyLinkedBO), linkedMetadatas[1]);
		}

		#region Help TestGetMetadataFromBO

		[MetadataContext(MetadataContext.DummyBO)]
		public class DummyBO11
		{
			public bool IsDummy { get; set; }
			public DummyLinkedBO LinkedBO { get { return new DummyLinkedBO(); } }
			public DummyLinkedBO[] LinkedBOs { get { return new DummyLinkedBO[] { new DummyLinkedBO(), new DummyLinkedBO() }; } }
		}

		[MetadataContext(MetadataContext.DummyLinkedBO)]
		public class DummyLinkedBO
		{
		}

		public class DummyBO12 { }

		[MetadataContext(MetadataContext.DummyBOChild)]
		public class DummyBO11Child : DummyBO11 { }

		#endregion

		public void TestGetMetadataFromBOThrowException()
		{
			var expectedExceptionType = typeof(InvalidOperationException);
			var expectedExceptionMessage = "No public property 'IsDummy' is defined in business object type 'DummyBO13'.";
			var dummyBO = new DummyBO13();
			AssertExceptionThrown(expectedExceptionType, expectedExceptionMessage, () => { metadataProvider.GetMetadataFromBO(dummyBO); });
		}

		#region Help TestGetMetadataFromBOThrowException

		[MetadataContext(MetadataContext.DummyBO)]
		public class DummyBO13 { }

		#endregion

		public void GetMetadataFromType()
		{
			var actual = metadataProvider.GetMetadataFromType(typeof(DummyBO21));
			AssertType(typeof(DummyBO), actual);

			actual = metadataProvider.GetMetadataFromType(typeof(DummyBO22));
			AssertType(typeof(DummyBO), actual);

			actual = metadataProvider.GetMetadataFromType(typeof(DummyBO23));
			AssertEquals(null, actual);
		}

		#region Help GetMetadataFromType

		[MetadataContext(MetadataContext.DummyBO)]
		class DummyBO21 { }

		[MetadataContext(MetadataContext.NotApplied, "Enterprise.Metadata.Business.DummyBO, Enterprise.Metadata.Business, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350")]
		class DummyBO22 { }

		class DummyBO23 { }

		#endregion

		public void GetMetadataFromTypeThrowException()
		{
			var expectedExceptionType = typeof(InvalidOperationException);
			var expectedExceptionMessage = "No metadata is defined for context 'NotApplied'.";
			AssertExceptionThrown(expectedExceptionType, expectedExceptionMessage, () => { metadataProvider.GetMetadataFromType(typeof(DummyBO24)); });
		}

		#region Help GetMetadataFromTypeThrowException

		[MetadataContext(MetadataContext.NotApplied)]
		class DummyBO24 { }

		#endregion

		readonly MetadataProvider metadataProvider = new MetadataProvider();
	}
}
