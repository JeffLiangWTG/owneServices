using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(EUExportSupplierHeaderUserControl))]
	public class EUExportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<EUExportSupplierHeaderUserControl, JobDeclaration>
	{
		protected override IEnumerable<string> ExpectedControlList => DefaultControlList;

		public void TestGridLayoutContext()
		{
			using (var control = new EUExportSupplierHeaderUserControl())
			{
				AssertEquals("context is set", nameof(Customs.GUI.DeclarationType.Export), control.InvoiceHeadersBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestGridId()
		{
			using (var control = new EUExportSupplierHeaderUserControl())
			{
				AssertEquals("GridLayoutp2kAlYGfl+VNYTeGMw3VqA==", control.JobComInvoiceHeadersBoundGrid.InnerGrid.GridId);
			}
		}

		[RequiresSTA]
		public void TestAdditionalInfoTabPageCaptionAndUserControlType()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				EUDynamicControlTestHelper.AssertEUSupplierHeaderUserControlTabPageCaptionAndUserControlType<EUExportSupplierHeaderUserControl>(declaration, "AdditionalInfoTabPage", "additionalInfosUserControl1", "Additional Documents", typeof(AdditionalInfosUserControlWithGrid));
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
			{
				EUDynamicControlTestHelper.AssertEUSupplierHeaderUserControlTabPageCaptionAndUserControlType<EUExportSupplierHeaderUserControl>(declaration, "AdditionalInfoTabPage", "additionalInfosUserControl1", "[44] Additional Info", typeof(AdditionalInfosUserControl));
			}
		}
	}
}
