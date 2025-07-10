using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(MultiJobDeclarationForm))]
	sealed class MultiJobDeclarationFormTest : ZFormBasherTest
	{
		public void TestTariff()
		{
			var multiHeader = new MultiJobDeclarationHeader(Factory);
			multiHeader.Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (var form = new MultiJobDeclarationForm(multiHeader))
			{
				form.Show();

				var grid = form.FindSingle<ZGrid>("grid");
				var tariffColumn = grid.GetColumnStyle("JI_Tariff");
				AssertType<Universal.GUI.TariffColumnStyleInfo>("tariffColumn is TariffColumnStyleInfo", tariffColumn);
				AssertEquals("tariffColumn TariffType", Universal.Constants.TariffTypes.Import, (tariffColumn as Universal.GUI.TariffColumnStyleInfo).GetTariffType());
			}

			multiHeader.Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new MultiJobDeclarationForm(multiHeader))
			{
				form.Show();

				var grid = form.FindSingle<ZGrid>("grid");
				var tariffColumn = grid.GetColumnStyle("JI_Tariff");
				AssertType<Universal.GUI.TariffColumnStyleInfo>("tariffColumn is TariffColumnStyleInfo", tariffColumn);
				AssertEquals("tariffColumn TariffType", Universal.Constants.TariffTypes.Export, (tariffColumn as Universal.GUI.TariffColumnStyleInfo).GetTariffType());
			}
		}

		public void TestTariff_AHECC()
		{
			var multiHeader = new MultiJobDeclarationHeader(Factory);

			multiHeader.Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			using (var form = new MultiJobDeclarationForm(multiHeader))
			{
				form.Show();

				var grid = form.FindSingle<ZGrid>("grid");
				var tariffColumn = grid.GetColumnStyle("JI_Tariff");
				AssertType<AHECCTariffColumnStyleInfo>("tariffColumn is AHECCTariffColumnStyleInfo", tariffColumn);
			}

			multiHeader.Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new MultiJobDeclarationForm(multiHeader))
			{
				form.Show();

				var grid = form.FindSingle<ZGrid>("grid");
				var tariffColumn = grid.GetColumnStyle("JI_Tariff");
				AssertType<AHECCTariffColumnStyleInfo>("tariffColumn is AHECCTariffColumnStyleInfo", tariffColumn);
			}
		}

		protected override Form GetFormToBashCore() => new MultiJobDeclarationForm(new MultiJobDeclarationHeader(Factory));
	}
}
