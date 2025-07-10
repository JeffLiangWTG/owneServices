using System;
using System.Linq;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(OrgContactSchema))]
[assembly: UsesConstants(typeof(GlbStaffSchema))]
[assembly: UsesConstants(typeof(GlbPasswordHistorySchema))]
[assembly: UsesConstants(typeof(GlbPersonSchema))]
[assembly: UsesConstants(typeof(StorageDocsMasterKeySchema))]
[assembly: UsesConstants(typeof(StorageDocsSchema))]

namespace CargoWise.EntityFramework
{
	// The Compressor treats all binary that starts with "PZ" (0x505A) as compressed binary and attempts to uncompress it, which could lead to InvalidDataException, see WI00333773
	// This is to handle this Compressor behaviour for columns that we know should not be compressed/uncompressed at all
	public static class ZCompressorQuirks
	{
		// Known columns that should not be compressed or uncompressed
		// Currently contains all password hashing-related binary columns that should not be compressed
		[ThreadSafe]
		static readonly string[] ColumnsDoNotSupportCompression = {
			OrgContactSchema.Constants.OC_PasswordHash,
			OrgContactSchema.Constants.OC_PasswordSalt,
			GlbStaffSchema.Constants.GS_PasswordHash,
			GlbStaffSchema.Constants.GS_PasswordSalt,
			GlbStaffSchema.Constants.GS_SqlLoginPasswordHash,
			GlbPasswordHistorySchema.Constants.PWH_Hash,
			GlbPasswordHistorySchema.Constants.PWH_Salt,
			GlbPasswordHistorySchema.Constants.PWH_TruncatedHash,
			GlbPersonSchema.Constants.PER_PasswordHash,
			GlbPersonSchema.Constants.PER_PasswordSalt,
			StorageDocsMasterKeySchema.Constants.SCK_KeyValue,
			StorageDocsSchema.Constants.SC_EncryptedDataKey
		};

		public static bool IsColumnExcludedFromCompression(string columnName) => ColumnsDoNotSupportCompression.Any(c => c.Equals(columnName, StringComparison.OrdinalIgnoreCase));
	}
}
