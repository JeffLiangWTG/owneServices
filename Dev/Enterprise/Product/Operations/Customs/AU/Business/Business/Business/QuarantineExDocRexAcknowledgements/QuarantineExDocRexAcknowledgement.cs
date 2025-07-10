using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineExDocRexAcknowledgement : CusCodeData
	{
		public QuarantineExDocRexAcknowledgement(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : CusCodeData.Schema
		{
			public new const int CY_DataMaxLength = 10;
		}

		#region Properties

		[MaxLength(Schema.CY_DataMaxLength)]
		[ResourceStringData("AU.QuarantineExDocRexAcknowledgement.CY_Data", Caption = "Notice ID")]
		public override ZString CY_Data
		{
			get => base.CY_Data;
			set => base.CY_Data = value;
		}

		#endregion

		protected override CusCodeDataLookups GetNewLookups() => new QuarantineExDocRexAcknowledgementLookups(this);
		public new CusCodeDataLookups Lookups => (QuarantineExDocRexAcknowledgementLookups)base.Lookups;

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(QuarantineExDocHeader));

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = AcknowledgementCode;
			CY_Code = AcknowledgementCode;
		}

		public const string AcknowledgementCode = "ACK";
	}
}
