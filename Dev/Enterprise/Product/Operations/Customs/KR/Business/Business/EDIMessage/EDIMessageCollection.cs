using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	public class EDIMessageCollection : Enterprise.Messaging.Business.EDIMessageCollection
	{
		public EDIMessageCollection(BusinessObject master) : base(master)
		{
		}

		public new EDIMessage this[int index] => (EDIMessage)base[index];

		public new EDIMessage AddNew() => (EDIMessage)base.AddNew();

		public EDIMessage GetLastMessageMatching(Func<EDIMessage, bool> filter)
		{
			return this.Cast<EDIMessage>().Where(filter).OrderBy(x => x.EM_SystemCreateTimeUtc).LastOrDefault();
		}

		public EDIMessage GetLastMessageWithMatchingVersionNumber(ZString messageType, ZString versionNumber)
		{
			return GetLastMessageMatching(x => x.EM_MessageType == messageType && x.EM_ApplicationReference == versionNumber);
		}
	}
}
