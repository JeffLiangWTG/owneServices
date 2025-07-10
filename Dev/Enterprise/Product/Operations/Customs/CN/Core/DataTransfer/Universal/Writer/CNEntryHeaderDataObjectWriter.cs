using System.Collections.Generic;
using System.Globalization;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using BaseCusEntryHeader = Enterprise.Customs.Business.CusEntryHeader;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.CN.DataTransfer.Universal
{
	public class CNEntryHeaderDataObjectWriter : CustomsEntryHeaderDataObjectWriter
	{
		public CNEntryHeaderDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper) : base(manager, helper)
		{
		}

		protected override CustomsEntryLineDataObjectWriter GetNewCustomsEntryLineDataObjectWriter()
		{
			return new CNEntryLineDataObjectWriter(writeManager, helper);
		}

		protected override void PopulateAddInfo(BaseCusEntryHeader entryHeaderBO, EntryHeader entryHeaderData)
		{
			base.PopulateAddInfo(entryHeaderBO, entryHeaderData);

			if (entryHeaderBO is ICustomsEntryHeader entryHeaderCN)
			{
				if (entryHeaderData.AddInfoCollection == null)
				{
					entryHeaderData.AddInfoCollection = new List<UniversalAddInfo>();
				}

				var freightFeeMarkCode = entryHeaderCN.FreightFeeMarkCode;
				var freightFeeAmount = entryHeaderCN.FreightFeeAmount;
				if (!(freightFeeMarkCode.IsEmpty || freightFeeAmount.IsEmpty))
				{
					entryHeaderData.AddInfoCollection.Add(new UniversalAddInfo { Key = Constants.AddInfoKeys.EntryHeader.FreightFeeCurrencyCode, Value = entryHeaderCN.FreightFeeCurrencyCode });
					entryHeaderData.AddInfoCollection.Add(new UniversalAddInfo { Key = Constants.AddInfoKeys.EntryHeader.FreightFeeMarkCode, Value = freightFeeMarkCode });
					entryHeaderData.AddInfoCollection.Add(new UniversalAddInfo { Key = Constants.AddInfoKeys.EntryHeader.FreightFeeAmount, Value = freightFeeAmount.ToString("f4", CultureInfo.CurrentCulture) });
				}

				var insuranceFeeMarkCode = entryHeaderCN.InsuranceFeeMarkCode;
				var insuranceFeeAmount = entryHeaderCN.InsuranceFeeAmount;
				if (!(insuranceFeeMarkCode.IsEmpty || insuranceFeeAmount.IsEmpty))
				{
					entryHeaderData.AddInfoCollection.Add(new UniversalAddInfo { Key = Constants.AddInfoKeys.EntryHeader.InsuranceFeeCurrencyCode, Value = entryHeaderCN.InsuranceFeeCurrencyCode });
					entryHeaderData.AddInfoCollection.Add(new UniversalAddInfo { Key = Constants.AddInfoKeys.EntryHeader.InsuranceFeeMarkCode, Value = insuranceFeeMarkCode });
					entryHeaderData.AddInfoCollection.Add(new UniversalAddInfo { Key = Constants.AddInfoKeys.EntryHeader.InsuranceFeeAmount, Value = insuranceFeeAmount.ToString("f4", CultureInfo.CurrentCulture) });
				}

				var otherFeeMarkCode = entryHeaderCN.OtherFeeMarkCode;
				var otherFeeAmount = entryHeaderCN.OtherFeeAmount;
				if (!(otherFeeMarkCode.IsEmpty || otherFeeAmount.IsEmpty))
				{
					entryHeaderData.AddInfoCollection.Add(new UniversalAddInfo { Key = Constants.AddInfoKeys.EntryHeader.OtherFeeCurrencyCode, Value = entryHeaderCN.OtherFeeCurrencyCode });
					entryHeaderData.AddInfoCollection.Add(new UniversalAddInfo { Key = Constants.AddInfoKeys.EntryHeader.OtherFeeMarkCode, Value = otherFeeMarkCode });
					entryHeaderData.AddInfoCollection.Add(new UniversalAddInfo { Key = Constants.AddInfoKeys.EntryHeader.OtherFeeAmount, Value = otherFeeAmount.ToString("f4", CultureInfo.CurrentCulture) });
				}

				var overseasPartyCode = entryHeaderCN.OverseasPartyCode;
				if (!overseasPartyCode.IsEmpty)
				{
					entryHeaderData.AddInfoCollection.Add(new UniversalAddInfo { Key = Constants.AddInfoKeys.EntryHeader.OverseasPartyCode, Value = overseasPartyCode });
				}

				entryHeaderData.AddInfoCollection.Add(new UniversalAddInfo { Key = Constants.AddInfoKeys.EntryHeader.MarksAndNumbers, Value = entryHeaderCN.MarksAndNumbers });
				entryHeaderData.AddInfoCollection.Add(new UniversalAddInfo { Key = Constants.AddInfoKeys.EntryHeader.BillOfLading, Value = entryHeaderCN.BillOfLading });
				entryHeaderData.AddInfoCollection.Add(new UniversalAddInfo { Key = Constants.AddInfoKeys.EntryHeader.VesselName, Value = entryHeaderCN.VesselName });
				entryHeaderData.AddInfoCollection.Add(new UniversalAddInfo { Key = Constants.AddInfoKeys.EntryHeader.Voyage, Value = entryHeaderCN.Voyage });

				if (!entryHeaderCN.Remarks.IsEmpty)
				{
					entryHeaderData.AddInfoCollection.Add(new UniversalAddInfo { Key = Constants.AddInfoKeys.EntryHeader.Remarks, Value = entryHeaderCN.Remarks });
				}
			}
		}
	}
}
