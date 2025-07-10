using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;

namespace Enterprise.Customs.FR.GUI.UCC6TemporaryStorage
{
	public class UCC6TemporaryStorageForm : EU.TemporaryStorage.GUI.TemporaryStorageForm
	{
		public UCC6TemporaryStorageForm(TemporaryStorageHeader header) : base(header)
		{
		}

		protected override ResourceStringData BillsTabPageCaption => Header.AMA_ManifestType == FRConstants.TemporaryStorage.AppCodeLAD ? Res.GetData("C6284427-9636-4964-94E6-344B2F47A9AE", "Bills(LADT)") : Res.GetData("D7193CD9-787A-4163-BDAF-3254440808B8", "Bills(IST)");
	}
}
