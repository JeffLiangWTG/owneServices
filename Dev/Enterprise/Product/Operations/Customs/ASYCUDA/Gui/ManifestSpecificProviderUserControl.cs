using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public partial class ManifestSpecificProviderUserControl : ZUserControl
	{
		public ManifestSpecificProviderUserControl()
		{
			InitializeComponent();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				TearDownManifestHeader();
			}
			base.Dispose(disposing);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			TearDownManifestHeader();
			SetupManifestHeader(dataSource);
			base.SetDataBinding(dataSource, dataMember);
			AMA_ManifestTypeInfo_ValueChanged(null, null);
			AMA_TransportModeInfo_ValueChanged(null, null);
			SpecificCircumstanceIndicatorInfo_ValueChanged(null, null);
		}

		void TearDownManifestHeader()
		{
			if (Header != null)
			{
				UnHookManifestHeaderEvents(Header);
				Header = null;
			}
		}

		void SetupManifestHeader(object dataSource)
		{
			Header = GetManifestHeader(dataSource);
			if (Header != null)
			{
				HookManifestHeaderEvents(Header);
			}
		}

		AsycudaManifestHeader GetManifestHeader(object dataSource)
		{
			AsycudaManifestHeader result = null;
			if (dataSource is AsycudaManifestHeader header)
			{
				result = header;
			}
			else if (dataSource is AsycudaBill bill)
			{
				result = bill.Header;
			}
			return result;
		}

		protected AsycudaManifestHeader Header { get; private set; }

		protected void HookManifestHeaderEvents(AsycudaManifestHeader header)
		{
			if (header != null)
			{
				header.AMA_ManifestTypeInfo.ValueChanged += AMA_ManifestTypeInfo_ValueChanged;
				header.AMA_TransportModeInfo.ValueChanged += AMA_TransportModeInfo_ValueChanged;
				header.SpecificCircumstanceIndicatorInfo.ValueChanged += SpecificCircumstanceIndicatorInfo_ValueChanged;
				HookManifestHeaderEventsCore(header);
			}
		}

		protected virtual void HookManifestHeaderEventsCore(AsycudaManifestHeader header)
		{
		}

		protected void UnHookManifestHeaderEvents(AsycudaManifestHeader header)
		{
			if (header != null)
			{
				header.AMA_ManifestTypeInfo.ValueChanged -= AMA_ManifestTypeInfo_ValueChanged;
				header.AMA_TransportModeInfo.ValueChanged -= AMA_TransportModeInfo_ValueChanged;
				header.SpecificCircumstanceIndicatorInfo.ValueChanged -= SpecificCircumstanceIndicatorInfo_ValueChanged;
				UnHookManifestHeaderEventsCore(header);
			}
		}

		protected virtual void UnHookManifestHeaderEventsCore(AsycudaManifestHeader header)
		{
		}

		protected virtual Control ControlToAddManifestSpecificUserControl => null;

		protected void AMA_ManifestTypeInfo_ValueChanged(object sender, System.EventArgs e)
		{
			EnforceOnlyValidManifestTypes(sender, e);

			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(Header);

			var providerIdentifier = provider?.GetIdentifier(Header) ?? ZString.Empty;
			if (providerIdentifier != currentProviderIdentifier)
			{
				currentProviderIdentifier = providerIdentifier;
				OnProviderIdentifierChanged(provider);
			}

			AMA_ManifestTypeInfo_ValueChangedCore(sender, e);
		}
		ZString currentProviderIdentifier;

		protected void AMA_TransportModeInfo_ValueChanged(object sender, System.EventArgs e)
		{
			EnforceOnlyValidTransportMode(sender, e);
			AMA_TransportModeInfo_ValueChangedCore(sender, e);
		}

		protected void SpecificCircumstanceIndicatorInfo_ValueChanged(object sender, System.EventArgs e)
		{
			EnforceOnlyValidSpecificCircumstanceIndicator(sender, e);
			SpecificCircumstanceIndicatorInfo_ValueChangedCore(sender, e);
		}

		protected virtual void OnProviderIdentifierChanged(ApplicationGUIProvider provider)
		{
			SetManifestSpecificUserControlVisibility(provider);
		}

		[DpiState(DpiState.Unscaled)]
		int AutoScrollMinSizeWidth
		{
			get
			{
				if (!autoScrollMinSizeWidth.HasValue)
				{
					autoScrollMinSizeWidth = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(AutoScrollMinSize.Width);
				}
				return autoScrollMinSizeWidth.Value;
			}
		}
		int? autoScrollMinSizeWidth;

		[DpiState(DpiState.Unscaled)]
		int AutoScrollMinSizeHeight
		{
			get
			{
				if (!autoScrollMinSizeHeight.HasValue)
				{
					autoScrollMinSizeHeight = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(AutoScrollMinSize.Height);
				}
				return autoScrollMinSizeHeight.Value;
			}
		}
		int? autoScrollMinSizeHeight;

		protected void SetManifestSpecificUserControlVisibility(ApplicationGUIProvider provider)
		{
			var controlToAddManifestSpecificUserControl = ControlToAddManifestSpecificUserControl;
			if (controlToAddManifestSpecificUserControl.Controls.Count > 0)
			{
				foreach (Control control in controlToAddManifestSpecificUserControl.Controls)
				{
					control.Dispose();
				}
				controlToAddManifestSpecificUserControl.Controls.Clear();
			}
			var manifestSpecificUserControl = provider == null ? null : GetManifestSpecificUserControl(provider);
			if (manifestSpecificUserControl == null)
			{
				controlToAddManifestSpecificUserControl.Visible = false;
			}
			else
			{
				AutoScrollMinSize = ControlDpiScalingHelper.NewScaledSize(AutoScrollMinSizeWidth, AutoScrollMinSizeHeight + ControlDpiScalingHelper.UnscaleFromCurrentDpiY(manifestSpecificUserControl.Size.Height), true);
				controlToAddManifestSpecificUserControl.Visible = true;
				controlToAddManifestSpecificUserControl.Controls.Add(manifestSpecificUserControl);
				manifestSpecificUserControl.Dock = DockStyle.Fill;
				this.BindingSource.SetBindingMember(manifestSpecificUserControl, ManifestSpecificUserControlDataMember);
				manifestSpecificUserControl.SetDataBinding(DataSource, DataMember);
			}
		}
		protected virtual string ManifestSpecificUserControlDataMember => BindingSource.DataMember;
		protected virtual ZUserControl GetManifestSpecificUserControl(ApplicationGUIProvider provider) => null;
		protected virtual void AMA_ManifestTypeInfo_ValueChangedCore(object sender, System.EventArgs e)
		{
		}
		protected virtual void EnforceOnlyValidManifestTypes(object sender, System.EventArgs e)
		{
		}
		protected virtual void AMA_TransportModeInfo_ValueChangedCore(object sender, System.EventArgs e)
		{
		}
		protected virtual void EnforceOnlyValidTransportMode(object sender, System.EventArgs e)
		{
		}
		protected virtual void EnforceOnlyValidSpecificCircumstanceIndicator(object sender, System.EventArgs e)
		{
		}
		protected virtual void SpecificCircumstanceIndicatorInfo_ValueChangedCore(object sender, System.EventArgs e)
		{
		}
	}
}
