namespace Enterprise.DbUpgrader.Resource.Version
{
	public interface IVersionChangeInfo
	{
		VersionLabel DbReferenceVersion_Schema { get; }
		VersionLabel DbReferenceVersion_Data { get; }
		VersionLabel DbReferenceVersion_Script { get; }
		VersionLabel DbReferenceVersion_Transformation { get; }
		VersionLabel DbReferenceVersion_Clr { get; }

		bool IsRequired_Data { get; }
		bool IsRequired_Schema { get; }
		bool IsRequired_Script { get; }
		bool IsRequired_Transformation { get; }

		bool IsRequired_ClientDocuments { get; }
	}
}
