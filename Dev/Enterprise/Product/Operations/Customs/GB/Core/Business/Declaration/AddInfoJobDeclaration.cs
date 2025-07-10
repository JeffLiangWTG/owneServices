using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public partial class JobDeclaration
	{
		[MaxLength(JobDeclaration.Schema.ZG_VATDeferNumberMaxLength)]
		public override ZString JE_VATDeferNumber
		{
			get => base.JE_VATDeferNumber;
			set => base.JE_VATDeferNumber = value;
		}
	}
}
