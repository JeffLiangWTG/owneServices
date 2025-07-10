using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC025CMessageInterpreter : InboundMessageInterpreter<CC025CProvider>
	{
		public CC025CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, CC025CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"BC6EB0FB-AB69-4986-91F1-C0DD1B53B227",
			"A Goods Release Notification (IE025) message has been received for Job {0}.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MRN);
			yield return (Res.GetString("8E437770-C64C-4EFB-BEAC-B1F66C4729C7", "Release Date"), provider.ReleaseDate.ToShortDateString());
			yield return (Res.GetString("17BAAA9F-4AB1-4B24-BEB5-22E52C08D25C", "Release Indicator"), GetCodeAndDescription(provider.ReleaseIndicator, UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL164));
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			foreach (var houseConsignment in provider.HouseConsignments)
			{
				yield return (Res.GetString("CB0E0638-342F-4214-9B31-846118E35311", "House Consignment:"), new (string, string)[]
				{
					(CommonResStrings.SequenceNumber, houseConsignment.SequenceNumber.ToString()),
					(NctsCommonResStrings.ReleaseType, GetCodeAndDescription(houseConsignment.ReleaseType, UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL163)),
				});
				foreach (var consignmentItem in houseConsignment.ConsignmentItems)
				{
					yield return (Res.GetString("3CDD8171-3E01-4B31-A065-42383FEEFBD2", "Consignment Item:"), new (string, string)[]
					{
						(Res.GetString("8E114FFA-AAA0-436A-9A19-689D1FADB5BA", "Goods Item Number"), consignmentItem.GoodsItemNumber),
						(Res.GetString("1F424B25-7C47-494C-973B-379680DE4555", "Declaration Goods Item Number"), consignmentItem.DeclarationGoodsItemNumber),
						(NctsCommonResStrings.ReleaseType, GetCodeAndDescription(consignmentItem.ReleaseType, UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL163)),
					});
				}
			}
		}
	}
}
