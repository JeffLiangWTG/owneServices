using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.H7.Business;

public class AdditionalInfo : EU.H7.Business.AdditionalInfo
{
	public AdditionalInfo(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}
}
