using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DeclarationDVDAdditionalInfoWrapper : DeclarationDVDEUAndNationalCodesWrapper, IDeclarationDVDAdditionalInfo
	{
		public DeclarationDVDAdditionalInfoWrapper(AdditionalInfo addInfo) : base(GetCode(addInfo, true), GetCode(addInfo, false))
		{
			Argument.NotNull(addInfo, nameof(addInfo));

			Description = addInfo.CSI_Code.IsEmpty ? addInfo.CSI_Description : ZString.Empty;
		}

		public ZString Description { get; }

		static ZString GetCode(AdditionalInfo addInfo, bool isEU) => addInfo.CSI_NctsExportFromEC == isEU ? addInfo.CSI_Code : ZString.Empty;
	}
}
