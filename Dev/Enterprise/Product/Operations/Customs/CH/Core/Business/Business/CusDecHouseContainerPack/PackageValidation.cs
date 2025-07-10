using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class PackageValidation : CusDecHouseContainerPackValidation
{
	public PackageValidation(AutoCusDecHouseContainerPack parent)
		: base(parent)
	{
	}

	protected new Package Parent => (Package)base.Parent;

	PlausiValidation PlausiValidation => plausiValidation ?? (plausiValidation = PlausiValidation.New((JobDeclaration)Parent.Declaration));
	PlausiValidation plausiValidation;

	protected override void CheckCW_PackQty()
	{
		base.CheckCW_PackQty();

		if (!Parent.IsLoosePackaging && Parent.Declaration != null && Parent.Declaration.InvoiceLines.Any() && Parent.TotalPackQtyOnInvoiceLines < Parent.CW_PackQty)
		{
			Parent.CW_PackQtyInfo.AddMessageError(ValidationMessages.Package.TotalLineQtyTooSmall);
		}
	}

	protected override void CheckCW_MarksAndNos()
	{
		base.CheckCW_MarksAndNos();
		PlausiValidation.CheckNS30021R132_MarksAndNos(Parent.CW_MarksAndNosInfo, Parent);
	}

	protected override void CheckCW_PackType()
	{
		base.CheckCW_PackType();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CW_PackTypeInfo);
	}
}
