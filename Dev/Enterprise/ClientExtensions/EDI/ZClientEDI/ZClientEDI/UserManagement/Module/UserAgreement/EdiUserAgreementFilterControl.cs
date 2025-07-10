using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.UserManagement.Module
{
	public partial class EdiUserAgreementFilterControl : ZFilterStripControl
	{
#if DEBUG
		[Obsolete("This constructor is just for the designer", true)]
		public EdiUserAgreementFilterControl()
		{
			InitializeComponent();
		}
#endif

		public EdiUserAgreementFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
