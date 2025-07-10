using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
#if WINZOR
using System.Threading.Tasks;
#endif
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Internal;
#if WINZOR
using Enterprise.ZArchitecture.GUI.JSInterop;
using Microsoft.JSInterop;
#endif

namespace Enterprise.ZArchitecture.GUI
{
	[ToolboxItem(false)]
#if !WINZOR
	[DefaultBindingProperty("RtfZBlob")]
#else
	[DefaultBindingProperty("HtmlZBlob")]
#endif

	public partial class ZAutoCompleteTextBox : KRichTextBox,
		IDropFormParent,
		IHotkeyProvider,
		IBackColorMutable,
		IDataBoundControl
	{
		internal readonly ZTextBoxBaseContextMenuManager contextMenuManager;

		public IAutoCompleteField AutocompleteManager { get; set; }

		public ZAutoCompleteTextBox()
		{
			AcceptsTab = true;

			ColorChanger = new ActiveControlColorChanger(this);
			contextMenuManager = new ZTextBoxBaseContextMenuManager(this, this, new NoMacroBox());

			Hotkeys.RegisterHotKey(Keys.Control | Keys.V, MarkKeypressAsHandledWhenPastingImage);
			Hotkeys.RegisterHotKey(Keys.Shift | Keys.Insert, MarkKeypressAsHandledWhenPastingImage);
			Hotkeys.RegisterHotKey(Keys.Control | Keys.Z, Undo);

#if !WINZOR
			TextChanged += (o, e) => rtfBlobCache = ZBlob.Empty;
#else
			TextChanged += (o, e) => htmlBlobCache = ZBlob.Empty;
			dotNetObjectReference = DotNetObjectReference.Create(this);
#endif
		}

		bool MarkKeypressAsHandledWhenPastingImage(object sender, Keys key) => !(CanPaste(DataFormats.GetFormat(DataFormats.Text)) || CanPaste(DataFormats.GetFormat(DataFormats.UnicodeText)));

		readonly List<ICodeDescription> selectedItems = new List<ICodeDescription>();
		public IEnumerable<ICodeDescription> Tags => CollectTags();
		public IEnumerable<ICodeDescription> DistinctTags => Tags.Distinct();

		public event EventHandler<ItemSelectedEventArgs> ItemSelected;

		public char PasswordChar => '\0';

		List<ICodeDescription> CollectTags()
		{
			var finalTags = new List<ICodeDescription>();
			var text = Text;
			var orderedTags = selectedItems.OrderByDescending(x => x.Code.Length);
			foreach (var a in orderedTags)
			{
				var startIndex = text.IndexOf(AutocompleteManager.MagicChar + a.Code, StringComparison.CurrentCulture);
				if (startIndex != -1)
				{
					text = text.Remove(startIndex, a.Code.Length + 1);
					finalTags.Add(a);
				}
			}
			return finalTags;
		}

		void SelectItem(ICodeDescription item)
		{
			var strItem = item.Code;
#if !WINZOR
			var magicSymbol = LastMagicCharIndex;
			var cursorPosition = SelectionStart;

			Text = InsertAndReplace(Text, strItem, magicSymbol + 1, cursorPosition);

			SelectionStart = magicSymbol + strItem.Length + 1;
			SelectionLength = 0;
#endif
			selectedItems.Add(item);

			HideDropMenu();

			ItemSelected?.Invoke(this, new ItemSelectedEventArgs(item));
#if WINZOR
			InvokeRenderDispatcher(async () => await (Interop?.InsertTagAndUpdateTagsAsync(ElementReference,
				strItem, selectedItems.Select(t => t.Code)) ?? Task.CompletedTask));
#endif
		}

		public ICodeDescription LastSelectedItem => null;

#if !WINZOR
		static string InsertAndReplace(string s, string replacement, int startIndex, int endIndex)
		{
			return (startIndex == endIndex) ? s.Insert(startIndex, replacement) :
				s.Substring(0, startIndex) + replacement + s.Substring(endIndex);
		}

		#region Location

		[return: DpiState(DpiState.ScaledVariant)]
		Point GetCaretPosition()
		{
			var selectionStart = SelectionStart;
			Point dropDownPosition;
			using (SuspendColoringWords())
			{
				Text = Text.Insert(selectionStart, AutocompleteManager.MagicChar.ToString());
				dropDownPosition = GetPositionFromCharIndex(selectionStart + 1);
				Text = Text.Remove(selectionStart, 1);
			}
			SelectionStart = selectionStart;

			return dropDownPosition + MagicCharHeight;
		}

		Size MagicCharSize
		{
			get
			{
				if (magicCharSize.IsEmpty)
				{
					magicCharSize = GetSizeOfCharacter(AutocompleteManager.MagicChar);
				}
				return magicCharSize;
			}
		}
		Size magicCharSize = Size.Empty;

		Size MagicCharHeight
		{
			get
			{
				if (magicCharHeight.IsEmpty)
				{
					magicCharHeight = ControlDpiScalingHelper.NewScaledSize(0, MagicCharSize.Height, false);
				}
				return magicCharHeight;
			}
		}

		Size magicCharHeight = Size.Empty;

		[return: DpiState(DpiState.ScaledVariant)]
		Size GetSizeOfCharacter(char ch)
		{
			using (var graphics = CreateGraphics())
			{
				return graphics.MeasureString(ch.ToString(), Font).ToSize();
			}
		}

		#endregion
#endif
		#region Key handling
#if !WINZOR
		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			if (AutocompleteManager != null && e.KeyChar == AutocompleteManager.MagicChar)
			{
				ShowDropMenu();
			}

			base.OnKeyPress(e);
		}

