using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Common.Testing;
using Enterprise.Customs.ManifestBase;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	sealed class AsycudaManifestHeaderTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderAbstractTest
	{
		public void TestAMA_ManifestType()
		{
			CombineAssertions(() =>
			{
				TestAMA_ManifestType_ReadOnly();
				TestSetAMA_ManifestTypeDefaultNatureAndTransportMode();
			});
		}

		void TestAMA_ManifestType_ReadOnly()
		{
			var consol = Factory.New<ForwardingConsol>();
			ManifestHeader.SetParent(consol);

			Assert(ManifestHeader.AMA_ManifestTypeInfo.ReadOnly);

			ManifestHeader.AMA_OverrideFreightDefaults = true;
			Assert(ManifestHeader.AMA_ManifestTypeInfo.ReadOnly);
		}

		void TestSetAMA_ManifestTypeDefaultNatureAndTransportMode()
		{
			ManifestHeader.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			AssertEquals(JPJobMessageTypeList.Codes.Export, ManifestHeader.AMA_Nature);
			AssertEquals(TransportTypeList.Codes.Air, ManifestHeader.AMA_TransportMode);

			ManifestHeader.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			AssertEquals(JPJobMessageTypeList.Codes.Import, ManifestHeader.AMA_Nature);
			AssertEquals(TransportTypeList.Codes.Air, ManifestHeader.AMA_TransportMode);

			ManifestHeader.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			AssertEquals(JPJobMessageTypeList.Codes.Import, ManifestHeader.AMA_Nature);
			AssertEquals(TransportTypeList.Codes.Sea, ManifestHeader.AMA_TransportMode);

			ManifestHeader.AMA_ManifestType = JPManifestTypeCodeList.Codes.VAN;
			AssertEquals(JPJobMessageTypeList.Codes.Export, ManifestHeader.AMA_Nature);
			AssertEquals(TransportTypeList.Codes.Sea, ManifestHeader.AMA_TransportMode);
		}

		public void TestAMA_NatureReadOnly()
		{
			Assert(ManifestHeader.AMA_NatureInfo.ReadOnly);
		}

		public void TestAMA_TransportModeReadOnly()
		{
			Assert(ManifestHeader.AMA_TransportModeInfo.ReadOnly);
		}

		public void TestErrorMessageProcessingStrategyParent()
		{
			IErrorMessageProcessingStrategyParent header = Factory.New<AsycudaManifestHeader>();
			AssertType<ErrorMessageProcessingStrategy>(header.ProcessingStrategy);
		}

		public void TestInputReference()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_InputReference = string.Empty;

			Assert(header.AMA_InputReferenceInfo.ReadOnly);

			var provider = (IInputReferenceProvider)header;
			AssertEquals(string.Empty, provider.InputReference);

			Factory.Save();

			Assert("Should populate a new value in AMA_InputReference after the header is saved.", !header.AMA_InputReference.IsEmpty);
			AssertEquals(header.AMA_InputReference, provider.InputReference);

			var query = new ZQuery()
				.AddToFilter(CusEntryNumSchema.CE_ParentID, header.PK)
				.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.JP.InputReference)
				.AddToFilter(CusEntryNumSchema.CE_EntryIsSystemGenerated, false);

			var entryNum = Factory.Load<CusEntryNumber>(query).Single();
			AssertEquals(header.AMA_InputReference, entryNum.CE_EntryNum);
		}

		protected override Type ExpectedTypeOfContainer => typeof(AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>);

		public void TestDefaultCustomsAgentAndCredentialWhenSetAMA_TransportMode()
		{
			var (glbExternalPasswordPK1, glbExternalPasswordPK2, _) = new DefaultBrokerAndCredentialTestHelper().CreateBrokerStaffAndReturnPK();

			var item = new DefaultBrokerAndCredential(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);
			item.DefaultBrokerCode = "AN";
			item.ForwarderManifestAIR = "Test2002";
			item.ForwarderManifestSEA = "Test1001";

			using (JPRegistry.Instance.DefaultBrokerAndCredential.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK, item))
			{
				AssertEquals("AN", ManifestHeader.AMA_GS_NKCustomsAgent);
				AssertEquals(glbExternalPasswordPK2, ManifestHeader.AMA_CustomsAgentCredentialPK);

				ManifestHeader.AMA_GS_NKCustomsAgent = string.Empty;
				ManifestHeader.AMA_CustomsAgentCredentialPK = ZGuid.Empty;
				ManifestHeader.AMA_TransportMode = TransportTypeList.Codes.Sea;
				AssertEquals("AN", ManifestHeader.AMA_GS_NKCustomsAgent);
				AssertEquals(glbExternalPasswordPK1, ManifestHeader.AMA_CustomsAgentCredentialPK);

				ManifestHeader.AMA_GS_NKCustomsAgent = string.Empty;
				ManifestHeader.AMA_CustomsAgentCredentialPK = ZGuid.Empty;
				ManifestHeader.AMA_GS_NKCustomsAgent = "AN";
				AssertEquals(glbExternalPasswordPK1, ManifestHeader.AMA_CustomsAgentCredentialPK);
			}
		}

		public void TestViaLocationCode()
		{
			ManifestHeader.ViaLocationCode = "12345";

			var query = new ZQuery(GenAddOnColumnSchema.XA_Name, Constants.GenAddOnColumnFieldName.AMA_ViaLocationCode);
			var persistedObj = Factory.LoadTop1<GenAddOnColumn>(query);

			AssertNotNull(persistedObj);
			AssertEquals("12345", persistedObj.XA_Data);
		}

		public void TestSetAMA_GS_NKCustomsAgent()
		{
			var (glbExternalPasswordPK1, glbExternalPasswordPK2, _) = new DefaultBrokerAndCredentialTestHelper().CreateBrokerStaffAndReturnPK();

			var item = new DefaultBrokerAndCredential(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);
			item.DefaultBrokerCode = "AN";
			item.ForwarderManifestAIR = "Test2002";
			item.ForwarderManifestSEA = "Test1001";
			JPRegistry.Instance.DefaultBrokerAndCredential.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK, item);

			ManifestHeader.AMA_GS_NKCustomsAgent = string.Empty;
			AssertEquals(ZGuid.Empty, ManifestHeader.AMA_CustomsAgentCredentialPK);
		}

		public void TestDefaultGetConsolSynchronizerCore()
		{
			var consol = Factory.New<ForwardingConsol>();
			ManifestHeader.SetParent(consol);
			AssertType<AsycudaManifestHeaderSynchroniser>(ManifestHeader.Synchroniser);
		}

		public void TestBills()
		{
			var bills = ManifestHeader.Bills;
			AssertType<AsycudaBillCollection>(bills);
			AssertType<AsycudaBill>(Bill);
		}

		public void TestLookups()
		{
			AssertType<AsycudaManifestHeaderLookups>(ManifestHeader.Lookups);
		}

		public void TestIAsycudaManifestHeader()
		{
			var bizObj = GetNewBusinessObject();
			bizObj.FillWithValidTestData();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<AsycudaManifestHeader>(bizObj.PK).GetType());
		}

		public void TestAsycudaBillType()
		{
			var bill = ManifestHeader.Bills.AddNew();
			AssertEquals(bill.GetType(), typeof(AsycudaBill));
		}

		public void TestAMA_MasterBill()
		{
			AssertEquals(20, ManifestHeader.AMA_MasterBillInfo.MaxLength);
		}

		public void TestPortOfLoadingIATACode()
		{
			Factory.CreateUNLOCOData();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "JP001";
			AssertEquals(string.Empty, header.PortOfLoadingIATACode);

			header.AMA_RL_NKPortOfLoading = "JP002";
			AssertEquals("TKU", header.PortOfLoadingIATACode);

			header.AMA_RL_NKPortOfLoading = string.Empty;
			header.PortOfLoadingIATACode = "TKU";
			AssertEquals("JP002", header.AMA_RL_NKPortOfLoading);

			header.PortOfDischargeIATACode = "TK";
			AssertEquals("TK", header.PortOfDischargeIATACode);
			AssertEquals("JP002", header.AMA_RL_NKPortOfLoading);

			header.PortOfLoadingIATACode = string.Empty;
			AssertEquals("JP002", header.AMA_RL_NKPortOfLoading);
		}

		public void TestPortOfDischargeIATACode()
		{
			Factory.CreateUNLOCOData();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfDischarge = "JP001";
			AssertEquals(string.Empty, header.PortOfDischargeIATACode);

			header.AMA_RL_NKPortOfDischarge = "JP002";
			AssertEquals("TKU", header.PortOfDischargeIATACode);

			header.AMA_RL_NKPortOfDischarge = string.Empty;
			header.PortOfDischargeIATACode = "TKU";
			AssertEquals("JP002", header.AMA_RL_NKPortOfDischarge);

			header.PortOfDischargeIATACode = "TK";
			AssertEquals("TK", header.PortOfDischargeIATACode);
			AssertEquals("JP002", header.AMA_RL_NKPortOfDischarge);

			header.PortOfDischargeIATACode = string.Empty;
			AssertEquals("JP002", header.AMA_RL_NKPortOfDischarge);
		}

		public void TestAMA_OA_CarrierAndAMA_CarrierCodeWhenAIR()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = TransportTypeList.Codes.Air;
			var airline1 = Factory.NewWithValidTestData<RefAirline>();
			airline1.RM_TwoCharacterCode = "1A";
			var airline2 = Factory.NewWithValidTestData<RefAirline>();
			airline2.RM_TwoCharacterCode = "1B";
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsAirLine = true;
			orgHeader.MiscServ.OM_RM_Airline = airline1.PK;
			header.AMA_OA_Carrier = orgHeader.MainAddress.PK;
			Factory.Save();

			AssertEquals("1A", header.AMA_CarrierCode);

			header.AMA_OA_Carrier = ZGuid.Empty;
			header.AMA_CarrierCode = "1B";
			AssertEquals(ZGuid.Empty, header.AMA_OA_Carrier);

			header.AMA_CarrierCode = "1A";
			AssertEquals(orgHeader.MainAddress.PK, header.AMA_OA_Carrier);
		}

		public void TestAMA_OA_CarrierAndAMA_CarrierCodeWhenSEA()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			var shippingLine1 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine1.RSL_StandardCarrierAlphaCode = "1234";
			var shippingLine2 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine2.RSL_StandardCarrierAlphaCode = "5678";
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsShippingLine = true;
			orgHeader.OH_RSL_ShippingLine = shippingLine1.PK;
			header.AMA_OA_Carrier = orgHeader.MainAddress.PK;
			Factory.Save();

			AssertEquals("1234", header.AMA_CarrierCode);

			header.AMA_OA_Carrier = ZGuid.Empty;
			header.AMA_CarrierCode = "5678";
			AssertEquals(ZGuid.Empty, header.AMA_OA_Carrier);

			header.AMA_CarrierCode = "1234";
			AssertEquals(orgHeader.MainAddress.PK, header.AMA_OA_Carrier);
		}

		public void TestIsSubConsolidation()
		{
			ManifestHeader.IsSubConsolidation = false;
			AssertEquals("AMA_AgentType should be M", AgentTypeList.Codes.M, ManifestHeader.AMA_AgentType);
			ManifestHeader.IsSubConsolidation = true;
			AssertEquals("AMA_AgentType should be H", AgentTypeList.Codes.H, ManifestHeader.AMA_AgentType);
		}

		public void TestDocumentSupporterType()
		{
			AssertType<AsycudaManifestHeaderDocumentSupporter>(ManifestHeader.DocumentSupporter);
		}

		public void TestGetConsolSynchronizerCore()
		{
			var consol = Factory.New<ForwardingConsol>();
			ManifestHeader.SetParent(consol);
			AssertType<AsycudaManifestHeaderSynchroniser>(ManifestHeader.Synchroniser);
		}

		public void TestPackedItemRelationship()
		{
			AssertEquals("Should be RelationshipType.One", ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.One, ManifestHeader.PackedItemRelationship);
		}

		public void TestBookingNumber()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(header.AMA_BookingNumberInfo);
			CombineAssertions(() =>
			{
				AssertEquals("BookingNumber Caption", "Booking Number", resourceStringData.Caption);
				AssertEquals("BookingNumber ShortCaption", "Booking#", resourceStringData.ShortCaption);
				AssertEquals("BookingNumber FullDescription", "VAN/VAE Booking Number", resourceStringData.FullDescription);
			});

			header.AMA_BookingNumber = "AA";
			Factory.Save();

			var query = new ZQuery()
				.AddToFilter(CusEntryNumSchema.CE_ParentID, header.PK)
				.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.JP.BookingNumber)
				.AddToFilter(CusEntryNumSchema.CE_EntryIsSystemGenerated, false);

			var entryNum = Factory.LoadTop1<CusEntryNumber>(query);
			AssertEquals(header.AMA_BookingNumber, entryNum.CE_EntryNum);

			header.AMA_BookingNumber = "";
			Factory.Save();
			entryNum = Factory.LoadTop1<CusEntryNumber>(query);
			AssertNull(entryNum);
		}

		public void TestIATACode_ReadOnly()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SetParent(consol);
			AssertEquals(true, header.PortOfLoadingIATACodeInfo.ReadOnly);
			AssertEquals(true, header.PortOfDischargeIATACodeInfo.ReadOnly);
			header.AMA_OverrideFreightDefaults = true;
			AssertEquals(false, header.PortOfLoadingIATACodeInfo.ReadOnly);
			AssertEquals(false, header.PortOfDischargeIATACodeInfo.ReadOnly);
		}

		public void TestMessageSendingInProgress()
		{
			var messageSendingContext = new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.NVC01 };
			using (ManifestHeader.SetCurrentMessageSendingContext(messageSendingContext))
			{
				AssertEquals("NVC01", true, ManifestHeader.IsNVC01SendingInProgress);
			}

			messageSendingContext.ProcedureCode = JPProcedureCodeList.Codes.EDA;
			using (ManifestHeader.SetCurrentMessageSendingContext(messageSendingContext))
			{
				AssertEquals("NVC01", false, ManifestHeader.IsNVC01SendingInProgress);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			ManifestHeader.SuspendCheckBusinessObjectType();
			return ManifestHeader;
		}

		AsycudaManifestHeader ManifestHeader
		{
			get
			{
				if (manifestHeader == null)
				{
					manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				}
				return manifestHeader;
			}
		}
		AsycudaManifestHeader manifestHeader;

		AsycudaBill Bill
		{
			get
			{
				if (bill == null)
				{
					bill = ManifestHeader.Bills.AddNew();
				}
				return bill;
			}
		}

		AsycudaBill bill;
	}
}
