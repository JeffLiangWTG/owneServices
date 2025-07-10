using System;
using System.Collections.Generic;
using Enterprise.Customs.IN.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.GUI.Testing;

[TestedType(typeof(SendMessageGridColumnLayout))]
sealed class SendMessageGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<SendMessageGridColumnLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
		(ManifestMessageSendingObject.Schema.ShouldSend, typeof(ZCheckBoxColumnStyleInfo), 100),
		(ManifestMessageSendingObject.Schema.MessageType, typeof(ZDropEditColumnStyleInfo), 200),
		(ManifestMessageSendingObject.Schema.MessageTypeDescription, typeof(ZTextBoxColumnStyleInfo), 200),
		(ManifestMessageSendingObject.Schema.BillNumber, typeof(ZTextBoxColumnStyleInfo), 200),
	};

	protected override Type GridBoundEntityType => typeof(ManifestMessageSendingObject);

	protected override SendMessageGridColumnLayout CreateGridColumnLayoutProvider()
	{
		return new SendMessageGridColumnLayout(MessageSendingObjectParent);
	}

	ManifestMessageSendingObjectParent GetManifestMessageSendingObjectParent()
	{
		var header = Factory.NewWithValidTestData<CGMAsycudaManifestHeader>();
		return new ManifestMessageSendingObjectParent(header);
	}

	ManifestMessageSendingObjectParent MessageSendingObjectParent => messageSendingObjectParent ??= GetManifestMessageSendingObjectParent();
	ManifestMessageSendingObjectParent messageSendingObjectParent;
}
