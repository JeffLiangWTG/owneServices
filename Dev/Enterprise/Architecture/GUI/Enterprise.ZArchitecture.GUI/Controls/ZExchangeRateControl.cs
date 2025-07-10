using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Notifications;

namespace Enterprise.ZArchitecture.GUI
{
	[DefaultDataSourceBindingMember(null)]
	[SuppressFormDesignerAnalysis]
	[CompositeFieldControl]
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class ZExchangeRateControl : ZUserControl, IExtendedControl, INotificationDataMembers, IBindTo
	{
		#region Support Controls

		internal class ZExchangeRateCalcEdit : ZCalcEdit.Bare
		{
			const Keys RefetchExchangeRateHotkey = Keys.F5;

			public ZExchangeRateCalcEdit()
			{
				Hotkeys.AddDescription(RefetchExchangeRateHotkey, Res.GetString("f77bfa2a-fb41-4503-b1f1-ccfbadb9bd57", "Re-fetch exchange rate"));
			}

			protected override bool EnableConverterUnit { get; }

			protected override void OnValidated(EventArgs e)
			{
				base.OnValidated(e);
				Parent.OnValidated(e);
			}

			protected override void OnKeyDown(KeyEventArgs e)
			{
				if (e.KeyData == RefetchExchangeRateHotkey)
				{
					Parent.RefetchExchangeRate();
					Parent.OnValidated(e);
					e.Handled = true;
				}
				base.OnKeyDown(e);
			}

			new ZExchangeRateControl Parent
			{
				get { return (ZExchangeRateControl)base.Parent; }
			}
		}

		internal class ZExchangeRateFindBox : ZCodeFindBox.Bare
		{
			protected override IControlExtensionCollection NewExtensionCollection()
			{
				// we need only bare control w/out default extensions
				return new ControlExtensionCollection(this);
			}

			protected override void OnValidated(EventArgs e)
			{
				base.OnValidated(e);
				Parent.OnValidated(e);
			}

			new ZExchangeRateControl Parent
			{
				get { return (ZExchangeRateControl)base.Parent; }
			}
		}

		#endregion

		public bool ReadOnly
		{
			get { return CurrencyFindBox.ReadOnly && RateCalcEdit.ReadOnly; }
		}

		#region Custom Adornment Layout

		class ZExchangeRateControlAdornmentLayout : AdornmentLayout<ZExchangeRateControl>
		{
			public override IEnumerable<Control> GetBackroundAdornmentTargets(ZExchangeRateControl source)
			{
				yield return source.CurrencyFindBox.CodeBox;
				yield return source.RateCalcEdit;
			}

			public override IEnumerable<IIconLayout> GetIconAdornmentTargets(ZExchangeRateControl source)
			{
				yield return new IconLayout(source.CurrencyFindBox.PopupButton, IconAlignment.Center);
			}
		}

		#endregion

		#region Constructors

		static ZExchangeRateControl()
		{
			NotificationAdornmentFactory.RegisterCustomLayout(new ZExchangeRateControlAdornmentLayout());
		}

		public ZExchangeRateControl()
		{
			InitializeComponent();
			InitializeControl();
			InitializeExtensions();
		}

		#endregion

		#region Initialization

		void InitializeControl()
		{
			SetDecimalPlaces();
		}

		void InitializeExtensions()
		{
			Extensions = new DefaultControlExtensionCollection(this);

			RateCalcEdit.Extensions.Add(new HintExtension());
			CurrencyFindBox.Extensions.Add(new HintExtension());

			Extensions.Substitute<IHintExtension>(
				CompositeHintExtension.Create(RateCalcEdit, CurrencyFindBox));
		}

		#endregion

		#region IExtendedControl Members

		Control IExtendedControl.Host
		{
			get { return this; }
		}

		[Browsable(false)]
		public IControlExtensionCollection Extensions { get; private set; }

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

		protected Type BindingSourceDataSourceType
		{
			get { return KBindingSource.GetBindingSource(this) == null ? null : KBindingSource.GetBindingSource(this).DataSourceType; }
		}

		#endregion

		#region INotificationDataMembers Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		string[] INotificationDataMembers.NotificationDataMembers
		{
			get { return new string[] { DataMember + "+Rate", DataMember + "+Currency" }; }
		}

		#endregion

		#region IDataBoundControl Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Data member name")]
		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			RateCalcEdit.SetDataBinding(dataSource, string.IsNullOrEmpty(dataMember) ? "" : dataMember + "+Rate");
			CurrencyFindBox.BindToList = string.IsNullOrEmpty(dataMember) ? "" : dataMember + "+CurrencyList";
			CurrencyFindBox.SetDataBinding(dataSource, string.IsNullOrEmpty(dataMember) ? "" : dataMember + "+Currency");
			Extensions.SetDataBinding(dataSource, "");
		}

		public override Type DataSourceType
		{
			get { return typeof(ZExchangeRate); }
		}

		#endregion

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZExchangeRateControl>()
				.Property("ReadOnly", false, false)
				.Result;
		}

		#endregion

		#region Implementation

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Size MinimumSize
		{
			get { return base.MinimumSize; }
			set { base.MinimumSize = value; }
		}

		protected virtual void SetDecimalPlaces()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				RateCalcEdit.Decimals = 6;
			}
		}

		protected virtual void RefetchExchangeRate()
		{
			var bindingManager = GetBindingManager("");
			var exchangeRate = (bindingManager == null || bindingManager.Position == -1) ? null : bindingManager.GetCurrent() as ZExchangeRate;
			if (exchangeRate != null)
			{
				exchangeRate.RefetchExchangeRate();
			}
		}

		#endregion

		#region Overrides

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Extensions.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
