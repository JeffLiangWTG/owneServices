using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	public class EMCSCusContainerValidationTest : EU.EMCS.Business.Testing.EMCSCusContainerValidationTest
	{
		public override void TestCheckCO_ContainerNumber_Mandatory()
		{
			CombineAssertions(() =>
			{
				const string errorMsg = "You have not entered an Identity.";
				container.ZG_UnitCode = EU.EMCS.Business.EMCSTransportUnitCodeList.Codes.Container;
				declaration.ZG_DeferredSubmission = ZString.Empty;
				container.CO_ContainerNumber = ZString.Empty;
				AssertHasMessageError("DeferredSubmission not set", container.CO_ContainerNumberInfo, errorMsg);

				declaration.ZG_DeferredSubmission = EmcsDeferredSubmissionList.Codes.Nein;
				container.Validation.ValidateCO_ContainerNumber();
				AssertHasMessageError("DeferredSubmission is 'Nein'", container.CO_ContainerNumberInfo, errorMsg);

				declaration.SetConsolidatedDocument();
				container.Validation.ValidateCO_ContainerNumber();
				AssertNoMessageError("DeferredSubmission is 'JaZusammengefasstesEVd'", container.CO_ContainerNumberInfo, errorMsg);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = (EMCSJobDeclaration)container.Declaration;
		}
		EMCSJobDeclaration declaration;
	}
}
