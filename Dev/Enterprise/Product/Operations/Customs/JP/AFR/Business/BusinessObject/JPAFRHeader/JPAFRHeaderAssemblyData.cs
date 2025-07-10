using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(Enterprise.Customs.JP.AFR.Business.JPAFRHeaderAssemblyData),
	Enterprise.Core.Constants.DocManagerCodes.JPAFRHeader)]

namespace Enterprise.Customs.JP.AFR.Business
{
	public class JPAFRHeaderAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType
		{
			get { return typeof(JPAFRHeader); }
		}

		protected override Type CollectionType
		{
			get { return typeof(JPAFRHeaderCollection); }
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new JPAFRHeaderCollection(factory);
		}

		public override string ReferenceType
		{
			get { return Enterprise.Core.Constants.ReferenceTypes.SupplyChainLogistics; }
		}
	}
}
