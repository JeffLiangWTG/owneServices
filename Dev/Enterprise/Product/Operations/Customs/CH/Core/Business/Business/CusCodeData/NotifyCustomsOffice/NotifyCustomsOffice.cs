using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CH.Business;

public class NotifyCustomsOffice : CusCodeData
{
	public NotifyCustomsOffice(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : CusCodeData.Schema
	{
		public new const int CY_DataMaxLength = 8;
		public const string DataDescription = "DataDescription";
	}

	protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobComInvoiceLine));

	public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

	protected override CusCodeDataLookups GetNewLookups() => new NotifyCustomsOfficeLookups(this);

	public new NotifyCustomsOfficeLookups Lookups => (NotifyCustomsOfficeLookups)base.Lookups;

	protected override CusCodeDataValidation GetNewValidation() => new NotifyCustomsOfficeValidation(this);

	public new NotifyCustomsOfficeValidation Validation => (NotifyCustomsOfficeValidation)base.Validation;

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CY_ParentTableCode = ZArchitecture.Schema.JobComInvoiceLineSchema.Constants.Prefix;
		CY_Type = CusCodeDataTypeList.Codes.NotifyCustomsOffice;
	}

	public override bool SupportsNotes => false;

	protected override ZString HumanReadableNameCore => Res.GetString("Enterprise.Customs.CH.Business.NotifyCustomsOffice|HumanReadableNameCore", "Notify Customs Office");

	#region Properties
	public ZDateTime EffectiveAssessmentDate => Parent?.EffectiveAssessmentDate ?? ZDate.Today;

	[List(nameof(Lookups) + "." + nameof(NotifyCustomsOfficeLookups.NotifyCustomsOfficeList))]
	[ResourceStringData("Enterprise.Customs.CH.Business.NotifyCustomsOffice|CY_Data", Caption = "Customs Office")]
	[MaxLength(Schema.CY_DataMaxLength)]
	public override ZString CY_Data { get => base.CY_Data; set => base.CY_Data = value; }

	[ResourceStringData("Enterprise.Customs.CH.Business.NotifyCustomsOffice|DataDescription", Caption = "Description")]
	public ZString DataDescription => ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, CY_Data, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.CustomsOffice, EffectiveAssessmentDate)?.ZZD_Description ?? ZString.Empty;

	public ZPropertyInfo DataDescriptionInfo => GetZPropertyInfo(nameof(DataDescription));
	#endregion
}
