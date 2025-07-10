using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class G3EDIMessage : EDIMessage
	{
		public G3EDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZInt BillsCount => Header?.Bills.Count ?? ZInt.Zero;

		public AsycudaManifestHeader Header => EM_LinkedObject as AsycudaManifestHeader;
	}
}
