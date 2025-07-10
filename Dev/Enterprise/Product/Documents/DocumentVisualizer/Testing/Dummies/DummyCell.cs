using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Testing
{
	[DebuggerDisplay("[{TopRow},{LeftColumn}] : [{BottomRow},{RightColumn}] {Value}")]
	sealed class DummyCell : IDocumentCell
	{
		public IFormat Format
		{
			get; set;
		}

		public double Height
		{
			get; set;
		}

		public double Width
		{
			get; set;
		}

		public RectangleF Padding
		{
			get; set;
		}

		public int BottomRow
		{
			get; set;
		}

		public int LeftColumn
		{
			get; set;
		}

		public int RightColumn
		{
			get; set;
		}

		public int TopRow
		{
			get; set;
		}

		public bool HasErrors
		{
			get; set;
		}

		public void Add(INotification notification)
		{
			notifications.Add(notification);
		}

		public void Clear()
		{
			notifications.Clear();
		}

		public IEnumerable<INotification> Notifications => notifications;
		readonly List<INotification> notifications = new List<INotification>();

		public bool HasDynamicContent
		{
			get; set;
		}

		public bool HasOverriddenData
		{
			get; set;
		}

		public void CancelOverride()
		{
			HasOverriddenData = false;
		}

		public object Value
		{
			get => ValueProvider != null ? ValueProvider() : value;
			set => this.value = value;
		}

		object value;

		public Func<object> ValueProvider { get; set; }

		public object Evaluate()
		{
			return Value;
		}

		public IDrawing Drawing => drawing;
		IDrawing drawing;

		public void SetDrawing(IDrawing drawing)
		{
			this.drawing = drawing;
		}

		public IMacroExpression MacroExpression { get; set; }

		public IDictionary<string, IDynamicData> EditableData => editableData ?? (editableData = new Dictionary<string, IDynamicData>());
		IDictionary<string, IDynamicData> editableData;

		public IMacroScope Scope { get; set; }

		public IDictionary<string, Action> SubscribedObservers => valueChangedObservers ?? (valueChangedObservers = new Dictionary<string, Action>());
		IDictionary<string, Action> valueChangedObservers;

		public void Dispose()
		{
		}

		public void AddOnValueChangedAction(Action callback)
		{
		}

		public void RemoveOnValueChangedAction(Action action)
		{
		}
	}
}