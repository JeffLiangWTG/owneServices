using System;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ResourceStrings.GUI
{
	public partial class DocBuilderUsagesForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public DocBuilderUsagesForm()
		{
			InitializeComponent();
		}

		public DocBuilderUsagesForm(HelpDataString bo)
			: base(bo)
		{
			InitializeComponent();
		}

		public override string FormHeading
		{
			get { return Res.GetString("290ed76c-ccfc-423d-b65f-840a3158780e", "DocBuilder Resource String Usage"); }
		}
	}
}
