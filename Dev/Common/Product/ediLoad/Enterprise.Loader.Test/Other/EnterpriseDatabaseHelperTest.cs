using System;
using CargoWise.Data;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Loader.Testing
{
	public class EnterpriseDatabaseHelperTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			enterpriseDatabaseHelperMock = new Mock<EnterpriseDatabaseHelper>(new EnterpriseConfiguration { ServerName = Db.ServerName, DatabaseName = "master" });
		}

		public void TestHandleMissingMethodExceptionIsNotCaught()
		{
			// Arrange
			var exception = new MissingMethodException("ClassName", "MethodName");
			enterpriseDatabaseHelperMock
				.Protected()
				.Setup<bool>("OpenConnectionCore", ItExpr.IsAny<bool>())
				.Throws(exception);
			enterpriseDatabaseHelperMock.CallBase = true;
			var testHelper = enterpriseDatabaseHelperMock.Object;

			// Act
			var ex = AssertExceptionThrown<MissingMemberException>(() => testHelper.OpenConnection());

			// Assert
			AssertEquals(exception, ex);
		}

		Mock<EnterpriseDatabaseHelper> enterpriseDatabaseHelperMock;
	}
}
