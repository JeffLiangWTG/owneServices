using CargoWise.EntityFramework.Testing;
using static Enterprise.Customs.EU.NCTS.Business.MessageGeneration.NctsMessageFunctionSet;

namespace Enterprise.Customs.EU.NCTS.Business.MessageGeneration.Testing
{
	public class NctsMessageFunctionSetsProviderTest : TestCaseWithFactory
	{
		public void TestGetMessageFunction()
		{
			var provider = new NctsMessageFunctionSetsProvider();
			AssertType<ArrivalNotificationMessage>(provider.GetMessageFunction("007"));
			AssertType<ArrivalNotificationRejectionMessage>(provider.GetMessageFunction("008"));
			AssertType<CancellationDecisionMessage>(provider.GetMessageFunction("009"));
			AssertType<DeclarationAmendmentMessage>(provider.GetMessageFunction("013"));
			AssertType<DeclarationCancellationRequestMessage>(provider.GetMessageFunction("014"));
			AssertType<DeclarationDataMessage>(provider.GetMessageFunction("015"));
			AssertType<DeclarationRejectedMessage>(provider.GetMessageFunction("016"));
			AssertType<GoodsReleaseNotificationMessage>(provider.GetMessageFunction("025"));
			AssertType<AcceptanceNotificationMrnAllocatedMessage>(provider.GetMessageFunction("028"));
			AssertType<ReleaseOfTransitMessage>(provider.GetMessageFunction("029"));
			AssertType<UnloadingPermissionMessage>(provider.GetMessageFunction("043"));
			AssertType<UnloadingRemarksMessage>(provider.GetMessageFunction("044"));
			AssertType<WriteOffNotificationMessage>(provider.GetMessageFunction("045"));
			AssertType<NoReleaseForTransitMessage>(provider.GetMessageFunction("051"));
			AssertType<GuaranteeNotValidMessage>(provider.GetMessageFunction("055"));
			AssertType<UnloadingRemarksRejectionMessage>(provider.GetMessageFunction("058"));
			AssertType<ControlDecisionNotificationMessage>(provider.GetMessageFunction("060"));
			AssertType<InformationAboutNonArrivedMovementMessage>(provider.GetMessageFunction("141"));
			AssertType<EdifactNackMessage>(provider.GetMessageFunction("907"));
			AssertType<XmlNckMessage>(provider.GetMessageFunction("917"));
			AssertType<PositiveAcknowledgementMessage>(provider.GetMessageFunction("928"));
			AssertNull(provider.GetMessageFunction("F15"));
		}
	}
}
