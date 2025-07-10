using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("Container"), WrapperTypeName("ContainerPenalty")]
	public class ContainerPenaltyWrapper : GenericWrapper
	{
		public ContainerPenaltyWrapper(ContainerPenalty penaltyBO, BusinessObjectFactory factory)
			: base(penaltyBO, factory)
		{
			PenaltyBO = penaltyBO;
			PenaltyType = penaltyBO?.CPY_PenaltyType ?? ZString.Empty;
			FreeDays = penaltyBO?.FreeTimeAsDays ?? ZInt.Zero;
			FirstFreeDay = penaltyBO?.FirstFreeDay ?? ZDateTime.Empty;
			LastFreeDay = penaltyBO?.LastFreeDay ?? ZDateTime.Empty;
			PenaltyDays = penaltyBO?.DurationAsDays ?? ZInt.Zero;
		}

		readonly ContainerPenalty PenaltyBO;

		public ZString PenaltyType { get; private set; }
		public ZDateTime FirstFreeDay { get; private set; }
		public ZInt FreeDays { get; private set; }
		public ZDateTime LastFreeDay { get; private set; }
		public ZInt PenaltyDays { get; private set; }

		#region Container

		public ContainerWrapper Container
		{
			get { return container ?? (container = GetContainer()); }
		}
		ContainerWrapper container;

		protected ContainerWrapper GetContainer()
		{
			CommonContainer containerBO = PenaltyBO?.Container;
			return new ContainerWrapperFromFreight(containerBO, Factory);
		}

		#endregion

		#region AvailableDate

		public ZDateTime AvailableDate
		{
			get { return GetAvailableDate(); }
		}

		protected ZDateTime GetAvailableDate()
		{
			if (PenaltyBO != null)
			{
				var containerBO = PenaltyBO.Container;
				return (PenaltyBO.DetentionDirection == Constants.ContainerDetentionDirection.Import
					? containerBO?.JC_FCLAvailable
					: containerBO?.JC_FCLWharfGateIn) ?? ZDateTime.Empty;
			}

			return ZDateTime.Empty;
		}

		#endregion
	}
}
