using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public partial class AdditionalFeesUserControl : ZUserControl, ISupportMultipleResourceStringDataSupporter, ISupportMultipleResourceStringData
{
	public AdditionalFeesUserControl()
	{
		InitializeComponent();
	}

	public ISupportMultipleResourceStringData SupportMultipleResourceStringData => this;

	public IReadOnlyList<string> MultipleKeysToUse => new[] { UniversalReferenceConstants.RateTypes.AdditionalFees };
}
