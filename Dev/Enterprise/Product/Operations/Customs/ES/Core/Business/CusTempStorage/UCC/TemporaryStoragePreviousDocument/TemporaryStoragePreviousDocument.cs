using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.ES.Business.CusTempStorage
{
	public class TemporaryStoragePreviousDocument : EU.Business.CusTempStorage.TemporaryStoragePreviousDocument
	{
		public TemporaryStoragePreviousDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("67D41172-1044-4572-ADDE-4D86F4565C27", Caption = "Flight Number", MediumCaption = "Flight Number", ShortCaption = "Flight Number", FullDescription = "Number of Flight")]
		public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }
	}
}
