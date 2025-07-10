using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class StaticControlCustomisation : ControlCustomisationBase
	{
		public StaticControlCustomisation(BMControlCustomisation customisation)
			: base(customisation)
		{
		}

		#region Properties

		#region BackGroundColor

		protected override ZString GetBackgroundColorCore()
		{
			if (ControlType == StaticControlTypeList.Codes.AttachedTagsIndicator)
			{
				return Color.Transparent.Name;
			}

			return base.GetBackgroundColorCore();
		}

		#endregion

		#region ControlType

		[List("Lookups.ControlTypes")]
		public override ZString ControlType
		{
			get { return base.ControlType; }
			set
			{
				base.ControlType = value;

				if (value == StaticControlTypeList.Codes.CloseButton)
				{
					Width = 25;
					Height = 25;
					Label = ZString.Empty;
				}
				else if (value == StaticControlTypeList.Codes.SaveButton)
				{
					Width = 40;
					Height = 22;
				}
				else if (value == StaticControlTypeList.Codes.OpenJobButton)
				{
					Width = 61;
					Height = 22;
				}
				else if (value == StaticControlTypeList.Codes.NudgeControls)
				{
					Width = 84;
					Height = 26;
				}
				else if (value == StaticControlTypeList.Codes.TaskStatusIndicator)
				{
					Width = StatusIndicatorSizeInPixels;
					Height = StatusIndicatorSizeInPixels;
				}
				else if (value == StaticControlTypeList.Codes.StatusButtons)
				{
					Width = 211;
					Height = 26;
				}
				else if (value == StaticControlTypeList.Codes.DateAcceptabilityPicture)
				{
					Width = 58;
					Height = 20;
				}

				if (ControlManagesOwnAppearance(value))
				{
					BackgroundColor = ForegroundColor = ZString.Empty;
				}
			}
		}

		public const int StatusIndicatorSizeInPixels = 12;

		internal bool IsReadOnlyControlType
		{
			get { return IsReadonlyControlType(ControlType); }
		}

		public static bool IsReadonlyControlType(string controlType)
		{
			switch (controlType)
			{
				case StaticControlTypeList.Codes.AttachedTagsIndicator:
				case StaticControlTypeList.Codes.DateAcceptabilityPicture:
				case StaticControlTypeList.Codes.Label:
				case StaticControlTypeList.Codes.TaskStatusIndicator:

					return true;

				default:
					return false;
			}
		}

		#endregion

		#region Label

		protected override string GetLabelForControlType()
		{
			switch (ControlType)
			{
				case StaticControlTypeList.Codes.CloseButton:
					return Res.GetString("a16d16af-e2bd-4320-999b-66235698df93", "Close");

				case StaticControlTypeList.Codes.OpenJobButton:
					return Res.GetString("e3d2cac3-7cc2-4adb-8949-0b57b2d671be", "Open Job");

				case StaticControlTypeList.Codes.SaveButton:
					return Res.GetString("d3e856cb-5ca8-41f8-8a1f-a398e1c5b5ad", "Save");
			}

			return string.Empty;
		}

		#endregion

		#region ReadOnliness

		protected bool Width_ReadOnly
		{
			get { return IsControlTypeFixedSize(ControlType); }
		}

		protected bool Height_ReadOnly
		{
			get { return IsControlTypeFixedSize(ControlType); }
		}

		protected bool BackgroundColor_ReadOnly
		{
			get { return ControlManagesOwnAppearance(ControlType); }
		}

		protected bool ForegroundColor_ReadOnly
		{
			get { return ControlManagesOwnAppearance(ControlType); }
		}

		protected bool Font_ReadOnly
		{
			get { return ControlManagesOwnAppearance(ControlType); }
		}

		protected bool FontSize_ReadOnly
		{
			get { return ControlManagesOwnAppearance(ControlType); }
		}

		protected bool IsBold_ReadOnly
		{
			get { return ControlManagesOwnAppearance(ControlType); }
		}

		protected bool IsReadOnly_ReadOnly
		{
			get { return ControlManagesOwnAppearance(ControlType); }
		}

		static bool IsControlTypeFixedSize(string controlType)
		{
			switch (controlType)
			{
				case StaticControlTypeList.Codes.DateAcceptabilityPicture:
				case StaticControlTypeList.Codes.NudgeControls:
				case StaticControlTypeList.Codes.StatusButtons:
				case StaticControlTypeList.Codes.TaskStatusIndicator:
					return true;

				default:
					return false;
			}
		}

		static bool ControlManagesOwnAppearance(string controlType)
		{
			switch (controlType)
			{
				case StaticControlTypeList.Codes.AttachedTagsIndicator:
				case StaticControlTypeList.Codes.StatusButtons:
				case StaticControlTypeList.Codes.TaskStatusIndicator:
					return true;

				default:
					return false;
			}
		}

		#endregion

		#endregion

		#region New Properties

		protected override IEnumerable<ControlCustomisationBase> GetParentCustomisationRecords()
		{
			return Parent.CustomisedControls.Cast<ControlCustomisationBase>();
		}

		public override bool ManagesOwnSize
		{
			get { return ControlsWhichManageOwnSize.Any(s => s == ControlType); }
		}

		IEnumerable<string> ControlsWhichManageOwnSize
		{
			get
			{
				yield return StaticControlTypeList.Codes.TaskStatusIndicator;
				yield return StaticControlTypeList.Codes.WorkingStatusButton;
				yield return StaticControlTypeList.Codes.CompletedStatusButton;
				yield return StaticControlTypeList.Codes.SuspendStatusButton;
			}
		}

		#endregion

		#region Lookups

		protected internal override CodeDescriptionPairList ControlTypes
		{
			get { return Factory.GetCachedValue<StaticControlTypeList>(); }
		}

		#endregion

		#region Validation

		public new ControlCustomisationValidationBase Validation
		{
			get { return GetNewValidation(); }
		}

		protected override ControlCustomisationValidationBase GetNewValidation()
		{
			return new StaticControlCustomisationValidation(this);
		}

		#endregion
	}
}
