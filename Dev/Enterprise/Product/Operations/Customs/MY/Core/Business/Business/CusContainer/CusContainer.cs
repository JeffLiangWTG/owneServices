using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.MY.Business
{
	public class CusContainer : TypeSafeCusContainer, Integration.Customs.MY.ICusContainer
	{
		public CusContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implementation

		#region protected override

		protected override Customs.Business.CusContainerLookups GetNewLookups()
		{
			return new CusContainerLookups(this);
		}

		protected override Customs.Business.CusContainerValidation GetNewValidation()
		{
			return new CusContainerValidation(this);
		}

		protected override System.Type GetJobDeclarationType()
		{
			return typeof(JobDeclaration);
		}

		#endregion

		#endregion
	}
}
