using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class AttributesUserControlTest : TestCaseWithFactory
	{
		public void TestDataSourceType()
		{
			using (var control = new AttributesUserControl())
			{
				AssertEquals("DataSourceType", typeof(AttributeCusCodeDataCollection), control.DataSourceType);
			}
		}

		public void TestContentColumnBindToDecimalPlaces()
		{
			using (var control = new AttributesUserControl())
			{
				AssertEquals("Content column BindToDecimalPlaces must be CY_DataDecimalPlaces", "CY_DataDecimalPlaces", ((ZMultiControlColumnStyleInfo)control.AttributesGrid.GetColumnStyle("Content")).BindToDecimalPlaces);
			}
		}

		public void TestComponentTypes()
		{
			using (var control = new AttributesUserControl())
			{
				AssertNotNull(control.AttributesGrid);

				AssertType<ZGrid>("AttributesGrid must be ZGrid", control.AttributesGrid);
				AssertType<ZGroupBox>("AttributesGroupBox must be ZGrid", control.AttributesGroupBox);
			}
		}

		public void TestColumnsTypes()
		{
			using (var form = new ZForm(Factory.New<JobDeclaration>()))
			using (var userControl = new AttributesUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				CombineAssertions(() =>
				{
					AssertEquals("CY_Data must be AttributeMultiControlColumnStyle", typeof(AttributeMultiControlColumnStyle), userControl.AttributesGrid.Columns[AttributeCusCodeData.Schema.Content].ColumnStyle.GetType());
					AssertEquals("CY_Code must be ZTextBoxColumnStyle", typeof(ZTextBoxColumnStyle), userControl.AttributesGrid.Columns[AttributeCusCodeData.Schema.CY_Code].ColumnStyle.GetType());
					AssertEquals("Label must be ZTextBoxColumnStyle", typeof(ZTextBoxColumnStyle), userControl.AttributesGrid.Columns["Label"].ColumnStyle.GetType());
					AssertEquals("IsMandatory must be ZCheckBoxColumnStyle", typeof(ZCheckBoxColumnStyle), userControl.AttributesGrid.Columns["IsMandatory"].ColumnStyle.GetType());
					AssertEquals("FillOrientation must be ZTextBoxColumnStyle", typeof(ZTextBoxColumnStyle), userControl.AttributesGrid.Columns["FillOrientation"].ColumnStyle.GetType());
				});
			}

			using (var form = new ZForm(Factory.New<CusGoodsCatalog>()))
			using (var userControl = new AttributesUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				CombineAssertions(() =>
				{
					AssertEquals("CY_Data must be AttributeMultiControlColumnStyle", typeof(AttributeMultiControlColumnStyle), userControl.AttributesGrid.Columns[AttributeCusCodeData.Schema.Content].ColumnStyle.GetType());
					AssertEquals("CY_Code must be ZTextBoxColumnStyle", typeof(ZTextBoxColumnStyle), userControl.AttributesGrid.Columns[AttributeCusCodeData.Schema.CY_Code].ColumnStyle.GetType());
					AssertEquals("Label must be ZTextBoxColumnStyle", typeof(ZTextBoxColumnStyle), userControl.AttributesGrid.Columns["Label"].ColumnStyle.GetType());
					AssertEquals("IsMandatory must be ZCheckBoxColumnStyle", typeof(ZCheckBoxColumnStyle), userControl.AttributesGrid.Columns["IsMandatory"].ColumnStyle.GetType());
					AssertEquals("FillOrientation must be ZTextBoxColumnStyle", typeof(ZTextBoxColumnStyle), userControl.AttributesGrid.Columns["FillOrientation"].ColumnStyle.GetType());
					AssertEquals("ParentAttributeCode must be ZTextBoxColumnStyle", typeof(ZTextBoxColumnStyle), userControl.AttributesGrid.Columns["ParentAttributeCode"].ColumnStyle.GetType());
					AssertEquals("ConditionDescription must be ZTextBoxColumnStyle", typeof(ZTextBoxColumnStyle), userControl.AttributesGrid.Columns["ConditionDescription"].ColumnStyle.GetType());
					AssertEquals("StartDate must be ZTextBoxColumnStyle", typeof(ZDateEditColumnStyle), userControl.AttributesGrid.Columns["StartDate"].ColumnStyle.GetType());
					AssertEquals("EndDate must be ZTextBoxColumnStyle", typeof(ZDateEditColumnStyle), userControl.AttributesGrid.Columns["EndDate"].ColumnStyle.GetType());
				});
			}
		}

		public void TestLayoutForCusGoodsCatalog()
		{
			var goodCatalog = Factory.New<CusGoodsCatalog>();
			using (var form = new ZForm(goodCatalog))
			using (var userControl = new AttributesUserControl())
			{
				userControl.SetDataBinding(goodCatalog, "Attributes");
				userControl.Dock = DockStyle.Fill;
				form.Controls.Add(userControl);
				form.Show();
				CombineAssertions(() =>
				{
					AssertNull("User control should NOT have TaxType column", userControl.AttributesGrid.Columns["TaxType"]);
					AssertNull("User control should NOT have LegalBase column", userControl.AttributesGrid.Columns["LegalBase"]);
					AssertNotNull("User control should have CY_Code column", userControl.AttributesGrid.Columns["CY_Code"]);
					AssertNotNull("User control should have Label column", userControl.AttributesGrid.Columns["Label"]);
					AssertNotNull("User control should have Content column", userControl.AttributesGrid.Columns["Content"]);
					AssertNotNull("User control should have IsMandatory column", userControl.AttributesGrid.Columns["IsMandatory"]);
					AssertNotNull("User control should have FillOrientation column", userControl.AttributesGrid.Columns["FillOrientation"]);
					AssertNull("User control should NOT have Example column", userControl.AttributesGrid.Columns["Example"]);
					AssertNotNull("User control should have ParentAttributeCode column", userControl.AttributesGrid.Columns["ParentAttributeCode"]);
					AssertNotNull("User control should have ConditionDescription column", userControl.AttributesGrid.Columns["ConditionDescription"]);
					AssertNotNull("User control should have Start column", userControl.AttributesGrid.Columns["StartDate"]);
					AssertNotNull("User control should have EndDate column", userControl.AttributesGrid.Columns["EndDate"]);

					AssertEquals("Attributes", userControl.AttributesGroupBox.CaptionResourceString.Caption);
				});
			}
		}

		public void TestLayoutForImportTtceAttributes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new ZForm(declaration))
			using (var userControl = new AttributesUserControl())
			{
				userControl.SetDataBinding(declaration, "FilteredInvoiceLines.TaxRegimeAttributes");
				userControl.Dock = DockStyle.Fill;
				form.Controls.Add(userControl);
				form.Show();
				CombineAssertions(() =>
				{
					AssertNotNull("User control should have TaxType column", userControl.AttributesGrid.Columns["TaxType"]);
					AssertNotNull("User control should have LegalBase column", userControl.AttributesGrid.Columns["LegalBase"]);
					AssertNotNull("User control should have CY_Code column", userControl.AttributesGrid.Columns["CY_Code"]);
					AssertNotNull("User control should have Label column", userControl.AttributesGrid.Columns["Label"]);
					AssertNotNull("User control should have Content column", userControl.AttributesGrid.Columns["Content"]);
					AssertNotNull("User control should have IsMandatory column", userControl.AttributesGrid.Columns["IsMandatory"]);
					AssertNotNull("User control should have FillOrientation column", userControl.AttributesGrid.Columns["FillOrientation"]);
					AssertNull("User control should NOT have Example column", userControl.AttributesGrid.Columns["Example"]);
					AssertNotNull("User control should have ParentAttributeCode column", userControl.AttributesGrid.Columns["ParentAttributeCode"]);
					AssertNull("User control should NOT have ConditionDescription column", userControl.AttributesGrid.Columns["ConditionDescription"]);

					AssertEquals("Attributes", userControl.AttributesGroupBox.CaptionResourceString.Caption);
				});
			}
		}

		public void TestLayoutForImportNcmAttributes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new ZForm(declaration))
			using (var userControl = new AttributesUserControl())
			{
				userControl.SetDataBinding(declaration, "FilteredInvoiceLines.Attributes");
				userControl.Dock = DockStyle.Fill;
				form.Controls.Add(userControl);
				form.Show();
				CombineAssertions(() =>
				{
					AssertNull("User control should NOT have TaxType column", userControl.AttributesGrid.Columns["TaxType"]);
					AssertNull("User control should NOT have LegalBase column", userControl.AttributesGrid.Columns["LegalBase"]);
					AssertNotNull("User control should have CY_Code column", userControl.AttributesGrid.Columns["CY_Code"]);
					AssertNotNull("User control should have Label column", userControl.AttributesGrid.Columns["Label"]);
					AssertNotNull("User control should have Content column", userControl.AttributesGrid.Columns["Content"]);
					AssertNotNull("User control should have IsMandatory column", userControl.AttributesGrid.Columns["IsMandatory"]);
					AssertNotNull("User control should have FillOrientation column", userControl.AttributesGrid.Columns["FillOrientation"]);
					AssertNull("User control should NOT have Example column", userControl.AttributesGrid.Columns["Example"]);
					AssertNotNull("User control should have ParentAttributeCode column", userControl.AttributesGrid.Columns["ParentAttributeCode"]);
					AssertNotNull("User control should have ConditionDescription column", userControl.AttributesGrid.Columns["ConditionDescription"]);

					AssertEquals("Attributes", userControl.AttributesGroupBox.CaptionResourceString.Caption);
				});
			}
		}

		public void TestLayoutForExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new ZForm(declaration))
			using (var userControl = new AttributesUserControl())
			{
				userControl.SetDataBinding(declaration, "FilteredInvoiceLines.Attributes");
				userControl.Dock = DockStyle.Fill;
				form.Controls.Add(userControl);
				form.Show();
				CombineAssertions(() =>
				{
					AssertNull("User control should NOT have TaxType column", userControl.AttributesGrid.Columns["TaxType"]);
					AssertNull("User control should NOT have LegalBase column", userControl.AttributesGrid.Columns["LegalBase"]);
					AssertNotNull("User control should have CY_Code column", userControl.AttributesGrid.Columns["CY_Code"]);
					AssertNotNull("User control should have Label column", userControl.AttributesGrid.Columns["Label"]);
					AssertNotNull("User control should have Content column", userControl.AttributesGrid.Columns["Content"]);
					AssertNotNull("User control should have IsMandatory column", userControl.AttributesGrid.Columns["IsMandatory"]);
					AssertNotNull("User control should have FillOrientation column", userControl.AttributesGrid.Columns["FillOrientation"]);
					AssertNull("User control should NOT have Example column", userControl.AttributesGrid.Columns["Example"]);
					AssertNull("User control should NOT have Example column", userControl.AttributesGrid.Columns["ParentAttributeCode"]);
					AssertNull("User control should NOT have Example column", userControl.AttributesGrid.Columns["ConditionDescription"]);

					AssertEquals("Attributes", userControl.AttributesGroupBox.CaptionResourceString.Caption);
				});
			}
		}

		public void TestLayoutForLPCO()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new ZForm(declaration))
			using (var userControl = new AttributesUserControl())
			{
				userControl.SetDataBinding(declaration, "FilteredInvoiceLines.Attributes");
				userControl.Dock = DockStyle.Fill;
				form.Controls.Add(userControl);
				form.Show();
				CombineAssertions(() =>
				{
					AssertNull("User control should NOT have TaxType column", userControl.AttributesGrid.Columns["TaxType"]);
					AssertNull("User control should NOT have LegalBase column", userControl.AttributesGrid.Columns["LegalBase"]);
					AssertNotNull("AttributesGrid should have CY_Code column", userControl.AttributesGrid.Columns["CY_Code"]);
					AssertNotNull("AttributesGrid should have Label column", userControl.AttributesGrid.Columns["Label"]);
					AssertNotNull("AttributesGrid should have Content column", userControl.AttributesGrid.Columns["Content"]);
					AssertNotNull("AttributesGrid should have IsMandatory column", userControl.AttributesGrid.Columns["IsMandatory"]);
					AssertNotNull("AttributesGrid should have FillOrientation column", userControl.AttributesGrid.Columns["FillOrientation"]);
					AssertNotNull("AttributesGrid should have Example column", userControl.AttributesGrid.Columns["Example"]);
					AssertNull("User control should NOT have Example column", userControl.AttributesGrid.Columns["ParentAttributeCode"]);
					AssertNull("User control should NOT have Example column", userControl.AttributesGrid.Columns["ConditionDescription"]);

					AssertEquals("Line Attributes", userControl.AttributesGroupBox.CaptionResourceString.Caption);
				});
			}
		}
	}
}
