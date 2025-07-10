using System;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Manifest.Business;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.GUI.Testing
{
	[TestedType(typeof(ILBillPartiesBuilder))]
	public sealed class ILBillPartiesBuilderTest : ColumnLayoutBuilderAbstractTest<ILBillPartiesBuilder, AsycudaBill, CommonBillPartiesControlBag>
	{
		public void TestILSpecificFieldsCaption()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.FillWithValidTestData();
			manifest.AMA_ManifestType = "785";
			manifest.AMA_Nature = ShipmentTypeList.Codes.Import23;
			var bill = manifest.Bills.AddNew();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingleOrDefault<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingleOrDefault<ZTabControl>(c => c.Name == "mainTabControl");

				var billsAndPacksTabPage = mainTabControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;

				var billsAndPacksTabControl = mainTabControl.FindSingleOrDefault<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var billsTabPage = billsAndPacksTabControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "billPartiesTabPage");
				billsAndPacksTabControl.SelectedTab = billsTabPage;

				var asycudaBillPartiesUserControl = billsTabPage.FindSingleOrDefault<AsycudaBillPartiesUserControl>(c => c.Name == "asycudaBillPartiesUserControl");

				AssertEquals("ConsigneeRegoNoTextBox", "VAT No.", asycudaBillPartiesUserControl.FindSingle<ZTextBox>(nameof(CommonBillPartiesControlBag.ConsigneeRegoNoTextBox)).CaptionResourceString.Caption);
			}

			var manifest1 = Factory.New<AsycudaManifestHeader>();
			manifest1.FillWithValidTestData();
			manifest1.AMA_ManifestType = "785";
			manifest1.AMA_Nature = ShipmentTypeList.Codes.Export22;
			var bill1 = manifest1.Bills.AddNew();

			using (var form = new ManifestForm(manifest1))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingleOrDefault<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingleOrDefault<ZTabControl>(c => c.Name == "mainTabControl");

				var billsAndPacksTabPage = mainTabControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;

				var billsAndPacksTabControl = mainTabControl.FindSingleOrDefault<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var billsTabPage = billsAndPacksTabControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "billPartiesTabPage");
				billsAndPacksTabControl.SelectedTab = billsTabPage;

				var asycudaBillPartiesUserControl = billsTabPage.FindSingleOrDefault<AsycudaBillPartiesUserControl>(c => c.Name == "asycudaBillPartiesUserControl");

				AssertEquals("ConsigneeRegoNoTextBox", "Reg.No", asycudaBillPartiesUserControl.FindSingle<ZTextBox>(nameof(CommonBillPartiesControlBag.ConsigneeRegoNoTextBox)).CaptionResourceString.Caption);
			}
		}

		protected override ILBillPartiesBuilder GetColumnLayoutBuilderForTesting() => new ILBillPartiesBuilder();

		protected override int ExpectedMaxColumns => 3;

		protected override void SetUp()
		{
			base.SetUp();
			disposableAction = ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ALL");
		}

		protected override void TearDown()
		{
			base.TearDown();
			disposableAction?.Dispose();
		}

		IDisposable disposableAction;
	}
}
