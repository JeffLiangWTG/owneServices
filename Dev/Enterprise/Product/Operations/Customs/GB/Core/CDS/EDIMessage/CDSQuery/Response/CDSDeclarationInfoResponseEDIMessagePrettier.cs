using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using static System.FormattableString;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSDeclarationInfoResponseEDIMessagePrettier : CDSEDIMessagePrettier<CDSDeclarationInfoResponseEDIMessage>
	{
		public CDSDeclarationInfoResponseEDIMessagePrettier(CDSDeclarationInfoResponseEDIMessage responseMessage) : base(responseMessage)
		{
			response = Message.MessageDataObject as DeclarationStatusResponse;
		}

		public override ZString MakeHumanReadable()
		{
			var interpretation = MessagePrettierCss.CSS
					+ ToH3IfNotEmpty(Invariant($"A response to a CDS Declaration Query"))
					+ GetDeclarationStatusDetails().JoinAsString();

			return interpretation;
		}

		public IEnumerable<ZString> GetDeclarationStatusDetails()
		{
			if (response.DeclarationStatusDetails.Count == 0)
			{
				yield return ToPIfNotEmpty("Please view the 'message text' tab for full details");
			}
			foreach (DeclarationStatusResponseDeclarationStatusDetails details in response.DeclarationStatusDetails)
			{
				yield return ToKeyValuePairSection(new (ZString key, ZString value)[]
					{
					("Acceptance Date Time", details.Declaration.AcceptanceDateTime.Item.ToZDateTime().ToString()),
					("ID", details.Declaration.ID.Value),
					("Version ID", details.Declaration.VersionID.Value),
					("Received Date Time", details.Declaration.ReceivedDateTime.Item.ToZDateTime().ToString()),
					("ROE", details.Declaration.ROE),
					("ICS", GetCodeAndDescription(details.Declaration.ICS, ICSList)),
					("IRC", details.Declaration.IRC),
					("Goods Released Date Time",details?.Declaration.GoodsReleasedDateTime?.Item.ToZDateTime().ToString()),
					("Function Code", details.Declaration1.FunctionCode.Value),
					("Type Code", details.Declaration1.TypeCode.Value),
					("Goods Item Quantity", details.Declaration1?.GoodsItemQuantity.Value.ToString()),
					("Total Package Quantity", details.Declaration1?.TotalPackageQuantity.Value.ToString()),
					("Submitter", details.Declaration1?.Submitter.ID.Value),
					("UCR",details.Declaration1.GoodsShipment.UCR.TraderAssignedReferenceID.Value),
					}) + GetPreviousDocumentsHtml(details.Declaration1.GoodsShipment);
			}
		}

		ZString GetPreviousDocumentsHtml(DeclarationGoodsShipment entry)
		{
			var interpretation = ZString.Empty;
			if (entry.PreviousDocument.Any())
			{
				var tableCreator = GetHtmlTableCreator();
				tableCreator.WriteRow(ToStrongIfNotEmpty("ID"), ToStrongIfNotEmpty("Type Code"));

				foreach (var doc in entry.PreviousDocument)
				{
					tableCreator.WriteRow(doc.ID.Value, doc.TypeCode.Value);
				}
				interpretation = ToUlIfNotEmpty(ToTableSection(ToStrongIfNotEmpty("Previous Documents"), tableCreator));
			}
			return interpretation;
		}

		DeclarationStatusICSList ICSList => fICSList ?? (fICSList = new DeclarationStatusICSList());
		DeclarationStatusICSList fICSList;

		readonly DeclarationStatusResponse response;
	}
}
