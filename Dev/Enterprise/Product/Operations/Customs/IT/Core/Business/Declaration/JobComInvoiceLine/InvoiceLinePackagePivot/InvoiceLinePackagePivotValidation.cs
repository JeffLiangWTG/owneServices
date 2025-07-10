using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class InvoiceLinePackagePivotValidation : CusHouseContPackInvoiceLinePivotValidation
{
	public InvoiceLinePackagePivotValidation(AutoCusHouseContPackInvoiceLinePivot parent) : base(parent)
	{
	}

	protected override void CheckCHC_NumberOfPacks()
	{
		var declaration = Declaration;
		var isMergeDone = declaration?.IsMergeDone ?? false;
		if (isMergeDone)
		{
			CheckNumberOfPacksAfterMerge(declaration);
			return;
		}
		base.CheckCHC_NumberOfPacks();
	}

	#region CheckCHC_NumberOfPacks

	void CheckNumberOfPacksAfterMerge(JobDeclaration declaration)
	{
		if (PackagePivot.CHC_NumberOfPacks.IsEmpty)
		{
			ValidateWhenNumberOfPacksEmpty(declaration);
			return;
		}

		var isUcc6Export = declaration?.IsUCC6AndIsExport ?? false;
		if (isUcc6Export)
		{
			packageCheckStrategy = new PackageCheckR0219Strategy(PackagePivot);
			packageCheckStrategy.Check();
		}
	}

	void ValidateWhenNumberOfPacksEmpty(JobDeclaration declaration)
	{
		var package = PackagePivot.Package;
		var isPackTypeEmptyOrBulk = package is null || package.CW_PackType.IsEmpty || package.IsEmptyPackTypeAllowed;
		if (isPackTypeEmptyOrBulk)
		{
			return;
		}

		var isUcc6 = declaration?.IsUCC6 ?? false;
		if (!isUcc6)
		{
			packageCheckStrategy = new PackageCheckRN22Strategy(PackagePivot);
			packageCheckStrategy.Check();
			return;
		}

		CheckNumberOfPacksR0364(declaration);
	}

	void CheckNumberOfPacksR0364(JobDeclaration declaration)
	{
		var isExport = declaration?.IsExport ?? false;
		if (!isExport)
		{
			return;
		}

		var isTransitionPeriod = declaration?.IsTransitionPeriodAES30 ?? false;
		if (isTransitionPeriod)
		{
			packageCheckStrategy = new PackageCheckTransitionPeriodR0364Strategy(PackagePivot);
			packageCheckStrategy.Check();
			return;
		}

		packageCheckStrategy = new PackageCheckR0364Strategy(PackagePivot);
		packageCheckStrategy.Check();
	}

	#endregion

	IPackageCheckStrategy packageCheckStrategy;

	InvoiceLinePackagePivot PackagePivot => (InvoiceLinePackagePivot)Parent;

	JobDeclaration Declaration => (JobDeclaration)PackagePivot.Declaration;
}
