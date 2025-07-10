using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.OperationalActions
{
	public class CreditCODDataObjectLookups : ZLookups
	{
		public CreditCODDataObjectLookups(CreditCODDataObject parent) : base(parent)
		{
		}

		public new CreditCODDataObject Parent => (CreditCODDataObject)base.Parent;

		public CodeDescriptionPairList CreditMethodList => Factory.GetCachedValue<CreditMethodList>();

		public CusEntryHeaderCollectionForCreditingCOD ReleasingEntryNoList
		{
			get
			{
				return new CusEntryHeaderCollectionForCreditingCOD(Factory, true);
			}
		}

		public CusEntryHeaderCollectionForCreditingCOD PreviousEntryNoList
		{
			get
			{
				return new CusEntryHeaderCollectionForCreditingCOD(Factory, false);
			}
		}
	}
}
