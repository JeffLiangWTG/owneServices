using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Integration;
using Eu = Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class EntryFeePaymentPartyUnderstander : Eu.EntryFeePaymentPartyUnderstander
	{
		public EntryFeePaymentPartyUnderstander(Eu.JobDeclaration dec, CusEntryHeader header)
			: base(dec)
		{
			this.header = header;
		}

		public override bool ShouldBrokerPayThisFee(string feeCode, string methodOfPayment, ILogger logger)
		{
			var alwayBroker = Declaration.Factory.GetCachedValue("GB.EntryFeePaymentPartyUnderstander.AlwaysBroker", delegate
			{
				return Declaration.Factory.GetCachedListMatchAllAttributes(
					Declaration.GetDefaultDataGroupingCode(),
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment,
					new[] { new KeyValuePair<ZString, ZString>(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.AutoRating, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Autorating_AlwaysBroker) },
					Universal.RefCusCodeListTypes.IncludeParentDataGroupingOptions.ChildOnly);
			});

			if (alwayBroker.ContainsCode(methodOfPayment)) //"D"  or "P" > FAS
			{
				// Business rule: “The FAS account money always comes out of  the FAS account attached to the badge/role”  - see eDocs on WI.
				logger?.Log(LogType.Information, "Fee " + feeCode + " with MoP=" + methodOfPayment + " is always paid by broker - include in rating");
				return true;
			}

			var basedOnBox48 = Declaration.Factory.GetCachedValue("GB.EntryFeePaymentPartyUnderstander.SometimesBrokerSeeBox48", delegate
			{
				return Declaration.Factory.GetCachedListMatchAllAttributes(
					Declaration.GetDefaultDataGroupingCode(),
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment,
					new[] { new KeyValuePair<ZString, ZString>(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.AutoRating, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Autorating_SometimesBrokerSeeBox48) },
					Universal.RefCusCodeListTypes.IncludeParentDataGroupingOptions.ChildOnly);
			});
			if (basedOnBox48.ContainsCode(methodOfPayment)) //F, Q >  deferred
			{
				return ShouldBrokerPayThisDeferredFee(feeCode, methodOfPayment, logger);
			}

			var basedOnDataElement83 = Declaration.Factory.GetCachedValue("GB.EntryFeePaymentPartyUnderstander.SeeDataElement83", delegate
			{
				return Declaration.Factory.GetCachedListMatchAllAttributes(
					Declaration.GetDefaultDataGroupingCode(),
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment,
					new[] { new KeyValuePair<ZString, ZString>(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.AutoRating, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Autorating_SeeDataElement83) },
					Universal.RefCusCodeListTypes.IncludeParentDataGroupingOptions.ChildOnly);
			});
			if (basedOnDataElement83.ContainsCode(methodOfPayment))
			{
				return IsBrokersOwnCashGuaranteeAccount(feeCode, methodOfPayment, logger);
			}

			return base.ShouldBrokerPayThisFee(feeCode, methodOfPayment, logger);
		}

		public bool ShouldBrokerPayThisDeferredFee(string fee, string methodOfPayment, ILogger logger)
		{
			var deferredToDeclarant = EU.Business.DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14; // A
			var firstDanType = Declaration.JE_PaymentMethod;
			var secondDanType = Declaration.ZG_VATDeferType;

			// *** For an explanation of deferment in the UK, see here:  Intranet > Development > Team Discussion > Deferment in the UK - DAN  
			if (fee == Enterprise.Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Vat)
			{
				if (secondDanType.IsEmpty && firstDanType.IsEmpty)
				{
					// No deferment - broker pays
					logger?.Log(LogType.Information, $"Deferred Fee {fee} with no deferment, with MoP={methodOfPayment}, include for autorating");
					return true;
				}

				if (secondDanType.IsEmpty && !firstDanType.IsEmpty)
				{
					// First DAN defined, second blank, so VAT (second) inherits from first: VAT is deferred to the FIRST dan.
					var isRated = firstDanType == deferredToDeclarant;
					logger?.Log(LogType.Information, $"Deferred Fee {fee} with 1DAN={firstDanType} and no 2DAN, with MoP={methodOfPayment}, {(isRated ? "in" : "ex")}clude for rating");
					return isRated;
				}

				if (!secondDanType.IsEmpty && !firstDanType.IsEmpty)
				{
					// VAT is deferred to the second dan, which is explicitly defined.
					var isRated = secondDanType == deferredToDeclarant;
					logger?.Log(LogType.Information, $"Deferred Fee {fee} with 1DAN={firstDanType} and 2DAN={secondDanType}, with MoP={methodOfPayment}, {(isRated ? "in" : "ex")}clude for rating");
					return isRated;
				}
			}
			else
			{
				// Non-VAT taxes
				var isRated = firstDanType == deferredToDeclarant || firstDanType.IsEmpty;
				logger?.Log(LogType.Information, $"Deferred Fee {fee} with 1DAN={firstDanType}, with MoP={methodOfPayment}, {(isRated ? "in" : "ex")}clude for rating");
				return isRated;
			}

			return false;
		}

		public bool IsBrokersOwnCashGuaranteeAccount(string fee, string methodOfPayment, ILogger logger)
		{
			var cashGuarantees = Declaration.Guarantees.OfType<GBGuarantee>().Where(x => x.PW_Password == GbConstants.CdsCashAccountGuaranteeType && x.IsRelatedToEntryInstruction(header?.EntryInstruction));
			if (!cashGuarantees.Any())
			{
				logger?.Log(LogType.Information, $"Fee {fee} with MoP={methodOfPayment} excluded because no cash guarantee is given");
				return false;
			}
			var guaranteeNumbers = cashGuarantees.SelectMany(GetGuaranteeNumbers).Distinct().ToArray();
			if (guaranteeNumbers.Length != 1)
			{
				logger?.Log(LogType.Information, $"Fee {fee} with MoP={methodOfPayment} excluded because {(guaranteeNumbers.Length == 0 ? "no" : "multiple potential")} values for cash guarantee EORI were found");
				return false;
			}

			var eori = (Declaration as JobDeclaration)?.GetEori() ?? ZString.Empty;
			var isOwnAccount = guaranteeNumbers[0] == eori;

			logger?.Log(LogType.Information, $"Fee {fee} with MoP={methodOfPayment} {(isOwnAccount ? "in" : "ex")}cluded because the submitter's EORI {eori} {(isOwnAccount ? "matches" : "does not match")} the guarantee {guaranteeNumbers[0]}");
			return isOwnAccount;
		}

		IEnumerable<ZString> GetGuaranteeNumbers(GBGuarantee guarantee)
		{
			if (!guarantee.PW_BondNumber.IsEmpty)
			{
				yield return guarantee.PW_BondNumber;
			}
			if (!guarantee.PW_BondNumber2.IsEmpty && guarantee.PW_BondNumber2 != guarantee.PW_BondNumber)
			{
				yield return guarantee.PW_BondNumber2;
			}
			if (!guarantee.PW_HolderIdentification.IsEmpty && guarantee.PW_HolderIdentification != guarantee.PW_BondNumber && guarantee.PW_HolderIdentification != guarantee.PW_BondNumber2)
			{
				yield return guarantee.PW_HolderIdentification;
			}
		}

		readonly CusEntryHeader header;
	}
}
