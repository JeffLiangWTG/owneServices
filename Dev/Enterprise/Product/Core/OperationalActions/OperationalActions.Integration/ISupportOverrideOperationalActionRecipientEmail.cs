using System;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Integration;

public interface ISupportOverrideOperationalActionRecipientEmail
{
	bool IsOverrideRecipientEmailEnabled(Type bizObjType, ZGuid[] targets);
}
