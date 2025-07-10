using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	class FreightWrapperFromCFSContainer : FreightWrapper
	{
		public FreightWrapperFromCFSContainer(CFSContainer containerBO, BusinessObjectFactory factory)
			: base(containerBO, factory)
		{
			this.containerBO = containerBO;
		}
		readonly CFSContainer containerBO;

		protected override ZString GetJobNumberHeading()
		{
			return Res.GetString("640d37cb-17cb-4f73-9a13-f3ecc8653543", "Job Number");
		}

		protected override ZString GetJobNumber()
		{
			return containerBO.JC_ContainerJobID;
		}

		protected override RouteWrapperCollection GetShipmentRoutes()
		{
			return new RouteWrapperCollection(containerBO, Factory);
		}

		protected override ContainerWrapperCollection GetContainers()
		{
			return new ContainerWrapperCollection(containerBO, Factory);
		}

		protected override PackageWrapperCollection GetPackages()
		{
			var packageWrapperCollection = new PackageWrapperCollection(containerBO, Factory);

			packageWrapperCollection.Sort<PackageWrapperFromFreightPackage>((a, b)
				=> a.Parent.CFSShipment.JS_UniqueConsignRef.CompareTo(b.Parent.CFSShipment.JS_UniqueConsignRef));

			return packageWrapperCollection;
		}
	}
}
