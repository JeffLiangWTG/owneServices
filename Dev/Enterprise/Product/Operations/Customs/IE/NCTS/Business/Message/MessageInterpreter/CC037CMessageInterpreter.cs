using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC037CMessageInterpreter : InboundMessageInterpreter<CC037CProvider>
	{
		public CC037CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, CC037CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"C77F910D-31E9-471B-A3F2-87E31758477A",
			"Response Query on Guarantee Message (IE037) has been received. NCTS has sent the response on Query on Guarantee for Job {0}.",
		relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (Res.GetString("0E03F661-349B-4927-86BF-FA1607A2090A", "Requester Identification Number"), provider.RequesterIdentificationNumber);
			yield return (Res.GetString("59BAA855-B15A-4D96-AC74-6F83BD012E90", "Requester Role"), provider.RequesterIdentificationRole);
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			foreach (var guaranteeReference in provider.GuaranteeReferences)
			{
				yield return (Res.GetString("7F1928C1-4BE1-426F-B87A-7873695FFEE0", "Guarantee Information:"), new (string, string)[]
				{
						(Res.GetString("2C6F1E78-4DE7-454D-AE98-5F7994076FD8", "Guarantee Reference Sequence Number"), guaranteeReference.SequenceNumber.ToString()),
						(Res.GetString("FE96FE94-2F4A-4789-A69C-463160913456", "Guarantee Reference GRN"), guaranteeReference.GRN),
						(Res.GetString("59AB352A-2B03-4F14-80DA-12BA92F080F2", "Guarantee Monitoring code"), guaranteeReference.GuaranteeMonitoringCode),
				});

				yield return (Res.GetString("4B14CDE1-9043-415A-AED4-2CEC92E021C3", "Guarantee Query Information:"), new (string, string)[]
				{
						(Res.GetString("20E718C4-F8F4-459E-A42F-01AF37E7A926", "Guarantee Query Identifier"), guaranteeReference.QueryIdentifier),
						(Res.GetString("82D5AD6F-C4D3-47DF-A904-6055EAE27E4E", "Guarantee Query Period From Date"), guaranteeReference.QueryPeriodFromDate.ToShortDateString()),
						(Res.GetString("F89A6968-7EF8-4E24-B181-0232DABB5219", "Guarantee Query Period To Date"), guaranteeReference.QueryPeriodToDate.ToShortDateString()),
				});

				foreach (var usage in guaranteeReference.Usages)
				{
					yield return (Res.GetString("FC584FC9-D043-4BB5-8404-7983DFE2EFF2", "Usage Information:"), new (string, string)[]
					{
						(Res.GetString("120412EA-1B2C-4CC4-9E62-49F5F9F1E26E", "Usage Sequence Number"), usage.SequenceNumber.ToString()),
						(Res.GetString("A6DEE543-3B69-45F6-A407-FED0EF12E1BA", "Usage MRN"), usage.MRN),
						(Res.GetString("C2D95CA4-FE70-436A-AA1E-F2E4F01151BE", "Usage Covered Amount"), ToAmountAndCurrency(usage.CoveredAmount, usage.Currency)),
						(Res.GetString("8B7124A5-525E-4304-85D9-132F7539CC66", "Usage Lock Date"), usage.LockDate.ToShortDateString()),
						(Res.GetString("F8752717-C12D-4CED-A959-3D3226B7CA8C", "Arrival Date & Time"), usage.ArrivalDateAndTime.ToLongTimeString()),
						(Res.GetString("7ECB0432-E2B1-4B4F-A8F0-F987D38E08CB", "Release Date"), usage.ReleaseDate.ToShortDateString()),
					});
				}

				var exposure = guaranteeReference.Exposure;
				if (exposure != null)
				{
					yield return (Res.GetString("10A50A09-E984-4184-A341-20C502B704F6", "Exposure Information:"), new (string, string)[]
					{
							(Res.GetString("51D83E6C-3923-4F5E-9632-713ECFD48E1C", "Exposure"), exposure.Exposure.ToString()),
							(Res.GetString("E63D175C-1CBF-4336-931F-8925AC59EBDF", "Exposure Counter"), exposure.ExposureCounter),
							(Res.GetString("29083168-11C9-4568-B473-DF6BEC80DF84", "Balance"), ToAmountAndCurrency(exposure.Balance, exposure.Currency)),
					});
				}

				var guarantor = guaranteeReference.Guarantor;
				if (guarantor != null)
				{
					var guarantorAddress = guarantor.Address;
					var guarantorContact = guarantor.Contact;
					yield return (Res.GetString("CDEE0239-5A4C-4B4E-A72F-BA13C50A386B", "Guarantor Information:"), new (string, string)[]
					{
						(Res.GetString("E3335C50-511D-40A5-9358-236276D04699", "Identification number"), guarantor.Id),
						(Res.GetString("45472AD0-93BC-4AC4-9D85-1EAFAFE34E94", "Name"), guarantor.Name),
						(Res.GetString("491D237A-2A3E-4E46-A4C3-4A6F8E2A02E7", "Street & Number"), guarantorAddress?.StreetAndNumber ?? ZString.Empty),
						(Res.GetString("3F143772-3514-40DC-9D0B-5209A1251DB4", "Postcode"), guarantorAddress?.Postcode ?? ZString.Empty),
						(Res.GetString("124DD02E-3A4E-4D47-9249-6D773D2B156C", "City"), guarantorAddress?.City ?? ZString.Empty),
						(Res.GetString("FDCFD76F-BCE0-4EF5-BFA2-011848CD5934", "Country"), guarantorAddress?.Country ?? ZString.Empty),
						(Res.GetString("F1B81586-5428-42ED-8A58-790D76C17343", "Contact Person Name"), guarantorContact?.Name ?? ZString.Empty),
						(Res.GetString("F5D95A07-CE8A-4213-BD90-56675F2AA9BF", "Phone Number"), guarantorContact?.PhoneNumber ?? ZString.Empty),
						(Res.GetString("5789087C-49BB-448D-B05D-4B64C63E1FF6", "Email Address"), guarantorContact?.EmailAddress ?? ZString.Empty),
					});
				}

				if (guaranteeReference.ComprehensiveGuarantee != null)
				{
					var comprehensiveGuaranteeDetails = new List<(string, string)>();
					var comprehensiveGuarantee = guaranteeReference.ComprehensiveGuarantee;
					comprehensiveGuaranteeDetails.Add((Res.GetString("BB89D3BD-8D61-4152-BDC6-C1538818EC07", "Reference Amount"), comprehensiveGuarantee.ReferenceAmount.ToString()));
					comprehensiveGuaranteeDetails.Add((Res.GetString("4EF82118-AC45-408F-96AC-C06C6B46A0C9", "Percentage of Reference Amount"), comprehensiveGuarantee.PercentageOfReferenceAmount));
					comprehensiveGuaranteeDetails.Add((Res.GetString("1DD485E7-EED9-4AA9-82FF-CBDE80522437", "Guarantee Amount"), ToAmountAndCurrency(comprehensiveGuarantee.GuaranteeAmount, comprehensiveGuarantee.Currency)));
					comprehensiveGuaranteeDetails.Add((Res.GetString("FD29AA82-91CA-4881-9999-C887247B3FF3", "No. of certificates"), comprehensiveGuarantee.NumberOfCertificates));
					comprehensiveGuaranteeDetails.Add((Res.GetString("4CA09F6D-762C-4B7F-9DA2-90E1B3106AC6", "Validity Start Date"), comprehensiveGuarantee.ValidityStartDate.ToShortDateString()));
					comprehensiveGuaranteeDetails.Add((Res.GetString("FEB22602-808E-4FFE-BD48-C56AC0252E2C", "Validity End Date"), comprehensiveGuarantee.ValidityEndDate.ToShortDateString()));
					comprehensiveGuaranteeDetails.Add((NctsCommonResStrings.InvalidityReasonCode, comprehensiveGuarantee.InvalidityReasonCode));
					comprehensiveGuaranteeDetails.Add((NctsCommonResStrings.InvalidityReasonText, comprehensiveGuarantee.InvalidityReasonText));
					comprehensiveGuaranteeDetails.Add((Res.GetString("B4ADD7CD-217A-40AA-970E-FA59D97A14F3", "Liability liberation date"), comprehensiveGuarantee.LiabilityLiberationDate.ToShortDateString()));
					comprehensiveGuaranteeDetails.Add((Res.GetString("0659798B-FA45-4671-8FFB-A18BE66212EB", "Restricted use for suspended goods"), ToYesOrNo(comprehensiveGuarantee.RestrictedUseForSuspendedGoods)));

					foreach (var validityLimitation in comprehensiveGuarantee.ValidityLimitations)
					{
						comprehensiveGuaranteeDetails.Add((Res.GetString("83A45DBF-85A1-4603-AA8F-194F79636CE7", "Validity Limitation Sequence Number"), validityLimitation.SequenceNumber.ToString()));
						comprehensiveGuaranteeDetails.Add((Res.GetString("34CF7167-88C5-4CAE-91F8-A57790600A06", "Guarantee Not Valid In"), validityLimitation.GuaranteeNotValidIn));
					}

					yield return ((string summary, IReadOnlyCollection<(string Key, string Value)>))((NoResString)"Comprehensive Guarantee Information:", comprehensiveGuaranteeDetails.AsReadOnly());
				}

				var individualGuaranteeByGuarantor = guaranteeReference.IndividualGuaranteeByGuarantor;
				if (individualGuaranteeByGuarantor != null)
				{
					yield return (Res.GetString("5C7E4605-5FF9-4A3A-A357-411550CF9052", "Individual Guarantee by Guarantor:"), new (string, string)[]
					{
						(Res.GetString("41837DDA-8BDB-4E60-BF7B-B3DA26E5EEC8", "Guarantee Amount"), ToAmountAndCurrency(individualGuaranteeByGuarantor.GuaranteeAmount, individualGuaranteeByGuarantor.Currency)),
					});
				}

				var individualGuaranteeVoucher = guaranteeReference.IndividualGuaranteeVoucher;
				if (individualGuaranteeVoucher != null)
				{
					yield return (Res.GetString("52D01B53-CEE0-4A17-9C15-A1AF5B8D61C5", "Individual Guarantee Voucher:"), new (string, string)[]
					{
						(Res.GetString("C78124BC-51C0-421A-A9C4-5C1A9DF39840", "Issue Date"), individualGuaranteeVoucher.IssueDate.ToShortDateString()),
						(Res.GetString("377EE170-ECA4-454A-AB0A-8D404CDA4287", "Expiry Date"), individualGuaranteeVoucher.ExpiryDate.ToShortDateString()),
						(Res.GetString("6356E994-1FF8-4F8F-8D4B-B6D1FAF52A98", "Copy Given"), ToYesOrNo(individualGuaranteeVoucher.CopyGiven)),
						(Res.GetString("1A488A33-E7A5-4F1C-8B16-C162211AE319", "TIR Carnet"), ToYesOrNo(individualGuaranteeVoucher.TIRCarnet)),
						(Res.GetString("5ABFCC8B-55DD-4C82-8FC7-51B159AD55E7", "Voucher Amount"),  ToAmountAndCurrency(individualGuaranteeVoucher.VoucherAmount, individualGuaranteeVoucher.Currency)),
					});
				}
			}
		}

		ZString ToAmountAndCurrency(ZDecimal amount, ZString currency) => ToAmountAndCurrency(amount.ToString(), currency);

		ZString ToAmountAndCurrency(ZString amount, ZString currency) => amount + " " + currency;

		internal string ToYesOrNo(string value)
		{
			var result = string.Empty;
			switch (value.Trim())
			{
				case "0":
					result = YesNoList.Descriptions.No;
					break;
				case "1":
					result = YesNoList.Descriptions.Yes;
					break;
				default:
					result = "";
					break;
			}
			return result;
		}
	}
}
