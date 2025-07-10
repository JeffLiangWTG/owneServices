using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class QuarantineEUTariffFindBoxTest : TestCaseWithFactory
	{
		public void TestTariffTypeGetDataGrouping()
		{
			using (var findBox = new QuarantineEUTariffFindBox())
			{
				AssertEquals("TariffType", "EXP", findBox.TariffType);
				AssertEquals("GetDataGrouping", "EUN", findBox.GetDataGrouping());
			}
		}

		public void TestGetSelectNomenclatureModes()
		{
			using (var findBox = new QuarantineEUTariffFindBox())
			{
				var modes = findBox.GetSelectNomenclatureModes.Invoke();
				AssertCollectionContains(SelectionStyle.Subheading, modes);
				AssertCollectionContains(SelectionStyle.EightCharNomenclature, modes);
				AssertCollectionContains(SelectionStyle.Tariff, modes);
				AssertEquals("NomenclatureModes", 3, modes.Count);
			}
		}

		public void TestGetTariffFormatter()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			var invoiceLine = dec.InvoiceLines.AddNew();
			invoiceLine.JI_JZ = invoiceHeader.PK;
			var quarantineLine = invoiceLine.QuarantineExDocLine;
			using (var findBox = new QuarantineEUTariffFindBoxForTest())
			{
				findBox.SetCurrentItem(quarantineLine);
				AssertSame("Is using formatter from quarantine line", quarantineLine.EUTariffFormatter, findBox.GetTariffFormatterExposed());
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;
		}

		sealed class QuarantineEUTariffFindBoxForTest : QuarantineEUTariffFindBox
		{
			public QuarantineEUTariffFindBoxForTest() : base()
			{
			}

			public void SetCurrentItem(object currentItem) => CurrentItem = currentItem;

			public ITariffFormatter GetTariffFormatterExposed() => GetTariffFormatter();
		}
	}
}
