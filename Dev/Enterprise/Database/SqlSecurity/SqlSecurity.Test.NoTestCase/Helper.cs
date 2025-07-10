using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.SqlSecurity.Test.NoTestCase
{
	static class NoTestCaseHelper
	{
		public static IStaffInfoProvider GetStaffInfoProviderMockFromStaffInfoCollection(IEnumerable<DbUserManager.StaffLoginInfo> staffInfoCollection)
		{
			var staffInfoProviderMock = new Mock<IStaffInfoProvider>();
			staffInfoProviderMock
				.Setup(provider => provider.GetStaffLoginsInfo())
				.Returns(staffInfoCollection);

			return staffInfoProviderMock.Object;
		}
	}
}
