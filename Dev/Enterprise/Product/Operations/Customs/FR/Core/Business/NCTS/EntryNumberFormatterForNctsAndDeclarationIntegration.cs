using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.NctsAndDeclarationIntegration;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.EU.NCTS.Business.NctsConstants;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class EntryNumberFormatterForNctsAndDeclarationIntegration : IEntryNumberFormatterForNctsAndDeclarationIntegration
	{
		public EntryNumberFormatterForNctsAndDeclarationIntegration(BusinessObjectFactory factory)
		{
			_factory = factory;
		}

		public (ZString Class, ZString Type, ZString Reference, ZInt? EntryLineNumber) FormatEntryNumber(CusEntryNumber sourceEntryNumber, ZString jobType)
		{
			string type;
			switch (jobType)
			{
				case NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5:
				case NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase4:
					type = PreviousDocumentCodeList.Codes._820;
					break;
				case NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure:
					type = PreviousDocumentCodeList.Codes._821;
					break;
				case NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure:
					type = PreviousDocumentCodeList.Codes._822;
					break;
				case NctsTypeOfDeclaration.Codes.TirDeclaration:
					type = PreviousDocumentCodeList.Codes._952;
					break;
				default:
					type = GetTop1RefCusProcedure(jobType)?.ZZ6_Category;
					break;
			}
			return (PreviousDocumentClassList.Codes.PreviousDocument, type, sourceEntryNumber.CE_EntryNum, null);
		}

		RefCusProcedure GetTop1RefCusProcedure(ZString jobType)
		{
			var query = new ZQuery(
				new ZQuery(RefCusProcedureSchema.ZZ6_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.France),
				new ZQuery(RefCusProcedureSchema.ZZ6_Group, jobType)
			);
			return _factory.LoadTop1<RefCusProcedure>(query);
		}

		readonly BusinessObjectFactory _factory;
	}
}
