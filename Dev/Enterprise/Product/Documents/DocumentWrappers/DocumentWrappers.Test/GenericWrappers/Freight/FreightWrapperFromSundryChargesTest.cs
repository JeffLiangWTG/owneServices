using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromSundryCharges))]
	sealed class FreightWrapperFromSundryChargesTest : FreightWrapperTest
	{
		public override void TestTrackingBusinessObjectPK()
		{
			var charges = (SundryCharges)GetNewBusinessObjectToWrap();
			var wrapper = new FreightWrapperFromSundryCharges(charges, Factory);

			AssertEquals("TrackingBusinessObjectPK", charges.PK, wrapper.TrackingBusinessObjectPK);
		}

		#region Implementation

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "JobNumber", "JobNumber 1" },
					{ "JobNumberBarcodeText", "^ASC=JobNumber 1;;|" },
					{ "JobNumberBarcodeTextForFont", "È^ASC=JobNumber¯1;;|<Ê" },
					{ "JobNumberBarcodeTextWithoutDocManagerCodes", "ÈJobNumber¯1*Ê" },
					{ "JobNumberHeading", "Job Number" }
				};
			}
		}

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
InvoicingJob : JobNumber 1
Principal : BILL TO PARTY\nAUSTRALIA";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var charges = (SundryCharges)GetNewBusinessObjectToWrap();
			charges.D4_JobNumber = "JobNumber 1";
			charges.D4_OH_BillToParty = GetOrgHeader("BILL TO PARTY").PK;

			new Job.Loader(charges).TryLoadOrCreate();

			return new FreightWrapperFromSundryCharges(charges, Factory);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			var charges = Factory.NewWithValidTestData<SundryCharges>();
			charges.D4_OH_BillToParty = Factory.NewWithValidTestData<OrgHeader>().PK;

			return charges;
		}

		protected override bool IsCarrierUsed
		{
			get
			{
				return false;
			}
		}

		#endregion
	}
}
