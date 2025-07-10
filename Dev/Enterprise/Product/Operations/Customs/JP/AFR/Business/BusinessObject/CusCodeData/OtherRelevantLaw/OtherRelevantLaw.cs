using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class OtherRelevantLaw : CusCodeDataWithSequenceNumberLine
	{
		public OtherRelevantLaw(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("7991483D-358A-4155-87FD-2EB87C51A015", "Other Relevant Law"); }
		}

		[ResourceStringData("JPOtherRelevantLaw|CY_Data", Caption = "Other Relevant Law", ShortCaption = "ORL", MediumCaption = "Other Law")]
		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(OtherRelevantLawLookups.OtherRelevantLawsAndOrdinancesCodeList))]
		public override ZString CY_Data
		{
			get { return base.CY_Data; }
			set { base.CY_Data = value; }
		}

		public new OtherRelevantLawLookups Lookups
		{
			get { return (OtherRelevantLawLookups)base.Lookups; }
		}

		public new OtherRelevantLawValidation Validation
		{
			get { return (OtherRelevantLawValidation)base.Validation; }
		}

		public const string ORLType = "ORL";

		#region Implementation

		protected override ShortSequenceNumberGenerator GetSequenceNumberGenerator(BusinessObject bizObj)
		{
			var bill = (JPAFRBills)bizObj;
			return bill.OtherRelevantLawSequenceNumberGenerator;
		}

		protected override CusCodeDataLookups GetNewLookups()
		{
			return new OtherRelevantLawLookups(this);
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new OtherRelevantLawValidation(this);
		}

		protected override string Type
		{
			get { return ORLType; }
		}

		#endregion
	}
}
