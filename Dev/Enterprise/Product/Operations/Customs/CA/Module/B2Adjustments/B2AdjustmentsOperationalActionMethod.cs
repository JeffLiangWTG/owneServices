using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public class B2AdjustmentsOperationalActionMethod : OperationalActionMethod
	{
		public B2AdjustmentsOperationalActionMethod()
			: base(new ZGuid("4bef3a98-174a-49d3-aaf6-7cc521925537"))
		{
		}

		public override FilterRequirementList GetFilterRequirements()
		{
			var result = base.GetFilterRequirements();

			result.Add(FilterConstants.Country, new[]
			{
				Core.Constants.CountryCodes.Canada
			});

			return result;
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new B2AdjustmentsOperationalActionMethodApplicator(factory);
		}

		public override IComponent NewGuiControl()
		{
			return new B2AdjustmentsOperationActionControl();
		}

		public override bool HasControl => true;

		public override bool HasSettings => false;

		public override string Name => MethodName;
		public override string Description => MethodDescription;

		string MethodName => Res.GetString("cbc605e6-55cf-492d-9ef3-180257a671c9", "Insert Date Submitted operational action");
		string MethodDescription => Res.GetString("5f2c9b1e-53d1-402c-ab3a-8800c9b300cd", "Insert Date Submitted (CA)");
	}
}
