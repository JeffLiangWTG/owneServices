using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(HarmonisedCodeWrapper))]
	sealed class HarmonisedCodeWrapperTest : Base.Testing.GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			var wrapperEmpty = new HarmonisedCodeWrapper(null, Factory);
			AssertEquals("wrapperEmpty.ToString()", ZString.Empty, wrapperEmpty.ToString());
			AssertEquals("wrapperEmpty.Country.Code", ZString.Empty, wrapperEmpty.Country.Code);
			AssertEquals("wrapperEmpty.Code", ZString.Empty, wrapperEmpty.Code);
		}

		public void TestWrapperMappingFull()
		{
			#region Setup

			var shipment = Factory.New<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			var hc = packline.HarmonisedCodes.AddNew();
			hc.JLH_RN_NKCountry = "CN";
			hc.JLH_Code = "8888";

			var wrapper = new HarmonisedCodeWrapper(hc, Factory);

			#endregion

			AssertEquals("wrapper.ToString()", "8888", wrapper.ToString());
			AssertEquals("wrapper.Country.Code", "CN", wrapper.Country.Code);
			AssertEquals("wrapper.Code", "8888", wrapper.Code);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
HarmonisedCode                                   (Default Field: Code)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Country                                 Country
Code                                    String
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"Country : CA - Canada
Registry : (No Default Field Value Available on Registry)";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var packline = Factory.New<PackLine>();
			var hc = packline.HarmonisedCodes.AddNew();
			hc.JLH_RN_NKCountry = "CA";
			return new HarmonisedCodeWrapper(hc, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var packline = Factory.New<PackLine>();
			var hc = packline.HarmonisedCodes.AddNew();
			return new HarmonisedCodeWrapper(hc, Factory);
		}
	}
}
