using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Tasks.FTP.Testing
{
	[TestedType(typeof(FtpProfileCollection))]
	sealed class FtpProfileCollectionBOTest : NonPersistentBusinessObjectCollectionTestCase<FtpProfileCollection>
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(FtpProfileCollection);
		}

		protected override FtpProfileCollection GetCollectionToTest()
		{
			return new FtpProfileCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new FtpProfile(Factory);
		}
	}
}
