using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBRR60MessageData
	{
		ZDateTime NoticeDateTime { get; }
		ZString ExportDeclarationNumber { get; }
		ZString SupplierName { get; }
		IEnumerable<ICommodity> Commodity { get; }
	}

	interface ICommodity
	{
		ZString ModelName { get; }
		ZString ComplementDescription { get; }
	}

	class GOVCBRR60MessageData : IGOVCBRR60MessageData
	{
		public ZDateTime NoticeDateTime { get; set; }
		public ZString ExportDeclarationNumber { get; set; }
		public ZString SupplierName { get; set; }
		public IEnumerable<ICommodity> Commodity { get; set; }
	}

	class Commodity : ICommodity
	{
		public ZString ModelName { get; set; }
		public ZString ComplementDescription { get; set; }
	}
}
