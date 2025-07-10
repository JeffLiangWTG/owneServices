using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Web.Business
{
	public interface IWrappedBizOProvider
	{
		BusinessObject GetWrappedBizO();
		string GetWrappedBindTo(string bindTo);
	}
}