using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.JP.Common.JPMessageActionList;
using static Enterprise.Customs.JP.Common.JPProcedureCodeList;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class ManifestMessageSendingObjectLookups : ZLookups
	{
		public ManifestMessageSendingObjectLookups(BusinessObject parent) : base(parent)
		{
		}

		protected new ManifestMessageSendingObject Parent => (ManifestMessageSendingObject)base.Parent;

		public CodeDescriptionPairList MessageTypeList => Factory.GetCachedValue<JPManifestProcedureCodeList>();

		public CodeDescriptionPairList ActionList
		{
			get
			{
				switch (Parent.Parent.CurrentProcedureCode)
				{
					case JPManifestProcedureCodeList.Codes.NVC01:
						return Factory.GetCachedValue<NVC01MessageActionList>();
					default:
						return Factory.GetCachedValue<HDF01MessageActionList>();
				}
			}
		}

		public CodeDescriptionPairList ReasonList => Factory.GetCachedValue<ReasonList>();
	}
}
