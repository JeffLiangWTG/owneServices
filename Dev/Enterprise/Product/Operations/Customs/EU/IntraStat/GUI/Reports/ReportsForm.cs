using System;
using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.Intrastat.GUI
{
	public partial class ReportsForm : ZTemplateForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public ReportsForm()
		{
			InitializeComponent();
		}

		public ReportsForm(CusIntrastatGroup dataSource) : base(dataSource)
		{
			InitializeComponent();
			ControllerID = ControllerIDs.Customs.EU.IntrastatReports;
		}

		public override string FormCaption => Report.HumanReadableName;

		CusIntrastatGroup Report => (CusIntrastatGroup)BusinessEntity;
	}
}
