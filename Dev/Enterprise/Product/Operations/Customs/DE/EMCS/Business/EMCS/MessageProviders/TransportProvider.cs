using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.EU.EMCS.Business;
using CusAuthorisationRuleTypeList = Enterprise.Customs.DE.Business.CusAuthorisationRuleTypeList;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class TransportProvider : IEMCSTransport
	{
		readonly EMCSCusContainer container;
		readonly TransportProviderHelper helper;

		public TransportProvider(EMCSCusContainer container)
		{
			this.container = Argument.NotNull(container, nameof(container));
			helper = new TransportProviderHelper(container);
		}

		public string UnitCode => helper.UnitCode;

		public string IdentityOfUnit => helper.IdentityOfUnit;

		public string CommercialSealIdentification => helper.CommercialSealIdentification;

		public ITextAndLanguage ComplementaryInformation
		{
			get
			{
				if (comment == null)
				{
					var result = container.Comment;
					var declaration = container.Declaration;
					if (declaration != null && declaration.IsConsolidatedDocument())
					{
						var authorization = declaration.SupplierDocumentaryAddress.Organisation.GetAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords
							, CusAuthorisationRuleTypeList.Codes.Usage
							, new ZString[] { CusAuthorisationUsageRuleList.Codes.AccreditedExporter });
						if (authorization != null)
						{
							var authorizationInfo = authorization.CPH_Number;
							var authorizationRule = authorization.CusAuthorisationRules.FirstOrDefault(x => x.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.BusinessReference);
							var cprValue = authorizationRule?.CPR_ValueFrom ?? ZString.Empty;
							if (!cprValue.IsEmpty)
							{
								authorizationInfo += " " + cprValue;
							}

							result = (authorizationInfo + ". " + result).Trim();
						}
					}
					comment = new TextAndLanguageProvider(result);
				}
				return comment;
			}
		}
		ITextAndLanguage comment;

		public ITextAndLanguage SealInformation => sealInformation ?? (sealInformation = new TextAndLanguageProvider(container.SealDetails));
		ITextAndLanguage sealInformation;
	}
}
