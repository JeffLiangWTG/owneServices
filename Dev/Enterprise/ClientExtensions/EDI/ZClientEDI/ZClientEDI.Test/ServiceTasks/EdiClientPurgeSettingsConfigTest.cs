using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.ServiceTasks.Tests
{
	class EdiClientPurgeSettingsConfigTest : TestCase
	{
		public void TestGetPurgeSettingsForEDISettingWithNewApplicationCode()
		{
			var expectedDuration = 2;
			var expectedUnit = TimeUnit.Week;
			using (ClientHookLoader.Instance.OverrideClientHookForTest(new TestClientOverride()))
			{
				var res = EDIMessagePurgeSettings.GetAllPurgeSettings();
				var applicationCodeObj1 = res.GetApplicationCodeObj("EEE");
				var applicationCodeObj2 = res.GetApplicationCodeObj("JJJ");

				AssertNotNull(applicationCodeObj1);
				AssertInterchange(expectedDuration, expectedUnit, applicationCodeObj1.Interchanges[0]);
				AssertMessageType("", "", "FFF", "FFF", expectedDuration, expectedUnit, applicationCodeObj1.MessageTypes[0]);
				AssertNotNull(applicationCodeObj2);
				AssertEquals(expectedDuration, applicationCodeObj2.PurgeTime);
				AssertEquals(expectedUnit, applicationCodeObj2.PurgeTimeUnit);
			}
		}

		public void TestGetPurgeSettingsForEDISettingWithExistedApplicationCodeWithoutMessageType()
		{
			var expectedDuration = 2;
			var expectedUnit = TimeUnit.Week;
			var expectedInterchangesCount = 1;
			var expectedMessageTypesCount = 0;
			using (ClientHookLoader.Instance.OverrideClientHookForTest(new TestClientOverrideForOriginConfig()))
			{
				var res = EDIMessagePurgeSettings.GetAllPurgeSettings();
				var originApplicationCodeObj = res.GetApplicationCodeObj("NDM");
				expectedMessageTypesCount = originApplicationCodeObj.MessageTypes.Count + 1;
			}
			using (ClientHookLoader.Instance.OverrideClientHookForTest(new TestClientOverride()))
			{
				var res = EDIMessagePurgeSettings.GetAllPurgeSettings();
				var applicationCodeObj = res.GetApplicationCodeObj("NDM");
				AssertNotNull(applicationCodeObj);
				AssertEquals(expectedInterchangesCount, applicationCodeObj.Interchanges.Count);
				AssertEquals(expectedMessageTypesCount, applicationCodeObj.MessageTypes.Count);
				Assert(applicationCodeObj.Interchanges.OfType<InterchangeObj>().Any(i => i.PurgeTime == expectedDuration && i.PurgeTimeUnit == expectedUnit));
				var messageTypeObj = applicationCodeObj.MessageTypes.OfType<MessageTypeObj>().FirstOrDefault(x => x.MessageSubType == "GGG");
				AssertMessageType("", "", "GGG", "GGG", expectedDuration, expectedUnit, messageTypeObj);
			}
		}

		public void TestGetPurgeSettingsForEDISettingWithExistedApplicationCodeWithMessageType()
		{
			var expectedDuration = 2;
			var expectedUnit = TimeUnit.Week;
			var expectedInterchangesCount = 1;
			var expectedMessageTypesCount = 0;
			using (ClientHookLoader.Instance.OverrideClientHookForTest(new TestClientOverrideForOriginConfig()))
			{
				var res = EDIMessagePurgeSettings.GetAllPurgeSettings();
				var originApplicationCodeObj = res.GetApplicationCodeObj("UDM");
				expectedMessageTypesCount = originApplicationCodeObj.MessageTypes.Count + 1;
			}
			using (ClientHookLoader.Instance.OverrideClientHookForTest(new TestClientOverride()))
			{
				var res = EDIMessagePurgeSettings.GetAllPurgeSettings();
				var applicationCodeObj = res.GetApplicationCodeObj("UDM");
				AssertNotNull(applicationCodeObj);
				AssertEquals(expectedInterchangesCount, applicationCodeObj.Interchanges.Count);
				AssertEquals(expectedMessageTypesCount, applicationCodeObj.MessageTypes.Count);
				Assert(applicationCodeObj.Interchanges.OfType<InterchangeObj>().Any(i => i.PurgeTime == expectedDuration && i.PurgeTimeUnit == expectedUnit));
				var messageTypeObj = applicationCodeObj.MessageTypes.OfType<MessageTypeObj>().FirstOrDefault(x => x.MessageType == "HHH");
				AssertMessageType("HHH", "HHH", "III", "III", expectedDuration, expectedUnit, messageTypeObj);
			}
		}

		static void AssertInterchange(int expectedDuration, CargoWise.Types.ZGuid expectedUnit, InterchangeObj interchangeObj)
		{
			AssertEquals(expectedDuration, interchangeObj.PurgeTime);
			AssertEquals(expectedUnit, interchangeObj.PurgeTimeUnit);
		}

		static void AssertMessageType(string messageTypeCode, string messageTypeDescription, string messageSubTypeCode, string messageSubTypeDescription, int expectedDuration, CargoWise.Types.ZGuid expectedUnit, MessageTypeObj messageType)
		{
			AssertEquals(messageTypeCode, messageType.MessageType);
			AssertEquals(messageTypeDescription, messageType.MessageTypeDescription);
			AssertEquals(messageSubTypeCode, messageType.MessageSubType);
			AssertEquals(messageSubTypeDescription, messageType.MessageSubTypeDescription);
			AssertEquals(expectedDuration, messageType.PurgeTime);
			AssertEquals(expectedUnit, messageType.PurgeTimeUnit);
		}

		class TestClientOverride : ClientHook
		{
			public override ITypeDeciderDictionary ClientTypeDeciders
			{
				get
				{
					var list = new Dictionary<Type, ITypeDecider>
					{
						{ typeof(ZClientSpecificPurgeSettingConfig), new TypeDeciderImpl(typeof(EdiClientPurgeSettingsConfigForTest)) }
					};
					return new TypeDeciderDictionary(list);
				}
			}

			public override Clients Client
			{
				get { return Clients.EDI; }
			}

			public override string ClientDisplayName
			{
				get { return "For Test"; }
			}
		}

		class EdiClientPurgeSettingsConfigForTest : ZClientSpecificPurgeSettingConfig
		{
			public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
			{
				yield return AddApplicationCodeMessageSubTypePurgeType("EEE", new InterchangeObjCollection() { NewInterchangeConfigObj(2, TimeUnit.Week) }, new MessageSubTypePurgeTypeObjCollection().Add("FFF", "FFF", 2, TimeUnit.Week));
				yield return AddApplicationCodeMessageSubTypePurgeType("NDM", new InterchangeObjCollection() { NewInterchangeConfigObj(2, TimeUnit.Week) }, new MessageSubTypePurgeTypeObjCollection().Add("GGG", "GGG", 2, TimeUnit.Week));
				yield return AddApplicationCodeMessageTypeAndSubTypePurgeType("UDM", new InterchangeObjCollection() { NewInterchangeConfigObj(2, TimeUnit.Week) }, new MessageTypeAndSubTypePurgeTypeObjCollection().Add("HHH", "HHH", "III", "III", 2, TimeUnit.Week));
				yield return AddApplicationCodePurgeType("JJJ", new InterchangeObjCollection() { NewInterchangeConfigObj(2, TimeUnit.Week) }, 2, TimeUnit.Week);
			}
		}

		class TestClientOverrideForOriginConfig : ClientHook
		{
			public override ITypeDeciderDictionary ClientTypeDeciders
			{
				get
				{
					var list = new Dictionary<Type, ITypeDecider>
					{
						{ typeof(ZClientSpecificPurgeSettingConfig), new TypeDeciderImpl(typeof(EdiClientPurgeSettingsOriginConfigForTest)) }
					};
					return new TypeDeciderDictionary(list);
				}
			}

			public override Clients Client
			{
				get { return Clients.EDI; }
			}

			public override string ClientDisplayName
			{
				get { return "For Test"; }
			}
		}

		class EdiClientPurgeSettingsOriginConfigForTest : ZClientSpecificPurgeSettingConfig
		{
			public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
			{
				return Array.Empty<ApplicationCodeObj>();
			}
		}
	}
}
