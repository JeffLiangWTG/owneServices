using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Rating.Business.DocumentPrinting.DocAmount;
using Enterprise.ResourceStrings.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("Value")]
	[WrapperTypeName("Pricing Page Table Column")]
	public class PricingPageColumnWrapper : GenericWrapper
	{
		public PricingPageColumnWrapper(string heading, DocAmount docAmount, BusinessObjectFactory factory, string currency = "")
			: base(null, factory)
		{
			Heading = heading;
			DocAmount = docAmount;
			Currency = currency;
		}

		DocAmount DocAmount { get; }

		[CodeStringFinderHint(typeof(ContainerisedTableStrategy), "AddValueToSubrowColumn")]
		public ZString Heading { get; private set; }

		public ZString Value => DocAmount.AmountAsString;

		/// <summary>
		/// Used in CompactTableStrategy because subRow is splitted by container and each subrow may have different currency
		/// (PricingPageTableSubRowWrapper.Currency is not set).
		/// Not used in Containerised/ChargeableTableStrategy because subrow is splitted by currency (subrow only contains FRT)
		/// (PricingPageTableSubRowWrapper.Currency is set).
		/// </summary>
		public ZString Currency { get; private set; }
	}
}
