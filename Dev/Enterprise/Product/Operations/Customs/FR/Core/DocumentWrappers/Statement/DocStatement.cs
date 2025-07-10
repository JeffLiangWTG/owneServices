using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.CusStatement;
using Enterprise.DocumentWrappers;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.FR.DocumentWrappers.Statement;

[CodeAlive("Used in documents DataContext")]
public class DocStatement : DocBaseWrapper
{
	DocStatement(CusStatementHeader cusStatementHeader, BusinessObjectFactory factoryToWrap)
		: base(cusStatementHeader, factoryToWrap)
	{ }

	public static DocStatement New(CusStatementHeader cusStatementHeader, BusinessObjectFactory factoryToWrap) => new DocStatement(cusStatementHeader, factoryToWrap);

	public CusStatementHeader CusStatementHeader => (CusStatementHeader)base.WrappedObject;

	public OrgHeader Importer => CusStatementHeader.Importer;

	#region properties
	public ZString Direction => CusStatementHeader.BranchDesignationDescription;
	public ZString PrintingPlace => GlbBranch.CurrentBranch?.HomePort?.Description ?? ZString.Empty;
	public ZString PrintingDate => ZDate.Today.ToString("dd/MM/yyyy");
	public ZString ImporterID => Importer?.GetEORI() ?? ZString.Empty;

	public ZString ImporterAddress
	{
		get
		{
			var result = new ZStringBuilder();
			if (Importer != null)
			{
				var importer = Importer;
				var address = importer.MainAddress;

				result.AppendLine(importer.OH_FullName);
				result.AppendLine(address.OA_Address1);
				if (!address.OA_Address2.IsEmpty)
				{
					result.AppendLine(address.OA_Address2);
				}
				result.AppendLine(address.OA_PostCode);
				result.Append(address.OA_City);
			}
			return result.ToString();
		}
	}

	public ZString RepresentativeID => CusStatementHeader.B2_ImporterCustomsID;

	public ZString CustomsOffice => GetCustomsOffice();

	public ZString AgreementNumber => CusStatementHeader.B2_EntryFilerCode;

	public ZString EntryNumber => CusStatementHeader.EntryNumber;

	public ZString ValidationDate => CusStatementHeader.Messages?.LastIncomingMessage?.EM_MessageDateTime.ToString("dd/MM/yyyy") ?? ZString.Empty;

	public ZString PeriodStartDate => CusStatementHeader.B2_PeriodStartDate.ToString("dd/MM/yyyy");

	public ZString PeriodEndDate => CusStatementHeader.B2_PeriodEndDate.ToString("dd/MM/yyyy");

	public ZString EntriesSummary => ZString.Join(" - ", CusStatementHeader.Entries.Cast<CusStatementLine>().Select(x => x.B3_EntryNum).Where(x => !x.IsEmpty).Distinct().ToArray());

	public ZString DefermentApprovalNumber => CusStatementHeader.B2_CheckNo;

	public DocStatementChargesCollection Charges => new DocStatementChargesCollection(CusStatementHeader.ChargesDetail.Charges, Factory);

	public ZDecimal TotalChargesAmount => GetTotalChargeAmount();

	#endregion

	ZDecimal GetTotalChargeAmount()
	{
		var charges = CusStatementHeader.ChargesDetail?.Charges.Where(x => x.B4_MethodOfPayment != FRConstants.MethodOfPayment.AI2 && x.B4_MethodOfPayment != FRConstants.MethodOfPayment._6);
		return charges != null ? charges.Cast<CusStatementLineCharge>().Sum(x => x.B4_ChargeAmount) : 0m;
	}

	ZString GetCustomsOffice()
	{
		var account = Importer?.DeltaAgreementNumberCollection.Cast<OrgCusAccount>().FirstOrDefault(a => a.CZ_Account == CusStatementHeader.B2_EntryFilerCode);
		return account != null ? account.CZ_Issuer : ZString.Empty;
	}
}
