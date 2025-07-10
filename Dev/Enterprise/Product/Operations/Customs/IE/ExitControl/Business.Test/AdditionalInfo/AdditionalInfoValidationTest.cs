using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	sealed class AdditionalInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_ReferenceNumber()
		{
			additionalInfo.CSI_ReferenceNumber = ZString.Empty;
			AssertHasMessageErrorContaining(additionalInfo.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		protected override void SetUp()
		{
			base.SetUp();
			additionalInfo = AdditionalInfoTest.GetNewBusinessObject(Factory);
		}

		AdditionalInfo additionalInfo;
	}
}
