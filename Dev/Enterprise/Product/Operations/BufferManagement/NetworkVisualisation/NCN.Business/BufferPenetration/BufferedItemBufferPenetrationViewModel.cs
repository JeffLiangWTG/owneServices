using System.ComponentModel;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Integration;
using CommonBufferPenetrationCalculator = CargoWise.PAVE.Common.Implementation.BufferPenetrationCalculator;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class BufferedItemBufferPenetrationViewModel : NonPersistentBusinessObject
	{
		public BufferedItemBufferPenetrationViewModel(IBuffer buffer, IBufferedItem bufferedItem)
			: base(Argument.NotNull((IBusiness)buffer, nameof(buffer)).Factory)
		{
			this.buffer = buffer;
			this.bufferedItem = Argument.NotNull(bufferedItem, nameof(bufferedItem));
			bufferedItemWithRelatedEntity = bufferedItem as IBufferedItemWithRelatedEntity;

			var penetration = BufferPenetrationCalculator.CalculatePenetrationPercentage(bufferedItem, buffer, WorkingTimeContext.Create((IBranchDepartmentProvider)buffer, null, Factory), Factory);

			PenetrationPercent = BufferViewModel.GetDisplayablePenetrationPercent(penetration.Penetration);
			IsOverflow = penetration.IsOverflowFromUpstreamBuffer;
			TimeSinceStartable = GetTimeSinceStartable();
		}

		readonly IBuffer buffer;
		readonly IBufferedItem bufferedItem;
		readonly IBufferedItemWithRelatedEntity bufferedItemWithRelatedEntity;

		#region Properties

		[ResourceStringData("BufferedItemBufferPenetration.BufferName", Caption = "Buffer", FullDescription = "The buffer this item affects.")]
		public ZString BufferName
		{
			get { return buffer.Name; }
		}

		[ResourceStringData("BufferedItemBufferPenetration.BufferedItemName", Caption = "Name", FullDescription = "The item which affects the penetration of this buffer.")]
		public ZString BufferedItemName
		{
			get { return bufferedItem.Name; }
		}

		[ResourceStringData("BufferedItemBufferPenetration.BufferedItemRelatedEntityName", Caption = "Related Entity Name", FullDescription = "The related entity this item represents.")]
		public ZString BufferedItemRelatedEntityName
		{
			get { return bufferedItemWithRelatedEntity != null ? bufferedItemWithRelatedEntity.RelatedEntityName : string.Empty; }
		}

		[ReadOnly(true)]
		[ResourceStringData("BufferedItemBufferPenetration.PenetrationPercent", Caption = "Penetration Percent", ShortCaption = "Penetration %", FullDescription = "Percentage penetration of this item into the buffer.")]
		public ZString PenetrationPercent { get; private set; }

		[ReadOnly(true)]
		[ResourceStringData("BufferedItemBufferPenetration.IsOverflow", Caption = "Overflow", FullDescription = "Indicates whether this buffer is penetrated by overflow from another upstream buffer.")]
		public ZBool IsOverflow { get; private set; }

		[ResourceStringData("BufferedItemBufferPenetration.ScheduledStartTime", Caption = "Scheduled Start Time", ShortCaption = "Start Time", FullDescription = "The time this item was scheduled to start.")]
		public ZDateTime ScheduledStartTime
		{
			get { return bufferedItem.StartableTime; }
		}

		[ReadOnly(true)]
		[ResourceStringData("BufferedItemBufferPenetration.TimeSinceStartable", Caption = "Time Since Startable", FullDescription = "The amount of time that has passed since this item was scheduled to start.")]
		public ZDateTime TimeSinceStartable { get; private set; }

		ZDateTime GetTimeSinceStartable()
		{
			var workingTimeContext = new WorkingTimeContextWithFactory(WorkingTimeContext.Create((IBranchDepartmentProvider)buffer), Factory);
			var age = CommonBufferPenetrationCalculator.GetAgeInMinutes(bufferedItem, workingTimeContext, ZDateTime.UtcNow.ToDateTime());
			return new ZInt(age).GetDateTimeFromMinutes();
		}

		[ResourceStringData("BufferedItemBufferPenetration.PlannedDuration", Caption = "Planned Duration", FullDescription = "The planned duration of this item according to its approved diagram schedule.")]
		public ZDateTime PlannedDuration
		{
			get { return new ZInt(bufferedItem.PlannedDurationInMinutes).GetDateTimeFromMinutes(); }
		}

		[ResourceStringData("BufferedItemBufferPenetration.RemainingEstimatedDuration", Caption = "Remaining Estimated Duration", MediumCaption = "Remaining Estimate", ShortCaption = "Remaining", FullDescription = "The remaining estimated time of the open tasks within to this item.")]
		public ZDateTime RemainingEstimatedDuration
		{
			get { return new ZInt(bufferedItem.RemainingEstimateInMinutes).GetDateTimeFromMinutes(); }
		}

		[ResourceStringData("BufferedItemBufferPenetration.Status", Caption = "Status")]
		public ZString Status
		{
			get { return bufferedItem.WorkStatus.ToString(); }
		}

		[ResourceStringData("BufferedItemBufferPenetration.BufferDuration", Caption = "Buffer Duration", FullDescription = "The size duration of the buffer according to its approved diagram schedule.")]
		public ZDateTime BufferDuration
		{
			get { return new ZInt(buffer.SizeInMinutes).GetDateTimeFromMinutes(); }
		}

		#endregion
	}
}
