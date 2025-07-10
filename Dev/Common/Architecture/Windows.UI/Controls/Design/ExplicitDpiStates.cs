using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace CargoWise.Windows.UI.Design
{
	/// <summary>
	/// This is used when we dont have access to the source to add a DpiStateAnnotation. When possible please use the annotation
	/// </summary>
	public static class ExplicitDpiStates
	{
		static readonly ImmutableDictionary<string, DpiState> explicitPropertyStates;

		[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static ExplicitDpiStates()
		{
			var builder = ImmutableDictionary.CreateBuilder<string, DpiState>();

			AddScaledVarientValues(builder);
			AddUnscaledValues(builder);
			AddScaleXValues(builder);
			AddScaleYValues(builder);
			AddWildcards(builder);

			explicitPropertyStates = builder.ToImmutable();
		}

		public static DpiState GetState(string fullClassName, string memberName, bool checkWildcardVariants = true)
		{
			DpiState result;
			if (explicitPropertyStates.TryGetValue(fullClassName + "." + memberName, out result))
			{
				return result;
			}

			if (checkWildcardVariants && explicitPropertyStates.TryGetValue("*." + memberName, out result))
			{
				return result;
			}

			return DpiState.Unknown;
		}

		#region State values

		static void AddScaledVarientValues(ImmutableDictionary<string, DpiState>.Builder builder)
		{
			builder.Add("System.Windows.Forms.Control.Bounds", DpiState.ScaledVariant);
			builder.Add("System.Windows.Forms.Control.Location", DpiState.ScaledVariant);
			builder.Add("System.Windows.Forms.Control.Margin", DpiState.ScaledVariant);
			builder.Add("System.Windows.Forms.Control.MaximumSize", DpiState.ScaledVariant);
			builder.Add("System.Windows.Forms.Control.MinimumSize", DpiState.ScaledVariant);
			builder.Add("System.Windows.Forms.Control.Padding", DpiState.ScaledVariant);
			builder.Add("System.Windows.Forms.Control.Size", DpiState.ScaledVariant);

			builder.Add("System.Windows.Forms.Form.ClientSize", DpiState.ScaledVariant);
			builder.Add("System.Windows.Forms.TabControl.ItemSize", DpiState.ScaledVariant);

			builder.Add("System.Windows.Forms.ToolBar.ButtonSize", DpiState.ScaledVariant);
			builder.Add("System.Windows.Forms.ScrollableControl.AutoScrollMinSize", DpiState.ScaledVariant);
		}

		static void AddScaleYValues(ImmutableDictionary<string, DpiState>.Builder builder)
		{
			builder.Add("System.Windows.Forms.Control.Height", DpiState.ScaleY);
			builder.Add("System.Windows.Forms.Control.Top", DpiState.ScaleY);
			builder.Add("System.Windows.Forms.Control.Bottom", DpiState.ScaleY);
			builder.Add("System.Windows.Forms.Control.FontHeight", DpiState.ScaleY);
			builder.Add("System.Windows.Forms.DataGridView.ColumnHeadersHeight", DpiState.ScaleY);
		}

		static void AddScaleXValues(ImmutableDictionary<string, DpiState>.Builder builder)
		{
			builder.Add("System.Windows.Forms.Control.Width", DpiState.ScaleX);
			builder.Add("System.Windows.Forms.Control.Left", DpiState.ScaleX);
			builder.Add("System.Windows.Forms.Control.Right", DpiState.ScaleX);
		}

		static void AddWildcards(ImmutableDictionary<string, DpiState>.Builder builder)
		{
			//ScaledVariant
			builder.Add("*.Bounds", DpiState.ScaledVariant);
			builder.Add("*.Location", DpiState.ScaledVariant);
			builder.Add("*.Margin", DpiState.ScaledVariant);
			builder.Add("*.MaximumSize", DpiState.ScaledVariant);
			builder.Add("*.MinimumSize", DpiState.ScaledVariant);
			builder.Add("*.Padding", DpiState.ScaledVariant);
			builder.Add("*.Size", DpiState.ScaledVariant);

			//ScaleY
			builder.Add("*.Height", DpiState.ScaleY);
			builder.Add("*.Top", DpiState.ScaleY);
			builder.Add("*.Bottom", DpiState.ScaleY);
			builder.Add("*.FontHeight", DpiState.ScaleY);

			//ScaleX
			builder.Add("*.Width", DpiState.ScaleX);
			builder.Add("*.Left", DpiState.ScaleX);
			builder.Add("*.Right", DpiState.ScaleX);
		}

		static void AddUnscaledValues(ImmutableDictionary<string, DpiState>.Builder builder)
		{
			builder.Add("System.Windows.Forms.DataGrid.RowNumber", DpiState.Unscaled);
			builder.Add("System.Windows.Forms.DataGrid.FirstVisibleColumn", DpiState.Unscaled);

			builder.Add("System.Windows.Forms.TextBoxBase.SelectionStart", DpiState.Unscaled);
			builder.Add("System.Windows.Forms.TextBoxBase.SelectionLength", DpiState.Unscaled);

			builder.Add("System.Windows.Forms.TextBoxBase.GetFirstCharIndexFromLine", DpiState.Unscaled);
			builder.Add("System.Windows.Forms.TextBoxBase.GetFirstCharIndexOfCurrentLine", DpiState.Unscaled);
			builder.Add("System.Windows.Forms.TextBoxBase.GetLineFromCharIndex", DpiState.Unscaled);
			builder.Add("System.Windows.Forms.TextBoxBase.GetCharIndexFromPosition", DpiState.Unscaled);

			builder.Add("System.Windows.Forms.ListView.VirtualListSize", DpiState.Unscaled);

			builder.Add("System.Windows.Forms.ScrollBar.LargeChange", DpiState.Unscaled);
			builder.Add("System.Windows.Forms.ScrollBar.SmallChange", DpiState.Unscaled);
			builder.Add("System.Windows.Forms.ScrollBar.Value", DpiState.Unscaled);
			builder.Add("System.Windows.Forms.ScrollBar.Minimum", DpiState.Unscaled);
			builder.Add("System.Windows.Forms.ScrollBar.Maximum", DpiState.Unscaled);

			builder.Add("System.Windows.Forms.ProgressBar.Value", DpiState.Unscaled);
			builder.Add("System.Windows.Forms.ProgressBar.Maximum", DpiState.Unscaled);
		}

		#endregion
	}
}
