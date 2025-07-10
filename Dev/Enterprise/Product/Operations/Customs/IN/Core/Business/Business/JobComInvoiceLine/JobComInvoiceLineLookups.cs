using System.Linq;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using RefCusCodeListTypesCodes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;

namespace Enterprise.Customs.IN.Business;

public class JobComInvoiceLineLookups : AutoINJobComInvoiceLineLookups
{
	public JobComInvoiceLineLookups(JobComInvoiceLine parent)
		: base(parent)
	{
	}

	string AccessoryCode => InvoiceLine.IsExport ? RefCusCodeListTypesCodes.ExportAccessoryCode : RefCusCodeListTypesCodes.ImportAccessoryCode;
	public CodeDescriptionPairList AccessoryStatusList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.India, AccessoryCode, InvoiceLine.EffectiveAssessmentDate);

	public ZZRefCusCodeListCombinedCollection EndUseCodes => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.India, RefCusCodeListTypesCodes.INCustomsEndUseCode, InvoiceLine.EffectiveAssessmentDate);

	public CodeDescriptionPairList JobWorkNotificationNoList => new CodeDescriptionPairList();

	public CodeDescriptionPairList OriginStateList => Factory.GetStateList(Core.Constants.CountryCodes.India, hasErrors: false);

	public CodeDescriptionPairList RewardItemList => Factory.GetCachedValue<YesNoList>();

	public override CodeDescriptionPairList InvoiceUQList => UniversalReferenceDataHelper.GetCustomsUnitOfQuantityList(Factory, InvoiceLine.EffectiveAssessmentDate);

	public CodeDescriptionPairList UnitUQList
	{
		get
		{
			var invoiceUQ = InvoiceLine.JI_InvoiceUQ;
			var date = InvoiceLine.EffectiveAssessmentDate;
			return Factory.GetCachedValue($"INJobComInvoiceLineLookups_UnitUQList_{invoiceUQ}_{date}", () =>
			{
				var result = new CodeDescriptionPairList();
				if (!invoiceUQ.IsEmpty)
				{
					var codes = RefCusPackListProvider.GetRefCusPackList(Factory, invoiceUQ).Select(x => x.RP_CustomsPack).Append(invoiceUQ).ToArray();
					var allUQList = UniversalReferenceDataHelper.GetCustomsUnitOfQuantityList(Factory, date);
					foreach (var code in codes)
					{
						if (allUQList.GetDescriptionFromCode(code) is string desc)
						{
							result.AddPair(code, desc);
						}
					}
					result.Sort();
				}
				return result;
			});
		}
	}

	public OrgHeaderCollection OrganizationList => new OrganisationsFindBoxCollection(Factory);
}