		void ColorWords(bool isNew = false)
		{
			using (SuspendColoringWords())
			{
				var originalSelectionStart = SelectionStart;
				var originalSelectionLength = SelectionLength;

				undoCount = 0;
				SelectionStart = 0;
				SelectionLength = Text.Length;
				SelectionColor = Color.Black;

				foreach (var tag in DistinctTags)
				{
					var matches = Regex.Matches(Text, Regex.Escape(AutocompleteManager.MagicChar + tag.Code));
					foreach (Match match in matches)
					{
						SelectionStart = match.Index;
						SelectionLength = match.Length;
						SelectionColor = Color.Blue;
					}
				}

				if (isNew)
				{
					SelectionStart = originalSelectionStart;
					SelectionLength = 1;
					SelectionColor = Color.Blue;
				}

				SelectionStart = originalSelectionStart;
				SelectionLength = originalSelectionLength;
			}
		}
#endif

		public new void Undo()
		{
			using (SuspendColoringWords())
			{
				if (CanUndo)
				{
					for (var i = 0; i <= undoCount; i++)
					{
						base.Undo();
					}
				}
			}
		}

		int undoCount;

		public new Color SelectionColor
		{
			get { return base.SelectionColor; }
			set
			{
				undoCount++;
				base.SelectionColor = value;
			}
		}

		internal bool SelectHighlightedOrFirstItem(ICodeDescription item = null)
		{
			var selectedItem = item ?? dropForm.SelectedItem ?? dropForm.FirstItem;
			if (selectedItem != null)
			{
				SelectItem(selectedItem);
				return true;
			}

			return false;
		}

		IDisposable SuspendColoringWords()
		{
#if !WINZOR
			suspendColoringWords = true;
			return new DisposableAction(() => suspendColoringWords = false);
#else
			return new DisposableAction(() => { });
#endif
		}

#if !WINZOR
		bool suspendColoringWords;

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			if (AutocompleteManager != null && DropMenuIsOpen && !suspendColoringWords)
			{
				if (GetCurrentWord() == null)
				{
					HideDropMenu();
				}
				else
				{
					dropForm.RefreshList();
				}
			}
			else if (!DropMenuIsOpen && !suspendColoringWords)
			{
				ColorWords();
			}
		}
#endif

		#endregion

