using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Enterprise.BusinessObjectGenerator.ModelView
{
	public class ModelViewContext
	{
		public virtual View ModelView { get; set; }

		public virtual string ModelName { get; set; }

		public string ParentDirectory { get; set; }

		public string DevelopmentAttributeName => "DevelopmentOnly";

		public string DefinitionNamespace { get; set; }

		public List<string> Columns { get; set; }

		public string AddInfoColumnName { get; set; }

		public string NAddInfoColumnName { get; set; }

		public string ClusterKeyColumnName { get; set; }

		public string PKColumnName { get; set; }

		public string FileNameSpace { get; }

		public bool HasIndex => ModelView?.AddInfos?.Any(a => a.Indexed) ?? false;

		public string FilePathOfModelDefinition
		{
			get
			{
				var fileName = ModelName + ".model.cs";
				return Path.Combine(ParentDirectory, fileName);
			}
		}

		public string FilePathOfSqlView
		{
			get
			{
				var fileName = ModelName + ".model.sql";
				return Path.Combine(ParentDirectory, fileName);
			}
		}

		public string FilePathOfSqlIndexView
		{
			get
			{
				var fileName = ModelName + ".index.sql";
				return Path.Combine(ParentDirectory, fileName);
			}
		}
	}
}
