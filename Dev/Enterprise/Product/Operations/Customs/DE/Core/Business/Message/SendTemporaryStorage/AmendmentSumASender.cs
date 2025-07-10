using System.Linq;
using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class AmendmentSumASender : TemporaryStorageSender
	{
		public AmendmentSumASender(CusTempStorageDec storageDec)
			: base(storageDec, TemporaryStorageMessageBuilderLoader.AmendmentSumA, new CUSPRLCusTempStorageDecProvider(storageDec, (line) => line.TSL_IsModified))
		{
		}

		public override (bool Can, ZString WhyCannotSend) CanSend
		{
			get
			{
				var (can, whyCannotSend) = base.CanSend;
				return (can && dataProvider.StorageLines.Any(), string.Join(System.Environment.NewLine, whyCannotSend, Res.GetString("645CBD36-5D6C-4811-B889-68B266A86202", "All Lines are already finalized or there are no changes to be sent.")));
			}
		}

		protected override string RegistrationNumber => IdentificationIdicatorIsREG ? ((ICUSPRLTempStorageDec)dataProvider).ATBNumber : string.Empty;

		protected override string MessageSubType => TemporaryStorageMessageSubTypeList.Codes.PreliminarySummaryDeclaration;
	}
}
