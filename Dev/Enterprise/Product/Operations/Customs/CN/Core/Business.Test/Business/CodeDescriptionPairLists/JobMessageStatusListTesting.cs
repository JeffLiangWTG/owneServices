using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.Shared;

namespace Enterprise.Customs.CN.Business.Testing
{
	class JobMessageStatusListTesting : TestCaseWithFactory
	{
		public void TestIsAwaiting()
		{
			AssertNoExceptionThrown("Should be runnable even under invalid status.", () => JobMessageStatusList.IsAwaiting("XXX"));
			var fullList = new JobMessageStatusList();
			foreach (var code in fullList.GetAllCodes())
			{
				if (code == "AWO" || code == "AWM" || code == "AWP")
				{
					AssertEquals("IsAwaiting should return true for " + code, true, JobMessageStatusList.IsAwaiting(code));
				}
				else
				{
					AssertEquals("IsAwaiting should return false for " + code, false, JobMessageStatusList.IsAwaiting(code));
				}
			}
		}

		public void TestIsAcknowledged()
		{
			AssertNoExceptionThrown("Should be runnable even under invalid status.", () => JobMessageStatusList.IsAcknowledged("XXX"));
			var fullList = new JobMessageStatusList();
			foreach (var code in fullList.GetAllCodes())
			{
				if (code == "ACO" || code == "ACM" || code == "ACP")
				{
					AssertEquals("IsAcknowledged should return true for " + code, true, JobMessageStatusList.IsAcknowledged(code));
				}
				else
				{
					AssertEquals("IsAcknowledged should return false for " + code, false, JobMessageStatusList.IsAcknowledged(code));
				}
			}
		}

		public void TestGetMessageStatus()
		{
			AssertMessageStatuses("", "ACO", "ERO", "CLO");
			AssertMessageStatuses("SNT", "ACO", "ERO", "CLO");
			AssertMessageStatuses("UNK", "ACO", "ERO", "CLO");
			AssertMessageStatuses("AWO", "ACO", "ERO", "CLO");
			AssertMessageStatuses("AWP", "ACP", "ERP", "CLP");
			AssertMessageStatuses("AWP", "ACP", "ERP", "CLP");
			AssertMessageStatuses("AWP", "ACP", "ERP", "CLP");
			AssertMessageStatuses("AWM", "ACM", "ERM", "CLM");
			AssertMessageStatuses("ACM", "ACM", "ERM", "CLM");
			AssertMessageStatuses("ERM", "ACM", "ERM", "CLM");
		}

		public void TestAvailableForCompleteDeclaration()
		{
			var availableForCompleteDeclarationList = new List<string>
			{
				JobMessageStatusList.Codes.ClearedPreliminaryDeclaration,
				JobMessageStatusList.Codes.AwaitingResponseCompletedDeclaration,
				JobMessageStatusList.Codes.AcknowledgedCompletedDeclaration,
				JobMessageStatusList.Codes.ErrorCompletedDeclaration,
				JobMessageStatusList.Codes.ClearedCompletedDeclaration,
			};
			void AssertAvailableForCompleteDeclaration(string status)
			{
				var available = availableForCompleteDeclarationList.Contains(status);
				AssertEquals("AvailableForCompleteDeclaration should return " + available, available, JobMessageStatusList.AvailableForCompleteDeclaration(status));
			}

			foreach (var status in new JobMessageStatusList().GetAllCodes())
			{
				AssertAvailableForCompleteDeclaration(status);
			}
		}

		public void TestGetAwaitingStatusByDeclarationType()
		{
			var expectedList = new List<(string declarationType, string expectedAwaitingStatus)>
			{
				(DeclarationTypeList.Codes.IntegratedDeclaration, JobMessageStatusList.Codes.AwaitingResponseIntegratedDeclaration),
				(DeclarationTypeList.Codes.PreliminaryDeclaration, JobMessageStatusList.Codes.AwaitingResponsePreliminaryDeclaration),
				(DeclarationTypeList.Codes.ManualDeclaration, JobMessageStatusList.Codes.AwaitingResponsePreliminaryDeclaration),
				(DeclarationTypeList.Codes.AutoDeclaration, JobMessageStatusList.Codes.AwaitingResponsePreliminaryDeclaration),
				(DeclarationTypeList.Codes.CompleteDeclaration, JobMessageStatusList.Codes.AwaitingResponseCompletedDeclaration),
				("", MessageStatusList.Codes.AwaitingOriginal),
				("XXX", MessageStatusList.Codes.AwaitingOriginal),
			};
			foreach (var (declarationType, expectedAwaitingStatus) in expectedList)
			{
				AssertEquals("GetAwaitingStatusByDeclarationType should return " + expectedAwaitingStatus + " for Declaration Type " + declarationType, expectedAwaitingStatus, JobMessageStatusList.GetAwaitingStatusByDeclarationType(declarationType));
			}
		}

		static void AssertMessageStatuses(string input, string expectedAcknowledgedStatus, string expectedErrorStatus, string expectedClearedStatus)
		{
			AssertEquals("Should return AcknowledgedStatus " + expectedAcknowledgedStatus + " for " + input, expectedAcknowledgedStatus, JobMessageStatusList.GetAcknowledgedStatus(input));
			AssertEquals("Should return ErrorStatus " + expectedErrorStatus + " for " + input, expectedErrorStatus, JobMessageStatusList.GetErrorStatus(input));
			AssertEquals("Should return ClearedStatus " + expectedClearedStatus + " for " + input, expectedClearedStatus, JobMessageStatusList.GetClearedStatus(input));
		}
	}
}
