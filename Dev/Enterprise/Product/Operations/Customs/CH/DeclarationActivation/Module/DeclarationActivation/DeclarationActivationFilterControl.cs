using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.DeclarationActivation.Module;

public sealed partial class DeclarationActivationFilterControl : ZFilterStripControl
{
	public DeclarationActivationFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject) : base(gridCollection, filterStripBusinessObject)
	{
		InitializeComponent();
	}

	protected override void AddFetchHints(object dataSource, string dataMember)
	{
		base.AddFetchHints(dataSource, dataMember);
	}
}
