using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using IntegrationLogging = Enterprise.Integration;

namespace Enterprise.SqlSecurity.Test
{
	public static class Extensions
	{
		public static bool? ToNullableBool(this object value)
		{
			if (value == DBNull.Value)
			{
				return null;
			}

			return value is int i ? i == 1 : (bool?)value;
		}

		public static byte[] ToNullableBytes(this object value)
		{
			return value == DBNull.Value ? null : (byte[])value;
		}

		public static string ToStringOrNull(this object value)
		{
			return value == DBNull.Value ? null : (string)value;
		}

		public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> enumerableToAppendTo, bool onlyAppendIfTrue, params T[] valuesToAppend)
		{
			if (onlyAppendIfTrue)
			{
				return enumerableToAppendTo.Concat(valuesToAppend);
			}

			return enumerableToAppendTo;
		}

		public static Mock<IntegrationLogging.ILogger> VerifyCalled(this Mock<IntegrationLogging.ILogger> logger, IntegrationLogging.LogType level, string failMessage, Times timesCalled, params string[] messageParts)
		{
			return VerifyCalled(logger, level, null, failMessage, timesCalled, messageParts);
		}

		public static Mock<IntegrationLogging.ILogger> VerifyCalled(this Mock<IntegrationLogging.ILogger> logger, IntegrationLogging.LogType level, Type exceptionType, string failMessage, Times timesCalled, params string[] messageParts)
		{
			bool[] matchedMessageParts = null;
			var matchedCount = 0;

			var compare = new Func<string, bool>(
				(message) =>
				{
					var newMatchedMessageParts = new bool[messageParts.Length];
					var messagePartIndex = -1;
					var newMatchedCount = 0;

					var matched = messageParts
					.Aggregate(true, (result, messagePart) =>
					{
						messagePartIndex++;
						if (!message.Contains(messagePart))
						{
							return false;
						}

						newMatchedCount++;
						newMatchedMessageParts[messagePartIndex] = true;
						return result;
					});

					matchedMessageParts = matchedCount > newMatchedCount ? matchedMessageParts : newMatchedMessageParts;
					matchedCount = matchedCount > newMatchedCount ? matchedCount : newMatchedCount;

					return matched;
				});

			try
			{
				if (exceptionType != null)
				{
					logger.Verify(
						x => x.Log(
							It.Is<IntegrationLogging.LogType>(l => l == level),
							It.Is<string>((m) => compare(m)),
							It.Is<Exception>(t => t.GetType().Equals(exceptionType))),
						timesCalled,
						failMessage);
				}
				else
				{
					logger.Verify(
						x => x.Log(
							It.Is<IntegrationLogging.LogType>(l => l == level),
							It.Is<string>((m) => compare(m))),
						timesCalled,
						failMessage);
				}
			}
			catch (MockException ex)
			{
				if (matchedMessageParts?.Contains(false) ?? false)
				{
					Assert.Fail($"{ex.Message}{System.Environment.NewLine} Details: Message parts for which no match was found:{System.Environment.NewLine}{string.Join(System.Environment.NewLine, messageParts.Where((message, index) => !matchedMessageParts[index]))}");
				}
				else
				{
					throw;
				}
			}

			return logger;
		}
	}
}
