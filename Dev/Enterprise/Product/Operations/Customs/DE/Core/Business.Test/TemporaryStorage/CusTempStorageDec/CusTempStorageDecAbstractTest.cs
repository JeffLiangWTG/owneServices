using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestsSubclassesOf(typeof(CusTempStorageDec))]
	public abstract class CusTempStorageDecAbstractTest<T> : EnterpriseBusinessObjectTestCase
		where T : CusTempStorageDec
	{
		public void TestGetLine()
		{
			var storageDec = GetCusTempStorageDecForTesting();
			var line = storageDec.CusTempStorageLines.AddNew();
			line.TSL_LineNo = new ZInt(2);
			CombineAssertions(() =>
			{
				AssertNull("Empty string", storageDec.GetLine(ZString.Empty));
				AssertNull("Non-numeric string", storageDec.GetLine("X1"));
				AssertEquals("Good job", line, storageDec.GetLine("2"));
			});
		}

		public void TestMessages()
		{
			var storageDec = GetCusTempStorageDecForTesting();
			var message = storageDec.Messages.AddNew();
			AssertEquals(storageDec, message.EM_LinkedObject);
			AssertEquals(storageDec.TableName, message.EM_LinkTable);
			AssertEquals(storageDec.PK, message.EM_LinkUniqueID);
			AssertEquals(EDIMessageStatusList.Codes.Queued, message.EM_Status);
			AssertEquals(true, message.ReadOnly);
		}

		protected abstract T GetCusTempStorageDecForTesting();
	}
}
