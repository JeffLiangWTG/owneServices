using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class DeclarationExceptionCodeCalculatorTest : TestCaseWithFactory
	{
		[TestDate(2016, 9, 1)]
		public void TestProcess()
		{
			try
			{
				CACustomsDataRegistry.Instance.TimeFrameForExceptionReporting.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 10);
				CACustomsDataRegistry.Instance.B3AcceptedButNotReportedOnDN.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 11);
				CACustomsDataRegistry.Instance.B3NoResponseThreshold.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 12);
				CACustomsDataRegistry.Instance.ReleaseNoResponseThreshold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 13);
				CACustomsDataRegistry.Instance.PostArrivalNotReleased.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 14);
				CACustomsDataRegistry.Instance.PARSNotreleasedOceanThreshold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 15);
				CACustomsDataRegistry.Instance.PARSNotreleasedAirThreshold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 16);
				CACustomsDataRegistry.Instance.PARSNotReleasedHighwayRailAndOtherThreshold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 17);
				SetupJobDeclarations();

				var entryHeader = declaration1.CustomsEntryHeaders.AddNew();
				entryHeader.CH_MessageType = "B3C";
				entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;
				declaration1.CA_K84StatementDate = ZDateTime.Empty;
				declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
				var message = entryHeader.Messages.AddNew();
				message.EM_ReceiveTransmit = "RCV";
				message.EM_MessageType = "B3C";
				message.EM_MessageSubType = "CLR";
				message.EM_SystemCreateTimeUtc = new ZDateTime(2016, 9, 1).AddDays(-12);
				Factory.Save();

				declaration1.CA_DeclarationException = CAExceptionCodeList.Codes.CustomsAmountDueDiscrepancy;
				var calculator = new DeclarationExceptionCodeCalculator(glbCompany.PK.ToGuid());
				calculator.SetDeclarationException(declaration1, Factory);
				AssertEquals(CAExceptionCodeList.Codes.CustomsAmountDueDiscrepancy, declaration1.CA_DeclarationException);

				declaration1.CA_DeclarationException = ZString.Empty;
				calculator.SetDeclarationException(declaration1, Factory);
				AssertEquals(CAExceptionCodeList.Codes.EntryLodgedAndAcceptedNotReportedONDN, declaration1.CA_DeclarationException);

				declaration1.CA_K84StatementDate = new ZDateTime(2016, 1, 1);
				entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Error;
				entryHeader.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
				message.EM_HeldUntilDate = new ZDateTime(2016, 9, 1).AddDays(-13);
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				Factory.Save();
				calculator.SetDeclarationException(declaration1, Factory);
				AssertEquals(CAExceptionCodeList.Codes.EntrySentButNoResponse, declaration1.CA_DeclarationException);

				ZDateTime timeInOttawa = EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("CAOTT", ZDateTime.UtcNow.ToDateTime());
				declaration1.B3EntryHeader.CH_Status = MessageStatusList.Codes.AwaitingReplace;
				declaration1.JE_EntryAuthorisationDate = new ZDateTime(2016, 1, 1);
				declaration1.CA_EstimatedPaymentDueDate = timeInOttawa;
				Factory.Save();
				calculator.SetDeclarationException(declaration1, Factory);
				AssertEquals(CAExceptionCodeList.Codes.ReleasedNotEntryConfirmed, declaration1.CA_DeclarationException);

				declaration1.CA_CSAEntry = true;
				Factory.Save();
				calculator.SetDeclarationException(declaration1, Factory);
				AssertNotEquals(CAExceptionCodeList.Codes.ReleasedNotEntryConfirmed, declaration1.CA_DeclarationException);
				declaration1.CA_CSAEntry = false;

				declaration1.CA_EstimatedPaymentDueDate = timeInOttawa.AddDays(5);
				var entryHeader2 = declaration1.CustomsEntryHeaders.AddNew();
				entryHeader2.CH_MessageType = "REL";
				entryHeader2.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
				var message2 = entryHeader2.Messages.AddNew();
				message2.EM_ReceiveTransmit = "TRX";
				message2.EM_MessageType = "REL";
				message2.EM_SystemCreateTimeUtc = new ZDateTime(2016, 9, 1).AddDays(-14);
				Factory.Save();
				calculator.SetDeclarationException(declaration1, Factory);
				AssertEquals(CAExceptionCodeList.Codes.ReleaseAndIIDSentNoResponse, declaration1.CA_DeclarationException);

				var timeZoneSet = Factory.NewWithValidTestData<RefTimeZoneSet>();
				timeZoneSet.HasDaylightSavings = true;
				var unlocoZ = Factory.New<RefUNLOCO>();
				unlocoZ.RL_Code = "!ZZ";
				unlocoZ.RL_R3 = timeZoneSet.PK;
				entryHeader2.CH_Status = MessageStatusList.Codes.ErrorReplace;
				declaration1.CA_ServiceOption = ServiceOptions.Codes.PARS;
				declaration1.JE_EntryAuthorisationDate = ZDateTime.Empty;
				declaration1.JE_TransportMode = TransportTypeList.Codes.Sea;
				declaration1.JE_RL_NKPortOfArrival = "!ZZ";
				declaration1.JE_DateOfFirstArrival = timeInOttawa.AddHours(-16);
				Factory.Save();
				calculator.SetDeclarationException(declaration1, Factory);
				AssertEquals(CAExceptionCodeList.Codes.NoTimelyRelease, declaration1.CA_DeclarationException);

				declaration1.JE_DateOfFirstArrival = timeInOttawa;
				Factory.Save();
				calculator.SetDeclarationException(declaration1, Factory);
				AssertEquals(ZString.Empty, declaration1.CA_DeclarationException);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		void SetupJobDeclarations()
		{
			glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_Code = "CA1";
			glbCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "AIR";
			glbCompany.GC_OH_OrgProxy = org1.PK;
			var caBranch1 = glbCompany.Branches.AddNew();
			caBranch1.GB_Code = "AAA";
			var caBranch2 = glbCompany.Branches.AddNew();
			caBranch2.GB_Code = "BBB";

			declaration1 = Factory.New<JobDeclaration>();
			declaration1.FillWithValidTestData();
			declaration1.JE_GB = caBranch1.PK;
			declaration1.JE_EntrySubmittedDate = new ZDateTime(2016, 9, 1);

			declaration2 = Factory.New<JobDeclaration>();
			declaration2.FillWithValidTestData();
			declaration2.JE_GB = caBranch2.PK;
			declaration2.JE_EntrySubmittedDate = ZDateTime.Empty;
			Factory.Save();
		}
		GlbCompany glbCompany;
		JobDeclaration declaration1;
		JobDeclaration declaration2;
	}
}
