using System;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.KR.Business
{
	public class FTAMessageSendingObject : JobDeclarationMiscMessageSendingObjectCore
	{
		public FTAMessageSendingObject(CusEntryHeader entry, ZString messageType)
			: base(entry, messageType)
		{
			declaration = entry.Declaration;
			invoice = entry.RandomHeader;
		}
		readonly JobDeclaration declaration;
		readonly JobComInvoiceHeader invoice;

		[ResourceStringData("CCE158B2-AA76-4DD0-93C1-41CB952E5B47", Caption = "Law Code")]
		[List(nameof(Lookups) + "." + nameof(FTAMessageSendingObjectLookups.FTARelationArticleCodeList))]
		public ZString LawCode => Header.EntryInstruction?.CEI_FTARelationArticleCode ?? ZString.Empty;
		[ResourceStringData("5346D1CB-C80B-435C-936A-97A9C612698D", Caption = "Departure Date")]
		public ZDateTime DepartureDate => declaration.JE_ExportDate;
		[ResourceStringData("034C3E5C-5D13-45F6-87D6-A60619135355", Caption = "Departure Port")]
		public ZString DeparturePort => declaration.PortOfLoading?.Description ?? ZString.Empty;
		[ResourceStringData("A6B51B0D-DA35-4BF6-A204-639A656F6FF0", Caption = "Departure Port")]
		[List(nameof(Lookups) + "." + nameof(FTAMessageSendingObjectLookups.PortOfLoadings))]
		public ZString DeparturePortOfLoading => declaration.JE_RL_NKPortOfLoading;
		[ResourceStringData("2B7ED364-FAB1-40DE-8D33-91E8238CD060", Caption = "Customs Disbursement Bill #")]
		public ZString CustomsDisbursementBill
		{
			get
			{
				var result = ZString.Empty;
				var instruction = Header.EntryInstruction;
				if (instruction != null)
				{
					result = MessageFunctions.GetFormattedNumber(instruction.CEI_StatementNumber5WN, new int[] { 0, 3, 5, 7, 8, 14 });
				}
				return result;
			}
		}

		[ResourceStringData("C08AA764-5EA0-4E14-9481-5524CAD2CF89", Caption = "Manufacturer")]
		public ZString ManufacturCompanyName => invoice.ManufacturerAddress?.CompanyName ?? ZString.Empty;
		[ResourceStringData("277DBAE1-1CFC-4767-8B50-D71C40A51EEA", Caption = "Manufacturer Area Address")]
		public ZString ManufacturAddress => invoice.ManufacturerAddress?.GetAddressDetails() ?? ZString.Empty;
		[ResourceStringData("B048AD85-67AF-4F0B-AA87-64F86B86001F", Caption = "Manufacturer Area Post Code")]
		public ZString ManufacturPostCode => invoice.ManufacturerAddress?.Postcode ?? ZString.Empty;
		[ResourceStringData("C25063A3-11B0-4D3E-BAC5-44394A976DFB", Caption = "Departure Country")]
		[List(nameof(Lookups) + "." + nameof(FTAMessageSendingObjectLookups.Countries))]
		public ZString DepartureCountry => declaration.JE_RL_NKPortOfLoading.SubstringSafe(0, 2);
		[ResourceStringData("03C7AA51-DD85-46A9-96AD-E7DCFE71FC62", Caption = "Transshipment YN")]
		[List(nameof(Lookups) + "." + nameof(FTAMessageSendingObjectLookups.YNCodeList))]
		public ZString TransshipmentYN => declaration.TransshipmentYN;
		[ResourceStringData("E0ABCFC7-4031-4E1F-9414-C9B8EA22515F", Caption = "Transshipment Date")]
		public ZDateTime TransshipmentDate => declaration.JE_TransshipmentDate;
		[ResourceStringData("44DF03D8-90DC-4C93-8EC9-AE8D347C7B37", Caption = "Transshipment Country")]
		[List(nameof(Lookups) + "." + nameof(FTAMessageSendingObjectLookups.Countries))]
		public ZString TransshipmentCountry => declaration.TransshipmentCountryCode;
		[ResourceStringData("34659103-3FEC-4EB6-8D6E-8117D7D06005", Caption = "Transshipment Port")]
		[List(nameof(Lookups) + "." + nameof(FTAMessageSendingObjectLookups.PortOfLoadings))]
		public ZString TransshipmentPort => declaration.JE_TransshipmentPort;
		[ResourceStringData("679B291B-C5F9-4CED-9E95-C99F62C32AEA", Caption = "Importer")]
		public ZString ImporterCompanyName => declaration.ImporterAddress?.CompanyName ?? ZString.Empty;
		[ResourceStringData("87443D62-AA51-4021-80E2-C098FF670333", Caption = "Exporter")]
		public ZString ExporterCompanyName => invoice.Supplier?.OH_FullName ?? ZString.Empty;

		[ResourceStringData("9A3F0110-4AFC-42AC-91C5-FBA747C18F3D", Caption = "Manufacturer")]
		[List(nameof(Lookups) + "." + nameof(FTAMessageSendingObjectLookups.ConsignorList))]
		public ZGuid Manufacturer => invoice.JZ_OA_ManufacturerAddress;
		public ZAddress Manufacturer_ZAddress
		{
			get
			{
				if (manufacturer_ZAddress == null)
				{
					manufacturer_ZAddress = GetNewManufacturer_ZAddress();
				}
				return manufacturer_ZAddress;
			}
		}
		ZAddress manufacturer_ZAddress;
		public ZPropertyInfo ManufacturerInfo => GetZPropertyInfo(nameof(Manufacturer));
		ZAddress GetNewManufacturer_ZAddress() => new ZAddress(ManufacturerInfo);

		[ResourceStringData("CF749B64-8A60-4657-BA0C-62CA65BDE58D", Caption = "Importer")]
		[List(nameof(Lookups) + "." + nameof(FTAMessageSendingObjectLookups.ConsigneeList))]
		public ZGuid Importer => declaration.JE_OA_ImporterAddress;
		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress Importer_ZAddress
		{
			get
			{
				if (importer_ZAddress == null)
				{
					importer_ZAddress = GetNewImporter_ZAddress();
				}
				return importer_ZAddress;
			}
		}
		ZAddress importer_ZAddress;
		public ZPropertyInfo ImporterInfo => GetZPropertyInfo(nameof(Importer));
		ZAddress GetNewImporter_ZAddress() => new ZAddress(ImporterInfo);

		[ResourceStringData("EB1CEDD5-1317-47D6-A3BE-5C79B45CF4B9", Caption = "Exporter")]
		[List(nameof(Lookups) + "." + nameof(FTAMessageSendingObjectLookups.ConsignorList))]
		public ZGuid Exporter => invoice.JZ_OH_Supplier;

		public MessageSendingEntryLineObjectCollection FTALines
		{
			get
			{
				if (ftaLines == null)
				{
					ftaLines = new MessageSendingEntryLineObjectCollection(Header);
					ftaLines.PopulateElementsFromMergedLines(x => !x.CL_FTASequenceNumber.IsEmpty, MessageType);
				}
				return ftaLines;
			}
		}
		MessageSendingEntryLineObjectCollection ftaLines;

		public MessageSendingInvoiceLineCollection FTAInvoiceLines
		{
			get
			{
				if (fTAInvoiceLines == null)
				{
					fTAInvoiceLines = new MessageSendingInvoiceLineCollection(Factory);
					fTAInvoiceLines.PopulateElementsFTA(Header);
				}
				return fTAInvoiceLines;
			}
		}
		MessageSendingInvoiceLineCollection fTAInvoiceLines;

		public override ZString AmendmentType => throw new NotImplementedException();
		public override ZString AmendmentTypeDescription => throw new NotImplementedException();
		public new FTAMessageSendingObjectLookups Lookups => new FTAMessageSendingObjectLookups(this);
		public  new FTAMessageSendingObjectValidation Validation => (FTAMessageSendingObjectValidation)base.Validation;
		protected override Customs.Business.JobDeclarationMessageSendingObjectValidation GetNewValidation() => new FTAMessageSendingObjectValidation(this);
	}
}
