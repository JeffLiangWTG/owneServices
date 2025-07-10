using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class PackageValidation : EU.Business.Declaration.PackageValidation
{
	public PackageValidation(AutoCusDecHouseContainerPack parent) : base(parent)
	{
	}

	new Package Parent => (Package)base.Parent;

	protected override void CheckCW_PackQty()
	{
		base.CheckCW_PackQty();

		CheckPackQtyCannotBeOrMustBeZero(Parent.CW_PackQtyInfo);
	}

	protected override void CheckCW_PackType()
	{
		base.CheckCW_PackType();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CW_PackTypeInfo);
	}

	protected override void CheckCW_MarksAndNos()
	{
		base.CheckCW_MarksAndNos();
		if (IsDeclarationExport && IsUCC6AndTransitionPeriodAES30 && Parent.CW_MarksAndNos.Length > SADConstants.CustomsFieldMaxLength.Package.MarksAndNosForExport)
		{
			Parent.CW_MarksAndNosInfo.AddMessageError(ValidationCaptions.PackageValidation.MarksExceedsMaxLength42);
		}
	}

	#region Implementation

	ZBool IsDeclarationExport => Parent.Declaration?.IsExport ?? ZBool.False;

	bool IsUCC6AndTransitionPeriodAES30 => Parent.Declaration is JobDeclaration jobDeclaration && jobDeclaration.IsUCC6 && jobDeclaration.IsTransitionPeriodAES30;

	void CheckPackQtyCannotBeOrMustBeZero(ZPropertyInfo targetPropertyInfo)
	{
		var isUCC6AndIsExport = Parent.Declaration?.IsUCC6AndIsExport ?? false;
		if (!isUCC6AndIsExport)
		{
			return;
		}

		var packQuantity = Parent.CW_PackQty;
		var isBulkPackage = Parent.IsBulk;
		if (packQuantity == 0 && !isBulkPackage)
		{
			targetPropertyInfo.AddMessageError(ValidationCaptions.Package.PackQtyCannotBeZeroForTheSelectedPackType);
		}
		else if (packQuantity > 0 && isBulkPackage)
		{
			targetPropertyInfo.AddMessageError(ValidationCaptions.Package.PackQtyMustBeZeroForTheSelectedPackType);
		}
	}

	#endregion
}
