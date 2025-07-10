using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business
{
	public static class NullifyRelationships
	{
		public static List<SchemaGuidColumn> FkToJobHeader
		{
			get
			{
				return new List<SchemaGuidColumn>()
				{
					AccTransactionHeaderSchema.AH_JH,
					AccTransactionLinesSchema.AL_JH,
					AccHotChequeSchema.AQ_JH,
				};
			}
		}
	}
}
