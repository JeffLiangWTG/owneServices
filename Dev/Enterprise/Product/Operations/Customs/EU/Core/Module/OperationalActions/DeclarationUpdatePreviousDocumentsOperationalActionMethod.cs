using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.OperationalActions;
using Enterprise.Customs.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.EU.Module.OperationalActions
{
	public class DeclarationUpdatePreviousDocumentsOperationalActionMethod : OperationalActionMethod
	{
		public DeclarationUpdatePreviousDocumentsOperationalActionMethod() : base(new ZGuid("70B05DC4-178B-4308-ACBF-EB09AAC07BF5"))
		{
		}

		public DeclarationUpdatePreviousDocumentsOperationalActionMethod(ZGuid guid) : base(guid)
		{
		}

		public override string Name => Res.GetString("710A7F9A-37A3-4ECF-9035-3BBF75A6B675", "Previous Documents");

		public override string Description => Res.GetString("015078C4-5B94-4307-87E0-8F2997F9BFBC", "Previous Documents");

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return DeclarationUpdatePreviousDocumentsApplicator.GetByDataGroupingCode(factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		public override IComponent NewGuiControl()
		{
			return new DeclarationUpdatePreviousDocumentsApplicatorControl();
		}

		public override bool HasControl => true;

		public override FilterRequirementList GetFilterRequirements()
		{
			var result = base.GetFilterRequirements();
			result.Add(new FilterIsInEuropeanCustomsUnionOrInheritsFromEUConstraint().Name, new[] { "Y" });
			return result;
		}
	}
}
