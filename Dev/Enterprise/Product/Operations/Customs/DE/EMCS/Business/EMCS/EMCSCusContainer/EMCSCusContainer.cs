using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class EMCSCusContainer : EU.EMCS.Business.EMCSCusContainer
		, Integration.Customs.DEEMCS.IEMCSCusContainer
	{
		public EMCSCusContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new EMCSCusContainerValidation Validation => (EMCSCusContainerValidation)base.Validation;

		protected override Customs.Business.CusContainerValidation GetNewValidation() => new EMCSCusContainerValidation(this);

		public new EMCSJobDeclaration Declaration => (EMCSJobDeclaration)base.Declaration;
	}
}
