using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using Enterprise.ZArchitecture.Environment;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Diagnostics.Testing
{
	class AlwaysOnDelayProviderTest : TestCase
	{
		public void TestGetMaximumBacklogExceptionAccessMainDbFromSecondaryReplica()
		{
			// Arrange
			var replica = $"{Guid.NewGuid()}";
			var alwaysOnDelayProviderMock = new Mock<AlwaysOnDelayProvider>(Db.DatabaseName) { CallBase = true };
			alwaysOnDelayProviderMock.Protected()
				.Setup<Tuple<bool, string, List<string>>>("GetSecondaryAsyncReplicas")
				.Returns(new Tuple<bool, string, List<string>>(true, "failureReason", new List<string> { replica }));

			using (ObjectFactory.Substitute(Mock.Of<ISystemDataRegistry>(x => x.ReportingDbServerThreshold == TimeSpan.FromMinutes(1))))
			{
				// Act
				var ex = AssertExceptionThrown<AlwaysOnDelayProviderException>(() => alwaysOnDelayProviderMock.Object.GetCurrentBacklog());

				// Assert
				var expectedMessage = $"{nameof(AlwaysOnDelayProvider)} has encountered an error to GetMaximumBacklog on replica: {replica}, database: {Db.DatabaseName}.";
				AssertNotNull(ex);
				Assert(ex.Message.StartsWith(expectedMessage));
				Assert(ex.InnerException is System.Data.Common.DbException);
			}
		}
	}
}
