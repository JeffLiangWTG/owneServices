using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.OperationalActions;
using Enterprise.Customs.Module;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.FR.Module
{
	public class UpdateProductAdditionalInformationMethod : OperationalActionMethod, IApplicatorValidationSupport
	{
		public UpdateProductAdditionalInformationMethod()
			: base(new ZGuid("1E6F397A-C8C0-49C0-B3F6-EE4577495A35"))
		{
		}

		string NameAndDescription => Res.GetString("C8896EE5-A8AD-4467-986C-7D4ADDAB1733", "Update Product Additional Information");

		public override string Name => NameAndDescription;

		public override string Description => NameAndDescription;

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new UpdateProductAdditionalInformationApplicator(factory, this);
		}

		public bool IsValid { get; set; }

		public override FilterRequirementList GetFilterRequirements()
		{
			var result = base.GetFilterRequirements();
			result.Add(new FilterIsDeltaIEEnabledForImportsOrExportsConstraint().Name, new string[1] { "Y" });
			result.Add(new FilterIsUnderFrenchCustomsJurisdictionConstraint().Name, new string[1] { "Y" });
			return result;
		}

		public override IComponent NewGuiControl() => new UpdateProductAdditionalInformationApplicatorControl();

		public override bool HasControl => true;
	}
}
