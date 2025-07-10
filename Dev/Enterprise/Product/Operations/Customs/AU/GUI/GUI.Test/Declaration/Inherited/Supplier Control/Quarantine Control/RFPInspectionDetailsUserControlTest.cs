using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class RFPInspectionDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestEnable_AuthorisationEstablishment()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			using (var form = new ZForm(declaration))
			using (var ctr = new RFPInspectionDetailsUserControl())
			{
				form.Controls.Add(ctr);
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctr, "Invoices");
				form.Show();
				Application.DoEvents();
				var groupBox = ctr.FindSingle<ZGroupBox>("AuthorisationEstablishmentGroupBox");
				Assert("AuthorisationEstablishmentGroupBox should be Enable", groupBox.Enabled);
				invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Wool;
				Assert("AuthorisationEstablishmentGroupBox should be Disable for Wool produce.", !groupBox.Enabled);
				invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.SkinsAndHides;
				Assert("AuthorisationEstablishmentGroupBox should be Disable for SkinsAndHides produce.", !groupBox.Enabled);
			}
		}

		public void TestVisible_ProduceType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			using (var form = new ZForm(declaration))
			using (var ctr = new RFPInspectionDetailsUserControl())
			{
				form.Controls.Add(ctr);
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctr, "Invoices");
				form.Show();
				Application.DoEvents();
				var authorisationDateDateEdit = ctr.FindSingle<ZDateEdit>("QH_AuthorisationDateDateEdit");
				var authorisationCommentsTextBox = ctr.FindSingle<ZTextBox>("QH_AuthorisationCommentsTextBox");
				var authorisationFlagCheckBox = ctr.FindSingle<ZCheckBox>("QH_AuthorisationFlagCheckBox");
				Assert("Precondition", !invoiceHeader.IsNEXDOCSActive);
				Assert("QH_AuthorisationDateDateEdit should be invisible when the invoice is EXDOCS", !authorisationDateDateEdit.Visible);
				Assert("QH_AuthorisationCommentsTextBox should be invisible when the invoice is EXDOCS", !authorisationCommentsTextBox.Visible);
				Assert("QH_AuthorisationFlagCheckBox should be invisible when the invoice is EXDOCS", !authorisationFlagCheckBox.Visible);
				invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
				Assert("Precondition", invoiceHeader.IsNEXDOCSActive);
				Assert("QH_AuthorisationDateDateEdit should be visible when the invoice is NEXDOCS", authorisationDateDateEdit.Visible);
				Assert("QH_AuthorisationCommentsTextBox should be visible when the invoice is NEXDOCS", authorisationCommentsTextBox.Visible);
				Assert("QH_AuthorisationFlagCheckBox should be visible when the invoice is NEXDOCS", authorisationFlagCheckBox.Visible);
			}
		}

		public void TestVisible_StorageLocation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			using (var form = new ZForm(declaration))
			using (var ctr = new RFPInspectionDetailsUserControl())
			{
				form.Controls.Add(ctr);
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctr, "Invoices");
				form.Show();
				Application.DoEvents();
				var textBox = ctr.FindSingle<ZTextBox>("QH_StorageEstablishmentTextBox");
				var addressControl = ctr.FindSingle<ZAddressControl>("QH_OA_StorageEstablishmentAddressGuidFindBox");
				invoiceHeader.QuarantineExDocHeader.QH_StorageLocation = EXDOCCodeOrganisation.Codes.Code;
				Assert("QH_StorageEstablishmentTextBox should be visible when the QH_StorageLocation is CODE.", textBox.Visible);
				Assert("QH_OA_StorageEstablishmentAddressGuidFindBox should be invisible when the QH_StorageLocation is CODE.", !addressControl.Visible);
				invoiceHeader.QuarantineExDocHeader.QH_StorageLocation = EXDOCCodeOrganisation.Codes.Organisation;
				Assert("QH_StorageEstablishmentTextBox should be invisible when the QH_StorageLocation is Organisation.", !textBox.Visible);
				Assert("QH_OA_StorageEstablishmentAddressGuidFindBox should be visible when the QH_StorageLocation is Organisation.", addressControl.Visible);
			}
		}

		public void TestVisible_AuthorisationLocation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			using (var form = new ZForm(declaration))
			using (var ctr = new RFPInspectionDetailsUserControl())
			{
				form.Controls.Add(ctr);
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctr, "Invoices");
				form.Show();
				Application.DoEvents();
				var textBox = ctr.FindSingle<ZTextBox>("QH_AuthorisationEstablishmentTextBox");
				var codeFindBox = ctr.FindSingle<ZCodeFindBox>("QH_AuthorisationEstablishmentCodeFindBox");
				var addressControl = ctr.FindSingle<ZAddressControl>("QH_OA_AuthorisationEstablishmentAddressGuidFindBox");
				invoiceHeader.QuarantineExDocHeader.QH_AuthorisationLocation = EXDOCCodeOrganisation.Codes.Code;
				Assert("QH_AuthorisationEstablishmentTextBox should be visible when the QH_AuthorisationLocation is CODE.", textBox.Visible);
				Assert("QH_AuthorisationEstablishmentCodeFindBox should be invisible when the QH_AuthorisationLocation is CODE.", !codeFindBox.Visible);
				Assert("QH_OA_AuthorisationEstablishmentAddressGuidFindBox should be invisible when the QH_AuthorisationLocation is CODE.", !addressControl.Visible);
				invoiceHeader.QuarantineExDocHeader.QH_AuthorisationLocation = QuarantineExDocHeaderLookups.AqisPlaceCode;
				Assert("QH_AuthorisationEstablishmentTextBox should be invisible when the QH_AuthorisationLocation is AqisPlaceCode.", !textBox.Visible);
				Assert("QH_AuthorisationEstablishmentCodeFindBox should be visible when the QH_AuthorisationLocation is AqisPlaceCode.", codeFindBox.Visible);
				Assert("QH_OA_AuthorisationEstablishmentAddressGuidFindBox should be invisible when the QH_AuthorisationLocation is AqisPlaceCode.", !addressControl.Visible);
				invoiceHeader.QuarantineExDocHeader.QH_AuthorisationLocation = EXDOCCodeOrganisation.Codes.Organisation;
				Assert("QH_AuthorisationEstablishmentTextBox should be invisible when the QH_AuthorisationLocation is Organisation.", !textBox.Visible);
				Assert("QH_AuthorisationEstablishmentCodeFindBox should be invisible when the QH_AuthorisationLocation is Organisation.", !codeFindBox.Visible);
				Assert("QH_OA_AuthorisationEstablishmentAddressGuidFindBox should be visible when the QH_AuthorisationLocation is Organisation.", addressControl.Visible);
			}
		}

		public void TestVisible_CatchZone()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			using (var form = new ZForm(declaration))
			using (var ctr = new RFPInspectionDetailsUserControl())
			{
				form.Controls.Add(ctr);
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(ctr, "Invoices");
				form.Show();
				Application.DoEvents();
				var textBox = ctr.FindSingle<ZTextBox>("QH_OriginCatchZoneTextBox");
				var grid = ctr.FindSingle<ZGrid>("NexDocCatchZonesGrid");
				CombineAssertions(() =>
				{
					Assert("Precondition", !invoiceHeader.IsNEXDOCSActive);
					Assert("QH_OriginCatchZoneTextBox should be visible when the invoice is EXDOCS",
						textBox.Visible);
					Assert("NexDocCatchZonesGrid should be invisible when the invoice is EXDOCS", !grid.Visible);
				});

				invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
				CombineAssertions(() =>
				{
					Assert("Precondition", invoiceHeader.IsNEXDOCSActive);
					Assert("QH_OriginCatchZoneTextBox should be invisible when the invoice is NEXDOCS",
						!textBox.Visible);
					Assert("NexDocCatchZonesGrid should be visible when the invoice is NEXDOCS", grid.Visible);
				});
			}
		}
	}
}
