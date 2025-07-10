using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class SingleLineEntryLookups : ZLookups
	{
		public SingleLineEntryLookups(SingleLineEntry parent) : base(parent)
		{
		}

		public RefCurrencyCollection Currencies => new RefCurrencyCollection(Factory);

		public TariffViewCollection Tariffs =>
			TariffViewCollection.GetCachedCollection(Factory,
				Parent.DataGrouping,
				Parent.TariffType,
				Parent.EffectiveDate);

		public RefCusProcedureCollection CPCList =>
			GetCachedCPCCollection(Factory,
				Parent.DataGrouping,
				Parent.TariffType,
				Parent.EffectiveDate);

		RefCusProcedureCollection GetCachedCPCCollection(BusinessObjectFactory factory, ZString dataGroupingCode, ZString tariffType, ZDateTime effectiveValuationDate)
		{
			var key = string.Format(CultureInfo.InvariantCulture,
				"{0}_{1}_{2}_{3}",
				nameof(RefCusProcedureCollection),
				dataGroupingCode,
				tariffType,
				effectiveValuationDate);

			return factory.GetCachedValue(key, () =>
			{
				var result = new RefCusProcedureCollection(Factory,
					dataGroupingCode,
					effectiveValuationDate,
					string.Empty,
					tariffType);

				return result;
			});
		}

		new SingleLineEntry Parent => (SingleLineEntry)base.Parent;
	}
}
