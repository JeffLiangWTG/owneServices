using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public class EDIMessageContentFilterManager : IEDIMessageContentFilterManager
	{
		public EDIMessageContentFilterManager(IFactory factory, string purposeCode)
		{
			this.factory = factory;
			this.purposeCode = purposeCode;
		}

		readonly IFactory factory;
		readonly string purposeCode;

		public IEDIMessagePurpose EDIMessagePurpose
		{
			get
			{
				if (eDIMessagePurpose == null)
				{
					eDIMessagePurpose = ObjectFactory.New<IEDIMessageContentFilterHelper>().GetEDIMessagePurpose(factory, purposeCode);
				}
				return eDIMessagePurpose;
			}
		}

		IEDIMessagePurpose eDIMessagePurpose;

		public IEDIMessageContentFilter EDIMessageContentFilter
		{
			get
			{
				if (eDIMessageContentFilter == null)
				{
					eDIMessageContentFilter = ObjectFactory.New<IEDIMessageContentFilterHelper>().GetEDIMessageContentFilter(factory, EDIMessagePurpose);
				}
				return eDIMessageContentFilter;
			}
		}

		IEDIMessageContentFilter eDIMessageContentFilter;
	}
}
