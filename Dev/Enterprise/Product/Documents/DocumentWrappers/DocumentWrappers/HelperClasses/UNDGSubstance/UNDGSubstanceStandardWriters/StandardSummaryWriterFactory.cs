using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	static class StandardSummaryWriterFactory
	{
		internal static IUNDGStandardSummaryWriter GetWriter(string standard)
		{
			switch (standard)
			{
				case UNDGSubstanceLookups.UNDGSubstanceStandardTypes.ADN:
					return new ADNStandardSummaryWriter();

				case UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA:
					return new IATAStandardSummaryWriter();

				case UNDGSubstanceLookups.UNDGSubstanceStandardTypes.RID:
					return new RIDStandardSummaryWriter();

				case UNDGSubstanceLookups.UNDGSubstanceStandardTypes.ADR:
					return new ADRStandardSummaryWriter();

				case UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR:
					return new CFRStandardSummaryWriter();

				case UNDGSubstanceLookups.UNDGSubstanceStandardTypes.JTT:
					return new JTTStandardSummaryWriter();

				default:
					return new IMOStandardSummaryWriter();
			}
		}
	}
}
