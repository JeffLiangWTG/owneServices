using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business;

public class CusLineTariffDetailCollection : CusLineTariffDetailCollection<CusLineTariffDetail>
{
	public CusLineTariffDetailCollection(JobComInvoiceLine parent, ZString type) : base(parent)
	{
		this.type = Argument.NotNullOrEmpty(type, nameof(type));
	}
	readonly ZString type;

	JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)Master;

	protected override ZQuery CreateRelationshipFilter()
	{
		ZQuery result = base.CreateRelationshipFilter();
		result.AddToFilter(JoinCondition.And, CusLineTariffDetailSchema.BZ_Type, SQLComparisonOperator.Equal, type);
		return result;
	}

	protected override bool AllowNewCore => isAdditionalFees;

	protected override bool AllowRemoveCore => isAdditionalFees;

	bool isAdditionalFees => type == RateTypes.AdditionalFees;

	protected override void SetCollectionRelationships(BusinessObject dependent)
	{
		base.SetCollectionRelationships(dependent);
		(dependent as CusLineTariffDetail).BZ_Type = type;
	}

	public void RebuildAdditionalTaxes()
	{
		RemoveAndDeleteAll();

		if (type == RateTypes.AdditionalTaxes && InvoiceLine != null)
		{
			var tariffs = InvoiceLine.GetApplicableAdditionalTaxTariffs();
			var tariffsGroupedByTaxType = tariffs.GroupBy(t => t.ZZ1_TariffCode.SubstringSafe(0, 3));

			foreach (var tariffsGroup in tariffsGroupedByTaxType)
			{
				if (tariffsGroup.Key.Length == 3)
				{
					var additionalTax = AddNew();
					additionalTax.BZ_TaxType = tariffsGroup.Key;
					if (tariffsGroup.Count() == 1)
					{
						additionalTax.BZ_Tariff = tariffsGroup.Single().ZZ1_TariffCode.Left(additionalTax.BZ_TariffInfo.MaxLength);
					}
				}
			}
		}
	}
}
