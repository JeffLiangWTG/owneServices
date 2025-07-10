using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(EUNonLayoutImportSupplierHeaderUserControl))]
	sealed class EUNonLayoutImportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<EUNonLayoutImportSupplierHeaderUserControl, JobDeclaration>
	{
		public void TestAgreedPlaceControlsVisibility_AgreedPlaceCodeSupport()
		{
			AssertAgreedPlaceControlsVisibility(true);
		}

		[RequiresSTA]
		public void TestAgreedPlaceControlsVisibility_AgreedPlaceCodeNotSupport()
		{
			AssertAgreedPlaceControlsVisibility(false);
		}

		public void TestGroupInvoiceDropEditNotVisible()
		{
			using (var userControl = new EUNonLayoutImportSupplierHeaderUserControl())
			{
				var groupInvoiceDropEdit = userControl.FindSingleOrDefault<ZDropEdit>("GroupInvoiceDropEdit");
				AssertNotNull("GroupInvoiceDropEdit", groupInvoiceDropEdit);
				AssertEquals("GroupInvoiceDropEdit.Visible", false, groupInvoiceDropEdit.Visible);
			}
		}

		public void TestRemoveBuyerAddressColumn()
		{
			using (var userControl = new EUNonLayoutImportSupplierHeaderUserControl())
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				userControl.JobDeclaration = declaration;
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
				{
					userControl.InitializeGridLayout();
					AssertNull(userControl.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeader.Schema.JZ_OA_BuyerAddress]);
					AssertNull(userControl.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns["BuyerOrgPK"]);
				}
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					userControl.InitializeGridLayout();
					AssertNotNull(userControl.JobComInvoiceHeadersBoundGrid.ColumnStyles.OfType<ZGridColumnInfo>().First(c => c.ColumnName == Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_OA_BuyerAddress));
					AssertNotNull(userControl.JobComInvoiceHeadersBoundGrid.ColumnStyles.OfType<ZGridColumnInfo>().First(c => c.ColumnName == "BuyerOrgPK"));
				}
			}
		}

		protected override IEnumerable<string> ExpectedControlList => DefaultControlList;

		void AssertControlVisibility(Control parentUserControl, ZString controlName, ZBool expectedResult)
		{
			var controlToCheck = parentUserControl.Controls.Find(controlName, true).FirstOrDefault();
			AssertNotNull(controlToCheck);
			AssertEquals(expectedResult, controlToCheck.Visible);
		}

		void AssertAgreedPlaceControlsVisibility(bool agreedPlaceCodeSupport)
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetAgreedPlaceCodeSupport(declaration, agreedPlaceCodeSupport))
			using (var form = new ZForm(declaration))
			using (var userControl = new EUNonLayoutImportSupplierHeaderUserControl())
			{
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();
				CombineAssertions(() =>
				{
					invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CarriageAndInsurancePaidTo;
					AssertControlVisibility(userControl, "AgreedPlaceCodeFindBox", agreedPlaceCodeSupport);

					invoice.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
					AssertControlVisibility(userControl, "AgreedPlaceCodeFindBox", false);
				});
			}
		}
	}
}
