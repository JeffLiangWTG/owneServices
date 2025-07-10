
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1
{
	public sealed class VoucherType : NonPersistentBusinessObject, IObsoleteValidation
	{
		public const string LocID = "T103";
		public ZString VoucherTypeNumber { get; set; }
		public ZString VoucherTypeName { get; set; }
		public ZString VoucherTypeAbbreviation { get; set; }
	}

	public sealed class VoucherTypeCollection : NonPersistentBusinessObjectCollection<VoucherType>	{
		public VoucherTypeCollection(int typeCount, BusinessObjectFactory factory)
			: base(factory)
		{
			if (typeCount == 3)
			{ AddDefaultElements3Type(); }

			if (typeCount == 1)
			{ AddDefaultElements1Type(); }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new VoucherType();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "China's Accounting fixed value")]
		void AddDefaultElements1Type()
		{
			VoucherType element = AddNew();
			element.VoucherTypeNumber = "1";
			element.VoucherTypeName = "记账凭证";
			element.VoucherTypeAbbreviation = "记";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "China's Accounting fixed value")]
		void AddDefaultElements3Type()
		{
			VoucherType element = AddNew();
			element.VoucherTypeNumber = "1";
			element.VoucherTypeName = "收款凭证";
			element.VoucherTypeAbbreviation = "收";

			element = AddNew();
			element.VoucherTypeNumber = "2";
			element.VoucherTypeName = "付款凭证";
			element.VoucherTypeAbbreviation = "付";

			element = AddNew();
			element.VoucherTypeNumber = "3";
			element.VoucherTypeName = "转账凭证";
			element.VoucherTypeAbbreviation = "转";
		}
	}
}