#if !WINZOR
		string GetCurrentWord()
		{
			var magicCharIndex = LastMagicCharIndex;
			if (magicCharIndex < 0)
			{
				return null;
			}

			var wordStart = magicCharIndex + 1;
			if (wordStart >= Text.Length)
			{
				return string.Empty;
			}

			var length = SelectionStart - wordStart;
			if (length < 0)
			{
				return null;
			}

			var textSinceMagicChar = Text.Substring(wordStart, length);
			return textSinceMagicChar.Any(char.IsWhiteSpace) ? null : textSinceMagicChar;
		}

		int LastMagicCharIndex
		{
			get { return SelectionStart > 0 ? Text.LastIndexOf(AutocompleteManager.MagicChar, SelectionStart - 1) : -1; }
		}
#endif

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			return Hotkeys.ProcessCmdKey(this, keyData) || base.ProcessCmdKey(ref msg, keyData);
		}

		string IHotkeyProvider.TypeNameForDisplay => Res.GetString("6fc8a5fd-77cb-4515-abd0-986b44f8f911", "Text Box");
		public HotkeyRegister Hotkeys { get; } = new HotkeyRegister();

		public ActiveControlColorChanger ColorChanger { get; }

		public bool EnableValidStateColor { get; set; }

		#region Hiding the menu when its no longer valid

		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);
			HideDropMenu();
		}

		readonly Keys[] keysToCloseDropFor = new[] { Keys.Home, Keys.End, Keys.Escape, Keys.Delete, Keys.Left, Keys.Right };
		readonly Keys[] keysToSelectFor = new[] { Keys.Tab, Keys.Enter };
		protected override void OnKeyDown(KeyEventArgs e)
		{
#if !WINZOR
			var newSelectionStart = -1;
#endif
			if (DropMenuIsOpen)
			{
				if (dropForm.HandleCommandKey(e.KeyCode))
				{
					e.Handled = true;
					e.SuppressKeyPress = true;
				}
				else if (keysToCloseDropFor.Contains(e.KeyCode & ~Keys.Modifiers))
				{
					HideDropMenu();
				}
			}
#if !WINZOR
			else if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
			{
				newSelectionStart = RemoveTags(e.KeyCode == Keys.Delete);
			}
#endif

			base.OnKeyDown(e);

#if !WINZOR
			if (newSelectionStart >= 0)
			{
				SelectionStart = newSelectionStart;
			}
#endif

			if (DropMenuIsOpen && keysToSelectFor.Contains(e.KeyCode))
			{
				e.Handled = SelectHighlightedOrFirstItem();
				HideDropMenu();
#if !WINZOR
				if (e.KeyCode == Keys.Tab || e.KeyCode == Keys.Enter)
				{
					ColorWords();
				}
#endif
			}
		}

#if !WINZOR
		int RemoveTags(bool isDelete)
		{
			return SelectionLength == 0 ? RemoveTag(isDelete) : RemoveTagsHighlighted(isDelete);
		}

		int RemoveTag(bool isDelete)
		{
			var deleteModifier = isDelete ? -1 : 0;

			bool CurserIsWithinMatch(Match n) => SelectionStart > n.Index + deleteModifier && SelectionStart < n.Index + n.Length + 1 + deleteModifier;
			var match = DistinctTags
				.SelectMany(x => Regex.Matches(Text, Regex.Escape(AutocompleteManager.MagicChar + x.Code)).Cast<Match>())
				.FirstOrDefault(CurserIsWithinMatch);

			if (match != null)
			{
				Text = Text.Remove(match.Index, match.Length)
						.Insert(match.Index, " ");
				return match.Index + deleteModifier + 1;
			}

			return SelectionStart;
		}

		int RemoveTagsHighlighted(bool isDelete)
		{
			var leftDelete = GrabLargestMatch(x => SelectionStart > x.Index && SelectionStart < x.Index + x.Length);
			var rightDelete = GrabLargestMatch(x => SelectionStart + SelectionLength > x.Index && SelectionStart + SelectionLength < x.Index + x.Length + 1);

			if (leftDelete != null && rightDelete != null)
			{
				Text = Text.Remove(leftDelete.Index, rightDelete.Index + rightDelete.Length - leftDelete.Index)
				.Insert(leftDelete.Index, isDelete ? string.Empty : " ");
				return leftDelete.Index + 1;
			}
			else if (leftDelete != null)
			{
				Text = Text.Remove(leftDelete.Index, SelectionStart + SelectionLength - leftDelete.Index)
					.Insert(leftDelete.Index, isDelete ? string.Empty : " ");
				return leftDelete.Index + 1;
			}
			else if (rightDelete != null)
			{
				var selectionStartToReturn = SelectionStart;
				Text = Text.Remove(SelectionStart, rightDelete.Index + rightDelete.Length - SelectionStart)
					.Insert(selectionStartToReturn, isDelete ? string.Empty : " ");
				return selectionStartToReturn + 1;
			}
			return SelectionStart;
		}

		Match GrabLargestMatch(Func<Match, bool> predicate)
		{
			return DistinctTags
				.SelectMany(x => Regex.Matches(Text, Regex.Escape(AutocompleteManager.MagicChar + x.Code)).Cast<Match>())
				.FirstOrDefault(predicate);
		}
