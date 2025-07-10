using System.Collections;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class CusEntryLineLookups : Customs.Business.CusEntryLineLookups
	{
		public CusEntryLineLookups(CusEntryLine parent)
			: base(parent)
		{
		}

		public CusEntryLine EntryLine
		{
			get { return Parent; }
		}

		protected new CusEntryLine Parent
		{
			get { return (CusEntryLine)base.Parent; }
		}

		public ICollection Countries => new RefCountryCollection(Factory);

		public CodeDescriptionPairList DutyRateCodeList
		{
			get
			{
				return Factory.GetCachedValue<DutyRateCodeList>();
			}
		}

		public CodeDescriptionPairList PreferenceCodeList
		{
			get
			{
				var codes = new CodeDescriptionPairList();
				codes.AddPair(Parent.PreferenceCode, Parent.PreferenceCodeDescription);
				return codes;
			}
		}
	}
}
