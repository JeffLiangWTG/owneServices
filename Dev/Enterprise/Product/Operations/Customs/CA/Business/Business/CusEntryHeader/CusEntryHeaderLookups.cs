using Enterprise.Customs.Common.CA;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusEntryHeaderLookups : Customs.Business.CusEntryHeaderLookups
	{
		public CusEntryHeaderLookups(CusEntryHeader parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList MessageStatusList
		{
			get { return new MessageStatusList(new MessageTypeList().GetMultilingualDescriptionFromCode(Parent.CH_MessageType)); }
		}

		public override CodeDescriptionPairList CH_EntryStatusList
		{
			get
			{
				var entry = Parent;
				if (entry.IsB3C)
				{
					return Factory.GetCachedValue<B3EntryStatusList>();
				}
				else if (entry.IsCAD)
				{
					return Factory.GetCachedValue<CADEntryStatusList>();
				}
				else
				{
					return base.CH_EntryStatusList;
				}
			}
		}

		#region Implementation
		protected new CusEntryHeader Parent
		{
			get { return (CusEntryHeader)base.Parent; }
		}
		#endregion Implementation
	}
}
