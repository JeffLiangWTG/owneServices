using System;
using System.Globalization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.Business
{
	public class XQueryFilterInfo
	{
		public XQueryFilterInfo(ZString path, int maxLength = int.MaxValue)
		{
			Path = path;
			MaxLength = maxLength;
		}
		public ZString Path { get; }
		public int MaxLength { get; }

		public XQueryFilterInfo(ZString path1, ZString path2, int maxLength1 = int.MaxValue, int maxLength2 = int.MaxValue)
		{
			Path1 = path1;
			Path2 = path2;
			MaxLength1 = maxLength1;
			MaxLength2 = maxLength2;
		}

		public ZString Path1 { get; }

		public ZString Path2 { get; }

		public int MaxLength1 { get; }

		public int MaxLength2 { get; }
	}

	public static class XQueryFilterHelper
	{
		public static IDisposable SetNamespaceTemp(string namespaceVersion)
		{
			XQueryFilterHelper.namespaceVersion = namespaceVersion;
			return new DisposableAction(() => XQueryFilterHelper.namespaceVersion = UniversalXmlInfo.Namespace_2011_11);
		}
		[ThreadSafe]
		static string namespaceVersion = UniversalXmlInfo.Namespace_2011_11;

		public static ZQuery GenerateXQuery(Func<SchemaStringColumn, ZQuery> createZQuery, XQueryFilterInfo info)
		{
			return createZQuery(GenerateXQueryColumn(ObjectFactory.Get<IShipmentXQueryPaths>().GetNamespace(namespaceVersion, info.Path), info.MaxLength));
		}

		public static ZQuery GenerateXQuery(Func<SchemaStringColumn, SchemaStringColumn, ZQuery> createZQuery, XQueryFilterInfo info)
		{
			return createZQuery(GenerateXQueryColumn(ObjectFactory.Get<IShipmentXQueryPaths>().GetNamespace(namespaceVersion, info.Path1), info.MaxLength1),
				GenerateXQueryColumn(ObjectFactory.Get<IShipmentXQueryPaths>().GetNamespace(namespaceVersion, info.Path2), info.MaxLength2));
		}

		// Create a virtual column for searching.
		// Because StmTemplateRecordSchema.STR_Data is xml data type, we need to do serching by XQuery.
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "part of sql clause")]
		static SchemaStringColumn GenerateXQueryColumn(ZString path, int maxLength = int.MaxValue)
		{
			return new SchemaStringColumn(StmTemplateRecordSchema.Instance,
						string.Format(CultureInfo.InvariantCulture, @"{0}.value('declare default element namespace ""{1}""; ({2})[1]', 'varchar({3})')",
							StmTemplateRecordSchema.STR_Data.Name, namespaceVersion, path, maxLength != int.MaxValue ? maxLength.ToString(CultureInfo.InvariantCulture) : "max"),
						0, System.Data.SqlDbType.VarChar, "", true, maxLength);
		}
	}
}
