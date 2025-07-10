using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.DE.Business
{
	public class CusExitItem : EU.Business.CusExitItem, Integration.Customs.DE.ICusExitItem
	{
		public CusExitItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "This would resolve to an abstract class which is undesirable")]
		public new class Schema : EU.Business.CusExitItem.Schema
		{
			public const string ReferenceNumberUCR = nameof(CusExitItem.ReferenceNumberUCR);
			public const string RegistrationNumberAWB = nameof(CusExitItem.RegistrationNumberAWB);

			public const int ReferenceNumberUCRMaxLength = 35;
			public const int RegistrationNumberAWBMaxLength = 35;
		}

		[ResourceStringData("Enterprise.Customs.DE.Business.CusExitItem|ReferenceNumberUCR", Caption = "Reference Number UCR")]
		[MaxLength(Schema.ReferenceNumberUCRMaxLength)]
		public ZString ReferenceNumberUCR
		{
			get => UCRCusEntryNumberWrapper.EntryNumber;
			set => UCRCusEntryNumberWrapper.SetEntryNumber(value, ReferenceNumberUCRInfo);
		}

		CusEntryNumberWrapper UCRCusEntryNumberWrapper => ucrCusEntryNumberWrapper ?? (ucrCusEntryNumberWrapper = new CusEntryNumberWrapper(this, CusEntryNumberTypes.Standard.UniqueConsignementReference));
		CusEntryNumberWrapper ucrCusEntryNumberWrapper;

		public ZPropertyInfo ReferenceNumberUCRInfo => GetZPropertyInfo(Schema.ReferenceNumberUCR);

		[ResourceStringData("Enterprise.Customs.DE.Business.CusExitItem|RegistrationNumberAWB", Caption = "Registration Number (ext.)")]
		[MaxLength(Schema.RegistrationNumberAWBMaxLength)]
		public ZString RegistrationNumberAWB
		{
			get => AWBCusEntryNumberWrapper.EntryNumber;
			set => AWBCusEntryNumberWrapper.SetEntryNumber(value, RegistrationNumberAWBInfo);
		}

		CusEntryNumberWrapper AWBCusEntryNumberWrapper => awbCusEntryNumberWrapper ?? (awbCusEntryNumberWrapper = new CusEntryNumberWrapper(this, CusEntryNumberTypes.Germany.AirWaybillEntryNumber));
		CusEntryNumberWrapper awbCusEntryNumberWrapper;

		public ZPropertyInfo RegistrationNumberAWBInfo => GetZPropertyInfo(Schema.RegistrationNumberAWB);

		public new CusExitDetail CusExitDetail => (CusExitDetail)base.CusExitDetail;

		public new CusExitItemLookups Lookups => (CusExitItemLookups)base.Lookups;

		public new CusExitItemValidation Validation => (CusExitItemValidation)base.Validation;

		protected override EU.Business.CusExitItemLookups GetNewLookups() => new CusExitItemLookups(this);

		protected override EU.Business.CusExitItemValidation GetNewValidation() => new CusExitItemValidation(this);
	}
}
