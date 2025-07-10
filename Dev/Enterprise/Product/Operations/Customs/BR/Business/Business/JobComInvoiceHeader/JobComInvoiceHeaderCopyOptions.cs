using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public class JobComInvoiceHeaderCopyOptions : NonPersistentBusinessObject
	{
		public JobComInvoiceHeaderCopyOptions()
		{
		}

		public static class Schema
		{
			public const string AllLines = "AllLines";
			public const string OnlyLinesRequireLicense = "OnlyLinesRequireLicense";
		}

		#region AllLines

		public ZBool AllLines
		{
			get { return fAllLines; }
			set { SetNonPersistentPropertyValue(AllLinesInfo, ref fAllLines, value); }
		}

		ZBool fAllLines;

		public ZPropertyInfo AllLinesInfo => GetZPropertyInfo(Schema.AllLines);

		#endregion

		#region OnlyLinesRequireLicense

		public ZBool OnlyLinesRequireLicense
		{
			get { return fOnlyLinesRequireLicense; }
			set { SetNonPersistentPropertyValue(OnlyLinesRequireLicenseInfo, ref fOnlyLinesRequireLicense, value); }
		}

		ZBool fOnlyLinesRequireLicense;

		public ZPropertyInfo OnlyLinesRequireLicenseInfo => GetZPropertyInfo(Schema.OnlyLinesRequireLicense);

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AllLines = true;
		}
	}
}
