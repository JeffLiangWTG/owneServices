namespace Enterprise.Customs.IE.EMCS.Business
{
	public class EMCSPackageCollection : EU.EMCS.Business.EMCSPackageCollection
	{
		public EMCSPackageCollection(EMCSJobDeclaration declaration) : base(declaration)
		{
		}

		public new EMCSPackage AddNew() => (EMCSPackage)base.AddNew();

		public new EMCSPackage this[int index] => (EMCSPackage)base[index];
	}
}
