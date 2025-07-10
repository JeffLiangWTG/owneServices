using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.H7.Business.UniversalDataTransfer;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestsSubclassesOf(typeof(H7ApplicationBusinessProvider))]
	public abstract class H7ApplicationBusinessProviderTest<T, THeader> : ASYCUDA.Business.Testing.ApplicationBusinessProviderAbstractTest<T, THeader>
		where T : H7ApplicationBusinessProvider
		where THeader : AsycudaManifestHeader
	{
		public void TestGetNewMessageSendingObjectParent()
		{
			var header = CreateNewManifest();
			var applicationBusinessProvider = header.ApplicationBusinessProvider as T;
			var messageSendingObjectParent = applicationBusinessProvider.GetNewMessageSendingObjectParent(header);

			var messageSendingObjectType = typeof(H7ApplicationBusinessProvider).GetProperty("MessageSendingObjectType", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).GetValue(applicationBusinessProvider, null) as Type;
			var expectedParentType = typeof(MessageSendingObjectParent<>).MakeGenericType(messageSendingObjectType);

			AssertType($"MessageSendingObjectParent should be of generic type MessageSendingObjectParent<{messageSendingObjectType}>", expectedParentType, messageSendingObjectParent);
		}

		public void TestInboundEDIMessageApplicationCode()
		{
			var header = CreateNewManifest();
			var applicationBusinessProvider = header.ApplicationBusinessProvider as T;
			AssertEquals(ExpectedInboundEDIMessageApplicationCode, applicationBusinessProvider.InboundEDIMessageApplicationCode);
		}

		public void TestGetNewUploadDocumentsMessageSendingObjectParent()
		{
			var header = CreateNewManifest();
			var applicationBusinessProvider = header.ApplicationBusinessProvider as T;
			AssertEquals(ExpectedUploadDocumentSendingObjectParentType, applicationBusinessProvider.GetNewUploadDocumentsMessageSendingObjectParent(header).GetType());
		}

		public void TestGetNewDocumentRequestMessageSendingObjectParent()
		{
			var header = CreateNewManifest();
			var applicationBusinessProvider = header.ApplicationBusinessProvider as T;
			AssertEquals(ExpectedDocumentRequestSendingObjectParentType, applicationBusinessProvider.GetNewDocumentRequestMessageSendingObjectParent(header).GetType());
		}

		public void TestCountryCodes()
		{
			var header = CreateNewManifest();
			var applicationBusinessProvider = header.ApplicationBusinessProvider;
			AssertContainsExactElementsInAnyOrder(ExpectedCountryCodes, applicationBusinessProvider.CountryCodes);
		}

		public void TestGetMessageProcessorDependOnTriggerAction()
		{
			var header = CreateNewManifest();
			var applicationBusinessProvider = header.ApplicationBusinessProvider as T;

			CombineAssertions(() =>
			{
				AssertEquals(ExpectedSendCustomsDeclarationMessageProcessorType, applicationBusinessProvider.GetMessageProcessorDependOnTriggerAction(WorkflowTriggerActionTypeConstants.Codes.SendCustomsDeclaration, header).GetType());
				AssertEquals(ExpectedSendG3CustomsDeclarationMessageProcessorType, applicationBusinessProvider.GetMessageProcessorDependOnTriggerAction(WorkflowTriggerActionTypeConstants.Codes.SendG3CustomsDeclaration, header)?.GetType());
			});
		}

		public void TestSupportsSendG3CustomsDeclaration()
		{
			var header = CreateNewManifest();
			var applicationBusinessProvider = header.ApplicationBusinessProvider as T;
			AssertEquals(ExpectedSupportsSendG3CustomsDeclaration, applicationBusinessProvider.SupportsSendG3CustomsDeclaration);
		}

		public void TestGetPackedItemTariffDataGrouping()
		{
			var header = CreateNewManifest();
			AssertEquals(ExpectedPackedItemTariffDataGrouping, header.ApplicationBusinessProvider.PackedItemTariffDataGrouping);
		}

		public void TestGetPackedItemTariffType()
		{
			var header = CreateNewManifest();
			AssertEquals(UniversalReferenceConstants.CusTariffTypes.ImportTariff, header.ApplicationBusinessProvider.PackedItemTariffType);
		}

		protected override IEnumerable<IManifestType> ExpectedManifestTypes => new IManifestType[] { new ManifestType(
			EUH7ManifestTypes.Codes.EH7,
			EUH7ManifestTypes.Descriptions.EH7,
			new[] { TransportModes.Air, TransportModes.Sea, TransportModes.Rail, TransportModes.Road },
			new[] { ApplicationCodeTypeList.Codes.EuH7 },
			MessageLevel.Bill,
			ShipmentTypeList.Import23Only()
		) };

		protected override Type ExpectedMessagingProviderType => typeof(MessagingProvider);
		protected override Type ExpectedFeatureProviderType => typeof(FeatureProvider);
		protected override Type ExpectedGetCustomsDeclarationDataObjectWriterType => typeof(EUH7AsycudaForCustomsDeclarationDataObjectWriter);
		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterType => typeof(EUH7AsycudaManifestHeaderDataObjectWriter);
		protected override Type ExpectedGetAsycudaManifestDataObjectReaderHelperType => typeof(EUH7AsycudaManifestDataObjectReaderHelper);
		protected override Type ExpectedAsycudaBillEventContextReaderType => typeof(AsycudaBillEventContextReader);
		protected virtual string ExpectedInboundEDIMessageApplicationCode => EDIMessage.ApplicationCodes.EUH7;
		protected virtual Type ExpectedUploadDocumentSendingObjectParentType => typeof(UploadDocumentsSendingActionParent<UploadDocumentsSendingAction>);
		protected virtual Type ExpectedDocumentRequestSendingObjectParentType => typeof(DocumentRequestSendingActionParent<DocumentRequestSendingAction>);
		protected virtual IEnumerable<string> ExpectedCountryCodes => Enumerable.Empty<string>();
		protected virtual Type ExpectedSendCustomsDeclarationMessageProcessorType => typeof(EUH7SendCustomsDeclarationMessageProcessor);
		protected virtual bool ExpectedSupportsSendG3CustomsDeclaration => false;
		protected virtual Type ExpectedSendG3CustomsDeclarationMessageProcessorType => null;
		protected virtual ZString ExpectedPackedItemTariffDataGrouping => ZString.Empty;
	}
}
