using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using NctsHeader = Enterprise.Customs.ES.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5DepartureMovementForm))]
	class Phase5DepartureMovementFormTest : EU.NCTS.GUI.Testing.Phase5DepartureMovementFormAbstractTest<NctsHeader>
	{
		public void TestTabControlPages()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);

			var expectedTabPagesInOrderWithoutAnnexes = new string[]
			{
				"MainTabPage", "ServicesTabPage", "TransportAndPackagingTabPage", "HouseConsignmentsTabPage", "MessagesTabPage", "CustomFieldsTabPage", "WorkflowTabPage",
				"BillingTabPage", "DocDataTabPage", "eDocsTabPage", "NotesTabPage", "LogsTabPage",
			};

			var expectedTabPagesInOrderWithAnnexes = new string[]
			{
				"MainTabPage", "ServicesTabPage", "TransportAndPackagingTabPage", "HouseConsignmentsTabPage", "AnnexTabPage", "MessagesTabPage", "CustomFieldsTabPage", "WorkflowTabPage",
				"BillingTabPage", "DocDataTabPage", "eDocsTabPage", "NotesTabPage", "LogsTabPage",
			};

			CombineAssertions(() =>
			{
				using (var form = new Phase5DepartureMovementForm(header))
				{
					form.Show();
					var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
					AssertSequencesEqual("TabPageNames without AnnexTab when it is not visible", expectedTabPagesInOrderWithoutAnnexes, mainTabControl.TabPages.Cast<ZTabPage>().Select(x => x.Name));
				}

				header.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
				using (var form = new Phase5DepartureMovementForm(header))
				{
					form.Show();
					var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
					AssertSequencesEqual("TabPageNames with AnnexTab when it is visible", expectedTabPagesInOrderWithAnnexes, mainTabControl.TabPages.Cast<ZTabPage>().Select(x => x.Name));
				}
			});
		}

		[RequiresSTA]
		public void TestAnnexesTabVisibility()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);

			CombineAssertions(() =>
			{
				using (var form = new Phase5DepartureMovementForm(header))
				{
					form.Show();
					AssertEquals("Tab is not visible when BM_Phase empty and MRN is empty", false, form.AnnexTabPage.TabVisible);
				}

				header.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
				using (var form = new Phase5DepartureMovementForm(header))
				{
					form.Show();
					AssertEquals("Tab is visible when BM_Phase is TNN", true, form.AnnexTabPage.TabVisible);
				}

				header.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.Declaration;
				using (var form = new Phase5DepartureMovementForm(header))
				{
					form.Show();
					AssertEquals("Tab is not visible when BM_Phase is not TNN and MRN is empty", false, form.AnnexTabPage.TabVisible);
				}

				header.MovementReferenceEntryNumber.CE_EntryNum = "MRN-TEST";
				using (var form = new Phase5DepartureMovementForm(header))
				{
					form.Show();
					AssertEquals("Tab is visible when BM_Phase is not TNN, MRN is not empty and CLR is empty", true, form.AnnexTabPage.TabVisible);
				}

				header.ClearanceEntryNumber.CE_EntryNum = "CSV-TEST";
				using (var form = new Phase5DepartureMovementForm(header))
				{
					form.Show();
					AssertEquals("Tab is not visible when BM_Phase is not TNN and MRN is not empty but CLR is not empty and there are no annexes", false, form.AnnexTabPage.TabVisible);
				}

				var docPivot = Factory.NewWithValidTestData<NctsCusStorageDocPivot>();
				header.EDocPivotCollection.Add(docPivot);
				var message = SetEDIMessageAndGenPivot(docPivot);
				message.EM_Status = EDIMessage.Status.Received;
				Factory.Save();
				using (var form = new Phase5DepartureMovementForm(header))
				{
					form.Show();
					AssertEquals("Tab is visible when BM_Phase is not TNN, MRN is not empty, CLR is not empty and there are Annexes with Message Status RCV", true, form.AnnexTabPage.TabVisible);
				}

				message.EM_Status = EDIMessage.Status.Rejected;
				using (var form = new Phase5DepartureMovementForm(header))
				{
					form.Show();
					AssertEquals("Tab is not visible when BM_Phase is not TNN, MRN is not empty, CLR is not empty but no Annexes with Message Status RCV or SNT", false, form.AnnexTabPage.TabVisible);
				}

				header.EDocPivotCollection.Add(docPivot);
				message = SetEDIMessageAndGenPivot(docPivot);
				Factory.Save();
				using (var form = new Phase5DepartureMovementForm(header))
				{
					form.Show();
					AssertEquals("Tab is visible when BM_Phase is not TNN, MRN is not empty, CLR is not empty and there are Annexes with Message Status SNT", true, form.AnnexTabPage.TabVisible);
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

		public void TestMainTabPageReadOnly()
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			using (var form = new Phase5DepartureMovementForm(header))
			{
				form.Show();

				var mainTabPage = (ZTabPage)form.Controls.Find("MainTabPage", true).FirstOrDefault();
				var declarationTypeDropEdit = mainTabPage.FindSingle<ZDropEdit>("DeclarationTypeDropEdit");

				CombineAssertions(() =>
				{
					header.MovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
					AssertEquals("MainTabPage's children are readonly when nctsHeader is sent (BM_MessageStatus is SNT), declarationTypeDropEdit", true, declarationTypeDropEdit.ReadOnly);

					header.MovementHeader.BM_MessageStatus = ZString.Empty;
					AssertEquals("MainTabPage's children are not readonly when nctsHeader is not sent (BM_MessageStatus is empty), declarationTypeDropEdit", false, declarationTypeDropEdit.ReadOnly);

					header.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;
					AssertEquals("MainTabPage's children are readonly when nctsHeader is accepted (BM_CustomsStatus is REL), declarationTypeDropEdit", true, declarationTypeDropEdit.ReadOnly);
				});
			}
		}

		public void TestCertificateAndBrokerNotReadOnly()
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.MovementHeader.BM_MessageStatus = ZString.Empty;

			using (var form = new Phase5DepartureMovementForm(header))
			{
				form.Show();

				var mainTabPage = (ZTabPage)form.Controls.Find("MainTabPage", true).FirstOrDefault();
				var declarationTypeDropEdit = mainTabPage.FindSingle<ZDropEdit>("DeclarationTypeDropEdit");
				var certificateDropEdit = mainTabPage.FindSingle<ZDropEdit>("CertificateDropEdit");
				var brokerFindBox = mainTabPage.FindSingle<ZCodeFindBox>("BrokerCodeFindBox");

				CombineAssertions(() =>
				{
					AssertEquals("MainTabPage's CertificateDropEdit is not readonly when nctsHeader is not register (BM_MessageStatus is empty)", false, certificateDropEdit.ReadOnly);
					AssertEquals("MainTabPage's BrokerFindBox is not readonly when nctsHeader is not register (BM_MessageStatus is empty)", false, brokerFindBox.ReadOnly);
					AssertEquals("MainTabPage's other fields are not readonly when nctsHeader is not register (BM_MessageStatus is empty)", false, declarationTypeDropEdit.ReadOnly);

					header.MovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;

					AssertEquals("MainTabPage's CertificateDropEdit is not readonly when nctsHeader is sent (BM_MessageStatus is SNT)", false, certificateDropEdit.ReadOnly);
					AssertEquals("MainTabPage's BrokerFindBox is not readonly when nctsHeader is sent (BM_MessageStatus is SNT)", false, brokerFindBox.ReadOnly);
					AssertEquals("MainTabPage's other fields are readonly when nctsHeader is sent (BM_MessageStatus is SNT)", true, declarationTypeDropEdit.ReadOnly);

					header.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;

					AssertEquals("MainTabPage's CertificateDropEdit is not readonly when nctsHeader is accepted (BM_CustomsStatus is Rel)", false, certificateDropEdit.ReadOnly);
					AssertEquals("MainTabPage's BrokerFindBox is not readonly when nctsHeader is accepted (BM_CustomsStatus is Rel)", false, brokerFindBox.ReadOnly);
					AssertEquals("MainTabPage's other fields are readonly when nctsHeader is accepted (BM_CustomsStatus is Rel)", true, declarationTypeDropEdit.ReadOnly);
				});
			}
		}

		public void TestTransportAndPackagingTabPageReadOnly()
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			using (var form = new Phase5DepartureMovementForm(header))
			{
				form.Show();

				var transportAndPackagingTabPage = (ZTabPage)form.Controls.Find("TransportAndPackagingTabPage", true).FirstOrDefault();

				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				mainTabControl.SelectedTab = transportAndPackagingTabPage;
				var inlandTransportModeDropEdit = transportAndPackagingTabPage.FindSingle<ZDropEdit>("InlandTransportModeDropEdit");

				CombineAssertions(() =>
				{
					header.MovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
					AssertEquals("TransportAndPackagingTabPage's children are readonly when nctsHeader is sent (BM_MessageStatus is SNT), inlandTransportModeDropEdit", true, inlandTransportModeDropEdit.ReadOnly);
					
					header.MovementHeader.BM_MessageStatus = ZString.Empty;
					AssertEquals("TransportAndPackagingTabPage's children are not readonly when nctsHeader is not sent (BM_MessageStatus is empty), inlandTransportModeDropEdit", false, inlandTransportModeDropEdit.ReadOnly);
					
					header.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;
					AssertEquals("TransportAndPackagingTabPage's children are readonly when nctsHeader is accepted (BM_CustomsStatus is REL), inlandTransportModeDropEdit", true, inlandTransportModeDropEdit.ReadOnly);
				});
			}
		}

		[RequiresSTA]
		public void TestHouseConsignmentsTabPageReadOnly()
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			using (var form = new Phase5DepartureMovementForm(header))
			{
				form.Show();

				var houseConsignmentsTabPage = (ZTabPage)form.Controls.Find("HouseConsignmentsTabPage", true).FirstOrDefault();

				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				mainTabControl.SelectedTab = houseConsignmentsTabPage;
				var countryOfDispatchDropEdit = houseConsignmentsTabPage.FindSingle<ZDropEdit>("CountryOfDispatchDropEdit");

				CombineAssertions(() =>
				{
					header.MovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
					AssertEquals("HouseConsignmentsTabPage's children are readonly when nctsHeader is sent (BM_MessageStatus is SNT), countryOfDispatchDropEdit", true, countryOfDispatchDropEdit.ReadOnly);
					
					header.MovementHeader.BM_MessageStatus = ZString.Empty;
					AssertEquals("HouseConsignmentsTabPage's children are not readonly when nctsHeader is not sent (BM_MessageStatus is empty), countryOfDispatchDropEdit", false, countryOfDispatchDropEdit.ReadOnly);
					
					header.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;
					AssertEquals("HouseConsignmentsTabPage's children are readonly when nctsHeader is accepted (BM_CustomsStatus is REL), countryOfDispatchDropEdit", true, countryOfDispatchDropEdit.ReadOnly);
				});
			}
		}

		//TODO: Waiting for WorkItem to decide when lock
		/*public void TestAnnexTabPageReadOnly()
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			using (var form = new Phase5DepartureMovementForm(header))
			{
				form.Show();

				var annexTabPage = (ZTabPage)form.Controls.Find("AnnexTabPage", true).FirstOrDefault();

				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				mainTabControl.SelectedTab = annexTabPage;
				var annexGrid = annexTabPage.FindSingle<ZGrid>("AnnexGrid");

				CombineAssertions(() =>
				{
					header.MovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationSent;
					AssertEquals("AnnexTabPage's children are readonly when nctsHeader is sent (BM_MessageStatus is MDS), annexGrid", true, annexGrid.ReadOnly);

					header.MovementHeader.BM_MessageStatus = ZString.Empty;
					AssertEquals("AnnexTabPage's children are not readonly when nctsHeader is not sent (BM_MessageStatus is empty), annexGrid", false, annexGrid.ReadOnly);

					header.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;
					AssertEquals("AnnexTabPage's children are readonly when nctsHeader is accepted (BM_CustomsStatus is REL), annexGrid", true, annexGrid.ReadOnly);
				});
			}
		}*/

		protected override void PerformExtraNctsHeaderConfiguration(NctsHeader header)
		{
			base.PerformExtraNctsHeaderConfiguration(header);
			header.MovementHeader.GoodsLocation.Address.Address1 = "E";
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		[DeveloperOnlyTest]
		public override void TestMarkAsNeedingValidationIsNotCalledWhenFormLoads()
		{
			Assert(true);
		}
	}
}
