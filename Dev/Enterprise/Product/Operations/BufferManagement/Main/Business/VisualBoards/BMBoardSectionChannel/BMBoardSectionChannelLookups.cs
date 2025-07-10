using System.Globalization;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	public class BMBoardSectionChannelLookups : AutoBMBoardSectionChannelLookups
	{
		public BMBoardSectionChannelLookups(AutoBMBoardSectionChannel parent)
			: base(parent)
		{
		}

		protected new BMBoardSectionChannel Parent => (BMBoardSectionChannel)base.Parent;

		public ChannelTypeList ChannelTypes
		{
			get
			{
				return Factory.GetCachedValue(GetType().Name + "." + nameof(ChannelTypes), () =>
				{
					var list = new ChannelTypeList();
					list.RemoveCode(ChannelTypeList.Codes.NotChanneled);
					return list;
				});
			}
		}

		public IBusinessObjectCollection ParentList
		{
			get
			{
				return Factory.GetCachedValue<IBusinessObjectCollection>(ParentListCacheKey, () =>
				{
					switch (Parent.MSC_ChannelType)
					{
						case ChannelTypeList.Codes.Resource:
							return new GlbStaffCollection(Factory);

						case ChannelTypeList.Codes.Group:
							return new GlbGroupCollection(Factory);

						case ChannelTypeList.Codes.Capability:
							return new GlbCapabilityCollection(Factory);

						case ChannelTypeList.Codes.Tag:
							return new TagMagnitudeCollection(Factory);

						default:
							return new GlbStaffCollection(Factory, ZQuery.NoResultQuery);
					}
				});
			}
		}

		internal string ParentListCacheKey => string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", GetType().Name, nameof(ParentList), Parent.MSC_ChannelType);
	}
}
