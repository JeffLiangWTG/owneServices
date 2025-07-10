using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow.Testing
{
	[TestedType(typeof(GlowDeleteReceiveConsignment))]
	internal class GlowDeleteReceiveConsignment_HasReceiveConsignmentRTUDivotTest : GlowDelete_HasReceiveConsignmentRTUDivot
	{
		protected override string TableName => WhsItemReceiveConsignmentSchema.Constants.TableName;
		protected override string PKName => WhsItemReceiveConsignmentSchema.Constants.PK;
		protected override string AutoVersion => "WRC_AutoVersion";

		protected override string StoredProcedureName => "GlowDeleteReceiveConsignment";
		protected override string StoredProcedureAttributeName => "@ReceiveConsignmentPK";

		protected override Guid GetEntityPK()
		{
			return pk_rcn;
		}
	}
}

