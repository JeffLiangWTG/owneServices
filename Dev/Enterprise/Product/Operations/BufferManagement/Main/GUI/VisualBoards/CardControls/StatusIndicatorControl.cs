using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public class StatusIndicatorControl : ZUserControl
	{
		public StatusIndicatorControl(ITaskCardComponentParent parent, BMBoardSectionOrientation orientation)
		{
			this.parent = parent;
			this.orientation = orientation;

			parent.StatusUpdated += Parent_StatusUpdated;
			this.BindingContextChanged += Parent_StatusUpdated;

			BorderStyle = BorderStyle.None;
		}

		readonly ITaskCardComponentParent parent;
		readonly BMBoardSectionOrientation orientation;

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				parent.StatusUpdated -= Parent_StatusUpdated;
			}
		}

		void Parent_StatusUpdated(object sender, EventArgs e)
		{
			UpdateStatus();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (!IsDisposed && !Disposing)
			{
				if (dataSource != null)
				{
					UpdateStatus();
				}
			}
		}

		const int StatusIconSize = StaticControlCustomisation.StatusIndicatorSizeInPixels;
		const int StatusIconPadding = 2;

		void UpdateStatus()
		{
			Controls.RemoveAndDisposeAll();

			var applicableImages = new List<Image>();

			var cardContent = parent.CardContent;
			var taskStatus = cardContent.GetCustomAttribute<ZString>(StaticControlProperty.TaskStatus);

			if (cardContent.CardType == CardType.Task)
			{
				var requiresResourceWithCapability = cardContent.GetCustomAttribute<bool>(StaticControlProperty.RequiresResourceWithCapability);
				if (requiresResourceWithCapability)
				{
					applicableImages.Add(NextAvailableResourceImage);
				}
			}

			if (taskStatus == ProcessTaskStatusCodeList.Codes.Working)
			{
				applicableImages.Add(WorkingStatusImage);
			}
			else if (taskStatus == ProcessTaskStatusCodeList.Codes.Suspended)
			{
				applicableImages.Add(SuspendedStatusImage);
			}

			if (parent.CardContent.IsCurrent && taskStatus != ProcessTaskStatusCodeList.Codes.Working && IsStillCurrentWhenConsideringCacheReachAround)
			{
				applicableImages.Add(CurrentTaskImage);
			}

			if (cardContent.CardType == CardType.Task)
			{
				var prePostConstraintPosition = cardContent.GetCustomAttribute<ConstraintStatus>(StaticControlProperty.ConstraintStatus);

				switch (prePostConstraintPosition)
				{
					case ConstraintStatus.PreConstraint:
						applicableImages.Add(parent.Cell.IsConstraintAtRisk(cardContent.CapacityDto.PenetratedComponentsPK, isPreConstraintPosition: true) ?
							CCRPreconstraintAtRiskTaskImage :
							CCRPreconstraintTaskImage);
						break;
					case ConstraintStatus.PostConstraint:
						applicableImages.Add(parent.Cell.IsConstraintAtRisk(cardContent.CapacityDto.PenetratedComponentsPK, isPreConstraintPosition: false) ?
							CCRPostconstraintAtRiskTaskImage :
							CCRPostconstraintTaskImage);
						break;
				}
			}

			if (orientation == BMBoardSectionOrientation.Horizontal)
			{
				ControlDpiScalingHelper.SetWidth(this, StatusIconSize * applicableImages.Count + (StatusIconPadding * (applicableImages.Count - 1)), true);
				ControlDpiScalingHelper.SetHeight(this, StatusIconSize, true);
			}
			else
			{
				ControlDpiScalingHelper.SetWidth(this, StatusIconSize, true);
				ControlDpiScalingHelper.SetHeight(this, StatusIconSize * applicableImages.Count + (StatusIconPadding * (applicableImages.Count - 1)), true);
			}

			for (int i = 0; i < applicableImages.Count; i++)
			{
				var pictureBox = new OptimisticPictureBox(shouldDisposeImageOnControlDispose: false) // Image instances are retained statically to avoid memory over-consumption.
				{
					Image = applicableImages[i],
					Size = ControlDpiScalingHelper.NewScaledSize(StatusIconSize, StatusIconSize),
					SizeMode = PictureBoxSizeMode.StretchImage
				};
				ControlDpiScalingHelper.SetTop(pictureBox, orientation == BMBoardSectionOrientation.Horizontal ? 0 : i * StatusIconSize + (i * StatusIconPadding), true);
				ControlDpiScalingHelper.SetLeft(pictureBox, orientation == BMBoardSectionOrientation.Horizontal ? i * StatusIconSize + (i * StatusIconPadding) : 0, true);
				Controls.Add(pictureBox);
			}

#if DEBUG
			ShownImages = applicableImages;
#endif
		}

