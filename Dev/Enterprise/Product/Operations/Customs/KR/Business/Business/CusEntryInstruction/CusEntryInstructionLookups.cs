using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class CusEntryInstructionLookups : Customs.Business.CusEntryInstructionLookups
	{
		public CusEntryInstructionLookups(CusEntryInstruction parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList BondedFactoryUseCodeList => Factory.GetCachedValue<BondedFactoryUseCodeList>();

		public CodeDescriptionPairList AgreedRateList => Factory.GetCachedValue<YesNoList>();

		public CodeDescriptionPairList FTARelationArticleCodeList => Factory.GetCachedValue<FTALawCodeList>();

		public CodeDescriptionPairList RefundTypeList => Factory.GetCachedValue<RefundTypeList>();

		public CodeDescriptionPairList RefundCauseCodeList => Factory.GetCachedValue<RefundCauseCodeList>();

		public CodeDescriptionPairList RefundReasonCodeList => Factory.GetCachedValue<RefundReasonCodeList>();

		public CodeDescriptionPairList StatementNumber5WNList
		{
			get
			{
				var entry = Parent.EntryHeader;
				return Factory.GetCachedValue("StatementNumber5WNList" + Parent.PK, () =>
				{
					var result = new CodeDescriptionPairList();
					if (entry != null)
					{
						foreach (var amendmentSessionalData in Parent.AmendmentSessionalDataCollection)
						{
							if (amendmentSessionalData.RefundSessionalData != null)
							{
								var statementNumber = amendmentSessionalData.RefundSessionalData.CustomsDisbursementBill;
								var matchingStatement = entry.StatementLines.FirstOrDefault(x => x.IndividualCustomsDisbursementBillNo == statementNumber);

								if (matchingStatement != null && !result.ContainsCode(statementNumber))
								{
									result.AddPair(statementNumber, matchingStatement.StatementNumber5WNDescription);
								}
							}
						}
						foreach (var statementLine in entry.StatementLines)
						{
							if (statementLine.StatementHeader.B2_RMNumber != ZString.Empty && !result.ContainsCode(statementLine.IndividualCustomsDisbursementBillNo))
							{
								result.AddPair(statementLine.StatementHeader.B2_StatementNumber, statementLine.StatementNumber5WNDescription);
							}
						}
					}
					return result;
				});
			}
		}

		public new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;
	}
}
