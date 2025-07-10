using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// A Module FindBox that allows users to hook to the 'Selected' event.
	/// The 'Selected' event is called when the object is selected.
	/// The argurment of the 'Selected' event contains the array of selected BusinessObjects.
	/// </summary>
	[SuppressFormDesignerAnalysis]
	public class ZSelectedEventPopupFindBox : ZPopupFindBox
	{
		#region Bare

		[WTG.StaticAnalysis.Annotation.CodeAlive("There are future possible usages.")]
		[ToolboxItem(false)]
		public new class Bare : ZSelectedEventPopupFindBox
		{
			protected override IControlExtensionCollection NewExtensionCollection()
			{
				return new ControlExtensionCollection(this);
			}
		}

		#endregion

		public ZSelectedEventPopupFindBox()
		{
			InitializeComponent();
		}

		/// <summary>
		/// The 'Selected' event is called when the object is selected.
		/// The argurment of the 'Selected' event contains the array of selected BusinessObjects.
		/// </summary>
		public event EmbeddedModulePopup.SelectedEventHandler Selected;

		public override void SelectFromPopupForm(bool autoSelect = false)
		{
			var oldCode = Code;
			try
			{
				Code = "";
				base.SelectFromPopupForm(autoSelect);
			}
			finally
			{
				Code = oldCode;
			}
		}

		#region CodeBox

		public override ZCodeBox GetCodeBox()
		{
			return new ZEventPopupFindBoxCodeBox(this);
		}

		class ZEventPopupFindBoxCodeBox : ZCodeBox
		{
			public ZEventPopupFindBoxCodeBox(ZSelectedEventPopupFindBox findBox)
				: base(findBox)
			{
			}

			protected new ZSelectedEventPopupFindBox ParentFindBox
			{
				get { return (ZSelectedEventPopupFindBox)base.ParentFindBox; }
			}

			protected override void OnKeyDown(KeyEventArgs e)
			{
				if (e.KeyData != Keys.F3)
				{
					base.OnKeyDown(e);
				}
			}
		}

		#endregion

		#region IDataBoundControl Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Property name")]
		public override void SetDataBinding(object dataSource, string dataMember)
		{
			CodeBox.DataBindings.RemoveBinding("Text");
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource != null)
			{
				CodeBox.DataBindings.Add(new KBinding("Text", dataSource, dataMember, true));
			}
		}

		protected override bool RequiresListDataBinding
		{
			get { return true; }
		}

		#endregion

		#region IFetchHintGenerator Members

		protected override void AddFetchHints(object dataSource, string dataMember)
		{
			if (string.IsNullOrEmpty(dataMember) || dataMember == BindTo)
			{
				GetFetchHintHandler(this, dataSource).Add();
			}
		}

		protected virtual ZCodeFindBoxFetchHintHandler GetFetchHintHandler(IBindToList control, object dataSource)
		{
			return new ZCodeFindBoxFetchHintHandler(control, dataSource);
		}

		#endregion

		#region Implementation
		void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// CodeBox
			// 
			this.CodeBox.BringToFront();
			this.CodeBox.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// PopupButton
			// 
			this.PopupButton.Dock = System.Windows.Forms.DockStyle.Right;
			//
			// ZFindBoxUserControl
			//
			this.ResumeLayout(false);
		}

		protected sealed override IFindBoxPopup GetNewPopupForm()
		{
			var result = GetNewPopupFormCore();
			HookupSelectedEvent(result);
			return result;
		}

		protected virtual IFindBoxPopup GetNewPopupFormCore()
		{
			return base.GetNewPopupForm();
		}

		void HookupSelectedEvent(IFindBoxPopup popup)
		{
			var embeddedPopup = popup as EmbeddedModulePopup;
			if (embeddedPopup != null)
			{
				embeddedPopup.Selected += Selected;
			}
		}

		protected sealed override void OnPopupFormClosed(IFindBoxPopup popupForm)
		{
			var embeddedPopup = popupForm as EmbeddedModulePopup;
			if (embeddedPopup != null)
			{
				embeddedPopup.Selected -= Selected;
			}
			OnPopupFormClosedCore(popupForm);
		}

		protected virtual void OnPopupFormClosedCore(IFindBoxPopup popupForm)
		{
			base.OnPopupFormClosed(popupForm);
		}

		#endregion
	}
}
