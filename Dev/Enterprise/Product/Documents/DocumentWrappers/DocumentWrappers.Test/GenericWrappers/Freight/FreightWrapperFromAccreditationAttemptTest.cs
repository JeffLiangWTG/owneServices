using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromAccreditationAttempt))]
	sealed class FreightWrapperFromAccreditationAttemptTest : FreightWrapperTest
	{
		public override void TestTrackingBusinessObjectPK()
		{
			var attempt = Factory.New<GlbAccreditationAttempt>();
			var wrapper = new FreightWrapperFromAccreditationAttempt(attempt, Factory);
			AssertEquals("TrackingBusinessObjectPK", attempt.PK, wrapper.TrackingBusinessObjectPK);
		}

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "JobNumber", "John Smith" },
					{ "JobNumberBarcodeTextWithoutDocManagerCodes", new TextBarcode("John Smith").TextAs128sFontString },
				};
			}
		}

		#region Implementation

		protected override ZString OverriddenExpectedDefaultFormatting => @"AccreditationAttempt : (No Default Field Value Available on AccreditationAttempt)";

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var attempt = GetNewBusinessObjectToWrap() as GlbAccreditationAttempt;
			return new FreightWrapperFromAccreditationAttempt(attempt, Factory);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			var accreditation = Factory.New<GlbAccreditation>();
			accreditation.HAC_Code = "CCO";
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "John Smith";
			var attempt = Factory.New<GlbAccreditationAttempt>();
			attempt.HAA_HAC = accreditation.PK;
			attempt.HAA_PER = person.PK;
			return attempt;
		}

		#endregion
	}
}
