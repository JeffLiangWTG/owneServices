using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.GUI.PlugIn.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Plugin.Testing
{
	class GBSupportingDocumentsFieldsControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceType()
		{
			SupportingDocumentsControlTestHelper.AssertBindingSource(typeof(GBSupportingDocumentsFieldsControl), typeof(JobDeclaration));
		}

		public void TestGroupBox()
		{
			SupportingDocumentsControlTestHelper.AssertGroupBox(typeof(GBSupportingDocumentsFieldsControl), false);
		}

		public void TestFields()
		{
			SupportingDocumentsControlTestHelper.AssertFields(typeof(GBSupportingDocumentsFieldsControl), fieldsDetails);
		}

		public void TestFieldsProperties()
		{
			using (var control = new GBSupportingDocumentsFieldsControl())
			{
				var groupBox = SupportingDocumentsControlTestHelper.GetFieldsGroupBox(control);
				CombineAssertions(() =>
				{
					AssertEquals("CSI_Code: CharacterCasing", CharacterCasing.Upper, groupBox.FindSingleOrDefault<ZCodeFindBox>("CSI_CodeCodeFindBox").CodeBox.CharacterCasing);
					AssertEquals("CSI_ReferenceNumberTextBox: CharacterCasing", CharacterCasing.Normal, groupBox.FindSingleOrDefault<ZTextBox>("CSI_ReferenceNumberTextBox").CharacterCasing);
					AssertEquals("CSI_ReferenceNumberCodeFindBox: CharacterCasing", CharacterCasing.Upper, groupBox.FindSingleOrDefault<ZCodeFindBox>("CSI_ReferenceNumberCodeFindBox").CodeBox.CharacterCasing);
					AssertEquals("CSI_Action: CharacterCasing", CharacterCasing.Upper, groupBox.FindSingleOrDefault<ZDropEdit>("CSI_ActionsDropEdit").CharacterCasing);
					AssertEquals("CSI_Availability: CharacterCasing", CharacterCasing.Upper, groupBox.FindSingleOrDefault<ZDropEdit>("CSI_AvailabilityDropEdit").CharacterCasing);
					AssertEquals("CSI_SubType: CharacterCasing", CharacterCasing.Upper, groupBox.FindSingleOrDefault<ZTextBox>("CSI_SubTypeTextBox").CharacterCasing);
					AssertEquals("CSI_UnitOfQuantity: CharacterCasing", CharacterCasing.Upper, groupBox.FindSingleOrDefault<ZDropEdit>("CSI_UnitOfQuantityDropEdit").CharacterCasing);
					AssertEquals("CSI_UnitOfQuantity2: CharacterCasing", CharacterCasing.Upper, groupBox.FindSingleOrDefault<ZTextBox>("CSI_UnitOfQuantity2TextBox").CharacterCasing);
					AssertEquals("CSI_RX_NKCurrency: CharacterCasing", CharacterCasing.Upper, groupBox.FindSingleOrDefault<ZCodeFindBox>("CSI_RX_NKCurrencyCodeFindBox").CodeBox.CharacterCasing);
					AssertEquals("CSI_Description: CharacterCasing", CharacterCasing.Upper, groupBox.FindSingleOrDefault<ZDropEdit>("CSI_DescriptionDropEdit").CharacterCasing);
					AssertEquals("CSI_ReferenceNumber2: CharacterCasing", CharacterCasing.Upper, groupBox.FindSingleOrDefault<ZTextBox>("CSI_ReferenceNumber2TextBox").CharacterCasing);
					AssertEquals("CSI_Quantity: Decimals", 3, groupBox.FindSingleOrDefault<ZCalcEdit>("CSI_QuantityCalcEdit").Decimals);
					AssertEquals("CSI_Value: Decimals", 5, groupBox.FindSingleOrDefault<ZCalcEdit>("CSI_ValueCalcEdit").Decimals);
					AssertEquals("CSI_Quantity2: Decimals", 5, groupBox.FindSingleOrDefault<ZCalcEdit>("CSI_Quantity2CalcEdit").Decimals);
				});
			}
		}

		public void TestGroupBoxCaptionChangesWithApplicationCode()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = "CHF";
				using (var control = new GBSupportingDocumentsFieldsControl())
				{
					control.JobDeclaration = declaration;
					control.Show();
					AssertEquals("JE_ApplicationCode = 'CHF'", "[44] Supporting Documents", SupportingDocumentsControlTestHelper.GetFieldsGroupBox(control).Text);
				}

				declaration.JE_ApplicationCode = "CDS";
				using (var control = new GBSupportingDocumentsFieldsControl())
				{
					control.JobDeclaration = declaration;
					control.Show();
					AssertEquals("JE_ApplicationCode = 'CDS'", "[UCC 2/3 && 8/7] Supporting Documents", SupportingDocumentsControlTestHelper.GetFieldsGroupBox(control).Text);
				}
			});
		}

		IEnumerable<(string ControlName, int TabIndex, Type ControlType)> fieldsDetails => new (string, int, Type)[]
		{
			("CSI_CodeCodeFindBox", 0, typeof(ZCodeFindBox)),
			("CSI_ReferenceNumberTextBox", 1, typeof(ZTextBox)),
			("CSI_ReferenceNumberCodeFindBox", 2, typeof(ZCodeFindBox)),
			("CSI_ActionsDropEdit", 3, typeof(ZDropEdit)),
			("CSI_AvailabilityDropEdit", 4, typeof(ZDropEdit)),
			("CSI_SubTypeTextBox", 5, typeof(ZTextBox)),
			("CSI_QuantityCalcEdit", 6, typeof(ZCalcEdit)),
			("CSI_UnitOfQuantityDropEdit", 7, typeof(ZDropEdit)),
			("CSI_Quantity2CalcEdit", 8, typeof(ZCalcEdit)),
			("CSI_UnitOfQuantity2TextBox", 9, typeof(ZTextBox)),
			("CSI_ValueCalcEdit", 10, typeof(ZCalcEdit)),
			("CSI_RX_NKCurrencyCodeFindBox", 11, typeof(ZCodeFindBox)),
			("CSI_DateOfIssueDateEdit", 12, typeof(ZDateEdit)),
			("CSI_DateOfExpiryDateEdit", 13, typeof(ZDateEdit)),
			("CSI_DescriptionDropEdit", 14, typeof(ZDropEdit)),
			("CSI_ReferenceNumber2TextBox", 15, typeof(ZTextBox)),
		};
	}
}
