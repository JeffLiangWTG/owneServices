using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public static class ParameterisationHelper
	{
		public static ZSqlParameter CreateUtcNowParameter(SchemaColumn column)
		{
			return ZSqlParameter.New("@UtcNow", ZDateTime.UtcNow.ToDateTime(), column);
		}

		public static ZSqlParameter CreateCurrentComponentParameter(BMComponentAcceptabilityBand band)
		{
			return ZSqlParameter.New("@CurrentComponentPK", band.BAB_FC_Component.ToGuid(), ProcessHeaderSchema.FH_FC_CurrentComponent);
		}

		public static ZSqlParameter CreateComponentPKParameter(ZGuid componentPk)
		{
			return ZSqlParameter.New("@ComponentPK", componentPk.ToGuid(), ProcessHeaderSchema.FH_FC_CurrentComponent);
		}

		public static ZSqlParameter CreateReleaseGroupParameter(AcceptabilityBandSqlBuilderParameters parameters)
		{
			return ZSqlParameter.New("@ReleaseGroupPK", parameters.ReleaseGroupPK.ToGuid(), ProcessHeaderSchema.FH_GG_ReleaseGroup);
		}

		public static ZSqlParameter CreateTagPKParameter(AcceptabilityBandSqlBuilderParameters parameters)
		{
			return ZSqlParameter.New("@TagPK", parameters.Tag.PK.ToGuid(), TagLinkSchema.TGL_TGM_Magnitude);
		}
	}
}
