using System;

namespace Enterprise.Customs.IT.Business;

[Serializable]
public class MoreThanOneEntryHeaderFoundException : CustomsMessageProcessorException
{
	public MoreThanOneEntryHeaderFoundException()
		: base(Res.GetString("497C9E98-A809-4725-A4DE-18FEF197EB12", "More than one entry header found."))
	{
	}

#if NETFRAMEWORK
	protected MoreThanOneEntryHeaderFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		: base(info, context)
	{
	}
#endif
}
