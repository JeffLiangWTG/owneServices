using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.GUI.Testing;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.Customs.CH.NCTS.GUI.Testing.NCTS.MessageSending;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.CH.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[TestedType(typeof(ArrivalMessageSendingForm))]
class ArrivalMessageSendingFormTest : BaseMessageSendingFormTest<ArrivalMessageSendingFormTest.ArrivalMessageSendingFormForTesting>
{
	protected override string MovementType => NctsMovementType.Codes.Arrival;

	protected override ArrivalMessageSendingFormForTesting CreateMessageSendingForm() => new ArrivalMessageSendingFormForTesting(new NctsHeaderArrivalMessageSendingObjectParent(NctsHeader));

	public void TestSendingObjectsGridColumns()
	{
		using (var form = new ArrivalMessageSendingFormForTesting(new NctsHeaderArrivalMessageSendingObjectParent(NctsHeader)))
		{
			form.Show();
			var messageSendingObjectsGrid = form.MessageSendingObjectsGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();

			CombineAssertions(() =>
			{
				AssertEquals("Column count", 4, messageSendingObjectsGrid.Length);
				UserControlTestHelper.AssertColumnStyles(messageSendingObjectsGrid, NctsHeaderDepartureMessageSendingObject.Schema.ShouldSend, 0);
				UserControlTestHelper.AssertColumnStyles<ZTextBoxColumnStyleInfo>(messageSendingObjectsGrid, NctsHeaderDepartureMessageSendingObject.Schema.LRN, 1);
				UserControlTestHelper.AssertColumnStyles<ZTextBoxColumnStyleInfo>(messageSendingObjectsGrid, NctsHeaderDepartureMessageSendingObject.Schema.MRN, 2);
				UserControlTestHelper.AssertColumnStyles<ZDropEditColumnStyleInfo>(messageSendingObjectsGrid, NctsHeaderDepartureMessageSendingObject.Schema.MessageType, 3);
			});
		}
	}

	public void TestMultipleMrnUserControl_Visible()
	{
		(var masterHeader, _, _) = CreateHeaderWithMultipleMRN();
		using (var form = new ArrivalMessageSendingFormForTesting(new NctsHeaderArrivalMessageSendingObjectParent(masterHeader)))
		{
			form.Show();
			AssertEquals("Visibile", true, form.MultipleMrnUserControl.Visible);
		}
	}

	public void TestMultipleMrnUserControl_Hidden()
	{
		using (var form = new ArrivalMessageSendingFormForTesting(new NctsHeaderArrivalMessageSendingObjectParent(NctsHeader)))
		{
			form.Show();
			AssertEquals("Not Visibile", false, form.MultipleMrnUserControl.Visible);
		}
	}

	(NctsHeader masterHeader, NctsHeader firstHeader, NctsHeader secondHeader) CreateHeaderWithMultipleMRN()
	{
		const string lrn = "22CH123456789012N0";

		var masterHeader = CreateNctsHeader();
		masterHeader.ArrivalMovementHeader.BM_PaperlessInbondNum = lrn;
		masterHeader.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;

		var mrn1 = masterHeader.ArrivalMovementHeader.MovementReferenceNumbers.AddNew();
		mrn1.CSI_ReferenceNumber = "123456789CH";
		mrn1.CSI_Status = YesNoList.Codes.Yes;

		var mrn2 = masterHeader.ArrivalMovementHeader.MovementReferenceNumbers.AddNew();
		mrn2.CSI_ReferenceNumber = "987654321CH";
		mrn2.CSI_Status = YesNoList.Codes.Yes;

		var childHeader1 = CreateNctsHeader();
		childHeader1.ArrivalMovementHeader.MultipleMRNIndicator = false;
		childHeader1.ArrivalMovementHeader.BM_PaperlessInbondNum = lrn;
		childHeader1.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
		childHeader1.ArrivalMovementHeader.BM_NoChangesToReport = true;
		childHeader1.MovementReferenceNumberSetter(mrn1.CSI_ReferenceNumber);

		var childHeader2 = CreateNctsHeader();
		childHeader2.ArrivalMovementHeader.MultipleMRNIndicator = false;
		childHeader2.ArrivalMovementHeader.BM_PaperlessInbondNum = lrn;
		childHeader2.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
		childHeader2.ArrivalMovementHeader.BM_NoChangesToReport = true;
		childHeader2.MovementReferenceNumberSetter(mrn2.CSI_ReferenceNumber);

		masterHeader.ArrivalMovementHeader.RelatedArrivalMovements.AddPivotFor(childHeader1.ArrivalMovementHeader);
		masterHeader.ArrivalMovementHeader.RelatedArrivalMovements.AddPivotFor(childHeader2.ArrivalMovementHeader);

		return (masterHeader, childHeader1, childHeader2);
	}

	NctsHeader CreateNctsHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		return nctsHeader;
	}

	internal class ArrivalMessageSendingFormForTesting : ArrivalMessageSendingForm, IMessageSendingFormForTesting
	{
		internal ArrivalMessageSendingFormForTesting(NctsHeaderArrivalMessageSendingObjectParent sendingObjectParent) : base(sendingObjectParent)
		{
		}

		public ZButton SendButtonExposed => SendButton;

		public ZCheckBox SendWithValidationErrorsCheckBoxExposed => SendWithValidationErrorsCheckBox;

		internal new ZGrid MessageSendingObjectsGrid => base.MessageSendingObjectsGrid;
	}
}
