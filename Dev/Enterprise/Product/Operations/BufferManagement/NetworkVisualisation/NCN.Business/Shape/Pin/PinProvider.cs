namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	class PinProvider : IShapePinProvider
	{
		internal PinProvider(ShapeNetworkEntity entity)
		{
			this.entity = entity;
		}

		readonly ShapeNetworkEntity entity;

		IShapePin IShapePinProvider.Pin
		{
			get
			{
				if (entity.Owner != null)
				{
					if (entity.IsApproved || entity.IsPinned)
					{
						return new ShapePin
						{
							Ancestor = entity.IsPinned ? entity.Root : entity.Owner,
							XOffset = entity.GetXOffsetFromParent(),
							YOffset = entity.GetYOffsetFromParent()
						};
					}
				}

				return null;
			}
		}
	}
}
