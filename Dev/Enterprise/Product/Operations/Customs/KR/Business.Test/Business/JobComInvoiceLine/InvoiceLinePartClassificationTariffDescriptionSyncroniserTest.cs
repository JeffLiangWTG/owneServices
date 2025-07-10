using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class InvoiceLinePartClassificationTariffDescriptionSyncroniserTest : Customs.Business.Testing.InvoiceLinePartClassificationTariffDescriptionSyncroniserAbstractTest
	{
		public override void TestDescriptionOnTarrifWhenMerged()
		{
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345"))
			{
				base.TestDescriptionOnTarrifWhenMerged();
			}
		}

		protected override ZString TariffCode => "0000000001";

		protected override ZString TariffCode2 => "0000000002";

		protected override ZString TariffDescription => TariffDescriptionCore;

		protected override ZString TariffDescription2 => TariffDescriptionCore2;

		protected override Type DeclarationTypeForTest => typeof(JobDeclaration);

		internal const string TariffDescriptionCore = "";

		internal const string TariffDescriptionCore2 = "";
	}
}
