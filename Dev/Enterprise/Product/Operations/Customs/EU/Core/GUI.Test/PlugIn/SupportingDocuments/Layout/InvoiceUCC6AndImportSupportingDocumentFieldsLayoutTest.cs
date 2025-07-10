using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	[TestedType(typeof(InvoiceUCC6AndImportSupportingDocumentFieldsLayout))]
	sealed class InvoiceUCC6AndImportSupportingDocumentFieldsLayoutTest : LayoutsAbstractTest
	{
		public void TestSetVisibilityForEucdm()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var document = invoice.SupportingDocuments.AddNew();
			AssertSetVisibilityForEucdm(true, true);
			AssertSetVisibilityForEucdm(false, true);
			AssertSetVisibilityForEucdm(true, false);
			AssertSetVisibilityForEucdm(false, false);

			void AssertSetVisibilityForEucdm(bool useEucdmSupportingDocumentGoodsShipment, bool useEucdmSupportingDocumentGoodsShipmentAndItem)
			{
				CombineAssertions($"UseEucdmSupportingDocumentGoodsShipment: {useEucdmSupportingDocumentGoodsShipment}, UseEucdmSupportingDocumentGoodsShipmentAndItem: {useEucdmSupportingDocumentGoodsShipmentAndItem}", () =>
				{
					using (ConfigurationTestHelper.TemporarySetupUseEucdmSupportingDocumentGoodsShipmentAndItem(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfigurationAndReturnMock(declaration, "IsUCC6Core", true), useEucdmSupportingDocumentGoodsShipment, useEucdmSupportingDocumentGoodsShipmentAndItem))
					using (var form = new ZForm(document))
					using (var userControl = new InvoiceLayoutSupportingDocumentsFieldsControl(declaration))
					{
						form.Controls.Add(userControl);
						userControl.SetDataBinding(declaration, "");
						form.Show();

						var quantityCalcEdit = userControl.Controls.Find(nameof(SupportingDocumentFieldsControlBag.QuantityCalcEdit), true).FirstOrDefault();
						var unitOfQuantityDropEdit = userControl.Controls.Find(nameof(SupportingDocumentFieldsControlBag.UnitOfQuantityDropEdit), true).FirstOrDefault();
						var quantity2CalcEdit = userControl.Controls.Find(nameof(SupportingDocumentFieldsControlBag.Quantity2CalcEdit), true).FirstOrDefault();
						var unitOfQuantity2TextBox = userControl.Controls.Find(nameof(SupportingDocumentFieldsControlBag.UnitOfQuantity2TextBox), true).FirstOrDefault();
						var valueCalcEdit = userControl.Controls.Find(nameof(SupportingDocumentFieldsControlBag.ValueCalcEdit), true).FirstOrDefault();
						var currencyCodeFindBox = userControl.Controls.Find(nameof(SupportingDocumentFieldsControlBag.CurrencyCodeFindBox), true).FirstOrDefault();
						AssertEquals("QuantityCalcEdit", !useEucdmSupportingDocumentGoodsShipment, quantityCalcEdit.Visible);
						AssertEquals("UnitOfQuantityDropEdit", !useEucdmSupportingDocumentGoodsShipment, unitOfQuantityDropEdit.Visible);
						AssertEquals("Quantity2CalcEdit", !useEucdmSupportingDocumentGoodsShipment, quantity2CalcEdit.Visible);
						AssertEquals("UnitOfQuantity2TextBox", !useEucdmSupportingDocumentGoodsShipment, unitOfQuantity2TextBox.Visible);
						AssertEquals("ValueCalcEdit", !useEucdmSupportingDocumentGoodsShipment, valueCalcEdit.Visible);
						AssertEquals("CurrencyCodeFindBox", !useEucdmSupportingDocumentGoodsShipment, currencyCodeFindBox.Visible);

						var statusDropEdit = userControl.Controls.Find(nameof(SupportingDocumentFieldsControlBag.StatusDropEdit), true).FirstOrDefault();
						var dateOfIssueDateEdit = userControl.Controls.Find(nameof(SupportingDocumentFieldsControlBag.DateOfIssueDateEdit), true).FirstOrDefault();
						AssertEquals("StatusDropEdit", !useEucdmSupportingDocumentGoodsShipmentAndItem, statusDropEdit.Visible);
						AssertEquals("DateOfIssueDateEdit", !useEucdmSupportingDocumentGoodsShipmentAndItem, dateOfIssueDateEdit.Visible);
					}
				});
			}
		}

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				var euBag = SupportingDocumentFieldsControlBag.Instance;

				yield return new List<(ControlReference, ControlWidthClass)>
				{
					(euBag.CodeCodeFindBox, ControlWidthClass.Auto),
					(euBag.ReferenceNumberCodeFindBox, ControlWidthClass.Auto),
					(euBag.ReferenceNumberTextBox, ControlWidthClass.Auto),
					(euBag.AdditionalDescriptionTextBox, ControlWidthClass.Auto),
					(euBag.StatusDropEdit, ControlWidthClass.Auto),
					(euBag.DateOfIssueDateEdit, ControlWidthClass.Auto),
					(euBag.DateOfExpiryDateEdit, ControlWidthClass.Auto),
				};

				yield return new List<(ControlReference, ControlWidthClass)>
				{
					(euBag.QuantityCalcEdit, ControlWidthClass.Auto),
					(euBag.UnitOfQuantityDropEdit, ControlWidthClass.Auto),
					(euBag.Quantity2CalcEdit, ControlWidthClass.Auto),
					(euBag.UnitOfQuantity2TextBox, ControlWidthClass.Auto),
					(euBag.ValueCalcEdit, ControlWidthClass.Auto),
					(euBag.CurrencyCodeFindBox, ControlWidthClass.Auto),
				};
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new SupportingDocumentFieldsLayoutBuilder<Business.Declaration.MultiLineAddInfos.SupportingDocument>();

		protected override IPanelLayoutProvider GetNewPanelLayoutProvider() => new InvoiceUCC6AndImportSupportingDocumentFieldsLayout(null);
	}
}
