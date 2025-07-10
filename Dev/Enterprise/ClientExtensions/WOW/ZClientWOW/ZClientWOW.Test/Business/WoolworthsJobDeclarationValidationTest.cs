using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WoolworthsJobDeclaration))]
	public class WoolworthsJobDeclarationValidationTest : InvoiceOrderLinkTestCase
	{
		#region Metadata
		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Enterprise.Metadata.Business.AUJobDeclaration);
			}
		}

		#endregion
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		public void TestValidateInvoiceMatchesOrderDelivery_PortOfLoading()
		{
			fOrder.JD_RL_NKPortOfLoading = "AUSYD";
			fDeclaration.JE_RL_NKPortOfLoading = "AUMEL";
			AssertEquals("Has warning because the PortOfLoading is not consistent", true, fDeclaration.JE_RL_NKPortOfLoadingInfo.HasWarnings());
			fOrder.JD_RL_NKPortOfLoading = "AUSYD";
			fDeclaration.JE_RL_NKPortOfLoading = "AUSYD";
			AssertEquals("No warning because the PortOfLoading is now consistent", false, fDeclaration.JE_RL_NKPortOfLoadingInfo.HasWarnings());
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<JobDeclaration>();
		}
		#endregion
	}
}
