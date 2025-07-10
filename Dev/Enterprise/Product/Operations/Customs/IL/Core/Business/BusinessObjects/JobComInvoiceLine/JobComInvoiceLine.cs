using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.FetchStrategies;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.IL.Business
{
	public partial class JobComInvoiceLine : AutoILJobComInvoiceLine,
		ICusSupportingInfoTypeSupporter
	{
		public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("0A3C72DA-6970-4D7F-B2B5-7AB6E58F2F89", Caption = "Seq.No")]
		public override ZShort JI_LineNo { get => base.JI_LineNo; set => base.JI_LineNo = value; }

		[DecimalPlaces(3)]
		public override ZDecimal JI_CustomsQuantity { get => base.JI_CustomsQuantity; set => base.JI_CustomsQuantity = value; }

		[DecimalPlaces(3)]
		[ResourceStringData("9AFCE257-17D1-4771-9429-0943AE321D7F", Caption = "Statistical Qty")]
		public override ZDecimal JI_CustomsSecondQuantity { get => base.JI_CustomsSecondQuantity; set => base.JI_CustomsSecondQuantity = value; }

		[DecimalPlaces(3)]
		[ResourceStringData("ADC34752-C327-4C05-A52C-673E54166CDA", Caption = "Additional Qty")]
		public override ZDecimal JI_CustomsThirdQuantity { get => base.JI_CustomsThirdQuantity; set => base.JI_CustomsThirdQuantity = value; }

		[ResourceStringData("C9723544-E102-4BF6-8D37-4AE8E4B26718", Caption = "Countable Qty")]
		public override ZDecimal JI_BondedWhsQuantity { get => base.JI_BondedWhsQuantity; set => base.JI_BondedWhsQuantity = value; }

		[ResourceStringData("42769FF1-9A70-4728-BA86-3DB62FE00B0B", Caption = "VAT Code")]
		public override ZString JI_ZZF_NKTaxType { get => base.JI_ZZF_NKTaxType; set => base.JI_ZZF_NKTaxType = value; }

		[ResourceStringData("03961414-77D2-4112-856E-01552CA53444", Caption = "Preference Doc.#")]
		public override ZString JI_PreferenceDocNumber { get => base.JI_PreferenceDocNumber; set => base.JI_PreferenceDocNumber = value; }

		#region Permits

		[ChildEditable(true)]
		public PermitCollection Permits
		{
			get
			{
				if (permits == null)
				{
					permits = new PermitCollection(this);
					permits.Load();
					RegisterEditableChildObject(permits);
				}
				return permits;
			}
		}
		PermitCollection permits;

		#endregion Permits

		#region PreviousDocuments

		[ChildEditable(true)]
		public PreviousDocumentCollection PreviousDocuments
		{
			get
			{
				if (fPreviousDocuments == null)
				{
					fPreviousDocuments = new PreviousDocumentCollection(this);
					fPreviousDocuments.Load();
					RegisterEditableChildObject(fPreviousDocuments);
				}
				return fPreviousDocuments;
			}
		}
		PreviousDocumentCollection fPreviousDocuments;

		#endregion PreviousDocuments

		#region ICusSupportingInfoTypeSupporter

		IDictionary<ZString, Type> ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
		{
			return new Dictionary<ZString, Type>()
			{
				{ Common.IL.CusSupportingInfoTypeList.Codes.Permit, typeof(Permit) },
				{ Common.IL.CusSupportingInfoTypeList.Codes.PreviousDocument, typeof(PreviousDocument) },
			};
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		#endregion ICusSupportingInfoTypeSupporter

		// GetTariffDescription - to be overridden once the Tariff is setup for a new country
		protected override ZString GetTariffDescription(ZString tariffCode) => "TARIFF_DESCRIPTION";
		protected override ZString CustomsCountryCodeCore => Core.Constants.CountryCodes.Israel;
		protected override Type TypeOfPartUsedCore => typeof(OrgSupplierPart);

		public override ZString UniversalTariffType => Universal.Constants.TariffTypes.Import;

		public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;
	}
}
