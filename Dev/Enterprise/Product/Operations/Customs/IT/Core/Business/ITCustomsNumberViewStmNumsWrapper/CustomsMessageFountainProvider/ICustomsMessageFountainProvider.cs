using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IT.Business;

public interface ICustomsMessageFountainProvider
{
	ZString DeclarantTaxNumber { get; }
	ZString Node { get; }
	ZString FountainPrefix { get; }
	ZString FountainType { get; }
	GlbCompany Company { get; }
	ITCustomsNumberViewStmNumsWrapper Wrapper { get; }
	INumberFountainProxy TryGetNumberFountain();
	ZBool HasClonableNumberRanges { get; }
	void TryCloneLastYearNumberRanges();
}
