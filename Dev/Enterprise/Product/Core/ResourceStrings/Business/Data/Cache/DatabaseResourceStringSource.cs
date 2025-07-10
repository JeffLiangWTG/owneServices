using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ResourceStrings.Business
{
	public class DatabaseResourceStringSource : XmlResourceStringSource
	{
		public DatabaseResourceStringSource(string language)
			: base(language)
		{ }

		protected override Stream GetXmlStream()
		{
			var stmData = GetStmDataRecord();
			return stmData != null ? stmData.GetSD_BinaryValueReader() : null;
		}

		public override void WriteAll(IEnumerable<ResourceStringData> resources)
		{
			using (var ms = new MemoryStream())
			{
				var serializer = new ResourceStringXmSerializer(ms, Language);
				serializer.Serialize(resources);
				if (serializer.ProccesedCount == 0)
				{
					using (var cmd = Db.Connection.Command(string.Format(Culture.Invariant, "delete from {0} where {1} = @name", StmData.Schema.TableName, StmData.Schema.SD_Name)))
					{
						cmd.AddParameter("name", SqlDbType.VarChar, DataRecordName);
						cmd.ExecuteNonQuery();
					}
				}
				else
				{
					using (var cmd = Db.Connection.Command(string.Format(Culture.Invariant,
						@"if exists (select null from {0} where {2} = @name)
							update {0} set {3} = @binaryValue where {2} = @name
						  else
							insert into {0} ({1}, {2}, {3}, {4}) values (newid(), @name, @binaryValue, 'BIN')"
						, StmData.Schema.TableName, StmData.Schema.PK, StmData.Schema.SD_Name, StmData.Schema.SD_BinaryValue, StmData.Schema.SD_Type)))
					{
						cmd.AddParameter("name", SqlDbType.VarChar, DataRecordName);
						cmd.AddParameter("binaryValue", SqlDbType.VarBinary, ms.GetBuffer());
						cmd.ExecuteNonQuery();
					}
				}
			}
		}

		public static Dictionary<string, ResourceStringSourceDataPair> CreateAll(string[] languages)
		{
			var res = new Dictionary<string, ResourceStringSourceDataPair>();
			var dataRecordNames = languages.Select(language => language = DataRecordNamePrefix + "-" + language).ToArray();
			var query = new ZQuery(StmDataSchema.SD_Name, dataRecordNames);
			var allData = new BusinessObjectFactory().Load<StmData>(query);
			foreach (var data in allData)
			{
				var language = data.SD_Name.Substring(DataRecordNamePrefix.Length + 1);
				var source = new DatabaseResourceStringSource(language);
				res.Add(language,
					new ResourceStringSourceDataPair(source, DatabaseResourceStringSource.ReadAll(data.GetSD_BinaryValueReader())));
			}

			foreach (var language in languages)
			{
				if (!res.ContainsKey(language))
				{
					res.Add(language, new ResourceStringSourceDataPair(new DatabaseResourceStringSource(language), Enumerable.Empty<ResourceStringData>()));
				}
			}
			return res;
		}

		StmData GetStmDataRecord()
		{
			var query = new ZQuery(StmDataSchema.SD_Name, DataRecordName);
			return Factory.LoadTop1<StmData>(query);
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory() { NameForDebugging = "DatabaseResourceStringSource_New" }); }
		}
		BusinessObjectFactory factory;

		string DataRecordName
		{
			get { return DataRecordNamePrefix + "-" + Language; }
		}

		const string DataRecordNamePrefix = "ResourceStrings";

#if DEBUG
		public static void Disable()
		{
			disabled = true;
		}

		[SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule")]
		[SuppressMessage("Microsoft.Usage", "CA2211:Non-constant fields should not be visible")]
		public static bool disabled;
#endif
	}
}
