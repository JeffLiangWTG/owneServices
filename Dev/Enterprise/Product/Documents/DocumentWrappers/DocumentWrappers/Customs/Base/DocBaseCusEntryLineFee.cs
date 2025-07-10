using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.DocumentWrappers.Customs.Base
{
	public class DocBaseCusEntryLineFee : DocBaseWrapper
	{
		protected DocBaseCusEntryLineFee(CusEntryLineFee cusEntryLineFee, BusinessObjectFactory factoryToWrap)
			: base(cusEntryLineFee, factoryToWrap)
		{
		}

		public static DocBaseCusEntryLineFee New(CusEntryLineFee cusEntryLineFee, BusinessObjectFactory factoryToWrap)
		{
			if (cusEntryLineFee is Enterprise.Customs.AU.Declaration.Business.CusEntryLineFee)
			{
				return AU.DocCusEntryLineFee.New((Enterprise.Customs.AU.Declaration.Business.CusEntryLineFee)cusEntryLineFee, factoryToWrap);
			}
			else
			{
				return new DocBaseCusEntryLineFee(cusEntryLineFee, factoryToWrap);
			}
		}

		CusEntryLineFee CusEntryLineFee
		{
			get { return (CusEntryLineFee)WrappedObject; }
		}

		public override string ToString()
		{
			return Description;
		}

		public ZDecimal ChargeAmount
		{
			get { return CusEntryLineFee.CF_ChargeAmount; }
		}

		public ZString ChargeType
		{
			get { return CusEntryLineFee.CF_ChargeType; }
		}

		public ZString Description
		{
			get
			{
				if (fDescription.IsEmpty)
				{
					if (CusEntryLineFee.EntryLine != null && CusEntryLineFee.EntryLine.Header != null)
					{
						fDescription = CusEntryLineFee.EntryLine.Header.EntryChargeTypeList.GetDescriptionFromCode(ChargeType);
					}
				}
				return fDescription;
			}
		}

		protected ZString fDescription;
	}
}
