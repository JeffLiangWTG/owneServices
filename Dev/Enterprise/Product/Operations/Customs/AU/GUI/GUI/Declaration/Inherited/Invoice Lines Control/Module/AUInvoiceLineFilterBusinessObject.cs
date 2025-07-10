using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public class AUInvoiceLineFilterBusinessObject : InvoiceLineFilterBusinessObject
	{
		public AUInvoiceLineFilterBusinessObject(Func<Customs.Business.IInvoicesProvider> getInvoicesProvider, Func<ZString, ZBool> isColumnAvailable)
			: base(getInvoicesProvider, isColumnAvailable)
		{
			this.getInvoicesProvider = getInvoicesProvider;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
			=> new AUInvoiceLineFilterBusinessObject(getInvoicesProvider, IsColumnAvailable);

		readonly Func<Customs.Business.IInvoicesProvider> getInvoicesProvider;

		protected override void AddOrUpdateFunc(ModuleFilterCollection filters)
		{
			base.AddOrUpdateFunc(filters);
			var hasCustomsErrorFilter = new ModuleTextFilter(AUInvoiceLineFilterConstants.HasCustomsError, value => new ZQuery(), GetHasCustomsErrorOptions);
			AddCustomFilterFunc(filters, hasCustomsErrorFilter, GetHasCustomsErrorFilterFunc);
		}

		bool GetHasCustomsErrorFilterFunc(ModuleFilter moduleFilter, BaseJobComInvoiceLine jobComInvoiceLine)
		{
			var result = true;
			var hasCustomsErrorFilter = (ModuleTextFilter)moduleFilter;
			var invoiceLine = (JobComInvoiceLine)jobComInvoiceLine;
			if (hasCustomsErrorFilter.Property == "Yes")
			{
				result = invoiceLine.HasNoChangesAndHasErrorResponse;
			}
			else if (hasCustomsErrorFilter.Property == "No")
			{
				result = !invoiceLine.HasNoChangesAndHasErrorResponse;
			}
			return result;
		}

		IList GetHasCustomsErrorOptions()
		{
			return Factory.GetCachedValue("HasCustomsErrorOptions", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair("Yes", "Shows only Invoice Lines with a Customs Error");
				result.AddPair("No", "Shows only Invoice Lines without a Customs Error");
				result.AddPair("All", "All");
				return result;
			});
		}
	}
}
