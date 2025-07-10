using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EXDOCValidationHelperTest : TestCaseWithFactory
	{
		public void TestCheckForProduceTypeIsHorticultureOrGrainsAndPlants()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.EXDOCS_Errata53_1, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
				AssertEquals(true, EXDOCCommodityCodes.IsHorticultureOrGrainsAndPlants(quarantineHeader.QH_ProduceType));
				AssertEquals(true, UniversalReferenceHelper.Errata53Enabled());

				quarantineHeader.QH_RL_NKBorderInspectionPort = "ADALV";
				quarantineHeader.Validation.ValidateQH_RL_NKBorderInspectionPort();
				AssertHasMessageErrorContaining(quarantineHeader.QH_RL_NKBorderInspectionPortInfo, "Border Inspection Port must not be present when Produce Type is Horticulture or Grains and Seeds");
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();

			var invoiceHeader = helper.Header1;
			quarantineHeader = invoiceHeader.QuarantineExDocHeader;
		}

		QuarantineExDocHeader quarantineHeader;
	}
}
