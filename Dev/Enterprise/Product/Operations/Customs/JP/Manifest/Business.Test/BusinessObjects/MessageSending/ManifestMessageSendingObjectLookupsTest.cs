using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.JP.Common;
using NUnit.Framework;
using static Enterprise.Customs.JP.Common.JPMessageActionList;
using static Enterprise.Customs.JP.Common.JPProcedureCodeList;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(ManifestMessageSendingObjectLookups))]
	sealed class ManifestMessageSendingObjectLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestReasonList()
		{
			AssertType<ReasonList>(Lookups.ReasonList);
		}

		public void TestMessageTypeList()
		{
			AssertType<JPManifestProcedureCodeList>(Lookups.MessageTypeList);
		}

		public void TestMessageActionList()
		{
			AssertMessageActionListType<HDF01MessageActionList>(Header, JPProcedureCodeList.Codes.HDF01);
			AssertMessageActionListType<NVC01MessageActionList>(Header, JPProcedureCodeList.Codes.NVC01);
		}

		void AssertMessageActionListType<T>(AsycudaManifestHeader header, string procedureCode)
		{
			using (header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = procedureCode }))
			{
				var sendingObjectParent = new ManifestMessageSendingObjectParent(header);
				var sendingObject = sendingObjectParent.SendingObjectsCollection.FirstOrDefault() as ManifestMessageSendingObject;
				AssertType<T>(sendingObject.Lookups.ActionList);
			}
		}

		AsycudaManifestHeader Header
		{
			get
			{
				header ??= Factory.New<AsycudaManifestHeader>();
				header.Bills.AddNew();
				return header;
			}
		}
		AsycudaManifestHeader header;

		ManifestMessageSendingObjectParent SendingObjectParent => sendingObjectParent ??= new ManifestMessageSendingObjectParent(Header);
		ManifestMessageSendingObjectParent sendingObjectParent;

		ManifestMessageSendingObject SendingObject => sendingObject ??= SendingObjectParent.SendingObjectsCollection.FirstOrDefault() as ManifestMessageSendingObject;
		ManifestMessageSendingObject sendingObject;

		ManifestMessageSendingObjectLookups Lookups => SendingObject.Lookups;
	}
}
