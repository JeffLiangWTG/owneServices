using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	[TestedType(typeof(NonUCC6SupportingDocumentFieldsLayout))]
	sealed class NonUCC6SupportingDocumentFieldsLayoutTest : LayoutsAbstractTest
	{
		public void TestReferenceNumberCodeVisiblities()
		{
			Factory.CreateSupportingDocumentCodeLists(new TestSupportingDocumentCodeList("2800", hasPermitAttribute: true), new TestSupportingDocumentCodeList("3200", hasPermitAttribute: false));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var document = invoice.SupportingDocuments.AddNew();
			document.CSI_Code = "2800";

			using (var form = new ZForm(document))
			using (var userControl = new LayoutSupportingDocumentsFieldsControl(declaration))
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(declaration, "");
				form.Show();

				var referenceNumberCodeFindBox = userControl.Controls.Find(nameof(SupportingDocumentFieldsControlBag.ReferenceNumberCodeFindBox), true).FirstOrDefault();
				var referenceNumberTextBox = userControl.Controls.Find(nameof(SupportingDocumentFieldsControlBag.ReferenceNumberTextBox), true).FirstOrDefault();

				AssertEquals(true, referenceNumberCodeFindBox.Visible);
				AssertEquals(false, referenceNumberTextBox.Visible);

				document.CSI_Code = "3200";

				AssertEquals(false, referenceNumberCodeFindBox.Visible);
				AssertEquals(true, referenceNumberTextBox.Visible);
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
	}
}
