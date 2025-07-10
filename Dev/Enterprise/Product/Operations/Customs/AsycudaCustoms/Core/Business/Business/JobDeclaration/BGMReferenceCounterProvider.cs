using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.Business;

public interface IBGMReferenceCounterProvider
{
	int GetBGMReferenceCounter(ZGuid parentId, int incrementStep);
}

sealed class BGMReferenceCounterProvider : IBGMReferenceCounterProvider
{
	BGMReferenceCounterProvider()
	{
	}

	internal static readonly Overridable<IBGMReferenceCounterProvider> Instance = new(new BGMReferenceCounterProvider());

	[SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "<Pending>")]
	public int GetBGMReferenceCounter(ZGuid parentId, int incrementStep)
	{
		var nextBGMReferenceCounter = -1;

		if (incrementStep > 0)
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			using (var cmd = connection.Command(GetCounterSqlText))
			{
				cmd.AddParameterBasedOnDbColumn("@XA_ParentID", parentId.ToGuid(), GenAddOnColumnSchema.XA_ParentID);
				cmd.AddParameterBasedOnDbColumn("@XA_Name", JobDeclaration.BGMReferenceCounterString, GenAddOnColumnSchema.XA_Name);
				cmd.AddParameterBasedOnDbColumn("@XA_SystemLastEditUser", GlbStaff.CurrentUser?.GS_Code.ToString() ?? User.UnKnownUserCode, GenAddOnColumnSchema.XA_SystemLastEditUser);
				cmd.AddParameter("@LockName", SqlDbType.NVarChar, string.Join("_", "Lock", JobDeclaration.BGMReferenceCounterString, parentId.ToString()));  // SQL Parameter
				cmd.AddParameter("@incrementStep", SqlDbType.Int, incrementStep);

				nextBGMReferenceCounter = Convert.ToInt32(cmd.ExecuteScalar());
			}
		}

		return nextBGMReferenceCounter;
	}

	const string GetCounterSqlText = "EXEC IncrementBGMReferenceCounter @LockName, @XA_Name, @XA_ParentID, @XA_SystemLastEditUser, @incrementStep";
}
