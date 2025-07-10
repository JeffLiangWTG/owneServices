using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class DeclarationTypesCodeDescriptionPairProvider : Integration.Customs.EU.NCTS.IDeclarationTypesCodeDescriptionPairProvider, DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			var factory = new BusinessObjectFactory();
			var header = factory.New<NctsHeader>();
			header.BH_HeaderType = NctsMovementType.Codes.Departure;
			return header.MovementHeader.Lookups.DeclarationTypeList;
		}
	}
}
