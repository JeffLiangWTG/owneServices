using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(ExportInvoiceLineTemplate))]
	sealed class ExportInvoiceLineTemplateTest : BaseInvoiceLineTemplateTest
	{
		protected override ZString DeclarationMessageType => JPJobMessageTypeList.Codes.Export;

		protected override ZUserControl GetControlForTest() => new ExportInvoiceLineTemplate();

		public void TestExportControls()
		{
			var declaration = Factory.New<JobDeclaration>();
			using var control = GetControlForTest();
			control.SetDataBinding(declaration, "");

			CombineAssertions(() =>
			{
				var tariffFindBox = control.FindSingle<Universal.GUI.TariffFindBox>("TariffFindBox");
				AssertEndsWith("Tariff is to bind to JI_FormattedTariff", "JI_FormattedTariff", control.BindingSource.GetBindingMember(tariffFindBox));
				AssertEquals("TariffFindBox should show description box", true, tariffFindBox.ShowDescriptionBox);
				AssertContainsExactElementsInAnyOrder("Selection Style can be tariff or 4-character nomenclature", new List<SelectionStyle> { SelectionStyle.Tariff, SelectionStyle.Heading }, tariffFindBox.SelectNomenclatureModes);
			});
		}
	}
}
