using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.ImportSiscomex.Outgoing;

namespace Enterprise.Customs.BR.Business.ImportSiscomex
{
	public class DeclarationDetachProvider : IDeclarationDetach
	{
		public DeclarationDetachProvider(TariffDetach tariffDetach)
		{
			this.tariffDetach = Argument.NotNull(tariffDetach, nameof(tariffDetach));
		}

		public static DeclarationDetachProvider New(TariffDetach tariffDetach) => tariffDetach == null ? null : new DeclarationDetachProvider(tariffDetach);

		readonly TariffDetach tariffDetach;

		public string ReferenceNumber => tariffDetach.CY_Code;
	}
}

