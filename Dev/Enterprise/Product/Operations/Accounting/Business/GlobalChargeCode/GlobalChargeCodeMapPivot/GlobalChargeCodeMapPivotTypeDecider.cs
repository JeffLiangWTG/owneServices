using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.GlobalChargeCode
{
	public class GlobalChargeCodeMapPivotTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Guid globalChargeCodePK = (Guid)row[GlobalChargeCodeMapPivot.Schema.YP_YG];
			ZGuid organisationPK = factory.Load<GlobalChargeCodeMap>(globalChargeCodePK).YG_OH;

			if (organisationPK.IsEmpty)
			{
				return typeof(GlobalChargeCodeMapPivotIntercompany);
			}
			else
			{
				return typeof(GlobalChargeCodeMapPivotOrganization);
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
