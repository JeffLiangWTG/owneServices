using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business.Documents.DocDataObjects;

public class EntryLineGroup
{
	public EntryLineGroup(IEnumerable<CusEntryLine> entryLines, int groupNumber)
	{
		EntryLines = entryLines;
		GroupNumber = groupNumber;
		BuildDV1Captions();
	}

	void BuildDV1Captions()
	{
		DV1ForOfficialUseCaption = Res.GetString("9FA174BB-CFF9-4698-A804-A119DB46DE95", "FOR OFFICIAL USE");
		DV1CalculationSheetNoCaption = Res.GetString("81FD5166-2C2C-419B-B9DF-A260AA8D89A4", "Calculation Sheet No");
		DV1BoxACaption = Res.GetString("913B8565-4FD6-4F8E-A8DA-699C6BC2A541", "Basis of calculation");
		DV1ItemCaption = Res.GetString("F5E543FC-F72E-4FC4-B5B3-BD2FD92FDEA0", "Item");
		DV1Box11aCaption = Res.GetString("CF49625A-514F-41EC-BC16-1DA427168311", "11(a) Price paid or payable in CURRENCY OF INVOICE");
		DV1Box11bCaption = Res.GetString("45848AC7-AEF6-4845-A80B-048DDFA9CD88", "(b) Indirect payment (see box 8(b))");
		DV1Box11cCaption = Res.GetString("52BB699A-F14C-4C77-AA48-68895B167A97", "(c) Exchange rate");
		DV1Box12Caption = Res.GetString("57DBC278-7024-43C1-9268-7AE86A9E29D9", "12 Total A in NATIONAL CURRENCY");
		DV1BoxBCaption = Res.GetString("5D7040CD-19BA-4CD3-9AC0-8D275001559F", "Additions");
		DV1Box13Caption = Res.GetString("49B023BA-9C99-4CCC-8870-80EE0B699794", "13 Costs incurred by the buyer:");
		DV1Box13aCaption = Res.GetString("BEAD20C8-794F-4E57-9C0F-19EE94941177", "(a) Commissions, except buying commission");
		DV1Box13bCaption = Res.GetString("4A846588-6685-4290-94EA-98FDFB7ECFAE", "(b) brokerage");
		DV1Box13cCaption = Res.GetString("7AF1F2AB-902D-4EAA-82C8-72C3A45FF09B", "(c) containers and packing");
		DV1Box14Caption = Res.GetString("07541206-598D-4224-93EE-057CDB60240B", "14 Goods and services supplied by the buyer free of charge or at reduced cost for use in connection with the production and sale for an export of the imported goods (the values shown represent an apportionment where appropriate):");
		DV1Box14aCaption = Res.GetString("1F358A7D-0E28-4E3E-B38F-FE75D8B17F0B", "(a) Material, components, parts and similar items incorporated in the imported goods");
		DV1Box14bCaption = Res.GetString("5A0BB9A4-A089-4647-A01C-79C0CAEEA3D0", "(b) Tools, dies, moulds and similar items used in the production of the imported goods");
		DV1Box14cCaption = Res.GetString("EE7BB308-CE76-4271-95D6-8843147C454F", "(c) Materials consumed in the production of the imported goods");
		DV1Box14dCaption = Res.GetString("69BBD42A-AA41-451D-B185-E22180EB8F53", "(D) Engineering, development, artwork, design work and plans and sketches undertaken elsewhere than in the Union and necessary for the productions of the imported goods");
		DV1Box15Caption = Res.GetString("B83B8059-3D7D-4866-8248-4F567BB156A9", "15 Royalties and license fees (see box 9(a))");
		DV1Box16Caption = Res.GetString("E7C4D6E5-7C93-4028-8226-E6E40B3A2C17", "16 Proceeds of any subsequent resale, disposal or use accruing to the seller (see box 9(b))");
		DV1Box17Caption = Res.GetString("E9BECC13-445C-4870-BBFA-7C5F37181CB7", "17 Costs of delivery to the place of introduction in the EU:");
		DV1Box17abCaption = Res.GetString("3C91D9B1-520C-4ACF-9C7D-37C46AB794E9", "(a & b) Transport, loading and handling charges");
		DV1Box17cCaption = Res.GetString("A7F768D9-997F-4233-BA44-E0E59F87AC32", "(c) Insurance");
		DV1Box18Caption = Res.GetString("1A57DD7D-78D4-40E7-87F9-2EDB7C67650F", "18 Total B in NATIONAL CURRENCY");
		DV1BoxCCaption = Res.GetString("0E5A83D1-8A23-45DF-B459-1580F92F6285", "Deductions");
		DV1Box19Caption = Res.GetString("CF805637-8540-4A7E-A4D1-A516CADE9F5B", "19 Costs of transport after introduction in the EU");
		DV1Box20Caption = Res.GetString("A4205054-7DEF-469E-BD70-424926EB376B", "20 Charges for construction, erection, assembly, maintenance or technical assistance undertaken after importation");
		DV1Box21Caption = Res.GetString("24BC943C-C22D-4848-8B80-C72ACE8A1FB0", "21 Other charges (specify)");
		DV1Box22Caption = Res.GetString("B8D8C1F8-A250-4884-A0B8-35EE2AAC1F3B", "22 Duties and taxes payable in the Union by reason of the importation or sale of the goods");
		DV1Box23Caption = Res.GetString("4C5369D2-0992-4DBF-A622-C1ED878EEE75", "23 Total C in NATIONAL CURRENCY");
		DV1Box24Caption = Res.GetString("AF7ABB5D-90C9-41D7-86A2-BE2587509850", "24 CUSTOMS VALUE DECLARED (A+B-C)");
	}

