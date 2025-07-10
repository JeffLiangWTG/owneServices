using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class TariffAdditionalCodeWrapper : ITariffAdditionalCode
	{
		public TariffAdditionalCodeWrapper(EU.Business.SupplementaryCode additionalSupCodeItem)
		{
			Argument.NotNull(additionalSupCodeItem, "CACO cannot be null");

			tariffCode = additionalSupCodeItem.CY_Code;
			tariffDescription = additionalSupCodeItem.CY_Data;
			tariffIsComplete = ZBool.False;
		}

		public TariffAdditionalCodeWrapper(SupportingDocument supportingDocument)
		{
			//If SupportingDoc -> is DispoPart
			Argument.NotNull(supportingDocument, "Supporting Document cannot be null");

			tariffCode = supportingDocument?.CSI_Code ?? ZString.Empty;
			tariffDescription = ZString.Empty;
			tariffIsComplete = ZBool.False;
		}

		public TariffAdditionalCodeWrapper(AdditionalInfo additionalInfo)
		{
			//if AdditionalInfo -> is MenSpec
			Argument.NotNull(additionalInfo, "Additional Info / Special Mention cannot be null");

			tariffCode = additionalInfo?.CSI_Code ?? ZString.Empty;
			tariffDescription = additionalInfo?.CSI_Description ?? ZString.Empty;
			tariffIsComplete = ZBool.False;
		}

		public TariffAdditionalCodeWrapper(ZString code)
		{
			tariffCode = code;
			tariffDescription = ZString.Empty;
			tariffIsComplete = ZBool.False;
		}

		#region Members

		public ZString Code => tariffCode;

		public ZString Description => tariffDescription;

		public ZBool IsCompleted => tariffIsComplete;

		#endregion

		protected readonly ZString tariffCode;
		protected readonly ZString tariffDescription;
		protected readonly ZBool tariffIsComplete;
	}
}
