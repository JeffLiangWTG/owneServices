using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageHeaderValueSetStrategy : IValueSetStrategy
	{
		public TemporaryStorageHeaderValueSetStrategy(TemporaryStorageHeader header)
		{
			Header = header;
		}

		protected readonly TemporaryStorageHeader Header;

		public void ValueSet(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			ValueSetCore(valueThatHasChanged, oldValue);
		}

		protected virtual void ValueSetCore(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			switch (valueThatHasChanged.Name)
			{
				case TemporaryStorageHeader.Schema.AMA_MessageType:
					ChangeIsENSReuse();
					break;
				case CusTempStorage.TemporaryStorageHeader.Schema.AMA_TransportMode:
					ChangeTransportType();
					break;
			}
		}

		void ChangeIsENSReuse()
		{
			if (Header.AMA_MessageType == PNTSMessageTypeList.Codes.PresentationNotification)
			{
				Header.IsENSReuse = ZBool.False;
			}
		}

		void ChangeTransportType()
		{
			var lookups = Header.Lookups;
			var transportTypeList = lookups.TransportTypeList;
			if (transportTypeList.Count == 1)
			{
				Header.TransportType = transportTypeList[0].Code;
			}
		}
	}
}
