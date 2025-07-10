using System.Collections.Generic;

namespace Enterprise.Customs.IT.Business.Declaration;

public interface IR0219CheckablePackage : ICheckablePackage
{
	IEnumerable<IR0219CheckablePackage> GetRelatedEntryPackagesWithPacksEqualToZero();
}
