using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.DocumentScanning.Business
{
	public class StorageReference : AutoStorageReference, ICanBeSavedByDocumentFactory
	{
		public StorageReference(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public StorageMain StorageMain
		{
			get
			{
				return Factory.Load<StorageMain>(SR_SM);
			}
		}

		[RelatedBusinessObject("StorageMain")]
		public override ZGuid SR_SM
		{
			get { return base.SR_SM; }
			set
			{
				base.SR_SM = value;
			}
		}
	}
}
