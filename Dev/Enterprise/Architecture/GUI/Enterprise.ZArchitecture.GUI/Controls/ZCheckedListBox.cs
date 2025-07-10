using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	[DefaultBindingProperty("BindingItems")]
	public class ZCheckedListBox : KCheckedListBox, IBindTo
	{
		#region Constructors

		public ZCheckedListBox()
		{
			InitializeEventHandlers();
			InitializeDisposable();
		}

		void FixHeight()
		{
			if (MultiColumn && Items.Count > 0 && ColumnWidth > 0)
			{
				var columnsCount = Width / ColumnWidth;
				var rows = (int)Math.Ceiling(Items.Count * 1.0 / columnsCount);
				var requiredHeight = ControlDpiScalingHelper.MarkAsScaled(ItemHeight * rows); //It's multiplying scaled by a factor.
				if (Height < requiredHeight)
				{
					ControlDpiScalingHelper.SetHeight(this, requiredHeight, false);
				}
			}
		}

		#endregion

		#region Initialization

		void InitializeEventHandlers()
		{
			ItemCheck += CheckedListBox_ItemCheck;
		}

		void InitializeDisposable()
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		#endregion

		#region BindingItems

		public ZBoolDescriptionPairList BindingItems
		{
			get { return fBindingItems; }
			set
			{
				if (value != fBindingItems)
				{
					UnhookBindingItemsEvents();
					fBindingItems = value;

					if (fBindingItems != null)
					{
						HookBindingItemsEvents();
					}

					UpdateItems();

					if (BindingItemsChanged != null)
					{
						BindingItemsChanged(this, EventArgs.Empty);
					}
				}
			}
		}
		ZBoolDescriptionPairList fBindingItems;

		void UpdateItems()
		{
			Items.Clear();
			if (fBindingItems != null)
			{
				try
				{
					isUpdatingItems = true;
					foreach (var pair in fBindingItems)
					{
						Items.Add(pair.Description, pair.Value);
					}
				}
				finally
				{
					isUpdatingItems = false;
				}
			}
		}
		bool isUpdatingItems;

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (Visible)
			{
				UpdateItems();
				FixHeight();
			}
		}

		public event EventHandler BindingItemsChanged;

		#endregion

		#region Checked Updated from GUI

		internal void CheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			if (e.Index < BindingItems.Count && e.Index >= 0)
			{
				var item = BindingItems[e.Index];
				BindingItems.SuspendOnChanged();
				try
				{
					if (SingleCheckMode && CheckedItems.Count > 0)
					{
						for (var i = 0; i < Items.Count; i++)
						{
							if (i != e.Index)
							{
								SetItemChecked(i, false);
							}
						}
					}

					item.Value = (e.NewValue == CheckState.Checked);
				}
				finally
				{
					BindingItems.ResumeOnChanged();
				}

				if (!isUpdatingItems)
				{
					BindingItems.FireOnPairChanged(item);
				}
			}
		}

		#endregion

		#region Checked Updated from Business

		void BindingItems_OnPairChangedForBinding(ZBoolDescriptionPairChangedEventArgs e)
		{
			var itemIndex = IndexOfPair(e.Pair);
			if (itemIndex != -1)
			{
				this.SetItemChecked(itemIndex, e.Pair.Value);
			}
		}

		int IndexOfPair(ZBoolDescriptionPair pair)
		{
			var result = -1;
			for (var i = 0; i < Items.Count && result == -1; i++)
			{
				if ((ZString)Items[i] == pair.Description)
				{
					result = i;
				}
			}

			return result;
		}

		#endregion

		#region Hooking / Unhooking BindingItems events

		void HookBindingItemsEvents()
		{
			if (fBindingItems != null)
			{
				fBindingItems.OnPairChangedForBinding += new ZBoolDescriptionPairChangedEventHandler(BindingItems_OnPairChangedForBinding);
			}
		}

		void UnhookBindingItemsEvents()
		{
			if (fBindingItems != null)
			{
				fBindingItems.OnPairChangedForBinding -= new ZBoolDescriptionPairChangedEventHandler(BindingItems_OnPairChangedForBinding);
			}
		}

		#endregion

		#region Dispose()

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				this.ItemCheck -= new ItemCheckEventHandler(CheckedListBox_ItemCheck);
				UnhookBindingItemsEvents();
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}
		}

		#endregion

		#region IBindTo Members

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string BindTo
		{
			get { return BindingMemberHelper.BindingMember; }
			set { BindingMemberHelper.BindingMember = value; }
		}

		ControlBindingMemberHelper BindingMemberHelper
		{
			get { return bindingMemberHelper ?? (bindingMemberHelper = ControlBindingMemberHelper.Get(this)); }
		}
		ControlBindingMemberHelper bindingMemberHelper;

		[DefaultValue(false)]
		public bool SingleCheckMode { get; set; }

		#endregion

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZCheckedListBox>()
				.Property<ZBoolDescriptionPairList>("BindingItems", null)
				.Result;
		}

		#endregion

#region Testing
#if DEBUG
		internal void OnMouseMoveExposed(MouseEventArgs args, int index = -1)
		{
#if WINZOR
			currentMouseOverIndex = index;
#endif
			OnMouseMove(args);
		}
#endif
		#endregion
	}
}
