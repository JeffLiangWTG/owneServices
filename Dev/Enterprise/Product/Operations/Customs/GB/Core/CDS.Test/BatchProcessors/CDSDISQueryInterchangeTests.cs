using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CDSDISQueryInterchange))]
	public class CDSDISQueryInterchangeTests : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var interchange = Factory.New<CDSDISQueryInterchange>();
			AssertEquals(EDIMessage.ApplicationCodes.UniversalDataMessaging, interchange.EI_ApplicationCode);
		}

		protected override bool CanPersistedObjectBeDeleted => false;
	}
}
