using System;
using Enterprise.Customs.ES.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public partial class ShowResultsGridForm : ZChildForm
	{
		[Obsolete("This constructor is just for the designer")]
		public ShowResultsGridForm()
			: base()
		{
			InitializeComponent();
		}

		public ShowResultsGridForm(WriteOffResultCollection writeOffParent)
			: base(writeOffParent)
		{
			InitializeComponent();
		}
	}
}
