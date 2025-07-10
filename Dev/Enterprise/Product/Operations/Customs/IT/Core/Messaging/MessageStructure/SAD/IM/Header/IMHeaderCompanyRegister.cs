using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMHeaderCompanyRegister
{
	public IMHeaderCompanyRegister(IIMHeaderCompanyRegister companyRegister)
	{
		this.companyRegister = Argument.NotNull(companyRegister, nameof(companyRegister));
	}

	readonly IIMHeaderCompanyRegister companyRegister;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 1, false)]
	[MessageFieldImportRules("O")]
	public ZString Number => companyRegister.Number;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 2, false)]
	[MessageFieldImportRules("O")]
	public ZString Series => companyRegister.Series;

	[MessageLayout(Order = 2)]
	[MessageFieldDateDDMMYYYYRepresentation]
	[MessageFieldImportRules("O")]
	public ZDate Date => companyRegister.Date;
}
