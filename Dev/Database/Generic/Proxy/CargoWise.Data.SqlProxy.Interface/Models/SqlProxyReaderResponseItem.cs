// TODO: Remove this suppression once in .net 8
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using Newtonsoft.Json;

namespace CargoWise.Data.SqlProxy.Interface.Models;

[JsonConverter(typeof(SqlProxyReaderResponseItemConverter))]
public abstract class SqlProxyReaderResponseItem
{
	[JsonProperty(GlowReaderResponseJsonProperty.Type)]
	public abstract string Type { get; }
}

public class SqlReaderResponseHeader : SqlProxyReaderResponseItem
{
	public override string Type => GlowReaderResponseItemType.Header;

	[JsonRequired]
	public SqlReaderResponseHeaderColumn[] Columns { get; set; }

	[JsonProperty(GlowReaderResponseJsonProperty.RecordsAffected)]
	[JsonRequired]
	public int RecordsAffected { get; set; }

	[JsonRequired]
	public bool HasRows { get; set; }
}

public class SqlReaderResponseRow : SqlProxyReaderResponseItem
{
	public override string Type => GlowReaderResponseItemType.Row;

	[JsonRequired]
	public SqlValue[] Values { get; set; }

	[JsonRequired]
	public int Depth { get; set; }
}

public class SqlReaderResponseHeaderColumn
{
	[JsonProperty(GlowReaderResponseJsonProperty.Name)]
	[JsonRequired]
	public string Name { get; set; }

	[JsonProperty(GlowReaderResponseJsonProperty.Type)]
	[JsonRequired]
	public string Type { get; set; }
}

[WTG.StaticAnalysis.Annotation.CodeAlive("SQL Over Http Connection")]
public static class GlowReaderResponseItemType
{
	public const string Row = "row";
	public const string Header = "header";
}

[WTG.StaticAnalysis.Annotation.CodeAlive("SQL Over Http Connection")]
public static class GlowReaderResponseJsonProperty
{
	public const string Name = "name";
	public const string Type = "type";
	public const string Values = "values";
	public const string Depth = "depth";
	public const string Columns = "columns";
	public const string RecordsAffected = "records";
	public const string HasRows = "hasRows";
}
