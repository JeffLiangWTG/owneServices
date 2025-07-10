using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.EMCSPhase4_1;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.IE.EMCS.Business.Testing.Phase4_1
{
	public class EMCSMessageBuilderLoaderTest : TestCaseWithFactory
	{
		public void TestCancellationOfEAD()
		{
			var emcsDataProvider = new Mock<IEMCSMessageHeader>();
			AssertType<IE810MessageBuilder>(EMCSMessageBuilderLoader.Instance.GetMessageBuilder(EMCSMessageBuilderLoader.CancellationOfEAD, emcsDataProvider.Object));
		}

		public void TestSubmitDraftEAD()
		{
			var emcsDataProvider = new Mock<IEMCSMessageHeader>();
			AssertType<IE815MessageBuilder>(EMCSMessageBuilderLoader.Instance.GetMessageBuilder(EMCSMessageBuilderLoader.SubmitDraftEAD, emcsDataProvider.Object));
		}

		public void TestReportOfReceipt()
		{
			var emcsDataProvider = new Mock<IEMCSMessageHeader>();
			AssertType<IE818MessageBuilder>(EMCSMessageBuilderLoader.Instance.GetMessageBuilder(EMCSMessageBuilderLoader.ReportOfReceipt, emcsDataProvider.Object));
		}

		public void TestChangeOfDestination()
		{
			var emcsDataProvider = new Mock<IEMCSMessageHeader>();
			AssertType<IE813MessageBuilder>(EMCSMessageBuilderLoader.Instance.GetMessageBuilder(EMCSMessageBuilderLoader.ChangeOfDestination, emcsDataProvider.Object));
		}

		public void TestExplanationOnDelayForDelivery()
		{
			var emcsDataProvider = new Mock<IEMCSMessageHeader>();
			AssertType<IE837MessageBuilder>(EMCSMessageBuilderLoader.Instance.GetMessageBuilder(EMCSMessageBuilderLoader.ExplanationOnDelayForDelivery, emcsDataProvider.Object));
		}

		public void TestExplanationOnReasonForShortage()
		{
			var emcsDataProvider = new Mock<IEMCSMessageHeader>();
			AssertType<IE871MessageBuilder>(EMCSMessageBuilderLoader.Instance.GetMessageBuilder(EMCSMessageBuilderLoader.ExplanationOnReasonForShortage, emcsDataProvider.Object));
		}

		public void TestRejectionOfEAD()
		{
			var emcsDataProvider = new Mock<IEMCSMessageHeader>();
			AssertType<IE819MessageBuilder>(EMCSMessageBuilderLoader.Instance.GetMessageBuilder(EMCSMessageBuilderLoader.RejectionOfEAD, emcsDataProvider.Object));
		}

		protected override void SetUp()
		{
			base.SetUp();
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.EMCS_Phase4_1, Core.Constants.CountryCodes.Ireland, ZDateTime.Today, true);
			var systemCode = GlbCompany.CurrentCompany.LicenceEnterpriseCode + GlbCompany.CurrentCompany.LicenceServerID;
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionalityAttribute(Constants.FunctionalityTypes.EMCS_Phase4_1,
				Core.Constants.CountryCodes.Ireland,
				ZDateTime.Today,
				Core.Constants.Customs.Universal.RefCusCodeList.Attributes.System,
				systemCode);
		}

		protected override void TearDown()
		{
			base.TearDown();
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.EMCS_Phase4_1, Core.Constants.CountryCodes.Ireland, ZDateTime.Today, false);
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionalityAttribute(Constants.FunctionalityTypes.EMCS_Phase4_1,
				Core.Constants.CountryCodes.Ireland,
				ZDateTime.Today,
				Core.Constants.Customs.Universal.RefCusCodeList.Attributes.System,
				string.Empty);
		}
	}
}
