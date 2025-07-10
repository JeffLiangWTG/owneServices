using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public static class InvoiceLineFormHelper
	{
		public static void SwitchOutCPCFindBoxToBeFormattedProcedureCodeFindBox(EUInvoiceLineUserControl form, ZBool isImport)
		{
			form.ClassificationDetailsGroupBox.Controls.Remove(form.CPCFindBox);
			form.CPCFindBox.Dispose();
			form.CPCFindBox = null;
			form.CPCFindBox = new FormattedProcedureCodeFindBox();
			form.CPCFindBox.AllowDrop = true;
			form.BindingSource.SetBindingMember(form.CPCFindBox, "FilteredInvoiceLines.JI_FormattedProcedure");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((EU.Business.Declaration.JobComInvoiceLine)((EU.Business.Declaration.JobDeclaration)null).FilteredInvoiceLines.SyncRoot).JI_FormattedProcedure);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((EU.Business.Declaration.JobComInvoiceLine)((EU.Business.Declaration.JobDeclaration)null).FilteredInvoiceLines.SyncRoot).Lookups.CPCList);
			form.CPCFindBox.BindToList = "FilteredInvoiceLines.Lookups+CPCList";
			form.CPCFindBox.Location = form.CPCFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 150, true);
			form.CPCFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusProcedure;
			form.CPCFindBox.Name = "CPCFindBox";
			form.CPCFindBox.PreBoundMaxLength = 8;
			form.CPCFindBox.ShouldResize = true;
			form.CPCFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 17, true);
			form.CPCFindBox.TabIndex = isImport ? 25 : 17;
			form.ClassificationDetailsGroupBox.Controls.Add(form.CPCFindBox);
		}

		public static void SwitchOutCPCColumnToBeFormattedProcedureColumn(EUInvoiceLineUserControl form)
		{
			var column = form.CustomsInvoiceLinesBoundGrid.ColumnStyles.OfType<ZCodeFindBoxColumnStyleInfo>().FirstOrDefault(x => x.ColumnName == JobComInvoiceLine.Schema.JI_FormattedProcedure);

			if (column != null)
			{
				form.CustomsInvoiceLinesBoundGrid.ColumnStyles.Remove(column);

				var newColumn = new FormattedProcedureCodeFindBoxColumnStyleInfo();
				newColumn.ColumnName = JobComInvoiceLine.Schema.JI_FormattedProcedure;
				newColumn.IsMandatory = true;
				newColumn.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusProcedure;
				form.CustomsInvoiceLinesBoundGrid.ColumnStyles.Insert(6, newColumn);
			}
		}
	}
}
