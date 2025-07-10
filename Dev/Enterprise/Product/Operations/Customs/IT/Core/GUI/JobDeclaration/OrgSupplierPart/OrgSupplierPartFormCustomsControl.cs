using System;

namespace Enterprise.Customs.IT.GUI;

public class OrgSupplierPartFormCustomsControl : EU.GUI.EUOrgSupplierPartFormCustomsControl
{
	protected override Type GetSupportingDocumentsUserControlType() => typeof(PartPivotLayoutSupportingDocumentsUserControl);

	protected override Type GetPreviousDocumentsUserControlType() => typeof(PreviousDocumentsUserControl);

	protected override Type GetSupplierPartTaxUserControlType() => typeof(OrgSupplierPartTaxUserControl);
}
