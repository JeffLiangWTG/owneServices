using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromDetentionAdvice))]
	[SetOrgAllowMixedCase(true)]
	sealed class FreightWrapperFromDetentionAdviceTest : FreightWrapperTest
	{
		public void TestClient()
		{
			OrgHeader client = Factory.New<OrgHeader>();
			client.OH_FullName = "Client Org";
			DetentionAdviceHeader detention = new DetentionAdviceHeader(client);
			FreightWrapperFromDetentionAdvice wrapper = new FreightWrapperFromDetentionAdvice(detention, Factory);
			AssertEquals("wrapper.Client", "Client Org", wrapper.Client.CompanyName);
		}

		public void TestParents()
		{
			OrgHeader client = Factory.New<OrgHeader>();
			DetentionAdviceHeader detention = new DetentionAdviceHeader(client);
			FreightWrapperFromDetentionAdvice wrapper = new FreightWrapperFromDetentionAdvice(detention, Factory);
			AssertEquals("BusinessObjectToLogAgainst", client, ((IBODocDataProvider)wrapper).BusinessObjectToLogAgainst);
			AssertEquals("ParentBusinessObject", client, ((IBODocDataProvider)wrapper).ParentBusinessObject);
		}

		#region Implementation

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "JobNumberHeading", "Job Number" }
				};
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new FreightWrapperFromDetentionAdvice(detention, Factory);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return new DetentionAdviceHeader(Factory);
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

			detention = GetNewBusinessObjectToWrap() as DetentionAdviceHeader;
		}

		DetentionAdviceHeader detention;

		#endregion
	}
}
