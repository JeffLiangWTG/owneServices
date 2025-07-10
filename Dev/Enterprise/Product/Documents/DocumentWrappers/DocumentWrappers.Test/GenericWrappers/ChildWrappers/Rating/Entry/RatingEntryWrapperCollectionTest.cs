using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(RatingEntryWrapperCollection))]
	sealed class RatingEntryWrapperCollectionTest : GenericWrapperCollectionTest<RatingEntryWrapperCollection>
	{
		public void TestPopulated()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();
			RateEntry entry1 = tariff.AddRateEntry("FCL");
			RateEntry entry2 = tariff.AddRateEntry("LCL");

			RatingEntryWrapperCollection collection = new RatingEntryWrapperCollection(new RateEntry[] { entry1, entry2 }, Factory);

			AssertContainsExactElementsInAnyOrder(
				(e) => e.TI_RateCategory.ToString(),
				new RateEntry[] { entry1, entry2 },
				Array.ConvertAll(collection.ToArray<RatingEntryWrapper>(), (w) => (RateEntry)w.WrappedObject));
		}

		#region Implementation

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();
			RateEntry entry = tariff.AddRateEntry("FCL");

			return new RatingEntryWrapper(entry, Factory);
		}

		protected override RatingEntryWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new RatingEntryWrapperCollection(Factory);
		}

		#endregion
	}
}
