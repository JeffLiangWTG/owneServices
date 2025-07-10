using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	sealed class MessageHeaderProviderBaseOnlyTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new MessageHeaderProviderForTest(null));
		}

		[TestDate(2019, 10, 29)]
		public void TestDateOfPreparation()
		{
			AssertEquals(new DateTime(2019, 10, 29, 0, 0, 0), messageHeaderProvider.PreparationDateAndTimeCET.Date);
		}

		[TestDate(2019, 10, 29, 10, 37, 56)]
		public void TestTimeOfPreparation()
		{
			AssertEquals("11:37:56", messageHeaderProvider.PreparationDateAndTimeCET.Time);
		}

		public void TestMessageRecipientConsignorDeclaration()
		{
			CombineAssertions(() =>
			{
				emcsDeclaration.JE_DeclarantType = EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignor;
				AssertEquals("No Office", ZString.Empty, messageHeaderProvider.Recipient);
				var competentOffice = emcsDeclaration.CustomsOffices[0];
				competentOffice.CY_Code = OfficeCodes_EMCS.Codes.CompetentAuthorityOfDispatch;
				competentOffice.CY_Data = "AuthOfce";
				AssertEquals("Office", "AuthOfce", new MessageHeaderProviderForTest(emcsDeclaration).Recipient);
			});
		}

		public void TestMessageRecipientConsigneeDeclaration()
		{
			CombineAssertions(() =>
			{
				emcsDeclaration.JE_DeclarantType = EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignee;
				AssertEquals("No Office", ZString.Empty, messageHeaderProvider.Recipient);
				var competentOffice = emcsDeclaration.CustomsOffices[0];
				competentOffice.CY_Code = OfficeCodes_EMCS.Codes.CompetentAuthorityOfArrival;
				competentOffice.CY_Data = "AuthOfce";
				AssertEquals("Office", "AuthOfce", new MessageHeaderProviderForTest(emcsDeclaration).Recipient);
			});
		}

		public void TestMessageSender_Registry()
		{
			var currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();

			CombineAssertions(() =>
			{
				AssertEquals("No Exception just empty", ZString.Empty, messageHeaderProvider.Sender);
				using (DECustomsDataRegistry.Instance.EMCSExciseTraderNumber.SetTemporaryValue(Guid.NewGuid(), Guid.Empty, Guid.Empty, "DE98000003100"))
				{
					AssertEquals(ZString.Empty, new MessageHeaderProviderForTest(emcsDeclaration).Sender);
				}
				using (DECustomsDataRegistry.Instance.EMCSExciseTraderNumber.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "DE98000003100"))
				{
					AssertEquals("Bin not set", ZString.Empty, new MessageHeaderProviderForTest(emcsDeclaration).Sender);
				}
				using (DECustomsDataRegistry.Instance.EMCSExciseTraderNumber.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "DE98000003100"))
				{
					using (DECustomsDataRegistry.Instance.EMCSParticipantIdentificationNumber.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "9800000310012345123456000"))
					{
						AssertEquals("DE98000003100", new MessageHeaderProviderForTest(emcsDeclaration).Sender);
					}
				}
			});
		}

		public void TestMessageSender_Consignor()
		{
			CombineAssertions(() =>
			{
				emcsDeclaration.JE_DeclarantType = EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignor;
				var consignor = Factory.New<OrgHeader>();
				AddCustomsRegNumber(consignor, GermanyOrgCusCodeInfo.OrgCusCodes.EMCSParticipantIdentificationNumber, "EPI2020");
				emcsDeclaration.SupplierDocumentaryAddress.OrganisationPK = consignor.PK;
				AssertEquals("Consignor has an EPI code but no Trader Excise Number", ZString.Empty, messageHeaderProvider.Sender);

				consignor.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, "DE12345678TEN", Core.Constants.CountryCodes.Greece);
				AssertEquals("Consignor has an EPI code and Trader Excise Number", "DE12345678TEN", new MessageHeaderProviderForTest(emcsDeclaration).Sender);
			});
		}

		public void TestMessageSender_Consignee()
		{
			CombineAssertions(() =>
			{
				emcsDeclaration.JE_DeclarantType = EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignee;
				var consignee = Factory.New<OrgHeader>();
				AddCustomsRegNumber(consignee, GermanyOrgCusCodeInfo.OrgCusCodes.EMCSParticipantIdentificationNumber, "EPI2020");
				emcsDeclaration.ImporterDocumentaryAddress.OrganisationPK = consignee.PK;
				AssertEquals("Consignee has an EPI code but no Trader Excise Number", ZString.Empty, messageHeaderProvider.Sender);

				consignee.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, "DE87654321TEN", Core.Constants.CountryCodes.Greece);
				AssertEquals("Consignee has an EPI code and Trader Excise Number", "DE87654321TEN", new MessageHeaderProviderForTest(emcsDeclaration).Sender);
			});
		}

		public void TestMessageSender_DispatchWarehouse()
		{
			emcsDeclaration.JE_DeclarantType = EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignor;
			var dispatchWarehouse = Factory.New<OrgHeader>();
			emcsDeclaration.DispatchWarehouseDocumentaryAddress.OrganisationPK = dispatchWarehouse.PK;
			var mainAddress = dispatchWarehouse.MainAddress;
			AddCustomsRegNumber(mainAddress, GermanyOrgCusCodeInfo.OrgCusCodes.EMCSWarehouseParticipantIdentificationNumber, "WPI918");
			AssertEquals("Warehouse has an WPI code but no Trader ID", ZString.Empty, messageHeaderProvider.Sender);

			mainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, "DE687678686TID", Core.Constants.CountryCodes.Greece);
			AssertEquals("Warehouse has an WPI code and Trader ID", "DE687678686TID", new MessageHeaderProviderForTest(emcsDeclaration).Sender);
		}

		public void TestMessageSender_DestinationWarehouse()
		{
			CombineAssertions(() =>
			{
				emcsDeclaration.JE_DeclarantType = EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignee;
				var destinationWarehouseOrg = Factory.New<OrgHeader>();
				emcsDeclaration.DestinationWarehouseDocumentaryAddress.OrganisationPK = destinationWarehouseOrg.PK;
				var mainAddress = destinationWarehouseOrg.MainAddress;
				AddCustomsRegNumber(mainAddress, GermanyOrgCusCodeInfo.OrgCusCodes.EMCSWarehouseParticipantIdentificationNumber, "WPI918");
				AssertEquals("Warehouse has an WPI code but no Trader ID", ZString.Empty, messageHeaderProvider.Sender);

				mainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, "DE987235810TID", Core.Constants.CountryCodes.Greece);
				AssertEquals("Warehouse has an WPI code and Trader ID", "DE987235810TID", new MessageHeaderProviderForTest(emcsDeclaration).Sender);
			});
		}

		public void TestBin_Registry()
		{
			var currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			CombineAssertions(() =>
			{
				AssertEquals("No Exception just empty", ZString.Empty, messageHeaderProvider.Bin);
				using (DECustomsDataRegistry.Instance.EMCSParticipantIdentificationNumber.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "9800000310012345123456000"))
				{
					AssertEquals("Message Sender Registry Empty", ZString.Empty, new MessageHeaderProviderForTest(emcsDeclaration).Bin);
				}
				using (DECustomsDataRegistry.Instance.EMCSExciseTraderNumber.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "DE98000003100"))
				{
					using (DECustomsDataRegistry.Instance.EMCSParticipantIdentificationNumber.SetTemporaryValue(Guid.NewGuid(), Guid.Empty, Guid.Empty, "9800000310012345123456000"))
					{
						AssertEquals("Different Company", ZString.Empty, new MessageHeaderProviderForTest(emcsDeclaration).Bin);
					}
					using (DECustomsDataRegistry.Instance.EMCSParticipantIdentificationNumber.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "9800000310012345123456000"))
					{
						AssertEquals("Valid", "9800000310012345123456000", new MessageHeaderProviderForTest(emcsDeclaration).Bin);
					}
				}
			});
		}

		public void TestBin_Consigor()
		{
			CombineAssertions(() =>
			{
				emcsDeclaration.JE_DeclarantType = EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignor;
				var consignor = Factory.New<OrgHeader>();
				emcsDeclaration.SupplierDocumentaryAddress.OrganisationPK = consignor.PK;
				AssertEquals("No Customs Code", ZString.Empty, messageHeaderProvider.Bin);
				AddCustomsRegNumber(consignor, GermanyOrgCusCodeInfo.OrgCusCodes.EMCSParticipantIdentificationNumber, "1234567890EPI");
				AssertEquals("EPI Customs Code", "1234567890EPI", new MessageHeaderProviderForTest(emcsDeclaration).Bin);
			});
		}

		public void TestBin_Consignee()
		{
			CombineAssertions(() =>
			{
				emcsDeclaration.JE_DeclarantType = EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignee;
				var consignee = Factory.New<OrgHeader>();
				emcsDeclaration.ImporterDocumentaryAddress.OrganisationPK = consignee.PK;
				AssertEquals("No Customs Code", ZString.Empty, messageHeaderProvider.Bin);
				AddCustomsRegNumber(consignee, GermanyOrgCusCodeInfo.OrgCusCodes.EMCSParticipantIdentificationNumber, "987654321EPI");
				AssertEquals("Consignee has an EPI code", "987654321EPI", new MessageHeaderProviderForTest(emcsDeclaration).Bin);
			});
		}

		public void TestBin_DispatchWarehouse()
		{
			CombineAssertions(() =>
			{
				emcsDeclaration.JE_DeclarantType = EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignor;
				var dispatchWarehouse = Factory.New<OrgHeader>();
				emcsDeclaration.DispatchWarehouseDocumentaryAddress.OrganisationPK = dispatchWarehouse.PK;
				AssertEquals("No Customs Code", ZString.Empty, messageHeaderProvider.Bin);
				AddCustomsRegNumber(dispatchWarehouse.MainAddress, GermanyOrgCusCodeInfo.OrgCusCodes.EMCSWarehouseParticipantIdentificationNumber, "WPI918");
				AssertEquals("Warehouse has an WPI", "WPI918", new MessageHeaderProviderForTest(emcsDeclaration).Bin);
			});
		}

		public void TestBin_DestinationWarehouse()
		{
			CombineAssertions(() =>
			{
				emcsDeclaration.JE_DeclarantType = EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignee;
				var destinationWarehouseOrg = Factory.New<OrgHeader>();
				emcsDeclaration.DestinationWarehouseDocumentaryAddress.OrganisationPK = destinationWarehouseOrg.PK;
				AssertEquals("No Customs Code", ZString.Empty, messageHeaderProvider.Bin);
				AddCustomsRegNumber(destinationWarehouseOrg.MainAddress, GermanyOrgCusCodeInfo.OrgCusCodes.EMCSWarehouseParticipantIdentificationNumber, "WPI807");
				emcsDeclaration.DestinationWarehouseDocumentaryAddress.OrganisationPK = destinationWarehouseOrg.PK;
				AssertEquals("Warehouse has an WPI", "WPI807", new MessageHeaderProviderForTest(emcsDeclaration).Bin);
			});
		}

		public void TestInterchangeControlReference()
		{
			AssertEquals(EDIInterchange.InterchangeNumberPlaceHolder, messageHeaderProvider.InterchangeControlReference);
		}

		public void TestMessageIdentifier()
		{
			AssertEquals(EDIMessage.SendersReferencePlaceHolder, messageHeaderProvider.MessageIdentifier);
		}

		protected override void SetUp()
		{
			base.SetUp();
			emcsDeclaration = Factory.New<EMCSJobDeclaration>();
			messageHeaderProvider = new MessageHeaderProviderForTest(emcsDeclaration);
		}
		EMCSJobDeclaration emcsDeclaration;
		MessageHeaderProviderForTest messageHeaderProvider;

		void AddCustomsRegNumber(OrgHeader orgHeader, ZString codeType, ZString customsRegNo)
		{
			var customsCode = orgHeader.CustomsCodes.AddNew();
			customsCode.OK_CodeType = codeType;
			customsCode.OK_CustomsRegNo = customsRegNo;
			customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
		}

		void AddCustomsRegNumber(OrgAddress orgAddress, ZString codeType, ZString customsRegNo)
		{
			var customsCode = orgAddress.CustomsCodes.AddNew();
			customsCode.OK_CodeType = codeType;
			customsCode.OK_CustomsRegNo = customsRegNo;
			customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
		}
	}

	class MessageHeaderProviderForTest : MessageHeaderProvider<HeaderProviderBase>, CargoWise.Customs.DE.MessageContracts.EMCS.IEMCSMessageHeader
	{
		public MessageHeaderProviderForTest(EMCSJobDeclaration emcsJobDeclaration)
			: base(emcsJobDeclaration)
		{
		}
	}
}
