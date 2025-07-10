using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.Declaration.Testing
{
	class EntryInstructionDetailBasicUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryInstructions.AddNew();
			using (var form = new ZForm(declaration))
			using (var control = new EntryInstructionDetailBasicUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					AssertNotNull("StyleDropEdit", control.FindSingleOrDefault<ZDropEditWithFixedWidth>("StyleDropEdit"));
					AssertNotNull("AssessmentDateEdit", control.FindSingleOrDefault<ZDateEdit>("AssessmentDateEdit"));
					AssertNotNull("ToWarehouseAddressControl", control.FindSingleOrDefault<ZAddressControl>("ToWarehouseAddressControl"));
					AssertNotNull("FromWarehouseAddressControl", control.FindSingleOrDefault<ZAddressControl>("FromWarehouseAddressControl"));
					AssertNotNull("DescriptionTextBox", control.FindSingleOrDefault<ZTextBox>("DescriptionTextBox"));
					AssertNotNull("SubStyleDropEdit", control.FindSingleOrDefault<ZDropEdit>("SubStyleDropEdit"));
					AssertNotNull("CPCDropEdit", control.FindSingleOrDefault<ZDropEdit>("CPCDropEdit"));
					AssertNotNull("ValuationBypassCodeDropEdit", control.FindSingleOrDefault<ZDropEdit>("ValuationBypassCodeDropEdit"));
					AssertNotNull("ValuationBypassReasonTextBox", control.FindSingleOrDefault<ZTextBox>("ValuationBypassReasonTextBox"));
					AssertNotNull("TotalInnerPackagesIntEdit", control.FindSingleOrDefault<ZIntEdit>("TotalInnerPackagesIntEdit"));
				});
			}
		}

		public void TestLocationOfGoods()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			using (var form = new ZForm(declaration))
			using (var control = new EntryInstructionDetailBasicUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					var locationOfGoodsPanel = control.FindSingleOrDefault<ZPanel>("LocationOfGoodsPanel");
					AssertEquals("Show Location Of Goods when IsUCC6 And IsImport", true, locationOfGoodsPanel.Visible);

					declaration.JE_MessageType = "EXP";
					Application.DoEvents();
					AssertEquals("Hide Location Of Goods When Export", false, locationOfGoodsPanel.Visible);

					declaration.JE_MessageType = "IMP";
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
					Application.DoEvents();
					AssertEquals("Hide Location Of Goods When Not IsUCC6", false, locationOfGoodsPanel.Visible);

					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
					Application.DoEvents();
					AssertEquals("Show Location Of Goodds", true, locationOfGoodsPanel.Visible);
				});
			}
		}

		public void TestValuationByPassCodeAndReason()
		{
			var declaration = Factory.New<JobDeclaration>();

			using (var frm = new ZForm(declaration))
			using (var control = new EntryInstructionDetailsUserControl())
			{
				frm.Controls.Add(control);
				frm.Show();

				Application.DoEvents();

				var valuationCodeCtrl = (ZDropEdit)control.Controls.Find("ValuationBypassCodeDropEdit", true)[0];
				AssertNotNull("ValuationBypassCodeDropEdit", valuationCodeCtrl);

				var valuationReasonCtrl = (ZTextBox)control.Controls.Find("ValuationBypassReasonTextBox", true)[0];
				AssertNotNull("ValuationBypassReasonTextBox", valuationReasonCtrl);

				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
				AssertEquals("Valuation Code control should not be visible for DeltaG exports", false, valuationCodeCtrl.Visible);
				AssertEquals("Valuation Reason control should not be visible for DeltaG exports", false, valuationReasonCtrl.Visible);

				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				AssertEquals("Valuation Code control should not be visible for DeltaIE exports", false, valuationCodeCtrl.Visible);
				AssertEquals("Valuation Reason control should not be visible for DeltaIE exports", false, valuationReasonCtrl.Visible);

				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
				AssertEquals("Valuation Code control should be visible for DeltaG imports", true, valuationCodeCtrl.Visible);
				AssertEquals("Valuation Reason control should be visible for DeltaG imports", true, valuationReasonCtrl.Visible);

				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				AssertEquals("Valuation Code control should not be visible for DeltaIE imports", false, valuationCodeCtrl.Visible);
				AssertEquals("Valuation Reason control should not be visible for DeltaIE imports", false, valuationReasonCtrl.Visible);
			}
		}
	}
}
