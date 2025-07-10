using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR5GZMessageData
	{
		ZDateTime NoticeDateTime { get; }
		ZString ImportDeclarationNumber { get; }
		ZString CustomsManagerName { get; }
		IEnumerable<KeyValuePair<ZString, ZString>> Unsettled { get; }
	}

	class GOVCBR5GZMessageData : IGOVCBR5GZMessageData
	{
		public ZDateTime NoticeDateTime { get; set; }
		public ZString ImportDeclarationNumber { get; set; }
		public ZString CustomsManagerName { get; set; }
		public IEnumerable<KeyValuePair<ZString, ZString>> Unsettled { get; set; }
	}
}
