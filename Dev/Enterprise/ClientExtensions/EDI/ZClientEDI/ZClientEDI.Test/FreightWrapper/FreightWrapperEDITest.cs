using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Client.EDI.Business.Test
{
	internal abstract class FreightWrapperEDITest<T> : FreightWrapperTest where T : BusinessObject, IJobHeaderParent, IJobNumber
	{
		public override void TestTrackingBusinessObjectPK()
		{
			var wrapper = (FreightWrapper)GetSetupWrapperForDefaultFormatting();
			AssertEquals("TrackingBusinessObjectPK", ediBusinessObject.PK, wrapper.TrackingBusinessObjectPK);
		}

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string> { { "JobNumberHeading", "Job Number" } };
			}
		}

		#region Implementation
		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return GetNewFreightWrapperCore();
		}

		protected abstract GenericWrapper GetNewFreightWrapperCore();
		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.NewWithValidTestData<T>();
		}

		protected override void SetUp()
		{
			base.SetUp();
			ediBusinessObject = (T)GetNewBusinessObjectToWrap();
		}

		protected T ediBusinessObject;
		#endregion
	}
}