#endif

		#endregion

		[SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Justification = "It's complaining about the ColorChanger's AutoProperty's backing field. We are safely disposing it.")]
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				dropForm?.Dispose();
				ColorChanger.Dispose();
				contextMenuManager.Dispose();
#if WINZOR
				dotNetObjectReference?.Dispose();
#endif
			}

			base.Dispose(disposing);
		}

		#region Drop Form

		ZDropForm dropForm;

		bool DropMenuIsOpen => dropForm != null;

		void ShowDropMenu()
		{
			if (DropMenuIsOpen)
			{
				HideDropMenu();
			}

			dropForm = new ZDropForm(this);
#if !WINZOR
			dropForm.ShowDropDown(PointToScreen(GetCaretPosition()), false);
			ColorWords(isNew: true);
#else
			dropForm.ShowDropDown(PointToScreen(new Point(0,0)), false);
#endif
		}

		void HideDropMenu()
		{
			if (dropForm != null)
			{
				dropForm.HideDropDown();
				dropForm.Dispose();

				dropForm = null;
			}
		}

		Font IDropFormParent.Font => Font;
		int IDropFormParent.MaxItemsToShowInDropDown => 30; // Arbitrary
		string IDropFormParent.Code => GetCurrentWord();
		ZDropEdit.ShowInDropDownList IDropFormParent.ShowInDropDown => ZDropEdit.ShowInDropDownList.OnlyShowDescription;
		int IDropFormParent.MinDropDownWidth => ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
		bool IDropFormParent.ShowHorizontalScrollBar => false;
		bool IDropFormParent.SupportsEmptyCode => false;
		bool IDropFormParent.ShouldCloseOnMouseDown(Point mouseLoc) => true;
		bool IDropFormParent.IsItemValidForAutoComplete(ICodeDescription item) => true;
		public bool ShowColorInDropDown => false;
		MultilingualString IDropFormParent.GetMultilingualValue(ICodeDescription item) => (NoResString)item.Code;

		IList IDropFormParent.GetFilteredListForDropDown()
		{
			var word = GetCurrentWord();
			return word != null ? AutocompleteManager.GetList(word) : new List<ICodeDescription>();
		}

		void IDropFormParent.OnDropDownClosed()
		{
		}

		void IDropFormParent.OnItemSelected(ICodeDescription item, bool commitValue)
		{
			SelectHighlightedOrFirstItem(item);
			HideDropMenu();
		}

#if DEBUG
		internal ZDropForm DropForm_Exposed => dropForm;
#endif

		#endregion

		#region IDataBoundControl

		bool contextMenuInitialised;
		public override ContextMenuStrip ContextMenuStrip
		{
			get
			{
				if (!contextMenuInitialised && base.ContextMenuStrip == null)
				{
					contextMenuInitialised = true;
					contextMenuManager.InitializeContextMenu();
				}
				return base.ContextMenuStrip;
			}
			set => base.ContextMenuStrip = value;
		}

		string IDataBoundControl.DataMember => DataBoundControl.GetDefaultImplementation(this).DataMember;
		object IDataBoundControl.DataSource => DataBoundControl.GetDefaultImplementation(this).DataSource;
		Type IDataBoundControl.DataSourceType => DataBoundControl.GetDefaultImplementation(this).DataSourceType;

