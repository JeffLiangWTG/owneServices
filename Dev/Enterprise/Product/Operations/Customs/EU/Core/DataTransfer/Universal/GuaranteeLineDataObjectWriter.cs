using CargoWise.Common;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public class GuaranteeLineDataObjectWriter : DataObjectWriter<CommonGuarantee, UniversalCustoms.Guarantee>
	{
		public GuaranteeLineDataObjectWriter(IDataWritingManager manager, Customs.DataTransfer.Universal.UniversalDataObjectWriterHelper helper)
			: base(manager)
		{
			this.helper = Argument.NotNull(helper, "UniversalDataObjectWriterHelper helper");
		}

		protected readonly Customs.DataTransfer.Universal.UniversalDataObjectWriterHelper helper;

		protected sealed override UniversalCustoms.Guarantee PopulateDataObject(CommonGuarantee guaranteeLineBO)
		{
			var guaranteeLineData = new UniversalCustoms.Guarantee()
			{
				ActivityCode = new CodeDescriptionPair() { Code = guaranteeLineBO.PW_ActivityCode },
				BondType = new CodeDescriptionPair1Char() { Code = guaranteeLineBO.PW_BondType },
				BondFiledPort = new CodeDescriptionPair8Char() { Code = guaranteeLineBO.PW_BondFiledPort },
				BondNumber = guaranteeLineBO.PW_BondNumber,
				BondNumber2 = guaranteeLineBO.PW_BondNumber2,
				SuretyCode = guaranteeLineBO.PW_SuretyCode,
				AccessCode = guaranteeLineBO.PW_Password,
				BondAmount = guaranteeLineBO.PW_BondAmount,
				HolderIdentification = guaranteeLineBO.PW_HolderIdentification,
				BondCurrency = new Currency() { Code = guaranteeLineBO.PW_RX_NKCurrency },
				CountryOfIssue = new Country() { Code = guaranteeLineBO.PW_RN_NKCountryOfIssue },
				ValidityLimitation = guaranteeLineBO.PW_ValidityLimitation
			};

			return guaranteeLineData;
		}
	}
}
