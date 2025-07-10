using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Testing
{
	public sealed class DummyDocument : DummyWorksheet, IDocument
	{
		public double HorizontalPrintOffset { get; set; }

		public IDynamicData Data { get; set; }

		public string DataContext { get; set; }

		public bool IsTranslatable { get; set; }

		public string Language { get; set; }

		public bool IsValid { get; set; }

		IEnumerable<IPage> IDocument.Pages => pages;

		public List<IPage> Pages => pages;

		readonly List<IPage> pages = new List<IPage>();

		public IDocumentCell GetDocumentCell(int row, int column)
		{
			return (IDocumentCell)GetCell(row, column);
		}

		IMacroScope IMacroScopeProvider.Scope
		{
			get
			{
				if (scope == null)
				{
					scope = new MacroScope(Data);
					scope.SetVariable(VariableNames.DocumentInternal, this);
				}

				return scope;
			}
		}

		IMacroScope scope;

		public void Dispose()
		{
			scope?.Dispose();
		}

		public IEnumerable<INotification> Notifications
		{
			get
			{
				return cells
					.Values
					.OfType<INotificationProvider>()
					.Distinct()
					.SelectMany(cell => cell.Notifications)
					.Concat(notifications);
			}
		}

		readonly HashSet<INotification> notifications = new HashSet<INotification>(new NotificationEqualityComparer());

		public void Add(INotification notification)
		{
			notifications.Add(notification);
		}

		public void Clear()
		{
			notifications.Clear();
		}

		public IMacroEvaluationContext Context
		{
			get
			{
				if (context == null)
				{
					context = new IMacroLibrary[] { new StandardLibrary(), new DocumentLibrary() }.CreateContext();
				}

				return context;
			}
		}

		IMacroEvaluationContext context;

		public IReadOnlyDictionary<string, Func<object>> Resources
		{
			get { return resources ?? (resources = new Dictionary<string, Func<object>>()); }
			set { resources = value; }
		}

		IReadOnlyDictionary<string, Func<object>> resources;
	}
}
