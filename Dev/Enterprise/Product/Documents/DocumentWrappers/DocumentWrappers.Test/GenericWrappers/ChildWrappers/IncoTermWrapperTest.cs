using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(IncoTermWrapper))]
	sealed class IncoTermWrapperTest : Base.Testing.GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			var wrapper = new IncoTermWrapper(ZString.Empty, new CodeDescriptionPairList(), null, Factory);

			AssertEquals("wrapper.Code", ZString.Empty, wrapper.Code);
			AssertEquals("wrapper.CodeAndDescription", ZString.Empty, wrapper.CodeAndDescription);
			AssertEquals("wrapper.Description", ZString.Empty, wrapper.Description);
			AssertEquals("wrapper.PaymentType", ZString.Empty, wrapper.PaymentType.CodeAndDescription);
		}

		public void TestCollectByPaymentType()
		{
			var wrapper = new IncoTermWrapper(Core.Constants.DomesticPaymentTerms.CollectThirdParty,
				new CodeDescriptionPairList(OLookUpEditType.DomesticPaymentTerms), IncoTermWrapper.Deciders.ByPaymentType(Core.Constants.PaymentType.Collect), Factory);

			AssertEquals("wrapper.Code", "C3P", wrapper.Code);
			AssertEquals("wrapper.CodeAndDescription", "C3P - Collect 3rd Party", wrapper.CodeAndDescription);
			AssertEquals("wrapper.Description", "Collect 3rd Party", wrapper.Description);
			AssertEquals("wrapper.PaymentType", "CCX", wrapper.PaymentType.Code);
			AssertEquals("wrapper.PaymentType", "Collect", wrapper.PaymentType.Description);
			AssertEquals("wrapper.PaymentType", "CCX - Collect", wrapper.PaymentType.CodeAndDescription);
		}

		public void TestPrepaidByPaymentType()
		{
			var wrapper = new IncoTermWrapper(Core.Constants.IncoTerms.FreeCarrier,
				new IncoTermsCodeDescriptionPairList(), IncoTermWrapper.Deciders.ByPaymentType(Core.Constants.PaymentType.Prepaid), Factory);

			AssertEquals("wrapper.Code", "FCA", wrapper.Code);
			AssertEquals("wrapper.CodeAndDescription", "FCA - FCA - Free Carrier (seller is responsible for origin, buyer for loading)", wrapper.CodeAndDescription);
			AssertEquals("wrapper.Description", "FCA - Free Carrier (seller is responsible for origin, buyer for loading)", wrapper.Description);
			AssertEquals("wrapper.PaymentType", "PPD", wrapper.PaymentType.Code);
			AssertEquals("wrapper.PaymentType", "Prepaid", wrapper.PaymentType.Description);
			AssertEquals("wrapper.PaymentType", "PPD - Prepaid", wrapper.PaymentType.CodeAndDescription);
		}

		public void TestCollectByChargeGroup()
		{
			var incoCodes = new string[]
								{
									Core.Constants.IncoTerms.ExWorks, Core.Constants.IncoTerms.FreeCarrier,
									Core.Constants.IncoTerms.FreeAlongsideShip, Core.Constants.IncoTerms.FreeOnBoard
								};

			foreach (var incoCode in incoCodes)
			{
				var wrapper = new IncoTermWrapper(incoCode, new IncoTermsCodeDescriptionPairList(),
												  IncoTermWrapper.Deciders.ByChargeGroup(ChargeCodeGroupList.Codes.Freight), Factory);

				AssertEquals("wrapper.Code", incoCode, wrapper.Code);
				AssertEquals("wrapper.PaymentType", "CCX", wrapper.PaymentType.Code);
				AssertEquals("wrapper.PaymentType", "Collect", wrapper.PaymentType.Description);
				AssertEquals("wrapper.PaymentType", "CCX - Collect", wrapper.PaymentType.CodeAndDescription);
			}
		}

		public void TestPrepaidByChargeGroup()
		{
			var incoCodes = new string[]
								{
									Core.Constants.IncoTerms.CostAndFreight, Core.Constants.IncoTerms.CostInsuranceAndFreight,
									Core.Constants.IncoTerms.CarriagePaidTo,
									Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, Core.Constants.IncoTerms.DeliveredAtFrontier,
									Core.Constants.IncoTerms.DeliveredExShip, Core.Constants.IncoTerms.DeliveredExQuay,
									Core.Constants.IncoTerms.DeliveredDutyUnpaid, Core.Constants.IncoTerms.DeliveredDutyPaid
								};

			foreach (var incoCode in incoCodes)
			{
				var wrapper = new IncoTermWrapper(incoCode, new IncoTermsCodeDescriptionPairList(),
												  IncoTermWrapper.Deciders.ByChargeGroup(ChargeCodeGroupList.Codes.Freight), Factory);

				AssertEquals("wrapper.Code", incoCode, wrapper.Code);
				AssertEquals("wrapper.PaymentType", "PPD", wrapper.PaymentType.Code);
				AssertEquals("wrapper.PaymentType", "Prepaid", wrapper.PaymentType.Description);
				AssertEquals("wrapper.PaymentType", "PPD - Prepaid", wrapper.PaymentType.CodeAndDescription);
			}
		}

		public void TestUnknownByChargeGroup()
		{
			var incoCodes = new string[]
								{
									Core.Constants.DomesticPaymentTerms.Collect, Core.Constants.DomesticPaymentTerms.CollectCOD,
									Core.Constants.DomesticPaymentTerms.CollectThirdParty, Core.Constants.DomesticPaymentTerms.Prepaid
								};

			foreach (var incoCode in incoCodes)
			{
				var wrapper = new IncoTermWrapper(incoCode, new IncoTermsCodeDescriptionPairList(),
												  IncoTermWrapper.Deciders.ByChargeGroup(ChargeCodeGroupList.Codes.Freight), Factory);

				AssertEquals("wrapper.Code", incoCode, wrapper.Code);
				AssertEquals("wrapper.PaymentType", ZString.Empty, wrapper.PaymentType.Code);
				AssertEquals("wrapper.PaymentType", ZString.Empty, wrapper.PaymentType.Description);
				AssertEquals("wrapper.PaymentType", ZString.Empty, wrapper.PaymentType.CodeAndDescription);
			}
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
INCO Term                          (Default Field: CodeAndDescription)
======================================================================
Name                                    Type
----------------------------------------------------------------------
PaymentType                             CodeAndDescription
Code                                    String
CodeAndDescription                      String
Description                             String
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get { return @"PaymentType : 
Registry : (No Default Field Value Available on Registry)"; }
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new IncoTermWrapper(ZString.Empty, new IncoTermsCodeDescriptionPairList(), null, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new IncoTermWrapper(ZString.Empty, new IncoTermsCodeDescriptionPairList(), null, Factory);
		}
	}
}
