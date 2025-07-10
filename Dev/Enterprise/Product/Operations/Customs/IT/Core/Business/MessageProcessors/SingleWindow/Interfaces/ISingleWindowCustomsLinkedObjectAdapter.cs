using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IT.Business;

public interface ISingleWindowCustomsLinkedObjectAdapter : ICustomsLinkedObjectAdapter
{
	DocManagerInfo DocManagerInfo { get; }

	void AddLog(Event eventType, ZDateTime eventDate, KeyValuePair<string, string>[] eventAttributes);

	void SetEntryCustomsChannel(ZString entryCustomsChannel);

	void SetEntryAsCleared(ZDateTime releaseDateTime);

	void InsertOrUpdateReleaseCode(ZString releaseCode, ZDateTime releaseDate);

	ZBool IsEntryCleared { get; }
}
