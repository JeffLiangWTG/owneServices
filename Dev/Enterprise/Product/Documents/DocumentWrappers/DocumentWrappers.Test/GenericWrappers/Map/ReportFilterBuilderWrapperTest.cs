using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map.Testing
{
	[TestedType(typeof(ReportFilterBuilderWrapper))]
	sealed class ReportFilterBuilderWrapperTest : GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			var wrapperEmpty = new ReportFilterBuilderWrapper(new FilterBuilderDocumenter("", "", new List<string>()), Factory);
			AssertEquals("wrapperEmpty.ToString()", "", wrapperEmpty.ToString());
			AssertEquals("wrapperEmpty.Useage", ZString.Empty, wrapperEmpty.Useage);
			AssertEquals("wrapperEmpty.Explanation", ZString.Empty, wrapperEmpty.Explanation);
			AssertEquals("wrapperEmpty.SupportLookup", false, wrapperEmpty.SupportLookup);
			AssertEquals("wrapperEmpty.SupportedProperties", ZString.Empty, wrapperEmpty.SupportedProperties);
			AssertEquals("wrapperEmpty.ValueProviderDocumenters", "", wrapperEmpty.ValueProviderDocumenters);
		}

		public void TestFull()
		{
			var reportFilterBuilder = new FilterBuilderDocumenter("Date", "Used as a date time picker", new List<string>() { "type", "default" }, true);
			var documenters = new List<ValueProviderDocumenter>();
			documenters.Add(new ValueProviderDocumenter("<TextMacro.Value>", (NoResString)"Dummy description"));
			reportFilterBuilder.ValueProviderDocumenters = documenters;

			var wrapper = new ReportFilterBuilderWrapper(reportFilterBuilder, Factory);
			AssertEquals("wrapper.ToString()", "Date", wrapper.ToString());
			AssertEquals("wrapper.Useage", "Date", wrapper.Useage);
			AssertEquals("wrapper.Explanation", "Used as a date time picker", wrapper.Explanation);
			AssertEquals("wrapperEmpty.SupportLookup", true, wrapper.SupportLookup);
			AssertEquals("wrapperEmpty.SupportedProperties", string.Format(@"type{0}default", System.Environment.NewLine), wrapper.SupportedProperties);

			AssertEquals("wrapperEmpty.ValueProviderDocumenters", "<TextMacro.Value>: Dummy description", wrapper.ValueProviderDocumenters);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
ReportFilterBuilder                            (Default Field: Useage)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Explanation                             String
SupportedProperties                     String
Useage                                  String
ValueProviderDocumenters                String
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get { return @"Registry : (No Default Field Value Available on Registry)"; }
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new CodeMultilingualDescriptionWrapper("tab", (NoResString)"desides which group the filter belongs to", Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new ReportFilterBuilderWrapper(new FilterBuilderDocumenter("", "", new List<string>()), Factory);
		}
	}
}
