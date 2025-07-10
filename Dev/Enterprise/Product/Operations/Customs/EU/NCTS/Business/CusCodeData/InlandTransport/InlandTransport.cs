using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;

namespace Enterprise.Customs.EU.NCTS.Business
{
	[SingleObjectAroundARow]
	[DependentBusinessObject(typeof(NctsDepartureMovementHeader), nameof(NctsDepartureMovementHeader.InlandTransportList))]
	public class InlandTransport : CusCodeData, IAdditionalWagonProvider
	{
		public InlandTransport(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => Res.GetString("7FCF1DC7-A870-406E-B8FB-093856E20145", "Inland Transport");

		[ResourceStringData("9871761A-7895-4FB8-84CF-0834ABA5897B", Caption = "Nationality")]
		[List(nameof(Lookups) + "." + nameof(InlandTransportLookups.Countries))]
		public override ZString CY_Code
		{
			get => base.CY_Code;
			set => base.CY_Code = value;
		}

		[List(nameof(Lookups) + "." + nameof(InlandTransportLookups.TransportNationalityList))]
		public ZString WagonNationality
		{
			get => CY_Code;
			set => CY_Code = value;
		}

		public ZWrappedPropertyInfo WagonNationalityInfo => GetWrappedZPropertyInfo(nameof(WagonNationality), x => CY_CodeInfo);

		[MaxLength(35)]
		[ResourceStringData("0AC561FE-538D-4012-B178-CA42E3F831E4", Caption = "Wagon Number", MediumCaption = "Wagon No.")]
		public override ZString CY_Data
		{
			get => base.CY_Data;
			set => base.CY_Data = value;
		}

		public ZString WagonNumber
		{
			get => CY_Data;
			set => CY_Data = value;
		}

		public ZWrappedPropertyInfo WagonNumberInfo => GetWrappedZPropertyInfo(nameof(WagonNumber), x => CY_DataInfo);

		public new InlandTransportLookups Lookups => (InlandTransportLookups)base.Lookups;

		public new InlandTransportValidation Validation => (InlandTransportValidation)base.Validation;

		protected override Customs.Business.CusCodeDataLookups GetNewLookups() => new InlandTransportLookups(this);

		protected override CusCodeDataValidation GetNewValidation() => new InlandTransportValidation(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = NctsConstants.CusCodeDataTypes.TransportInland;
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(NctsDepartureMovementHeader));
	}
}
