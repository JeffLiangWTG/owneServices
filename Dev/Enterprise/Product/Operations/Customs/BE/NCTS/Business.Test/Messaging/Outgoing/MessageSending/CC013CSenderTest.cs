using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	sealed class CC013CSenderTest : NCTSMessageSenderTest<CC013CSender, ICC013C>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CC013CSender(null));
		}

		protected override string MovementType => NctsMovementType.Codes.Departure;

		protected override ZString EntryType => NctsMessageTypeList.Codes.Amendment;

		protected override ZString ExpectedPhaseStatus => NctsMovementHeaderTransactionStatusList.Codes.Amendment;

		protected override ZString ParentTableName => Customs.Business.AutoCusInBondMoveHeader.Schema.TableName;

		protected override void SetUpMockProviderData(Mock<ICC013C> mockProvider)
		{
			var currentDateTime = new DateTime(2023, 01, 31, 15, 47, 45);
			mockProvider.Setup(m => m.MessageType).Returns(Constants.MessageTypes.CC013C);
			mockProvider.Setup(m => m.PreparationDateTime).Returns(currentDateTime);
			mockProvider.Setup(m => m.MessageRecipient).Returns("NCTS.BE");
			mockProvider.Setup(m => m.MessageIdentification).Returns("SENDERS REFERENCE PLACE HOLDER");
			mockProvider.Setup(m => m.MessageSender).Returns("CW1@");

			mockProvider.Setup(m => m.TransitOperation).Returns(Mock.Of<ITransitOperation>(t =>
				t.DeclarationType == "TIR" &&
				t.AdditionalDeclarationType == "D" &&
				t.Security == 0 &&
				t.ReducedDatasetIndicator &&
				t.CommunicationLanguageAtDeparture == "NL" &&
				t.LimitDate == currentDateTime));
			mockProvider.Setup(m => m.Authorisations).Returns(new List<IAuthorization> { Mock.Of<IAuthorization>(a => a.SequenceNumber == "1" && a.Type == "C524" && a.ReferenceNumber == "CPH") });
			mockProvider.Setup(m => m.CustomsOfficeOfDeparture).Returns("DepId");
			mockProvider.Setup(m => m.CustomsOfficeOfDestination).Returns("DesID");
			mockProvider.Setup(m => m.HolderOfTheTransitProcedure).Returns(Mock.Of<IHolderOfTheTransitProcedure>(t =>
				t.IdentificationNumber == "BEHolderID" &&
				t.TirHolderIdentificationNumber == "TIR" &&
				t.ContactPerson == Mock.Of<IContactPerson>(p =>
					p.Name == "HolderContact" &&
					p.PhoneNumber == "PhoneNumber" &&
					p.EMailAddress == "EMailAddress")));
			mockProvider.Setup(m => m.Representative).Returns(Mock.Of<INCTSRepresentative>(t =>
				t.IdentificationNumber == "RepId" &&
				t.Status == 2 &&
				t.ContactPerson == Mock.Of<IContactPerson>(p =>
					p.Name == "RepresentativeContact" &&
					p.PhoneNumber == "PhoneNumber" &&
					p.EMailAddress == "EMailAddress")));
			mockProvider.Setup(m => m.Consignment).Returns(Mock.Of<IConsignmentType20>(c =>
				c.CountryOfDispatch == Core.Constants.CountryCodes.Belgium &&
				c.CountryOfDestination == Core.Constants.CountryCodes.Germany &&
				c.ContainerIndicator &&
				c.InlandModeOfTransport == 3 &&
				c.ModeOfTransportAtTheBorder == 3 &&
				c.GrossMass == 1200 &&
				c.ReferenceNumberUCR == "ref" &&
				c.Carrier == Mock.Of<IParty>(p =>
					p.Name == "Carrier" &&
					p.NameMaxlength == 35 &&
					p.ContactPerson == Mock.Of<IContactPerson>(co =>
						co.Name == "CarrierContact" &&
						co.PhoneNumber == "PhoneNumber" &&
						co.EMailAddress == "EMailAddress") &&
					p.Address == Mock.Of<IAddress>(a =>
						a.Country == Core.Constants.CountryCodes.Belgium &&
						a.City == "Antwerp")) &&
				c.Consignee == Mock.Of<IParty>(p =>
					p.Name == "Consignee" &&
					p.NameMaxlength == 35 &&
					p.ContactPerson == Mock.Of<IContactPerson>(co =>
						co.Name == "ConsigneeContact" &&
						co.PhoneNumber == "PhoneNumber" &&
						co.EMailAddress == "EMailAddress") &&
					p.Address == Mock.Of<IAddress>(a =>
						a.Country == Core.Constants.CountryCodes.Belgium &&
						a.City == "Brussels")) &&
				c.Consignor == Mock.Of<IParty>(p =>
					p.Name == "Consignor" &&
					p.NameMaxlength == 35 &&
					p.ContactPerson == Mock.Of<IContactPerson>(co =>
						co.Name == "ConsignorContact" &&
						co.PhoneNumber == "PhoneNumber" &&
						co.EMailAddress == "EMailAddress") &&
					p.Address == Mock.Of<IAddress>(a =>
						a.Country == Core.Constants.CountryCodes.Germany &&
						a.City == "Berlin")) &&
				c.ActiveBorderTransportMeans == new List<IActiveBorderTransportMeans> { Mock.Of<IActiveBorderTransportMeans>(a =>
					a.SequenceNumber == 1 &&
					a.CustomsOfficeAtBorderReferenceNumber == "BE101000" &&
					a.TypeOfIdentification == 40 &&
					a.IdentificationNumber == "IATA0000" &&
					a.Nationality == Core.Constants.CountryCodes.Belgium &&
					a.ConveyanceReferenceNumber == "conref" ) } &&
				c.PlaceOfLoading == Mock.Of<IPlace>(p => p.UnLocode == "ABCDE") &&
				c.PlaceOfUnloading == Mock.Of<IPlace>(p => p.Country == Core.Constants.CountryCodes.Germany && p.Location == "XYZ") &&
				c.TransportChargesMethodOfPayment == "LOL" &&
				c.HouseConsignments == new List<IHouseConsignmentType10> { Mock.Of<IHouseConsignmentType10>(h =>
					h.CountryOfDispatch == "BE" &&
					h.TransportChargesMethodOfPayment == "C" &&
					h.SequenceNumber == 1 &&
					h.ReferenceNumberUCR == "referenceucr" &&
					h.GrossMass == 100 &&
					h.AdditionalInformation == new List<IAdditionalInformation> { Mock.Of<IAdditionalInformation>(a =>
						a.SequenceNumber == 1 &&
						a.Code == "B" &&
						a.Text == "text") } &&
					h.AdditionalReferences == new List<IDocument> { Mock.Of<IDocument>(a =>
						a.SequenceNumber == 1 &&
						a.Type == "D" &&
						a.ReferenceNumber == "ref") } &&
					h.AdditionalSupplyChainActors == new List<IAdditionalSupplyChainActor> { Mock.Of<IAdditionalSupplyChainActor>(a =>
						a.SequenceNumber == 1 &&
						a.IdentificationNumber == "ID" &&
						a.Role == "ADM") } &&
					h.Consignee == Mock.Of<IParty>(p =>
						p.Name == "Consignee" &&
						p.NameMaxlength == 35 &&
						p.ContactPerson == Mock.Of<IContactPerson>(co =>
							co.Name == "ConsigneeContact" &&
							co.PhoneNumber == "PhoneNumber" &&
							co.EMailAddress == "EMailAddress") &&
						p.Address == Mock.Of<IAddress>(a =>
							a.Country == Core.Constants.CountryCodes.Belgium &&
							a.City == "Brussels")) &&
					h.Consignor == Mock.Of<IParty>(p =>
						p.Name == "Consignor" &&
						p.NameMaxlength == 35 &&
						p.ContactPerson == Mock.Of<IContactPerson>(co =>
							co.Name == "ConsignorContact" &&
							co.PhoneNumber == "PhoneNumber" &&
							co.EMailAddress == "EMailAddress") &&
						p.Address == Mock.Of<IAddress>(a =>
							a.Country == Core.Constants.CountryCodes.Germany &&
							a.City == "Berlin")) &&
					h.DepartureTransportMeans == new List<IDepartureTransportMeans> { Mock.Of<IDepartureTransportMeans>(d =>
						d.SequenceNumber == 1 &&
						d.IdentificationNumber == "ID" &&
						d.Nationality == "BE" &&
						d.TypeOfIdentification == 1) } &&
					h.PreviousDocuments == new List<IPreviousDocument> { Mock.Of<IPreviousDocument>(p =>
						p.SequenceNumber == 1 &&
						p.Type == "A" &&
						p.ComplementOfInformation == "complement" &&
						p.ReferenceNumber == "ref") } &&
					h.SupportingDocuments == new List<ISupportingDocument> { Mock.Of<ISupportingDocument>(s =>
						s.SequenceNumber == 1 &&
						s.DocumentLineItemNumber == 1 &&
						s.Type == "A" &&
						s.ComplementOfInformation == "complement" &&
						s.ReferenceNumber == "ref") } &&
					h.TransportDocuments == new List<ITransportDocument> { Mock.Of<ITransportDocument>(t =>
						t.SequenceNumber == 1 &&
						t.Type == "C" &&
						t.ReferenceNumber == "ref") } &&
					h.ConsignmentItems == new List<IConsignmentItemType09> { Mock.Of<IConsignmentItemType09>(i =>
						i.AdditionalInformation == new List<IAdditionalInformation> { Mock.Of<IAdditionalInformation>(a =>
							a.SequenceNumber == 1 &&
							a.Text == "text" &&
							a.Code == "A") } &&
						i.AdditionalReferences == new List<IDocument> { Mock.Of<IDocument>(a =>
							a.SequenceNumber == 1 &&
							a.Type == "A" &&
							a.ReferenceNumber == "ref") } &&
						i.AdditionalSupplyChainActors == new List<IAdditionalSupplyChainActor> { Mock.Of<IAdditionalSupplyChainActor>(a =>
							a.SequenceNumber == 1 &&
							a.IdentificationNumber == "ID" &&
							a.Role == "ADM") } &&
						i.Commodity == Mock.Of<ICommodity>(co =>
							co.CombinedNomenclatureCode == "123456" &&
							co.CusCode == "10" &&
							co.DangerousGoods == new List<IDangerousGoods> { Mock.Of<IDangerousGoods>(d =>
								d.SequenceNumber == 1 &&
								d.UNNumber == "10") } &&
							co.DescriptionOfGoods == "desc" &&
							co.GrossMass == 1 &&
							co.HarmonizedSystemSubHeadingCode == "78" &&
							co.NetMass == 2) &&
						i.Consignee == Mock.Of<IParty>(p =>
							p.Name == "Consignee" &&
							p.NameMaxlength == 35 &&
							p.ContactPerson == Mock.Of<IContactPerson>(co =>
								co.Name == "ConsigneeContact" &&
								co.PhoneNumber == "PhoneNumber" &&
								co.EMailAddress == "EMailAddress") &&
							p.Address == Mock.Of<IAddress>(a =>
								a.Country == Core.Constants.CountryCodes.Belgium &&
								a.City == "Brussels")) &&
						i.CountryOfDestination == "BE" &&
						i.CountryOfDispatch == "DE" &&
						i.DeclarationGoodsItemNumber == 2 &&
						i.DeclarationType == "H1" &&
						i.GoodsItemNumber == 1 &&
						i.Packagings == new List<IPackaging>() { Mock.Of<IPackaging>(p =>
							p.SequenceNumber == 1 &&
							p.NumberOfPackages == 10 &&
							p.TypeOfPackages == "B1" &&
							p.ShippingMarks == "shipping") } &&
						i.PreviousDocuments == new List<IPreviousDocumentExtended> { Mock.Of<IPreviousDocumentExtended>(p =>
							p.SequenceNumber == 1 &&
							p.Type == "A" &&
							p.ComplementOfInformation == "complement" &&
							p.ReferenceNumber == "ref" &&
							p.GoodsItemNumber == 1 &&
							p.MeasurementUnitAndQualifier == "GR" &&
							p.NumberOfPackages == 10 &&
							p.Quantity == 10 &&
							p.TypeOfPackages == "B1") } &&
						i.ReferenceNumberUCR == "ref" &&
						i.SupportingDocuments == new List<ISupportingDocument> { Mock.Of<ISupportingDocument>(s =>
							s.SequenceNumber == 1 &&
							s.DocumentLineItemNumber == 1 &&
							s.Type == "A" &&
							s.ComplementOfInformation == "complement" &&
							s.ReferenceNumber == "ref") } &&
						i.TransportChargesMethodOfPayment == "C" &&
						i.TransportDocuments == new List<ITransportDocument> { Mock.Of<ITransportDocument>(t =>
							t.SequenceNumber == 1 &&
							t.Type == "C" &&
							t.ReferenceNumber == "ref")
						})
					})
				}));
		}

		public void TestFillDeclarationGoodsItemNumberOnSending()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = header.Bills.AddNew();
			bill.B0_Weight = 1;
			bill.B0_WeightUQ = "KG";
			var goodsItem = bill.GoodsItems.AddNew();
			goodsItem.BY_Description = "Test";

			CombineAssertions(() =>
			{
				AssertEquals("Initial situation: no Declaration Goods Item Number generated.", 0, goodsItem.BY_DeclarationGoodsItemNumber);
				var declarationSenderProvider = new CC013CSender(new MessageSendingAction(header.MovementHeader));
				declarationSenderProvider.Send();
				AssertEquals("After creation of DeclarationMessageSenderProvider: Declaration Goods Item Number is generated", 1, goodsItem.BY_DeclarationGoodsItemNumber);
			});
		}

		public void TestTransactionAddedForDeletedGuarantee()
		{
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var guaranteeHeader = Factory.NewWithValidTestData<BE.Business.CusGuaranteeHeader>();
			guaranteeHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			guaranteeHeader.CPH_Number = "GUA1";
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddMonths(-1);
			guaranteeHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
			guaranteeHeader.CPH_SystemCreateTimeUtc = ZDate.Today;
			guaranteeHeader.CPH_Type = "TRA";
			guaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
			guaranteeHeader.CPH_Balance = 1000.0m;
			guaranteeHeader.CPH_OH_PermitHolder = org1.PK;
			var transaction = guaranteeHeader.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 1000.0m, 0, transactionType: Customs.Business.PermitTransactionTypeList.Codes.OBL, isAggregated: true);

			var guaranteeHeader2 = Factory.NewWithValidTestData<BE.Business.CusGuaranteeHeader>();
			guaranteeHeader2.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			guaranteeHeader2.CPH_Number = "GUA2";
			guaranteeHeader2.CPH_StartDate = ZDate.Today.AddMonths(-1);
			guaranteeHeader2.CPH_EndDate = ZDate.Today.AddMonths(1);
			guaranteeHeader2.CPH_SystemCreateTimeUtc = ZDate.Today;
			guaranteeHeader2.CPH_Type = "TRA";
			guaranteeHeader2.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
			guaranteeHeader2.CPH_Balance = 2000.0m;
			guaranteeHeader2.CPH_OH_PermitHolder = org1.PK;
			var transaction2 = guaranteeHeader2.AddTransaction("OPENING2", "OPENING2", ZString.Empty, ZString.Empty, 2000.0m, 0, transactionType: Customs.Business.PermitTransactionTypeList.Codes.OBL, isAggregated: true);

			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "LRN123456789";

			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondNumber = "GUA1";
			guarantee.PW_BondAmount = 145.0m;
			guarantee.PW_CPH_Guarantee = guaranteeHeader.PK;

			guaranteeHeader.AddTransaction(nctsHeader.MovementHeader.BM_PaperlessInbondNum,
											"NCTS write-off " + nctsHeader.MovementHeader.BM_PaperlessInbondNum + " [" + nctsHeader.MovementReferenceNumber + "]",
											"001",
											ZString.Empty,
											guarantee.PW_BondAmount * -1,
											0,
											status: Customs.Business.PermitTransactionStatusList.Codes.Pending);

			guarantee.PW_BondNumber = "GUA2";
			guarantee.PW_CPH_Guarantee = guaranteeHeader2.PK;

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Initial balance GUA1", 855.0m, guaranteeHeader.CPH_Calc_TotalBalanceIncludingPendingDecimal);
				AssertEquals("Initial balance GUA2", 2000.0m, guaranteeHeader2.CPH_Calc_TotalBalanceIncludingPendingDecimal);
				AssertEquals("Initial situation GUA1", 2, guaranteeHeader.GetTransactions().Count());
				AssertEquals("Initial situation GUA2", 1, guaranteeHeader2.GetTransactions().Count());

				messageSender.Send();
				AssertEquals("New balance GUA1", 1000.0m, guaranteeHeader.CPH_Calc_TotalBalanceIncludingPendingDecimal);
				AssertEquals("Extra transaction on GUA1", 3, guaranteeHeader.GetTransactions().Count());
				var newTransactionGUA1 = guaranteeHeader.GetTransactions().Last();
				AssertEquals("New transaction GUA1 should have status 'PND'", Customs.Business.PermitTransactionStatusList.Codes.Pending, newTransactionGUA1.CPL_TransactionStatus);
				AssertEquals("New transaction GUA1 should reference the LRN of the NCTS Header", "LRN123456789", newTransactionGUA1.CPL_Reference);
				AssertEquals("New transaction GUA1 amount must be 145", 145.0m, newTransactionGUA1.CPL_TranValue);

				AssertEquals("New balance GUA2", 1855.0m, guaranteeHeader2.CPH_Calc_TotalBalanceIncludingPendingDecimal);
				AssertEquals("Extra transaction on GUA2", 2, guaranteeHeader2.GetTransactions().Count());
				var newTransactionGUA2 = guaranteeHeader2.GetTransactions().Last();
				AssertEquals("New transaction GUA2 should have status 'PND'", Customs.Business.PermitTransactionStatusList.Codes.Pending, newTransactionGUA2.CPL_TransactionStatus);
				AssertEquals("New transaction GUA2 should reference the LRN of the NCTS Header", "LRN123456789", newTransactionGUA2.CPL_Reference);
				AssertEquals("New transaction GUA1 amount must be -145", -145.0m, newTransactionGUA2.CPL_TranValue);
			});
		}

		public void TestTransactionAddedForChangedGuaranteeAmount()
		{
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var guaranteeHeader = Factory.NewWithValidTestData<BE.Business.CusGuaranteeHeader>();
			guaranteeHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			guaranteeHeader.CPH_Number = "GUA1";
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddMonths(-1);
			guaranteeHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
			guaranteeHeader.CPH_SystemCreateTimeUtc = ZDate.Today;
			guaranteeHeader.CPH_Type = "TRA";
			guaranteeHeader.CPH_RN_NKCountryCode = "BE";
			guaranteeHeader.CPH_Balance = 1000.0m;
			guaranteeHeader.CPH_OH_PermitHolder = org1.PK;
			var transaction = guaranteeHeader.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 1000.0m, 0, transactionType: Customs.Business.PermitTransactionTypeList.Codes.OBL, isAggregated: true);

			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "LRN123456789";

			nctsHeader.MovementHeader.Guarantees.RemoveAndDeleteAll();
			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondNumber = "GUA1";
			guarantee.PW_BondAmount = 145.0m;

			guaranteeHeader.AddTransaction(nctsHeader.MovementHeader.BM_PaperlessInbondNum,
										   "NCTS write-off " + nctsHeader.MovementHeader.BM_PaperlessInbondNum + " [" + nctsHeader.MovementReferenceNumber + "]",
										   "001",
										   ZString.Empty,
										   guarantee.PW_BondAmount * -1,
										   0,
										   status: Customs.Business.PermitTransactionStatusList.Codes.Pending);

			guarantee.PW_BondAmount = 150.0m;
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Initial situation GUA1", 2, guaranteeHeader.GetTransactions().Count());

				messageSender.Send();

				AssertEquals("Extra transaction on GUA1", 3, guaranteeHeader.GetTransactions().Count());
				var newTransactionGUA1 = guaranteeHeader.GetTransactions().Last();
				AssertEquals("New transaction GUA1 should have status 'PND'", Customs.Business.PermitTransactionStatusList.Codes.Pending, newTransactionGUA1.CPL_TransactionStatus);
				AssertEquals("New transaction GUA1 should reference the LRN of the NCTS Header", "LRN123456789", newTransactionGUA1.CPL_Reference);
				AssertEquals("New transaction GUA1 amount must be -5", -5.0m, newTransactionGUA1.CPL_TranValue);
			});
		}

		[TestDate(2024, 11, 19)]
		public void TestSend_ShouldSetValuationDate()
		{
			AssertEquals("Precondition", ZDateTime.Empty, nctsHeader.MovementHeader.BM_ValuationDate);

			messageSender.Send();

			AssertEquals(ZDateTime.Now, nctsHeader.MovementHeader.BM_ValuationDate);
		}
	}
}
