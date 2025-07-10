using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentVisualizer.Business
{
	public sealed class DocumentPivot : IDocumentPivot
	{
		DocumentPivot(Key key, IReadOnlyCollection<IStmMenuTemplatePivot> pivots)
		{
			this.key = key;
			this.pivots = pivots;
		}

		readonly Key key;
		readonly IReadOnlyCollection<IStmMenuTemplatePivot> pivots;

		public string DocumentTitle => key.DocumentTitle;
		public string DataContext => key.DataContext;
		public string DataStoreName => key.DataStoreName;
		public string DocType => key.DocType;
		public Guid TemplatePK => key.TemplatePK;

		public string Purpose => key.Purpose;
		public string MenuName => key.MenuName;
		public string[] DeliveryModes => deliveryModes ?? (deliveryModes = pivots.Select(p => p.SI_PrintCopyType.ToString()).ToArray());
		string[] deliveryModes;

		public bool SaveCopyToEDocs => key.SaveCopyToEDocs;
		public bool IsSystemDefined => key.IsSystemDefined;

		public static IDocumentPivot[] Create(IReadOnlyCollection<IStmMenuTemplatePivot> pivots)
		{
			Key GetPivotKey(IStmMenuTemplatePivot pivot)
			{
				return new Key
				{
					DocumentTitle = pivot.SI_DocumentTitle,
					DataStoreName = pivot.SI_DataStoreName,
					DataContext = pivot.Template?.SO_DataContext ?? ZString.Empty,
					DocType = pivot.DocType?.RT_DocType ?? ZString.Empty,
					TemplatePK = pivot.SI_SO.ToGuid(),
					Purpose = pivot.MenuItem?.SU_Purpose ?? ZString.Empty,
					MenuName = pivot.MenuItem?.SU_MenuName ?? ZString.Empty,
					SaveCopyToEDocs = pivot.DocType?.RT_LogSystemCreatedDocsToEDocs ?? false,
					IsSystemDefined = pivot.SI_IsSystemDefined,
				};
			}

			return pivots
				.GroupBy(GetPivotKey)
				.Select(g => new DocumentPivot(g.Key, g.ToArray()))
				.ToArray();
		}

		struct Key
		{
			public string DocumentTitle { get; set; }
			public string DataStoreName { get; set; }
			public string DataContext { get; set; }
			public string DocType { get; set; }
			public Guid TemplatePK { get; set; }
			public string Purpose { get; set; }
			public string MenuName { get; set; }
			public bool SaveCopyToEDocs { get; set; }
			public bool IsSystemDefined { get; set; }
		}
	}
}
