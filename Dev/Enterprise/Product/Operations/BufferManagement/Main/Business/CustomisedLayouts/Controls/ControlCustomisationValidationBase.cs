using System;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class ControlCustomisationValidationBase : ZValidation
	{
		public ControlCustomisationValidationBase(ControlCustomisationBase parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly ControlCustomisationBase parent;

		public override Type AutoValidationType
		{
			get { return parent.GetType(); }
		}

		public override void ValidateAll()
		{
			ValidateControlType();
			ValidateLeft();
			ValidateTop();
			ValidateWidth();
			ValidateHeight();
			ValidateBackgroundColor();
			ValidateForegroundColor();
			ValidateFontSize();
			ValidateFont();
			ValidateIsReadOnly();
			ValidateAlignment();
		}

		public void ValidateControlType()
		{
			ValidateCalculatedProperty(parent.ControlTypeInfo);
		}

		protected virtual void CheckControlType()
		{
			ListValidation.ErrorIfInvalidCode(parent.ControlTypeInfo);
			MandatoryValidation.CheckEntered(parent.ControlTypeInfo);
		}

		public void ValidateLeft()
		{
			ValidateCalculatedProperty(parent.LeftInfo);
		}

		protected void CheckLeft()
		{
			CompareValidation.CheckGreaterThanOrEqualTo(parent.LeftInfo, 0m);
		}

		public void ValidateTop()
		{
			ValidateCalculatedProperty(parent.TopInfo);
		}

		protected void CheckTop()
		{
			CompareValidation.CheckGreaterThanOrEqualTo(parent.TopInfo, 0m);
		}

		public void ValidateWidth()
		{
			ValidateCalculatedProperty(parent.WidthInfo);
		}

		protected void CheckWidth()
		{
			CompareValidation.CheckGreaterThanOrEqualTo(parent.WidthInfo, 0m);
		}

		public void ValidateHeight()
		{
			ValidateCalculatedProperty(parent.HeightInfo);
		}

		protected void CheckHeight()
		{
			CompareValidation.CheckGreaterThanOrEqualTo(parent.HeightInfo, 0m);
		}

		public void ValidateBackgroundColor()
		{
			ValidateCalculatedProperty(parent.BackgroundColorInfo);
		}

		protected virtual void CheckBackgroundColor()
		{
			ListValidation.ErrorIfInvalidCode(parent.BackgroundColorInfo);
		}

		public void ValidateForegroundColor()
		{
			ValidateCalculatedProperty(parent.ForegroundColorInfo);
		}

		protected void CheckForegroundColor()
		{
			ListValidation.ErrorIfInvalidCode(parent.ForegroundColorInfo);
		}

		public void ValidateFontSize()
		{
			ValidateCalculatedProperty(parent.FontSizeInfo);
		}

		protected void CheckFontSize()
		{
			CompareValidation.CheckGreaterThanOrEqualTo(parent.FontSizeInfo, 5m);
		}

		public void ValidateFont()
		{
			ValidateCalculatedProperty(parent.FontInfo);
		}

		protected void CheckFont()
		{
			ListValidation.ErrorIfInvalidCode(parent.FontInfo);
		}

		public void ValidateIsReadOnly()
		{
			ValidateCalculatedProperty(parent.IsReadOnlyInfo);
		}

		protected virtual void CheckIsReadOnly()
		{
		}

		public void ValidateAlignment()
		{
			ValidateCalculatedProperty(parent.AlignmentInfo);
		}

		protected void CheckAlignment()
		{
			ListValidation.ErrorIfInvalidCode(parent.AlignmentInfo);
		}

		public void ValidateOrientation()
		{
			ValidateCalculatedProperty(parent.OrientationInfo);
		}

		protected virtual void CheckOrientation()
		{
			ListValidation.ErrorIfInvalidCode(parent.OrientationInfo);
		}

		public void ValidatePlayButtonBehavior()
		{
			ValidateCalculatedProperty(parent.PlayButtonBehaviorInfo);
		}

		protected virtual void CheckPlayButtonBehavior()
		{
			if (parent.ControlType == StaticControlTypeList.Codes.StatusButtons || parent.ControlType == StaticControlTypeList.Codes.WorkingStatusButton)
			{
				MandatoryValidation.CheckEntered(parent.PlayButtonBehaviorInfo, Res.GetString("0a440ac8-489f-4746-9d77-5f0e6a6353d7", "Play button behavior"));
			}

			ListValidation.ErrorIfInvalidCode(ResString.GetMultilingualString("fd0c1310-a38e-480f-882a-80475a0181b4", "Please select a valid Play button behavior."), parent.PlayButtonBehaviorInfo);
		}

		public void ValidateSuspendButtonBehavior()
		{
			ValidateCalculatedProperty(parent.SuspendButtonBehaviorInfo);
		}

		protected virtual void CheckSuspendButtonBehavior()
		{
			if (parent.ControlType == StaticControlTypeList.Codes.StatusButtons || parent.ControlType == StaticControlTypeList.Codes.SuspendStatusButton)
			{
				MandatoryValidation.CheckEntered(parent.SuspendButtonBehaviorInfo, Res.GetString("3fe048df-4f22-42f5-980e-302dc683cd7c", "Suspend button behavior"));
			}

			ListValidation.ErrorIfInvalidCode(ResString.GetMultilingualString("92207d70-b204-44db-abd0-782d97f3c9dc", "Please select a valid Suspend button behavior."), parent.SuspendButtonBehaviorInfo);
		}

		public void ValidateCloseTaskButtonBehavior()
		{
			ValidateCalculatedProperty(parent.CloseTaskButtonBehaviorInfo);
		}

		protected virtual void CheckCloseTaskButtonBehavior()
		{
			if (parent.ControlType == StaticControlTypeList.Codes.StatusButtons || parent.ControlType == StaticControlTypeList.Codes.CompletedStatusButton)
			{
				MandatoryValidation.CheckEntered(parent.CloseTaskButtonBehaviorInfo, Res.GetString("d19c0bae-61de-4ffa-b50a-1e44a5c3bc55", "Close Task button behavior"));
			}

			ListValidation.ErrorIfInvalidCode(ResString.GetMultilingualString("1c6e7209-99bf-4451-b4f6-04b4a9004ae2", "Please select a valid Close Task button behavior."), parent.CloseTaskButtonBehaviorInfo);
		}
	}
}
