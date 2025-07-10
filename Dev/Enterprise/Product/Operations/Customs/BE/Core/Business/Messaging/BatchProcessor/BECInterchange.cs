using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BE.Business;

public class BECInterchange : EDIInterchange, Integration.Customs.BE.IEDIInterchange
{
	public BECInterchange(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		EI_ApplicationCode = ApplicationCodes.BECustoms;
	}
}
