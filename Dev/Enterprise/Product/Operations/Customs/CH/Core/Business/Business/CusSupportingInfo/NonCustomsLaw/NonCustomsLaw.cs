using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class NonCustomsLaw : CusSupportingInfo
{
	public new class Schema : Customs.Business.CusSupportingInfo.Schema
	{
		public const int CodeMaxLength = 3;
		public const string CodeDescription = "CodeDescription";
	}

	public NonCustomsLaw(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

	public new NonCustomsLawLookups Lookups => (NonCustomsLawLookups)base.Lookups;

	public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

	protected override CusSupportingInfoLookups GetNewLookups() => new NonCustomsLawLookups(this);

	protected override CusSupportingInfoValidation GetNewValidation() => new NonCustomsLawValidation(this);

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CSI_Type = Common.CH.CusSupportingInfoTypeList.Codes.NonCustomsLaw;
	}

	#region Properties

	[ResourceStringData("Enterprise.Customs.CH.Business.NonCustomsLaw|CSI_Code", Caption = "Type")]
	[MaxLength(Schema.CodeMaxLength)]
	public override ZString CSI_Code { get => base.CSI_Code; set => base.CSI_Code = value; }

	[ResourceStringData("Enterprise.Customs.CH.Business.NonCustomsLaw|CodeDescription", Caption = "Description")]
	public ZString CodeDescription
	{
		get => (Lookups.CodeList as ICodeDescriptionPairList).GetDescriptionFromCode(CSI_Code);
	}

	public ZPropertyInfo CodeDescriptionInfo
	{
		get { return GetZPropertyInfo(Schema.CodeDescription); }
	}

	#endregion
}
