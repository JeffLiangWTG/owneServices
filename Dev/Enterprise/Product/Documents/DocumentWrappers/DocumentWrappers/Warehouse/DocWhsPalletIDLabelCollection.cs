using CargoWise.EntityFramework;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsPalletIDLabelCollection : DocWhsLabelCollection
	{
		#region Constructors

		public DocWhsPalletIDLabelCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		#region New

		public new DocWhsPalletIDLabel this[int index]
		{
			get { return (DocWhsPalletIDLabel)base[index]; }
		}

		#endregion
	}
}
