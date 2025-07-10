using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.OperationalActions;
using Enterprise.Customs.Module;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.EU.Module.OperationalActions
{
	public class DeclarationUpdateSupportingDocumentsOperationalActionMethod : OperationalActionMethod
	{
		public DeclarationUpdateSupportingDocumentsOperationalActionMethod() : base(new ZGuid("180B9AF6-494A-40EF-9345-906B2EEF734E"))
		{
		}

		public DeclarationUpdateSupportingDocumentsOperationalActionMethod(ZGuid guid) : base(guid)
		{
		}

		public override string Name => Res.GetString("1B45C56E-0054-42B8-9AB0-2A528AD9DBE2", "Supporting Documents");

		public override string Description => Res.GetString("5CFC317E-D2D7-4743-A3A7-E49C3FDC42B1", "Supporting Documents");

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new DeclarationUpdateSupportingDocumentsApplicator(factory);
		}

		public override IComponent NewGuiControl()
		{
			return new DeclarationUpdateSupportingDocumentsApplicatorControl();
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
