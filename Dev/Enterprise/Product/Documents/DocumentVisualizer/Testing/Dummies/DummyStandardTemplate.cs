using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DummyStandardTemplate : IStandardTemplate
	{
		public DummyStandardTemplate(IWorksheet worksheet)
		{
			this.worksheet = Argument.NotNull(worksheet, "worksheet");
		}

		readonly IWorksheet worksheet;

		public IStandardTemplateDefinition TemplateMetaData
		{
			get; set;
		}

		#region ITemplate members

		public string DataContext => Enterprise.DocumentVisualizer.Integration.DataContext.UXML;

		public bool IsSystemDefined { get; set; } = true;

		public bool EnableTranslation { get; set; }

		public IConfigSection Config => TemplateMetaData?.Config;

		public ITemplateSection PageHeader => TemplateMetaData?.PageHeader;

		public IEnumerable<IBodySection> Body => TemplateMetaData?.Body ?? Enumerable.Empty<IBodySection>();

		public ITemplateSection PageFooter => TemplateMetaData?.PageFooter;

		#endregion

		#region IWorksheet members

		public IReadOnlyList<IRow> Rows => worksheet.Rows;

		public IReadOnlyList<IColumn> Columns => worksheet.Columns;

		public ICell GetCell(int row, int column) => worksheet.GetCell(row, column);

		public Margins Margins => worksheet.Margins;

		public string Name => TemplateMetaData != null
			? TemplateMetaData.Config.DocumentName.GetValue<string>()
			: string.Empty;

		public PageDimensions PageDimensions => worksheet.PageDimensions;

		public bool PrintContentCenteredHorizontally { get; set; }

		IEnumerable<int> IWorksheet.PageBreaks => Enumerable.Empty<int>();

		#endregion

		#region INotificationsProvider members

		public IEnumerable<INotification> Notifications => Enumerable.Empty<Notification>();

		#endregion

		#region IDimensionsCalculator members

		double IDimensionsCalculator.CalculateRowHeight(double rowHeight) => rowHeight;

		double IDimensionsCalculator.CalculateColumnWidth(double columnWidth) => columnWidth;

		#endregion

		#region IResourceProvider members

		IReadOnlyDictionary<string, Func<object>> IResourceProvider.Resources => worksheet.Resources;

		#endregion

		#region ITemplate members

		public TemplateKind Kind => TemplateKind.Standard;
		public IWorksheet Worksheet => worksheet;
		public object Definition => this;

		#endregion
	}
}
