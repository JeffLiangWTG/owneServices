using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusMAWBDataObjectWriter : DataTransfer.Universal.AirManifest.CusMAWBDataObjectWriter<Customs.Business.CusMAWB, CusHAWB, AirManifestDataObjectWriterHelper>
	{
		public CusMAWBDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override AirManifestDataObjectWriterHelper CreateNewHVLVAirDataObjectWriterHelper(Customs.Business.CusMAWB mawbBO)
		{
			return new AirManifestDataObjectWriterHelper(mawbBO);
		}

		protected override DataTransfer.Universal.AirManifest.CusHAWBDataObjectWriter<CusHAWB, AirManifestDataObjectWriterHelper> GetNewCusHAWBDataObjectWriter(AirManifestDataObjectWriterHelper mawbHelper)
		{
			return new CusHAWBDataObjectWriter(writeManager, mawbHelper);
		}

		protected override bool ShouldKeepExistingData(Customs.Business.CusMAWB mawbBO)
		{
			return !HasRecipientRole(RecipientRoleType.AAD) && !HasRecipientRole(RecipientRoleType.HCA);
		}
	}
}
