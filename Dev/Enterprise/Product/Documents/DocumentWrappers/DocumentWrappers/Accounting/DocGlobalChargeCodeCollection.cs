using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.GlobalChargeCode;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocGlobalChargeCodeCollection : DocumentWrapperCollection
	{
		public DocGlobalChargeCodeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocGlobalChargeCodeCollection(IEnumerable<GlobalChargeCodeMap> chargeCodeMaps, BusinessObjectFactory factory)
			: base(factory)
		{
			foreach (var chargeCode in chargeCodeMaps)
			{
				Add(DocGlobalChargeCode.New(chargeCode, factory));
			}
		}

		public new DocGlobalChargeCode this[int index]
		{
			get { return (DocGlobalChargeCode)base[index]; }
		}
	}
}
