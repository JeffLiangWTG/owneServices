using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class AESInboundEDIMessageLookupsTest : TestCaseWithFactory
	{
		public void TestMessageTypeList()
		{
			var list = Factory.New<AESInboundEDIMessage>().Lookups.MessageTypeList;
			AssertSame("MessageTypeList", list, Factory.New<AESInboundEDIMessage>().Lookups.MessageTypeList);
			var dictionary = new SortedDictionary<string, string>();
			AddToDictionary(dictionary, new AESOutgoingMessageTypeList());
			AddToDictionary(dictionary, new AESIncomingMessageTypeList());
			var count = dictionary.Count;
			AssertEquals("Count", count, list.Count);
			var i = 0;
			foreach (var pair in dictionary)
			{
				var actual = list[i++];
				AssertEquals("Code", pair.Key, actual.Code);
				AssertEquals("Description", pair.Value, actual.Description);
			}
		}

		void AddToDictionary(SortedDictionary<string, string> dictionary, ICodeDescriptionPairList list)
		{
			foreach (ICodeDescription pair in list)
			{
				dictionary.Add(pair.Code, pair.Description);
			}
		}
	}
}