	public IEnumerable<CusEntryLine> EntryLines { get; set; }

	public int GroupNumber { get; set; }

	public EntryLineDataObject Item1 => Item1Core;
	protected virtual EntryLineDataObject Item1Core => item1 ?? (item1 = EntryLines.Any() ? new EntryLineDataObject(EntryLines.FirstOrDefault()) : null);

	public EntryLineDataObject Item2 => Item2Core;
	protected virtual EntryLineDataObject Item2Core => item2 ?? (item2 = EntryLines.Count() > 1 ? new EntryLineDataObject(EntryLines.Skip(1).FirstOrDefault()) : null);

	public EntryLineDataObject Item3 => Item3Core;
	protected virtual EntryLineDataObject Item3Core => item3 ?? (item3 = EntryLines.Count() > 2 ? new EntryLineDataObject(EntryLines.Skip(2).FirstOrDefault()) : null);

	#region DV1 captions

	public ZString DV1ForOfficialUseCaption { get; private set; }
	public ZString DV1CalculationSheetNoCaption { get; private set; }
	public ZString DV1BoxACaption { get; private set; }
	public ZString DV1ItemCaption { get; private set; }
	public ZString DV1Box11aCaption { get; private set; }
	public ZString DV1Box11bCaption { get; private set; }
	public ZString DV1Box11cCaption { get; private set; }
	public ZString DV1Box12Caption { get; private set; }
	public ZString DV1BoxBCaption { get; private set; }
	public ZString DV1Box13Caption { get; private set; }
	public ZString DV1Box13aCaption { get; private set; }
	public ZString DV1Box13bCaption { get; private set; }
	public ZString DV1Box13cCaption { get; private set; }
	public ZString DV1Box14Caption { get; private set; }
	public ZString DV1Box14aCaption { get; private set; }
	public ZString DV1Box14bCaption { get; private set; }
	public ZString DV1Box14cCaption { get; private set; }
	public ZString DV1Box14dCaption { get; private set; }
	public ZString DV1Box15Caption { get; private set; }
	public ZString DV1Box16Caption { get; private set; }
	public ZString DV1Box17Caption { get; private set; }
	public ZString DV1Box17abCaption { get; private set; }
	public ZString DV1Box17cCaption { get; private set; }
	public ZString DV1Box18Caption { get; private set; }
	public ZString DV1BoxCCaption { get; private set; }
	public ZString DV1Box19Caption { get; private set; }
	public ZString DV1Box20Caption { get; private set; }
	public ZString DV1Box21Caption { get; private set; }
	public ZString DV1Box22Caption { get; private set; }
	public ZString DV1Box23Caption { get; private set; }
	public ZString DV1Box24Caption { get; private set; }
	#endregion

	EntryLineDataObject item1;
	EntryLineDataObject item2;
	EntryLineDataObject item3;
}
