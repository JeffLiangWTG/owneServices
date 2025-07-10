using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow.Testing
{
	[TestedType(typeof(GlowDeleteReceiveTransportationUnit))]
	class GlowDeleteReceiveTransportationUnit_HasReceiveConsignmentRTUDivotTest : GlowDelete_HasReceiveConsignmentRTUDivot
	{
		protected override string TableName => WhsItemReceiveTransportationUnitSchema.Constants.TableName;
		protected override string PKName => WhsItemReceiveTransportationUnitSchema.Constants.PK;
		protected override string AutoVersion => "WRH_AutoVersion";

		protected override string StoredProcedureName => "GlowDeleteReceiveTransportationUnit";

		protected override string StoredProcedureAttributeName => "@ReceiveTransportationUnitPK";

		protected override Guid GetEntityPK()
		{
			return pk_rtu;
		}
	}
}
