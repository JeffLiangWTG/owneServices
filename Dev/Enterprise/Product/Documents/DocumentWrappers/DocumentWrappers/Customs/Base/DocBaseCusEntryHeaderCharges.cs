using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.DocumentWrappers.Customs.Base
{
	public class DocBaseCusEntryHeaderCharges : DocBaseWrapper
	{
		protected DocBaseCusEntryHeaderCharges(CusEntryHeaderCharges cusEntryHeaderCharges, BusinessObjectFactory factoryToWrap)
			: base(cusEntryHeaderCharges, factoryToWrap)
		{
		}

		public static DocBaseCusEntryHeaderCharges New(CusEntryHeaderCharges cusEntryHeaderCharges, BusinessObjectFactory factoryToWrap)
		{
			if (cusEntryHeaderCharges is Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderCharges)
			{
				return AU.DocCusEntryHeaderCharges.New((Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderCharges)cusEntryHeaderCharges, factoryToWrap);
			}
			else
			{
				return new DocBaseCusEntryHeaderCharges(cusEntryHeaderCharges, factoryToWrap);
			}
		}

		CusEntryHeaderCharges CusEntryHeaderCharges
		{
			get { return (CusEntryHeaderCharges)WrappedObject; }
		}

		public override string ToString()
		{
			return Description;
		}

		public ZDecimal ChargeAmount
		{
			get { return CusEntryHeaderCharges.C1_ChargeAmount; }
		}

		public ZString ChargeType
		{
			get { return CusEntryHeaderCharges.C1_ChargeType; }
		}

		public ZString Description
		{
			get
			{
				if (fDescription.IsEmpty)
				{
					if (EntryHeader != null)
					{
						fDescription = EntryHeader.EntryChargeTypeList.GetDescriptionFromCode(ChargeType);
					}
				}
				return fDescription;
			}
		}

		protected ZString fDescription;

		protected virtual CusEntryHeader EntryHeader
		{
			get { return CusEntryHeaderCharges.EntryHeader; }
		}
	}
}
