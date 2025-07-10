using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class NveCusCodeData : CusCodeData
	{
		public NveCusCodeData(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : CusCodeData.Schema
		{
			public const string Position = "Position";
			public const string Specification = "Specification";
		}

		public override bool SupportsNotes => false;

		public override bool ReadOnly => base.ReadOnly || (Parent is JobComInvoiceLine parent && parent.HasLinkedInvoiceLine);

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobComInvoiceLine), typeof(CusClassPartPivot));

		#region Type Safe

		protected override CusCodeDataValidation GetNewValidation() => new NveCusCodeDataValidation(this);

		public new NveCusCodeDataValidation Validation => (NveCusCodeDataValidation)base.Validation;

		public new NveCusCodeDataLookups Lookups => (NveCusCodeDataLookups)base.Lookups;

		protected override CusCodeDataLookups GetNewLookups() => new NveCusCodeDataLookups(this);

		#endregion

		#region Properties

		RefCusTariffBRCharacteristic tariffCharacteristic;
		public RefCusTariffBRCharacteristic TariffCharacteristic
		{
			get => tariffCharacteristic;
			set
			{
				tariffCharacteristic = value;
				CY_Order = MapPosition(tariffCharacteristic?.NomenclatureGroup?.ZZ5_Value.Length ?? tariffCharacteristic?.Tariff?.ZZ1_TariffCode.Length ?? 0);
			}
		}

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.BR.Business.NveCusCodeData|CY_Code", Caption = "Attribute")]
		public override ZString CY_Code { get => base.CY_Code; set => base.CY_Code = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.NveCusCodeData|CY_Order", Caption = "Position")]
		public ZString Position => Lookups.PositionList.GetDescriptionFromCode(CY_Order.ToString());

		public ZPropertyInfo PositionInfo => GetZPropertyInfo(Schema.Position);

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(NveCusCodeDataLookups.SpecificationList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.NveCusCodeData|Specification", Caption = "Specification")]
		public ZString Specification
		{
			get
			{
				return Lookups.SpecificationList.GetDescriptionFromCode(CY_Data);
			}
			set
			{
				CY_Data = Lookups.SpecificationList.GetCodeFromDescription(value);
				SpecificationInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SpecificationInfo => GetWrappedZPropertyInfo(Schema.Specification, x => CY_DataInfo);

		[ResourceStringData("Enterprise.Customs.BR.Business.NveCusCodeData|Attribute", Caption = "Attribute Text")]
		public ZString Attribute => TariffCharacteristic?.ZB1_Text ?? ZString.Empty;

		public override ZString CY_Data
		{
			get => base.CY_Data;
			set
			{
				base.CY_Data = value;
				SpecificationInfo.RefreshBinding();
			}
		}

		#endregion

		public void CopyValuesIfEntered(NveCusCodeData nve)
		{
			if (!nve.CY_Data.IsEmpty)
			{
				CY_Data = nve.CY_Data;
			}
		}

		ZShort MapPosition(int length)
		{
			switch (length)
			{
				case 2:
					return new ZShort(PositionList.Codes.Chapter);
				case 4:
					return new ZShort(PositionList.Codes.Position);
				case 5:
					return new ZShort(PositionList.Codes.SubPositionLevel1);
				case 6:
					return new ZShort(PositionList.Codes.SubPositionLevel2);
				case 7:
					return new ZShort(PositionList.Codes.Item);
				default:
					return new ZShort(PositionList.Codes.SubItem);
			}
		}
	}
}
