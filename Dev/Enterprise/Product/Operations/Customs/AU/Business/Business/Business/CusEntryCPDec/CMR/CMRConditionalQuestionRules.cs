using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class CMRConditionalQuestionRules
	{
		public CMRConditionalQuestionRules(CMRCusEntryCPDec parent, ZInt cPDecNum)
		{
			messageTypeRuleDefinition = "?";
			pUPRuleDefinition = ZString.Empty;
			lowValueShipmentRuleDefinition = ZString.Empty;
			natureRuleDefinition = ZString.Empty;
			section70RuleDefinition = ZString.Empty;
			fCLRuleDefinition = ZString.Empty;
			lCLRuleDefinition = ZString.Empty;
			quotedRuleDefinition = ZString.Empty;
			deliveredRuleDefinition = ZString.Empty;
			refundReasonRuleDefinition = ZString.Empty;
			ownersAccountRuleDefinition = ZString.Empty;
			paidRuleDefinition = ZString.Empty;
			aTDRuleDefinition = ZString.Empty;
			gSTDeferredRuleDefinition = ZString.Empty;
			refundValueRuleDefinition = ZString.Empty;

			this.parent = parent;
			entryHeaderAttachee = this.parent.EntryHeader;
			string stringValue = cPDecNum.ToString();
			for (int i = 0; i <= ruleDefs.GetUpperBound(0); i++)
			{
				if (ruleDefs[i, 0] == stringValue)
				{
					messageTypeRuleDefinition = ruleDefs[i, 1];
					pUPRuleDefinition = ruleDefs[i, 2];
					lowValueShipmentRuleDefinition = ruleDefs[i, 3];
					natureRuleDefinition = ruleDefs[i, 4];
					section70RuleDefinition = ruleDefs[i, 5];
					fCLRuleDefinition = ruleDefs[i, 6];
					lCLRuleDefinition = ruleDefs[i, 7];
					quotedRuleDefinition = ruleDefs[i, 8];
					deliveredRuleDefinition = ruleDefs[i, 9];
					refundReasonRuleDefinition = ruleDefs[i, 10];
					ownersAccountRuleDefinition = ruleDefs[i, 11];
					paidRuleDefinition = ruleDefs[i, 12];
					aTDRuleDefinition = ruleDefs[i, 13];
					gSTDeferredRuleDefinition = ruleDefs[i, 14];
					refundValueRuleDefinition = ruleDefs[i, 15];

					break;
				}
			}
		}
		readonly CMRCusEntryCPDec parent;
		readonly ICPQAHeaderAttachee entryHeaderAttachee;

		readonly ZString messageTypeRuleDefinition;
		readonly ZString pUPRuleDefinition;
		readonly ZString lowValueShipmentRuleDefinition;
		readonly ZString natureRuleDefinition;
		readonly ZString section70RuleDefinition;
		readonly ZString fCLRuleDefinition;
		readonly ZString lCLRuleDefinition;
		readonly ZString quotedRuleDefinition;
		readonly ZString deliveredRuleDefinition;
		readonly ZString refundReasonRuleDefinition;
		readonly ZString ownersAccountRuleDefinition;
		readonly ZString paidRuleDefinition;
		readonly ZString aTDRuleDefinition;
		readonly ZString gSTDeferredRuleDefinition;
		readonly ZString refundValueRuleDefinition;

		readonly ZString[,] ruleDefs =
		  {
{ "1", "L", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
{ "2", "LA", "Y", "", "", "", "", "", "", "", "", "", "", "", "", "" },
{ "3", "LA", "", "Y", "*N10*N20*N10/N20", "", "", "", "", "", "", "", "", "", "", "" },
{ "4", "A", "", "", "*N10*N20*N10/N20", "Y", "", "", "", "", "", "", "", "", "", "" },
{ "5", "LA", "", "", "", "", "", "", "Y", "", "", "", "", "", "", "" },
{ "6", "LA", "", "", "", "", "Y", "", "", "N", "", "", "", "", "", "" },
{ "7", "LA", "", "", "", "", "Y", "", "", "N", "", "", "", "", "", "" },
{ "8", "LA", "", "", "", "", "", "Y", "", "N", "", "", "", "", "", "" },
{ "9", "LA", "", "", "", "", "", "Y", "", "N", "", "", "", "", "", "" },
{ "10", "AW", "", "", "", "", "", "", "", "", "", "N", "Y", "", "", "Y" },
{ "11", "A", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
{ "12", "W", "", "", "", "", "", "", "", "", "", "", "", "Y", "", "" },
{ "13", "W", "", "", "", "", "", "", "", "", "", "", "N", "", "", "" },
{ "14", "AW", "", "", "", "", "", "", "", "", "", "", "Y", "", "", "" },
{ "15", "AW", "", "", "*N10*N10/N20*N30", "", "", "", "", "", "", "", "Y", "", "N", "" },
{ "16", "S", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
{ "17", "S", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
{ "18", "S", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
{ "19", "S", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
{ "282", "A", "", "", "", "", "", "", "", "", "Y", "", "Y", "", "", "" },
{ "326", "X", "", "", "", "", "", "", "", "", "Y", "Y", "", "", "", "Y" },
{ "375", "X", "", "", "", "", "", "", "", "", "", "", "", "", "", "" }
			};

		LodgementQuestionKeys LodgementQuestionKey
		{
			get
			{
				if (!fLodgementQuestionKeyDone && entryHeaderAttachee != null)
				{
					fLodgementQuestionKey = entryHeaderAttachee.LodgementQuestionKey;
					fLodgementQuestionKeyDone = true;
				}
				return fLodgementQuestionKey;
			}
		}
		LodgementQuestionKeys fLodgementQuestionKey;
		bool fLodgementQuestionKeyDone;

		public void ResetCachedProperties()
		{
			fLodgementQuestionKeyDone = false;
		}

		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = parent.Declaration;
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;

		CusEntryHeader EntryHeader
		{
			get
			{
				if (fEntryHeader == null)
				{
					fEntryHeader = parent.EntryHeader;
				}
				return fEntryHeader;
			}
		}
		CusEntryHeader fEntryHeader;

		[AttributeUsage(AttributeTargets.Property)]
		public sealed class DefinedRule : Attribute
		{
		}

		public bool IsConditional
		{
			get
			{
				bool result = true;
				if (EntryHeader != null)
				{
					foreach (PropertyInfo ruleProperty in typeof(CMRConditionalQuestionRules).GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
					{
						if (ruleProperty.GetCustomAttributes(typeof(DefinedRule), false).Length > 0)
						{
							result = (bool)ruleProperty.GetValue(this, null);
							if (!result)
							{
								break;
							}
						}
					}
				}
				return !result;
			}
		}

		public delegate bool DelayedDelegate();

		bool GetRuleValue(ZString ruleDefinition, DelayedDelegate ruleDelegate)
		{
			bool result = true;
			if (!ruleDefinition.IsEmpty && Declaration != null && EntryHeader != null)
			{
				result = ruleDelegate();
			}
			if (ruleDefinition == "N")
			{
				result = !result;
			}
			return result;
		}

		[DefinedRule]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used in CMRConditionalQuestionRulesTest")]
		bool IsRequiredMessageType
		{
			get
			{
				bool result = true;
				if (!messageTypeRuleDefinition.IsEmpty && Declaration != null)
				{
					string messageType = "A";

					if (parent.GeneratorMessageType == MessageTypesForCPQAGenerator.Withdraw)
					{
						messageType = "W";
					}
					else if (LodgementQuestionKey.IsSAC)
					{
						messageType = "S";
					}
					else if (LodgementQuestionKey.IsSACWithLine)
					{
						messageType = "L";
					}
					else if (EntryHeader != null)
					{
						messageType = EntryHeader.IsStatusPostLodge ? "A" : "L";
					}
					result = messageTypeRuleDefinition.Contains(messageType);
				}
				return result;
			}
		}

		[DefinedRule]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used in CMRConditionalQuestionRulesTest")]
		bool IsRequiredNature
		{
			get
			{
				bool result = true;
				if (!natureRuleDefinition.IsEmpty && Declaration != null && EntryHeader != null)
				{
					result = natureRuleDefinition.Contains("*" + EntryHeader.GetNatureForImportCMR());
				}
				return result;
			}
		}

		[DefinedRule]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used in CMRConditionalQuestionRulesTest")]
		bool IsPUPSpecified => GetRuleValue(pUPRuleDefinition, () => LodgementQuestionKey.IsPaidUnderProtest);

		[DefinedRule]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used in CMRConditionalQuestionRulesTest")]
		bool IsLowValueShipment => GetRuleValue(lowValueShipmentRuleDefinition, () => LodgementQuestionKey.TotalCustomsValue <= Deminimus);

		[DefinedRule]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used in CMRConditionalQuestionRulesTest")]
		bool IsSection70 => GetRuleValue(section70RuleDefinition, () => false);  // always not mandatory (ie conditional)

		[DefinedRule]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used in CMRConditionalQuestionRulesTest")]
		bool IsWETorLCTQuoted => GetRuleValue(quotedRuleDefinition, () => LodgementQuestionKey.IsABNQuotedForLCTAndWET);

		[DefinedRule]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used in CMRConditionalQuestionRulesTest")]
		bool HasFCLContainer => GetRuleValue(fCLRuleDefinition, () => LodgementQuestionKey.IsSea && LodgementQuestionKey.HasFCLOrFCXLines);

		[DefinedRule]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used in CMRConditionalQuestionRulesTest")]
		bool HasLCLContainer => GetRuleValue(lCLRuleDefinition, () => LodgementQuestionKey.IsSea && LodgementQuestionKey.HasLCLLines);

		[DefinedRule]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used in CMRConditionalQuestionRulesTest")]
		bool IsShipmentDelivered => GetRuleValue(deliveredRuleDefinition, () => EntryHeader.IsGoodsDelivered);

		[DefinedRule]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used in CMRConditionalQuestionRulesTest")]
		bool IdRefundReasonSpecifiedOnOneOrMoreLines => GetRuleValue(refundReasonRuleDefinition, () => LodgementQuestionKey.IsRefundAmendment);

		[DefinedRule]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used in CMRConditionalQuestionRulesTest")]
		bool IsRefundToOwnersAccount
		{
			get
			{
				return GetRuleValue(ownersAccountRuleDefinition, () =>
				{
					var payMethod = Declaration.JE_PaymentMethod == JobDeclaration.PaymentMethods.Default
						? PaymentDetailRetriever.PartyToPayString()
						: Declaration.JE_PaymentMethod;
					return payMethod != JobDeclaration.PaymentMethods.Broker;
				});
			}
		}

		PaymentDetailRetriever PaymentDetailRetriever
		{
			get
			{
				if (fPaymentDetailRetriever == null)
				{
					fPaymentDetailRetriever = new PaymentDetailRetriever(Declaration);
				}
				return fPaymentDetailRetriever;
			}
		}
		PaymentDetailRetriever fPaymentDetailRetriever;

		[DefinedRule]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used in CMRConditionalQuestionRulesTest")]
		bool HasEntryBeenPaid
		{
			get
			{
				return GetRuleValue(paidRuleDefinition, () =>
				{
					if (!LodgementQuestionKey.IsSACWithLine && !LodgementQuestionKey.IsNature30 && (LodgementQuestionKey.IsSAC || LodgementQuestionKey.TotalCustomsValue <= Deminimus))
					{
						return true;
					}
					else
					{
						return LodgementQuestionKey.IsPaid;
					}
				});
			}
		}

		[DefinedRule]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used in CMRConditionalQuestionRulesTest")]
		bool IsEntryPaidOrHasATDBeenReceived
		{
			get
			{
				return GetRuleValue(aTDRuleDefinition, () =>
				{
					if (!EntryHeader.ATDSecurityCode.IsEmpty ||
								(!LodgementQuestionKey.IsSACWithLine && !LodgementQuestionKey.IsNature30 && (LodgementQuestionKey.IsSAC || LodgementQuestionKey.TotalCustomsValue <= Deminimus)))
					{
						return true;
					}
					else
					{
						return LodgementQuestionKey.IsPaid;
					}
				});
			}
		}

		[DefinedRule]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used in CMRConditionalQuestionRulesTest")]
		bool IsGSTDeferred => GetRuleValue(gSTDeferredRuleDefinition, () => LodgementQuestionKey.IsGSTDeferred);

		[DefinedRule]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used in CMRConditionalQuestionRulesTest")]
		bool IsARefundExpected
		{
			get
			{
				return GetRuleValue(refundValueRuleDefinition, () =>
				{
					if (parent.GeneratorMessageType == MessageTypesForCPQAGenerator.Withdraw)
					{
						return LodgementQuestionKey.IsSACWithLine || LodgementQuestionKey.IsNature30 || LodgementQuestionKey.TotalCustomsValue > Deminimus;
					}
					else
					{
						return EntryHeader.IsARefundDue;
					}
				});
			}
		}

		ZDecimal Deminimus
		{
			get
			{
				var factory = parent == null ? new BusinessObjectFactory() : parent.Factory;
				return UniversalReferenceHelper.GetDeminimus(factory);
			}
		}
	}
}
