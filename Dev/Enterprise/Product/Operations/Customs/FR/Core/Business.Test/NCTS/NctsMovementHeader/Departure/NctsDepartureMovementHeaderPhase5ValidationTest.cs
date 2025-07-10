using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	class NctsDepartureMovementHeaderPhase5ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTirCarnetExpiryDate()
		{
			departureMovement.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
			departureMovement.Validation.ValidateTirCarnetExpiryDate();
			AssertNoMessageErrors("TIR with No Expiry", departureMovement.TirCarnetExpiryDateInfo);
		}

		public void TestCheckBM_MessageStatus()
		{
			var log = nctsHeader.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Reference = CustomsMessageStatusList.Codes.MessageNotDeliveredToCustoms;
				log.SL_SE_NKEvent = Events.FrenchCustomsMessageStatus.Code;
				log.SL_EventTime = ZDateTime.Now;
				var errorContextItem = log.SourceInfoItems.AddNew();
				errorContextItem.Key = "Error";
				errorContextItem.Data = "Error Description";
			}

			var log2 = nctsHeader.Logs.AddNew();
			using (log2.LockForUpdatingKeyFieldsForTesting())
			{
				log2.SL_Reference = CustomsMessageStatusList.Codes.MessageNotDeliveredToCustoms;
				log2.SL_SE_NKEvent = Events.FrenchCustomsMessageStatus.Code;
				log2.SL_EventTime = ZDateTime.Now.AddDays(-1);
				var errorContextItem = log2.SourceInfoItems.AddNew();
				errorContextItem.Key = "Error";
				errorContextItem.Data = "Error Description 2";
			}

			AssertNoWarning("No warning to expect when BM_MessageStatus is not REJ.", departureMovement.BM_MessageStatusInfo, "Error Description");

			departureMovement.BM_MessageStatus = EDIMessageStatusList.Codes.Rejected;
			AssertHasWarning("Warning should match most recent event.", departureMovement.BM_MessageStatusInfo, "Error Description");
		}

		public void TestCheckChargePaymentOrDestinationI_ListValidation()
		{
			AssertListValidationInvalidCodeMessageError(departureMovement.ChargePaymentOrDestinationIDInfo, false);

			departureMovement.ChargePaymentOrDestinationID = "AAA";
			AssertListValidationInvalidCodeMessageError(departureMovement.ChargePaymentOrDestinationIDInfo, true);
		}

		public void TestCheckChargePaymentOrDestination_MandatoryValidation()
		{
			SetupPortTaxes();
			AssertEquals("Prerequisite: ChargePaymentOrDestinationID list should be empty.", 0, departureMovement.FRLookups.PaymentDestinationList.Count);
			AssertNoMessageErrorContaining(departureMovement.ChargePaymentOrDestinationIDInfo, MandatoryValidation.YouHaveNotEntered);

			var departureOffice = departureMovement.CustomsOffices.AddNew();
			departureOffice.CY_Code = "DEP";
			departureOffice.CY_Data = "FR000120";
			departureMovement.BM_RL_NKPortOfPresentation = "FRBAS";
			AssertEquals("Prerequisite: ChargePaymentOrDestinationID list should not be empty.", "010, 202", departureMovement.FRLookups.PaymentDestinationList.CodesAsString);
			AssertNoMessageErrorContaining(departureMovement.ChargePaymentOrDestinationIDInfo, MandatoryValidation.YouHaveNotEntered);

			departureMovement.ChargePaymentOrDestinationID = "010";
			AssertNoMessageErrorContaining(departureMovement.ChargePaymentOrDestinationIDInfo, MandatoryValidation.YouHaveNotEntered);

			departureOffice.CY_Data = "FR000133";
			AssertEquals("Prerequisite: ChargePaymentOrDestinationID list should not be empty.", "333", departureMovement.FRLookups.PaymentDestinationList.CodesAsString);

			departureMovement.ChargePaymentOrDestinationID = ZString.Empty;
			AssertHasMessageErrorContaining("Message error should be added when one of the THI codes in the list has harbour rate but no value is selected.", departureMovement.ChargePaymentOrDestinationIDInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestLocationOfGoodsIsMandatory()
		{
			CombineAssertions("Location of Goods should be mandatory.", () =>
			{
				AssertLocationOfGoodsMandatory(true, departureMovement, ZString.Empty);
				AssertLocationOfGoodsMandatory(false, departureMovement, "V;D;LV00001");
			});
		}

		public void AssertLocationOfGoodsMandatory(bool shouldBeMandatory, NctsDepartureMovementHeader departureMovementHeader, ZString locationOfGoods)
		{
			if (!string.IsNullOrEmpty(locationOfGoods))
			{
				var splitlocationOfGoods = locationOfGoods.Split(";");
				departureMovementHeader.GoodsLocation.CGL_Qualifier = splitlocationOfGoods[0];
				departureMovementHeader.GoodsLocation.CGL_Type = splitlocationOfGoods[1];
				departureMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = splitlocationOfGoods[2];
				AssertEquals("Prerequisite: GoodsLocationDescription is not empty.", departureMovementHeader.GoodsLocationDescription, locationOfGoods);
			}
			else
			{
				departureMovementHeader.GoodsLocation.CGL_Qualifier = ZString.Empty;
				departureMovementHeader.GoodsLocation.CGL_Type = ZString.Empty;
				departureMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = ZString.Empty;
				AssertEquals("Prerequisite: GoodsLocationDescription is empty.", departureMovementHeader.GoodsLocationDescription, ZString.Empty);
			}

			departureMovementHeader.Validation.ValidateGoodsLocationDescription();

			if (shouldBeMandatory)
			{
				AssertHasMessageErrorContaining("MessageError is expected for LocationOfGoods when GoodsLocationDescription is empty.", departureMovementHeader.GoodsLocationDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			}
			else
			{
				AssertNoMessageErrorContaining("No MessageError is expected for LocationOfGoods when GoodsLocationDescription is entered.", departureMovementHeader.GoodsLocationDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}

		void SetupPortTaxes()
		{
			var referenceDataHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "010", "BORDEAUX BASSENS", "FRBAS", "FR000120");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "202", "BORDEAUX-BASSENS", "FRBAS", "FR000120");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "333", "BORDEAUX-BASSENS", "FRBAS", "FR000133");
			referenceDataHelper.CreateHarbourRate("IMP", "333", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "IF([TLCL] > 1, MAX(2, 0.5 * [TLCL]), 0)", "FR");
			Factory.Save();
		}

		public void TestCheckBM_PresentationDateTime()
		{
			var message = "[NAT050] A date is mandatory in case of Pre-Lodged declaration";
			
			using (var deciderTestContext = new EU.NCTS.Business.Testing.MovementHeaderValidationDeciderTestContext<IFRNctsDepartureMovementHeaderPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.DisableRule(c => c.IsRuleNAT050Active);

				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

				var movementHeader = nctsHeader.MovementHeader;
				movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
				movementHeader.BM_PresentationDateTime = ZDateTimeOffset.Empty;
				AssertNoMessageErrorContaining("BM_PresentationDateTime is mandatory when BM_AdditionalDeclarationType is not equal to D - BM_PresentationDateTime is empty But rule is disable.", movementHeader.BM_PresentationDateTimeInfo, message);

				deciderTestContext.EnableRule(c => c.IsRuleNAT050Active);
				movementHeader.BM_PresentationDateTime = ZDateTimeOffset.Now;
				movementHeader.BM_PresentationDateTime = ZDateTimeOffset.Empty;
				AssertHasMessageErrorContaining("BM_PresentationDateTime is mandatory when BM_AdditionalDeclarationType is not equal to D - BM_PresentationDateTime is empty and rule is enable.", movementHeader.BM_PresentationDateTimeInfo, message);

				movementHeader.BM_PresentationDateTime = ZDateTimeOffset.Now;
				AssertNoMessageErrorContaining("BM_PresentationDateTime is mandatory when BM_AdditionalDeclarationType is not equal to D - BM_PresentationDateTime is not empty.", movementHeader.BM_PresentationDateTimeInfo, message);

				movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
				movementHeader.BM_PresentationDateTime = ZDateTimeOffset.Empty;
				AssertNoMessageErrorContaining("BM_PresentationDateTime is not mandatory when BM_AdditionalDeclarationType is not equal to D.", movementHeader.BM_PresentationDateTimeInfo, message);
			}
		}

		public void TestCheckRuleNat103()
		{
			const string messageError = "[NAT103] Date Limit cannot be empty.";
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var departureMovement = nctsHeader.MovementHeader;

			CombineAssertions(() =>
			{
				using (var deciderTestContext = new EU.NCTS.Business.Testing.MovementHeaderValidationDeciderTestContext<IFRNctsDepartureMovementHeaderPhase5ValidationDecider>(Factory))
				{
					deciderTestContext.EnableRule(c => c.IsRuleNAT103Active);
					var validation = departureMovement.Validation;

					departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
					departureMovement.BM_ExportDate = ZDateTime.Empty;
					departureMovement.IsSimplifiedNctsProcedure = true;
					validation.ValidateBM_ExportDate();
					AssertHasMessageError("Export date cannot be empty irrespective of IsSimplifiedNctsProcedure or AdditionalDeclarationType", departureMovement.BM_ExportDateInfo, messageError);

					departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
					departureMovement.BM_ExportDate = ZDateTime.Empty;
					departureMovement.IsSimplifiedNctsProcedure = false;
					validation.ValidateBM_ExportDate();
					AssertHasMessageError("Export date cannot be empty irrespective of IsSimplifiedNctsProcedure or AdditionalDeclarationType", departureMovement.BM_ExportDateInfo, messageError);

					departureMovement.BM_ExportDate = ZDateTime.Now;
					validation.ValidateBM_ExportDate();
					AssertNoMessageError("No error mesage if export date is not empty", departureMovement.BM_ExportDateInfo, messageError);
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departureMovement = nctsHeader.MovementHeader;
		}
		NctsHeader nctsHeader;
		NctsDepartureMovementHeader departureMovement;
	}
}
