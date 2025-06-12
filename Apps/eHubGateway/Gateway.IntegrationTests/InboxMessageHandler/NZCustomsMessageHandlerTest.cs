using System;
using System.IO;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Integration;
using NUnit.Framework;

namespace CargoWise.eHub.Gateway.IntegrationTests.InboxMessageHandler
{
	[TestFixture]
	public class NZCustomsMessageHandlerTest : GatewayIntegrationTestBase
	{
		[Test]
		public void TestFail_IsFailNZCustomsMessageDeliveryConfigured()
		{
			var oldSettingValue = GetRemoteSettings(FailNZCustomsMessageDeliveryKey);

			try
			{
				SetRemoteSettings(FailNZCustomsMessageDeliveryKey, "true");

				var adapter = CreateAdapter(TestClientID, TestClientPassword);

				using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(SampleMessage)))
				{
					var message = new eHubMessage(Guid.NewGuid(), TestClientID, NZCustomsID, MessageSchemaType.Xml, ApplicationCode.NZCustoms, "http://cargowise.com/ehub/products/#NZCustoms", stream);
					adapter.Outbox.AddMessage(message);
					adapter.SendMessages();

					var expectedInboxMessageContent = stream.CompressAndEncode().ReadToEnd();
					AssertInboxMessage(message.TrackingID, TestClientPK, ApplicationCode.NZCustoms, NZCustomsPK, "http://cargowise.com/ehub/products/#NZCustoms", "", "", expectedInboxMessageContent, 0, 255);
					AssertEHubError("GTW", "UKN", "Cannot be delivered as this system does not have a NZCustoms connection.", GetErrorDetails(TestClientID, NZCustomsID));
				}
			}
			finally
			{
				SetRemoteSettings(FailNZCustomsMessageDeliveryKey, oldSettingValue.ToString());
			}
		}

		[Test]
		public void TestFail_SenderHasNoLicence()
		{
			var adapter = CreateAdapter(TestClientNoLicenceID, TestClientPassword);

			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(SampleMessage)))
			{
				var message = new eHubMessage(Guid.NewGuid(), TestClientNoLicenceID, NZCustomsID, MessageSchemaType.Xml, ApplicationCode.NZCustoms, "http://cargowise.com/ehub/products/#NZCustoms", stream);
				adapter.Outbox.AddMessage(message);
				adapter.SendMessages();

				var expectedInboxMessageContent = stream.CompressAndEncode().ReadToEnd();
				AssertInboxMessage(message.TrackingID, TestClientNoLicencePK, ApplicationCode.NZCustoms, NZCustomsPK, "http://cargowise.com/ehub/products/#NZCustoms", "", "", expectedInboxMessageContent, 0, 255);
				AssertEHubError("GTW", "UKN", "Only production licensed CW1/Enterprise systems are permitted to send production messages to NZ Customs.", GetErrorDetails(TestClientNoLicenceID, NZCustomsID));
			}
		}

		[Test]
		public void TestFail_SenderHasNoProductionLicence()
		{
			var adapter = CreateAdapter(TestClientNoProductionLicenceID, TestClientPassword);

			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(SampleMessage)))
			{
				var message = new eHubMessage(Guid.NewGuid(), TestClientNoProductionLicenceID, NZCustomsID, MessageSchemaType.Xml, ApplicationCode.NZCustoms, "http://cargowise.com/ehub/products/#NZCustoms", stream);
				adapter.Outbox.AddMessage(message);
				adapter.SendMessages();

				var expectedInboxMessageContent = stream.CompressAndEncode().ReadToEnd();
				AssertInboxMessage(message.TrackingID, TestClientNoProductionLicencePK, ApplicationCode.NZCustoms, NZCustomsPK, "http://cargowise.com/ehub/products/#NZCustoms", "", "", expectedInboxMessageContent, 0, 255);
				AssertEHubError("GTW", "UKN", "Only production licensed CW1/Enterprise systems are permitted to send production messages to NZ Customs.", GetErrorDetails(TestClientNoProductionLicenceID, NZCustomsID));
			}
		}

		[Test]
		public void TestFail_eHub2GatewayServiceIsNotAvailable()
		{
			var adapter = CreateAdapter(TestClientWithProductionLicenceID, TestAuthenticatedClientPassword);

			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(SampleMessage)))
			{
				var message = new eHubMessage(Guid.NewGuid(), TestClientWithProductionLicenceID, NZCustomsID, MessageSchemaType.Xml, ApplicationCode.NZCustoms, "http://cargowise.com/ehub/products/#NZCustoms", stream);
				adapter.Outbox.AddMessage(message);
				try
				{
					adapter.SendMessages();
					Assert.Fail("RegistrationException is expected since eHubAdapter wraps FaultExceptions with a message containing \"not a valid ediEnterprise licence code.\" into RegistrationException.");
				}
				catch (RegistrationException e)
				{
					StringAssert.Contains("eHub Gateway under maintenance. Messages will be resubmitted to eHub automatically on next service task run. Please ignore the following message: Server under maintenance and have not a valid ediEnterprise licence code. ExceptionID: ", e.Message);
					Assert.IsInstanceOf(typeof(FaultException<ExceptionDetail>), e.InnerException);
					var innerException = e.InnerException.ToString();
					Assert.IsTrue(Regex.IsMatch(innerException, @"System\.ServiceModel\.EndpointNotFoundException: Could not connect to net\.tcp://(.*?):11809/eHub2Gateway/eHub2Gateway\.svc"));
				}
			}
		}

	    [Test]
		public void TestDuplicatedMessageReference_TransactionNotRollback()
	    {
	        var trackingID = Guid.NewGuid();

	        var adapter = CreateAdapter(TestClientID, TestClientPassword);
	        var sampleMessage = @"<NZCustoms xmlns=""http://cargowise.com/ehub/products/""><Reference>00FHX766H</Reference><Content>TWltZS1WZXJzaW9uOiAxLjANCkNvbnRlbnQtVHJhbnNmZXItRW5jb2Rpbmc6IGJhc2U2NA0KQ29udGVudC1UeXBlOiBhcHBsaWNhdGlvbi9wa2NzNy1taW1lOyBzbWltZS10eXBlPXNpZ25lZC1kYXRhOyBuYW1lID0gInNtaW1lLnA3bSINCg0KTUlJSXlnWUpLb1pJaHZjTkFRY0NvSUlJdXpDQ0NMY0NBUUV4RGpBTUJnZ3Foa2lHOXcwQ0JRVUFNSUlCaVFZSktvWklodmNOQVFjQg0Kb0lJQmVnU0NBWFpOYVcxbExWWmxjbk5wYjI0NklERXVNQTBLUTI5dWRHVnVkQzFVY21GdWMyWmxjaTFGYm1OdlpHbHVaem9nWW1Geg0KWlRZMERRcERiMjUwWlc1MExWUjVjR1U2SUdGd2NHeHBZMkYwYVc5dUwyVmthV1poWTNRN0lHNWhiV1U5UmtwS05EYzBVbDg0T0RZdQ0KWldScERRcERiMjUwWlc1MExVUnBjM0J2YzJsMGFXOXVPaUJoZEhSaFkyaHRaVzUwT3lCbWFXeGxibUZ0WlQxR1NrbzBOelJTWHpnNA0KTmk1bFpHa05DZzBLVmxVMVFrOXBjM1ZRZVVGdVZsVTFRMHN4Vms5VU1FMDJUWGwwUjFOcmJ6Qk9lbEpUVDJwd1IxTnJiekJPZWxKVA0KU3pCR1FsRlVUWHBPYTAxeVRWUm5kMDVVU1hoUGFrVTBUbFJKY2cwS1QwUm5Na294Vms5VFEzTjRTekJPVUZSc1VsTlVSSEJGVDJwTg0KTmxaVk5HNVdWVTVLUzNwQmQwMUVRWGROUkVGM1RVUkJlRTFxUVRGTE1FWkNVVlJOZWs1clRUWlBhMFpDVVZSTmVnMEtUbXROY2xKcg0KY0V0T1JHTXdWV2x6TkVveFZrOVdRM042UzNwRmJsWlZOV0ZMZWtWeVQwUm5Na3AzUFQwTkNxQ0NCVUF3Z2dVOE1JSUVwYUFEQWdFQw0KQWhBRHZQWVp0QTZWUjVNVDQ1aWJ0blQ0TUEwR0NTcUdTSWIzRFFFQkJRVUFNSUdQTVJzd0dRWURWUVFLRXhKV1pYSnBVMmxuYmlCQg0KZFhOMGNtRnNhV0V4RnpBVkJnTlZCQXNURGtkaGRHVnJaV1Z3WlhJZ1VFdEpNVGd3TmdZRFZRUUxFeTlVWlhKdGN5QnZaaUIxYzJVZw0KWVhRZ2FIUjBjSE02THk5M2QzY3VaWE5wWjI0dVkyOXRMbUYxTDBkTFVsQkJMekVkTUJzR0ExVUVBeE1VUjJGMFpXdGxaWEJsY2lCVQ0KV1ZCRklETWdRMEV3SGhjTk1UWXhNakUwTURBd01EQXdXaGNOTVRneE1qRTBNak0xT1RVNVdqQmhNUXN3Q1FZRFZRUUdFd0pCVlRFUg0KTUE4R0ExVUVDQk1JVm1samRHOXlhV0V4S3pBcEJnTlZCQW9VSWt4RlRWQlNTVVZTUlNCSFRFOUNRVXdnVEU5SFNWTlVTVU5USUZCVQ0KV1NCTVZFUXhFakFRQmdOVkJBTVRDVU5oY21kdmQybHpaVENDQVNJd0RRWUpLb1pJaHZjTkFRRUJCUUFEZ2dFUEFEQ0NBUW9DZ2dFQg0KQUwzb2tybzZsN1VPdmZwYmROOFRJME5zd0FVL2JMeDRFNjQwb0g2WTdSTG9kZFMwdmxjUFU3RnZhT01YQWJIUy9CbWFCbm5jdm1FNg0KVXBSTm5UOWFlS1ZnclFzblNnSTFDNEI5c2FMSjJEK2FxNTlyVTYwMHRBbCtEZ1o1Q0JSY0hzeDB5Ukh0M1g3RS9Rekc4OFBVT3h5Tg0KdVhzNWltdTd1a0NaNFVIS0lYVUR1QmlZN1RPa29weHhkaUFCYk45VU0wZGRjM0pkbDQ2ektIYWxQTGg3OGxWVW04MHNMWnE3b2lvKw0KNTExL0tpY0RkQXdsNjQ1dWhzRjVlU2Z4aEk4c3VKMnp2YmlvRzVtUk9QSWtGT0ZyNmhQT2pkaExDMU5teHBBV2tmTXZaWXhhOWZCNQ0KL0llU3VPTS84VURQaEdkajEzd3l1KzlzUTlqT1dPMG5ZWnlScDdFQ0F3RUFBYU9DQWtBd2dnSThNQXdHQTFVZEV3RUIvd1FDTUFBdw0KZ2dFR0JnTlZIUjhFZ2Y0d2dmc3dnZmlnZ2ZXZ2dmS0dQV2gwZEhBNkx5OXZibk5wZEdWamNtd3VaWE5wWjI0dVkyOXRMbUYxTDBkaA0KZEdWclpXVndaWEpVZVhCbE0wTkJMMHhoZEdWemRFTlNUQzVqY215R2diQnNaR0Z3T2k4dlpHbHlaV04wYjNKNUxtVnphV2R1TG1Odg0KYlM1aGRTOWpiajFIWVhSbGEyVmxjR1Z5SUZSWlVFVWdNeUJEUVN4dmRUMVVaWEp0Y3lCdlppQjFjMlVnWVhRZ2FIUjBjSE02THk5Mw0KZDNjdVpYTnBaMjR1WTI5dExtRjFMMGRMVWxCQkx5eHZkVDFIWVhSbGEyVmxjR1Z5SUZCTFNTeHZQVlpsY21sVGFXZHVJRUYxYzNSeQ0KWVd4cFlUOWpaWEowYVdacFkyRjBaWEpsZG05allYUnBiMjVzYVhOME8ySnBibUZ5ZVRBZkJnTlZIU01FR0RBV2dCUzEyeG5HanZGaw0KNTl5QXAra2lqaDRtQkNXME5EQWRCZ05WSFE0RUZnUVVWNjM2YkkvRUUvQS9sVVBJY0ZsZE1yWGJFWGt3TlFZSUt3WUJCUVVIQVFFRQ0KS1RBbk1DVUdDQ3NHQVFVRkJ6QUJoaGxvZEhSd2N6b3ZMMjlqYzNBdVpYTnBaMjR1WTI5dExtRjFNQTRHQTFVZER3RUIvd1FFQXdJRQ0KOERBbkJnTlZIUkVFSURBZWdSeGpZWEpuYjNkcGMyVkFiR2RzYjJkcGMzUnBZM011WTI5dExtRjFNQmNHQmlva0FZSk5BUVFORmdzMw0KTnpFMk9ETTBOakF6TURCR0JnTlZIU0FFUHpBOU1Ec0dDaW9rcWZ5MFk0Sk5BZ2d3TFRBckJnZ3JCZ0VGQlFjQ0FSWWZhSFIwY0hNNg0KTHk5M2QzY3VaWE5wWjI0dVkyOXRMbUYxTDBkTFVsQkJMekFSQmdsZ2hrZ0JodmhDQVFFRUJBTUNCNEF3RFFZSktvWklodmNOQVFFRg0KQlFBRGdZRUFFNjlBTTZTbkFhYXp3TzkyREhHS2U1T2VERnJqd1gzOW44dWMrU1doVGFDKzNMK0pRQTJRcEZJVFQvODEwUllDbG1tVA0KZGQ5YURNRzBMZ21Cd3RDd2lUVmw0bFdiNVg3NjF3OWtOb2FkdGZGMTN2TmpqMXVSZEFRb2lQbThXUzQybDdZUjN0RmU5R0hWcTErSw0KcHROOGFNOXVUT0tnQnY5YU4xSWFFbkdHQ1lreGdnSFBNSUlCeXdJQkFUQ0JwRENCanpFYk1Ca0dBMVVFQ2hNU1ZtVnlhVk5wWjI0Zw0KUVhWemRISmhiR2xoTVJjd0ZRWURWUVFMRXc1SFlYUmxhMlZsY0dWeUlGQkxTVEU0TURZR0ExVUVDeE12VkdWeWJYTWdiMllnZFhObA0KSUdGMElHaDBkSEJ6T2k4dmQzZDNMbVZ6YVdkdUxtTnZiUzVoZFM5SFMxSlFRUzh4SFRBYkJnTlZCQU1URkVkaGRHVnJaV1Z3WlhJZw0KVkZsUVJTQXpJRU5CQWhBRHZQWVp0QTZWUjVNVDQ1aWJ0blQ0TUF3R0NDcUdTSWIzRFFJRkJRQXdEUVlKS29aSWh2Y05BUUVCQlFBRQ0KZ2dFQWpOWElnbjZKMFpPeWRXWVM0NzlmMmF5ODNQL2FFZ1ZGZHF5dFpCVVJiU1NZaHZOMVR3eTRqdnBOU0JTcFBYWlhRWWdMVTBhTg0KbS8rMHZIY2huSmVzQUs4OFlGUy91YzI2eVk2WXZkYnBjZGhMMTNzN09CaXMzKzcyRVM1dGF1WWFxQXQ3ZHlTQlFTRHNQTWxWek42SQ0KYWVkaEI2SEJMVGJsTmg0aGttSldDSFV1K3FkZzhmVmVTV2c4V2dLREo1THI1ZEhFdU9ObDBJV1JHTEV5Mm43ckozSi83dHhLU1A1Uw0KRzVvYkhuMEJuTjRmMVdGODRHSER4R2tKUm50YXU1ZE0vWjh2emgyZlJzaEVMMEZDbDlCQXVaWmliUklCSm1Mcm1vdHdnelNWMkR4Mw0KTGJzM3Q4Ty9PakJGZjk1c1BZcHRUdWdSc08ra2lETVRMZ3l3NVlGaFRBPT0NCg==</Content></NZCustoms>";

	        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleMessage)))
	        {
	            var message = new eHubMessage(trackingID, TestClientID, NZCustomsID, MessageSchemaType.Xml,
	                ApplicationCode.NZCustoms, "http://cargowise.com/ehub/products/", stream);
	            adapter.Outbox.AddMessage(message);
                adapter.SendMessages();
                

	            var expectedInboxMessageContent = stream.CompressAndEncode().ReadToEnd();
	            AssertInboxMessage(trackingID, TestClientPK, ApplicationCode.NZCustoms, NZCustomsPK,
	                "http://cargowise.com/ehub/products/", "", "", expectedInboxMessageContent, 0, 255);
                AssertEHubError("GTW", "UKN", "Only production licensed CW1/Enterprise systems are permitted to send production messages to NZ Customs.", GetErrorDetails(TestClientID, NZCustomsID));

	            adapter.RetrieveMessages();
	            AssertContainsFailedStatusMessage(adapter.Inbox, trackingID, TestClientID);
	        }
	    }

	    [Test]
		[Explicit("Requires BizTalk with configured eHub2Gateway service")]
		public void TestClientIDNotExistMessage__TransactionRollBack()
	    {
	        var trackingID = Guid.NewGuid();
	        var non_ExistsClientId = "2471E75B-AF62-4BA8-AD10-1424BA7E646E";

	        var adapter = CreateAdapter(TestClientID, TestClientPassword);
	        var sampleMessage = @"<NZCustoms xmlns=""http://cargowise.com/ehub/products/""><Reference>00FHX766H</Reference><Content>TWltZS1WZXJzaW9uOiAxLjANCkNvbnRlbnQtVHJhbnNmZXItRW5jb2Rpbmc6IGJhc2U2NA0KQ29udGVudC1UeXBlOiBhcHBsaWNhdGlvbi9wa2NzNy1taW1lOyBzbWltZS10eXBlPXNpZ25lZC1kYXRhOyBuYW1lID0gInNtaW1lLnA3bSINCg0KTUlJSXlnWUpLb1pJaHZjTkFRY0NvSUlJdXpDQ0NMY0NBUUV4RGpBTUJnZ3Foa2lHOXcwQ0JRVUFNSUlCaVFZSktvWklodmNOQVFjQg0Kb0lJQmVnU0NBWFpOYVcxbExWWmxjbk5wYjI0NklERXVNQTBLUTI5dWRHVnVkQzFVY21GdWMyWmxjaTFGYm1OdlpHbHVaem9nWW1Geg0KWlRZMERRcERiMjUwWlc1MExWUjVjR1U2SUdGd2NHeHBZMkYwYVc5dUwyVmthV1poWTNRN0lHNWhiV1U5UmtwS05EYzBVbDg0T0RZdQ0KWldScERRcERiMjUwWlc1MExVUnBjM0J2YzJsMGFXOXVPaUJoZEhSaFkyaHRaVzUwT3lCbWFXeGxibUZ0WlQxR1NrbzBOelJTWHpnNA0KTmk1bFpHa05DZzBLVmxVMVFrOXBjM1ZRZVVGdVZsVTFRMHN4Vms5VU1FMDJUWGwwUjFOcmJ6Qk9lbEpUVDJwd1IxTnJiekJPZWxKVA0KU3pCR1FsRlVUWHBPYTAxeVRWUm5kMDVVU1hoUGFrVTBUbFJKY2cwS1QwUm5Na294Vms5VFEzTjRTekJPVUZSc1VsTlVSSEJGVDJwTg0KTmxaVk5HNVdWVTVLUzNwQmQwMUVRWGROUkVGM1RVUkJlRTFxUVRGTE1FWkNVVlJOZWs1clRUWlBhMFpDVVZSTmVnMEtUbXROY2xKcg0KY0V0T1JHTXdWV2x6TkVveFZrOVdRM042UzNwRmJsWlZOV0ZMZWtWeVQwUm5Na3AzUFQwTkNxQ0NCVUF3Z2dVOE1JSUVwYUFEQWdFQw0KQWhBRHZQWVp0QTZWUjVNVDQ1aWJ0blQ0TUEwR0NTcUdTSWIzRFFFQkJRVUFNSUdQTVJzd0dRWURWUVFLRXhKV1pYSnBVMmxuYmlCQg0KZFhOMGNtRnNhV0V4RnpBVkJnTlZCQXNURGtkaGRHVnJaV1Z3WlhJZ1VFdEpNVGd3TmdZRFZRUUxFeTlVWlhKdGN5QnZaaUIxYzJVZw0KWVhRZ2FIUjBjSE02THk5M2QzY3VaWE5wWjI0dVkyOXRMbUYxTDBkTFVsQkJMekVkTUJzR0ExVUVBeE1VUjJGMFpXdGxaWEJsY2lCVQ0KV1ZCRklETWdRMEV3SGhjTk1UWXhNakUwTURBd01EQXdXaGNOTVRneE1qRTBNak0xT1RVNVdqQmhNUXN3Q1FZRFZRUUdFd0pCVlRFUg0KTUE4R0ExVUVDQk1JVm1samRHOXlhV0V4S3pBcEJnTlZCQW9VSWt4RlRWQlNTVVZTUlNCSFRFOUNRVXdnVEU5SFNWTlVTVU5USUZCVQ0KV1NCTVZFUXhFakFRQmdOVkJBTVRDVU5oY21kdmQybHpaVENDQVNJd0RRWUpLb1pJaHZjTkFRRUJCUUFEZ2dFUEFEQ0NBUW9DZ2dFQg0KQUwzb2tybzZsN1VPdmZwYmROOFRJME5zd0FVL2JMeDRFNjQwb0g2WTdSTG9kZFMwdmxjUFU3RnZhT01YQWJIUy9CbWFCbm5jdm1FNg0KVXBSTm5UOWFlS1ZnclFzblNnSTFDNEI5c2FMSjJEK2FxNTlyVTYwMHRBbCtEZ1o1Q0JSY0hzeDB5Ukh0M1g3RS9Rekc4OFBVT3h5Tg0KdVhzNWltdTd1a0NaNFVIS0lYVUR1QmlZN1RPa29weHhkaUFCYk45VU0wZGRjM0pkbDQ2ektIYWxQTGg3OGxWVW04MHNMWnE3b2lvKw0KNTExL0tpY0RkQXdsNjQ1dWhzRjVlU2Z4aEk4c3VKMnp2YmlvRzVtUk9QSWtGT0ZyNmhQT2pkaExDMU5teHBBV2tmTXZaWXhhOWZCNQ0KL0llU3VPTS84VURQaEdkajEzd3l1KzlzUTlqT1dPMG5ZWnlScDdFQ0F3RUFBYU9DQWtBd2dnSThNQXdHQTFVZEV3RUIvd1FDTUFBdw0KZ2dFR0JnTlZIUjhFZ2Y0d2dmc3dnZmlnZ2ZXZ2dmS0dQV2gwZEhBNkx5OXZibk5wZEdWamNtd3VaWE5wWjI0dVkyOXRMbUYxTDBkaA0KZEdWclpXVndaWEpVZVhCbE0wTkJMMHhoZEdWemRFTlNUQzVqY215R2diQnNaR0Z3T2k4dlpHbHlaV04wYjNKNUxtVnphV2R1TG1Odg0KYlM1aGRTOWpiajFIWVhSbGEyVmxjR1Z5SUZSWlVFVWdNeUJEUVN4dmRUMVVaWEp0Y3lCdlppQjFjMlVnWVhRZ2FIUjBjSE02THk5Mw0KZDNjdVpYTnBaMjR1WTI5dExtRjFMMGRMVWxCQkx5eHZkVDFIWVhSbGEyVmxjR1Z5SUZCTFNTeHZQVlpsY21sVGFXZHVJRUYxYzNSeQ0KWVd4cFlUOWpaWEowYVdacFkyRjBaWEpsZG05allYUnBiMjVzYVhOME8ySnBibUZ5ZVRBZkJnTlZIU01FR0RBV2dCUzEyeG5HanZGaw0KNTl5QXAra2lqaDRtQkNXME5EQWRCZ05WSFE0RUZnUVVWNjM2YkkvRUUvQS9sVVBJY0ZsZE1yWGJFWGt3TlFZSUt3WUJCUVVIQVFFRQ0KS1RBbk1DVUdDQ3NHQVFVRkJ6QUJoaGxvZEhSd2N6b3ZMMjlqYzNBdVpYTnBaMjR1WTI5dExtRjFNQTRHQTFVZER3RUIvd1FFQXdJRQ0KOERBbkJnTlZIUkVFSURBZWdSeGpZWEpuYjNkcGMyVkFiR2RzYjJkcGMzUnBZM011WTI5dExtRjFNQmNHQmlva0FZSk5BUVFORmdzMw0KTnpFMk9ETTBOakF6TURCR0JnTlZIU0FFUHpBOU1Ec0dDaW9rcWZ5MFk0Sk5BZ2d3TFRBckJnZ3JCZ0VGQlFjQ0FSWWZhSFIwY0hNNg0KTHk5M2QzY3VaWE5wWjI0dVkyOXRMbUYxTDBkTFVsQkJMekFSQmdsZ2hrZ0JodmhDQVFFRUJBTUNCNEF3RFFZSktvWklodmNOQVFFRg0KQlFBRGdZRUFFNjlBTTZTbkFhYXp3TzkyREhHS2U1T2VERnJqd1gzOW44dWMrU1doVGFDKzNMK0pRQTJRcEZJVFQvODEwUllDbG1tVA0KZGQ5YURNRzBMZ21Cd3RDd2lUVmw0bFdiNVg3NjF3OWtOb2FkdGZGMTN2TmpqMXVSZEFRb2lQbThXUzQybDdZUjN0RmU5R0hWcTErSw0KcHROOGFNOXVUT0tnQnY5YU4xSWFFbkdHQ1lreGdnSFBNSUlCeXdJQkFUQ0JwRENCanpFYk1Ca0dBMVVFQ2hNU1ZtVnlhVk5wWjI0Zw0KUVhWemRISmhiR2xoTVJjd0ZRWURWUVFMRXc1SFlYUmxhMlZsY0dWeUlGQkxTVEU0TURZR0ExVUVDeE12VkdWeWJYTWdiMllnZFhObA0KSUdGMElHaDBkSEJ6T2k4dmQzZDNMbVZ6YVdkdUxtTnZiUzVoZFM5SFMxSlFRUzh4SFRBYkJnTlZCQU1URkVkaGRHVnJaV1Z3WlhJZw0KVkZsUVJTQXpJRU5CQWhBRHZQWVp0QTZWUjVNVDQ1aWJ0blQ0TUF3R0NDcUdTSWIzRFFJRkJRQXdEUVlKS29aSWh2Y05BUUVCQlFBRQ0KZ2dFQWpOWElnbjZKMFpPeWRXWVM0NzlmMmF5ODNQL2FFZ1ZGZHF5dFpCVVJiU1NZaHZOMVR3eTRqdnBOU0JTcFBYWlhRWWdMVTBhTg0KbS8rMHZIY2huSmVzQUs4OFlGUy91YzI2eVk2WXZkYnBjZGhMMTNzN09CaXMzKzcyRVM1dGF1WWFxQXQ3ZHlTQlFTRHNQTWxWek42SQ0KYWVkaEI2SEJMVGJsTmg0aGttSldDSFV1K3FkZzhmVmVTV2c4V2dLREo1THI1ZEhFdU9ObDBJV1JHTEV5Mm43ckozSi83dHhLU1A1Uw0KRzVvYkhuMEJuTjRmMVdGODRHSER4R2tKUm50YXU1ZE0vWjh2emgyZlJzaEVMMEZDbDlCQXVaWmliUklCSm1Mcm1vdHdnelNWMkR4Mw0KTGJzM3Q4Ty9PakJGZjk1c1BZcHRUdWdSc08ra2lETVRMZ3l3NVlGaFRBPT0NCg==</Content></NZCustoms>";

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleMessage)))
	        {
	            var message = new eHubMessage(trackingID, TestClientID, non_ExistsClientId, MessageSchemaType.Xml,
	                ApplicationCode.NZCustoms, "http://cargowise.com/ehub/products/", stream);
	            adapter.Outbox.AddMessage(message);

	            try
	            {
	                adapter.SendMessages();
	                Assert.Fail("RegistrationException is expected since eHubAdapter wraps FaultExceptions with a message containing \"not a valid ediEnterprise licence code.\" into RegistrationException.");
	            }
	            catch (Exception ex)
	            {
	                var expectedExceptionStringPos =
	                    ex.Message.IndexOf("Recipient ID 2471E75B-AF62-4BA8-AD10-1424BA7E646E could not be found.");
	                Assert.Greater(expectedExceptionStringPos, -1);
                    AssertEmptyInboxMessage(trackingID);
                    AssertEHubError("GTW", "UKN", "Only production licensed CW1/Enterprise systems are permitted to send production messages to NZ Customs.", null);
	            }
	        }
	    }

	    [Test]
		[Explicit("Requires BizTalk with configured eHub2Gateway service")]
		public void TestDuplicatedMessageReference_TransactionRollbacksWhenInsertErrorFailedWithAdapterException() 
	    {
	        var trackingID = Guid.NewGuid();
            var adapter = CreateAdapter(TestClientID, TestClientPassword);
	        try
	        {
	            using (var connection = OpenEHubTransactionsConnection())
	            using (var command = connection.CreateCommand())
	            {
	                command.CommandText = "ALTER TABLE eHubError ALTER COLUMN EE_Description VARCHAR(5)";
	                command.ExecuteReader();
	            }
	            var sampleMessage = @"<NZCustoms xmlns=""http://cargowise.com/ehub/products/""><Reference>00FHX766H</Reference><Content>TWltZS1WZXJzaW9uOiAxLjANCkNvbnRlbnQtVHJhbnNmZXItRW5jb2Rpbmc6IGJhc2U2NA0KQ29udGVudC1UeXBlOiBhcHBsaWNhdGlvbi9wa2NzNy1taW1lOyBzbWltZS10eXBlPXNpZ25lZC1kYXRhOyBuYW1lID0gInNtaW1lLnA3bSINCg0KTUlJSXlnWUpLb1pJaHZjTkFRY0NvSUlJdXpDQ0NMY0NBUUV4RGpBTUJnZ3Foa2lHOXcwQ0JRVUFNSUlCaVFZSktvWklodmNOQVFjQg0Kb0lJQmVnU0NBWFpOYVcxbExWWmxjbk5wYjI0NklERXVNQTBLUTI5dWRHVnVkQzFVY21GdWMyWmxjaTFGYm1OdlpHbHVaem9nWW1Geg0KWlRZMERRcERiMjUwWlc1MExWUjVjR1U2SUdGd2NHeHBZMkYwYVc5dUwyVmthV1poWTNRN0lHNWhiV1U5UmtwS05EYzBVbDg0T0RZdQ0KWldScERRcERiMjUwWlc1MExVUnBjM0J2YzJsMGFXOXVPaUJoZEhSaFkyaHRaVzUwT3lCbWFXeGxibUZ0WlQxR1NrbzBOelJTWHpnNA0KTmk1bFpHa05DZzBLVmxVMVFrOXBjM1ZRZVVGdVZsVTFRMHN4Vms5VU1FMDJUWGwwUjFOcmJ6Qk9lbEpUVDJwd1IxTnJiekJPZWxKVA0KU3pCR1FsRlVUWHBPYTAxeVRWUm5kMDVVU1hoUGFrVTBUbFJKY2cwS1QwUm5Na294Vms5VFEzTjRTekJPVUZSc1VsTlVSSEJGVDJwTg0KTmxaVk5HNVdWVTVLUzNwQmQwMUVRWGROUkVGM1RVUkJlRTFxUVRGTE1FWkNVVlJOZWs1clRUWlBhMFpDVVZSTmVnMEtUbXROY2xKcg0KY0V0T1JHTXdWV2x6TkVveFZrOVdRM042UzNwRmJsWlZOV0ZMZWtWeVQwUm5Na3AzUFQwTkNxQ0NCVUF3Z2dVOE1JSUVwYUFEQWdFQw0KQWhBRHZQWVp0QTZWUjVNVDQ1aWJ0blQ0TUEwR0NTcUdTSWIzRFFFQkJRVUFNSUdQTVJzd0dRWURWUVFLRXhKV1pYSnBVMmxuYmlCQg0KZFhOMGNtRnNhV0V4RnpBVkJnTlZCQXNURGtkaGRHVnJaV1Z3WlhJZ1VFdEpNVGd3TmdZRFZRUUxFeTlVWlhKdGN5QnZaaUIxYzJVZw0KWVhRZ2FIUjBjSE02THk5M2QzY3VaWE5wWjI0dVkyOXRMbUYxTDBkTFVsQkJMekVkTUJzR0ExVUVBeE1VUjJGMFpXdGxaWEJsY2lCVQ0KV1ZCRklETWdRMEV3SGhjTk1UWXhNakUwTURBd01EQXdXaGNOTVRneE1qRTBNak0xT1RVNVdqQmhNUXN3Q1FZRFZRUUdFd0pCVlRFUg0KTUE4R0ExVUVDQk1JVm1samRHOXlhV0V4S3pBcEJnTlZCQW9VSWt4RlRWQlNTVVZTUlNCSFRFOUNRVXdnVEU5SFNWTlVTVU5USUZCVQ0KV1NCTVZFUXhFakFRQmdOVkJBTVRDVU5oY21kdmQybHpaVENDQVNJd0RRWUpLb1pJaHZjTkFRRUJCUUFEZ2dFUEFEQ0NBUW9DZ2dFQg0KQUwzb2tybzZsN1VPdmZwYmROOFRJME5zd0FVL2JMeDRFNjQwb0g2WTdSTG9kZFMwdmxjUFU3RnZhT01YQWJIUy9CbWFCbm5jdm1FNg0KVXBSTm5UOWFlS1ZnclFzblNnSTFDNEI5c2FMSjJEK2FxNTlyVTYwMHRBbCtEZ1o1Q0JSY0hzeDB5Ukh0M1g3RS9Rekc4OFBVT3h5Tg0KdVhzNWltdTd1a0NaNFVIS0lYVUR1QmlZN1RPa29weHhkaUFCYk45VU0wZGRjM0pkbDQ2ektIYWxQTGg3OGxWVW04MHNMWnE3b2lvKw0KNTExL0tpY0RkQXdsNjQ1dWhzRjVlU2Z4aEk4c3VKMnp2YmlvRzVtUk9QSWtGT0ZyNmhQT2pkaExDMU5teHBBV2tmTXZaWXhhOWZCNQ0KL0llU3VPTS84VURQaEdkajEzd3l1KzlzUTlqT1dPMG5ZWnlScDdFQ0F3RUFBYU9DQWtBd2dnSThNQXdHQTFVZEV3RUIvd1FDTUFBdw0KZ2dFR0JnTlZIUjhFZ2Y0d2dmc3dnZmlnZ2ZXZ2dmS0dQV2gwZEhBNkx5OXZibk5wZEdWamNtd3VaWE5wWjI0dVkyOXRMbUYxTDBkaA0KZEdWclpXVndaWEpVZVhCbE0wTkJMMHhoZEdWemRFTlNUQzVqY215R2diQnNaR0Z3T2k4dlpHbHlaV04wYjNKNUxtVnphV2R1TG1Odg0KYlM1aGRTOWpiajFIWVhSbGEyVmxjR1Z5SUZSWlVFVWdNeUJEUVN4dmRUMVVaWEp0Y3lCdlppQjFjMlVnWVhRZ2FIUjBjSE02THk5Mw0KZDNjdVpYTnBaMjR1WTI5dExtRjFMMGRMVWxCQkx5eHZkVDFIWVhSbGEyVmxjR1Z5SUZCTFNTeHZQVlpsY21sVGFXZHVJRUYxYzNSeQ0KWVd4cFlUOWpaWEowYVdacFkyRjBaWEpsZG05allYUnBiMjVzYVhOME8ySnBibUZ5ZVRBZkJnTlZIU01FR0RBV2dCUzEyeG5HanZGaw0KNTl5QXAra2lqaDRtQkNXME5EQWRCZ05WSFE0RUZnUVVWNjM2YkkvRUUvQS9sVVBJY0ZsZE1yWGJFWGt3TlFZSUt3WUJCUVVIQVFFRQ0KS1RBbk1DVUdDQ3NHQVFVRkJ6QUJoaGxvZEhSd2N6b3ZMMjlqYzNBdVpYTnBaMjR1WTI5dExtRjFNQTRHQTFVZER3RUIvd1FFQXdJRQ0KOERBbkJnTlZIUkVFSURBZWdSeGpZWEpuYjNkcGMyVkFiR2RzYjJkcGMzUnBZM011WTI5dExtRjFNQmNHQmlva0FZSk5BUVFORmdzMw0KTnpFMk9ETTBOakF6TURCR0JnTlZIU0FFUHpBOU1Ec0dDaW9rcWZ5MFk0Sk5BZ2d3TFRBckJnZ3JCZ0VGQlFjQ0FSWWZhSFIwY0hNNg0KTHk5M2QzY3VaWE5wWjI0dVkyOXRMbUYxTDBkTFVsQkJMekFSQmdsZ2hrZ0JodmhDQVFFRUJBTUNCNEF3RFFZSktvWklodmNOQVFFRg0KQlFBRGdZRUFFNjlBTTZTbkFhYXp3TzkyREhHS2U1T2VERnJqd1gzOW44dWMrU1doVGFDKzNMK0pRQTJRcEZJVFQvODEwUllDbG1tVA0KZGQ5YURNRzBMZ21Cd3RDd2lUVmw0bFdiNVg3NjF3OWtOb2FkdGZGMTN2TmpqMXVSZEFRb2lQbThXUzQybDdZUjN0RmU5R0hWcTErSw0KcHROOGFNOXVUT0tnQnY5YU4xSWFFbkdHQ1lreGdnSFBNSUlCeXdJQkFUQ0JwRENCanpFYk1Ca0dBMVVFQ2hNU1ZtVnlhVk5wWjI0Zw0KUVhWemRISmhiR2xoTVJjd0ZRWURWUVFMRXc1SFlYUmxhMlZsY0dWeUlGQkxTVEU0TURZR0ExVUVDeE12VkdWeWJYTWdiMllnZFhObA0KSUdGMElHaDBkSEJ6T2k4dmQzZDNMbVZ6YVdkdUxtTnZiUzVoZFM5SFMxSlFRUzh4SFRBYkJnTlZCQU1URkVkaGRHVnJaV1Z3WlhJZw0KVkZsUVJTQXpJRU5CQWhBRHZQWVp0QTZWUjVNVDQ1aWJ0blQ0TUF3R0NDcUdTSWIzRFFJRkJRQXdEUVlKS29aSWh2Y05BUUVCQlFBRQ0KZ2dFQWpOWElnbjZKMFpPeWRXWVM0NzlmMmF5ODNQL2FFZ1ZGZHF5dFpCVVJiU1NZaHZOMVR3eTRqdnBOU0JTcFBYWlhRWWdMVTBhTg0KbS8rMHZIY2huSmVzQUs4OFlGUy91YzI2eVk2WXZkYnBjZGhMMTNzN09CaXMzKzcyRVM1dGF1WWFxQXQ3ZHlTQlFTRHNQTWxWek42SQ0KYWVkaEI2SEJMVGJsTmg0aGttSldDSFV1K3FkZzhmVmVTV2c4V2dLREo1THI1ZEhFdU9ObDBJV1JHTEV5Mm43ckozSi83dHhLU1A1Uw0KRzVvYkhuMEJuTjRmMVdGODRHSER4R2tKUm50YXU1ZE0vWjh2emgyZlJzaEVMMEZDbDlCQXVaWmliUklCSm1Mcm1vdHdnelNWMkR4Mw0KTGJzM3Q4Ty9PakJGZjk1c1BZcHRUdWdSc08ra2lETVRMZ3l3NVlGaFRBPT0NCg==</Content></NZCustoms>";

                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleMessage)))
	            {
	                var message = new eHubMessage(trackingID, TestClientID, NZCustomsID, MessageSchemaType.Xml, ApplicationCode.NZCustoms, "http://cargowise.com/ehub/products/", stream);
	                adapter.Outbox.AddMessage(message);
	                try
	                {
	                    adapter.SendMessages();
	                    Assert.Fail("RegistrationException is expected since eHubAdapter wraps FaultExceptions with a message containing \"not a valid ediEnterprise licence code.\" into RegistrationException.");
	                }
                    catch (eHubAdapterException e)
	                {
	                    Assert.IsTrue(e.Message.Contains("There was a failure executing the receive pipeline"));
                        Assert.IsTrue(e.Message.Contains("Transaction count after EXECUTE indicates a mismatching number of BEGIN and COMMIT statements"));

	                    var messageExceptionDictionary = e.GetMessageExceptionDictionary();
	                    Assert.IsNotEmpty(messageExceptionDictionary, "MessageExceptionDictionary should not be empty.");
	                    Assert.AreEqual(1, messageExceptionDictionary.Count);

	                    var messageException1 = messageExceptionDictionary[trackingID];
	                    Assert.IsTrue(messageException1.Contains("There was a failure executing the receive pipeline"));
                        Assert.IsTrue(messageException1.Contains("Transaction count after EXECUTE indicates a mismatching number of BEGIN and COMMIT statements"));
	                }
	            }

                AssertEmptyInboxMessage(trackingID);
	        }
	        finally
	        {
	            using (var connection = OpenEHubTransactionsConnection())
	            using (var command = connection.CreateCommand())
	            {
                    command.CommandText = "ALTER TABLE eHubError ALTER COLUMN EE_Description NVARCHAR(MAX)";
	                command.ExecuteScalar();
	            }
	        }
	    }

	    [Test]
		[Explicit("Requires BizTalk with configured eHub2Gateway service")]
		public void TestDuplicatedMessageReference_TransactionRollbacksWhenInsertErrorFailedWithRegistrationException()
	    {
	        var trackingID = Guid.NewGuid();
	        var adapter = CreateAdapter(TestClientID, TestClientPassword);
	        try
	        {
	            using (var connection = OpenEHubTransactionsConnection())
	            using (var command = connection.CreateCommand())
	            {
                    command.CommandText = "EXEC sp_rename 'InsertError', 'ChangeProcedureForTesting'";
                    command.ExecuteScalar();
	            }
	            var sampleMessage = @"<NZCustoms xmlns=""http://cargowise.com/ehub/products/""><Reference>00FHX766H</Reference><Content>TWltZS1WZXJzaW9uOiAxLjANCkNvbnRlbnQtVHJhbnNmZXItRW5jb2Rpbmc6IGJhc2U2NA0KQ29udGVudC1UeXBlOiBhcHBsaWNhdGlvbi9wa2NzNy1taW1lOyBzbWltZS10eXBlPXNpZ25lZC1kYXRhOyBuYW1lID0gInNtaW1lLnA3bSINCg0KTUlJSXlnWUpLb1pJaHZjTkFRY0NvSUlJdXpDQ0NMY0NBUUV4RGpBTUJnZ3Foa2lHOXcwQ0JRVUFNSUlCaVFZSktvWklodmNOQVFjQg0Kb0lJQmVnU0NBWFpOYVcxbExWWmxjbk5wYjI0NklERXVNQTBLUTI5dWRHVnVkQzFVY21GdWMyWmxjaTFGYm1OdlpHbHVaem9nWW1Geg0KWlRZMERRcERiMjUwWlc1MExWUjVjR1U2SUdGd2NHeHBZMkYwYVc5dUwyVmthV1poWTNRN0lHNWhiV1U5UmtwS05EYzBVbDg0T0RZdQ0KWldScERRcERiMjUwWlc1MExVUnBjM0J2YzJsMGFXOXVPaUJoZEhSaFkyaHRaVzUwT3lCbWFXeGxibUZ0WlQxR1NrbzBOelJTWHpnNA0KTmk1bFpHa05DZzBLVmxVMVFrOXBjM1ZRZVVGdVZsVTFRMHN4Vms5VU1FMDJUWGwwUjFOcmJ6Qk9lbEpUVDJwd1IxTnJiekJPZWxKVA0KU3pCR1FsRlVUWHBPYTAxeVRWUm5kMDVVU1hoUGFrVTBUbFJKY2cwS1QwUm5Na294Vms5VFEzTjRTekJPVUZSc1VsTlVSSEJGVDJwTg0KTmxaVk5HNVdWVTVLUzNwQmQwMUVRWGROUkVGM1RVUkJlRTFxUVRGTE1FWkNVVlJOZWs1clRUWlBhMFpDVVZSTmVnMEtUbXROY2xKcg0KY0V0T1JHTXdWV2x6TkVveFZrOVdRM042UzNwRmJsWlZOV0ZMZWtWeVQwUm5Na3AzUFQwTkNxQ0NCVUF3Z2dVOE1JSUVwYUFEQWdFQw0KQWhBRHZQWVp0QTZWUjVNVDQ1aWJ0blQ0TUEwR0NTcUdTSWIzRFFFQkJRVUFNSUdQTVJzd0dRWURWUVFLRXhKV1pYSnBVMmxuYmlCQg0KZFhOMGNtRnNhV0V4RnpBVkJnTlZCQXNURGtkaGRHVnJaV1Z3WlhJZ1VFdEpNVGd3TmdZRFZRUUxFeTlVWlhKdGN5QnZaaUIxYzJVZw0KWVhRZ2FIUjBjSE02THk5M2QzY3VaWE5wWjI0dVkyOXRMbUYxTDBkTFVsQkJMekVkTUJzR0ExVUVBeE1VUjJGMFpXdGxaWEJsY2lCVQ0KV1ZCRklETWdRMEV3SGhjTk1UWXhNakUwTURBd01EQXdXaGNOTVRneE1qRTBNak0xT1RVNVdqQmhNUXN3Q1FZRFZRUUdFd0pCVlRFUg0KTUE4R0ExVUVDQk1JVm1samRHOXlhV0V4S3pBcEJnTlZCQW9VSWt4RlRWQlNTVVZTUlNCSFRFOUNRVXdnVEU5SFNWTlVTVU5USUZCVQ0KV1NCTVZFUXhFakFRQmdOVkJBTVRDVU5oY21kdmQybHpaVENDQVNJd0RRWUpLb1pJaHZjTkFRRUJCUUFEZ2dFUEFEQ0NBUW9DZ2dFQg0KQUwzb2tybzZsN1VPdmZwYmROOFRJME5zd0FVL2JMeDRFNjQwb0g2WTdSTG9kZFMwdmxjUFU3RnZhT01YQWJIUy9CbWFCbm5jdm1FNg0KVXBSTm5UOWFlS1ZnclFzblNnSTFDNEI5c2FMSjJEK2FxNTlyVTYwMHRBbCtEZ1o1Q0JSY0hzeDB5Ukh0M1g3RS9Rekc4OFBVT3h5Tg0KdVhzNWltdTd1a0NaNFVIS0lYVUR1QmlZN1RPa29weHhkaUFCYk45VU0wZGRjM0pkbDQ2ektIYWxQTGg3OGxWVW04MHNMWnE3b2lvKw0KNTExL0tpY0RkQXdsNjQ1dWhzRjVlU2Z4aEk4c3VKMnp2YmlvRzVtUk9QSWtGT0ZyNmhQT2pkaExDMU5teHBBV2tmTXZaWXhhOWZCNQ0KL0llU3VPTS84VURQaEdkajEzd3l1KzlzUTlqT1dPMG5ZWnlScDdFQ0F3RUFBYU9DQWtBd2dnSThNQXdHQTFVZEV3RUIvd1FDTUFBdw0KZ2dFR0JnTlZIUjhFZ2Y0d2dmc3dnZmlnZ2ZXZ2dmS0dQV2gwZEhBNkx5OXZibk5wZEdWamNtd3VaWE5wWjI0dVkyOXRMbUYxTDBkaA0KZEdWclpXVndaWEpVZVhCbE0wTkJMMHhoZEdWemRFTlNUQzVqY215R2diQnNaR0Z3T2k4dlpHbHlaV04wYjNKNUxtVnphV2R1TG1Odg0KYlM1aGRTOWpiajFIWVhSbGEyVmxjR1Z5SUZSWlVFVWdNeUJEUVN4dmRUMVVaWEp0Y3lCdlppQjFjMlVnWVhRZ2FIUjBjSE02THk5Mw0KZDNjdVpYTnBaMjR1WTI5dExtRjFMMGRMVWxCQkx5eHZkVDFIWVhSbGEyVmxjR1Z5SUZCTFNTeHZQVlpsY21sVGFXZHVJRUYxYzNSeQ0KWVd4cFlUOWpaWEowYVdacFkyRjBaWEpsZG05allYUnBiMjVzYVhOME8ySnBibUZ5ZVRBZkJnTlZIU01FR0RBV2dCUzEyeG5HanZGaw0KNTl5QXAra2lqaDRtQkNXME5EQWRCZ05WSFE0RUZnUVVWNjM2YkkvRUUvQS9sVVBJY0ZsZE1yWGJFWGt3TlFZSUt3WUJCUVVIQVFFRQ0KS1RBbk1DVUdDQ3NHQVFVRkJ6QUJoaGxvZEhSd2N6b3ZMMjlqYzNBdVpYTnBaMjR1WTI5dExtRjFNQTRHQTFVZER3RUIvd1FFQXdJRQ0KOERBbkJnTlZIUkVFSURBZWdSeGpZWEpuYjNkcGMyVkFiR2RzYjJkcGMzUnBZM011WTI5dExtRjFNQmNHQmlva0FZSk5BUVFORmdzMw0KTnpFMk9ETTBOakF6TURCR0JnTlZIU0FFUHpBOU1Ec0dDaW9rcWZ5MFk0Sk5BZ2d3TFRBckJnZ3JCZ0VGQlFjQ0FSWWZhSFIwY0hNNg0KTHk5M2QzY3VaWE5wWjI0dVkyOXRMbUYxTDBkTFVsQkJMekFSQmdsZ2hrZ0JodmhDQVFFRUJBTUNCNEF3RFFZSktvWklodmNOQVFFRg0KQlFBRGdZRUFFNjlBTTZTbkFhYXp3TzkyREhHS2U1T2VERnJqd1gzOW44dWMrU1doVGFDKzNMK0pRQTJRcEZJVFQvODEwUllDbG1tVA0KZGQ5YURNRzBMZ21Cd3RDd2lUVmw0bFdiNVg3NjF3OWtOb2FkdGZGMTN2TmpqMXVSZEFRb2lQbThXUzQybDdZUjN0RmU5R0hWcTErSw0KcHROOGFNOXVUT0tnQnY5YU4xSWFFbkdHQ1lreGdnSFBNSUlCeXdJQkFUQ0JwRENCanpFYk1Ca0dBMVVFQ2hNU1ZtVnlhVk5wWjI0Zw0KUVhWemRISmhiR2xoTVJjd0ZRWURWUVFMRXc1SFlYUmxhMlZsY0dWeUlGQkxTVEU0TURZR0ExVUVDeE12VkdWeWJYTWdiMllnZFhObA0KSUdGMElHaDBkSEJ6T2k4dmQzZDNMbVZ6YVdkdUxtTnZiUzVoZFM5SFMxSlFRUzh4SFRBYkJnTlZCQU1URkVkaGRHVnJaV1Z3WlhJZw0KVkZsUVJTQXpJRU5CQWhBRHZQWVp0QTZWUjVNVDQ1aWJ0blQ0TUF3R0NDcUdTSWIzRFFJRkJRQXdEUVlKS29aSWh2Y05BUUVCQlFBRQ0KZ2dFQWpOWElnbjZKMFpPeWRXWVM0NzlmMmF5ODNQL2FFZ1ZGZHF5dFpCVVJiU1NZaHZOMVR3eTRqdnBOU0JTcFBYWlhRWWdMVTBhTg0KbS8rMHZIY2huSmVzQUs4OFlGUy91YzI2eVk2WXZkYnBjZGhMMTNzN09CaXMzKzcyRVM1dGF1WWFxQXQ3ZHlTQlFTRHNQTWxWek42SQ0KYWVkaEI2SEJMVGJsTmg0aGttSldDSFV1K3FkZzhmVmVTV2c4V2dLREo1THI1ZEhFdU9ObDBJV1JHTEV5Mm43ckozSi83dHhLU1A1Uw0KRzVvYkhuMEJuTjRmMVdGODRHSER4R2tKUm50YXU1ZE0vWjh2emgyZlJzaEVMMEZDbDlCQXVaWmliUklCSm1Mcm1vdHdnelNWMkR4Mw0KTGJzM3Q4Ty9PakJGZjk1c1BZcHRUdWdSc08ra2lETVRMZ3l3NVlGaFRBPT0NCg==</Content></NZCustoms>";

	            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleMessage)))
	            {
	                var message = new eHubMessage(trackingID, TestClientID, NZCustomsID, MessageSchemaType.Xml, ApplicationCode.NZCustoms, "http://cargowise.com/ehub/products/", stream);
	                adapter.Outbox.AddMessage(message);
	                try
	                {
	                    adapter.SendMessages();
	                    Assert.Fail("RegistrationException is expected since eHubAdapter wraps FaultExceptions with a message containing \"not a valid ediEnterprise licence code.\" into RegistrationException.");
	                }
                    catch (RegistrationException e)
	                {
	                    Assert.IsTrue(e.Message.StartsWith("eHub Gateway under maintenance. Messages will be resubmitted to eHub automatically on next service task run. Please ignore the following message: Server under maintenance and have not a valid ediEnterprise licence code."));
	                }
	            }

	            AssertEmptyInboxMessage(trackingID);
	        }
	        finally
	        {
	            using (var connection = OpenEHubTransactionsConnection())
	            using (var command = connection.CreateCommand())
	            {
                    command.CommandText = "EXEC sp_rename 'ChangeProcedureForTesting', 'InsertError'";
	                command.ExecuteScalar();
	            }
	        }
	    }

		public static string GetErrorDetails(string sender, string recipient)
		{
			return $"<ErrorDetail><SourceParty>{sender}</SourceParty><DestinationParty>{recipient}</DestinationParty></ErrorDetail>";
		}

		const string NZCustomsID = "NZCustoms";
		readonly Guid NZCustomsPK = Guid.Parse("B3013901-04ED-43AE-A287-B6669813CF6E");
		const string TestClientNoLicenceID = "TestClientNoLicence";
		readonly Guid TestClientNoLicencePK = Guid.Parse("AD73D667-F825-4C34-8A96-EA64866EDA63");
		const string TestClientNoProductionLicenceID = "ENTTSTSVZ";
		readonly Guid TestClientNoProductionLicencePK = Guid.Parse("F1AD7276-54FB-4A78-8FFC-88C1E66CA57C");
		const string TestClientWithProductionLicenceID = "ENTTSTSVR";
		const string FailNZCustomsMessageDeliveryKey = "FailNZCustomsMessageDelivery";
		const string SampleMessage = @"<NZCustoms xmlns=""http://cargowise.com/ehub/products/""><Reference>51358596K</Reference><Type>Text</Type><Authentication /><Content>VU5BOisuPyAnVU5CK1VOT0E6Mis1MTM1ODU5Nks6WlpaK0NVU1NXVDpaWlorMTcwMTA2OjA5MzkrNDUnVU5IKzQ1K0NVU0RFQzpEOjk2QjpVTidCR00rOTI5K0IwMDAwMjgxMCs5J0NTVCsrMTA6MTA1OjE0MydMT0MrOStVU0xHQidMT0MrMTErTlpBS0wnTE9DKzQxK05aQUtMJ0RUTSsxNTE6MjAxNzAxMTI6MTAyJ0dJUytBVEY6MTEwOjE0MzoxMjM0NTYnR0lTK01DRDoxMTA6MTQzOllOTk5OJ01FQStXVCtBQUQrS0dNOjEyNTg2J0VRRCtDTitQT05VMzI1NjI1MCsrKys1J1JGRitCTTpMR0JBS0wyODEwJ1BBQyswKytQSydSRkYrQk06MDAyODEwJ1JGRitBQVE6UE9OVTMyNTYyNTAnUEFDKzgrKzA3J1REVCsyMCs1MDkrMSsrKysrOjo6Q0FQIENPUlJJRU5URVMnTkFEK0FMKzUxMzUyMzY4SjpaWlo6MTQzJ05BRCtDQis1MTM1ODU5Nks6WlpaOjE0MydVTlMrRCdETVMrMUJOWjAwOC0xKzkzNSdUT0QrKytGT0I6MTA2OjE0MydDU1QrMSs4NDMxNDEwMDAwSzoxNjk6MTQzJ0ZUWCtBQUErKytCVUNLRVRTIFNIT1ZFTFMgR1JBQlMgQU5EIEdSSVBTJ0xPQysyNytVUydMT0MrMzUrVVMnTkFEK1NVKzAwNzEwODQxWTpaWlo6MTQzJ01PQSsxNDoxMzAwMC4wMDpOWkQnQ1VYKzIrKzEuMDAnTU9BKzQwOjEzMDAwJ01PQSs2NDoxMjE0J01PQSs3MDozJ0dJUytOOjEwOToxNDMnVEFYKzErQ1VEJ01PQSsxNjE6NjUwLjAwJ1RBWCsxK0dTVCdNT0ErMTYxOjIyMzAuMDUnVU5TK1MnQ05UKzQ6MSdDTlQrNToxJ0NOVCsxMTo4J1RBWCszK0NVRCsrMTMwMDAnTU9BKzE2MTo2NTAuMDAnVEFYKzMrR1NUJ01PQSsxNjE6MjIzMC4wNSdUQVgrNCtUT1QnTU9BKzE2MToyODgwLjA1J0dJUytCOjEzNDoxNDMnQVVUK0xLTERAQkdJSkRLSEdGQEErNDAwMDYyMDZFJ1VOVCs1MCs0NSdVTlorMSs0NSc=</Content></NZCustoms>";
	}
}
