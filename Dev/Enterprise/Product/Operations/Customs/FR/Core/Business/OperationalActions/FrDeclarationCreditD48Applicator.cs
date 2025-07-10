using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.OperationalActions
{
	public class FrDeclarationCreditD48Applicator : AutoFrDeclarationCreditD48Applicator
	{
		public FrDeclarationCreditD48Applicator(BusinessObjectFactory factory) : base((NoResString)"FR Credit D48", factory)
		{
		}

		[ResourceStringData("FrDeclarationCreditD48Applicator.D48DocumentCode", Caption = "Document Code")]
		public override ZString D48DocumentCode { get => base.D48DocumentCode; set => base.D48DocumentCode = value; }

		[ResourceStringData("FrDeclarationCreditD48Applicator.ReferenceNumber", Caption = "Reference Number")]
		public override ZString ReferenceNumber { get => base.ReferenceNumber; set => base.ReferenceNumber = value; }

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var runner = new CreditD48OperationalActionRunner(log, targets.OfType<JobDeclaration>());
			runner.CreditD48(this);
		}
	}
}
