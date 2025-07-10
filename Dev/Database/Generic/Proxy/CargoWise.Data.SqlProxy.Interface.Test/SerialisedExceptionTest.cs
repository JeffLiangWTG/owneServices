using System.Data.SqlClient;
using CargoWise.Data.SqlProxy.Interface.Models;
using Newtonsoft.Json;

namespace CargoWise.Data.SqlProxy.Interface.Test;

class SerialisedExceptionTest
{
	[TestCaseSource(nameof(ExceptionTestData))]
	public void SerializeException(Exception ex)
	{
		var serialisedException = SerialisedException.FromException(ex);
		var jsonString = JsonConvert.SerializeObject(serialisedException);
		var deserializeObject = JsonConvert.DeserializeObject<SerialisedException>(jsonString);

		if (deserializeObject == null)
		{
			Assert.Fail($"Failed to deserialize the exception {ex}.");
			return;
		}

		Assert.That(deserializeObject, Is.Not.Null);
		Assert.That(deserializeObject.ExceptionType, Is.EqualTo(serialisedException!.ExceptionType));
		Assert.That(deserializeObject.Message, Is.EqualTo(serialisedException!.Message));

		if (ex is SqlException sqlException)
		{
			Assert.That(deserializeObject.IsSqlException, Is.True);
			Assert.That(deserializeObject.Number, Is.EqualTo(sqlException.Number));

			var deserializedException = deserializeObject.ToException();
			Assert.That(deserializedException, Is.Not.Null);
			Assert.That(deserializedException, Is.InstanceOf<SqlException>());

			if (deserializedException is SqlException deserializedSqlException)
			{
				Assert.That(deserializedSqlException.Number, Is.EqualTo(sqlException.Number));
				Assert.That(deserializedSqlException.Message, Is.EqualTo(sqlException.Message));
			}
		}
		else if (ex is AggregateException aggregateException)
		{
			Assert.That(deserializeObject.IsAggregate, Is.True);
			if (aggregateException.InnerExceptions?.Count > 0)
			{
				Assert.That(deserializeObject.InnerExceptions.Count, Is.EqualTo(aggregateException.InnerExceptions.Count));
				Assert.That(deserializeObject.InnerExceptions[0]!.Message, Is.EqualTo(aggregateException.InnerExceptions[0].Message));
				Assert.That(deserializeObject.InnerExceptions[0]!.ExceptionType, Is.EqualTo(aggregateException.InnerExceptions[0].GetType().AssemblyQualifiedName));
			}
		}
		else
		{
			var deserializedException = deserializeObject.ToException();
			Assert.That(deserializedException, Is.Not.Null);
			Assert.That(deserializedException!.GetType(), Is.EqualTo(ex.GetType()));
			Assert.That(deserializedException.Message, Is.EqualTo(ex.Message));
		}
	}

	[Test]
	public void SerializeSqlException()
	{
		var sqlException = SqlExceptionBuilder.CreateSqlException(1222, "Lock request time out period exceeded.");
		SerializeException(sqlException!);
	}

	[Test]
	public void SerializeArgumentNullException()
	{
		var ane = new ArgumentNullException("paramName", "Test Exception");
		SerializeException(ane);
	}

	[TestCaseSource(nameof(ExceptionTestData))]
	public void SerializeAggregateException(Exception innerException)
	{
		var aggregateException = new AggregateException("Test Exception", innerException);
		SerializeException(aggregateException);
	}

	static IEnumerable<Exception> ExceptionTestData()
	{
		yield return new DatabaseUpgradedException();
		yield return new DatabaseUpgradeInProgressException();
		yield return new OutOfMemoryException("1");
		yield return new AccessViolationException("2");
		yield return new IOException("3");
		yield return new AccessViolationException("4");
		yield return new FileNotFoundException("5");
		yield return new Exception("6");
		yield return new InvalidOperationException("7");
		yield return new ArgumentException("8");
	}
}
