using System.Collections.Immutable;
using System.Linq;
using CargoWise.Application;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using static Enterprise.Customs.IE.Business.UniversalReferenceConstants;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.Customs.IE.Business.Declaration
{
	sealed class ImportJobComInvoiceHeaderValidation : CommonJobComInvoiceHeaderValidation
	{
		public ImportJobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader) : base(invoiceHeader)
		{
		}

		protected override TypeOfValidationForMissingMandatoryChargesForIncoterm ValidationForMissingMandatoryCharges =>
			Parent.IsRuleBR4010Active ? TypeOfValidationForMissingMandatoryChargesForIncoterm.MessageError : TypeOfValidationForMissingMandatoryChargesForIncoterm.None;

		protected override void CheckJZ_OA_ExporterAddress()
		{
			base.CheckJZ_OA_ExporterAddress();
			CheckRuleBR3013();
		}

		void CheckRuleBR3013()
		{
			var parent = Parent;
			if (parent.ExporterAddress != null && !parent.InvoiceLines.Cast<JobComInvoiceLine>().All(line => line.AdditionalInfos.Cast<AdditionalInfo>().Any(info => info.IsAnAdditionalInformation && info.CSI_Code == Constants.AdditionalInformationCodes._00200)))
			{
				parent.JZ_OA_ExporterAddressInfo.AddMessageError(Res.GetString("6D293F35-0C3E-4E48-BB28-6425F65AC831", "[BR3013] Please enter an Additional Information code where Kind is 'INF' and Full Type is '00200' to the Invoice Lines > Additional Documents grid for all invoice lines under this invoice."));
			}
		}

		protected override void CheckJZ_IncoTerm()
		{
			base.CheckJZ_IncoTerm();
			CheckRuleBR4010();
			CheckRuleBR4014();
			CheckRuleBR4016();
		}

		void CheckRuleBR4010()
		{
			if (Parent.IsRuleBR4010Active)
			{
				var incoTermsToVerify = new ZString[] { Core.Constants.IncoTerms.ExWorks, Core.Constants.IncoTerms.FreeCarrier, Core.Constants.IncoTerms.FreeAlongsideShip, Core.Constants.IncoTerms.FreeOnBoard, };

				CheckForGivenIncoTermsIfChargesAmountIsGreaterThanZero(incoTermsToVerify, AISChargeCodeList.Codes.AK, Res.GetString("F30D6320-02A0-4D6E-ACE5-2341EBC46C79", "[BR4010] When Incoterm is EXW, FCA, FAS or FOB, a charge of type AK is required and must be greater than 0."));

				var br4010Message = Res.GetString("2B2AFB20-F208-48EC-9B3A-6D850491F2CD", "[BR4010] When Incoterm is EXW, FCA, FAS or FOB, a charge of type 1X is required and the amount for 1X must be equal to the amount for BA.");
				CheckForValidIncoTermsAndCharges(incoTermsToVerify, new ZString[] { AISChargeCodeList.Codes._1X, AISChargeCodeList.Codes.BA }, br4010Message);

				CheckIncoTermIfChargesArePresentForGivenCodes(incoTermsToVerify, mandatoryChargeCode: AISChargeCodeList.Codes.BA, validationMessageError: Res.GetString("7A2C9AFE-F1DF-4D7F-8505-A663FD33FEDA", "[BR4010] When Incoterm is EXW, FCA, FAS or FOB, a charge of type BA is required."));
			}
		}

		void CheckRuleBR4014()
		{
			var parent = Parent;
			var agreedPlaceCode = parent.ZG_AgreedPlaceCode;
			if (parent.JZ_IncoTerm == Core.Constants.IncoTerms.CostInsuranceAndFreight && !agreedPlaceCode.IsEmpty && !IsAgreedPlaceCodeStartsWithEUCountryCode(agreedPlaceCode))
			{
				parent.JZ_IncoTermInfo.AddMessageError(Res.GetString("C0268A58-1641-4E6B-8E35-FCB89292269C", "[BR4014] When Incoterm is CIF, Agreed Place Code must be inside the EU."));
			}
		}

		void CheckRuleBR4016()
		{
			var parent = Parent;
			if (parent.JZ_IncoTerm == Core.Constants.IncoTerms.DeliveredDutyPaid
				&& parent.RequestedProcedures.Overlaps([ProcedureCodes.ProcedureCode._51, ProcedureCodes.ProcedureCode._53])
			)
			{
				parent.JZ_IncoTermInfo.AddMessageError(Res.GetString("90680176-98DE-4098-B2D0-8FA388F6D3FB", "[BR4016] Incoterm cannot be DDP when Requested Procedure is 51 or 53."));
			}
		}

		void CheckIncoTermIfChargesArePresentForGivenCodes(ZString[] incoTermsToVerify, ZString mandatoryChargeCode, string validationMessageError)
		{
			var parent = Parent;
			var incoTerm = parent.JZ_IncoTerm;
			if (incoTerm.IsEmpty || !incoTerm.In(incoTermsToVerify))
			{
				return;
			}

			var chargeCodes = parent.Charges.Cast<InvoiceCharge>().Select(c => c.J7_ChargeType);
			var groupChargeCodes = parent.GroupCharges.Select(c => c.J7_ChargeType);
			var allChargeCodes = chargeCodes.Union(groupChargeCodes).ToArray();
			if (allChargeCodes.All(c => c != mandatoryChargeCode))
			{
				parent.JZ_IncoTermInfo.AddMessageError(validationMessageError);
			}
		}

		void CheckForGivenIncoTermsIfChargesAmountIsGreaterThanZero(ZString[] incoTermsToVerify, ZString chargeCode, string validationMessageError)
		{
			var parent = Parent;
			var incoTerm = parent.JZ_IncoTerm;
			if (incoTerm.IsEmpty || !incoTerm.In(incoTermsToVerify))
			{
				return;
			}

			if (!parent.Charges.Cast<InvoiceCharge>().Any(c => c.J7_ChargeType == chargeCode && c.J7_Amount > 0) && !parent.GroupCharges.Cast<InvoiceApportionCharge>().Any(c => c.J7_ChargeType == chargeCode && c.J7_Amount > 0))
			{
				parent.JZ_IncoTermInfo.AddMessageError(validationMessageError);
			}
		}

		void CheckForValidIncoTermsAndCharges(ZString[] incoTermsToVerify, ZString[] chargeCodes, string validationMessageError)
		{
			var parent = Parent;
			var incoTerm = parent.JZ_IncoTerm;
			if (incoTerm.IsEmpty || !incoTerm.In(incoTermsToVerify))
			{
				return;
			}

			if (!parent.Charges.Cast<InvoiceCharge>().Any(c => chargeCodes.Contains(c.J7_ChargeType) && c.J7_Amount >= 0) && !parent.GroupCharges.Cast<InvoiceApportionCharge>().Any(c => chargeCodes.Contains(c.J7_ChargeType) && c.J7_Amount >= 0))
			{
				parent.JZ_IncoTermInfo.AddMessageError(validationMessageError);
			}
		}

		bool IsAgreedPlaceCodeStartsWithEUCountryCode(ZString agreedPlaceCode)
		{
			if (agreedPlaceCode.Length < 2)
			{
				return false;
			}

			var euMemberProvider = ObjectFactory.Get<IEuropeanUnionCustomsMembersProvider>();
			return euMemberProvider.IsMemberOfEU(agreedPlaceCode.Substring(0, 2));
		}

		protected override bool IncoTermRequired => !((Parent.JobDeclaration?.IsUCC5 ?? false) && Parent.CusEntryInstructions.Cast<CusEntryInstruction>().All(x => x.IsH2 || x.IsI1));

		protected override void CheckJZ_ValuationCode()
		{
			base.CheckJZ_ValuationCode();
			CheckRuleBR8051();
			CheckRuleCD8051();
		}

		void CheckRuleBR8051()
		{
			var parent = Parent;
			switch (parent.JZ_ValuationCode)
			{
				case NatureOfTransactionList.Codes._51:
				case NatureOfTransactionList.Codes._52:
					// do nothing
					break;
				default:
					if (parent.RequestedProcedures.Contains(ProcedureCodes.ProcedureCode._61) && parent.HasInvoiceLineWithPreviousProcedure21Or22)
					{
						parent.JZ_ValuationCodeInfo.AddMessageError(Res.GetString("48c2b4de-91f6-42aa-848f-2da99dd51dcd", "[BR8051] Nature of Transaction must be 51 (same original export country) or 52 (different original export country) when Requested Procedure is 61 and Previous Procedure is 21 or 22."));
					}
					break;
			}
		}

		void CheckRuleCD8051()
		{
			var parent = Parent;
			if (parent.JZ_ValuationCode.IsEmpty
				&& ValidationDecider is IRuleCD8051ForJZ_ValuationCodeDecider valuationDecider && valuationDecider.IsActive(parent)
				&& parent.RequestedProcedures.Overlaps([ProcedureCodes.ProcedureCode._51, ProcedureCodes.ProcedureCode._61])
			)
			{
				parent.JZ_ValuationCodeInfo.AddMessageError(Res.GetString("76B7A870-3CEA-4D44-B786-B5AF040844F5", "[CD8051] Nature of Transaction is required when Requested Procedure is 51 or 61."));
			}
		}

		protected override bool IsJZ_ValuationCodeMandatory => false;

		protected override void CheckJZ_RX_NKInvoice_Currency()
		{
			base.CheckJZ_RX_NKInvoice_Currency();

			CheckRuleBR6142();
		}

		void CheckRuleBR6142()
		{
			var parent = Parent;

			if (!parent.JZ_RX_NKInvoice_Currency.IsEmpty &&
				parent.JZ_RX_NKInvoice_Currency != Core.Constants.CurrencyCodes.EuropeanUnion &&
				parent.CusEntryInstructions.Cast<CusEntryInstruction>().Any(x => x.IsI1EntryAndTotalPriceLessThanOrEqualTo22EUR))
			{
				parent.JZ_RX_NKInvoice_CurrencyInfo.AddMessageError(Res.GetString("9F42F2A0-FD6B-4A7D-A7C0-C7548A93245C", "[BR6142] Invoice Currency must be 'EUR' when Declaration Type is I1 and Lines Price sharing same instruction less than €22."));
			}
		}
	}
}
