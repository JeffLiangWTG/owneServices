using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.GUI;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Core.Constants;
using NctsHeader = Enterprise.Customs.ES.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	sealed class Phase5NctsUserControlForPluginTest : TestCaseWithFactory
	{
		public void TestArrivalUserControl()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);

			using (var control = new Phase5NctsUserControlForPluginForTest(header))
			{
				control.SetDataBinding(header, "");

				CombineAssertions(() =>
				{
					var arrivalNotification = control.FindSingleOrDefault<ZTabPage>("ArrivalNotificationTabPage");
					AssertEquals("arrivalNotification is visible", true, arrivalNotification.TabVisible);
					var arrivalNotificationUserControl = control.NctsArrivalUserControlExposed;
					AssertNotNull("ArrivalNotificationUserControl isn't null", arrivalNotificationUserControl);
					AssertType<Phase5ArrivalNotificationTabUserControl>("ArrivalNotificationUserControl type", arrivalNotificationUserControl);
				});
			}
		}

		public void TestAnnexesTabUserControl()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;

			using (var control = new Phase5NctsUserControlForPlugin(header))
			{
				CombineAssertions(() =>
				{
					AssertEquals("AnnexTabPage is visible when required", true, control.AnnexTabPage.TabVisible);
					AssertEquals("AnnexTabPage caption", "Annexes", control.AnnexTabPage.CaptionResourceString.Caption);
					var annexesTabUserControl = control.AnnexesTabUserControl;
					AssertNotNull("AnnexesTabUserControl isn't null", annexesTabUserControl);
					AssertType<AnnexesTabUserControl>("AnnexesTabUserControl type", annexesTabUserControl);
				});
			}
		}

		public void TestAnnexTabPage()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);

			CombineAssertions(() =>
			{
				using (var control = new Phase5NctsUserControlForPlugin(header))
				{
					AssertEquals("Tab is not visible when BM_Phase empty and MRN is empty", false, control.AnnexTabPage.TabVisible);
				}

				header.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
				using (var control = new Phase5NctsUserControlForPlugin(header))
				{
					AssertEquals("Tab is visible when BM_Phase is TNN", true, control.AnnexTabPage.TabVisible);
				}

				header.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.Declaration;
				using (var control = new Phase5NctsUserControlForPlugin(header))
				{
					AssertEquals("Tab is not visible when BM_Phase is not TNN and MRN is empty", false, control.AnnexTabPage.TabVisible);
				}

				header.MovementReferenceEntryNumber.CE_EntryNum = "MRN-TEST";
				using (var control = new Phase5NctsUserControlForPlugin(header))
				{
					AssertEquals("Tab is visible when BM_Phase is not TNN, MRN is not empty and CLR is empty", true, control.AnnexTabPage.TabVisible);
				}

				header.ClearanceEntryNumber.CE_EntryNum = "CSV-TEST";
				using (var control = new Phase5NctsUserControlForPlugin(header))
				{
					AssertEquals("Tab is not visible when BM_Phase is not TNN and MRN is not empty but CLR is not empty and there are no annexes", false, control.AnnexTabPage.TabVisible);
				}

				var docPivot = Factory.NewWithValidTestData<NctsCusStorageDocPivot>();
				header.EDocPivotCollection.Add(docPivot);
				var message = SetEDIMessageAndGenPivot(docPivot);
				message.EM_Status = EDIMessage.Status.Received;
				Factory.Save();
				using (var control = new Phase5NctsUserControlForPlugin(header))
				{
					AssertEquals("Tab is visible when BM_Phase is not TNN, MRN is not empty, CLR is not empty and there are Annexes with Message Status RCV", true, control.AnnexTabPage.TabVisible);
				}

				message.EM_Status = EDIMessage.Status.Rejected;
				using (var control = new Phase5NctsUserControlForPlugin(header))
				{
					AssertEquals("Tab is not visible when BM_Phase is not TNN, MRN is not empty, CLR is not empty but no Annexes with Message Status RCV or SNT", false, control.AnnexTabPage.TabVisible);
				}

				header.EDocPivotCollection.Add(docPivot);
				message = SetEDIMessageAndGenPivot(docPivot);
				Factory.Save();
				using (var control = new Phase5NctsUserControlForPlugin(header))
				{
					AssertEquals("Tab is visible when BM_Phase is not TNN, MRN is not empty, CLR is not empty and there are Annexes with Message Status SNT", true, control.AnnexTabPage.TabVisible);
				}

				var nctsHeaderArrival = Factory.New<NctsHeader>();
				nctsHeaderArrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeaderArrival.SetMovementType(NctsMovementType.Codes.Arrival);
				nctsHeaderArrival.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
				using (var control = new Phase5NctsUserControlForPlugin(header))
				{
					AssertEquals("Tab is not visible when Arrival, even when BM_Phase is TNN", true, control.AnnexTabPage.TabVisible);
				}
			});

			ESEDIMessage SetEDIMessageAndGenPivot(NctsCusStorageDocPivot docPivot)
			{
				var message = Factory.New<ESEDIMessage>();
				header.Messages.Add(message);
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message.EM_Status = EDIMessage.Status.Sent;
				var messagePivot = Factory.New<GenPivot>();
				messagePivot.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
				messagePivot.XX_Relation1ID = docPivot.PK;
				messagePivot.XX_Relation1TableCode = docPivot.TablePrefix;
				messagePivot.XX_Relation2ID = message.PK;
				messagePivot.XX_Relation2TableCode = message.TablePrefix;

				return message;
			}
		}

		sealed class Phase5NctsUserControlForPluginForTest : Phase5NctsUserControlForPlugin
		{
			public Phase5NctsUserControlForPluginForTest(NctsHeader nctsMovement) : base(nctsMovement)
			{
			}

			public ZUserControl NctsArrivalUserControlExposed => NctsArrivalUserControl;
		}
	}
}
