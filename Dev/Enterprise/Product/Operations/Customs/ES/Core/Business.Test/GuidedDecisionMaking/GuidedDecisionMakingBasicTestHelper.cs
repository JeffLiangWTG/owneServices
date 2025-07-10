using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.ES.Business.Testing
{
	public static class GuidedDecisionMakingBasicTestHelper
	{
		public static GuidedDecisionMakingBasic CreateGuidedDecisionMakingBasicForTest(BusinessObjectFactory factory, bool isDestinationCanaryIsland = false)
		{
			var gDMBasicSource = new Mock<IESGuidedDecisionMakingSource>();
			gDMBasicSource.Setup(x => x.DataGrouping).Returns("DG1");
			gDMBasicSource.Setup(x => x.ParentDataGrouping).Returns("PDG");
			gDMBasicSource.Setup(x => x.UserLanguage).Returns("EN");
			gDMBasicSource.Setup(x => x.DutyRateTypeCode).Returns("OTH");
			gDMBasicSource.Setup(x => x.AllowQuickAdditionalCodeScreen).Returns(true);
			gDMBasicSource.Setup(x => x.AllowQuickConditionScreen).Returns(true);
			gDMBasicSource.Setup(x => x.EffectiveDate).Returns(ZDate.BrettsBirthday);
			gDMBasicSource.Setup(x => x.TariffCode).Returns("1111111111");
			gDMBasicSource.Setup(x => x.CountryCode).Returns("FR");
			gDMBasicSource.Setup(x => x.CountryOfOrigin).Returns("FR");
			gDMBasicSource.Setup(x => x.Preference).Returns("P1");
			gDMBasicSource.Setup(x => x.QuotaOrderNumber).Returns("Number1");
			gDMBasicSource.Setup(x => x.CustomsFirstQuantity).Returns(100m);
			gDMBasicSource.Setup(x => x.CustomsSecondQuantity).Returns(200m);
			gDMBasicSource.Setup(x => x.CustomsSecondUnitQty).Returns("LPA");
			gDMBasicSource.Setup(x => x.CustomsThirdQuantity).Returns(300m);
			gDMBasicSource.Setup(x => x.CustomsThirdUnitQty).Returns("HLT");
			gDMBasicSource.Setup(x => x.TariffType).Returns("DEF");
			gDMBasicSource.Setup(x => x.IsImport).Returns(true);
			gDMBasicSource.Setup(x => x.IsExport).Returns(false);
			gDMBasicSource.Setup(x => x.SupplementaryCodes).Returns(new List<ZString>() { "ADD1" });
			gDMBasicSource.Setup(x => x.SupportingAndAdditionalDocuments).Returns(Enumerable.Empty<(ZString Code, ZString Reference, ZDateTime DateOfIssue)>());
			gDMBasicSource.Setup(x => x.DestinationStateIsCanaryIsland).Returns(isDestinationCanaryIsland);

			return new GuidedDecisionMakingBasic(gDMBasicSource.Object, factory);
		}
	}
}
