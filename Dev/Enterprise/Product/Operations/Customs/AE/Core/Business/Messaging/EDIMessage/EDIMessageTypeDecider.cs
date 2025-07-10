using System;
using System.Data;
using CargoWise.EntityFramework;
using static Enterprise.Integration.Customs.AE;

namespace Enterprise.Customs.AE.Business;

public class EDIMessageTypeDecider : TypeDecider, IEDIMessageTypeDecider
{
	public override Type GetTypeForBinding() => null;

	public override Type GetTypeForNew() => null;

	public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory) => typeof(AEEDIMessage);
}
