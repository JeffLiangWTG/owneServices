using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.Customs.GB.ICS.Messaging.IE315;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.GB.ICS.ServiceTasks.Testing
{
	[TestedType(typeof(ICSMessageSenderServiceTask))]
	public class ICSMessageSenderServiceTaskTest : ServiceTaskTestCase<ICSMessageSenderServiceTask>
	{
		public void TestProcessGIGMessage()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeaderSS>();
			manifestHeader.AMA_ManifestType = ICSManifestTypes.Codes.SAS;
			manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedKingdom;

			SetupBranchAndCredentialForTest(manifestHeader);

			var entryNumber = CusEntryNumber.LoadOrCreate(manifestHeader, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, Core.Constants.CountryCodes.UnitedKingdom);
			entryNumber.CE_EntryNum = "GMRO0000F2KW";

			var ediMessage = Factory.New<IcsSsGreatBritainEDIMessage>();
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			ediMessage.EM_ApplicationCode = Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.GbMessageICSGreatBritain;
			ediMessage.EM_MessageOwner = "ABC";
			ediMessage.EM_MessageText = @"ANything";
			ediMessage.EM_MessageSubType = Constants.ICSMessageSubTypes.NEW;
			ediMessage.EM_LinkedObject = manifestHeader;
			Factory.Save();

			InitialiseAndRunTaskSchedule(new ICSMessageSenderServiceTask());

			var message = new BusinessObjectFactory().Load<EDIMessage>(ediMessage.PK);

			AssertEquals(EDIMessageStatusList.Codes.Sent, message.EM_Status);
		}

		public void TestProcessGINMessage()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			SetupBranchAndCredentialForTest(header);

			var helper = new ICSEDIMessagingHelper(header);
			helper.SendMessageAndSave(GBMessageTypeList.Codes.New);

			InitialiseAndRunTaskSchedule(new ICSMessageSenderServiceTask());
			var newFactory = new BusinessObjectFactory();
			var createdMessage = newFactory.LoadTop1<IcsNorthernIrelandEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, header.PK));
			var createdInterchange = newFactory.LoadTop1<EDIInterchange>(new ZQuery(EDIInterchangeSchema.PK, createdMessage.EM_EI));

			CombineAssertions("EDIInterchange with correct values should have been created.", () =>
			{
				AssertNotNull("Not null", createdInterchange);
				AssertEquals("EI_InterchangeType", Constants.ICSMessageSubTypes.NEW, createdInterchange.EI_InterchangeType);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Sent, createdMessage.EM_Status);
			});
		}

		void SetupBranchAndCredentialForTest(AsycudaManifestHeaderBase header)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ImporterA";
			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB999999999888", Core.Constants.CountryCodes.UnitedKingdom);
			var aaaBranch = Factory.New<GlbBranch>();
			aaaBranch.GB_Code = "AAA";
			aaaBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			aaaBranch.GB_OH_OrgProxy = orgHeader.PK;
			Factory.Save();
			header.AMA_GB = aaaBranch.PK;

			TestDataHelper.CreateCredentials(Factory, GlbCompany.CurrentCompany.PK, "111", "GB999999999888", PasswordTypesList.Codes.CDS, ZDateTime.Today.AddDays(10));
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"UK Customs ICS Great Britain messages outbound",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GbMessageICSGreatBritain),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"UK Customs ICS Northern Ireland messages outbound",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GbMessageICSNorthernIreland),
				};
			}
		}
	}
}
