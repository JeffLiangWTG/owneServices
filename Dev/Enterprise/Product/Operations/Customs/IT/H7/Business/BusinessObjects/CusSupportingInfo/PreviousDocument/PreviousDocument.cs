using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.H7.Business;

public class PreviousDocument : EU.H7.Business.PreviousDocument
{
	public PreviousDocument(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}
}
