using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.Core;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Notifications;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	/// <summary>
	/// This control can be used in cases when description is unique instead of code, and binding member is actually description too.
	/// It allows to search by both code and description.
	/// </summary>
	[SuppressFormDesignerAnalysis]
	public partial class DescriptionFindBox : ZPopupFindBox
	{
		public DescriptionFindBox()
		{
			InitializeComponent();
		}

		#region Notification Layout

		static DescriptionFindBox()
		{
			NotificationAdornmentFactory.RegisterCustomLayout(new DescriptionFindBoxNotificationLayout());
		}

		class DescriptionFindBoxNotificationLayout : AdornmentLayout<DescriptionFindBox>
		{
			public override IEnumerable<Control> GetBackroundAdornmentTargets(DescriptionFindBox source)
			{
				return new[] { source.CodeBox, source.DescriptionBox };
			}

			public override IEnumerable<IIconLayout> GetIconAdornmentTargets(DescriptionFindBox source)
			{
				yield return new IconLayout(source.PopupButton, IconAlignment.Center);
			}
		}

		#endregion

		#region CodeBoxLength

		[Category(ZGUIConstants.DesignerCategory)]
		[DefaultValue(5)]
		public int CodeBoxLength
		{
			get { return codeBoxLength; }
			set
			{
				codeBoxLength = value;
				ControlDpiScalingHelper.SetWidth(ref DescriptionBox, TextBoxControlSize.GetControlWidth(DescriptionBox, codeBoxLength), false);
			}
		}

		int codeBoxLength;

		#endregion

		#region Update Code/Description

		#region Events

		void CodeBox_Validated(object sender, EventArgs e)
		{
			UpdateDescriptionFromCode();
		}

		void DescriptionBox_Validated(object sender, EventArgs e)
		{
			UpdateCodeFromDescription();
		}

		void OnBindingComplete(object sender, BindingCompleteEventArgs e)
		{
			UpdateDescriptionFromCode();
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			UpdateDescriptionFromCode();
		}

		#endregion

		#region UpdateCodeFromDescription

		void UpdateCodeFromDescription()
		{
			if (!updating && !Description.Equals(cachedDescription, StringComparison.OrdinalIgnoreCase) && Visible && !DesignModeFinder.IsDesigning)
			{
				updating = true;
				cachedDescription = Description;
				cachedCode = string.IsNullOrEmpty(Description) ? string.Empty
					: (((FindBoxListProvider)ListProvider).CodeFromDescription(Description) ?? Constants.FindBoxMessages.InvalidSelection);
				((BusinessObject)CurrentItem)[DataMember] = cachedCode;
				updating = false;
			}
		}

		#endregion

		#region UpdateDescriptionFromCode

		void UpdateDescriptionFromCode()
		{
			if (!updating && !Code.Equals(cachedCode, StringComparison.OrdinalIgnoreCase) && Visible && !DesignModeFinder.IsDesigning)
			{
				updating = true;
				cachedCode = Code;
				cachedDescription = string.IsNullOrEmpty(Code) ? string.Empty : (ListProvider.DescriptionFromCode(Code) ?? "{INV}");
				Description = cachedDescription;
				updating = false;
			}
		}

		#endregion

		string cachedCode;
		string cachedDescription;
		bool updating;

		#endregion

		#region AutoCompleteDescription

		void DescriptionBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == '=' && !ReadOnly)
			{
				e.Handled = true;
				AutoCompleteDescription();
			}
		}

		void AutoCompleteDescription()
		{
			var previousDescription = DescriptionBox.Text;
			DescriptionBox.Text = ((FindBoxListProvider)ListProvider).NearestDescriptionMatch(DescriptionBox.Text);
			if (DescriptionBox.Text.StartsWith(previousDescription))
			{
				DescriptionBox.SelectionStart = Math.Min(DescriptionBox.Text.Length, previousDescription.Length);
				DescriptionBox.SelectionLength = Math.Max(0, DescriptionBox.Text.Length - previousDescription.Length);
			}
			else
			{
				DescriptionBox.SelectAll();
			}
		}

		#endregion

		#region IDataBoundControl Members

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			const string textPropertyName = "Text";
			CodeBox.DataBindings.RemoveBinding(textPropertyName);
			DescriptionBox.DataBindings.RemoveBinding(textPropertyName);
			DataBindings.RemoveBinding("List");

			base.SetDataBinding(dataSource, dataMember);
			if (dataSource != null && !this.IsDesignMode())
			{
				CodeBox.DataBindings.RemoveBinding(textPropertyName);
				var binding = new KBinding(textPropertyName, dataSource, dataMember, true);
				binding.BindingComplete += OnBindingComplete;
				CodeBox.DataBindings.Add(binding);
			}
		}

		public override Type DataSourceType
		{
			get { return typeof(ZString); }
		}

		protected override bool RequiresListDataBinding
		{
			get { return true; }
		}

		#endregion

		#region IFindBox Members

		#region Description

		protected override string Description
		{
			get { return DescriptionBox.Text; }
			set { DescriptionBox.Text = value; }
		}

		#endregion

		#region ListProvider

		protected override IFindBoxListProvider ListProvider
		{
			get { return new FindBoxListProvider((IBusinessObjectCollection)List); }
		}

		#region FindBoxListProvider

		sealed class FindBoxListProvider : CargoWise.EntityFramework.FindBoxListProvider
		{
			public FindBoxListProvider(IBusinessObjectCollection collection)
				: base(collection)
			{
			}

			public string NearestDescriptionMatch(string description)
			{
				return NearestDescriptionMatch(description, true);
			}
		}

		#endregion

		#endregion

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
	}
}
