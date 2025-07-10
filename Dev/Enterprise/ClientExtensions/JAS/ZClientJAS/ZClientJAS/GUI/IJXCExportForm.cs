
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.JAS.GUI
{
	public interface IJXCExportForm : IZForm
	{
		void ValidateAll();
		BusinessObject BusinessEntity { get; }
	}
}
