using CargoWise.EntityFramework;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsPackageLabelCollection : DocWhsLabelCollection
	{
		#region Constructors

		public DocWhsPackageLabelCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		#region New

		public new DocWhsPackageLabel this[int index]
		{
			get { return (DocWhsPackageLabel)base[index]; }
		}

		#endregion
	}
}
