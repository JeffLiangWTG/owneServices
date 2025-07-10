using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.CusTempStorage
{
	public class TemporaryStorageAdditionalInfo : EU.Business.CusTempStorage.TemporaryStorageAdditionalInfo
	{
		public TemporaryStorageAdditionalInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new TemporaryStorageAdditionalInfoLookups Lookups => (TemporaryStorageAdditionalInfoLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups()
		{
			return new TemporaryStorageAdditionalInfoLookups(this);
		}

		protected override CusSupportingInfoValidation GetNewValidation() => new TemporaryStorageAdditionalInfoValidation(this);

		[ResourceStringData("IE.TemporaryStorageAdditionalInfo|CSI_Code", Caption = "Code")]
		public override ZString CSI_Code { get => base.CSI_Code; set => base.CSI_Code = value; }

		[ResourceStringData("IE.TemporaryStorageAdditionalInfo|CSI_Description", Caption = "Description")]
		public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value; }
	}
}
