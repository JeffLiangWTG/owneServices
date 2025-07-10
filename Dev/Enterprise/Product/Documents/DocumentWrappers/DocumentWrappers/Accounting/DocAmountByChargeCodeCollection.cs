using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers
{
	public class DocAmountByChargeCodeCollection : GenericWrapperCollection<DocAmountByChargeCode>
	{
		public DocAmountByChargeCodeCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override DocumentWrapper WrapObject(object objectToWrap)
		{
			return DocAmountByChargeCode.New((AmountByChargeCode)objectToWrap, Factory);
		}

		protected override IBODocDataProvider GetRow(ZString index)
		{
			var includeCharges = index.Split(new char[] { ',' }).ToList();
			var trimIncludeCharges = new List<ZString>();
			includeCharges.ForEach(x => trimIncludeCharges.Add(x.Trim()));

			IEnumerable<DocAmountByChargeCode> collection = this.OfType<DocAmountByChargeCode>().Where(x => trimIncludeCharges.Contains(x.ChargeCode));
			ZDecimal total = collection.Sum(x => x.TotalAmount);
			ZDecimal totalExTax = collection.Sum(x => x.TotalAmountExTax);
			return DocAmountByChargeCode.New(new AmountByChargeCode(index, total, totalExTax), Factory);
		}
	}

	public class DocAmountByChargeCodeCollectionExcept : GenericWrapperCollection<DocAmountByChargeCode>
	{
		public DocAmountByChargeCodeCollectionExcept(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override DocumentWrapper WrapObject(object objectToWrap)
		{
			return DocAmountByChargeCode.New((AmountByChargeCode)objectToWrap, Factory);
		}

		protected override IBODocDataProvider GetRow(ZString index)
		{
			var excludeCharges = index.Split(new char[] { ',' }).ToList();
			var trimExcludedCharge = new List<ZString>();
			excludeCharges.ForEach(x => trimExcludedCharge.Add(x.Trim()));

			IEnumerable<DocAmountByChargeCode> collection = this.OfType<DocAmountByChargeCode>().Where(x => !trimExcludedCharge.Contains(x.ChargeCode));
			ZDecimal total = collection.Sum(x => x.TotalAmount);
			ZDecimal totalExTax = collection.Sum(x => x.TotalAmountExTax);
			return DocAmountByChargeCode.New(new AmountByChargeCode(index, total, totalExTax), Factory);
		}
	}
}

