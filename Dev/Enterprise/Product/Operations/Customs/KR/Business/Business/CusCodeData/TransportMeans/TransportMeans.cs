using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business
{
	public class TransportMeans : CusCodeData
	{
		public TransportMeans(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : CusCodeData.Schema
		{
			public const int CY_Code_MaxLength = 35;
			public const int CY_Data_MaxLength = 20;
			public const int Description_MaxLength = 9;
		}

		public override bool SupportsNotes => false;

		[MaxLength(Schema.CY_Data_MaxLength)]
		[ResourceStringData("39316242-E741-4830-84ED-12E61C821A47", Caption = "Transport Vehicle Reg No")]
		public override ZString CY_Data
		{
			get => base.CY_Data;
			set => base.CY_Data = value;
		}

		[MaxLength(Schema.CY_Code_MaxLength)]
		[List(nameof(Lookups) + "." + nameof(TransportMeansLookups.RefVessels))]
		[ResourceStringData("E2CB298A-A424-4C81-8BD1-519CD291DAA3", Caption = "Working Vessel Name")]
		public override ZString CY_Code
		{
			get => base.CY_Code;
			set
			{
				base.CY_Code = value;
				Validation.ValidateDescription();
			}
		}

		[ResourceStringData("1F4A4397-7D17-491D-A865-205078BB8A01", Caption = "Seq #")]
		[ReadOnly(true)]
		public override ZShort CY_Order
		{
			get => base.CY_Order;
			set => base.CY_Order = value;
		}

		[MaxLength(Schema.Description_MaxLength)]
		[ResourceStringData("5395B38F-3AA5-4FB0-97B0-A361AC835AF3", Caption = "Vessel ID")]
		public override ZString Description => CY_Code.IsEmpty ? ZString.Empty : RefVessel.LookupVesselByCode(CY_Code, Factory)?.RV_MalaysiaVesselId ?? ZString.Empty;

		public override bool CY_DataAllowWesternEuropeanCharactersOnly => false;
		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobDeclaration));

		public new TransportMeansValidation Validation => (TransportMeansValidation)base.Validation;

		public new TransportMeansLookups Lookups => (TransportMeansLookups)base.Lookups;

		protected override Customs.Business.CusCodeDataValidation GetNewValidation() => new TransportMeansValidation(this);

		protected override CusCodeDataLookups GetNewLookups() => new TransportMeansLookups(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.TransportMean;
		}
	}
}
