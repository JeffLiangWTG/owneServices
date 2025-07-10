using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Shared;
using Enterprise.ZArchitecture.Schema;
using Grpc.Core;
using Xware.Xt.Grpc.Application;
using static Enterprise.xTMessaging.Shared.Test.TestUtils;

namespace Enterprise.xTMessaging.Business.Test
{
	sealed class SaveToEDIInterchangeHandlerTest : TestCaseWithFactory
	{
		public void TestFetchHints()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();

			var msgIds = new List<MsgIdUri>();
			var metaDictionary = new Dictionary<MsgIdUri, Dictionary<string, string>>();

			var appCodes = new[] { "TST", "TS2", "TS3" };

			for (ulong i = 0; i < 100u; i++)
			{
				var msgIdUri = new MsgIdUri { Msgid = i + 1 };
				msgIds.Add(msgIdUri);
				var testMsgMetaData = new Dictionary<string, string>();
				var testMsgTrackingId = ZGuid.NewZGuid();
				testMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = appCodes[i % 3];
				testMsgMetaData[Constants.CustomMsgAttributes.SourceParty] = "TSTSENDER";
				testMsgMetaData[Constants.CustomMsgAttributes.DestinationParty] = "TSTRECIPIENT";
				testMsgMetaData[Constants.CustomMsgAttributes.MessageTrackingID] = testMsgTrackingId.ToString();
				testMsgMetaData[Constants.CustomMsgAttributes.MessageType] = "MST";
				metaDictionary.Add(msgIdUri, testMsgMetaData);

				var oppositeInterchange = Factory.New<EDIInterchange>();
				oppositeInterchange.EI_ApplicationCode = appCodes[i % 3];
				oppositeInterchange.EI_InterchangeType = "MST";
				oppositeInterchange.EI_SessionGUID = testMsgTrackingId;
				oppositeInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
				oppositeInterchange.EI_GB = branch.PK;
				oppositeInterchange.EI_From = "TSTSENDER";
				oppositeInterchange.EI_To = "TSTRECIPIENT";
				oppositeInterchange.EI_TransportType = EDIInterchange.TransportType.xT;
				oppositeInterchange.EI_Status = EDIInterchange.Status.Sent;
			}

			Factory.Save();

			var expectedDbHits = new Dictionary<string, int>
			{
				{ EDIInterchangeSchema.Constants.TableName, appCodes.Length },
				{ GlbBranchSchema.Constants.TableName, 1 },
			};

