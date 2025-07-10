using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AsycudaTransportMeansCollection : DependentBusinessObjectCollection<AsycudaTransportMeans, BusinessObject>
	{
		public AsycudaTransportMeansCollection(BusinessObject parent)
			: base(parent)
		{
			MaxCountValidationEnable(99);
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => CusTransportMeansSchema.TPM_ParentID;
	}
}
