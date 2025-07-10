using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Common;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.BufferManagement.Business;
using Color = System.Drawing.Color;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class BufferViewModel : NodeViewModel
	{
		public BufferViewModel(INetworkEntity entity, NetworkViewModel networkViewModel)
			: base(entity, networkViewModel)
		{
			if (!(entity.AsShape() is BMNCNBufferShape buffer))
			{
				return;
			}

			if (networkViewModel?.Network?.DiagramEntity != null
					&& networkViewModel.Network.DiagramEntity is ShapeNetworkEntity rootEntity
					&& rootEntity.Shape.ScheduleBizo == null) // it may happen if a buffer is somehow incorrecly saved on a non-scaled diagram due to some defect like WI00608931
			{
				ErrorReporter.ReportOnce("Somehow a buffer was incorrectly placed on a non-scaled diagram possibly because of a defect. A data transformation to remove incorrectly placed buffer may be required.");
				return;
			}

			penetrationPercentage = GetBufferPenetration(networkViewModel, buffer);
		}

		static double GetBufferPenetration(NetworkViewModel networkViewModel, BMNCNBufferShape buffer)
		{
			if (!buffer.IsInDatabase || !(networkViewModel?.Network?.DiagramEntity is ShapeNetworkEntity rootEntity))
			{
				return (double)buffer.BufferPenetration;
			}

			// in this case, we're re-displaying a buffer once added to a diagram, and so we want to recalculate penetration
			var scheduleBizo = rootEntity.Shape.ScheduleBizo;

			scheduleBizo.Validation.ValidateBNC_GB_Branch();
			scheduleBizo.Validation.ValidateBNC_GE_Department();

			WorkingTimeContext context = scheduleBizo.Validation.IsBranchAndDepartmentValid ? WorkingTimeContext.Create(rootEntity.Shape) : WorkingTimeContext.Create(scheduleBizo.Factory);

			buffer.UpdateBufferPenetration(context);

			return (double)buffer.BufferPenetration;
		}

		readonly double penetrationPercentage;
		public override int LowerBarHeight => LowerBar != null ? (int)(Height * 0.12) : 0;

		#region Status Brush

		protected override double StatusColorsAngle => 0;

		protected override IEnumerable<ColorOffset> GetStatusColors()
		{
			var colors = new List<ColorOffset>();
			var startColor = Color.DodgerBlue;
			var middleColor = Color.Yellow;
			var endColor = Color.Red;

			if (Entity.Status.HasFlag(WorkStatus.Complete))
			{
				startColor = startColor.FadeTowardsWhite(1.5f);
				middleColor = middleColor.FadeTowardsWhite(1.5f);
				endColor = endColor.FadeTowardsWhite(1.5f);
			}

			colors.Add(new ColorOffset(startColor, 0.9 / 3.0));
			colors.Add(new ColorOffset(middleColor, 1.0 / 3.0));
			colors.Add(new ColorOffset(middleColor, 2.0 / 3.0));
			colors.Add(new ColorOffset(endColor, 2.1 / 3.0));

			return colors;
		}

		#endregion

		public override ProgressBar LowerBar => new ProgressBar(Color.Transparent, penetrationPercentage, StatusOpacity);

		#region New Properties

		public string PenetrationPercentLabel
		{
			get { return GetDisplayablePenetrationPercent((decimal)penetrationPercentage); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1068:DoNotUseMathRound", Justification = "I want a double rounded not a decimal")]
		public static string GetDisplayablePenetrationPercent(decimal penetrationPercentage)
		{
			return penetrationPercentage != 0.0m ? string.Format(CultureInfo.InvariantCulture, "{0} %", Math.Round(penetrationPercentage * 100.0m, 0)) : string.Empty; // I want a double rounded not a decimal
		}

		#endregion

		#region NodeViewModel Overrides

		public override string CompletionCriteriaPlaceholder
		{
			get { return string.Empty; }
		}

		public override bool SupportsChildEntities
		{
			get { return false; }
		}

		public override bool SupportsEditEntity
		{
			get { return true; }
		}

		public override bool SupportsShowItems
		{
			get { return false; }
		}

		public override bool IsEditableNotesVisible
		{
			get { return false; }
		}

		public override bool IsCompletionCriteriaVisible
		{
			get { return false; }
		}

		public override bool ShowScheduleDetails
		{
			get { return false; }
		}

		#endregion
	}
}
