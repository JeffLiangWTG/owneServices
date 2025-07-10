using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class DynamicContent : DocumentElement, IText, INotificationProvider
	{
		public DynamicContent(IDocumentCell cell, PointF location, SizeF size)
			: this(cell, location, size, RectangleF.Empty)
		{
		}

		public DynamicContent(IDocumentCell cell, PointF location, SizeF size, RectangleF padding)
			: base(location, size)
		{
			Argument.NotNull(cell, nameof(cell));

			this.cell = cell;
			this.Padding = padding;
		}

		readonly IDocumentCell cell;

		public IDocumentCell Cell => cell;

		public override ElementType ElementType => ElementType.DynamicContent;

		public string Content => Convert.ToString(cell.Value, CultureInfo.InvariantCulture);

		public RectangleF Padding { get; }
		public IFont Font { get; set; }
		public Alignment HAlignment { get; set; }
		public Alignment VAlignment { get; set; }
		public bool Wrap { get; set; }

		public Color BackgroundColor { get; set; }

		public IMacroScope Scope => cell.Scope;

		public IMacroExpression MacroExpression => cell.MacroExpression;

		public IDictionary<string, IDynamicData> EditableData => cell.EditableData;

		public bool HasDynamicContent => cell.HasDynamicContent;

		public bool HasOverriddenData => cell.HasOverriddenData;

		public void CancelOverride()
		{
			cell.CancelOverride();
		}

		public void AddOnValueChangedAction(Action action)
		{
			cell.AddOnValueChangedAction(action);
		}

		public void RemoveOnValueChangedAction(Action action)
		{
			cell.RemoveOnValueChangedAction(action);
		}

		public bool IsEditing
		{
			get; set;
		}

		public IEnumerable<INotification> Notifications => cell.Notifications;
	}
}
