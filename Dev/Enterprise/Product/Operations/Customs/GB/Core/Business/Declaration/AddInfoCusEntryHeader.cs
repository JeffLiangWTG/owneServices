using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class AddInfoCusEntryHeader : EU.Business.Declaration.AddInfoCusEntryHeader
	{
		public AddInfoCusEntryHeader(ZPropertyInfo addInfoProperty) : base(addInfoProperty)
		{
		}

		[List(nameof(Lookups) + "." + nameof(GBAddInfoCusEntryHeaderLookups.AmendmentReasonCodeList))]
		public override ZString ZG_AmendmentReasonCode
		{
			get { return base.ZG_AmendmentReasonCode; }
			set { base.ZG_AmendmentReasonCode = value; }
		}

		public new GBAddInfoCusEntryHeaderLookups Lookups => (GBAddInfoCusEntryHeaderLookups)base.Lookups;

		protected override EUAddInfoLookups GetNewLookups() => new GBAddInfoCusEntryHeaderLookups(this);
	}
}
