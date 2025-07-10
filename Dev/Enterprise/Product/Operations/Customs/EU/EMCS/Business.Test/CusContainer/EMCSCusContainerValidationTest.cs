using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	public class EMCSCusContainerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCO_ContainerNumber_ConformToISOStandard()
		{
			const string containerNumberDoesNotConformToISOStandard = "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.";

			container.ZG_UnitCode = EMCSTransportUnitCodeList.Codes.Container;
			container.CO_ContainerNumber = "12345";
			AssertHasWarning(container.CO_ContainerNumberInfo, containerNumberDoesNotConformToISOStandard);
			container.CO_ContainerNumber = "FAKE4100011";
			AssertNoWarning(container.CO_ContainerNumberInfo, containerNumberDoesNotConformToISOStandard);
		}

		public virtual void TestCheckCO_ContainerNumber_Mandatory()
		{
			CombineAssertions(() =>
			{
				container.ZG_UnitCode = EMCSTransportUnitCodeList.Codes.Container;
				container.CO_ContainerNumber = "12345";
				AssertNoMessageErrorContaining("Entered Identity", container.CO_ContainerNumberInfo, MandatoryValidation.YouHaveNotEntered);
				container.CO_ContainerNumber = ZString.Empty;
				AssertHasMessageErrorContaining("Empty Identity", container.CO_ContainerNumberInfo, MandatoryValidation.YouHaveNotEntered);
				container.ZG_UnitCode = EMCSTransportUnitCodeList.Codes.FixedTransportInstallations;
				container.CO_ContainerNumber = ZString.Empty;
				AssertNoMessageErrorContaining("Type is Fixed Transport", container.CO_ContainerNumberInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCO_ContainerNumber_ContainerIsUnique()
		{
			const string identityMustBeUnique = "Identity must be unique.";

			container.CO_ContainerNumber = "001";
			var container2 = container.Declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "001";
			container.Validation.ValidateAll();
			AssertHasMessageError(container.CO_ContainerNumberInfo, identityMustBeUnique);
			AssertHasMessageError(container2.CO_ContainerNumberInfo, identityMustBeUnique);

			container2.CO_ContainerNumber = "002";
			container.Validation.ValidateAll();
			AssertNoMessageError(container.CO_ContainerNumberInfo, identityMustBeUnique);
			AssertNoMessageError(container2.CO_ContainerNumberInfo, identityMustBeUnique);
		}

		public void TestCheckSealDetails_AllCharactersAllowed()
		{
			container.SealDetails = "à _!#$%&()*,-./0;<=>@[]^`{|}~+:?'Unicodetest-ÂÜ§$㐿㪳çËŠïÔчШĢøÅşŢǁǂ№™€";
			AssertNoErrors(container.SealDetailsInfo);
		}

		public void TestCheckComment_AllCharactersAllowed()
		{
			container.Comment = "abc123%!Unicodetest-ÂÜ§$㐿㪳çËŠïÔчШĢøÅşŢǁǂ№™€";
			AssertNoErrors(container.CommentInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<EMCSJobDeclaration>();
			container = declaration.CusContainers.AddNew();
		}

		protected EMCSCusContainer container;
	}
}
