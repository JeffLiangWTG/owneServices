using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public class ZLabelCaptionRenderer : LabelCaptionRenderer
	{
		public ZLabelCaptionRenderer()
		{
		}

		public ZLabelCaptionRenderer(Control control)
			: base(control)
		{
		}

		protected sealed override Control Control
		{
			get { return base.Control; }
			set
			{
				if (Control != null)
				{
					Control.ParentChanged -= new EventHandler(Control_ParentChanged);
					Control.DataBindings.CollectionChanged -= new CollectionChangeEventHandler(ControlDataBindings_CollectionChanged);
				}
				base.Control = value;
				if (Control != null)
				{
					Control.ParentChanged += new EventHandler(Control_ParentChanged);
					Control.DataBindings.CollectionChanged += new CollectionChangeEventHandler(ControlDataBindings_CollectionChanged);
				}
			}
		}

		protected override Font GetDefaultFont() => OFont.GetFont(); // Do not dynamically change this. Instead set the Font property.

		public override string[] Captions
		{
			get { return (base.Captions != null && base.Captions.Length > 0) ? base.Captions : DefaultCaptions; }
			set { base.Captions = value; }
		}

		public override bool IsCaptionOverridden
		{
			get { return base.IsCaptionOverridden || (base.Captions != null && base.Captions.Length > 0); }
		}

		public override void Refresh()
		{
			if (Control != null)
			{
				if (IsCaptionRenderingEnabled)
				{
					LabelCaptionCache.GetCaptions(Control, true);
				}
				base.Refresh();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (lastCaptionRenderingSupport != null)
			{
				lastCaptionRenderingSupport.CaptionRenderingEnabledChanged -= new EventHandler(CaptionRenderingSupport_CaptionRenderingEnabledChanged);
				lastCaptionRenderingSupport = null;
			}
			base.Dispose(disposing);
		}

		public static void InvalidateCache()
		{
			ZLabelCaptionCache.Instance.ClearCache();
		}

		#region Metadata

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZLabelCaptionRenderer>()
			.Property("Caption", string.Empty, true) // Property name
			.Result;
		}

		#endregion

		#region Implementation

		#region CopyCaptionToPropertyHumanReadableName

		protected override LabelCaptionMeasurement MeasureCaptionCore()
		{
			var result = base.MeasureCaptionCore();
			if (!string.IsNullOrEmpty(result.Caption)
#if DEBUG
				&& (!Globals.IsTest || CopyCaptionToPropertyHumanReadableNameForTest)
#endif
				)
			{
				// Use Caption rather than result.Caption, as last can be truncated or short caption, when first is always full caption.
				CopyCaptionToPropertyHumanReadableName(Caption);
			}
			return result;
		}

#if DEBUG
		public bool CopyCaptionToPropertyHumanReadableNameForTest { get; set; }
#endif

		void CopyCaptionToPropertyHumanReadableName(string caption)
		{
			if (!string.IsNullOrEmpty(caption) && !string.IsNullOrEmpty(LabelSeparator) && caption.EndsWith(LabelSeparator))
			{
				caption = caption.Substring(0, caption.Length - LabelSeparator.Length);
			}

			var dataBoundControl = Control as IDataBoundControl;
			if (!string.IsNullOrEmpty(caption) &&
				Control != null && Control.BindingContext != null &&
				dataBoundControl != null && !string.IsNullOrEmpty(dataBoundControl.DataMember))
			{
				var foundRightBindingManager = false;
				foreach (DictionaryEntry context in Control.BindingContext)
				{
					var bindingManager = ((WeakReference)context.Value).Target as BindingManagerBase;
					if (bindingManager != null)
					{
						var bizo = bindingManager.Position > -1 && bindingManager.Position < bindingManager.Count ? bindingManager.GetCurrent() as BusinessObject : null;
						foreach (Binding binding in bindingManager.Bindings)
						{
							if (binding != null && (binding.BindableComponent == Control || IsPartOfCombinedControl(binding.BindableComponent as Control)))
							{
								bindingManager.CurrentChanged -= BindingManager_CurrentChanged;
								bindingManager.CurrentChanged += BindingManager_CurrentChanged;

								foundRightBindingManager = true;

								var info = bizo != null && !bizo.IsDeleted ? bizo.FindPropertyInfo(binding.BindingMemberInfo.BindingField) : null;
								if (info != null && Control.Visible)
								{
									CopyCaptionToPropertyHumanReadableName(info, caption);
									break;
								}
							}
						}

						if (foundRightBindingManager)
						{
							break;
						}
					}
				}
			}
		}

		bool IsPartOfCombinedControl(Control component)
		{
			return component != null && component.Parent == Control &&
				(
					Control is ZCodeFindBox ||
					Control is ZDropEdit
				);
		}

		internal static void CopyCaptionToPropertyHumanReadableName(ZPropertyInfo propertyInfo, string caption)
		{
			if (propertyInfo != null && !string.IsNullOrEmpty(caption))
			{
				((IZPropertyInfoInternals)propertyInfo).SetHumanReadableNameFromGuiCaption(caption);
				var wrappedPropertyInfo = propertyInfo as ZWrappedPropertyInfo;
				if (wrappedPropertyInfo != null)
				{
					CopyCaptionToPropertyHumanReadableName(wrappedPropertyInfo.InnerInfo, caption);
				}
				else
				{
					if (propertyInfo.HasNotifications())
					{
						((IBusinessObjectInternals)propertyInfo.BizObj).Validate(propertyInfo);
					}
				}
			}
		}

		void BindingManager_CurrentChanged(object sender, EventArgs e)
		{
			var bindingManager = sender as BindingManagerBase;
			if (bindingManager != null)
			{
				bindingManager.CurrentChanged -= BindingManager_CurrentChanged;
			}
			if (Control != null)
			{
				Control.BeginInvoke(new Action(() => { CopyCaptionToPropertyHumanReadableName(Caption); }));
			}
		}

		#endregion

		string[] DefaultCaptions
		{
			get { return IsCaptionRenderingEnabled ? LabelCaptionCache.GetCaptions(Control) : Array.Empty<string>(); }
		}

		internal bool IsCaptionRenderingEnabled
		{
			get
			{
				if (isCaptionRenderingEnabled == null)
				{
					lastCaptionRenderingSupport = GetCaptionRenderingSupport();
					if (lastCaptionRenderingSupport != null)
					{
						isCaptionRenderingEnabled = lastCaptionRenderingSupport != null && lastCaptionRenderingSupport.CaptionRenderingEnabled.GetValueOrDefault();
						lastCaptionRenderingSupport.CaptionRenderingEnabledChanged -= new EventHandler(CaptionRenderingSupport_CaptionRenderingEnabledChanged);
						lastCaptionRenderingSupport.CaptionRenderingEnabledChanged += new EventHandler(CaptionRenderingSupport_CaptionRenderingEnabledChanged);
					}
				}
				return isCaptionRenderingEnabled.GetValueOrDefault();
			}
		}
		bool? isCaptionRenderingEnabled;
		ICaptionRenderingSupport lastCaptionRenderingSupport;

		ICaptionRenderingSupport GetCaptionRenderingSupport()
		{
			var current = Control is Form ? Control : Control.Parent;
			while (current != null)
			{
				var captionRenderingSupport = current as ICaptionRenderingSupport;
				if (captionRenderingSupport != null && captionRenderingSupport.CaptionRenderingEnabled != null)
				{
					return captionRenderingSupport;
				}
				current = current.Parent;
			}
			return null;
		}

		ZLabelCaptionCache LabelCaptionCache
		{
			get { return labelCaptionCache ?? (labelCaptionCache = GetLabelCaptionCache()); }
		}
		ZLabelCaptionCache labelCaptionCache;

		internal virtual ZLabelCaptionCache GetLabelCaptionCache()
		{
			return ZLabelCaptionCache.Instance;
		}

		void Control_ParentChanged(object sender, EventArgs e)
		{
			Invalidate();
		}

		void ControlDataBindings_CollectionChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Add)
			{
				Invalidate();
			}
		}

		void CaptionRenderingSupport_CaptionRenderingEnabledChanged(object sender, EventArgs e)
		{
			var captionRenderingSupport = (ICaptionRenderingSupport)sender;
			captionRenderingSupport.CaptionRenderingEnabledChanged -= new EventHandler(CaptionRenderingSupport_CaptionRenderingEnabledChanged);

			if (isCaptionRenderingEnabled == false) // This is a nullable type
			{
				isCaptionRenderingEnabled = null;
				if (IsCaptionRenderingEnabled)
				{
					Invalidate();
				}
			}
			isCaptionRenderingEnabled = null;
		}

		#region Translation Feedback

		public override bool InHotCaptionFeedbackMode()
		{
			return IsCaptionRenderingEnabled && TranslationFeedbackManager.InTranslationFeedbackMode();
		}

		public override void PaintHotCaptionHighlight(Graphics graphics, Rectangle bounds)
		{
			TranslationFeedbackManager.PaintCaptionHighlight(graphics, bounds);
		}

		public override void OpenCaptionFeedback()
		{
			TranslationFeedbackManager.OpenFeedbackForm(Control);
		}

		#endregion

		#endregion
	}
}
