using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromPrintSummary))]
	sealed class FreightWrapperFromPrintSummaryTest : FreightWrapperTest
	{
		public override void TestTrackingBusinessObjectPK()
		{
			var wrapper = (FreightWrapperFromPrintSummary)GetSetupWrapperForDefaultFormatting();
			AssertEquals("TrackingBusinessObjectPK", PrintSummary.PK, wrapper.TrackingBusinessObjectPK);
		}

		#region Implementation

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new FreightWrapperFromPrintSummary(PrintSummary, Factory);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return new PrintSummary(Factory, null);
		}

		protected override bool IsCarrierUsed
		{
			get
			{
				return false;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			PrintSummary = new PrintSummary(Factory, null);
		}

		PrintSummary PrintSummary;

		#endregion
	}
}
