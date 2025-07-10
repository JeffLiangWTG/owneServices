using CargoWise.Common;
using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using CargoWise.Types;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.IL.Manifest.Business.MessagesWrappers
{
	public class DeclarationConsignmentConsignmentItemGovernmentProcedureWrapper : IDeclarationConsignmentConsignmentItemGovernmentProcedure
	{
		DeclarationConsignmentConsignmentItemGovernmentProcedureWrapper(AsycudaManifestHeader asycudaManifestHeader)
		{
			this.asycudaManifestHeader = Argument.NotNull(asycudaManifestHeader, nameof(asycudaManifestHeader));
		}

		public static IDeclarationConsignmentConsignmentItemGovernmentProcedure NewOrNull(AsycudaManifestHeader asycudaManifestHeader) => asycudaManifestHeader == null ? null : new DeclarationConsignmentConsignmentItemGovernmentProcedureWrapper(asycudaManifestHeader);

		public ICodeType CurrentCode => CodeTypeWrapper.NewOrNull(GetCurrentCode(asycudaManifestHeader.AMA_Nature));

		ZString GetCurrentCode(ZString nature)
		{
			switch (nature)
			{
				case ShipmentTypeList.Codes.Import23:
					return "4000000";

				case ShipmentTypeList.Codes.Export22:
					return "1000000";

				default:
					return null;
			}
		}

		readonly AsycudaManifestHeader asycudaManifestHeader;
	}
}
