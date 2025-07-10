using System.Collections.Generic;

namespace Enterprise.BufferManagement.Business
{
	public class IVisualBoardChannelEqualityComparer : IEqualityComparer<IVisualBoardChannel>
	{
		bool IEqualityComparer<IVisualBoardChannel>.Equals(IVisualBoardChannel x, IVisualBoardChannel y)
		{
			return x != null
				&& y != null
				&& x.EntityPK == y.EntityPK
				&& x.ChannelEntityCode == y.ChannelEntityCode
				&& x.EntityType == y.EntityType;
		}

		int IEqualityComparer<IVisualBoardChannel>.GetHashCode(IVisualBoardChannel obj)
		{
			return obj.EntityPK.GetHashCode() ^ obj.ChannelEntityCode.GetHashCode() ^ obj.EntityType.GetHashCode();
		}
	}
}
