using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Messaging.Business.Testing
{
	public class InterchangeResenderTest : TestCaseWithFactory
	{
		public class TestRespondedInterchange : EDIInterchange
		{
			public TestRespondedInterchange(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool ForceAcknowledge;

			public override ZString EI_Status => ForceAcknowledge ? (ZString)"ACK" : base.EI_Status;
		}

		public void TestGetInstance()
		{
			AssertEquals("Type", typeof(InterchangeResender), InterchangeResender.GetInstance(Interchange).GetType());
			Interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
			AssertEquals("Type", typeof(CMRInterchangeResender), InterchangeResender.GetInstance(Interchange).GetType());
		}

		public void TestUnacknowledgeInterchangeCanBeResent()
		{
			var resender = InterchangeResender.GetInstance(Interchange);
			Assert("Interchange is Resent", resender.Resend());

			var dBFactory = new BusinessObjectFactory();
			var resentInterchange = dBFactory.Load<EDIInterchange>(Interchange.PK);
			AssertEquals("Interchange should be set to queued", EDIInterchange.Status.Queued, resentInterchange.EI_Status);
		}

		public void TestAcknowledgeInterchangeCanBeResentByForcing()
		{
			Interchange.ForceAcknowledge = true;
			var resender = InterchangeResender.GetInstance(Interchange);
			Assert("Interchange not Resent without being forced", !resender.Resend());
			Interchange.ForceAcknowledge = false;

			var dBFactory = new BusinessObjectFactory();

			var resentInterchange = dBFactory.Load<EDIInterchange>(Interchange.PK);
			AssertEquals("Interchange should not have been set to queued", EDIInterchange.Status.Sent, resentInterchange.EI_Status);
			Interchange.ForceAcknowledge = true;
			Assert("Can force Acknowledged interchange to be resent", resender.Resend(true));
			Interchange.ForceAcknowledge = false;

			dBFactory = new BusinessObjectFactory();
			resentInterchange = dBFactory.Load<EDIInterchange>(Interchange.PK);
			AssertEquals("Acknowledged Interchange should be forced to queued", EDIInterchange.Status.Queued, resentInterchange.EI_Status);
		}

		public void TestInterchangeQueueForCMRInterchange()
		{
			Interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
			Interchange.EI_Status = EDIInterchange.Status.Sent;
			Interchange.EI_RetryCount = 3;
			Factory.Save();

			var cmrResender = InterchangeResender.GetInstance(Interchange);
			AssertType<CMRInterchangeResender>("Is CMRInterchangeResender", cmrResender);
			Assert("Sent the Interchange", cmrResender.Resend());

			Interchange.Reload();
			AssertEquals("Interchange should be queued", EDIInterchange.Status.Queued, Interchange.EI_Status);
			AssertEquals("Retry Count Should Be Reduced", 2, Interchange.EI_RetryCount);
		}

		public void TestFailedCMRInterchangeCanBeResent()
		{
			Interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
			Interchange.EI_Status = EDIInterchange.Status.Failed;
			Interchange.EI_RetryCount = 3;

			var message = Interchange.ContainedMessages.AddNew();
			message.MessageNumberStrategy = new MockMessageNumberStrategy();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			message.EM_Status = EDIMessage.Status.Failed;

			Factory.Save();

			var cmrResender = InterchangeResender.GetInstance(Interchange);
			AssertType<CMRInterchangeResender>("Is CMRInterchangeResender", cmrResender);
			Assert("Sent the Interchange", cmrResender.Resend());
			Factory.Save();

			Interchange.Reload();
			AssertEquals("EI_Status should be SendPending", EDIInterchange.Status.SendPending, Interchange.EI_Status);
			AssertEquals("Retry Count Should Be Reduced", 2, Interchange.EI_RetryCount);

			message.Reload();
			AssertEquals("Message should be Sent", EDIMessage.Status.Sent, message.EM_Status);
		}

		#region Implementation

		class MockMessageNumberStrategy : IMessageNumberStrategy
		{
			public string GetMessageReferenceNumber() => Guid.NewGuid().ToString("N");
		}

		TestRespondedInterchange Interchange;

		protected override void SetUp()
		{
			base.SetUp();

			Interchange = Factory.New<TestRespondedInterchange>();
			Interchange.EI_From = "02029203";
			Interchange.EI_To = "30394039";
			Interchange.EI_HeaderText = "UNA:+.? 'UNB+UNOA:1+06050000010027+06050004400727+040324:1108+478++CUSRES'";
			Interchange.EI_BodyText = "UNH+100100+CUSRES:002:912:UN'BGM+961++9:200403241107:203+9+ZZ:802*8610033*4'LOC+12:AUSYD'RFF+ACW:66300'UNT+5+100100'UNH+100200+CUSRES:002:912:UN'BGM+963+66300+9:200403241107:203+9+ZZ:802*8610033*4'ERP+2:66300:0'ERC+W:SCA:95+060:SCA:95'UNT+5+100200'";
			Interchange.EI_FooterText = "UNZ+2+478'";
			Interchange.EI_Status = EDIInterchange.Status.Sent;
			Interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();
		}

		#endregion
	}
}
