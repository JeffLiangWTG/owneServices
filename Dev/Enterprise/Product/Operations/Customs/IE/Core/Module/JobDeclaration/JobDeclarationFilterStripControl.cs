using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IE.Module
{
	public partial class JobDeclarationFilterStripControl : EU.Module.JobDeclarationFilterStripControl
	{
		public JobDeclarationFilterStripControl(JobDeclarationModule module, IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(module, gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		public static class ResourceStrings
		{
			public static ResourceStringData CustomsDocStatus => Res.GetData("415B7BF2-BA3B-44D9-9DBD-8ED12977D3EB", "Doc. Status", "Customs Doc. Status", "Customs Document Status");
			public static ResourceStringData CustomsDocStatusDesc => Res.GetData("C56C3BDB-C4B1-4576-9F01-082978F8AC24", "Doc. Status Desc.", "Customs Doc. Status Desc.", "Customs Doc. Status Description", "Customs Document Status Description");
			public static ResourceStringData DeclarationType => Res.GetData("BDB2EC61-4831-481E-B988-861FA7B37180", "Declaration Type");
		}

		protected override void InitializeAdditionalGridColumns()
		{
			base.InitializeAdditionalGridColumns();
			AddColumns();
			RemoveColumns();
		}

		protected override bool SupportsExitControlCore => true;

		void AddColumns()
		{
			grid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = ResourceStrings.CustomsDocStatus,
				ColumnName = JobDeclaration.Schema.CustomsDocStatus,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			});

			grid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = ResourceStrings.CustomsDocStatusDesc,
				ColumnName = JobDeclaration.Schema.CustomsDocStatusDesc,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110)
			});

			grid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = ResourceStrings.DeclarationType,
				ColumnName = IE.Business.Declaration.JobDeclaration.Schema.DeclarationType,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110)
			});
		}

		void RemoveColumns()
		{
			FilteredGrid.SetAvailability(false, JobDeclaration.Schema.JE_EntryAuthorisationDate);
		}
	}
}
