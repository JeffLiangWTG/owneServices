using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsCusInBondContainerTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForNew() => null;

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;
			var parentPk = (row != null) ? new ZGuid(row[CusInBondContainer.Schema.BC_ParentID]) : ZGuid.Empty;
			if (parentPk.IsValid)
			{
				var parentTableCode = new ZString(row[CusInBondContainer.Schema.BC_ParentTableCode]);
				if (parentTableCode.EqualsIgnoringCase(CusInBondHeaderSchema.Constants.Prefix)
					&& factory.Load<NctsHeader>(parentPk) is NctsHeader header)
				{
					result = header.ContainerType;
				}
			}

			return result ?? typeof(NctsCusInBondContainer);
		}
	}
}
