using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public abstract class ControlCustomisationBase : NonPersistentBusinessObject, IPreviewNotifierChild
	{
		protected ControlCustomisationBase(BMControlCustomisation customisation)
			: base(customisation.Factory)
		{
			this.customisation = customisation;
		}

		readonly BMControlCustomisation customisation;

		public BMControlCustomisation Parent
		{
			get { return customisation; }
		}

		#region Properties

		#region ControlType

		[XmlColumnProperty]
		[List("Lookups.ControlTypes")]
		[ResourceStringData("ControlCustomisationBase.ControlType", Caption = "Control Type", ShortCaption = "Type")]
		[MaxLength(3)]
		public virtual ZString ControlType
		{
			get { return GetXmlColumnPropertyValue<ZString>(ControlTypeInfo); }
			set
			{
				using (this.LayoutConfigUpdatedOnDisposeIfValueChanged(ControlType, value))
				{
					SetXmlColumnPropertyValue(ControlTypeInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateControlType();
						Validation.ValidateOrientation();
					}

					if (!ControlTypeInfo.HasErrors())
					{
						var label = GetLabelForControlType();
						if (!string.IsNullOrEmpty(label))
						{
							Label = label;
						}
					}

					ControlTypeDescriptionInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ControlTypeInfo
		{
			get { return GetZPropertyInfo(nameof(ControlType)); }
		}

		#endregion

		#region ControlTypeDescription

		[List("Lookups.ControlTypes")]
		[ResourceStringData("ControlCustomisationBase.ControlTypeDescription", Caption = "Control Type", ShortCaption = "Type")]
		public virtual ZString ControlTypeDescription
		{
			get { return Lookups.ControlTypes.GetDescriptionFromCode(ControlType); }
		}

		public ZPropertyInfo ControlTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(ControlTypeDescription)); }
		}

		public ZString ControlTypeDescriptionFieldType
		{
			get
			{
				var bmControlCustomisationLine = this as BMControlCustomisationLine;

				var showTextMacro =
					bmControlCustomisationLine != null
					&& bmControlCustomisationLine.PropertySource == PropertySourceList.Codes.Job;

				return showTextMacro
					? nameof(FieldType.TextMacro)
					: nameof(FieldType.TextDropEdit);
			}
		}

		#endregion

		#region Label

		[XmlColumnProperty]
		[ResourceStringData("ControlCustomisationBase.Label", Caption = "Label", FullDescription = "The label to show next to the control.")]
		[MaxLength(128)]
		public ZString Label
		{
			get { return GetXmlColumnPropertyValue<ZString>(LabelInfo); }
			set
			{
				using (this.LayoutConfigUpdatedOnDisposeIfValueChanged(Label, value))
				{
					SetXmlColumnPropertyValue(LabelInfo, value);
				}
			}
		}

		public ZPropertyInfo LabelInfo
		{
			get { return GetZPropertyInfo(nameof(Label)); }
		}

		protected virtual string GetLabelForControlType()
		{
			return string.Empty;
		}

		#endregion

		#region Left

		[XmlColumnProperty]
		[ResourceStringData("ControlCustomisationBase.Left", Caption = "Left", FullDescription = "The position of the field relative to the left-hand side of the layout.")]
		public ZInt Left
		{
			get { return GetXmlColumnPropertyValue<ZInt>(LeftInfo); }
			set
			{
				using (this.LayoutConfigUpdatedOnDisposeIfValueChanged(Left, value))
				{
					SetXmlColumnPropertyValue(LeftInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateLeft();
					}
				}
			}
		}

		public ZPropertyInfo LeftInfo
		{
			get { return GetZPropertyInfo(nameof(Left)); }
		}

		#endregion

		#region Top

		[XmlColumnProperty]
		[ResourceStringData("ControlCustomisationBase.Top", Caption = "Top", FullDescription = "The position of the field relative to the top of the control.")]
		public ZInt Top
		{
			get { return GetXmlColumnPropertyValue<ZInt>(TopInfo); }
			set
			{
				using (this.LayoutConfigUpdatedOnDisposeIfValueChanged(Top, value))
				{
					SetXmlColumnPropertyValue(TopInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateTop();
					}
				}
			}
		}

		public ZPropertyInfo TopInfo
		{
			get { return GetZPropertyInfo(nameof(Top)); }
		}

		#endregion

		#region Width

		[XmlColumnProperty]
		[ResourceStringData("ControlCustomisationBase.Width", Caption = "Width")]
		public ZInt Width
		{
			get { return GetXmlColumnPropertyValue<ZInt>(WidthInfo); }
			set
			{
				using (this.LayoutConfigUpdatedOnDisposeIfValueChanged(Width, value))
				{
					SetXmlColumnPropertyValue(WidthInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateWidth();
					}
				}
			}
		}

		public ZPropertyInfo WidthInfo
		{
			get { return GetZPropertyInfo(nameof(Width)); }
		}

		#endregion

		#region Height

		[XmlColumnProperty]
		[ResourceStringData("ControlCustomisationBase.Height", Caption = "Height")]
		public ZInt Height
		{
			get { return GetXmlColumnPropertyValue<ZInt>(HeightInfo); }
			set
			{
				using (this.LayoutConfigUpdatedOnDisposeIfValueChanged(Height, value))
				{
					SetXmlColumnPropertyValue(HeightInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateHeight();
					}
				}
			}
		}

		public ZPropertyInfo HeightInfo
		{
			get { return GetZPropertyInfo(nameof(Height)); }
		}

		#endregion

		#region BackgroundColor

		[XmlColumnProperty]
		[List("Lookups.ColorList")]
		[ResourceStringData("ControlCustomisationBase.BackgroundColor", Caption = "Background Color", ShortCaption = "Background")]
		[MaxLength(50)]
		public ZString BackgroundColor
		{
			get { return GetBackgroundColorCore(); }
			set
			{
				using (this.LayoutConfigUpdatedOnDisposeIfValueChanged(BackgroundColor, value))
				{
					SetXmlColumnPropertyValue(BackgroundColorInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateBackgroundColor();
					}
				}
			}
		}

		protected virtual ZString GetBackgroundColorCore()
		{
			return GetXmlColumnPropertyValue<ZString>(BackgroundColorInfo);
		}

		public ZPropertyInfo BackgroundColorInfo
		{
			get { return GetZPropertyInfo(nameof(BackgroundColor)); }
		}

		public Color BackgroundColorValue
		{
			get { return ColorList.ColorFromName(BackgroundColor); }
		}

		#endregion

		#region ForegroundColor

		[XmlColumnProperty]
		[List("Lookups.ColorList")]
		[ResourceStringData("ControlCustomisationBase.ForegroundColor", Caption = "Foreground Color", ShortCaption = "Foreground", FullDescription = "The text color of this field.")]
		[MaxLength(50)]
		public ZString ForegroundColor
		{
			get { return GetXmlColumnPropertyValue<ZString>(ForegroundColorInfo); }
			set
			{
				using (this.LayoutConfigUpdatedOnDisposeIfValueChanged(ForegroundColor, value))
				{
					SetXmlColumnPropertyValue(ForegroundColorInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateForegroundColor();
					}
				}
			}
		}

		public ZPropertyInfo ForegroundColorInfo
		{
			get { return GetZPropertyInfo(nameof(ForegroundColor)); }
		}

		public Color ForegroundColorValue
		{
			get { return ColorList.ColorFromName(ForegroundColor); }
		}

		#endregion

		#region Font

		[XmlColumnProperty]
		[List("Lookups.FontList")]
		[ResourceStringData("ControlCustomisationBase.Font", Caption = "Font")]
		[MaxLength(50)]
		public ZString Font
		{
			get { return GetXmlColumnPropertyValue<ZString>(FontInfo); }
			set
			{
				using (this.LayoutConfigUpdatedOnDisposeIfValueChanged(Font, value))
				{
					SetXmlColumnPropertyValue(FontInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateFont();
					}
				}
			}
		}

		public ZPropertyInfo FontInfo
		{
			get { return GetZPropertyInfo(nameof(Font)); }
		}

		public FontFamily FontFamily
		{
			get { return new FontFamily(Font); }
		}

		#endregion

		#region IsBold

		[XmlColumnProperty]
		[ResourceStringData("ControlCustomisationBase.IsBold", Caption = "Bold")]
		public ZBool IsBold
		{
			get { return GetXmlColumnPropertyValue<ZBool>(IsBoldInfo); }
			set
			{
				using (this.LayoutConfigUpdatedOnDisposeIfValueChanged(IsBold, value))
				{
					SetXmlColumnPropertyValue(IsBoldInfo, value);
				}
			}
		}

		public ZPropertyInfo IsBoldInfo
		{
			get { return GetZPropertyInfo(nameof(IsBold)); }
		}

		#endregion

		#region IsReadOnly

		[XmlColumnProperty]
		[ResourceStringData("ControlCustomisationBase.IsReadOnly", Caption = "Read Only")]
		public ZBool IsReadOnly
		{
			get { return GetXmlColumnPropertyValue<ZBool>(IsReadOnlyInfo); }
			set
			{
				using (this.LayoutConfigUpdatedOnDisposeIfValueChanged(IsReadOnly, value))
				{
					SetXmlColumnPropertyValue(IsReadOnlyInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateBackgroundColor();
						Validation.ValidateIsReadOnly();
						Validation.ValidateOrientation();
					}
				}
			}
		}

		public ZPropertyInfo IsReadOnlyInfo
		{
			get { return GetZPropertyInfo(nameof(IsReadOnly)); }
		}

		#endregion

		#region FontSize

		[XmlColumnProperty]
		[ResourceStringData("ControlCustomisationBase.FontSize", Caption = "Font Size")]
		public ZInt FontSize
		{
			get { return GetXmlColumnPropertyValue<ZInt>(FontSizeInfo); }
			set
			{
				using (this.LayoutConfigUpdatedOnDisposeIfValueChanged(FontSize, value))
				{
					SetXmlColumnPropertyValue(FontSizeInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateFontSize();
					}
				}
			}
		}

		public ZPropertyInfo FontSizeInfo
		{
			get { return GetZPropertyInfo(nameof(FontSize)); }
		}

		#endregion

		#region BringToFront

		[XmlColumnProperty]
		[ResourceStringData("ControlCustomisationBase.BringToFront", Caption = "Show On Top")]
		public ZBool BringToFront
		{
			get { return GetXmlColumnPropertyValue<ZBool>(BringToFrontInfo); }
			set
			{
				using (this.LayoutConfigUpdatedOnDisposeIfValueChanged(BringToFront, value))
				{
					SetXmlColumnPropertyValue(BringToFrontInfo, value);
				}
			}
		}

		public ZPropertyInfo BringToFrontInfo
		{
			get { return GetZPropertyInfo(nameof(BringToFront)); }
		}

		#endregion

		#region Alignment

		[XmlColumnProperty]
		[MaxLength(5)]
		[List("Lookups.AlignmentList")]
		[ResourceStringData("ControlCustomisationBase.Alignment", Caption = "Alignment")]
		public ZString Alignment
		{
			get { return GetXmlColumnPropertyValue<ZString>(AlignmentInfo); }
			set
			{
				using (this.LayoutConfigUpdatedOnDisposeIfValueChanged(Alignment, value))
				{
					SetXmlColumnPropertyValue(AlignmentInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateAlignment();
					}
				}
			}
		}

		public ZPropertyInfo AlignmentInfo
		{
			get { return GetZPropertyInfo(nameof(Alignment)); }
		}

		#endregion

		#region Orientation

		[XmlColumnProperty]
		[MaxLength(10)]
		[List("Lookups.OrientationList")]
		[ResourceStringData("ControlCustomisationBase.Orientation", Caption = "Orientation")]
		public ZString Orientation
		{
			get { return GetXmlColumnPropertyValue<ZString>(OrientationInfo); }
			set
			{
				using (this.LayoutConfigUpdatedOnDisposeIfValueChanged(Orientation, value))
				{
					SetXmlColumnPropertyValue(OrientationInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateAlignment();
					}
				}
			}
		}

		public ZPropertyInfo OrientationInfo
		{
			get { return GetZPropertyInfo(nameof(Orientation)); }
		}

		public BMBoardSectionOrientation OrientationValue
		{
			get { return !Orientation.IsEmpty ? (BMBoardSectionOrientation)Enum.Parse(typeof(BMBoardSectionOrientation), Orientation) : BMBoardSectionOrientation.Horizontal; }
		}

		#endregion

		#region PlayButtonBehavior

		[XmlColumnProperty]
		[MaxLength(3)]
		[List("Lookups.StatusButtonBehaviorOptionsList")]
		[ResourceStringData("ControlCustomisationBase.PlayButtonBehavior", Caption = "Play Task Button", FullDescription = "Sets how the system responds when the Play Task button is clicked.")]
		public ZString PlayButtonBehavior
		{
			get => GetXmlColumnPropertyValue<ZString>(PlayButtonBehaviorInfo);
			set
			{
				using (this.LayoutConfigUpdatedOnDisposeIfValueChanged(Orientation, value))
				{
					SetXmlColumnPropertyValue(PlayButtonBehaviorInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidatePlayButtonBehavior();
					}
				}
			}
		}

		public ZPropertyInfo PlayButtonBehaviorInfo
		{
			get { return GetZPropertyInfo(nameof(PlayButtonBehavior)); }
		}

		#endregion

		#region SuspendButtonBehavior

		[XmlColumnProperty]
		[MaxLength(3)]
		[List("Lookups.StatusButtonBehaviorOptionsList")]
		[ResourceStringData("ControlCustomisationBase.SuspendButtonBehavior", Caption = "Suspend Task Button", FullDescription = "Sets how the system responds when the Suspend Task button is clicked.")]
		public ZString SuspendButtonBehavior
		{
			get => GetXmlColumnPropertyValue<ZString>(SuspendButtonBehaviorInfo);
			set
			{
				using (this.LayoutConfigUpdatedOnDisposeIfValueChanged(Orientation, value))
				{
					SetXmlColumnPropertyValue(SuspendButtonBehaviorInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateSuspendButtonBehavior();
					}
				}
			}
		}

		public ZPropertyInfo SuspendButtonBehaviorInfo
		{
			get { return GetZPropertyInfo(nameof(SuspendButtonBehavior)); }
		}

		#endregion

		#region CloseTaskButtonBehavior

		[XmlColumnProperty]
		[MaxLength(3)]
		[List("Lookups.StatusButtonBehaviorOptionsList")]
		[ResourceStringData("ControlCustomisationBase.CloseTaskButtonBehavior", Caption = "Close Task Button", FullDescription = "Sets how the system responds when the Close Task button is clicked.")]
		public ZString CloseTaskButtonBehavior
		{
			get => GetXmlColumnPropertyValue<ZString>(CloseTaskButtonBehaviorInfo);
			set
			{
				using (this.LayoutConfigUpdatedOnDisposeIfValueChanged(Orientation, value))
				{
					SetXmlColumnPropertyValue(CloseTaskButtonBehaviorInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateCloseTaskButtonBehavior();
					}
				}
			}
		}

		public ZPropertyInfo CloseTaskButtonBehaviorInfo
		{
			get { return GetZPropertyInfo(nameof(CloseTaskButtonBehavior)); }
		}

		#endregion

		#endregion

		#region New Properties

		[ResourceStringData("ControlCustomisationBase.DisplaySequence", Caption = "Display Sequence", ShortCaption = "Seq.")]
		public ZInt DisplaySequence
		{
			get
			{
				var orderedLines = GetParentCustomisationRecords()
					.OrderBy(l => l.Top)
					.ThenBy(l => l.Left)
					.ToArray();

				return orderedLines.Select((l, index) => new { Line = l, Index = index }).First(pair => pair.Line == this).Index + 1;
			}
		}

		protected abstract IEnumerable<ControlCustomisationBase> GetParentCustomisationRecords();

		public virtual bool ManagesOwnSize
		{
			get { return false; }
		}

		#endregion

		#region SetDefaultsAfterAddedToCollection

		internal virtual void SetDefaultsAfterAddedToCollection()
		{
			var lastCustomisationLine = GetParentCustomisationRecords().MaxBySafe(c => c.DisplaySequence);
			if (lastCustomisationLine != null)
			{
				Top = lastCustomisationLine.Top + lastCustomisationLine.Height;
			}
		}

		#endregion

		#region Lookups

		public ControlCustomisationBaseLookups Lookups
		{
			get
			{
				if (lookups == null || !IsLookupsCachedInBase)
				{
					lookups = GetNewLookups();
				}

				return lookups;
			}
		}

		protected virtual ControlCustomisationBaseLookups GetNewLookups()
		{
			return new ControlCustomisationBaseLookups(this);
		}

		ControlCustomisationBaseLookups lookups;

		protected internal abstract CodeDescriptionPairList ControlTypes { get; }

		#endregion

		#region Validation

		public ControlCustomisationValidationBase Validation
		{
			get { return GetNewValidation(); }
		}

		protected abstract ControlCustomisationValidationBase GetNewValidation();

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#endregion

		#region IPreviewNotifierChild

		IPreviewReceiver IPreviewNotifierChild.PreviewReceiver
		{
			get { return customisation != null ? customisation.PreviewReceiver : null; }
		}

		#endregion

		#region BusinessObject overrides

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			Width = 200;
			Height = 20;
			BackgroundColor = Color.White.Name;
			ForegroundColor = Color.Black.Name;
			FontSize = 8;

			PlayButtonBehavior = StatusButtonBehaviorOptionsList.Codes.SaveAndClose;
			SuspendButtonBehavior = StatusButtonBehaviorOptionsList.Codes.SaveAndClose;
			CloseTaskButtonBehavior = StatusButtonBehaviorOptionsList.Codes.SaveAndClose;
		}

		public override void Delete()
		{
			base.Delete();
			if (Parent != null && Parent.PreviewReceiver != null)
			{
				Parent.PreviewReceiver.UpdatePreview();
			}
		}

		#endregion
	}
}
