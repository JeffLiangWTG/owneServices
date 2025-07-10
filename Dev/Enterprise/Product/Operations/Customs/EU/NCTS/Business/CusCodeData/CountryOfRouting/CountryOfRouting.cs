using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CountryOfRouting : CusCodeData, IShortSequenceNumberLine
	{
		public CountryOfRouting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CountryOfRoutingLookups Lookups => (CountryOfRoutingLookups)base.Lookups;

		public new CountryOfRoutingValidation Validation => (CountryOfRoutingValidation)base.Validation;

		protected override CusCodeDataValidation GetNewValidation() => new CountryOfRoutingValidation(this);

		public ICountryOfRoutingValidationDecider ValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, GetValidationDecider);
		CachedValue<ICountryOfRoutingValidationDecider> validationDeciderCached;

		ICountryOfRoutingValidationDecider GetValidationDecider() => Parent?.Configuration.CountryOfRoutingConfiguration.GetValidationDecider(Parent);

		protected override ZString HumanReadableNameCore => Res.GetString("0E2FB2E9-262D-4C23-A309-91178DE6E0D2", "Country/Region of Routing");

		protected override ZString HumanReadableNameForPluralCore => Res.GetString("E40371EB-D123-4FE8-9A9C-0D3EE322329C", "Countries/Regions of Routing");

		[ReadOnly(true)]
		[MaxLength(2)]
		[ResourceStringData("576DECBF-0904-45B0-B349-44E9BD07530C", Caption = "Sequence", ShortCaption = "Seq.")]
		public override ZShort CY_Order
		{
			get => base.CY_Order;
			set => base.CY_Order = value;
		}

		[ResourceStringData("EE870978-9E53-4C00-A1BC-0238177B59C0", Caption = "Country/Region", ShortCaption = "Ctry./Rgn.")]
		[List(nameof(Lookups) + "." + nameof(CountryOfRoutingLookups.CountryList))]
		public override ZString CY_Data
		{
			get => base.CY_Data;
			set => base.CY_Data = value;
		}

		[ResourceStringData("5F9738F8-FFEA-40B8-95B0-DCA1FD4E1464", Caption = "Country/Region Description", MediumCaption = "Ctry./Rgn. Description", ShortCaption = "Ctry./Rgn. Desc.")]
		public override ZString Description => Factory.GetCachedCountryNC008List(DefaultDataGroupingCode).GetDescriptionFromCode(CY_Data);

		public override ZString CY_ParentTableCode
		{
			get => base.CY_ParentTableCode;
			set
			{
				var oldvalue = base.CY_ParentTableCode;
				base.CY_ParentTableCode = value;
				if (!IsCopying && oldvalue != value && !CY_ParentTableCode.IsEmpty)
				{
					Parent?.CountryOfRoutingLineNumberGenerator.RecalculateWhenAdded(this);
				}
			}
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(NctsHeader));

		protected override Customs.Business.CusCodeDataLookups GetNewLookups() => new CountryOfRoutingLookups(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.CountryOfRouting;
			CY_Code = CusCodeDataTypeList.Codes.CountryOfRouting;
		}

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber { get => CY_Order; set => CY_Order = value; }

		ZGuid ISequenceNumberLine.FKToHeader => CY_ParentID;

		new NctsHeader Parent => (NctsHeader)base.Parent;

		public ZString DefaultDataGroupingCode => Parent?.DefaultDataGroupingCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
	}
}
