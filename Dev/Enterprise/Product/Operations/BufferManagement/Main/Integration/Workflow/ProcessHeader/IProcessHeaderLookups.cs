using CargoWise.Integration;

namespace Enterprise.BufferManagement.Integration
{
	public interface IProcessHeaderLookups
	{
		ICodeDescriptionPairList DateAcceptabilityList { get; }
		ICodeDescriptionPairList DeadlineTypeList { get; }
	}
}
