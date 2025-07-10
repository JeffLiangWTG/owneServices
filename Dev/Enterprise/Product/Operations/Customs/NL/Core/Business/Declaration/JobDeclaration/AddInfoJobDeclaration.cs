using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Customs.NL.Business.Declaration;

public partial class JobDeclaration
{
	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.SpecificCircumstanceIndicatorList))]
	[MaxLength(3)]
	public override ZString JE_SpecificCircumstanceIndicator { get => base.JE_SpecificCircumstanceIndicator; set => base.JE_SpecificCircumstanceIndicator = value; }
}
