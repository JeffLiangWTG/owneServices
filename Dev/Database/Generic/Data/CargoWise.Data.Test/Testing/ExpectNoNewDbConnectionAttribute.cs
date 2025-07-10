using System;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
	public sealed class ExpectNoNewDbConnectionAttribute : TestSetupAttribute
	{
		int newConnectionBeforeTestObjectId;

		public override void SetUp(TestCase testCase)
		{
			newConnectionBeforeTestObjectId = Db.NewExtraConnectionToMainDb().ObjectID;
		}

		public override void TearDown(TestCase testCase)
		{
			var newConnections = Db.NewExtraConnectionToMainDb().ObjectID - newConnectionBeforeTestObjectId - 1;
			if (newConnections > 0)
			{
				throw new UnexpectedDbConnectionException($"Test should not create new connections, but created {newConnections} connection(s).");
			}
		}

		[Serializable]
		class UnexpectedDbConnectionException : Exception
		{
			internal UnexpectedDbConnectionException(string message) : base(message)
			{
			}

#if NETFRAMEWORK
			protected UnexpectedDbConnectionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
			{
			}
#endif
		}
	}
}
