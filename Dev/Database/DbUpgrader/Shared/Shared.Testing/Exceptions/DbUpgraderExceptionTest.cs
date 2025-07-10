using System;
using System.IO;
using CargoWise.Data.Testing;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Shared.Testing.Exceptions
{
	abstract class DbUpgraderException<T> : TestCase where T : DbUpgraderException
	{
		public void TestMessage()
		{
			var dbUpgraderException = (T)Activator.CreateInstance(typeof(T), new object[] { nameof(TestMessage) });
			AssertEquals(nameof(TestMessage), dbUpgraderException.Message);
		}

		public void TestInnerException()
		{
			// Arrange
			var message = nameof(T);
			var innerExceptions = new Exception[]
				{
					SqlExceptionBuilder.CreateSqlException(1222, message),
					new InvalidOperationException(message),
					new OutOfMemoryException(message),
					new Exception(message)
				};

			foreach (var innerException in innerExceptions)
			{
				// Act
				var dbUpgraderException = (T)Activator.CreateInstance(typeof(T), new object[] { message, innerException });

				// Assert
				AssertEquals(innerException, dbUpgraderException.InnerException);
				AssertEquals(message, dbUpgraderException.Message);
			}
		}

		public void TestSerializable()
		{
			// Arrange
			var dbUpgraderException = (T)Activator.CreateInstance(typeof(T), new object[] { nameof(TestSerializable) });
			using (var memoryStream = new MemoryStream())
			{
				var jsonWriter = new JsonTextWriter(new StreamWriter(memoryStream));
				var jsonSerializer = new JsonSerializer();
				jsonSerializer.Serialize(jsonWriter, dbUpgraderException);
				jsonWriter.Flush();

				memoryStream.Position = 0;

				// Act
				var jsonReader = new JsonTextReader(new StreamReader(memoryStream));
				var deserializedException = jsonSerializer.Deserialize<T>(jsonReader);

				// Assert
				AssertNotNull(deserializedException);
				AssertEquals(dbUpgraderException.Message, deserializedException.Message);
			}
		}
	}
}
