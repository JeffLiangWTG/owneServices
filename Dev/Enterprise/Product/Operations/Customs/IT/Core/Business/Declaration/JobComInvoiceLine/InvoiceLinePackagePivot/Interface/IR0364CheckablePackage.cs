using System.Collections.Generic;

namespace Enterprise.Customs.IT.Business.Declaration;

public interface IR0364CheckablePackage : ICheckablePackage
{
	IEnumerable<IR0364CheckablePackage> GetRelatedEntryOtherPackagesWithSameTypeAndMarks();
	IEnumerable<IR0364CheckablePackage> GetRelatedEntryPackagesWithSameMarksAndPacksGreaterThanZero();
}