#if !WINZOR
		public virtual void SetDataBinding(object dataSource, string dataMember)
			=> DataBoundControl.GetDefaultImplementation(this).SetDataBinding(dataSource, dataMember);

		public static PropertyDescriptor[] GetPropertyDescriptors()
			=> new ControlPropertyDescriptorBuilder<ZRichTextBox>()
				.Property("RtfZBlob", ZBlob.Empty)
				.Result;

		[BindingMetaDataProperty(MetaDataTypes.MaxLength, "MaxLength")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZBlob RtfZBlob
		{
			get { return (!rtfBlobCache.IsEmpty) ? rtfBlobCache : (rtfBlobCache = ZBlob.FromUTF8(Rtf)); }
			set
			{
				Rtf = (value.IsEmpty) ? ZString.Empty : value.ToUTF8();
				rtfBlobCache = value;
			}
		}

		ZBlob rtfBlobCache = ZBlob.Empty;
#else
		public virtual void SetDataBinding(object dataSource, string dataMember)
			=> DataBoundControl.GetDefaultImplementation(this).SetDataBinding(dataSource, dataMember + "_HTML");

		public static PropertyDescriptor[] GetPropertyDescriptors()
			=> new ControlPropertyDescriptorBuilder<ZRichTextBox>()
				.Property("HtmlZBlob", ZBlob.Empty)
				.Result;

		[BindingMetaDataProperty(MetaDataTypes.MaxLength, "MaxLength")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZBlob HtmlZBlob
		{
			get { return (!htmlBlobCache.IsEmpty) ? htmlBlobCache : (htmlBlobCache = ZBlob.FromUTF8(Html)); }
			set
			{
				Html = (value.IsEmpty) ? ZString.Empty : value.ToUTF8();
				htmlBlobCache = value;
			}
		}

		ZBlob htmlBlobCache = ZBlob.Empty;
#endif
		public event EventHandler RtfZBlobChanged
		{
			add { TextChanged += value; }
			remove { TextChanged -= value; }
		}

		public override int MaxLength
		{
			get => base.MaxLength <= 0 ? VeryLargeLength : base.MaxLength;
			set => base.MaxLength = value <= 0 ? VeryLargeLength : value;
		}

		const int VeryLargeLength = 30_000;

		#endregion
	}

	public class ItemSelectedEventArgs : EventArgs
	{
		public ItemSelectedEventArgs(object item)
		{
			SelectedItem = item;
		}

		public object SelectedItem { get; }
	}
}

#region Testing
#if DEBUG

namespace Enterprise.ZArchitecture.GUI
{
	class DummyAutocompleteField<T> : IAutoCompleteField
	{
		readonly Func<T, string> getKey;
		readonly IList<T> items;

		public DummyAutocompleteField(IList<T> objects, Func<T, string> getKey)
		{
			this.items = objects;
			this.getKey = getKey;
		}

		public char MagicChar => '@';

		public string GetDisplayName(object item)
		{
			return getKey((T)item);
		}

		[SuppressWeaklyTypedCollectionMessage] // Matching the interface
		public IList GetList(string partialResult)
		{
			partialResult = partialResult.ToUpperInvariant();

			return items
				.Where(o => getKey(o).ToUpperInvariant().Contains(partialResult))
				.Select(code => new CodeDescriptionPair(GetDisplayName(code), GetDisplayName(code)))
				.ToList();
		}
	}

	class FruitsField : DummyAutocompleteField<string>
	{
		public static string[] AllFruits => new[] { "Apple", "Banana", "Lemon", "Pear", "Durion", "Tomato", "Carrots", "More Lemons", "Gorana", "Watermelon" };

		public FruitsField() : base(AllFruits, f => f) { }
	}
}

#endif
#endregion
