using System.Data;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.BE.Business.Declaration;

[DependentBusinessObject(typeof(JobDeclaration), nameof(JobDeclaration.CustomsEntryHeaders))]
public class CusEntryHeader : EU.Business.Declaration.CusEntryHeader, Integration.Customs.BE.ICusEntryHeader, ILRNGenerator
{
	public CusEntryHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : EU.Business.Declaration.CusEntryHeader.Schema
	{
		public const string ZG_ManualDeclaration = nameof(CusEntryHeader.ZG_ManualDeclaration);
	}

	public ZString MessageStatus
	{
		get => CH_Status;
		set => CH_Status = value;
	}

	public ZString LocalReferenceNumber
	{
		get => CH_BGMReference;
		set => CH_BGMReference = value;
	}

	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	public new ICusEntryLineCollection<CusEntryLine> MergedLines => (ICusEntryLineCollection<CusEntryLine>)base.MergedLines;
	protected override ICusEntryLineCollection<Customs.Business.CusEntryLine> GetMergedLineCollection() => new CusEntryLineCollection<CusEntryLine>(this);

	public ZBool ZG_ManualDeclaration => EntryInstruction?.ZG_ManualDeclaration ?? false;

	public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

	public override bool HasBeenLodgedAtCustoms => base.HasBeenLodgedAtCustoms && !(MovementReferenceNumber.IsEmpty && (MessageStatus.IsEmpty || MessageStatus == EDIMessage.Status.Rejected));

	public override ZBool ShouldSetUCRinBGMReferenceNumber => false;

	public bool IsWaitingForResponseOrHasBeenLodgedAtCustoms => IsWaitingForResponse || HasBeenLodgedAtCustoms;

	public ZString DeclarantEoriOfMainOfficeRegistrationNumber => RepresentativeOrDeclarantEoriOfMainOffice.SubstringSafe(2);

	public override ZString DefaultStatusDescription => ZString.Empty;

	protected override bool IsMrnEntryNumberTheOneWeWantToShow => true;

	protected override void OnFactorySaving()
	{
		base.OnFactorySaving();
		GenerateAndSetLocalReferenceNumberIfNeeded();
	}

	bool IsCurrentLocalReferenceNumberGeneratedForCurrentDeclarantEORI => Regex.IsMatch(LocalReferenceNumber, string.Format("^{0}{1}{2}$", "\\d{2}", DeclarantEoriOfMainOfficeRegistrationNumber, "\\d{" + (20 - DeclarantEoriOfMainOfficeRegistrationNumber.Length).ToString() + "}"));

	void GenerateAndSetLocalReferenceNumberIfNeeded()
	{
		if (Declaration != null && !HasBeenLodgedAtCustoms && !IsWaitingForResponse && !DeclarantEoriOfMainOfficeRegistrationNumber.IsEmpty && (CH_BGMReference.IsEmpty || !IsCurrentLocalReferenceNumberGeneratedForCurrentDeclarantEORI))
		{
			CH_BGMReference = LRNGeneratorHelper.GenerateLocalReferenceNumber(this);
		}
	}

	public void SetAllEntryLinesReadOnly()
	{
		((AllCusEntryLineCollection<CusEntryLine>)AllEntryLines).SetReadOnlyIncludingChildren(!(MovementReferenceNumber.IsEmpty || ZG_ManualDeclaration));
	}

	public new IAllCusEntryLineCollection<CusEntryLine> AllEntryLines => (IAllCusEntryLineCollection<CusEntryLine>)base.AllEntryLines;

	protected override IAllCusEntryLineCollection<Customs.Business.CusEntryLine> GetAllEntryLinesCollection() => new AllCusEntryLineCollection<CusEntryLine>(this);

	public INumberFountainProxy LrnNumberFountain => Env.NumberFountains.EULocalReferenceNumber(Declaration.Company.PK.ToGuid());
}
