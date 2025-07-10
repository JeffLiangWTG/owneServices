using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public sealed class EmptyDocument : IDocument
	{
		public EmptyDocument(string name, string dataContext, IDynamicData data = null)
		{
			this.Name = name;
			DataContext = string.IsNullOrEmpty(dataContext) ? (NoResString)"empty" : dataContext;
			this.Data = data ?? new object().MakeDynamic();
		}

		public string Name { get; }
		public Margins Margins => margins ?? (margins = new Margins());
		Margins margins;

		public PageDimensions PageDimensions => pageDimensions ?? (pageDimensions = new PageDimensions());
		PageDimensions pageDimensions;

		public bool PrintContentCenteredHorizontally => false;

		public IReadOnlyList<IRow> Rows => rows ?? (rows = new List<IRow>());
		IReadOnlyList<IRow> rows;

		public IReadOnlyList<IColumn> Columns => columns ?? (columns = new List<IColumn>());
		IReadOnlyList<IColumn> columns;

		public IEnumerable<int> PageBreaks => Enumerable.Empty<int>();

		public IMacroScope Scope => scope ?? (scope = new MacroScope(Data));
		IMacroScope scope;

		public IMacroEvaluationContext Context => null;

		public IEnumerable<INotification> Notifications => notifications ?? (notifications = new List<INotification>());
		List<INotification> notifications;

		public void Add(INotification notification)
		{
			if (notification == null)
			{
				return;
			}

			notifications = notifications ?? new List<INotification>();
			notifications.Add(notification);
		}

		public double HorizontalPrintOffset { get; set; }

		public IDynamicData Data { get; }

		public string DataContext { get; }

		public bool IsTranslatable { get; }

		public string Language { get; } = Enterprise.Core.Constants.Languages.EnglishAmerican;

		public IEnumerable<IPage> Pages => Enumerable.Empty<IPage>();

		public IReadOnlyDictionary<string, Func<object>> Resources => resources ?? (resources = new Dictionary<string, Func<object>>());
		IReadOnlyDictionary<string, Func<object>> resources;

		public bool IsValid => true;

		public ICell GetCell(int row, int column)
		{
			return GetDocumentCell(row, column);
		}

		public IDocumentCell GetDocumentCell(int row, int column)
		{
#if NET
			return new DocumentCell(this, new Core.Range
			{
				BottomRow = row,
				TopRow = row,
				LeftColumn = column,
				RightColumn = column
			});
#elif NETFRAMEWORK
			return new DocumentCell(this, new Range
			{
				BottomRow = row,
				TopRow = row,
				LeftColumn = column,
				RightColumn = column
			});
#else
#error Unexpected target platform
#endif
		}

		public void Dispose()
		{
		}
	}
}
