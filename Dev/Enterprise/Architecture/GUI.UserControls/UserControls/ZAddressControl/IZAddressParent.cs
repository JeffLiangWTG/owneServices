using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IZAddressParent
	{
		void SetControlSize();
		ZGuid ParseCode(string code);
		bool CheckZAddressBindingSuffix { get; }
	}
}
