using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.DocumentWrappers;
using EUNctsHeaderDocumentWrapper = Enterprise.DocumentWrappers.Customs.EU.NCTS.NctsHeaderDocumentWrapper;

namespace Enterprise.Customs.FR.DocumentWrappers.NCTS;

[WTG.StaticAnalysis.Annotation.CodeAlive("Used in documents DataContext")]
public class NctsHeaderDocumentWrapper : EUNctsHeaderDocumentWrapper
{
	protected NctsHeaderDocumentWrapper(NctsHeader nctsHeader, BusinessObjectFactory factory) : base(nctsHeader, factory)
	{
	}

	public static EUNctsHeaderDocumentWrapper New(NctsHeader nctsHeader, BusinessObjectFactory factoryToWrap)
	{
		Argument.NotNull(nctsHeader, nameof(nctsHeader));

		EUNctsHeaderDocumentWrapper nctsHeaderDocumentWrapper;

		if (ShouldUseSecurityNctsHeaderDocumentWrapper(nctsHeader))
		{
			nctsHeaderDocumentWrapper = SecurityNctsHeaderDocumentWrapper.New(nctsHeader, factoryToWrap);
		}
		else
		{
			nctsHeaderDocumentWrapper = new NctsHeaderDocumentWrapper(nctsHeader, factoryToWrap);
		}
		return nctsHeaderDocumentWrapper;
	}

	protected override DocBaseWrapperCollection<Enterprise.DocumentWrappers.Customs.EU.NCTS.NctsDepartureCargoDescWrapper> GetLinesCore()
	{
		return new NctsDepartureCargoDescWrapperCollection(NctsHeader, Factory);
	}

	protected override ZBool GetShowStampOnBoxC() => NctsHeader.FallBackIsActive && !BOXCAUTHORISATIONNUMBER.IsEmpty && MovementHeader.IsSimplifiedNctsProcedure;

	protected override ZString GetBoxCAuthorizedConsignorName()
	{
		var result = ZString.Empty;
		var locationOfGoodsCode = BOXCAUTHORISATIONNUMBER;
		if (!locationOfGoodsCode.IsEmpty && MovementHeader is NctsDepartureMovementHeader movementHeader)
		{
			result = movementHeader.FRLookups.AuthorizedLocationOfGoodsCodeList.FirstOrDefault(x => x.CPH_Number == locationOfGoodsCode)?.PermitHolder?.OH_Code ?? ZString.Empty;
		}
		return result;
	}

	protected override ZString GetBoxDSignature() => NctsHeader.FallBackIsActive && MovementHeader.IsSimplifiedNctsProcedure ? Res.GetString("90AD7407-0B85-4C98-9174-EB98B642D47C", "Authorized consignor — 99206") : ZString.Empty;

	protected override ZString GetBOX50Signature() => NctsHeader.FallBackIsActive && MovementHeader.IsSimplifiedNctsProcedure ? Res.GetString("6B1807CC-4B10-4189-AC9C-8ACB4D6712E6", "Signature waived — 99207") : ZString.Empty;
}
