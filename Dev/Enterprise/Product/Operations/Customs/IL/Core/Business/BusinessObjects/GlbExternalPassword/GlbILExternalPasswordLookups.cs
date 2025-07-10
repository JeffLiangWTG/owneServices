using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IL.Business
{
	public sealed class GlbILExternalPasswordLookups : GlbExternalPasswordLookups
	{
		public GlbILExternalPasswordLookups(AutoGlbExternalPassword parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList PasswordStatusList
			=> Factory.GetCachedValue("Enterprise.Customs.IL.Business.GlbILExternalPasswordLookups.PasswordStatusList", delegate
			{
				var codeDescriptionPairList = new CodeDescriptionPairList();
				codeDescriptionPairList.AddRange(base.PasswordStatusList);
				codeDescriptionPairList.AddPair(Constants.GlbILExternalPasswordLookups.PasswordAwa, Constants.GlbILExternalPasswordLookups.PasswordAwaiting);
				codeDescriptionPairList.AddPair(Constants.GlbILExternalPasswordLookups.PasswordReg, Constants.GlbILExternalPasswordLookups.PasswordRegistered);
				return codeDescriptionPairList;
			});

		public override GlbStaffCollection Staff
		{
			get
			{
				return Factory.GetCachedValue("IL.GlbILExternalPasswordLookups|Staff", () =>
				{
					var result = new ZDBOnlyQuery(typeof(GlbStaff));
					var subQry = new ZDBOnlySubQuery(typeof(GlbExternalPassword), GlbExternalPasswordSchema.GP_GS);
					subQry.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.ILS);
					result.AddSubQuery(GlbStaffSchema.PK, subQry, JoinCondition.And);

					return new GlbStaffCollection(Factory, result);
				});
			}
		}
	}
}
