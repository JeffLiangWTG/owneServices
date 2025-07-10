using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public partial class JobDeclaration
{
	[MaxLength(7)]
	public override ZString JE_AuthorisationNumber
	{
		get => base.JE_AuthorisationNumber;
		set
		{
			var oldValue = JE_AuthorisationNumber;
			base.JE_AuthorisationNumber = value;
			if (!IsCopying && oldValue != JE_AuthorisationNumber)
			{
				new JobDeclarationGoodsLocationFieldsDefaulter(this).DefaultLocationOfGoodsIfNeeded();
			}
		}
	}
}
