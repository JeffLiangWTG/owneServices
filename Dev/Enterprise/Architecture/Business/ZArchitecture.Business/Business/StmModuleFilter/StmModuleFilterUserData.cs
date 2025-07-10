using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public class StmModuleFilterUserData : AutoStmModuleFilterUserData
	{
		//const string ErrorReporterMessage = "Parent Layout is not found for Module Filter User Data";
		public StmModuleFilterUserData(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		internal void Initialise(FilterStripLayoutsHelper layoutsHelper)
		{
			S0_RelatedEntityID = layoutsHelper.CurrentUserPk;
			S0_RelatedEntityTableCode = layoutsHelper.CurrentUserTablePrefix;
		}

		#region Layout

		public StmModuleFilter Layout
		{
			get { return Factory.Load<StmModuleFilter>(S0_S9); }
		}

		#endregion

		#region Overrides

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override EnterpriseBusinessObject.AutologState AutoLoggingState => EnterpriseBusinessObject.AutologState.AutoLogged;

		protected override ZString CustomLogReferenceSuffix
		{
			get
			{
				return Res.GetString("a78013fb-0f7a-4280-96fa-0aeea5397871", "Rule Name: '{0}'", Layout?.S9_FilterName ?? Res.GetString("ed34d5e2-5f28-446c-a058-1991d3cf3ab5", "(Unknown)"));
			}
		}

		#endregion
	}
}
