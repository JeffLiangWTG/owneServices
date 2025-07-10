using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class PricingPageSetWrapperCollection : GenericWrapperCollection<PricingPageSetWrapper>
	{
		public PricingPageSetWrapperCollection(RatingHeader header, ZString menuTitle, BusinessObjectFactory factory)
			: base(factory)
		{
			this.header = header ?? throw new ArgumentNullException(nameof(header));
			this.menuTitle = menuTitle;
			this.lookup = new Dictionary<PricingPaginationStrategy, PricingPageSetWrapper>();
		}

		protected override DocumentWrapper WrapObject(object objectToWrap)
		{
			return new PricingPageSetWrapper((RatingHeader)objectToWrap, PricingPaginationStrategy.None, Factory);
		}

		protected override IBODocDataProvider GetRow(ZString index)
		{
			var strategy = GetPaginationStrategy(index);
			if (strategy == PricingPaginationStrategy.None)
			{
				return base.GetRow(index);
			}

			if (AgentPricingPageName.EqualsIgnoringCase(menuTitle))
			{
				strategy = strategy | PricingPaginationStrategy.IsAgentPricingPage;
			}

			if (!lookup.TryGetValue(strategy, out PricingPageSetWrapper result))
			{
				lookup.Add(strategy, result = new PricingPageSetWrapper(header, strategy, Factory));
			}

			return result;
		}

		PricingPaginationStrategy GetPaginationStrategy(string name)
		{
			switch (name)
			{
				case "ForwardingStandard":
					return PricingPaginationStrategy.ForwardingCategoryFilter | PricingPaginationStrategy.StandardStyle;
				case "ForwardingLandscapeSimple":
					return PricingPaginationStrategy.ForwardingCategoryFilter | PricingPaginationStrategy.LandscapeSimpleStyle;
				case "ForwardingLandscapeSimpleLoose":
					return PricingPaginationStrategy.ForwardingCategoryFilter | PricingPaginationStrategy.LandscapeSimpleStyle | PricingPaginationStrategy.AirModeAndNonContainerizedFilter;
				case "ForwardingLandscapeSimpleNonLoose":
					return PricingPaginationStrategy.ForwardingCategoryFilter | PricingPaginationStrategy.LandscapeSimpleStyle | PricingPaginationStrategy.NonAirModeOrContainerizedFilter;
				case "ForwardingLandscapeComplex":
					return PricingPaginationStrategy.ForwardingCategoryFilter | PricingPaginationStrategy.LandscapeComplexStyle;
				case "ForwardingLandscapeComplexLoose":
					return PricingPaginationStrategy.ForwardingCategoryFilter | PricingPaginationStrategy.LandscapeComplexStyle | PricingPaginationStrategy.AirModeAndNonContainerizedFilter;
				case "ForwardingLandscapeComplexNonLoose":
					return PricingPaginationStrategy.ForwardingCategoryFilter | PricingPaginationStrategy.LandscapeComplexStyle | PricingPaginationStrategy.NonAirModeOrContainerizedFilter;
				case "ForwardingCompact":
					return PricingPaginationStrategy.ForwardingCategoryFilter | PricingPaginationStrategy.LandscapeCompactStyle;

				case "ShippingStandard":
					return PricingPaginationStrategy.ShippingCategoryFilter | PricingPaginationStrategy.StandardStyle;
				case "ShippingLandscapeSimple":
					return PricingPaginationStrategy.ShippingCategoryFilter | PricingPaginationStrategy.LandscapeSimpleStyle;
				case "ShippingLandscapeComplex":
					return PricingPaginationStrategy.ShippingCategoryFilter | PricingPaginationStrategy.LandscapeComplexStyle;
				case "ShippingCompact":
					return PricingPaginationStrategy.ShippingCategoryFilter | PricingPaginationStrategy.LandscapeCompactStyle;

				case "ShippingDetention":
					return PricingPaginationStrategy.ShippingDetentionCategoryFilter | PricingPaginationStrategy.LandscapeSimpleStyle;

				case "CFS":
					return PricingPaginationStrategy.CFSCategoryFilter | PricingPaginationStrategy.LandscapeSimpleStyle;

				default:
					return PricingPaginationStrategy.None;
			}
		}

		readonly RatingHeader header;
		readonly ZString menuTitle;
		readonly Dictionary<PricingPaginationStrategy, PricingPageSetWrapper> lookup;
		internal ZString AgentPricingPageName = (NoResString)"Agent Pricing Page";
	}
}
