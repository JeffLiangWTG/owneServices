using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ExtendedHoursRequestLine : AutoExtendedHoursRequestLine
	{
		public ExtendedHoursRequestLine(ExtendedHoursRequestHeader parent)
			: base(parent.Factory)
		{
			Parent = parent;
			if (Parent.IsExport)
			{
				ReferenceNumberType = ReferenceNumberTypeList.Codes.EXP;
			}
			else
			{
				ReferenceNumberType = ReferenceNumberTypeList.Codes.IMP;
			}
			UQ = Core.Constants.Weight.Kilograms;
		}
		public ExtendedHoursRequestHeader Parent { get; }

		protected override ExtendedHoursRequestLineValidation GetNewValidation() => new ExtendedHoursRequestLineValidation(this);
		public int ReferenceNumber_MaxLength => ReferenceNumberType == ReferenceNumberTypeList.Codes.EXP ? 15 : 19;
		public int FormattedReferenceNumber_MaxLength => Parent.IsExport ? 17 : 21;

		[MaxLength(nameof(FormattedReferenceNumber_MaxLength))]
		[ResourceStringData("a9157a6d-0e79-4bb1-86a6-dfc85f49ba8d", Caption = "Entry Number")]
		[BusinessObjectTestExclude]
		public ZString FormattedReferenceNumber
		{
			get => MessageFunctions.GetFormattedEntryNumber(ReferenceNumber, Parent.IsExport ? ReferenceNumberType : CustomsEntryType);
			set
			{
				ReferenceNumber = MessageFunctions.GenerateUnformattedEntryNumber(value, ReferenceNumber_MaxLength);

				if (!IsValidationSuspended)
				{
					Validation.ValidateFormattedReferenceNumber();
				}
			}
		}
		public ZPropertyInfo FormattedReferenceNumberInfo => GetZPropertyInfo(nameof(FormattedReferenceNumber));

		[ResourceStringData("1E4E8C76-6DCC-4C49-B606-B109209A5A57", Caption = "Entry Number Type")]
		[ReadOnly(true)]
		public override ZString ReferenceNumberType { get => base.ReferenceNumberType; set => base.ReferenceNumberType = value; }

		[ResourceStringData("802BEA2A-A758-4C72-B1D9-F4B5333B806F", Caption = "Tariff Description")]
		public override ZString HSDescription { get => base.HSDescription; set => base.HSDescription = value; }

		[ResourceStringData("CA65C893-0F72-459B-A3B1-542D86679919", Caption = "Customs Value (USD)", MultipleKey = ElectronicDocumentTypeList.Codes._5AC)]
		[ResourceStringData("479EE3BF-AF28-478F-9781-EA41C3DFC2A8", Caption = "Customs Value (USD)", MultipleKey = ElectronicDocumentTypeList.Codes._5GW)]
		public override ZDecimal CustomsValue { get => base.CustomsValue; set => base.CustomsValue = value; }

		[ResourceStringData("D28E28D3-5930-44E4-88D5-CBFD9A846ADC", Caption = "Total Packages")]
		public override ZInt PackageCount { get => base.PackageCount; set => base.PackageCount = value; }

		[ResourceStringData("60471F5C-B6CC-4A48-A326-099C4FD063E8", Caption = "Total Gross Weight (KG)")]
		public override ZDecimal TotalWeight { get => base.TotalWeight; set => base.TotalWeight = value; }

		[ResourceStringData("17B1E65C-D98D-4A9E-AA7C-CC197F2BA319", Caption = "Bonded Area Code")]
		[List(nameof(Lookups) + "." + nameof(ExtendedHoursRequestLineLookups.BondedAreaCodeList))]
		public override ZString BondedAreaCode { get => base.BondedAreaCode; set => base.BondedAreaCode = value; }

		[ResourceStringData("A8B14AC8-F3ED-4D95-A9D2-763C39F72DC7", Caption = "Supplier Company Name")]
		public override ZString SupplierName { get => base.SupplierName; set => base.SupplierName = value; }

		[ResourceStringData("F6339ED4-7674-499B-A3DB-38292E6BFA22", Caption = "Payer Company Name")]
		public override ZString PayerCompanyName { get => base.PayerCompanyName; set => base.PayerCompanyName = value; }

		[ResourceStringData("E7339ED4-7674-499B-A3DB-38292E6BFA22", Caption = "Entry Type")]
		public ZString CustomsEntryType => MessageFunctions.GetEntryType(ReferenceNumber);

		public ExtendedHoursRequestLineLookups Lookups => new ExtendedHoursRequestLineLookups(this);
	}
}
