using System.Collections.Generic;

namespace Enterprise.Customs.IT.Business.Declaration;

public interface IRN22CheckablePackage : ICheckablePackage
{
	IEnumerable<IRN22CheckablePackage> GetRelatedEntryPreviousPackages();
}