			var factory = new BusinessObjectFactory();
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, factory))
			{
				var handler = new SaveToEdIInterchangeHandlerForTest_SaveCount(factory, new Logger());
				handler.HandleReceivedMessageBatch(msgIds,
					msgid => metaDictionary[msgid],
					msgid => new AsyncServerStreamingCall<ResultByteChunk>(new ResultByteChunkReaderForTest(""), null, null, null, () => { }),
					msgData =>
					{
						var testMsgBody = new MemoryStream();
						var testMsgBytes = Encoding.UTF8.GetBytes("Test Message");
						testMsgBody.Write(testMsgBytes, 0, testMsgBytes.Length);
						return testMsgBody;
					});
			}
		}
		public void TestFetchHints_GetEI_GBFromDestinationParty()
		{
			var msgIds = new List<MsgIdUri>();
			var metaDictionary = new Dictionary<MsgIdUri, Dictionary<string, string>>();

			var preCompany = Factory.NewWithValidTestData<GlbCompany>();
			preCompany.GC_Code = "CCC";
			var preBranch = preCompany.Branches.AddNew();
			preBranch.GB_Code = "BBB";
			preBranch.FillWithValidTestData();

			for (ulong i = 0; i < 100u; i++)
			{
				var msgIdUri = new MsgIdUri { Msgid = i + 1 };
				msgIds.Add(msgIdUri);
				var testMsgMetaData = new Dictionary<string, string>();
				var testMsgTrackingId = ZGuid.NewZGuid();
				testMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = "TST";
				testMsgMetaData[Constants.CustomMsgAttributes.SourceParty] = "TSTSENDER";
				var systemRegKey = CargoWise.Application.ObjectFactory.Get<IProductRegistration>().Key;
				var destinationParty = $"{systemRegKey.EnterpriseCode}C{i:D2}{systemRegKey.ServerCode}";
				testMsgMetaData[Constants.CustomMsgAttributes.DestinationParty] = destinationParty;
				testMsgMetaData[Constants.CustomMsgAttributes.MessageTrackingID] = testMsgTrackingId.ToString();
				testMsgMetaData[Constants.CustomMsgAttributes.MessageType] = "MST";
				metaDictionary.Add(msgIdUri, testMsgMetaData);

				if (i % 3 == 0)
				{
					var company = Factory.NewWithValidTestData<GlbCompany>();
					company.GC_Code = $"C{i:D2}";
					var branch = company.Branches.AddNew();
					branch.GB_Code = $"B{i:D2}";
					branch.FillWithValidTestData();
					if (i % 5 == 0)
					{
						var oppositeInterchange = Factory.New<EDIInterchange>();
						oppositeInterchange.EI_ApplicationCode = "TST";
						oppositeInterchange.EI_InterchangeType = "MST";
						oppositeInterchange.EI_SessionGUID = testMsgTrackingId;
						oppositeInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
						oppositeInterchange.EI_GB = preBranch.PK;
						oppositeInterchange.EI_From = "TSTSENDER";
						oppositeInterchange.EI_To = "TSTRECIPIENT";
						oppositeInterchange.EI_TransportType = EDIInterchange.TransportType.xT;
						oppositeInterchange.EI_Status = EDIInterchange.Status.Sent;
						oppositeInterchange.EI_XTInternalMsgID = new Random().Next(10000, 99999);
					}
				}
			}

			Factory.Save();

			var expectedDbHits = new Dictionary<string, int>
			{
				{ EDIInterchangeSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ GlbBranchSchema.Constants.TableName, 3 }
			};

			var factory = new BusinessObjectFactory();
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, factory))
			{
				var handler = new SaveToEdIInterchangeHandlerForTest_SaveCount(factory, new Logger());
				handler.HandleReceivedMessageBatch(msgIds,
					msgid => metaDictionary[msgid],
					msgid => new AsyncServerStreamingCall<ResultByteChunk>(new ResultByteChunkReaderForTest(""), null, null, null, () => { }),
					msgData =>
					{
						var testMsgBody = new MemoryStream();
						var testMsgBytes = Encoding.UTF8.GetBytes("Test Message");
						testMsgBody.Write(testMsgBytes, 0, testMsgBytes.Length);
						return testMsgBody;
					});
			}

			var messages = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Receive));
			AssertEquals(100, messages.Length);
			AssertEquals(27, messages.Count(m => m.EI_GB.ToString() != "27a55065-ac88-4ec3-8bed-e575e79172cb" && m.EI_GB.ToString() != preBranch.PK.ToString()));
		}

		public void TestEI_GB_ValidBranchFallbackByMessageTrackingIdQuery()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();

			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();

			Factory.Save();

			var logger = new TestLogger();
			var testMsgBody = new MemoryStream();
			var testMsgBytes = Encoding.UTF8.GetBytes("Test Message");
			testMsgBody.Write(testMsgBytes, 0, testMsgBytes.Length);
			var testMsgMetaData = new Dictionary<string, string>();
			var testMsgTrackingID = ZGuid.NewZGuid();
			testMsgMetaData[Constants.xTMsgAttributes.refexternal] = 12345UL.ToString();
			testMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = "TST";
			testMsgMetaData[Constants.CustomMsgAttributes.SourceParty] = "TSTSENDER";
			testMsgMetaData[Constants.CustomMsgAttributes.DestinationParty] = "TSTRECIPIENT";
			testMsgMetaData[Constants.CustomMsgAttributes.MessageTrackingID] = testMsgTrackingID.ToString();
			testMsgMetaData[Constants.CustomMsgAttributes.MessageType] = "MST";

			var oppositeInterchange = Factory.New<EDIInterchange>();
			oppositeInterchange.EI_ApplicationCode = "TST";
			oppositeInterchange.EI_InterchangeType = "MST";
			oppositeInterchange.EI_SessionGUID = testMsgTrackingID;
			oppositeInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			oppositeInterchange.EI_GB = branch.PK;
			oppositeInterchange.EI_From = "TSTRECIPIENT";
			oppositeInterchange.EI_To = "TSTSENDER";
			oppositeInterchange.EI_Status = EDIInterchangeStatusList.Codes.Sent;
			oppositeInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;

			Factory.Save();

			var handler = new SaveToEDIInterchangeHandler(logger);
			handler.HandleReceivedMessageBatch(new List<MsgIdUri>() { new MsgIdUri() { Msgid = 1u } },
				msgid => testMsgMetaData,
				msgid => new AsyncServerStreamingCall<ResultByteChunk>(new ResultByteChunkReaderForTest(""), null, null, null, () => { }),
				msgData => testMsgBody);

			var assertFactory = new BusinessObjectFactory();
			var reloadedEDIInterchanges = assertFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Receive));
			AssertEquals(1, reloadedEDIInterchanges.Length);
			var rld = reloadedEDIInterchanges[0];
			AssertEquals("TST", rld.EI_ApplicationCode);
			AssertEquals("TSTSENDER", rld.EI_From);
			AssertEquals("TSTRECIPIENT", rld.EI_To);
			AssertEquals(testMsgTrackingID, rld.EI_SessionGUID);
			AssertEquals("MST", rld.EI_InterchangeType);
			AssertEquals("Test Message", rld.EI_BodyText);
			AssertEquals("Should populate a correct branch.", branch.PK, rld.EI_GB);
			AssertEquals("Should contain expected xT MsgId", 1L, rld.EI_XTInternalMsgID);

			AssertArrayEqualsByElements(Array.Empty<string>(), logger.InfoLogs.ToArray());
		}

		public void TestEI_GB_CorrectBranchFallback_ByDestinationParty()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "AAA";

			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();

			Factory.Save();

			var logger = new TestLogger();
			var testMsgBody = new MemoryStream();
			var testMsgBytes = Encoding.UTF8.GetBytes("Test Message");
			testMsgBody.Write(testMsgBytes, 0, testMsgBytes.Length);
			var testMsgMetaData = new Dictionary<string, string>();
			var testMsgTrackingID = ZGuid.NewZGuid();
			testMsgMetaData[Constants.xTMsgAttributes.refexternal] = 12345UL.ToString();
			testMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = "TST";
			testMsgMetaData[Constants.CustomMsgAttributes.SourceParty] = "TSTSENDER";
			var systemRegKey = CargoWise.Application.ObjectFactory.Get<IProductRegistration>().Key;
			var destinationParty = $"{systemRegKey.EnterpriseCode}AAA{systemRegKey.ServerCode}";
			testMsgMetaData[Constants.CustomMsgAttributes.DestinationParty] = destinationParty;
			testMsgMetaData[Constants.CustomMsgAttributes.MessageTrackingID] = testMsgTrackingID.ToString();
			testMsgMetaData[Constants.CustomMsgAttributes.MessageType] = "MST";

			Factory.Save();

			var expectedDbHits = new Dictionary<string, int>
			{
				{ EDIInterchangeSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ GlbBranchSchema.Constants.TableName, 1 }
			};
			var factory = new BusinessObjectFactory();
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, factory))
			{
				var handler = new SaveToEdIInterchangeHandlerForTest_SaveCount(factory, logger);
				handler.HandleReceivedMessageBatch(new List<MsgIdUri>() { new MsgIdUri() { Msgid = 1u } },
					msgid => testMsgMetaData,
					msgid => new AsyncServerStreamingCall<ResultByteChunk>(
						new ResultByteChunkReaderForTest(""), null, null, null, () => { }),
					msgData => testMsgBody);
			}

			var assertFactory = new BusinessObjectFactory();
			var reloadedEDIInterchanges = assertFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Receive));
			AssertEquals(1, reloadedEDIInterchanges.Length);
			var rld = reloadedEDIInterchanges[0];
			AssertEquals("TST", rld.EI_ApplicationCode);
			AssertEquals("TSTSENDER", rld.EI_From);
			AssertEquals(destinationParty, rld.EI_To);
			AssertEquals(testMsgTrackingID, rld.EI_SessionGUID);
			AssertEquals("MST", rld.EI_InterchangeType);
			AssertEquals("Test Message", rld.EI_BodyText);
			AssertEquals("Should populate a correct branch.", branch.PK, rld.EI_GB);
			AssertEquals("Should contain expected xT MsgId", 1L, rld.EI_XTInternalMsgID);
		}

		public void TestSuccessfulSave()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();

			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();

			Factory.Save();

			var logger = new TestLogger();
			var testMsgBody = new MemoryStream();
			var testMsgBytes = Encoding.UTF8.GetBytes("Test Message");
			testMsgBody.Write(testMsgBytes, 0, testMsgBytes.Length);
			var testMsgMetaData = new Dictionary<string, string>();
			var testMsgTrackingID = ZGuid.NewZGuid();
			testMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = "TST";
			testMsgMetaData[Constants.CustomMsgAttributes.SourceParty] = "TSTSENDER";
			testMsgMetaData[Constants.CustomMsgAttributes.DestinationParty] = "TSTRECIPIENT";
			testMsgMetaData[Constants.CustomMsgAttributes.MessageTrackingID] = testMsgTrackingID.ToString();
			testMsgMetaData[Constants.CustomMsgAttributes.MessageType] = "MST";

			var oppositeInterchange = Factory.New<EDIInterchange>();
			oppositeInterchange.EI_ApplicationCode = "TST";
			oppositeInterchange.EI_InterchangeType = "MST";
			oppositeInterchange.EI_SessionGUID = testMsgTrackingID;
			oppositeInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			oppositeInterchange.EI_GB = branch.PK;
			oppositeInterchange.EI_From = "TSTRECIPIENT";
			oppositeInterchange.EI_To = "TSTSENDER";
			oppositeInterchange.EI_Status = EDIInterchangeStatusList.Codes.Sent;
			oppositeInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;

			Factory.Save();

			var handler = new SaveToEDIInterchangeHandler(logger);
			handler.HandleReceivedMessageBatch(new List<MsgIdUri>() { new MsgIdUri() { Msgid = 1u } },
				msgid => testMsgMetaData,
				msgid => new AsyncServerStreamingCall<ResultByteChunk>(new ResultByteChunkReaderForTest(""), null, null, null, () => { }),
				msgData => testMsgBody);

			var assertFactory = new BusinessObjectFactory();
			var reloadedEDIInterchanges = assertFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Receive));
			AssertEquals(1, reloadedEDIInterchanges.Length);
			var rld = reloadedEDIInterchanges[0];
			AssertEquals("TST", rld.EI_ApplicationCode);
			AssertEquals("TSTSENDER", rld.EI_From);
			AssertEquals("TSTRECIPIENT", rld.EI_To);
			AssertEquals(testMsgTrackingID, rld.EI_SessionGUID);
			AssertEquals("MST", rld.EI_InterchangeType);
			AssertEquals("Test Message", rld.EI_BodyText);
			AssertEquals("Should populate a correct branch.", branch.PK, rld.EI_GB);
			AssertEquals("Should contain expected xT MsgId", 1L, rld.EI_XTInternalMsgID);

			AssertArrayEqualsByElements(Array.Empty<string>(), logger.InfoLogs.ToArray());
		}

		public void TestSuccessfulSaveMatchingXTInternalMsgIDToRefExternalAttribute()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();

			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();

			Factory.Save();

			var logger = new TestLogger();
			var testMsgBody = new MemoryStream();
			var testMsgBytes = Encoding.UTF8.GetBytes("Test Message");
			testMsgBody.Write(testMsgBytes, 0, testMsgBytes.Length);
			var testMsgMetaData = new Dictionary<string, string>();
			var testMsgTrackingID = ZGuid.NewZGuid();
			testMsgMetaData[Constants.xTMsgAttributes.refexternal] = 12345UL.ToString();
			testMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = "TST";

			var oppositeInterchange = Factory.New<EDIInterchange>();
			oppositeInterchange.EI_ApplicationCode = "TST";
			oppositeInterchange.EI_InterchangeType = "MST";
			oppositeInterchange.EI_SessionGUID = testMsgTrackingID;
			oppositeInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			oppositeInterchange.EI_GB = branch.PK;
			oppositeInterchange.EI_From = "TSTRECIPIENT";
			oppositeInterchange.EI_To = "TSTSENDER";
			oppositeInterchange.EI_Status = EDIInterchangeStatusList.Codes.Sent;
			oppositeInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
			oppositeInterchange.EI_XTInternalMsgID = 12345L;

			Factory.Save();

			var handler = new SaveToEDIInterchangeHandler(logger);
			handler.HandleReceivedMessageBatch(new List<MsgIdUri>() { new MsgIdUri() { Msgid = 1u } },
				msgid => testMsgMetaData,
				msgid => new AsyncServerStreamingCall<ResultByteChunk>(new ResultByteChunkReaderForTest(""), null, null, null, () => { }),
				msgData => testMsgBody);

			var assertFactory = new BusinessObjectFactory();
			var reloadedEDIInterchanges = assertFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Receive));
			AssertEquals(1, reloadedEDIInterchanges.Length);
			var rld = reloadedEDIInterchanges[0];
			AssertEquals("TST", rld.EI_ApplicationCode);
			AssertEquals("TSTSENDER", rld.EI_From);
			AssertEquals("TSTRECIPIENT", rld.EI_To);
			AssertEquals(testMsgTrackingID, rld.EI_SessionGUID);
			AssertEquals("MST", rld.EI_InterchangeType);
			AssertEquals("Test Message", rld.EI_BodyText);
			AssertEquals("Should populate a correct branch.", branch.PK, rld.EI_GB);
			AssertEquals("Should contain expected xT MsgId", 1L, rld.EI_XTInternalMsgID);

			AssertArrayEqualsByElements(Array.Empty<string>(), logger.InfoLogs.ToArray());
		}

		public void TestSuccessfulSaveUsingRefExternalAttributeToQueryxTServer()
		{
			var logger = new TestLogger();
			var testMsgBody = new MemoryStream();
			var testMsgBytes = Encoding.UTF8.GetBytes("Test Message");
			testMsgBody.Write(testMsgBytes, 0, testMsgBytes.Length);
			var testMsgMetaData = new Dictionary<string, string>();
			testMsgMetaData[Constants.xTMsgAttributes.refexternal] = 12345UL.ToString();

			var orgMsgMetaData = new Dictionary<string, string>();
			var orgMsgTrackingID = ZGuid.NewZGuid();
			orgMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = "TST";
			orgMsgMetaData[Constants.CustomMsgAttributes.SourceParty] = "TSTSENDER";
			orgMsgMetaData[Constants.CustomMsgAttributes.DestinationParty] = "TSTRECIPIENT";
			orgMsgMetaData[Constants.CustomMsgAttributes.MessageTrackingID] = orgMsgTrackingID.ToString();
			orgMsgMetaData[Constants.CustomMsgAttributes.MessageType] = "MST";

			Dictionary<string, string> GetMessageMetaData(MsgIdUri msgid)
			{
				if (msgid.Msgid == 1U)
				{
					return testMsgMetaData;
				}
				return orgMsgMetaData;
			}

			Factory.Save();

			var handler = new SaveToEDIInterchangeHandler(logger);
			handler.HandleReceivedMessageBatch(new List<MsgIdUri>() { new MsgIdUri() { Msgid = 1u } },
				GetMessageMetaData,
				msgid => new AsyncServerStreamingCall<ResultByteChunk>(new ResultByteChunkReaderForTest(""), null, null, null, () => { }),
				msgData => testMsgBody);

			var assertFactory = new BusinessObjectFactory();
			var reloadedEDIInterchanges = assertFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Receive));
			AssertEquals(1, reloadedEDIInterchanges.Length);
			var rld = reloadedEDIInterchanges[0];
			AssertEquals("TST", rld.EI_ApplicationCode);
			AssertEquals("TSTRECIPIENT", rld.EI_From);
			AssertEquals("TSTSENDER", rld.EI_To);
			AssertEquals(orgMsgTrackingID, rld.EI_SessionGUID);
			AssertEquals("MST", rld.EI_InterchangeType);
			AssertEquals("Test Message", rld.EI_BodyText);
			AssertEquals("Should populate CurrentBranch since no EDIInterchange exists.", GlbBranch.CurrentBranch.PK, rld.EI_GB);
			AssertEquals("Should contain expected xT MsgId", 1L, rld.EI_XTInternalMsgID);

			AssertArrayEqualsByElements(Array.Empty<string>(), logger.InfoLogs.ToArray());
		}

		public void TestSuccessfulSaveWhenRefExternalIsxTObjReferenceToQueryServer()
		{
			var logger = new TestLogger();
			var testMsgBody = new MemoryStream();
			var testMsgBytes = Encoding.UTF8.GetBytes("Test Message");
			testMsgBody.Write(testMsgBytes, 0, testMsgBytes.Length);
			var testMsgMetaData = new Dictionary<string, string>();
			testMsgMetaData[Constants.xTMsgAttributes.refexternal] = "xt-msg:4ef4ba7d-f95f-4462-8a34-29bd2388772d";

			var orgMsgMetaData = new Dictionary<string, string>();
			var orgMsgTrackingID = ZGuid.NewZGuid();
			orgMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = "TST";
			orgMsgMetaData[Constants.CustomMsgAttributes.SourceParty] = "TSTSENDER";
			orgMsgMetaData[Constants.CustomMsgAttributes.DestinationParty] = "TSTRECIPIENT";
			orgMsgMetaData[Constants.CustomMsgAttributes.MessageTrackingID] = orgMsgTrackingID.ToString();
			orgMsgMetaData[Constants.CustomMsgAttributes.MessageType] = "MST";

			Dictionary<string, string> GetMessageMetaData(MsgIdUri msgid)
			{
				if (msgid.Msgid == 1U)
				{
					return testMsgMetaData;
				}
				else if (msgid.Uri == "xt-msg:4ef4ba7d-f95f-4462-8a34-29bd2388772d")
				{
					return orgMsgMetaData;
				}
				return null;
			}

			Factory.Save();

			var handler = new SaveToEDIInterchangeHandler(logger);
			handler.HandleReceivedMessageBatch(new List<MsgIdUri>() { new() { Msgid = 1u } },
				GetMessageMetaData,
				_ => new AsyncServerStreamingCall<ResultByteChunk>(new ResultByteChunkReaderForTest(""), null, null, null, () => { }),
				msgData => testMsgBody);

			var assertFactory = new BusinessObjectFactory();
			var reloadedEDIInterchanges = assertFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Receive));
			AssertEquals(1, reloadedEDIInterchanges.Length);
			var rld = reloadedEDIInterchanges[0];
			AssertEquals("TST", rld.EI_ApplicationCode);
			AssertEquals("TSTRECIPIENT", rld.EI_From);
			AssertEquals("TSTSENDER", rld.EI_To);
			AssertEquals(orgMsgTrackingID, rld.EI_SessionGUID);
			AssertEquals("MST", rld.EI_InterchangeType);
			AssertEquals("Test Message", rld.EI_BodyText);
			AssertEquals("Should populate CurrentBranch since no EDIInterchange exists.", GlbBranch.CurrentBranch.PK, rld.EI_GB);
			AssertEquals("Should contain expected xT MsgId", 1L, rld.EI_XTInternalMsgID);

			AssertArrayEqualsByElements(Array.Empty<string>(), logger.AllLogs.Select(x => x.Message).ToArray());
		}

		public void TestSuccessfulSaveUsingRefExternalAndKeepingOriginalAttrs()
		{
			var logger = new TestLogger();
			var testMsgBody = new MemoryStream();
			var testMsgBytes = Encoding.UTF8.GetBytes("Test Message");
			testMsgBody.Write(testMsgBytes, 0, testMsgBytes.Length);
			var testMsgMetaData = new Dictionary<string, string>();
			testMsgMetaData[Constants.xTMsgAttributes.refexternal] = 12345UL.ToString();
			testMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = "TSR";
			testMsgMetaData[Constants.CustomMsgAttributes.MessageType] = "MSR";

			var orgMsgMetaData = new Dictionary<string, string>();
			var orgMsgTrackingID = ZGuid.NewZGuid();
			orgMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = "TST";
			orgMsgMetaData[Constants.CustomMsgAttributes.SourceParty] = "TSTSENDER";
			orgMsgMetaData[Constants.CustomMsgAttributes.DestinationParty] = "TSTRECIPIENT";
			orgMsgMetaData[Constants.CustomMsgAttributes.MessageTrackingID] = orgMsgTrackingID.ToString();
			orgMsgMetaData[Constants.CustomMsgAttributes.MessageType] = "MST";

			Dictionary<string, string> GetMessageMetaData(MsgIdUri msgid)
			{
				if (msgid.Msgid == 1U)
				{
					return testMsgMetaData;
				}
				return orgMsgMetaData;
			}

			Factory.Save();

			var handler = new SaveToEDIInterchangeHandler(logger);
			handler.HandleReceivedMessageBatch(new List<MsgIdUri>() { new MsgIdUri() { Msgid = 1u } },
				GetMessageMetaData,
				msgid => new AsyncServerStreamingCall<ResultByteChunk>(new ResultByteChunkReaderForTest(""), null, null, null, () => { }),
				msgData => testMsgBody);

			var assertFactory = new BusinessObjectFactory();
			var reloadedEDIInterchanges = assertFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Receive));
			AssertEquals(1, reloadedEDIInterchanges.Length);
			var rld = reloadedEDIInterchanges[0];
			AssertEquals("TSR", rld.EI_ApplicationCode);
			AssertEquals("TSTRECIPIENT", rld.EI_From);
			AssertEquals("TSTSENDER", rld.EI_To);
			AssertEquals(orgMsgTrackingID, rld.EI_SessionGUID);
			AssertEquals("MSR", rld.EI_InterchangeType);
			AssertEquals("Test Message", rld.EI_BodyText);
			AssertEquals("Should populate CurrentBranch since no EDIInterchange exists.", GlbBranch.CurrentBranch.PK, rld.EI_GB);
			AssertEquals("Should contain expected xT MsgId", 1L, rld.EI_XTInternalMsgID);

			AssertArrayEqualsByElements(Array.Empty<string>(), logger.InfoLogs.ToArray());
		}

		public void TestSavingIndividuallyErrorLogging()
		{
			var logger = new TestLogger();
			var testMsgBody = new MemoryStream();
			var testMsgBytes = Encoding.UTF8.GetBytes("Test Message");
			testMsgBody.Write(testMsgBytes, 0, testMsgBytes.Length);
			var testMsgMetaData = new Dictionary<string, string>();
			testMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = "TST";
			testMsgMetaData[Constants.CustomMsgAttributes.SourceParty] = "TSTSENDER";
			testMsgMetaData[Constants.CustomMsgAttributes.DestinationParty] = "TSTRECIPIENT";
			testMsgMetaData[Constants.CustomMsgAttributes.MessageType] = "MST";
			var trackingId = Guid.NewGuid().ToString();
			testMsgMetaData[Constants.CustomMsgAttributes.MessageTrackingID] = trackingId;

			var handler = new SaveToEdIInterchangeHandlerForTest(logger, true, true);
			handler.HandleReceivedMessageBatch(new List<MsgIdUri>() { new MsgIdUri() { Msgid = 1u } },
				msgid => testMsgMetaData,
				msgid => new AsyncServerStreamingCall<ResultByteChunk>(new ResultByteChunkReaderForTest(""), null, null, null, () => { }),
				msgData => testMsgBody);

			var assertFactory = new BusinessObjectFactory();
			var reloadedEDIInterchanges = assertFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT));
			AssertEquals(0, reloadedEDIInterchanges.Length);

			AssertArrayEqualsByElements(new string[] { "Unable to save messages in batch. Each message will be saved separately with separate factory.", $@"Downloaded Message cannot be saved: 
** Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Unknown record

Inner Message = I am testing

 MsgId: 1
AppCode: TST
Sender: TSTSENDER
Recipient: TSTRECIPIENT
SessionID: {trackingId}
The message would be received in next run." }, logger.AllLogs.Select(x => x.Message.Trim()).ToArray());
			AssertEquals("Exception Key", Shared.Utils.ReceiveErrorReportKey, ErrorReporter.LastKeyReported);
			AssertContains("Message include MessageId", $@"Downloaded Message cannot be saved: 
** Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Unknown record

Inner Message = I am testing

 MsgId: 1
AppCode: TST
Sender: TSTSENDER
Recipient: TSTRECIPIENT
SessionID: {trackingId}
The message would be received in next run.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestSavingIndividuallyErrorThenRetry()
		{
			var logger = new TestLogger();
			var testMsgBody = new MemoryStream();
			var testMsgBytes = Encoding.UTF8.GetBytes("Test Message");
			testMsgBody.Write(testMsgBytes, 0, testMsgBytes.Length);
			var testMsgMetaData = new Dictionary<string, string>();
			testMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = "TST";
			testMsgMetaData[Constants.CustomMsgAttributes.SourceParty] = "TSTSENDER";
			testMsgMetaData[Constants.CustomMsgAttributes.DestinationParty] = "TSTRECIPIENT";
			testMsgMetaData[Constants.CustomMsgAttributes.MessageType] = "MST";
			var trackingId = Guid.NewGuid().ToString();
			testMsgMetaData[Constants.CustomMsgAttributes.MessageTrackingID] = trackingId;

			var handler = new SaveToEdIInterchangeHandlerForTest(logger, true, true);
			using (DirectxTMessagingRegistry.Instance.XTServerMessagesMaxReceivingRetryCount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3))
			{
				for (var i = 0; i <= 3; i++)
				{
					if (i > 0)
					{
						testMsgMetaData[Constants.CustomMsgAttributes.ReceivingRetryCount] = i.ToString();
					}

					handler.HandleReceivedMessageBatch(new List<MsgIdUri>() { new MsgIdUri() { Msgid = 1u } },
						msgid => testMsgMetaData,
						msgid => new AsyncServerStreamingCall<ResultByteChunk>(
							new ResultByteChunkReaderForTest(""), null, null, null, () => { }),
						msgData => testMsgBody);
				}
			}

			var assertFactory = new BusinessObjectFactory();
			var reloadedEDIInterchanges = assertFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT));
			AssertEquals(0, reloadedEDIInterchanges.Length);

			AssertArrayEqualsByElements(new string[] { "Unable to save messages in batch. Each message will be saved separately with separate factory.", $@"Downloaded Message cannot be saved: 
** Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Unknown record

Inner Message = I am testing

 MsgId: 1
AppCode: TST
Sender: TSTSENDER
Recipient: TSTRECIPIENT
SessionID: {trackingId}
The message would be received in next run.", "Unable to save messages in batch. Each message will be saved separately with separate factory.", $@"Downloaded Message cannot be saved: 
** Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Unknown record

Inner Message = I am testing

 MsgId: 1
AppCode: TST
Sender: TSTSENDER
Recipient: TSTRECIPIENT
SessionID: {trackingId}
The message would be received in next run.", "Unable to save messages in batch. Each message will be saved separately with separate factory.", $@"Downloaded Message cannot be saved: 
** Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Unknown record

Inner Message = I am testing

 MsgId: 1
AppCode: TST
Sender: TSTSENDER
Recipient: TSTRECIPIENT
SessionID: {trackingId}
The message would be received in next run.", "Unable to save messages in batch. Each message will be saved separately with separate factory.", $@"Downloaded Message cannot be saved: 
** Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Unknown record

Inner Message = I am testing

 MsgId: 1
AppCode: TST
Sender: TSTSENDER
Recipient: TSTRECIPIENT
SessionID: {trackingId}
The message would set to Error and would not retry to receive max retrying times were all failed." }, logger.AllLogs.Select(x => x.Message.Trim()).ToArray());
			ErrorReporter.Clear();
		}

		public void TestErrorOnGetMetadata()
		{
			var logger = new TestLogger();
			var testMsgBody = new MemoryStream();
			var testMsgBytes = Encoding.UTF8.GetBytes("Test Message");
			testMsgBody.Write(testMsgBytes, 0, testMsgBytes.Length);
			var testMsgMetaData = new Dictionary<string, string>();
			testMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = "TST";
			testMsgMetaData[Constants.CustomMsgAttributes.SourceParty] = "TSTSENDER";
			testMsgMetaData[Constants.CustomMsgAttributes.DestinationParty] = "TSTRECIPIENT";

			var handler = new SaveToEDIInterchangeHandler(logger);
			handler.HandleReceivedMessageBatch(new List<MsgIdUri>() { new MsgIdUri() { Msgid = 1u } },
				msgid => throw new Exception($"get metadata failed on message {msgid}"),
				msgid => new AsyncServerStreamingCall<ResultByteChunk>(new ResultByteChunkReaderForTest(""), null, null, null, () => { }),
				msgData => testMsgBody);

			var assertFactory = new BusinessObjectFactory();
			var reloadedEDIInterchanges = assertFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT));
			AssertEquals(0, reloadedEDIInterchanges.Length);

			AssertArrayEqualsByElements(new string[] { "Error on reading metadata of message 1: get metadata failed on message { \"msgid\": \"1\" }" }, logger.ErrorLogs.Select(x => x.Trim()).ToArray());
			AssertEquals("LogType", LogType.Error, logger.AllLogs.Where(x => x.Message.Contains("Error when downloading/reading message")).Select(x => x.Type).FirstOrDefault());
			AssertEquals("Exception Key", Shared.Utils.ReceiveErrorReportKey, ErrorReporter.LastKeyReported);
			AssertContains("Message include MessageId", "Error on reading metadata of message 1: get metadata failed on message { \"msgid\": \"1\" }", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestErrorInCreatingEDIInterchange_MissingKeyInformation()
		{
			var logger = new TestLogger();
			var testMsgBody = new MemoryStream();
			var testMsgBytes = Encoding.UTF8.GetBytes("Test Message");
			testMsgBody.Write(testMsgBytes, 0, testMsgBytes.Length);
			var testMsgMetaData = new Dictionary<string, string>();
			testMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = "TST";
			testMsgMetaData[Constants.CustomMsgAttributes.SourceParty] = "TSTSENDER";
			testMsgMetaData[Constants.CustomMsgAttributes.DestinationParty] = "TSTRECIPIENT";

			var handler = new SaveToEDIInterchangeHandler(logger);
			handler.HandleReceivedMessageBatch(new List<MsgIdUri>() { new MsgIdUri() { Msgid = 1u } },
				msgid => testMsgMetaData,
				msgid => new AsyncServerStreamingCall<ResultByteChunk>(new ResultByteChunkReaderForTest(""), null, null, null, () => { }),
				msgData => testMsgBody);

			var assertFactory = new BusinessObjectFactory();
			var reloadedEDIInterchanges = assertFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT));
			AssertEquals(0, reloadedEDIInterchanges.Length);

			AssertArrayEqualsByElements(new string[] { @"Error on generate metadataHelper of message:Missing Key Message Information MsgId: 1
AppCode: TST
Sender: TSTSENDER
Recipient: TSTRECIPIENT
SessionID:" }, logger.ErrorLogs.Select(x => x.Trim()).ToArray());
			AssertEquals("LogType", LogType.Error, logger.AllLogs.Where(x => x.Message.Contains("Error when downloading/reading message")).Select(x => x.Type).FirstOrDefault());
			AssertEquals("Exception Key", Shared.Utils.ReceiveErrorReportKey, ErrorReporter.LastKeyReported);
			AssertContains("Message include MessageId", @"Error on generate metadataHelper of message:Missing Key Message Information MsgId: 1
AppCode: TST
Sender: TSTSENDER
Recipient: TSTRECIPIENT
SessionID:  ", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestErrorInCreatingEDIInterchange_EmptySessionID()
		{
			var logger = new TestLogger();
			var testMsgBody = new MemoryStream();
			var testMsgBytes = Encoding.UTF8.GetBytes("Test Message");
			testMsgBody.Write(testMsgBytes, 0, testMsgBytes.Length);
			var testMsgMetaData = new Dictionary<string, string>();
			var testMsgTrackingID = ZGuid.Empty;
			testMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = "TST";
			testMsgMetaData[Constants.CustomMsgAttributes.SourceParty] = "TSTSENDER";
			testMsgMetaData[Constants.CustomMsgAttributes.DestinationParty] = "TSTRECIPIENT";
			testMsgMetaData[Constants.CustomMsgAttributes.MessageTrackingID] = testMsgTrackingID.ToString();

			var handler = new SaveToEDIInterchangeHandler(logger);
			handler.HandleReceivedMessageBatch(new List<MsgIdUri>() { new MsgIdUri() { Msgid = 1u } },
				msgid => testMsgMetaData,
				msgid => new AsyncServerStreamingCall<ResultByteChunk>(new ResultByteChunkReaderForTest(""), null, null, null, () => { }),
				msgData => testMsgBody);

			var assertFactory = new BusinessObjectFactory();
			var reloadedEDIInterchanges = assertFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT));
			AssertEquals(0, reloadedEDIInterchanges.Length);

			AssertArrayEqualsByElements(new string[] { @"Error on generate metadataHelper of message:Missing Key Message Information MsgId: 1
AppCode: TST
Sender: TSTSENDER
Recipient: TSTRECIPIENT
SessionID: 00000000-0000-0000-0000-000000000000" }, logger.ErrorLogs.Select(x => x.Trim()).ToArray());
			ErrorReporter.Clear();
		}

		public void TestErrorInCreatingEDIInterchange_GetMsgDataExecption()
		{
			var logger = new TestLogger();
			var testMsgBody = new MemoryStream();
			var testMsgBytes = Encoding.UTF8.GetBytes("Test Message");
			testMsgBody.Write(testMsgBytes, 0, testMsgBytes.Length);
			var testMsgMetaData = new Dictionary<string, string>();
			var testMsgTrackingID = ZGuid.NewZGuid();
			testMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = "TST";
			testMsgMetaData[Constants.CustomMsgAttributes.SourceParty] = "TSTSENDER";
			testMsgMetaData[Constants.CustomMsgAttributes.DestinationParty] = "TSTRECIPIENT";
			testMsgMetaData[Constants.CustomMsgAttributes.MessageTrackingID] = testMsgTrackingID.ToString();

			var handler = new SaveToEDIInterchangeHandler(logger);
			handler.HandleReceivedMessageBatch(new List<MsgIdUri>() { new MsgIdUri() { Msgid = 1u } },
				msgid => testMsgMetaData,
				msgid => new AsyncServerStreamingCall<ResultByteChunk>(new ResultByteChunkReaderForTest(""), null, null, null, () => { throw new Exception("mock exception in GetMsgData"); }),
				msgData => testMsgBody);

			var assertFactory = new BusinessObjectFactory();
			var reloadedEDIInterchanges = assertFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT));
			AssertEquals(0, reloadedEDIInterchanges.Length);

			AssertArrayEqualsByElements(new string[] { $@"Error when downloading/reading message:mock exception in GetMsgData MsgId: 1
AppCode: TST
Sender: TSTSENDER
Recipient: TSTRECIPIENT
SessionID: {testMsgTrackingID}" }, logger.ErrorLogs.Select(x => x.Trim()).ToArray());
			ErrorReporter.Clear();
		}

		public void TestErrorInCreatingEDIInterchange_LoadReplyIntoMemoryExecption()
		{
			var logger = new TestLogger();
			var testMsgBody = new MemoryStream();
			var testMsgBytes = Encoding.UTF8.GetBytes("Test Message");
			testMsgBody.Write(testMsgBytes, 0, testMsgBytes.Length);
			var testMsgMetaData = new Dictionary<string, string>();
			var testMsgTrackingID = ZGuid.NewZGuid();
			testMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = "TST";
			testMsgMetaData[Constants.CustomMsgAttributes.SourceParty] = "TSTSENDER";
			testMsgMetaData[Constants.CustomMsgAttributes.DestinationParty] = "TSTRECIPIENT";
			testMsgMetaData[Constants.CustomMsgAttributes.MessageTrackingID] = testMsgTrackingID.ToString();

			var handler = new SaveToEDIInterchangeHandler(logger);
			handler.HandleReceivedMessageBatch(new List<MsgIdUri>() { new MsgIdUri() { Msgid = 1u } },
				msgid => testMsgMetaData,
				msgid => new AsyncServerStreamingCall<ResultByteChunk>(new ResultByteChunkReaderForTest(""), null, null, null, () => { }),
				msgData =>
				{
					throw new Exception("mock exception in LoadReplyIntoMemory");
				});

			var assertFactory = new BusinessObjectFactory();
			var reloadedEDIInterchanges = assertFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT));
			AssertEquals(0, reloadedEDIInterchanges.Length);

			AssertArrayEqualsByElements(new string[] { $@"Error when downloading/reading message:mock exception in LoadReplyIntoMemory MsgId: 1
AppCode: TST
Sender: TSTSENDER
Recipient: TSTRECIPIENT
SessionID: {testMsgTrackingID}" }, logger.ErrorLogs.Select(x => x.Trim()).ToArray());
			ErrorReporter.Clear();
		}

		public void TestLoadOppositeEdiInterchangeFail()
		{
			var logger = new TestLogger();
			var testMsgBody = new MemoryStream();
			var testMsgBytes = Encoding.UTF8.GetBytes("Test Message");
			testMsgBody.Write(testMsgBytes, 0, testMsgBytes.Length);

			var testMsgMetaData = new Dictionary<string, string>();
			var testMsgTrackingID = ZGuid.NewZGuid();
			testMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = "TST";
			testMsgMetaData[Constants.CustomMsgAttributes.SourceParty] = "TSTSENDER";
			testMsgMetaData[Constants.CustomMsgAttributes.DestinationParty] = "TSTRECIPIENT";
			testMsgMetaData[Constants.CustomMsgAttributes.MessageTrackingID] = testMsgTrackingID.ToString();
			testMsgMetaData[Constants.CustomMsgAttributes.MessageType] = "MST";

			var oppositeInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			oppositeInterchange.EI_ApplicationCode = "TST";
			oppositeInterchange.EI_InterchangeType = "MST";
			oppositeInterchange.EI_SessionGUID = testMsgTrackingID;
			oppositeInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			oppositeInterchange.EI_From = "TSTRECIPIENT";
			oppositeInterchange.EI_To = "TSTSENDER";
			oppositeInterchange.EI_Status = EDIInterchangeStatusList.Codes.Sent;
			oppositeInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
			oppositeInterchange.EI_XTInternalMsgID = 1L;
			oppositeInterchange.EI_IsActive = true;
			Factory.Save();

			var handler = new SaveToEdIInterchangeHandlerForTest2(logger);
			handler.HandleReceivedMessageBatch(new List<MsgIdUri>() { new MsgIdUri() { Msgid = 1u } },
				msgid => testMsgMetaData,
				msgid => new AsyncServerStreamingCall<ResultByteChunk>(new ResultByteChunkReaderForTest(""), null, null, null, () => { }),
				msgData => testMsgBody);

			var assertFactory = new BusinessObjectFactory();
			var reloadedInterchanges = assertFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT).AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Receive));
			CombineAssertions(() =>
			{
				AssertEquals(1, reloadedInterchanges.Length);
				AssertEquals("TSTSENDER", reloadedInterchanges[0].EI_From);
				AssertEquals(1, logger.WarningLogs.Count);

				var logForAssert = logger.WarningLogs[0].Trim();
				AssertEquals(true, logForAssert.StartsWith("Failed to load opposite interchanges of messages("));
				AssertEquals(true, logForAssert.Contains("), would try to get original data from xT server later. Exception:"));
			});
		}

		public void TestZSaveExceptionHandling()
		{
			var logger = new TestLogger();
			var testMsgBody = new MemoryStream();
			var testMsgBytes = Encoding.UTF8.GetBytes("Test Message");
			testMsgBody.Write(testMsgBytes, 0, testMsgBytes.Length);

			var testMsgMetaData = new Dictionary<string, string>();
			var testMsgTrackingID = ZGuid.NewZGuid();
			testMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = "TST";
			testMsgMetaData[Constants.CustomMsgAttributes.SourceParty] = "TSTSENDER";
			testMsgMetaData[Constants.CustomMsgAttributes.DestinationParty] = "TSTRECIPIENT";
			testMsgMetaData[Constants.CustomMsgAttributes.MessageTrackingID] = testMsgTrackingID.ToString();
			testMsgMetaData[Constants.CustomMsgAttributes.MessageType] = "MST";

			Factory.Saving += (f) => throw new ZSaveException(new ZDataException(new ApplicationException("I am testing"), null, null), f);
			var handler = new SaveToEdIInterchangeHandlerForTest(logger, true);
			handler.HandleReceivedMessageBatch(new List<MsgIdUri>() { new MsgIdUri() { Msgid = 1u } },
				msgid => testMsgMetaData,
				msgid => new AsyncServerStreamingCall<ResultByteChunk>(new ResultByteChunkReaderForTest(""), null, null, null, () => { }),
				msgData => testMsgBody);

			var assertFactory = new BusinessObjectFactory();
			var reloadedEDIInterchanges = assertFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT));
			CombineAssertions(() =>
			{
				AssertEquals(1, reloadedEDIInterchanges.Length);
				AssertEquals(1, logger.WarningLogs.Count);

				var logForAssert = logger.WarningLogs[0].Trim();
				AssertEquals("Unable to save messages in batch. Each message will be saved separately with separate factory.", logForAssert);

				ErrorReporter.Clear();
			});
		}

		public void TestSaveInHeaderText()
		{
			var logger = new TestLogger();
			var testMsgBody = new MemoryStream();
			var testMsgBytes = Encoding.UTF8.GetBytes("Test Message");
			testMsgBody.Write(testMsgBytes, 0, testMsgBytes.Length);
			var testMsgMetaData = new Dictionary<string, string>();
			var testMsgTrackingID = ZGuid.NewZGuid();
			testMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = "TST";
			testMsgMetaData[Constants.CustomMsgAttributes.SourceParty] = "TSTSENDER";
			testMsgMetaData[Constants.CustomMsgAttributes.DestinationParty] = "TSTRECIPIENT";
			testMsgMetaData[Constants.CustomMsgAttributes.MessageTrackingID] = testMsgTrackingID.ToString();
			testMsgMetaData[Constants.CustomMsgAttributes.MessageType] = "MST";

			testMsgMetaData["country.private"] = "private";
			testMsgMetaData["custom.country.normal"] = "normal";
			testMsgMetaData["custom.test"] = "Test";
			testMsgMetaData["custom.country.Empty"] = "";

			var handler = new SaveToEDIInterchangeHandler(logger);
			handler.HandleReceivedMessageBatch(new List<MsgIdUri>() { new MsgIdUri() { Msgid = 1u } },
				msgid => testMsgMetaData,
				msgid => new AsyncServerStreamingCall<ResultByteChunk>(new ResultByteChunkReaderForTest(""), null, null, null, () => { }),
				msgData => testMsgBody);

			var assertFactory = new BusinessObjectFactory();
			var reloadedEDIInterchanges = assertFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT));
			AssertEquals(1, reloadedEDIInterchanges.Length);
			var rld = reloadedEDIInterchanges[0];

			AssertEquals("{" +
				"\"custom.country.normal\":\"normal\"," +
				"\"custom.test\":\"Test\"," +
				"\"custom.country.Empty\":\"\"" +
				"}", rld.EI_HeaderText);
		}

		public void TestHandleReceivedMessageBatch()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();
			Factory.Save();

			var logger = new TestLogger();

			var testMsgMetaData = new Dictionary<string, string>();
			var testMsgTrackingID = ZGuid.NewZGuid();
			testMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = "TST";
			testMsgMetaData[Constants.CustomMsgAttributes.SourceParty] = "TSTSENDER";
			testMsgMetaData[Constants.CustomMsgAttributes.DestinationParty] = "TSTRECIPIENT";
			testMsgMetaData[Constants.CustomMsgAttributes.MessageTrackingID] = testMsgTrackingID.ToString();
			testMsgMetaData[Constants.CustomMsgAttributes.MessageType] = "MST";

			var oppositeInterchange = Factory.New<EDIInterchange>();
			oppositeInterchange.EI_ApplicationCode = "TST";
			oppositeInterchange.EI_InterchangeType = "MST";
			oppositeInterchange.EI_SessionGUID = testMsgTrackingID;
			oppositeInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			oppositeInterchange.EI_GB = branch.PK;
			oppositeInterchange.EI_From = "TSTSENDER";
			oppositeInterchange.EI_To = "TSTRECIPIENT";

			var oppositeInterchange2 = Factory.New<EDIInterchange>();
			oppositeInterchange2.EI_ApplicationCode = "TST";
			oppositeInterchange2.EI_InterchangeType = "MST";
			oppositeInterchange2.EI_SessionGUID = ZGuid.NewZGuid();
			oppositeInterchange2.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			oppositeInterchange2.EI_GB = branch.PK;
			oppositeInterchange2.EI_From = "TSTSENDER";
			oppositeInterchange2.EI_To = "TSTRECIPIENT";

			Factory.Save();

			var handler = new SaveToEdIInterchangeHandlerForTest_SaveCount(new BusinessObjectFactory(), logger);
			handler.HandleReceivedMessageBatch(new List<MsgIdUri>() { new MsgIdUri() { Msgid = 1u }, new MsgIdUri() { Msgid = 2u } },
				msgid => testMsgMetaData,
				msgid => new AsyncServerStreamingCall<ResultByteChunk>(new ResultByteChunkReaderForTest(""), null, null, null, () => { }),
				msgData =>
				{
					var testMsgBody = new MemoryStream();
					var testMsgBytes = Encoding.UTF8.GetBytes("Test Message");
					testMsgBody.Write(testMsgBytes, 0, testMsgBytes.Length);
					return testMsgBody;
				});
			AssertEquals("should process in batch", 1, handler.FactorySaveCount);
			ErrorReporter.Clear();
		}
	}

	class SaveToEdIInterchangeHandlerForTest : SaveToEDIInterchangeHandler
	{
		public SaveToEdIInterchangeHandlerForTest(ILogger logger, bool batchFactoryWithException = false, bool singleFactoryWithException = false) : base(logger)
		{
			BatchFactoryWithException = batchFactoryWithException;
			SingleFactoryWithException = singleFactoryWithException;
		}

		bool BatchFactoryWithException { get; }
		bool SingleFactoryWithException { get; }
		bool shouldReturnBatchFactory = true;

		protected override BusinessObjectFactory GetFactory()
		{
			var factory = new BusinessObjectFactory();
			if (shouldReturnBatchFactory)
			{
				if (BatchFactoryWithException)
				{
					factory.Saving += (f) => throw new ZSaveException(new ZDataException(new ApplicationException("I am testing"), null, null), f);
				}
				shouldReturnBatchFactory = false;
			}
			else
			{
				if (SingleFactoryWithException)
				{
					factory.Saving += (f) => throw new ZSaveException(new ZDataException(new ApplicationException("I am testing"), null, null), f);
				}
			}
			return factory;
		}
	}

	class SaveToEdIInterchangeHandlerForTest2 : SaveToEDIInterchangeHandler
	{
		public SaveToEdIInterchangeHandlerForTest2(ILogger logger) : base(logger)
		{
		}

		protected override BusinessObjectFactory GetFactory()
		{
			var factory = new BusinessObjectFactory();
			factory.Loaded += Factory_Loaded;
			return factory;
		}

		void Factory_Loaded(object sender, LoadedEventArgs e)
		{
			throw new NotImplementedException();
		}
	}

	class SaveToEdIInterchangeHandlerForTest_SaveCount(BusinessObjectFactory factory, ILogger logger) : SaveToEDIInterchangeHandler(logger)
	{
		public int FactorySaveCount { get; private set; }
		public readonly BusinessObjectFactory Factory = factory;

		protected override BusinessObjectFactory GetFactory()
		{
			Factory.Saving += f => FactorySaveCount++;
			return Factory;
		}
	}
}
