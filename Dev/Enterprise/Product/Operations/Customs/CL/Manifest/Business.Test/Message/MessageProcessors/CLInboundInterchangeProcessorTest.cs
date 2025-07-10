using System.IO;
using System.Text;
using System.Xml;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	sealed class CLInboundInterchangeProcessorTest : TestCaseWithFactory
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateMesssage()
		{
			var airAcceptedResponse = GetExpectedMessageXML(Path.Combine(BaseSourcePath, CLMessagingConstants.AirAcceptedResponse));
			var seaAcceptedResponse = GetExpectedMessageXML(Path.Combine(BaseSourcePath, CLMessagingConstants.SeaAcceptedResponse));

			var interchange1 = CreateInterchange(MessageTypes.Codes.CHB, CLMessageConstants.CLCustomsForSeaMode, seaAcceptedResponse);
			var interchange2 = CreateInterchange(MessageTypes.Codes.CHE, CLMessageConstants.CLCustomsForAirMode, airAcceptedResponse);

			var processor = new CLInboundInterchangeProcessor(new string[] { ApplicationCodeList.Codes.CLCustoms });
			processor.ExecuteBatch();

			var processedInterchange1 = interchange1;
			var processedInterchange2 = interchange2;

			processedInterchange1.Reload();
			processedInterchange2.Reload();

			AssertMessage(processedInterchange1);
			AssertMessage(processedInterchange2);
		}

		public void TestCreateMesssageWithEmptyBody()
		{
			var interchange1 = CreateInterchange(MessageTypes.Codes.CHB, CLMessageConstants.CLCustomsForSeaMode, ZString.Empty);
			var interchange2 = CreateInterchange(MessageTypes.Codes.CHE, CLMessageConstants.CLCustomsForAirMode, ZString.Empty);

			var processor = new CLInboundInterchangeProcessor(new string[] { ApplicationCodeList.Codes.CLCustoms });
			processor.ExecuteBatch();

			var processedInterchange1 = interchange1;
			var processedInterchange2 = interchange2;

			processedInterchange1.Reload();
			processedInterchange2.Reload();

			AssertEmptyMessage(processedInterchange1);
			AssertEmptyMessage(processedInterchange2);
		}

		EDIInterchange CreateInterchange(ZString interchangeType, ZString from, ZString bodyText)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.CLCustoms;
			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_From = from;
			interchange.EI_To = CLMessageConstants.EHub;
			interchange.EI_BodyText = bodyText;
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_HeaderText = @"{""custom.FileName"":""IMP-BL-1.0-0000000001.xml""}";

			Factory.Save();
			return interchange;
		}

		void AssertMessage(EDIInterchange interchange)
		{
			CombineAssertions(() =>
			{
				var message = interchange.ContainedMessages[0];
				AssertEquals(message.EM_ApplicationCode, ApplicationCodeList.Codes.CLCustoms);
				AssertEquals(message.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive);
				AssertEquals(message.EM_Status, EDIMessageStatusList.Codes.Queued);
				AssertEquals(message.EM_MessageText, interchange.EI_BodyText);
				AssertEquals(message.EM_GB, interchange.EI_GB);
				AssertEquals(message.EM_GE, GlbDepartment.CurrentDepartment.PK);
				AssertEquals(message.EM_MessageNum, interchange.EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength));
				AssertEquals(message.EM_EI, interchange.PK);
			});
		}

		void AssertEmptyMessage(EDIInterchange interchange)
		{
			CombineAssertions(() =>
			{
				AssertEquals(EDIInterchange.Status.Error, interchange.EI_Status);
				AssertEquals("NO CL CUSTOMS DATA", interchange.Logs.MostRecentLogByEventTime(Events.ErrorReport).SL_Reference);
				AssertEquals(0, interchange.ContainedMessages.Count);
			});
		}

		public static ZString GetExpectedMessageXML(ZString path)
		{
			ZString result = "";
			using (var mStream = new MemoryStream())
			{
				using (var writer = new XmlTextWriter(mStream, Encoding.Unicode))
				{
					var document = new XmlDocument();
					try
					{
						document.Load(path);
						writer.Formatting = Formatting.Indented;
						document.WriteContentTo(writer);
						writer.Flush();
						mStream.Flush();
						mStream.Position = 0;
						using (var sReader = new StreamReader(mStream))
						{
							result = sReader.ReadToEnd();
						}
					}
					catch (XmlException)
					{
						result = path;
					}
				}
			}
			return result;
		}
	}
}
