using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.GlobalChargeCode
{
	public class GlobalChargeCodeMapTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			ZString organisationPK = (ZString)row[GlobalChargeCodeMap.Schema.YG_OH].ToString();
			if (organisationPK.IsEmpty)
			{
				return typeof(GlobalChargeCodeMapIntercompany);
			}
			else
			{
				return typeof(GlobalChargeCodeMapOrganization);
			}
		}

		public override Type GetTypeForNew()
		{
			throw new NoConcreteTypeException("New Transaction type cannot be determined");
		}

		public override Type GetTypeForBinding()
		{
			return null;
		}
	}
}
