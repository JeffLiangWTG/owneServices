using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.Module.Testing
{
	internal class EnquiryFilterControlTest : TestCaseWithFactory
	{
		public void TestAccountClassColumn()
		{
			using (EnquiryModuleForTest module = new EnquiryModuleForTest())
			using (EnquiryFilterControl filterControl = (EnquiryFilterControl)module.GetNewFilterControl())
			using (ZForm form = new ZForm())
			{
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();
				ZGridColumn column = filterControl.FilteredGrid.Columns["BillTo+CompanyData+ARDebtorGroup+OJ_Code"];
				column.IsVisible = true;
				filterControl.FilteredGrid.RefreshTableStyles();
				AssertNotNull("Column should be visible and bound", column.ColumnStyle.PropertyDescriptor);
			}
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		class EnquiryModuleForTest : EnquiryModule
		{
			public new IFilterControl GetNewFilterControl()
			{
				return base.GetNewFilterControl();
			}
		}
	}
}
