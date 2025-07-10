using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class FinalPriceReportByDateExtensionHeader : AutoFinalPriceReportByDateExtensionHeader
	{
		public FinalPriceReportByDateExtensionHeader(BusinessObjectFactory factory) : base(factory)
		{
			MessageType = ElectronicDocumentTypeList.Codes._5SG;

			lines = new Dictionary<ZString, FinalPriceReportByDateExtensionLine>();
		}

		readonly Dictionary<ZString, FinalPriceReportByDateExtensionLine> lines;

		public override void Delete()
		{
			base.Delete();
			FinalPriceReportByDateExtensionLines.RemoveAndDeleteAll();
		}

		[ChildEditable(true)]
		public FinalPriceReportByDateExtensionLineCollection FinalPriceReportByDateExtensionLines
		{
			get
			{
				if (finalPriceReportByDateExtension == null)
				{
					finalPriceReportByDateExtension = new FinalPriceReportByDateExtensionLineCollection(this);
					RegisterEditableChildObject(finalPriceReportByDateExtension);
				}

				return finalPriceReportByDateExtension;
			}
		}
		FinalPriceReportByDateExtensionLineCollection finalPriceReportByDateExtension;

		public ZBool HasExtensionLines => FinalPriceReportByDateExtensionLines.Count > 0;

		public ZPropertyInfo HasExtensionLinesInfo => GetZPropertyInfo(nameof(HasExtensionLines));

		protected override FinalPriceReportByDateExtensionHeaderValidation GetNewValidation() => new FinalPriceReportByDateExtensionHeaderValidation(this);

		public FinalPriceReportByDateExtensionHeaderLookups Lookups => new FinalPriceReportByDateExtensionHeaderLookups(this);

		[List(nameof(Lookups) + "." + nameof(FinalPriceReportByDateExtensionHeaderLookups.MessageTypeList))]
		[ResourceStringData("B28B2A40-AF77-4ECB-97FA-3C071609EB5B", Caption = "Message Type")]
		public ZString MessageType { get; private set; }
		public ZPropertyInfo MessageTypeInfo => GetZPropertyInfo(nameof(MessageType));

		[List(nameof(Lookups) + "." + nameof(FinalPriceReportByDateExtensionHeaderLookups.CustomsOfficeList))]
		[ResourceStringData("493D4F6A-15ED-4EA1-8B4D-2AF120A21F3C", Caption = "Customs Office")]
		public override ZString CustomsOffice { get => base.CustomsOffice; set => base.CustomsOffice = value; }

		[List(nameof(Lookups) + "." + nameof(FinalPriceReportByDateExtensionHeaderLookups.Branches))]
		[RelatedBusinessObject(nameof(Branch))]
		[ResourceStringData("333BB7B3-5C45-488F-8CB8-138B88742D9F", Caption = "Branch Code")]
		public override ZGuid GB_Branch { get => base.GB_Branch; set => base.GB_Branch = value; }
		public GlbBranch Branch => Factory.Load<GlbBranch>(GB_Branch);

		public ZDateTime ApprovalDateFrom5SH { get; private set; }
		public ZString ResultFrom5SH { get; private set; }
		public void PopulateFromMessage(EDIMessage message5SG)
		{
			if (message5SG.EM_MessageType == ElectronicDocumentTypeList.Codes._5SG)
			{
				CargoWise.Customs.KR.MessageDefinitions.GOVCBR5SG.Declaration declaration = null;
				using (var reader = message5SG.GetEM_MessageTextReader())
				{
					declaration = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<CargoWise.Customs.KR.MessageDefinitions.GOVCBR5SG.Declaration>(reader);
				}
				foreach (var goodShipmentLine in declaration.GoodsShipment)
				{
					var lineData = FinalPriceReportByDateExtensionLines.AddNew();
					lineData.ImportDeclarationNumber = goodShipmentLine.AdditionalDocument.Id.Value;
					if (DateTime.TryParseExact(goodShipmentLine.AdditionalInformation.LimitDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
					{
						lineData.ExtensionDate = dt;
					}
					lineData.ApplicationReason = goodShipmentLine.AdditionalInformation.Content.Value;

					lines.Add(lineData.ImportDeclarationNumber, lineData);
				}
			}
		}

		public void PopulateFromIncomingMessage(EDIMessage message5SH)
		{
			if (message5SH.EM_MessageType == ElectronicDocumentTypeList.Codes._5SH)
			{
				IGOVCBR5SHMessageData messageData5SH = null;
				using (var reader = message5SH.GetEM_MessageTextReader())
				{
					messageData5SH = new GOVCBR5SHDataProvider().GetMessageData(reader);
				}

				foreach (var entryDetail in messageData5SH.EntryDetails)
				{
					FinalPriceReportByDateExtensionLine line;
					if (lines.TryGetValue(entryDetail.ImportDeclarationNumber, out line))
					{
						line.EntryReleaseDateFrom5SH = entryDetail.EntryReleaseDate;
					}
				}
				ResultFrom5SH = message5SH.EM_MessageOwner;
				ApprovalDateFrom5SH = message5SH.EM_MessageOwner == CustomsEntryStatusTypeList.Codes.DMS ? ZDateTime.Empty : messageData5SH.ApprovalDate;
			}
		}

		public void PopulateEntryData(KREntryHeaderDetailsViewCollection krEntryHeaderDetailsViews)
		{
			foreach (var entryHeader in krEntryHeaderDetailsViews)
			{
				FinalPriceReportByDateExtensionLine line;
				if (lines.TryGetValue(entryHeader.KEH_EntryNum, out line))
				{
					line.EntryDetailsView = entryHeader;
				}
			}
		}

		public bool SendWithMessageErrorsIsAllowed => Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed;
	}
}
