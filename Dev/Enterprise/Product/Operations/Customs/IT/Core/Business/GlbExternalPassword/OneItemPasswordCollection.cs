using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business;

public abstract class OneItemPasswordCollection<TPassword> : DependentBusinessObjectCollection<TPassword, GlbStaff> where TPassword : MasterFiles.Business.GlbExternalPassword
{
	protected OneItemPasswordCollection(GlbStaff master, string validationMessage, ZQuery additionalFilter) : base(master, additionalFilter)
	{
		this.EnableMaxCountValidation(maxCount: 1, warnAtHalfway: false, CargoWise.ComponentModel.NotificationType.Error, validationMessage);
	}

	protected override void OnCountChanged(CollectionCountChangedEventArgs e)
	{
		base.OnCountChanged(e);
		if (Count > 1)
		{
			ErrorReporter.ReportOnce(FormattableString.Invariant($"{GetType().FullName} error"), "More than one element has been added to the collection. Only one is allowed");
		}
	}

	protected override void OnAdded(BusinessObject bizOAdded)
	{
		base.OnAdded(bizOAdded);
		if (!IsLoading && !bizOAdded.HasChanges)
		{
			bizOAdded.HasChanges = true;
		}
	}
}
