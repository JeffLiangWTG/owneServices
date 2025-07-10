using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromTransactionHeader))]
	sealed class FreightWrapperFromTransactionHeaderTest : FreightWrapperTest
	{
		public override void TestTrackingBusinessObjectPK()
		{
			var wrapper = (FreightWrapperFromTransactionHeader)GetSetupWrapperForDefaultFormatting();
			AssertEquals("TrackingBusinessObjectPK", Journal.PK, wrapper.TrackingBusinessObjectPK);
		}

		#region Implementation

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new FreightWrapperFromTransactionHeader(Journal, Factory);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<APJournal>();
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

			Journal = Factory.New<APJournal>();
		}

		APJournal Journal;

		#endregion
	}
}
