using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class SetEntryStatusDetailLookups : ZLookups
	{
		public SetEntryStatusDetailLookups(SetEntryStatusDetail parent)
			: base(parent)
		{
		}

		protected new SetEntryStatusDetail Parent => base.Parent as SetEntryStatusDetail;

		public ActiveCusEntryHeaderCollection ActiveEntryHeaders => Parent.JobDeclaration?.ActiveEntryHeaders;

		public CodeDescriptionPairList EntryStatusList
		{
			get
			{
				var dec = Parent.JobDeclaration;
				return dec == null ? new CodeDescriptionPairList() : dec.Lookups.EntryStatusList;
			}
		}
	}
}
