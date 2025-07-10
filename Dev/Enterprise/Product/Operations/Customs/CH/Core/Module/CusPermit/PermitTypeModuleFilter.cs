using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.Module;

public class PermitTypeModuleFilter : Customs.Module.PermitTypeModuleFilter
{
	public static class Schema
	{
		public const int Property1MaxLength = CusPermitHeader.Schema.CPH_TypeMaxLength;
	}

	public PermitTypeModuleFilter(ZString description, GetPermitTypeQuery queryDelegate, GetList getCountries)
		: base(description, queryDelegate, getCountries)
	{
	}

	[MaxLength(Schema.Property1MaxLength)]
	public override ZString Property1 { get => base.Property1; set => base.Property1 = value; }
}
