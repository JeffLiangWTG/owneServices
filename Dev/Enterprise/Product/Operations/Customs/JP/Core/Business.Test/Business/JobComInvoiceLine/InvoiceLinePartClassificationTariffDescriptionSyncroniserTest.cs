using System;
using CargoWise.Types;

namespace Enterprise.Customs.JP.Business.Testing
{
	sealed class InvoiceLinePartClassificationTariffDescriptionSyncroniserTest : Customs.Business.Testing.InvoiceLinePartClassificationTariffDescriptionSyncroniserAbstractTest
	{
		protected override ZString TariffCode => "000000000";

		protected override ZString TariffCode2 => "000000001";

		protected override ZString TariffDescription => ZString.Empty;

		protected override ZString TariffDescription2 => ZString.Empty;

		protected override Type DeclarationTypeForTest => typeof(JobDeclaration);

		protected override Customs.Business.BaseJobDeclaration CreateBaseJobDeclarationForMerge()
		{
			var result = base.CreateBaseJobDeclarationForMerge();
			result.JE_ApplicationCode = Enterprise.Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			return result;
		}

		protected override string GetPartDescriptionForTestUpdatingDescriptionInAnotherFactoryUpdatesDescriptionIfDescriptionWasGeneratedFromPartAfterLoad() => "NEW PART DESC FROM OTHER FACTORY SAVING";

		public override void TestDescriptionOnTarrifWhenMerged()
		{
			Assert("For Japan customs, invoiceLine.JI_Description is used as merge key, see GetKeyForLine in EntryCreationStrategy", true);
		}
	}
}
