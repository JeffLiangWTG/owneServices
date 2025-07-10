using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(EDIMessageCollection))]
	class EDIMessageCollectionTest : Enterprise.Messaging.Business.EDIMessageCollectionTest
	{
		public void TestAllowRemove()
		{
			var collection = entry.Messages;
			AssertEquals("AllowRemove", false, collection.AllowRemove);
		}

		public void TestIsMessageNewer()
		{
			var collection = new EDIMessageCollectionForTesting(Factory.New<JobDeclaration>());
			var ediMessage1 = Factory.New<EDIMessage>();
			ediMessage1.EM_ApplicationCode = "APA";
			ediMessage1.EM_MessageType = "MTA";
			ediMessage1.EM_Status = "STA";
			ediMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now;
			var ediMessage2 = Factory.New<EDIMessage>();
			ediMessage2.EM_ApplicationCode = "APA";
			ediMessage2.EM_MessageType = "MTB";
			ediMessage2.EM_Status = "STA";
			ediMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage2.EM_SystemCreateTimeUtc = ZDateTime.Now;
			Assert("ediMessage2 Newer", collection.IsMessageNewer_Exposed(ediMessage2, ediMessage1));
			ediMessage1.EM_MessageNum = "2";
			ediMessage2.EM_MessageNum = "1";
			Assert("ediMessage2 Older", !collection.IsMessageNewer_Exposed(ediMessage2, ediMessage1));
			ediMessage1.EM_MessageNum = "";
			ediMessage2.EM_MessageNum = "";
			var interchange1 = Factory.New<EDIInterchange>();
			interchange1.EI_InterchangeNum = "1";
			var interchange2 = Factory.New<EDIInterchange>();
			interchange2.EI_InterchangeNum = "2";
			ediMessage1.EM_EI = interchange2.PK;
			ediMessage2.EM_EI = interchange1.PK;
			Assert("ediMessage2 Older", !collection.IsMessageNewer_Exposed(ediMessage2, ediMessage1));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new EDIMessageCollection(entry);
		}

		public class EDIMessageCollectionForTesting : EDIMessageCollection
		{
			public EDIMessageCollectionForTesting(BusinessObject master)
				: base(master)
			{
			}

			public bool IsMessageNewer_Exposed(Enterprise.Messaging.Business.EDIMessage messageX, Enterprise.Messaging.Business.EDIMessage messageY)
			{
				return IsMessageNewer(messageX, messageY);
			}
		}

		JobDeclaration declaration
		{
			get
			{
				if (fdeclaration == null)
				{
					fdeclaration = Factory.New<JobDeclaration>();
				}
				return fdeclaration;
			}
		}
		JobDeclaration fdeclaration;

		CusEntryHeader entry
		{
			get
			{
				if (fentry == null)
				{
					fentry = declaration.CustomsEntryHeaders.AddNew();
				}
				return fentry;
			}
		}
		CusEntryHeader fentry;
	}
}
