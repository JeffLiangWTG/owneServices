using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsLabelCollection : DocumentWrapperCollection
	{
		#region Constructors

		public DocWhsLabelCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		#region New

		public new DocWhsLabel this[int index]
		{
			get { return (DocWhsLabel)base[index]; }
		}

		#endregion
	}
}
