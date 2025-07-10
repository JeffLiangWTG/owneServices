using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.CN.Module
{
	public class SendACDAOperationalActionMethod : OperationalActionMethod
	{
		public SendACDAOperationalActionMethod() : base(new ZGuid("62200495-BD04-41B0-9238-3AEEF68AC7EC"))
		{
		}

		public override string Name => Res.GetString("7EEF81A5-0884-46B6-8C43-E4C48FB0CEE6", "Send ACDA Message");

		public override string Description => Res.GetString("4669B79A-58D8-4F41-9F9A-0B26D34C84C2", "Send ACDA Message (CN)");

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new SendACDAOperationalActionMethodApplicator(factory);
		}

		public override FilterRequirementList GetFilterRequirements()
		{
			var result = base.GetFilterRequirements();

			result.Add(FilterConstants.Country, new[]
			{
				Core.Constants.CountryCodes.China
			});

			return result;
		}

		public override bool HasControl
		{
			get { return true; }
		}

		public override IComponent NewGuiControl()
		{
			return new SendACDAOperationActionControl();
		}
	}
}
