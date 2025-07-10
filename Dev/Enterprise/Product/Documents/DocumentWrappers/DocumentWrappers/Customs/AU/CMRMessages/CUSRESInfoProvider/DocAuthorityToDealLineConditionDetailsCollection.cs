using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocAuthorityToDealLineConditionDetailsCollection : DocumentWrapperCollection
	{
		public DocAuthorityToDealLineConditionDetailsCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocAuthorityToDealLineConditionDetails this[int index]
		{
			get { return (DocAuthorityToDealLineConditionDetails)base[index]; }
		}
	}
}
