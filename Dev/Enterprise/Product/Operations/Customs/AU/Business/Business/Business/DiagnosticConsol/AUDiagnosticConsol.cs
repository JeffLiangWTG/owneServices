using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.StabilityChecker;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUDiagnosticConsol : DiagnosticConsol
	{
		public AUDiagnosticConsol(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public const string StmNoteParentID = "5F30DAAD-2A32-46E2-9B6C-C062584E9C6F";

		protected override bool IsTestMode
		{
			get { return Env.Registry.CMRTestMode; }
		}

		public override ZString InitialMessage
		{
			get
			{
				return "This tool will send a test message to Australian Customs.\r\nEach stage of sending and processing the reply will be tracked and shown below.\r\nPress 'Start' to commence the test, or press 'Cancel' if you do not wish to run the test.\r\n";
			}
		}

		protected override ZGuid CreateTestMessage(BusinessObjectFactory testMessageFactory, ZString consolKey)
		{
			var testMessage = testMessageFactory.New<CMRSTREQMessage>();
			testMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			testMessage.EM_Status = EDIMessage.Status.Queued;
			testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage.EM_MessageText = @"UNH+<<MSGNO PLACEHOLDER>>+GENRAL:D:99B:UN'BGM+23:::STREQ+" + consolKey.Replace("-", "") + ":1+13'RFF+TN:AAACJJTKS'UNT+4+<<MSGNO PLACEHOLDER>>'";
			testMessage.EM_MessageType = CMRMessage.CMRMessageTypes.STREQ;
			testMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			testMessage.EM_GE = GlbDepartment.CurrentDepartment.PK;
			testMessage.EM_IsActive = true;
			return testMessage.PK;
		}

		public override List<RegistryAndCertificateCheckResult> RegistryAndCertificateChecks()
		{
			var result = new List<RegistryAndCertificateCheckResult>();

			if (GlbCompany.CurrentCompany.GC_CustomsRegistrationNo.IsEmpty)
			{
				result.Add(new RegistryAndCertificateCheckResult("There is no Customs Registration Number set on the current company", StabilityResultLevel.Critical));
			}
			else if (GlbCompany.CurrentCompany.GC_CustomsRegistrationNo.Length != 7)
			{
				result.Add(new RegistryAndCertificateCheckResult("The Customs Registration Number set on the current company is invalid; it should be 7 characters long", StabilityResultLevel.Critical));
			}

			var certificateChecker = new CertificateChecker(Factory);
			string message = string.Empty;
			StabilityResultLevel level;

			level = certificateChecker.CheckCompanyKey(out message, false);
			if (level != StabilityResultLevel.Healthy)
			{
				result.Add(new RegistryAndCertificateCheckResult(message, level));
			}

			level = certificateChecker.CheckCustomsKey(out message, false);
			if (level != StabilityResultLevel.Healthy)
			{
				result.Add(new RegistryAndCertificateCheckResult(message, level));
			}

			return result;
		}

		public override ZString ProcessTimerTick(out ZString diagnosticConsolAction)
		{
			diagnosticConsolAction = ZString.Empty;
			var result = ZString.Empty;

			if (cycleController == null)
			{
				var messagePK = diagnosticNote.MessagePK;
				var messageCreateTime = Factory.Load<EDIMessage>(messagePK)?.EM_SystemCreateTimeUtc ?? ZDateTime.Now.AddDays(-3);

				cycleController = new DiagnosticCycleController(messagePK, messageCreateTime, ConsolKey);
				result = diagnosticStatusList.GetDescriptionFromCode(InitialStatus);
			}
			else
			{
				ZString previousStatus = diagnosticNote.Status;

				try
				{
					diagnosticNote.Status = cycleController.CheckNextDiagnosticCycleStep(previousStatus);
				}
				catch (DiagnosticMessageException diagEx)
				{
					diagnosticNote.Status = diagEx.FailStatus;
					diagnosticConsolAction = DiagnosticConsolActions.Codes.Failed;
				}
				catch (InvalidOperationException ex)
				{
					diagnosticConsolAction = DiagnosticConsolActions.Codes.ErrorOccurredInProcessing;
					result = ex.Message;
				}

				if (diagnosticNote.Status != previousStatus)
				{
					if (diagnosticNote.Status == DiagnosticStatusList.Codes.OKSuccess)
					{
						diagnosticConsolAction = DiagnosticConsolActions.Codes.Success;
					}

					if (result.IsEmpty)
					{
						result = diagnosticStatusList.GetDescriptionFromCode(diagnosticNote.Status);
					}
				}
			}

			return result;
		}

		DiagnosticCycleController cycleController;
		readonly DiagnosticStatusList diagnosticStatusList = new DiagnosticStatusList();
	}
}
