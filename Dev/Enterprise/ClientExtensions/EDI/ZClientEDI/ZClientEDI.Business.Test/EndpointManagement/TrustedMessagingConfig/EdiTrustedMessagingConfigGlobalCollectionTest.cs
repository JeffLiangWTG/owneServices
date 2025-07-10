using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.TrustedMessaging.Business.Testing
{
	[TestedType(typeof(EdiTrustedMessagingConfigGlobalCollection))]
	public class EdiTrustedMessagingConfigGlobalCollectionTest : ActiveBusinessObjectCollectionTestCase<EdiTrustedMessagingConfigGlobalCollection>
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(EdiTrustedMessagingConfigGlobalCollection);
		}

		public void TestSetDefaultsForNewElementCore()
		{
			var collection = new EdiTrustedMessagingConfigGlobalCollection(Factory);
			AssertEquals(CertificateTypeList.Codes.CentralSystemCertificate, collection.AddNew().ETM_CertificateType);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var obj = base.GetNewElementToAddToTheCollection() as EdiTrustedMessagingConfig;
			obj.ETM_CertificateType = "CSC";
			return obj;
		}
	}
}
