using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.Module.Testing
{
	sealed class ASYCUDAManifestBillFilterStripControlTest : TestCaseWithFactory
	{
		public void TestNewZFilterStrip()
		{
			using (var control = new ASYCUDAManifestBillFilterStripControlForTest(Factory))
			using (var filterStrip = control.NewZFilterStripForTest())
			{
				AssertType(typeof(AsycudaModuleStrip), filterStrip);
			}
		}

		public void TestWorkflowCustomFieldColums()
		{
			CreateWorkflowWithCustomFields();
			using (var control = new ASYCUDAManifestBillFilterStripControlForTest(Factory))
			{
				string[] workflowColumns = control.FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(col => !string.IsNullOrEmpty(col.Caption) && col.Caption.StartsWith("UDF: ", StringComparison.Ordinal)).Select(col => col.Caption).ToArray();
				AssertContainsExactElementsInAnyOrder(new[] { "UDF: custom text", "UDF: custom int", "UDF: custom decimal", "UDF: custom datetime" }, workflowColumns);
			}
		}

		void CreateWorkflowWithCustomFields()
		{
			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = WorkflowDescriptors.GlobalManifestBillsWorkflowDescriptorCode;
			var customField1 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "UDF: custom text";
			customField1.XC_Type = AddOnColumnDataType.Codes.String;
			var customField2 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField2.XC_Name = "UDF: custom int";
			customField2.XC_Type = AddOnColumnDataType.Codes.Integer;
			var customField3 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField3.XC_Name = "UDF: custom decimal";
			customField3.XC_Type = AddOnColumnDataType.Codes.Decimal;
			var customField4 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField4.XC_Name = "UDF: custom datetime";
			customField4.XC_Type = AddOnColumnDataType.Codes.Datetime;
			Factory.Save();
		}
		public void TestGridColums()
		{
			IReadOnlyList<string> expectedColumnNames =
				[
					"Header+RegistrationNumber",
					"Header+RegistrationDate"
				];

			using (var form = new ZForm())
			using (var filterStrip = new ASYCUDAManifestBillFilterStripControl(new ASYCUDAManifestBillModuleCollection(Factory), new ASYCUDAManifestBillFilterStrip()))
			{
				CombineAssertions(() =>
				{
					form.Controls.Add(filterStrip);
					form.Show();
					var grid = filterStrip.FilteredGrid;
					for (var i = 0; i < expectedColumnNames.Count; i++)
					{
						Assert(expectedColumnNames[i], grid.Columns.Contains(expectedColumnNames[i]));
					}
				});
			}
		}

		sealed class ASYCUDAManifestBillFilterStripControlForTest : ASYCUDAManifestBillFilterStripControl
		{
			public ASYCUDAManifestBillFilterStripControlForTest(BusinessObjectFactory factory) : base(new ASYCUDAManifestBillModuleCollection(factory), new ASYCUDAManifestBillFilterStrip())
			{
			}

			public ZFilterStrip NewZFilterStripForTest() => NewZFilterStrip();
		}
	}
}
