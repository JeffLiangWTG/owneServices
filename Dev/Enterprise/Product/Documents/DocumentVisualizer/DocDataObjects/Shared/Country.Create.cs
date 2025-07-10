using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using UniversalCountry = Enterprise.UniversalDataBuss.DataObjects.Universal.Country;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public sealed partial class Country
	{
		public static Country Create(IContext context, IRefCountry refCountry)
		{
			_ = context ?? throw new ArgumentNullException(nameof(context));

			return new Country(context.Factory, context.Countries)
			{
				Code = refCountry?.RN_Code ?? ZString.Empty,
				Name = refCountry?.RN_Desc ?? ZString.Empty
			};
		}

		public static Country Create(IContext context, ICountry country)
		{
			_ = context ?? throw new ArgumentNullException(nameof(context));

			return new Country(context.Factory, context.Countries)
			{
				Code = country?.Code ?? ZString.Empty,
				name = country?.Name ?? ZString.Empty
			};
		}

		public static Country Create(IContext context, UniversalCountry country)
		{
			_ = context ?? throw new ArgumentNullException(nameof(context));

			return new Country(context.Factory, context.Countries)
			{
				Code = country?.Code ?? ZString.Empty,
				name = country?.Name ?? ZString.Empty
			};
		}
	}
}
