using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.FR.Module
{
	public partial class EntryHeaderFilterUserControl : EU.Module.EntryHeaderFilterUserControl
	{
		public EntryHeaderFilterUserControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		public new static class Schema
		{
			public const string CustomsProfile = "Declaration+JE_CustomsProfile";
			public const string DeltaMode = "Declaration+JE_DeltaMode";
			public const string TriggeringPointForValidation = "TriggeringPointForValidation";
			public const string ExportExitType = "Declaration+JE_ExportExitType";
		}
	}
}
