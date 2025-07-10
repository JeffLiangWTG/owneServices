using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.EU.Business.Declaration
{
	[SingleObjectAroundARow]
	[DependentBusinessObject(typeof(JobDeclaration), nameof(JobDeclaration.InlandTransports))]
	[SystemDefinedValues]
	public class InlandTransport : CusCodeData, IShortSequenceNumberLine
	{
		public InlandTransport(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : CusCodeData.Schema
		{
			public const string Nationality = nameof(InlandTransport.Nationality);
		}

		public new JobDeclaration Parent => (JobDeclaration)base.Parent;

		[MaxLength(2)]
		[ResourceStringData("5454F7B1-5A8A-4E40-AE3C-A59173C6780B", Caption = "Type of ID")]
		public override ZString CY_Code
		{
			get => base.CY_Code;
			set => base.CY_Code = value;
		}

		[MaxLength(35)]
		[ResourceStringData("9D922D07-1243-493E-94AE-70E6264F4CC4", Caption = "Transport ID")]
		public override ZString CY_Data
		{
			get => base.CY_Data;
			set => base.CY_Data = value;
		}

		public override ZShort CY_Order
		{
			get { return base.CY_Order; }
			set
			{
				if (value > 0)
				{
					var oldValue = CY_Order;

					base.CY_Order = value;

					if (!IsCopying && Parent != null)
					{
						Parent.InlandTransportLineNumberGenerator.RecalculateWhenRenumbered(this, oldValue);
					}
				}
			}
		}

		[ResourceStringData("08AAA564-71B8-42AD-BB31-7718344BC362", Caption = "Nationality")]
		[List(nameof(Lookups) + "." + nameof(InlandTransportLookups.TransportCountryList))]
		[MaxLength(2)]
		public ZString Nationality
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.Nationality); }
			set
			{
				ZString oldValue = Nationality;
				CheckMaximumLength(NationalityInfo, value);
				if (oldValue != value)
				{
					this.SetSystemDefinedValue(Schema.Nationality, AddOnColumnDataType.Codes.String, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateNationality();
					}
				}
				NationalityInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo NationalityInfo => GetZPropertyInfo(nameof(Nationality));

		public new InlandTransportLookups Lookups => (InlandTransportLookups)base.Lookups;

		public new InlandTransportValidation Validation => (InlandTransportValidation)base.Validation;

		protected override CusCodeDataLookups GetNewLookups() => new InlandTransportLookups(this);

		protected override CusCodeDataValidation GetNewValidation() => new InlandTransportValidation(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.TransportInland;
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobDeclaration));

		protected override ZString HumanReadableNameCore => Res.GetString("F2480A2A-65FA-488D-ABD2-657B69DFFAD2", "Inland Transport");

		#region IShortSequenceNumberLine

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber
		{
			get => CY_Order;
			set => CY_Order = value;
		}
		ZGuid ISequenceNumberLine.FKToHeader => CY_ParentID;

		#endregion

	}
}
