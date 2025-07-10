using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(ClientIncidentEstimate))]
	public class ClientIncidentEstimateTest : EnterpriseBusinessObjectTestCase
	{
		[TestDate(2014, 6, 30)]
		public void TestEstimateSentDatePopulatesOtherDates()
		{
			EDIDataRegistry.Instance.FeatureRequestEstimateAutoExpirePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 15);
			ZDateTime now = DateTime.Now;
			var estimate = Factory.New<ClientIncidentEstimate>();
			estimate.CIE_EstimateSentDateUTC = now;
			AssertEquals(now.AddDays(7), estimate.CIE_ExpressDeliveryOptionCutOffDateUTC);
			AssertEquals(now.AddDays(15), estimate.CIE_EstimateExpiryDateUTC);
		}

		public void TestUniqueIndexViolation()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();

			var estimate1 = Factory.New<ClientIncidentEstimate>();
			estimate1.CIE_IM = incident.PK;
			Factory.Save();

			var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var estimate2 = otherFactory.New<ClientIncidentEstimate>();
			estimate2.CIE_IM = incident.PK;

			try
			{
				otherFactory.Save();
				Fail("Should throw");
			}
			catch (ZSaveException ex)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZExceptionReporting.HandleSaveException(ex);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("While you have been working, another user has made changes. Please close and reopen the form again.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
