using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using static Enterprise.Customs.EU.NCTS.Business.MessageGeneration.NctsMessageFunctionSet;
using static Enterprise.Customs.FR.Business.MessageSending.FRNctsMessageFunctionSet;

namespace Enterprise.Customs.FR.Business.MessageSending.Testing
{
	class NctsMessageFunctionSetsProviderTest : EU.NCTS.Business.MessageGeneration.Testing.NctsMessageFunctionSetsProviderTest
	{
		public void TestGetMessageFunctionCore()
		{
			var provider = new NctsMessageFunctionSetsProviderForTest();
			AssertType<ArrivalNotificationMessage>(provider.GetMessageFunctionCore("007"));
			AssertType<ArrivalNotificationRejectionMessage>(provider.GetMessageFunctionCore("008"));
			AssertType<CancellationDecisionMessage>(provider.GetMessageFunctionCore("009"));
			AssertType<DeclarationAmendmentMessage>(provider.GetMessageFunctionCore("013"));
			AssertType<DeclarationCancellationRequestMessage>(provider.GetMessageFunctionCore("014"));
			AssertType<DeclarationDataMessage>(provider.GetMessageFunctionCore("015"));
			AssertType<DeclarationRejectedMessage>(provider.GetMessageFunctionCore("016"));
			AssertType<GoodsReleaseNotificationMessage>(provider.GetMessageFunctionCore("025"));
			AssertType<AcceptanceNotificationMrnAllocatedMessage>(provider.GetMessageFunctionCore("028"));
			AssertType<ReleaseOfTransitMessage>(provider.GetMessageFunctionCore("029"));
			AssertType<UnloadingPermissionMessage>(provider.GetMessageFunctionCore("043"));
			AssertType<UnloadingRemarksMessage>(provider.GetMessageFunctionCore("044"));
			AssertType<WriteOffNotificationMessage>(provider.GetMessageFunctionCore("045"));
			AssertType<NoReleaseForTransitMessage>(provider.GetMessageFunctionCore("051"));
			AssertType<GuaranteeNotValidMessage>(provider.GetMessageFunctionCore("055"));
			AssertType<UnloadingRemarksRejectionMessage>(provider.GetMessageFunctionCore("058"));
			AssertType<ControlDecisionNotificationMessage>(provider.GetMessageFunctionCore("060"));
			AssertType<InformationAboutNonArrivedMovementMessage>(provider.GetMessageFunctionCore("141"));
			AssertType<EdifactNackMessage>(provider.GetMessageFunctionCore("907"));
			AssertType<XmlNckMessage>(provider.GetMessageFunctionCore("917"));
			AssertType<PositiveAcknowledgementMessage>(provider.GetMessageFunctionCore("928"));
			AssertType<PrelodgeValidationMessage>(provider.GetMessageFunctionCore("F15"));
			AssertNull(provider.GetMessageFunctionCore("F10"));
		}
	}

	class NctsMessageFunctionSetsProviderForTest : NctsMessageFunctionSetsProvider
	{
		public new NctsMessageFunctionSet GetMessageFunctionCore(ZString messageCode) => base.GetMessageFunctionCore(messageCode);
	}
}
