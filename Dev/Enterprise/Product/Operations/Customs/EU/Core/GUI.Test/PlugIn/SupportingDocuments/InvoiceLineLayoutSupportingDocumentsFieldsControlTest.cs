using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	class InvoiceLineLayoutSupportingDocumentsFieldsControlTest : TestCaseWithFactory
	{
		public void TestGetLayout()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var document = invoice.SupportingDocuments.AddNew();

			using (ConfigurationTestHelper.TemporarySetupUseEucdmSupportingDocumentGoodsShipmentAndItem(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfigurationAndReturnMock(declaration, "IsUCC6Core", true), true, true))
			{
				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
				AssertSetVisibilityForEucdm("UCC6, Export, UseEucdmSupportingDocumentGoodsShipmentAndItem", false);

				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
				AssertSetVisibilityForEucdm("UCC6, Import, UseEucdmSupportingDocumentGoodsShipmentAndItem", false);

				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.MiscellaneousCustoms;
				AssertSetVisibilityForEucdm("UCC6, MiscellaneousCustoms, UseEucdmSupportingDocumentGoodsShipmentAndItem", true);
			}

			using (ConfigurationTestHelper.TemporarySetupUseEucdmSupportingDocumentGoodsShipmentAndItem(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfigurationAndReturnMock(declaration, "IsUCC6Core", false), true, true))
			{
				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
				AssertSetVisibilityForEucdm("non-UCC6, Import, UseEucdmSupportingDocumentGoodsShipmentAndItem", true);
			}

			using (ConfigurationTestHelper.TemporarySetupUseEucdmSupportingDocumentGoodsShipmentAndItem(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfigurationAndReturnMock(declaration, "IsUCC6Core", true), true, false))
			{
				AssertSetVisibilityForEucdm("UCC6, Import, non-UseEucdmSupportingDocumentGoodsShipmentAndItem", true);
			}

			void AssertSetVisibilityForEucdm(string message, bool expectedVisibility)
			{
				CombineAssertions(message, () =>
				{
					using (var form = new ZForm(document))
					using (var control = new InvoiceLineLayoutSupportingDocumentsFieldsControl(declaration))
					{
						form.Controls.Add(control);
						form.Show();

						var statusDropEdit = control.Controls.Find(nameof(SupportingDocumentFieldsControlBag.StatusDropEdit), true).FirstOrDefault();
						var dateOfIssueDateEdit = control.Controls.Find(nameof(SupportingDocumentFieldsControlBag.DateOfIssueDateEdit), true).FirstOrDefault();
						AssertEquals("StatusDropEdit", expectedVisibility, statusDropEdit?.Visible ?? false);
						AssertEquals("DateOfIssueDateEdit", expectedVisibility, dateOfIssueDateEdit?.Visible ?? false);
					}
				});
			}
		}
	}
}
