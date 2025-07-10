using CargoWise.Types;
using CargoWise.Workflow;

namespace Enterprise.ZArchitecture.Business
{
	static class WTE_EventReferenceExtensions
	{
		internal static EventLogReferenceBuilder AddGuidIfValid(this EventLogReferenceBuilder builder, ZGuid guid)
		{
			if (guid.IsValid)
			{
				return builder.AddMandatory(guid.ToString());
			}
			else
			{
				builder.AddReference(new EventLogReferenceBuilder.FreeTextReferencePart(""));
				return builder;
			}
		}

		internal static EventLogReferenceBuilder AddGuid(this EventLogReferenceBuilder builder, ZGuid guid)
		{
			return builder.AddMandatory(guid.ToString());
		}
	}
}
