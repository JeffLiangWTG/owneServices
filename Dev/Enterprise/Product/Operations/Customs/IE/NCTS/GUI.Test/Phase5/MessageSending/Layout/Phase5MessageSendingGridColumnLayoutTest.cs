using System;
using System.Collections.Generic;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using Moq.Protected;
using MessageSendingConfiguration = Enterprise.Customs.EU.NCTS.Business.MessageSendingConfiguration;
using NctsHeader = Enterprise.Customs.IE.NCTS.Business.NctsHeader;
using NctsHeaderMessageSendingObject = Enterprise.Customs.IE.NCTS.Business.NctsHeaderMessageSendingObject;
using NctsHeaderMessageSendingObjectParent = Enterprise.Customs.IE.NCTS.Business.NctsHeaderMessageSendingObjectParent;

namespace Enterprise.Customs.IE.NCTS.GUI.Testing
{
	sealed class Phase5MessageSendingGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<Phase5MessageSendingGridColumnLayout>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new Phase5MessageSendingGridColumnLayout(null));
		}

		public void TestReleaseRequestColumn_NotAddedIfConfigurationIsNotApplicable()
		{
			nctsConfiguration?.Dispose();
			nctsConfiguration = null;
			var mock = new Mock<MessageSendingConfiguration>();
			mock.Setup(m => m.MessageTypeList(It.IsAny<NctsHeader>())).Returns(new CodeDescriptionPairList());
			mock.Setup(m => m.ReleaseRequestCode).Returns("054");

			using (NctsConfigurationTestHelper.TemporarilySetMessageSendingConfiguration(Factory, mock.Object))
			{
				var layout = ((IGridColumnLayoutProvider)CreateGridColumnLayoutProvider()).Layout;
				CombineAssertions("When Mock Configuration do not allow column display", () =>
				{
					AssertNotNull("Layout", layout);
					AssertEquals("ReleaseRequest Column", false, layout.HasColumn("ReleaseRequest"));
				});
			}
		}

		public void TestJustificationColumn_NotAddedIfConfigurationIsNotApplicable()
		{
			nctsConfiguration?.Dispose();
			nctsConfiguration = null;
			var mock = new Mock<MessageSendingConfiguration>();
			mock.Setup(m => m.MessageTypeList(It.IsAny<NctsHeader>())).Returns(new CodeDescriptionPairList());
			mock.Setup(m => m.ReleaseRequestCode).Returns("054");

			using (NctsConfigurationTestHelper.TemporarilySetMessageSendingConfiguration(Factory, mock.Object))
			{
				var layout = ((IGridColumnLayoutProvider)CreateGridColumnLayoutProvider()).Layout;
				CombineAssertions("When Mock Configuration do not allow column display", () =>
				{
					AssertNotNull("Layout", layout);
					AssertEquals("Justification Column", false, layout.HasColumn("Justification"));
				});
			}
		}

		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
		{
			(NctsHeaderMessageSendingObject.Schema.ShouldSend, typeof(ZCheckBoxColumnStyleInfo), 40),
			(NctsHeaderMessageSendingObject.Schema.MessageType, typeof(ZDropEditColumnStyleInfo), 100),
			(NctsHeaderMessageSendingObject.Schema.LRN, typeof(ZTextBoxColumnStyleInfo), 160),
			(NctsHeaderMessageSendingObject.Schema.MRN, typeof(ZTextBoxColumnStyleInfo), 160),
			(NctsHeaderMessageSendingObject.Schema.ReleaseRequest, typeof(ZDropEditColumnStyleInfo), 100),
			(NctsHeaderMessageSendingObject.Schema.Justification, typeof(ZTextBoxColumnStyleInfo), 100),
			(NctsHeaderMessageSendingObject.Schema.MessageStatus, typeof(ZTextBoxColumnStyleInfo), 60),
			(NctsHeaderMessageSendingObject.Schema.DestinationCustomsOfficeCode, typeof(ZCodeFindBoxColumnStyleInfo), 100),
			(NctsHeaderMessageSendingObject.Schema.Consignee, typeof(ZCodeFindBoxColumnStyleInfo), 100),
			(NctsHeaderMessageSendingObject.Schema.TC11DeliveryDate, typeof(ZDateEditColumnStyleInfo), 60),
			(NctsHeaderMessageSendingObject.Schema.EnquiryText, typeof(ZTextBoxColumnStyleInfo), 160),
		};

		protected override Type GridBoundEntityType => typeof(NctsHeaderMessageSendingObject);

		protected override Phase5MessageSendingGridColumnLayout CreateGridColumnLayoutProvider()
		{
			var messageObjectSendingParent = new NctsHeaderMessageSendingObjectParent(header);
			return new Phase5MessageSendingGridColumnLayout(messageObjectSendingParent);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.BH_HeaderType = NctsMovementType.Codes.Departure;
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var messaeTypeList = new CodeDescriptionPairList();
			messaeTypeList.AddPairIfNotExist("014", "014");
			messaeTypeList.AddPairIfNotExist("054", "054");

			var messageSendingConfiguration = new Mock<MessageSendingConfiguration>();
			messageSendingConfiguration.Protected().Setup<bool>("ShowJustificationCore", ItExpr.IsAny<NctsHeader>()).Returns(true);
			messageSendingConfiguration.Setup(m => m.MessageTypeList(It.IsAny<NctsHeader>())).Returns(messaeTypeList);
			messageSendingConfiguration.Setup(m => m.ReleaseRequestCode).Returns("054");
			nctsConfiguration = NctsConfigurationTestHelper.TemporarilySetMessageSendingConfiguration(Factory, messageSendingConfiguration.Object);
		}

		protected override void TearDown()
		{
			base.TearDown();
			nctsConfiguration?.Dispose();
		}

		NctsHeader header;
		IDisposable nctsConfiguration;
	}
}
