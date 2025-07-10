using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public interface ICusSupportingInfoParent
{
	ZGuid PK { get; }

	JobDeclaration JobDeclaration { get; }

	ZDateTime DateOfValuation { get; }

	BusinessObjectFactory Factory { get; }

	void MarkAsNeedingValidation();

	ZDateTime EffectiveAssessmentDate { get; }

	HugeSequenceNumberGenerator AdditionalInformationLineNumberGenerator { get; }

	void ValidateNonTradingGoods();
}
