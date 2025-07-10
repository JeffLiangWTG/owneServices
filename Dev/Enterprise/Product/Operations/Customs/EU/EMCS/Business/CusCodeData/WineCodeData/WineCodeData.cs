using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public sealed class WineCodeData : CusCodeData
	{
		public WineCodeData(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString CY_Description
		{
			get => Lookups.CY_CodeList.GetDescriptionFromCode(CY_Code);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.WineCode;
		}

		protected override CusCodeDataLookups GetNewLookups()
		{
			return new WineCodeDataLookups(this);
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(EMCSJobComInvoiceLine)); }
		}

		public override bool ReadOnly
		{
			get => base.ReadOnly || IsMessageStatusSentOrAcknowledgedOnParent;
			set => base.ReadOnly = value;
		}

		public override bool CanDelete => base.CanDelete && !IsMessageStatusSentOrAcknowledgedOnParent;

		bool IsMessageStatusSentOrAcknowledgedOnParent => ((Parent as EMCSJobComInvoiceLine)?.IsMessageStatusSentOrAcknowledgedOnParent ?? false);
	}
}
