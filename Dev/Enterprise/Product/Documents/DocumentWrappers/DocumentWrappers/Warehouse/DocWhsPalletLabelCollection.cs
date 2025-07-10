using CargoWise.EntityFramework;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsPalletLabelCollection : DocWhsLabelCollection
	{
		#region Constructors

		public DocWhsPalletLabelCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		#region New

		public new DocWhsPalletLabel this[int index]
		{
			get { return (DocWhsPalletLabel)base[index]; }
		}

		#endregion
	}
}
