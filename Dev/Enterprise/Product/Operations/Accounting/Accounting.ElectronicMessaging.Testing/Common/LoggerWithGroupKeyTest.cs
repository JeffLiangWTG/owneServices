using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public class LoggerWithGroupKeyTest : TestCaseWithFactory
	{
		public void TestAddAndToStringResult()
		{
			var mockNormalLog = new Mock<INotification>();
			mockNormalLog.Setup(x => x.Message).Returns("AAAAAABBBBCCC");

			var mockNotificationWithGroupKey = new Mock<INotificationWithGroupKey>();
			mockNotificationWithGroupKey.Setup(x => x.Message).Returns("QQQQEEEETTTT");
			mockNotificationWithGroupKey.Setup(x => x.GroupKey).Returns("GroupKey");

			var logger = new LoggerWithGroupKey();
			logger.Add(mockNormalLog.Object);
			logger.Add(mockNotificationWithGroupKey.Object);

			mockNormalLog.Verify(x => x.Message, Times.Once);
			mockNormalLog.Verify(x => x.Type, Times.Never);
			mockNotificationWithGroupKey.Verify(x => x.Message, Times.Once);
			mockNotificationWithGroupKey.Verify(x => x.GroupKey, Times.Once);
			mockNotificationWithGroupKey.Verify(x => x.Type, Times.Never);

			AssertEquals("{\"\":[\"AAAAAABBBBCCC\"],\"GroupKey\":[\"QQQQEEEETTTT\"]}", logger.ToString());
		}

		public void TestEmptyToStringResult()
		{
			AssertEquals(string.Empty, new LoggerWithGroupKey().ToString());
		}

		public void TestTryDeserializeObject()
		{
			AssertSuccessCase(
				"{\"\":[\"AAAAAABBBBCCC\"],\"GroupKey\":[\"QQQQEEEETTTT\"]}",
				new Dictionary<string, string[]> {
					{ string.Empty, new string[] { "AAAAAABBBBCCC" } },
					{ "GroupKey", new string[] { "QQQQEEEETTTT" } }
				}
			);
			AssertSuccessCase(null, new Dictionary<string, string[]>());
			AssertSuccessCase(string.Empty, new Dictionary<string, string[]>());

			AssertFailCase("Input value must be dictionary formated string.",
				"{\"\":[\"AAAAAABBBBCCC\"],\"GroupKey\":\"QQQQEEEETTTT\"}"
			);
			AssertFailCase("empty value is not a valid value.", " ");

			void AssertSuccessCase(string rawValue, IDictionary<string, string[]> expectedResult)
			{
				AssertEquals(
					true,
					LoggerWithGroupKey.TryDeserializeObject(rawValue, out var sucessResult)
				);

				CombineAssertions("sucessResultWithItems", () => {
					AssertArrayEqualsByElements("Dictionary Keys", expectedResult.Keys.ToArray(), sucessResult.Keys.ToArray());

					foreach (var key in expectedResult.Keys)
					{
						AssertArrayEqualsByElements("Dictionary Values", expectedResult[key], sucessResult[key]);
					}
				});
			}

			void AssertFailCase(string comment, string rawValue)
			{
				AssertEquals(
					comment,
					false,
					LoggerWithGroupKey.TryDeserializeObject(rawValue, out var failResult)
				);
				AssertEquals("fail should should output null value", null, failResult);
			}
		}
	}
}
