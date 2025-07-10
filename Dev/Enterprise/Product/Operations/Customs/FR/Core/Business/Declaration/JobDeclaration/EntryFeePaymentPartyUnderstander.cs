using CargoWise.Types;
using Enterprise.Integration;
using static Enterprise.Customs.Business.UniversalReferenceConstants;
using Eu = Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class EntryFeePaymentPartyUnderstander : Eu.EntryFeePaymentPartyUnderstander
	{
		public EntryFeePaymentPartyUnderstander(Eu.JobDeclaration dec)
			: base(dec)
		{ }

		public override bool ShouldBrokerPayThisFee(string feeCode, string methodOfPayment, ILogger logger)
		{
			var result = false;
			switch (methodOfPayment)
			{
				case FRConstants.MethodOfPayment._1:
					result = ShouldBrokerPayThisFeeIfMethodsOfPaymentIs1(feeCode, methodOfPayment, logger);
					break;
				case FRConstants.MethodOfPayment._2:
					result = ShouldBrokerPayThisFeeIfMethodsOfPaymentIs2(feeCode, methodOfPayment, logger);
					break;
				case FRConstants.MethodOfPayment._6:
					result = ShouldBrokerPayThisFeeIfMethodsOfPaymentIs6(feeCode, methodOfPayment, logger);
					break;
				default:
					result = ShouldBrokerPayThisFeeIfMethodsOfPaymentIsTheOthers(feeCode, methodOfPayment, logger);
					break;
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Log strings")]
		bool ShouldBrokerPayThisFeeIfMethodsOfPaymentIs1(string fee, string methodOfPayment, ILogger logger)
		{
			var isVat = fee == RefCusRateTypes.Vat;
			var defermentAccountNumber = Declaration.JE_DefermentAccountNumber;
			var paymentMethodOfDec = Declaration.JE_PaymentMethod;
			var matchingDAN = CheckDeclarantHasCustomsRegNo(Enterprise.MasterFiles.Business.OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, defermentAccountNumber);
			var result = paymentMethodOfDec == MethodOfPaymentList.Codes.R && matchingDAN.IsAccountOwnedByDeclarant;

			if (isVat)
			{
				logger?.Log(LogType.Information, "Fee " + fee + " with 1MoP=" + methodOfPayment + " is always included in rating");
			}
			else if (paymentMethodOfDec.IsEmpty)
			{
				logger?.Log(LogType.Information, "Fee " + fee + " with 1MoP=" + methodOfPayment + " and no 2MoP is always exclude in rating");
			}
			else
			{
				logger?.Log(LogType.Information, "Fee " + fee + " with 1MoP=" + methodOfPayment + ", 2MoP=" + paymentMethodOfDec + matchingDAN.Log + ", " + (result ? "in" : "ex") + "clude in rating");
			}

			return isVat || result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Log strings")]
		bool ShouldBrokerPayThisFeeIfMethodsOfPaymentIs2(string fee, string methodOfPayment, ILogger logger)
		{
			var paymentMethodOfDec = Declaration.JE_PaymentMethod;
			var paymentMethodOfDecLog = paymentMethodOfDec.IsEmpty ? " and no 2MoP" : (" and 2MoP=" + paymentMethodOfDec);
			var result = (paymentMethodOfDec == MethodOfPaymentList.Codes.A || paymentMethodOfDec == MethodOfPaymentList.Codes.M);
			var isVat = fee == RefCusRateTypes.Vat;

			if (isVat)
			{
				logger?.Log(LogType.Information, "Fee " + fee + " with 1MoP=" + methodOfPayment + " is always included in rating");
			}
			else if (result)
			{
				logger?.Log(LogType.Information, "Fee " + fee + ", 1MoP=" + methodOfPayment + paymentMethodOfDecLog + " is always paid by broker - include in rating");
			}
			else
			{
				logger?.Log(LogType.Information, "Fee " + fee + ", 1MoP=" + methodOfPayment + paymentMethodOfDecLog + ", exclude in rating");
			}
			return isVat || result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Log strings")]
		bool ShouldBrokerPayThisFeeIfMethodsOfPaymentIs6(string fee, string methodOfPayment, ILogger logger)
		{
			var vatDeferNumber = Declaration.ZG_VATDeferNumber;
			var matchingALT = CheckDeclarantHasCustomsRegNo(Enterprise.MasterFiles.Business.OrgCusCode.FranceCodeTypes.ALT, vatDeferNumber);
			var isVat = fee == RefCusRateTypes.Vat;
			var result = isVat && matchingALT.IsAccountOwnedByDeclarant;
			if (isVat)
			{
				logger?.Log(LogType.Information, "Fee " + fee + " with MoP=" + methodOfPayment + matchingALT.Log + ", " + (result ? "in" : "ex") + "clude in rating");
			}
			else
			{
				logger?.Log(LogType.Information, "Fee " + fee + " with MoP=" + methodOfPayment + " is always exclude in rating");
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Log strings")]
		bool ShouldBrokerPayThisFeeIfMethodsOfPaymentIsTheOthers(string fee, string methodOfPayment, ILogger logger)
		{
			logger?.Log(LogType.Information, "Fee " + fee + " with MoP=" + methodOfPayment + " is always exclude for autorating");
			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Log strings")]
		(bool IsAccountOwnedByDeclarant, string Log) CheckDeclarantHasCustomsRegNo(ZString codeType, ZString regNo)
		{
			var isAccountOwnedByDeclarant = false;
			var log = " and no " + codeType;
			if (!regNo.IsEmpty)
			{
				if (Declaration.Declarant != null && Declaration.Declarant.Header.CustomsCodes.GetCustomsRegNo(codeType, Core.Constants.CountryCodes.France) == regNo)
				{
					isAccountOwnedByDeclarant = true;
					log = " and matching Declarant's " + codeType + "=" + regNo;
				}
				else if (Declaration.Importer != null && Declaration.Importer.CustomsCodes.GetCustomsRegNo(codeType, Core.Constants.CountryCodes.France) == regNo)
				{
					isAccountOwnedByDeclarant = false;
					log = " but matching Importer's " + codeType + "=" + regNo;
				}
				else
				{
					log = " and no matching " + codeType + "=" + regNo + " in Declarant and Importer";
				}
			}
			return (isAccountOwnedByDeclarant, log);
		}
	}
}
