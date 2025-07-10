using System;
using NUnit.Framework;

namespace CargoWise.Types.Tests
{
	public class ZBlobTest : IZTypeTest
	{
		public void TestEmpty()
		{
			AssertEquals("IsEmpty", true, ZBlob.Empty.IsEmpty);
			AssertEquals("IsValid", true, ZBlob.Empty.IsValid);
			Assert("Equals", ZBlob.Empty.Equals(ZBlob.Empty));
		}

		[ExpectException(typeof(ArgumentOutOfRangeException))]
		public void TestIndexerGetEmpty()
		{
			ZBlob blob = new ZBlob();
			byte dummy = blob[0];
		}

		public void TestConstructorWithNullByteArray()
		{
			byte[] byteArray = null;
			ZBlob blob = new ZBlob(byteArray);
			AssertEquals(ZBlob.Empty, blob);
		}

		[ExpectException(typeof(ArgumentOutOfRangeException))]
		public void TestIndexerGetNegative()
		{
			ZBlob blob = new ZBlob(new byte[] { 1, 4, 9, 16, 25 });
			byte dummy = blob[-1];
		}

		[ExpectException(typeof(ArgumentOutOfRangeException))]
		public void TestIndexerGetTooBig()
		{
			ZBlob blob = new ZBlob(new byte[] { 1, 4, 9, 16, 25 });
			byte dummy = blob[5];
		}

		public void TestIndexerGet()
		{
			ZBlob blob = new ZBlob(new byte[] { 1, 4, 9, 16, 25 });
			AssertEquals((byte)1, blob[0]);
			AssertEquals((byte)4, blob[1]);
			AssertEquals((byte)9, blob[2]);
			AssertEquals((byte)16, blob[3]);
			AssertEquals((byte)25, blob[4]);
		}

		public void TestArrayIsNotCopiedForPerformanceReasons()
		{
			byte[] originalArray = new byte[2000000];
			ZBlob blob = new ZBlob(originalArray);
			ZBlob anotherBlob = new ZBlob(blob);
			byte[] finalArray = anotherBlob;
			Assert(ReferenceEquals(originalArray, finalArray));
		}

		public void TestIndexOf()
		{
			ZBlob blob = new ZBlob(new byte[] { 1, 4, 9, 16, 25, 10, 13, 34, 43, 89, 10, 13, 25, 87, 34, 87, 9 });
			AssertEquals("IndexOf a byte returns the first occurance in the blob", 4, blob.IndexOf(25));
			AssertEquals("IndexOf a byte[] returns the first occurance in the blob", 5, blob.IndexOf(new byte[] { 10, 13 }));
			//			AssertEquals("IndexOf a byte returns the specified occurance in the blob", 12, Blob.IndexOf((byte)25, 2));
			//			AssertEquals("IndexOf a byte[] returns the specified occurance in the blob", 10, Blob.IndexOf(new byte[] { 10, 13 }, 2));

			AssertEquals("IndexOf a byte returns -1 when the first occurance is not found", -1, blob.IndexOf(92));
			AssertEquals("IndexOf a byte[] returns -1 when the first occurance is not found", -1, blob.IndexOf(new byte[] { 11, 23 }));
			//			AssertEquals("IndexOf a byte returns -1 when the specified occurance is not found", -1, Blob.IndexOf((byte)33, 2));
			//			AssertEquals("IndexOf a byte[] returns -1 when the specified occurance is not found", -1, Blob.IndexOf(new byte[] { 11, 23 }, 2));
		}

		public void TestSubBlobSafe()
		{
			ZBlob blob = new ZBlob(new byte[] { 1, 4, 9, 16, 25, 10, 13, 34, 43, 89, 10, 13, 25, 87, 34, 87, 9 });
			AssertEquals("SubBlobSafe with a position at the end, returns the last value", new ZBlob(new byte[] { 9 }), blob.SubBlobSafe(16));
			AssertEquals("SubBlobSafe with a position of 0, returns the whole blob", blob, blob.SubBlobSafe(0));
			AssertEquals("SubBlobSafe the middle of the blob returns the rest", new ZBlob(new byte[] { 34, 43, 89, 10, 13, 25, 87, 34, 87, 9 }), blob.SubBlobSafe(7));
			AssertEquals("SubBlobSafe with a postion less than 0 returns the whole blob", blob, blob.SubBlobSafe(-1));
			AssertEquals("SubBlobSafe of a value greater then the blob returns the whole blob", blob, blob.SubBlobSafe(19));
		}

		#region IZTypeTest Overrides

		public override void TestCompareTo()
		{
			foreach (object value in AllValues)
			{
				bool wasThrown = false;

				try
				{
					NewZ(value).CompareTo(value);
				}
				catch (NotSupportedException)
				{
					wasThrown = true;
				}

				Assert(MessageForValue(value), wasThrown);
			}
		}

		protected override bool IsValueThatWillCreateEmptyZType(object value)
		{
			return ZBlob.Empty.Equals(new ZBlob(value));
		}

		protected override IZType NewZ(object value)
		{
			return new ZBlob(value);
		}

		protected override object[] EmptyValues
		{
			get { return new object[] { null, DBNull.Value, ZBlob.Empty, new ZBlob(), Array.Empty<byte>(), Array.Empty<byte>() }; }
		}

		protected override object[] ValidValues
		{
			get { return new object[] { new byte[] { 0 }, new byte[] { 0, 1 } }; }
		}

		protected override object[] UnsupportedValues
		{
			get { return new object[] { new object(), 0.0, 0M, (byte)0, (short)0 }; }
		}

		#endregion
	}
}
