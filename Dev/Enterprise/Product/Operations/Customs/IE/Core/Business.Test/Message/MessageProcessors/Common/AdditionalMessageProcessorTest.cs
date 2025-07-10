using System.Collections;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.Testing
{
	class AdditionalMessageProcessorTest : TestCaseWithFactory
	{
		public void TestSetUpAdditionalProcessors()
		{
			var hashTable = (Hashtable)ObjectFactory.Get("IEAdditionalMessageProcessings");
			AssertNotNull(hashTable);
			CombineAssertions(() =>
			{
				foreach (var key in ProcessorKeys())
				{
					AssertEquals($"Key {key} not found in list of additional processors", true, hashTable.ContainsKey(key));
				}
			});
		}

		IEnumerable<string> ProcessorKeys()
		{
			yield return $"{EDIMessage.ApplicationCodes.IECustomsExport}|{AESOutgoingMessageTypeList.Codes.ExitNotification}|{EDIMessage.Status.Acknowledged}";
		}
	}
}
