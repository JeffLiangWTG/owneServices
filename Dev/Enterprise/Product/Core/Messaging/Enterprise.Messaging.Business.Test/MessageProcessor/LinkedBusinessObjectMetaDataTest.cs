using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Testing;

namespace Enterprise.Messaging.Business.MessageProcessor.Testing
{
	sealed class LinkedBusinessObjectMetaDataTest : TestCaseWithFactory
	{
		public void TestEmpty()
		{
			AssertSame(LinkedBusinessObjectMetaData.Empty, LinkedBusinessObjectMetaData.Empty);
		}

		public void TestConstructor()
		{
			Assert(ReferenceEquals(LinkedBusinessObjectMetaData.Empty, LinkedBusinessObjectMetaData.New(ZString.Empty, ZGuid.Empty, ZGuid.Empty, ZString.Empty)));
			var metaData = new LinkedBusinessObjectMetaData(DummyBusinessObjectSchema.Constants.TableName, ZGuid.BrettsGuid, ZGuid.BrettsGuid, "123");
			CombineAssertions(() =>
			{
				AssertEquals(metaData.LinkTableName, DummyBusinessObjectSchema.Constants.TableName);
				AssertEquals(metaData.LinkUniqueID, ZGuid.BrettsGuid);
				AssertEquals(metaData.BranchPk, ZGuid.BrettsGuid);
				AssertEquals(metaData.JobNumber, "123");
			});
		}

		public void TestEquals()
		{
			var metaData1 = new LinkedBusinessObjectMetaData(DummyBusinessObjectSchema.Constants.TableName, ZGuid.BrettsGuid, ZGuid.BrettsGuid, "123");
			var metaData2 = new LinkedBusinessObjectMetaData(DummyBusinessObjectSchema.Constants.TableName, ZGuid.BrettsGuid, ZGuid.BrettsGuid, "123");
			var metaData3 = new LinkedBusinessObjectMetaData("HI", ZGuid.BrettsGuid, ZGuid.BrettsGuid, "123");
			var metaData4 = new LinkedBusinessObjectMetaData(DummyBusinessObjectSchema.Constants.TableName, ZGuid.Invalid, ZGuid.BrettsGuid, "123");
			var metaData5 = new LinkedBusinessObjectMetaData(DummyBusinessObjectSchema.Constants.TableName, ZGuid.BrettsGuid, ZGuid.Invalid, "123");
			var metaData6 = new LinkedBusinessObjectMetaData(DummyBusinessObjectSchema.Constants.TableName, ZGuid.BrettsGuid, ZGuid.BrettsGuid, "456");
			AssertEquals("metaData1.Equals(metaData2)", true, metaData1.Equals(metaData2));
			AssertEquals("metaData2.Equals(metaData3)", false, metaData2.Equals(metaData3));
			AssertEquals("metaData2.Equals(metaData4)", false, metaData2.Equals(metaData4));
			AssertEquals("metaData2.Equals(metaData5)", false, metaData2.Equals(metaData5));
			AssertEquals("metaData2.Equals(metaData6)", false, metaData2.Equals(metaData6));
			AssertEquals("metaData3.Equals(metaData4)", false, metaData3.Equals(metaData4));
		}

		public void TestGetHashCode()
		{
			var metaData1 = new LinkedBusinessObjectMetaData(DummyBusinessObjectSchema.Constants.TableName, ZGuid.BrettsGuid, ZGuid.BrettsGuid, "123");
			var metaData2 = new LinkedBusinessObjectMetaData(DummyBusinessObjectSchema.Constants.TableName, ZGuid.BrettsGuid, ZGuid.BrettsGuid, "123");
			var metaData3 = new LinkedBusinessObjectMetaData("HI", ZGuid.BrettsGuid, ZGuid.BrettsGuid, "123");
			var metaData4 = new LinkedBusinessObjectMetaData(DummyBusinessObjectSchema.Constants.TableName, ZGuid.Invalid, ZGuid.BrettsGuid, "123");
			var metaData5 = new LinkedBusinessObjectMetaData(DummyBusinessObjectSchema.Constants.TableName, ZGuid.BrettsGuid, ZGuid.Invalid, "123");
			var metaData6 = new LinkedBusinessObjectMetaData(DummyBusinessObjectSchema.Constants.TableName, ZGuid.BrettsGuid, ZGuid.BrettsGuid, "456");
			AssertEquals(metaData1.GetHashCode(), metaData2.GetHashCode());
			AssertNotEquals(metaData1.GetHashCode(), metaData3.GetHashCode());
			AssertNotEquals(metaData1.GetHashCode(), metaData4.GetHashCode());
			AssertNotEquals(metaData1.GetHashCode(), metaData5.GetHashCode());
			AssertNotEquals(metaData1.GetHashCode(), metaData6.GetHashCode());
		}

		public void TestToString()
		{
			var metaData = new LinkedBusinessObjectMetaData(DummyBusinessObjectSchema.Constants.TableName, ZGuid.Invalid, ZGuid.Invalid, "123");
			AssertEquals(typeof(LinkedBusinessObjectMetaData).ToString() + $" (LinkTableName: DummyBizo, LinkUniqueID: {ZGuid.Invalid}, BranchPk: {ZGuid.Invalid}, JobNumber: 123)", metaData.ToString());
		}
	}
}
