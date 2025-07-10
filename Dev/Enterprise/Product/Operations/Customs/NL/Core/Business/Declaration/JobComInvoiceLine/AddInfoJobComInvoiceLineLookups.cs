using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.NL.Business.Declaration;

public class AddInfoJobComInvoiceLineLookups : EU.Business.Declaration.AddInfoJobComInvoiceLineLookups
{
	public AddInfoJobComInvoiceLineLookups(AddInfoJobComInvoiceLine parent) : base(parent)
	{
	}

	protected new AddInfoJobComInvoiceLine Parent => (AddInfoJobComInvoiceLine)base.Parent;

	public ZZRefCusCodeListCombinedCollection CountryOfDestinationList
	{
		get
		{
			var invoiceLine = Parent?.Parent;
			var codeListType = ZString.Empty;

			if (invoiceLine != null)
			{
				if (invoiceLine.IsImport)
				{
					codeListType = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM17;
				}
				else
				{
					codeListType = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17;
				}
			}

			var resultCollection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, invoiceLine.GetDefaultDataGroupingCode(), codeListType, ZDateTime.Today);
			return resultCollection;
		}
	}
}
