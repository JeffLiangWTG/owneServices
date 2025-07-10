using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Customs.EU.Business.Documents.DocDataObjects
{
	public class EntryHeaderDataObject : DocDataObject
	{
		public CusEntryHeader EntryHeader { get; set; }

		public EntryHeaderDataObject(CusEntryHeader entry)
		{
			EntryHeader = entry;
			hasRelatedParty = hasRelatedParty2 = EntryHeader.RandomHeader.RelatedIndicator;
			hasRestrictions = EntryHeader.RandomHeader.RelatedIndicator2;
			hasSaleConditions = EntryHeader.RandomHeader.RelatedIndicator3;
			hasDisposal = EntryHeader.RandomHeader.RelatedIndicator4;
			seller = entry.Declaration.SupplierDocumentaryAddress?.Address?.AddressFullFormatted ?? ZString.Empty;
			buyer = entry.Declaration.ImporterDocumentaryAddress?.Address?.AddressFullFormatted ?? ZString.Empty;
			declarant = entry.Declaration.Declarant?.AddressFullFormatted ?? ZString.Empty;
			incoTerms = entry.Declaration.IncoTerm;
			invoiceNumber = EntryHeader.InvoiceHeaders.Length == 1 ? EntryHeader.InvoiceHeaders.First().JZ_InvoiceNumber : ZString.Empty;
			hasRoyaltyCharges = HasInvoiceLineRoyaltyCharges || HasInvoiceRoyaltyCharges || HasGroupRoyaltyCharges;
			q7C = ZBool.False;
			BuildDV1Captions();
			BuildEntryLineGroups();
		}

		#region PrintDefaultFlag

		public ZBool PrintDefaultFlag => PrintDefaultFlagCore;

		protected virtual ZBool PrintDefaultFlagCore => ZBool.True;

		#endregion

		#region Q7C

		public ZBool Q7C
		{
			get => q7C;
			set
			{
				SetNonPersistentPropertyValue(Q7CInfo, ref q7C, value);
			}
		}

		ZBool q7C;

		public ZPropertyInfo Q7CInfo => GetZPropertyInfo(nameof(Q7C));

		public ZBool PrintQ7CFlag => PrintQ7CFlagCore;

		protected virtual ZBool PrintQ7CFlagCore => ZBool.True;

		#endregion

		#region HasRelatedParty

		public ZBool HasRelatedParty
		{
			get => hasRelatedParty;
			set
			{
				SetNonPersistentPropertyValue(HasRelatedPartyInfo, ref hasRelatedParty, value);
			}
		}

		ZBool hasRelatedParty;

		public ZPropertyInfo HasRelatedPartyInfo => GetZPropertyInfo(nameof(HasRelatedParty));

		#endregion

		#region HasRelatedParty2

		public ZBool HasRelatedParty2
		{
			get => hasRelatedParty2;
			set
			{
				SetNonPersistentPropertyValue(HasRelatedParty2Info, ref hasRelatedParty2, value);
			}
		}

		ZBool hasRelatedParty2;

		public ZPropertyInfo HasRelatedParty2Info => GetZPropertyInfo(nameof(HasRelatedParty2));

		#endregion

		#region HasRestrictions
		public ZBool HasRestrictions
		{
			get => hasRestrictions;
			set
			{
				SetNonPersistentPropertyValue(HasRestrictionsInfo, ref hasRestrictions, value);
			}
		}

		ZBool hasRestrictions;

		public ZPropertyInfo HasRestrictionsInfo => GetZPropertyInfo(nameof(HasRestrictions));

		#endregion

		#region HasSaleConditions
		public ZBool HasSaleConditions
		{
			get => hasSaleConditions;
			set
			{
				SetNonPersistentPropertyValue(HasSaleConditionsInfo, ref hasSaleConditions, value);
			}
		}

		ZBool hasSaleConditions;

		public ZPropertyInfo HasSaleConditionsInfo => GetZPropertyInfo(nameof(HasSaleConditions));

		#endregion

		#region HasDisposal
		public ZBool HasDisposal
		{
			get => hasDisposal;
			set
			{
				SetNonPersistentPropertyValue(HasDisposalInfo, ref hasDisposal, value);
			}
		}

		ZBool hasDisposal;

		public ZPropertyInfo HasDisposalInfo => GetZPropertyInfo(nameof(HasDisposal));

		#endregion

		#region HasRoyaltyCharges
		public ZBool HasRoyaltyCharges
		{
			get => hasRoyaltyCharges;
			set
			{
				SetNonPersistentPropertyValue(HasRoyaltyChargesInfo, ref hasRoyaltyCharges, value);
			}
		}

		ZBool hasRoyaltyCharges;

		public ZPropertyInfo HasRoyaltyChargesInfo => GetZPropertyInfo(nameof(HasRoyaltyCharges));

		#endregion

		#region Seller
		public ZString Seller
		{
			get => seller;
			set
			{
				SetNonPersistentPropertyValue(SellerInfo, ref seller, value);
			}
		}

		ZString seller;

		public ZPropertyInfo SellerInfo => GetZPropertyInfo(nameof(Seller));

		#endregion

		#region Buyer

		public ZString Buyer
		{
			get => buyer;
			set
			{
				SetNonPersistentPropertyValue(BuyerInfo, ref buyer, value);
			}
		}

		ZString buyer;

		public ZPropertyInfo BuyerInfo => GetZPropertyInfo(nameof(Buyer));

		#endregion

		#region Declarant
		public ZString Declarant
		{
			get => declarant;
			set
			{
				SetNonPersistentPropertyValue(DeclarantInfo, ref declarant, value);
			}
		}

		ZString declarant;

		public ZPropertyInfo DeclarantInfo => GetZPropertyInfo(nameof(Declarant));

		#endregion

		#region IncoTerms
		public ZString IncoTerms
		{
			get => incoTerms;
			set
			{
				SetNonPersistentPropertyValue(IncoTermsInfo, ref incoTerms, value);
			}
		}

		protected ZString incoTerms;

		public ZPropertyInfo IncoTermsInfo => GetZPropertyInfo(nameof(IncoTerms));

		#endregion

		#region InvoiceNumber

		public ZString InvoiceNumber
		{
			get => invoiceNumber;
			set
			{
				SetNonPersistentPropertyValue(InvoiceNumberInfo, ref invoiceNumber, value);
			}
		}

		protected ZString invoiceNumber;

		public ZPropertyInfo InvoiceNumberInfo => GetZPropertyInfo(nameof(InvoiceNumber));

		#endregion

		bool HasInvoiceRoyaltyCharges => EntryHeader.InvoiceHeaders.Any(x => x.Charges.Cast<InvoiceCharge>().Any(y => y.J7_ChargeType == UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge));

		bool HasInvoiceLineRoyaltyCharges => EntryHeader.InvoiceHeaders.SelectMany(x => x.InvoiceLines).Cast<JobComInvoiceLine>().Any(x => x.Charges.Cast<InvoiceLineCharge>().Any(y => y.J7_ChargeType == UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge));

		bool HasGroupRoyaltyCharges => EntryHeader.Declaration.TopGroupInvoice.Charges.Cast<Customs.Business.BaseGroupInvoiceCharge>().Any(x => x.J7_ChargeType == UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge);

		public int NumberOfCalculationSheets => (int)Math.Ceiling((EntryHeader.AllEntryLines.Count / 3.0));

		public ZString PlaceAndDate => string.Format(CultureInfo.CurrentCulture, "{0} {1}    {2}", EntryHeader?.Declaration?.Branch?.GB_BranchName ?? ZString.Empty, ZDate.Today.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture), EntryHeader?.Declaration?.Company?.CompanyName ?? ZString.Empty);

		public ZString MRNText => MRNTextCore;

		protected virtual ZString MRNTextCore => ZString.Empty;

		public ZString PreviousCustomsDecisions => PreviousCustomsDecisionsCore;

		protected virtual ZString PreviousCustomsDecisionsCore => ZString.Empty;

		public ZString Q7cDetails => Q7cDetailsCore;

		protected virtual ZString Q7cDetailsCore => ZString.Empty;

		public ZString Q8bDetails => Q8bDetailsCore;

		protected virtual ZString Q8bDetailsCore => ZString.Empty;

		public ZString Q9bDetails => Q9bDetailsCore;

		protected virtual ZString Q9bDetailsCore => ZString.Empty;

		#region DV1 Captions & logo

		void BuildDV1Captions()
		{
			DV1EuropeanCommunityCaption = Res.GetString("A5A62CE1-C7FC-404F-9676-F4A60C199351", "EUROPEAN COMMUNITY");
			DV1DocumentTitle = Res.GetString("36329E9C-1716-4272-BE5D-9BD994A6D1B9", "DECLARATION OF PARTICULARS RELATING TO CUSTOMS VALUE D.V.1");
			DV1Box1Caption = Res.GetString("1516DFFF-4D7E-4F4A-8AB7-48F02D142261", "1  NAME AND ADDRESS OF THE SELLER");
			DV1Box2aCaption = Res.GetString("FD62BCCE-9A92-4EAC-A02E-8603C6958D9A", "2a  NAME AND ADDRESS OF THE BUYER");
			DV1Box2bCaption = Res.GetString("474155D7-7B11-45F0-B9F8-ACF429ACA4FD", "2b  NAME AND ADDRESS OF THE DECLARANT");
			DV1Box3Caption = Res.GetString("72B041F5-BE65-42AB-9E24-2C278172002B", "3 TERMS OF DELIVERY");
			DV1Box4Caption = Res.GetString("6019D7EB-810E-4577-AF71-63EF60F8B188", "4 NUMBER AND DATE OF THE INVOICE");
			DV1Box5Caption = Res.GetString("F5C463D3-FEF0-4A8D-9EA2-3233B5AD5D6D", "5 NUMBER AND DATE OF CONTRACT");
			DV1Box6Caption = Res.GetString("48349209-AFFF-49F4-861B-B85F2F12EFD8", "6 Number and date of any previous customs decision concerning boxes 7 to 9");
			DV1Box7aCaption = Res.GetString("E6C68202-2401-4E57-9E5E-AEBFDF7CBA13", "7(a) Are buyer and seller related in the sense of Article 127 of Implementing Regulation (EU) 2015/2447?");
			DV1Box7aLine2Caption = Res.GetString("EA6ABAEC-295D-4BA1-9F29-E47297EEB1F6", "(If NO go to box 8)");
			DV1Box7bCaption = Res.GetString("EAF5BB1E-08F6-496B-9F58-6A1661B121AF", " (b) Did the relationship INFLUENCE the price of the imported goods?");
			DV1Box7cCaption = Res.GetString("F2AACC6B-2507-4BAC-ABBB-D9EF12D85325", "  (c)  Does the transaction value of the imported good CLOSELY APPROXIMATE to a value mentioned in Article 134(2) of the Implementing Regulation (EU) 2015/2447 (optional reply)?");
			DV1Box7cLine2Caption = Res.GetString("7B24DF89-A22F-48CB-9A01-8819BE6042F7", "  (If YES give details)");
			DV1Box8aCaption = Res.GetString("CA1A80D0-49BD-4D7B-BD38-AEF5D9C65B65", "8(a) Are there any RESTRICTIONS as to the disposition or use of the goods, other than restrictions which:");
			DV1Box8aLine2Caption = Res.GetString("267D47D2-963E-44A7-BAE2-64D86C4ACC6E", "  - are imposed or required by law or by the public authorities in the Union: or");
			DV1Box8aLine3Caption = Res.GetString("72F72285-4C56-451E-8998-65233C3E55E6", "   - limit the geographical area in which the goods may be resold: or");
			DV1Box8aLine4Caption = Res.GetString("66BFFBDC-F9D2-492E-A470-983B5188A046", "   - do not substantially affect the value of the goods?");
			DV1Box8bCaption = Res.GetString("6BD09A42-E4BD-4AAA-83F7-6A96A197A929", "  (b) Is the sale or price subject to CONDITIONS or CONSIDERATIONS for which a value cannot be  determine with respect to the goods being valued?");
			DV1Box8bLine2Caption = Res.GetString("D730CD11-4E23-45CC-93AD-C68BE2CBB1A0", "   Specify the nature of restrictions, conditions or considerations as appropriate");
			DV1Box8bLine3Caption = Res.GetString("75F0EDAC-6A66-4B5F-97E8-D569CFF1C8B8", "If the value of conditions or considerations can be determined, indicate the amount in box 11(b)");
			DV1Box9aCaption = Res.GetString("6CC70D49-7355-419B-AACD-54166A9C17AF", "9(a) Are there any ROYALTIES and LICENCE FEES related to the imported goods payable either directly or indirectly by the buyer as a condition of sale?");
			DV1Box9bCaption = Res.GetString("9CE3F32F-B3E5-4CBD-A13E-A094460A4445", "  (b) Is the sale or price subject to an arrangement under which part of the proceeds  of any subsequent RESALE, DISPOSAL or USE of the goods accrues directly or indirectly to the seller?");
			DV1Box9bLine2Caption = Res.GetString("78FEBC9A-27CA-496E-AA22-1571ED42B90F", "  If YES to either of these questions, specify conditions and, if possible, indicate the amounts in boxes 15 and 16");
			DV1Box10aCaption = Res.GetString("AECA9041-1827-44E6-AA87-1F6ADEEA74F0", "10(a) Number of calculation sheets attached");
			DV1Box10bCaption = Res.GetString("FF6E7FBF-8CA6-4D69-91ED-92255B5DD7AE", "10(b) Place, date and signature");
			DV1ForOfficialUseCaption = Res.GetString("9FA174BB-CFF9-4698-A804-A119DB46DE95", "FOR OFFICIAL USE");
			DV1CalculationSheetNoCaption = Res.GetString("81FD5166-2C2C-419B-B9DF-A260AA8D89A4", "Calculation Sheet No");
			DV1Yes = Res.GetString("087FCDF0-F45B-45FC-8268-33DBD292E362", "YES");
			DV1No = Res.GetString("ED8E3813-47B0-490A-BA7F-8E1AC15E4E5D", "NO");
		}

		public Image DV1DocumentLogo => GetDV1LogoFromCustomsCountryCode();

		public Image GetDV1LogoFromCustomsCountryCode()
		{
			var logo = Properties.Resources.ResourceManager.GetObject(EntryHeader.CountryCode + "_DV1logo");
			if (logo != null)
			{
				return (Image)logo;
			}
			else
			{
				return Properties.Resources.Transparent;
			}
		}

		public ZString DV1EuropeanCommunityCaption { get; private set; }
		public ZString DV1DocumentTitle { get; private set; }
		public ZString DV1Box1Caption { get; private set; }
		public ZString DV1Box2aCaption { get; private set; }
		public ZString DV1Box2bCaption { get; private set; }
		public ZString DV1Box3Caption { get; private set; }
		public ZString DV1Box4Caption { get; private set; }
		public ZString DV1Box5Caption { get; private set; }
		public ZString DV1Box6Caption { get; private set; }
		public ZString DV1Box7aCaption { get; private set; }
		public ZString DV1Box7aLine2Caption { get; private set; }
		public ZString DV1Box7bCaption { get; private set; }
		public ZString DV1Box7cCaption { get; private set; }
		public ZString DV1Box7cLine2Caption { get; private set; }
		public ZString DV1Box8aCaption { get; private set; }
		public ZString DV1Box8aLine2Caption { get; private set; }
		public ZString DV1Box8aLine3Caption { get; private set; }
		public ZString DV1Box8aLine4Caption { get; private set; }
		public ZString DV1Box8bCaption { get; private set; }
		public ZString DV1Box8bLine2Caption { get; private set; }
		public ZString DV1Box8bLine3Caption { get; private set; }
		public ZString DV1Box9aCaption { get; private set; }
		public ZString DV1Box9bCaption { get; private set; }
		public ZString DV1Box9bLine2Caption { get; private set; }
		public ZString DV1Box10aCaption { get; private set; }
		public ZString DV1Box10bCaption { get; private set; }
		public ZString DV1ForOfficialUseCaption { get; private set; }
		public ZString DV1CalculationSheetNoCaption { get; private set; }
		public ZString DV1Yes { get; private set; }
		public ZString DV1No { get; private set; }

		#endregion

		void BuildEntryLineGroups()
		{
			EntryLineGroups = EntryLineGroupsCore.ToList();  // EntryLineGroups must be evaluated before rendering in form builder, otherwise the captions won't be translated to expected language, see WI00482207
		}

		public List<EntryLineGroup> EntryLineGroups { get; private set; }

		protected virtual List<EntryLineGroup> EntryLineGroupsCore
		{
			get
			{
				var entryLineGroup = new List<EntryLineGroup>();
				var entryLines = EntryHeader.AllEntryLines;
				for (int i = 0; i < NumberOfCalculationSheets; i++)
				{
					var tempEntryLines = entryLines.Skip(i * 3).Take(3);

					if (tempEntryLines.Any())
					{
						entryLineGroup.Add(new EntryLineGroup(tempEntryLines.Cast<CusEntryLine>(), i + 1));
					}
				}

				return entryLineGroup;
			}
		}

		public EntryLineDataObject GroupedEntryLineDataObj => GroupedEntryLineDataObjCore;
		protected virtual EntryLineDataObject GroupedEntryLineDataObjCore => null;
	}
}
