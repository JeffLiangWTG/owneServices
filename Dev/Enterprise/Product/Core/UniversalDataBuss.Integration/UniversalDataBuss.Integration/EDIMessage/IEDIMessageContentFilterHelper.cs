using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IEDIMessageContentFilterHelper
	{
		IEDIMessagePurpose GetEDIMessagePurpose(IFactory factory, string purposeCode);
		IEDIMessageContentFilter GetEDIMessageContentFilter(IFactory factory, IEDIMessagePurpose purpose);
	}
}
