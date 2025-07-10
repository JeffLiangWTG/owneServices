using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Messaging.Business.XmlMessaging.Testing
{
	[TestedType(typeof(XmlEDIInterchange))]
	sealed class XmlEDIInterchangeTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		public void TestGetInterchangeTypeFromFileFormat()
		{
			AssertEquals(EDIInterchangeTypeList.Codes.XMS, Interchange.GetInterchangeTypeFromFileFormat(EDICommunicationsModeFileFormatList.Codes.XML));
			AssertEquals(EDIInterchangeTypeList.Codes.XMS, Interchange.GetInterchangeTypeFromFileFormat(EDICommunicationsModeFileFormatList.Codes.EXL));
			AssertEquals(EDIInterchangeTypeList.Codes.XDC, Interchange.GetInterchangeTypeFromFileFormat(EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent));
			AssertEquals(EDIInterchangeTypeList.Codes.XDC, Interchange.GetInterchangeTypeFromFileFormat(EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment));
			AssertEquals(EDIInterchangeTypeList.Codes.Unknown, Interchange.GetInterchangeTypeFromFileFormat(EDICommunicationsModeFileFormatList.Codes.FHL));
			AssertEquals(EDIInterchangeTypeList.Codes.Unknown, Interchange.GetInterchangeTypeFromFileFormat(EDICommunicationsModeFileFormatList.Codes.FWB));
			AssertEquals(EDIInterchangeTypeList.Codes.Unknown, Interchange.GetInterchangeTypeFromFileFormat(EDICommunicationsModeFileFormatList.Codes.IFS));
		}

		public void TestSetDefaultValues()
		{
			AssertEquals("EI_ApplicationCode", EDIInterchange.ApplicationCodes.XMS, Interchange.EI_ApplicationCode);
			AssertEquals("EI_Status", EDIInterchange.Status.eHubQueued, Interchange.EI_Status);
			AssertEquals("EI_TransportType", EDIInterchange.TransportType.eHub, Interchange.EI_TransportType);
			AssertDateTimeWithinOneSecond("EI_SystemCreateTimeUtc should be close to UtcNow", factoryCallTime.ToDateTime(), Interchange.EI_SystemCreateTimeUtc.ToDateTime());
			AssertEquals("SystemCreateUser should be CurrentUser Code", GlbStaff.CurrentUser.GS_Code, Interchange.EI_SystemCreateUser);
			AssertEquals("Upon creation, SystemLastEditTimeUtc should be same as SystemCreateTimeUtc", Interchange.EI_SystemLastEditTimeUtc, Interchange.EI_SystemCreateTimeUtc);
			AssertEquals("Upon creation, SystemLastEditUser should be same as SystemCreateUser", Interchange.EI_SystemCreateUser, Interchange.EI_SystemLastEditUser);
			AssertEquals("EI_SessionGUID should be same as PK", Interchange.PK, Interchange.EI_SessionGUID);
		}

		public void TestInterchangeNumberFountain_Lossy()
		{
			AssertEquals(false, Env.NumberFountains.XmlEDIInterchangeNumber.EnsureConsistentSequence);
		}

		public void TestXmlInterchangeDoesNotUseBaseNumberFountain()
		{
			T CreateNewIntercahngeWithEmptyNumber<T>()
				where T : EDIInterchange
			{
				var interchange = Factory.New<T>();
				interchange.EI_From = "XXXXXXXXX";
				interchange.EI_To = "YYYYYYYYY";
				return interchange;
			}

			var xmlInterchange1 = CreateNewIntercahngeWithEmptyNumber<XmlEDIInterchange>();
			var xmlInterchange2 = CreateNewIntercahngeWithEmptyNumber<XmlEDIInterchange>();
			var baseInterchange1 = CreateNewIntercahngeWithEmptyNumber<EDIInterchange>();
			var baseInterchange2 = CreateNewIntercahngeWithEmptyNumber<EDIInterchange>();

			Factory.Save();

			AssertEquals("Number fountain should be called once per ediInterchange", 4, NumberFountainProxy.NumberOfFountainCommands_ForTest.Value);

			void AssertEI_InterchangeNumberIsSet(EDIInterchange interchange)
			{
				if (!long.TryParse(interchange.EI_InterchangeNum, out long _))
				{
					Fail($"Interchange number should be set on save: {interchange.EI_InterchangeNum}");
				}
			}

			AssertEI_InterchangeNumberIsSet(xmlInterchange1);
			AssertEI_InterchangeNumberIsSet(xmlInterchange2);
			AssertEI_InterchangeNumberIsSet(baseInterchange1);
			AssertEI_InterchangeNumberIsSet(baseInterchange2);
		}

		XmlEDIInterchange Interchange
		{
			get { return interchange ?? (interchange = (XmlEDIInterchange)GetNewBusinessObject()); }
		}
		XmlEDIInterchange interchange;

		ZDateTime factoryCallTime;
		protected override BusinessObject GetNewBusinessObject()
		{
			factoryCallTime = ZDateTime.UtcNow;
			return Factory.New<XmlEDIInterchange>();
		}
	}
}
