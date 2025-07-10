using System;
using CargoWise.EntityFramework;

#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif

namespace Enterprise.Customs.AU.Declaration.Business;

[Serializable]
public class DiagnosticMessageException : ZException
{
	public string FailStatus { get; private set; }

	public DiagnosticMessageException(string failStatus, string message)
		: base(message)
	{
		FailStatus = failStatus;
	}

#if NETFRAMEWORK
	protected DiagnosticMessageException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		FailStatus = info.GetString(nameof(FailStatus));
	}

	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException(nameof(info));
		}

		info.AddValue(nameof(FailStatus), FailStatus);
		base.GetObjectData(info, context);
	}
#endif
}
