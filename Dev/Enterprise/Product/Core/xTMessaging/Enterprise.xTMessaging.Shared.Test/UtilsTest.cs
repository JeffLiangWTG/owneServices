using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.xTMessaging.Business;
using Grpc.Core;

namespace Enterprise.xTMessaging.Shared.Test
{
	public class UtilsTest : TestCaseWithFactory
	{
		public void TestConnectionException_DeadlineExceeded()
		{
			AssertConnectionException(StatusCode.DeadlineExceeded, Utils.DeadlineExceededErrorReportKey, "TestConnectionException 120 seconds timeout, please try it again.");
			AssertConnectionException(StatusCode.Unavailable, Utils.RpcErrorReportKey, "Failed to connect to xT, please check with the data in Registry and reference database.");

			void AssertConnectionException(StatusCode statusCode, string errorKey, string errorMessage)
			{
				var status = new Status(statusCode, statusCode.ToString());
				var rpcException = new RpcException(status);
				var messageTimeout = TimeSpan.FromSeconds(DirectxTMessagingRegistry.Instance.XTServerMessageTimeoutInSeconds.Value);
				var connectionException = Utils.GetMsgServerConnectionException("TestConnectionException", rpcException, messageTimeout);
				CombineAssertions(() =>
				{
					AssertEquals("Exception.Type", connectionException.GetType(), typeof(MsgServerConnectionException));
					AssertEquals("Exception.Message", errorKey, connectionException.Message);
					AssertEquals("Exception.ErrorDetail", errorMessage, connectionException.ErrorDetail);
				});
			}
		}

		public void TestAddToDictionaryIfValid()
		{
			var dict = new Dictionary<string, string>();

			Utils.AddToDictionaryIfValid(dict, "Test", "");
			AssertEquals(0, dict.Count);

			Utils.AddToDictionaryIfValid(dict, "", "testvalue");
			AssertEquals(0, dict.Count);

			Utils.AddToDictionaryIfValid(dict, "Test", "testvalue");
			AssertEquals(1, dict.Count);
			AssertEquals("testvalue", dict["Test"]);

			Utils.AddToDictionaryIfValid(dict, "Test", "testvalue2");
			AssertEquals(1, dict.Count);
			AssertEquals("testvalue", dict["Test"]);

			Utils.AddToDictionaryIfValid(dict, "Test", "testvalue3", true);
			AssertEquals(1, dict.Count);
			AssertEquals("testvalue3", dict["Test"]);

			Utils.AddToDictionaryIfValid(dict, "Test2", "testvalue4", true);
			AssertEquals(2, dict.Count);
			AssertEquals("testvalue3", dict["Test"]);
			AssertEquals("testvalue4", dict["Test2"]);
		}

		public void TestAddRangeToDictionaryIfValid()
		{
			var testDict = new Dictionary<string, string>();
			var testInput = new Dictionary<string, string>()
			{
				{ "EmptyValue", "" },
				{ "", "emptyKey" },
				{ "Test", "testvalue1" },
			};

			testDict.AddRangeToDictionaryIfValid(testInput);
			AssertContainsExactElementsInAnyOrder("Case - NonOverride", new string[] {
				"Test - testvalue1"
			}, testDict.Select(kvp => $"{kvp.Key} - {kvp.Value}"));

			testInput = new Dictionary<string, string>()
			{
				{ "Test", "testvalue2" },
			};
			testDict.AddRangeToDictionaryIfValid(testInput);
			AssertContainsExactElementsInAnyOrder("Case - NonOverride", new string[] {
				"Test - testvalue1"
			}, testDict.Select(kvp => $"{kvp.Key} - {kvp.Value}"));

			testDict.AddRangeToDictionaryIfValid(null);
			AssertContainsExactElementsInAnyOrder("Case - Null", new string[] {
				"Test - testvalue1"
			}, testDict.Select(kvp => $"{kvp.Key} - {kvp.Value}"));

			testDict.AddRangeToDictionaryIfValid(testInput, true);
			AssertContainsExactElementsInAnyOrder("Case - Override", new string[] {
				"Test - testvalue2"
			}, testDict.Select(kvp => $"{kvp.Key} - {kvp.Value}"));

			testInput = new Dictionary<string, string>()
			{
				{ Constants.CustomMsgAttributes.ApplicationCode, "testvalue2" },
			};
			testDict.AddRangeToDictionaryIfValid(testInput);
			AssertEquals("Attribute should not have been added as it is system level", 1, testDict.Count);
		}

		public void TestIsSystemMessageAttribute()
		{
			var constFields = typeof(Constants.CustomMsgAttributes).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(fi => fi.IsLiteral && !fi.IsInitOnly);
			var fieldsToBeIgnoredForSystemMessageAttributeCheck = new HashSet<string> { "CreateEDIMessage", "EDIMessageCreatorID", "MessageSubType", "ReceivingCount", "ReferenceNumber" };

			CombineAssertions(() =>
			{
				foreach (var constField in constFields.Where(f => !fieldsToBeIgnoredForSystemMessageAttributeCheck.Contains(f.Name)))
				{
					var constValue = constField.GetValue(null) as string;
					AssertEquals($"{constValue} should be a SystemMessageAttribute", true, constValue.IsSystemMessageAttribute());
				}
			});
		}

		public void TestConvertUnsignedLongToLong()
		{
			AssertEquals(123456789L, Utils.ConvertUnsignedLongToLong(123456789UL));
			AssertEquals(0L, Utils.ConvertUnsignedLongToLong(ulong.MaxValue));
			AssertEquals(long.MaxValue, Utils.ConvertUnsignedLongToLong(long.MaxValue));
		}
	}
}
