using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>))]
	sealed class NctsDepartureCargoDescCollectionTest : ActiveBusinessObjectCollectionTestCase<INctsDepartureCargoDescCollection<NctsDepartureCargoDesc>>
	{
		protected override INctsDepartureCargoDescCollection<NctsDepartureCargoDesc> GetCollectionToTest()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return new NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>(header.Bills.AddNew());
		}
	}

	[TestedType(typeof(NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>))]
	sealed class NctsDepartureCargoDescCollectionInBillTest : ActiveBusinessObjectCollectionTestCase<INctsDepartureCargoDescCollection<NctsDepartureCargoDesc>>
	{
		public void TestBY_RX_NKLinePriceCurrencyValueDefault()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = header.Bills.AddNew();
			var collection = bill.GoodsItems;

			var goodItem1 = collection.AddNew();
			AssertEquals(ZString.Empty, goodItem1.BY_RX_NKLinePriceCurrency);

			bill.B0_RX_NKLinePriceCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			var goodItem2 = collection.AddNew();
			AssertEquals(Core.Constants.CurrencyCodes.EuropeanUnion, goodItem2.BY_RX_NKLinePriceCurrency);
		}

		public void TestMaxCountValidation_DuringTransitionPhase()
		{
			TestMaxCountValidation(999, true, "You may enter a maximum of 999 Goods Items.");
		}

		public void TestMaxCountValidation_AfterTransitionPhase()
		{
			TestMaxCountValidation(1999, false, "[G0005] You may enter a maximum of 1999 Goods Items.");
		}

		public void TestAddNewElement()
		{
			var collection = GetCollectionToTest();

			var item1 = collection.AddNew();
			AssertEquals(ZString.Empty, item1.BY_RN_NKCountryOfOrigin);

			var item2 = collection.AddNew();

			AssertEquals(ZString.Empty, item2.BY_RN_NKCountryOfOrigin);
		}

		void TestMaxCountValidation(int expectedMaxCount, bool isNcts5TransitionPhase, string maxCountExceededMessage)
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, isNcts5TransitionPhase))
			{
				var collection = GetCollectionToTest();
				var maxCountValidator = collection.MaxCountValidator;
				var notification = maxCountValidator.Notification;
				CombineAssertions(() =>
				{
					AssertEquals("MaxCount", expectedMaxCount, maxCountValidator.MaxCount);
					AssertEquals("Notification Type", NotificationType.Error, notification.Type);
					AssertEquals("Notification Message", maxCountExceededMessage, notification.Message);
					AssertEquals("WarnAtHalfway", false, maxCountValidator.WarnAtHalfway);
				});
			}
		}

		public void TestCopyLastGoodsItemToNewLinesIfEnabled_Phase4()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = header.MovementHeader;
			var goodsItems = movementHeader.GoodsItems;

			goodsItems.CopyLastGoodsItemToNewLines = true;

			var item1 = goodsItems.AddNew();
			item1.BY_DeclarationGoodsItemNumber = 3;
			item1.BY_Description = "GoodsDescription";
			item1.BY_GrossWeight = 500;
			item1.BY_NetWeight = 490;
			item1.BY_GrossWeightUnit = "KG";
			item1.BY_NetWeightUnit = "KG";
			item1.BY_HarmonisedTariff = "6802210000";
			item1.BY_FormattedHarmonisedTariff = "6802.21.00 00";
			item1.BY_CusC4Number = "4125";

			var addInfo1 = item1.AdditionalInfos.AddNew();
			addInfo1.CSI_SubType = "REF";
			addInfo1.CSI_Code = "C658";
			addInfo1.CSI_ReferenceNumber = "ADDINFO1 REF";
			addInfo1.CSI_Description = "DESCRIPTION";

			var suppDoc1 = item1.SupportingDocuments.AddNew();
			suppDoc1.CSI_Code = "C673";
			suppDoc1.CSI_ReferenceNumber = "SUPDOC1 REF";
			suppDoc1.CSI_ItemNumber = 2;
			suppDoc1.CSI_ReferenceNumber2 = "INFORMATION";

			var pack1 = item1.Packages.AddNew();
			pack1.B5_UnitType = "PL";
			pack1.B5_MarksAndNumbers = "MARKS AND NUMBERS 1";
			pack1.B5_UnitCount = 5;

			var prevDoc1 = item1.PreviousDocuments.AddNew();
			prevDoc1.CSI_Code = "N821";
			prevDoc1.CSI_ReferenceNumber = "PREVDOC1 REF";
			prevDoc1.CSI_ItemNumber = 1;
			prevDoc1.CSI_PackQty = 5;
			prevDoc1.CSI_PackType = "PL";
			prevDoc1.CSI_Quantity = 2;
			prevDoc1.CSI_UnitOfQuantity = "BX";
			prevDoc1.CSI_ReferenceNumber2 = "COMPLEMENT OF INFORMATION";

			var fee1 = item1.Fees.AddNew();
			fee1.BFE_BaseValue = 30;
			fee1.BFE_ChargeAmount = 5;
			fee1.BFE_ChargeType = "ABC";
			fee1.BFE_MethodOfCalculation = "P";
			fee1.BFE_MethodOfPayment = "C";
			fee1.BFE_Rate = 16;

			var dangerousSubstance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First();
			var undg1 = item1.UNDGs.AddNew();
			undg1.DI_DG = dangerousSubstance.PK;

			var docAddress1 = item1.DocAddresses.AddNew();
			docAddress1.E2_Address1 = "Street 1";
			docAddress1.E2_Address2 = "Dep A";
			docAddress1.E2_Postcode = "1234";
			docAddress1.E2_City = "City";
			docAddress1.E2_CompanyName = "Company name";
			docAddress1.E2_Email = "info@companyname.com";
			docAddress1.E2_Phone = "0123456789";
			docAddress1.E2_RN_NKCountryCode = "FR";
			docAddress1.E2_Contact = "Albert";

			var supplyChainActor1 = item1.CusSupplyChainActorReferences.AddNew();
			supplyChainActor1.CFR_Code = "FW";
			supplyChainActor1.CFR_Reference = "SUPPLYCHAINACTOR1";
			var owner = Factory.New<OrgHeader>();
			var ownerAddress = owner.Addresses.AddNew();
			ownerAddress.OA_RN_NKCountryCode = "FR";
			supplyChainActor1.CFR_OA_Owner = ownerAddress.PK;

			var container1 = header.DepartureHeaderContainers.AddNew();
			container1.BC_ContainerNum = "CONTAINER1";
			pack1.ContainersPivot.AddPivotFor(container1);
			var container2 = header.DepartureHeaderContainers.AddNew();
			container2.BC_ContainerNum = "CONTAINER2";
			pack1.ContainersPivot.AddPivotFor(container2);
			var container3 = header.DepartureHeaderContainers.AddNew();
			container3.BC_ContainerNum = "CONTAINER3";

			CombineAssertions(() =>
			{
				movementHeader.GoodsItems.CopyLastGoodsItemToNewLines = true;
				var item2 = movementHeader.GoodsItems.AddNew();
				var pack2 = item2.Packages.FirstOrDefault();
				AssertSame("Package.Parent", item2, pack2.Parent);
				AssertGoodsItem("Item 2", item2, 2, 0, "GoodsDescription", 500, 490, "KG", "KG", "6802210000", "6802.21.00 00", "4125", 0, ZString.Empty, 1, 1, 1, 1, 1, 1, 1, 1);
				var addInfo2 = item2.AdditionalInfos.First();
				AssertAdditionalInfo("Additional Info 2", item2.AdditionalInfos.FirstOrDefault(), "REF", "C658", "ADDINFO1 REF", "DESCRIPTION");
				AssertSupportingDocument("Supporting Document 2", item2.SupportingDocuments.FirstOrDefault(), "C673", "SUPDOC1 REF", 2, "INFORMATION");
				AssertPackage("Package 2", pack2, "PL", "MARKS AND NUMBERS 1", 5);
				AssertPreviousDocument("Previous Document 2", item2.PreviousDocuments.FirstOrDefault(), "N821", "PREVDOC1 REF", 1, 5, "PL", 2, "BX", "COMPLEMENT OF INFORMATION");
				AssertFee("Fee 2", item2.Fees.FirstOrDefault(), 30, 5, "ABC", "P", "C", 16);
				AssertUNDG("UNDG 2", item2.UNDGs.FirstOrDefault(), dangerousSubstance.PK);
				AssertDocAddress("Doc Address 2", (JobDocAddress)item2.DocAddresses.FirstOrDefault(), "Street 1", "Dep A", "1234", "City", "Company name", "info@companyname.com", "0123456789", "FR", "Albert");
				AssertSupplyChainActor("Supply Chain Actor 2", item2.CusSupplyChainActorReferences.FirstOrDefault(), "FW", "SUPPLYCHAINACTOR1", ownerAddress.PK);
				AssertEquals("ContainersPivot contains container1", false, pack2.ContainersPivot.Contains(container1));
				AssertEquals("ContainersPivot contains container2", false, pack2.ContainersPivot.Contains(container2));
				AssertEquals("ContainersPivot contains container3", false, pack2.ContainersPivot.Contains(container3));

				movementHeader.GoodsItems.CopyLastGoodsItemToNewLines = false;
				var item3 = movementHeader.GoodsItems.AddNew();
				AssertGoodsItem("Item 3", item3, 3, 0, ZString.Empty, ZDecimal.Zero, ZDecimal.Zero, "KG", "KG", ZString.Empty, ZString.Empty, ZString.Empty, 0, ZString.Empty,  0, 0, 0, 0, 0, 0, 0, 0);
			});
		}

		public void TestCopyLastGoodsItemToNewLinesIfEnabled_Phase5()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = header.Bills.AddNew();
			var goodsItems = bill.GoodsItems;

			goodsItems.CopyLastGoodsItemToNewLines = true;

			var item1 = goodsItems.AddNew();
			item1.BY_DeclarationGoodsItemNumber = 3;
			item1.BY_Description = "GoodsDescription";
			item1.BY_GrossWeight = 500;
			item1.BY_NetWeight = 490;
			item1.BY_GrossWeightUnit = "KG";
			item1.BY_NetWeightUnit = "KG";
			item1.BY_HarmonisedTariff = "6802210000";
			item1.BY_FormattedHarmonisedTariff = "6802.21.00 00";
			item1.BY_CusC4Number = "4125";
			item1.BY_LinePrice = 5;
			item1.BY_RX_NKLinePriceCurrency = "USD";

			var addInfo1 = item1.AdditionalInfos.AddNew();
			addInfo1.CSI_SubType = "REF";
			addInfo1.CSI_Code = "C658";
			addInfo1.CSI_ReferenceNumber = "ADDINFO1 REF";
			addInfo1.CSI_Description = "DESCRIPTION";

			var suppDoc1 = item1.SupportingDocuments.AddNew();
			suppDoc1.CSI_Code = "C673";
			suppDoc1.CSI_ReferenceNumber = "SUPDOC1 REF";
			suppDoc1.CSI_ItemNumber = 2;
			suppDoc1.CSI_ReferenceNumber2 = "INFORMATION";

			var pack1 = item1.Packages.AddNew();
			pack1.B5_UnitType = "PL";
			pack1.B5_MarksAndNumbers = "MARKS AND NUMBERS 1";
			pack1.B5_UnitCount = 5;

			var prevDoc1 = item1.PreviousDocuments.AddNew();
			prevDoc1.CSI_Code = "N821";
			prevDoc1.CSI_ReferenceNumber = "PREVDOC1 REF";
			prevDoc1.CSI_ItemNumber = 1;
			prevDoc1.CSI_PackQty = 5;
			prevDoc1.CSI_PackType = "PL";
			prevDoc1.CSI_Quantity = 2;
			prevDoc1.CSI_UnitOfQuantity = "BX";
			prevDoc1.CSI_ReferenceNumber2 = "COMPLEMENT OF INFORMATION";

			var fee1 = item1.Fees.AddNew();
			fee1.BFE_BaseValue = 30;
			fee1.BFE_ChargeAmount = 5;
			fee1.BFE_ChargeType = "ABC";
			fee1.BFE_MethodOfCalculation = "P";
			fee1.BFE_MethodOfPayment = "C";
			fee1.BFE_Rate = 16;

			var dangerousSubstance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First();
			var undg1 = item1.UNDGs.AddNew();
			undg1.DI_DG = dangerousSubstance.PK;

			var docAddress1 = item1.DocAddresses.AddNew();
			docAddress1.E2_Address1 = "Street 1";
			docAddress1.E2_Address2 = "Dep A";
			docAddress1.E2_Postcode = "1234";
			docAddress1.E2_City = "City";
			docAddress1.E2_CompanyName = "Company name";
			docAddress1.E2_Email = "info@companyname.com";
			docAddress1.E2_Phone = "0123456789";
			docAddress1.E2_RN_NKCountryCode = "FR";
			docAddress1.E2_Contact = "Albert";

			var supplyChainActor1 = item1.CusSupplyChainActorReferences.AddNew();
			supplyChainActor1.CFR_Code = "FW";
			supplyChainActor1.CFR_Reference = "SUPPLYCHAINACTOR1";
			var owner = Factory.New<OrgHeader>();
			var ownerAddress = owner.Addresses.AddNew();
			ownerAddress.OA_RN_NKCountryCode = "FR";
			supplyChainActor1.CFR_OA_Owner = ownerAddress.PK;

			var container1 = header.DepartureHeaderContainers.AddNew();
			container1.BC_ContainerNum = "CONTAINER1";
			pack1.ContainersPivot.AddPivotFor(container1);
			var container2 = header.DepartureHeaderContainers.AddNew();
			container2.BC_ContainerNum = "CONTAINER2";
			pack1.ContainersPivot.AddPivotFor(container2);
			var container3 = header.DepartureHeaderContainers.AddNew();
			container3.BC_ContainerNum = "CONTAINER3";

			CombineAssertions(() =>
			{
				bill.GoodsItems.CopyLastGoodsItemToNewLines = true;
				var item2 = (NctsDepartureCargoDesc)((IBindingList)bill.GoodsItems).AddNew();
				var pack2 = item2.Packages.FirstOrDefault();
				AssertNotNull("Package.Parent not null", pack2.Parent);
				AssertSame("Package.Parent same instance", item2, pack2.Parent);
				AssertGoodsItem("Item 2", item2, 2, 0, "GoodsDescription", 500, 490, "KG", "KG", "6802210000", "6802.21.00 00", "4125", 5, "USD", 1, 1, 1, 1, 1, 1, 1, 1);
				var addInfo2 = item2.AdditionalInfos.First();
				AssertAdditionalInfo("Additional Info 2", item2.AdditionalInfos.FirstOrDefault(), "REF", "C658", "ADDINFO1 REF", "DESCRIPTION");
				AssertSupportingDocument("Supporting Document 2", item2.SupportingDocuments.FirstOrDefault(), "C673", "SUPDOC1 REF", 2, "INFORMATION");
				AssertPackage("Package 2", pack2, "PL", "MARKS AND NUMBERS 1", 5);
				AssertPreviousDocument("Previous Document 2", item2.PreviousDocuments.FirstOrDefault(), "N821", "PREVDOC1 REF", 1, 5, "PL", 2, "BX", "COMPLEMENT OF INFORMATION");
				AssertFee("Fee 2", item2.Fees.FirstOrDefault(), 30, 5, "ABC", "P", "C", 16);
				AssertUNDG("UNDG 2", item2.UNDGs.FirstOrDefault(), dangerousSubstance.PK);
				AssertDocAddress("Doc Address 2", (JobDocAddress)item2.DocAddresses.FirstOrDefault(), "Street 1", "Dep A", "1234", "City", "Company name", "info@companyname.com", "0123456789", "FR", "Albert");
				AssertSupplyChainActor("Supply Chain Actor 2", item2.CusSupplyChainActorReferences.FirstOrDefault(), "FW", "SUPPLYCHAINACTOR1", ownerAddress.PK);
				AssertEquals("ContainersPivot contains container1", true, pack2.ContainersPivot.Contains(container1));
				AssertEquals("ContainersPivot contains container2", true, pack2.ContainersPivot.Contains(container2));
				AssertEquals("ContainersPivot no contains container3", false, pack2.ContainersPivot.Contains(container3));

				bill.GoodsItems.CopyLastGoodsItemToNewLines = false;
				var item3 = bill.GoodsItems.AddNew();
				AssertGoodsItem("Item 3", item3, 3, 0, ZString.Empty, ZDecimal.Zero, ZDecimal.Zero, "KG", "KG", ZString.Empty, ZString.Empty, ZString.Empty, 0, ZString.Empty, 0, 0, 0, 0, 0, 0, 0, 0);
			});
		}

		void AssertGoodsItem(ZString itemId, NctsDepartureCargoDesc item, ZShort lineNo, ZInt declarationGoodsItemNumber, ZString description, ZDecimal grossWeight, ZDecimal netWeight, ZString grossWeightUnit,
							ZString netWeightUnit, ZString harmonisedTariff, ZString formattedHarmonisedTariff, ZString cusC4Number, ZDecimal linePrice, string linePriceCurrency, int additionalInfos, int supportingDocuments, int packages,
							int previousDocuments, int fees, int undgs, int docAddresses, int supplyChainActorReferences)
		{
			if (item != null)
			{
				AssertEquals($"{itemId} Line Number", lineNo, item.BY_LineNo);
				AssertEquals($"{itemId} Declaration Goods Item Number", declarationGoodsItemNumber, item.BY_DeclarationGoodsItemNumber);
				AssertEquals($"{itemId} Description", description, item.BY_Description);
				AssertEquals($"{itemId} Gross Weight", grossWeight, item.BY_GrossWeight);
				AssertEquals($"{itemId} Net weight", netWeight, item.BY_NetWeight);
				AssertEquals($"{itemId} Gross Weight Unit", grossWeightUnit, item.BY_GrossWeightUnit);
				AssertEquals($"{itemId} Net Weight Unit", netWeightUnit, item.BY_NetWeightUnit);
				AssertEquals($"{itemId} Harmonised Tariff", harmonisedTariff, item.BY_HarmonisedTariff);
				AssertEquals($"{itemId} Formatted Harmonised Tariff", formattedHarmonisedTariff, item.BY_FormattedHarmonisedTariff);
				AssertEquals($"{itemId} Cus Code Number", cusC4Number, item.BY_CusC4Number);
				AssertEquals($"{itemId} Line price", linePrice, item.BY_LinePrice);
				AssertEquals($"{itemId} Line price currency", linePriceCurrency, item.BY_RX_NKLinePriceCurrency);
				AssertEquals($"{itemId} Number of Additional Infos", additionalInfos, item.AdditionalInfos.Count);
				AssertEquals($"{itemId} Number of Supporting Documents", supportingDocuments, item.SupportingDocuments.Count);
				AssertEquals($"{itemId} Number of Packages", packages, item.Packages.Count);
				AssertEquals($"{itemId} Number of Previous Documents", previousDocuments, item.PreviousDocuments.Count);
				AssertEquals($"{itemId} Number of Fees", fees, item.Fees.Count);
				AssertEquals($"{itemId} Number of UNDGs", undgs, item.UNDGs.Count);
				AssertEquals($"{itemId} Number of DocAddresses", docAddresses, item.DocAddresses.Count);
				AssertEquals($"{itemId} Number of Supply Chain Actor References", supplyChainActorReferences, item.CusSupplyChainActorReferences.Count);
			}
			else
			{
				Fail($"{itemId} was null.");
			}
		}
		void AssertAdditionalInfo(ZString additionalInfoId, NctsAdditionalInfo additionalInfo, ZString subType, ZString code, ZString referenceNumber, ZString description)
		{
			if (additionalInfo != null)
			{
				AssertEquals($"{additionalInfoId} Sub Type", subType, additionalInfo.CSI_SubType);
				AssertEquals($"{additionalInfoId} Code", code, additionalInfo.CSI_Code);
				AssertEquals($"{additionalInfoId} Reference Number", referenceNumber, additionalInfo.CSI_ReferenceNumber);
				AssertEquals($"{additionalInfoId} Description", description, additionalInfo.CSI_Description);
			}
			else
			{
				Fail($"{additionalInfoId} was null.");
			}
		}

		void AssertSupportingDocument(ZString supportingDocumentId, NctsSupportingDocument supportingDocument, ZString code, ZString referenceNumber, ZInt itemNumber, ZString referenceNumber2)
		{
			if (supportingDocument != null)
			{
				AssertEquals($"{supportingDocumentId} Code", code, supportingDocument.CSI_Code);
				AssertEquals($"{supportingDocumentId} Reference Number", referenceNumber, supportingDocument.CSI_ReferenceNumber);
				AssertEquals($"{supportingDocumentId} Item Number", itemNumber, supportingDocument.CSI_ItemNumber);
				AssertEquals($"{supportingDocumentId} Reference Number 2", referenceNumber2, supportingDocument.CSI_ReferenceNumber2);
			}
			else
			{
				Fail($"{supportingDocumentId} was null.");
			}
		}

		void AssertPackage(ZString packageId, NctsPackage package, ZString unitType, ZString marksAndNumbers, ZLong unitCount)
		{
			if (package != null)
			{
				AssertEquals($"{packageId} Unit Type", unitType, package.B5_UnitType);
				AssertEquals($"{packageId} Marks and Numbers", marksAndNumbers, package.B5_MarksAndNumbers);
				AssertEquals($"{packageId} Unit Count", unitCount, package.B5_UnitCount);
			}
			else
			{
				Fail($"{packageId} was null.");
			}
		}

		void AssertPreviousDocument(ZString previousDocumentId, NctsPreviousDocument previousDocument, ZString code, ZString referenceNumber, ZInt itemNumber, ZInt packQty, ZString packType, ZDecimal quantity, ZString unitOfQuantity, ZString referenceNumber2)
		{
			if (previousDocument != null)
			{
				AssertEquals($"{previousDocumentId} Code", code, previousDocument.CSI_Code);
				AssertEquals($"{previousDocumentId} Reference Number", referenceNumber, previousDocument.CSI_ReferenceNumber);
				AssertEquals($"{previousDocumentId} Item Number", itemNumber, previousDocument.CSI_ItemNumber);
				AssertEquals($"{previousDocumentId} Pack Qty", packQty, previousDocument.CSI_PackQty);
				AssertEquals($"{previousDocumentId} Pack Type", packType, previousDocument.CSI_PackType);
				AssertEquals($"{previousDocumentId} Quantity", quantity, previousDocument.CSI_Quantity);
				AssertEquals($"{previousDocumentId} Unit of Quantity", unitOfQuantity, previousDocument.CSI_UnitOfQuantity);
				AssertEquals($"{previousDocumentId} Reference Number 2", referenceNumber2, previousDocument.CSI_ReferenceNumber2);
			}
			else
			{
				Fail($"{previousDocumentId} was null.");
			}
		}

		void AssertFee(ZString feeId, NctsCargoDescFee fee, ZDecimal baseAmount, ZDecimal chargeAmount, ZString chargeType, ZString methodOfCalculation, ZString methodOfPayment, ZDecimal rate)
		{
			if (fee != null)
			{
				AssertEquals($"{feeId} Base Value", baseAmount, fee.BFE_BaseValue);
				AssertEquals($"{feeId} Charge Amount", chargeAmount, fee.BFE_ChargeAmount);
				AssertEquals($"{feeId} Charge Type", chargeType, fee.BFE_ChargeType);
				AssertEquals($"{feeId} Method of Calculation", methodOfCalculation, fee.BFE_MethodOfCalculation);
				AssertEquals($"{feeId} Method of Payment", methodOfPayment, fee.BFE_MethodOfPayment);
				AssertEquals($"{feeId} Rate", rate, fee.BFE_Rate);
			}
			else
			{
				Fail($"{feeId} was null.");
			}
		}

		void AssertUNDG(ZString undgId, UNDGDataItem undgDataItem, ZGuid dangerousSubstancePK)
		{
			if (undgDataItem != null)
			{
				AssertEquals($"{undgId} Dangerous Substance PK", dangerousSubstancePK, undgDataItem.DI_DG);
			}
			else
			{
				Fail($"{undgId} was null.");
			}
		}

		void AssertDocAddress(ZString docAddressId, JobDocAddress docAddress, ZString address1, ZString address2, ZString postcode, ZString city, ZString companyName, ZString email, ZString phone, ZString countryCode, ZString contact)
		{
			if (docAddress != null)
			{
				AssertEquals($"{docAddressId} Address 1", address1, docAddress.E2_Address1);
				AssertEquals($"{docAddressId} Address 2", address2, docAddress.Address2);
				AssertEquals($"{docAddressId} Postcode", postcode, docAddress.E2_Postcode);
				AssertEquals($"{docAddressId} City", city, docAddress.E2_City);
				AssertEquals($"{docAddressId} Company Name", companyName, docAddress.E2_CompanyName);
				AssertEquals($"{docAddressId} E-mail", email, docAddress.E2_Email);
				AssertEquals($"{docAddressId} Phone", phone, docAddress.E2_Phone);
				AssertEquals($"{docAddressId} Country Code", countryCode, docAddress.E2_RN_NKCountryCode);
				AssertEquals($"{docAddressId} Contact", contact, docAddress.E2_Contact);
			}
			else
			{
				Fail($"{docAddressId} was null.");
			}
		}

		void AssertSupplyChainActor(ZString supplyChainActorReferenceId, CusSupplyChainActorReference supplyChainActorReference, ZString code, ZString reference, ZGuid ownerAddressPK)
		{
			if (supplyChainActorReference != null)
			{
				AssertEquals($"{supplyChainActorReferenceId} Code", code, supplyChainActorReference.CFR_Code);
				AssertEquals($"{supplyChainActorReferenceId} Reference", reference, supplyChainActorReference.CFR_Reference);
				AssertEquals($"{supplyChainActorReferenceId} Owner", ownerAddressPK, supplyChainActorReference.CFR_OA_Owner);
			}
			else
			{
				Fail($"{supplyChainActorReferenceId} was null.");
			}
		}

		protected override INctsDepartureCargoDescCollection<NctsDepartureCargoDesc> GetCollectionToTest()
		{
			var org = Factory.New<OrgHeader>();
			var address = org.Addresses.AddNew();
			address.OA_RL_NKRelatedPortCode = "FRPAR";

			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var nctsConfigurationMock = new Mock<NctsConfiguration>() { CallBase = true };
			var validationRuleConfiguration = new ValidationRuleConfiguration();
			nctsConfigurationMock
				.Protected()
				.Setup<ZBool>("FullLoadPortSupportCore")
				.Returns(true);

			nctsConfigurationMock
				.Protected()
				.Setup<ValidationRuleConfiguration>("GetNewValidationRuleConfiguration")
				.Returns(validationRuleConfiguration);
			header.Factory.ClearCachedValue<NctsConfiguration>($"NctsConfiguration_{GlbCompany.CurrentCompany.GC_RN_NKCountryCode}");

			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock
				.Setup(m => m.GetObject())
				.Returns(nctsConfigurationMock.Object);

			var nctsConfiguration = new KeyObjectHandleDictionaryObject { { GlbCompany.CurrentCompany.GC_RN_NKCountryCode, objectHandleMock.Object } };
			using (ObjectFactory.Substitute("NCTS.NctsConfiguration", nctsConfiguration))
			{
				header.SetMovementType(NctsMovementType.Codes.Departure);
				header.Consignor.E2_OA_Address = address.PK;
			}
			var bill = header.Bills.AddNew();
			return new NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>(bill);
		}
	}

	[TestedType(typeof(NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>))]
	sealed class NctsDepartureCargoDescCollectionTest_Ncts4 : ActiveBusinessObjectCollectionTestCase<INctsDepartureCargoDescCollection<NctsDepartureCargoDesc>>
	{
		public void TestAddNewElement()
		{
			var collection = GetCollectionToTest();
			var item1 = collection.AddNew();
			AssertEquals("Item1's Country Of Origin should equals to header.PortOfDispatch.Left(2).", "FR", item1.BY_RN_NKCountryOfOrigin);

			item1.BY_RN_NKCountryOfOrigin = "DE";
			var item2 = collection.AddNew();

			AssertEquals("DE", item2.BY_RN_NKCountryOfOrigin);
		}

		protected override INctsDepartureCargoDescCollection<NctsDepartureCargoDesc> GetCollectionToTest()
		{
			var org = Factory.New<OrgHeader>();
			var address = org.Addresses.AddNew();
			address.OA_RL_NKRelatedPortCode = "FRPAR";

			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.Consignor.E2_OA_Address = address.PK;
			return header.MovementHeader.GoodsItems;
		}
	}
}
