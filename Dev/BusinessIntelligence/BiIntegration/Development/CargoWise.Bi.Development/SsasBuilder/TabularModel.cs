using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CargoWise.Bi.Development.SsasBuilder
{
	public class TabularModel
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
		[JsonProperty(PropertyName = "compatibilityLevel")]
		public int CompatibilityLevel { get; set; }
		[JsonProperty(PropertyName = "model")]
		public Model Model { get; set; }
		[JsonProperty(PropertyName = "id")]
		public string Id { get; set; }
	}

	public class Model
	{
		[JsonProperty(PropertyName = "culture")]
		public string Culture { get; set; }
		[JsonProperty(PropertyName = "dataSources")]
		public List<DataSource> DataSourcesList { get; set; }
		[JsonProperty(PropertyName = "tables")]
		public List<Table> TablesList { get; set; }
		[JsonProperty(PropertyName = "relationships")]
		public List<Relationship> RelationshipsList { get; set; }
	}

	public class Relationship
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
		[JsonProperty(PropertyName = "fromCardinality")]
		public string FromCardinality { get; set; }
		[JsonProperty(PropertyName = "fromTable")]
		public string FromTable { get; set; }
		[JsonProperty(PropertyName = "fromColumn")]
		public string FromColumn { get; set; }
		[JsonProperty(PropertyName = "toTable")]
		public string ToTable { get; set; }
		[JsonProperty(PropertyName = "toColumn")]
		public string ToColumn { get; set; }
		[JsonProperty(PropertyName = "isHidden")]
		public bool? IsHidden { get; set; }
		[JsonProperty(PropertyName = "isActive")]
		public bool? IsActive { get; set; }
		[JsonProperty(PropertyName = "crossFilteringBehavior")]
		public string CrossFilteringBehavior { get; set; }
	}

	public class Table
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
		[JsonProperty(PropertyName = "isHidden", NullValueHandling = NullValueHandling.Ignore)]
		public bool? IsHidden { get; set; }
		[JsonProperty(PropertyName = "dataCategory")]
		public string DataCategory { get; set; }
		[JsonProperty(PropertyName = "columns")]
		public List<Column> ColumnsList { get; set; }
		[JsonProperty(PropertyName = "partitions")]
		public List<Partition> PartitionsList { get; set; }
		[JsonProperty(PropertyName = "hierarchies")]
		public List<Hierarchyy> HierarchiesList { get; set; }
		[JsonProperty(PropertyName = "measures")]
		public List<Measures> MeasuresList { get; set; }
		[JsonProperty(PropertyName = "annotations")]
		public List<Annotation> AnnotationsList { get; set; }
	}

	public class Hierarchyy
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "levels")]
		public List<Level> LevelsList { get; set; }
	}

	public class Level
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
		[JsonProperty(PropertyName = "ordinal")]
		public int? Ordinal { get; set; }
		[JsonProperty(PropertyName = "column")]
		public string Column { get; set; }
	}

	public class Partition
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "dataView")]
		public string DataView { get; set; }
		[JsonProperty(PropertyName = "source")]
		public Source Source { get; set; }
		[JsonProperty(PropertyName = "annotations", NullValueHandling = NullValueHandling.Ignore)]
		public List<Annotation> AnnotationsList { get; set; }
	}

	public class Source
	{
		[JsonProperty(PropertyName = "type")]
		public string Type { get; set; }
		[JsonProperty(PropertyName = "expression")]
		[JsonConverter(typeof(SingleOrArrayConverter<string>))]
		public List<string> Expression { get; set; }
		[JsonProperty(PropertyName = "query")]
		[JsonConverter(typeof(SingleOrArrayConverter<string>))]
		public List<string> Query { get; set; }
		[JsonProperty(PropertyName = "dataSource")]
		public string DataSource { get; set; }
	}

	public class Measures
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
		[JsonProperty(PropertyName = "expression")]
		[JsonConverter(typeof(SingleOrArrayConverter<string>))]
		public List<string> Expression { get; set; }
		[JsonProperty(PropertyName = "formatString")]
		public string FormatString { get; set; }
		[JsonProperty(PropertyName = "kpi")]
		public Kpi Kpi { get; set; }
		[JsonProperty(PropertyName = "isHidden", NullValueHandling = NullValueHandling.Ignore)]
		public bool? IsHidden { get; set; }
		[JsonProperty(PropertyName = "annotations")]
		public List<Annotation> AnnotationsList { get; set; }
	}

	public class Kpi
	{
		[JsonProperty(PropertyName = "targetExpression")]
		public string TargetExpression { get; set; }
		[JsonProperty(PropertyName = "targetFormatString")]
		public string TargetFormatString { get; set; }
		[JsonProperty(PropertyName = "statusGraphic")]
		public string StatusGraphic { get; set; }
		[JsonProperty(PropertyName = "statusExpression")]
		[JsonConverter(typeof(SingleOrArrayConverter<string>))]
		public List<string> StatusExpression { get; set; }
		[JsonProperty(PropertyName = "annotations")]
		public List<AnnotationSingleValue> AnnotationsList { get; set; }
	}

	public class Column
	{
		[JsonProperty(PropertyName = "type")]
		public string Type { get; set; }
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
		[JsonProperty(PropertyName = "dataType")]
		public string DataType { get; set; }
		[JsonProperty(PropertyName = "isNameInferred", NullValueHandling = NullValueHandling.Ignore)]
		public bool? IsNameInferred { get; set; }
		[JsonProperty(PropertyName = "isDataTypeInferred", NullValueHandling = NullValueHandling.Ignore)]
		public bool? IsDataTypeInferred { get; set; }
		[JsonProperty(PropertyName = "isHidden", NullValueHandling = NullValueHandling.Ignore)]
		public bool? IsHidden { get; set; }
		[JsonProperty(PropertyName = "isUnique", NullValueHandling = NullValueHandling.Ignore)]
		public bool? IsUnique { get; set; }
		[JsonProperty(PropertyName = "isKey", NullValueHandling = NullValueHandling.Ignore)]
		public bool? IsKey { get; set; }
		[JsonProperty(PropertyName = "isNullable", NullValueHandling = NullValueHandling.Ignore)]
		public bool? IsNullable { get; set; }
		[JsonProperty(PropertyName = "expression")]
		[JsonConverter(typeof(SingleOrArrayConverter<string>))]
		public List<string> Expression { get; set; }
		[JsonProperty(PropertyName = "sourceColumn")]
		public string SourceColumn { get; set; }
		[JsonProperty(PropertyName = "sortByColumn")]
		public string SortByColumn { get; set; }
		[JsonProperty(PropertyName = "summarizeBy")]
		public string SummarizeBy { get; set; }
		[JsonProperty(PropertyName = "keepUniqueRows", NullValueHandling = NullValueHandling.Ignore)]
		public bool? KeepUniqueRows { get; set; }
		[JsonProperty(PropertyName = "tableDetailPosition")]
		public int? TableDetailPosition { get; set; }
		[JsonProperty(PropertyName = "formatString")]
		public string FormatString { get; set; }
		[JsonProperty(PropertyName = "sourceProviderType")]
		public string SourceProviderType { get; set; }
		[JsonProperty(PropertyName = "isDefaultLabel", NullValueHandling = NullValueHandling.Ignore)]
		public bool? IsDefaultLabel { get; set; }
		[JsonProperty(PropertyName = "annotations")]
		public List<Annotation> AnnotationsList { get; set; }
	}

	public class DataSource
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
		[JsonProperty(PropertyName = "connectionString")]
		public string ConnectionString { get; set; }
		[JsonProperty(PropertyName = "impersonationMode")]
		public string ImpersonationMode { get; set; }
		[JsonProperty(PropertyName = "annotations")]
		public List<AnnotationSingleValue> AnnotationSingleValuesList { get; set; }
	}

	public class Annotation
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
		[JsonProperty(PropertyName = "value")]
		[JsonConverter(typeof(SingleOrArrayConverter<string>))]
		public List<string> Value { get; set; }
	}

	public class AnnotationSingleValue
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
		[JsonProperty(PropertyName = "value")]
		public string Value { get; set; }
	}

	class SingleOrArrayConverter<T> : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return (objectType == typeof(List<T>));
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			List<T> list = (List<T>)value;
			if (list.Count == 1)
			{
				value = list[0];
			}
			serializer.Serialize(writer, value);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JToken token = JToken.Load(reader);
			if (token.Type == JTokenType.Array)
			{
				return token.ToObject<List<T>>();
			}
			return new List<T> { token.ToObject<T>() };
		}

		public override bool CanWrite
		{
			get { return true; }
		}
	}
}
