using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.MX.Business;
using static Enterprise.Customs.MX.GUI.Testing.BaseInvoiceLineUserControlTest;

namespace Enterprise.Customs.MX.GUI.Testing
{
	class ExportInvoiceLineUserControlTest : BaseInvoiceLineUserControlAbstractTest
	{
		public void TestColumnLayoutContextForInvoiceLinesGrid()
		{
			using (var control = new ExportInvoiceLineUserControl())
			{
				AssertEquals("Column layout context should be export", nameof(Customs.GUI.DeclarationType.Export), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}

		protected override string JobMessageType => JobMessageTypeList.Codes.Export;

		protected override List<ZString> ExpectedColumnNamesListInOrder
		{
			get
			{
				if (expectedColumnNamesListOnThisOrder == null)
				{
					expectedColumnNamesListOnThisOrder = new List<ZString>
					{
						JobComInvoiceLine.Schema.JI_LineNo,
						JobComInvoiceLine.Schema.JI_Calc_MergedLineNumber,
						JobComInvoiceLine.Schema.JI_Calc_Invoice,
						JobComInvoiceLine.Schema.JI_PartNo,
						JobComInvoiceLine.Schema.JI_CC,
						JobComInvoiceLine.Schema.JI_InvoiceQuantity,
						JobComInvoiceLine.Schema.JI_InvoiceUQ,
						JobComInvoiceLine.Schema.JI_CustomsQuantity,
						JobComInvoiceLine.Schema.JI_CustomsUnitQty,
						JobComInvoiceLine.Schema.JI_LinePrice,
						JobComInvoiceLine.Schema.JI_Description,
						JobComInvoiceLine.Schema.JI_CountryOfOrigin,
						JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport,
						JobComInvoiceLine.Schema.JI_RH_NKCommodity_Code,
						JobComInvoiceLine.Schema.JI_Weight,
						JobComInvoiceLine.Schema.JI_WeightUQ,
						JobComInvoiceLine.Schema.JI_NetWeight,
						JobComInvoiceLine.Schema.JI_NetWeightUQ,
						JobComInvoiceLine.Schema.JI_Volume,
						JobComInvoiceLine.Schema.JI_VolumeUQ,
						JobComInvoiceLine.Schema.JI_OrderNumber,
						JobComInvoiceLine.Schema.JI_Calc_OrderLineNumberAndSubLine,
						JobComInvoiceLine.Schema.UnitPrice,
						JobComInvoiceLine.Schema.Observations,
						JobComInvoiceLine.Schema.JI_CustomAttrib1,
						JobComInvoiceLine.Schema.JI_CustomAttrib2,
						JobComInvoiceLine.Schema.JI_CustomAttrib3,
						JobComInvoiceLine.Schema.JI_CustomAttrib4,
						JobComInvoiceLine.Schema.JI_CustomAttrib5,
						JobComInvoiceLine.Schema.JI_CustomAttrib6,
						JobComInvoiceLine.Schema.JI_CustomTextBlob1,
						JobComInvoiceLine.Schema.JI_PartAttrib1,
						JobComInvoiceLine.Schema.JI_PartAttrib2,
						JobComInvoiceLine.Schema.JI_PartAttrib3,
						JobComInvoiceLine.Schema.JI_SerialNumber,
						JobComInvoiceLine.Schema.JI_CEI,
					};
				}
				return expectedColumnNamesListOnThisOrder;
			}
		}
		List<ZString> expectedColumnNamesListOnThisOrder;
	}
}