#if DEBUG
		public ICollection<Image> ShownImages { get; private set; }
#endif

		bool IsStillCurrentWhenConsideringCacheReachAround
		{
			get
			{
				// This is a hack. Detailed cards need to update things, but the PropertyCache does not get refreshed.
				var bizoContent = parent.CardContent as IBizoCardContent;
				return bizoContent == null || bizoContent.CardType == CardType.Workflow || bizoContent.Task.IsCurrent;
			}
		}

		#region Images

		public static Image WorkingStatusImage
		{
			get { return workingStatusImage ?? (workingStatusImage = CargoWise.NetworkVisualisation.Integration.Properties.Resources.play); }
		}
		[ThreadStatic]
		static Image workingStatusImage;

		public static Image NextAvailableResourceImage
		{
			get { return nextAvailableResourceImage ?? (nextAvailableResourceImage = Properties.Resources.question_mark_red); }
		}
		[ThreadStatic]
		static Image nextAvailableResourceImage;

		public static Image SuspendedStatusImage
		{
			get { return suspendedStatusImage ?? (suspendedStatusImage = CargoWise.NetworkVisualisation.Integration.Properties.Resources.pause); }
		}
		[ThreadStatic]
		static Image suspendedStatusImage;

		public static Image CurrentTaskImage
		{
			get { return currentTaskImage ?? (currentTaskImage = CargoWise.NetworkVisualisation.Integration.Properties.Resources.asterisk); }
		}
		[ThreadStatic]
		static Image currentTaskImage;

		public static Image CCRTaskImage
		{
			get { return ccrTaskImage ?? (ccrTaskImage = Properties.Resources.hourglass_icon_small); }
		}
		[ThreadStatic]
		static Image ccrTaskImage;

		public static Image CCRPreconstraintTaskImage
		{
			get { return ccrPreconstraintTaskImage ?? (ccrPreconstraintTaskImage = Properties.Resources.hourglass_preconstraint_icon_small); }
		}
		[ThreadStatic]
		static Image ccrPreconstraintTaskImage;

		public static Image CCRPostconstraintTaskImage
		{
			get { return ccrPostconstraintTaskImage ?? (ccrPostconstraintTaskImage = Properties.Resources.hourglass_postconstraint_icon_small); }
		}
		[ThreadStatic]
		static Image ccrPostconstraintTaskImage;

		public static Image CCRPreconstraintAtRiskTaskImage
		{
			get { return ccrPreconstraintAtRiskTaskImage ?? (ccrPreconstraintAtRiskTaskImage = Properties.Resources.hourglass_preconstraint_icon_small_red); }
		}
		[ThreadStatic]
		static Image ccrPreconstraintAtRiskTaskImage;

		public static Image CCRPostconstraintAtRiskTaskImage
		{
			get { return ccrPostconstraintAtRiskTaskImage ?? (ccrPostconstraintAtRiskTaskImage = Properties.Resources.hourglass_postconstraint_icon_small_red); }
		}
		[ThreadStatic]
		static Image ccrPostconstraintAtRiskTaskImage;

		#endregion
	}
}
