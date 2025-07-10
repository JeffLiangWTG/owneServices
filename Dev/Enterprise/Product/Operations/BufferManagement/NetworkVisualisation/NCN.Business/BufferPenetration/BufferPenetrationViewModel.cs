using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class BufferPenetrationViewModel : NonPersistentBusinessObject
	{
		public BufferPenetrationViewModel(BMNCNShape shape)
			: base(Argument.NotNull(shape, nameof(shape)).Factory)
		{
			this.shape = shape;
			buffer = shape as IBuffer;
		}

		readonly BMNCNShape shape;
		readonly IBuffer buffer;

		#region Properties

		public ZString Name
		{
			get { return shape.Name; }
		}

		public ZString PenetrationItemsHint
		{
			get
			{
				return buffer != null
					? Res.GetString("3b9eb559-b2bd-49b2-a656-a8129292bc23", "Items which affect the penetration of this buffer are listed below.")
					: Res.GetString("41a72b6c-f6fb-4d74-8ef1-e03a0163fb2f", "Buffers which this item can penetrate are listed below.");
			}
		}

		public ZString PenetrationCalculationHint
		{
			get
			{
				return Res.GetString("8df5fd0e-a510-4f78-aaef-535aa6e917de",
					@"For buffers an item directly penetrates, buffer penetration is calculated as: (Time Since Startable, minus Planned Duration, plus Remaining Estimate) divide by Buffer Duration.

A penetration amount greater than 100% on any buffer can 'overflow' onto other buffers downstream in the dependency network. The penetration for these overflow buffers is calculated as: overflow time divide by Buffer Duration.");
			}
		}

		#endregion

		#region Related Business Objects

		public BufferedItemBufferPenetrationViewModelCollection PenetrationItemsCollection
		{
			get
			{
				if (penetrationItemsCollection == null)
				{
					if (buffer != null)
					{
						penetrationItemsCollection = new BufferedItemBufferPenetrationViewModelCollection(buffer);
					}
					else
					{
						penetrationItemsCollection = new BufferedItemBufferPenetrationViewModelCollection(shape);
					}
				}

				return penetrationItemsCollection;
			}
		}

		BufferedItemBufferPenetrationViewModelCollection penetrationItemsCollection;

		#endregion
	}
}
