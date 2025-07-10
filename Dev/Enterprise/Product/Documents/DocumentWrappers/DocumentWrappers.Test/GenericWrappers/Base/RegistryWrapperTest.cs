using System;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Base.Testing
{
	[TestedType(typeof(RegistryWrapper))]
	sealed class RegistryWrapperTest : GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			RegistryWrapper wrapperEmpty = new RegistryWrapper(Factory);
			AssertEquals("wrapperEmpty.ToString()", "(No Default Field Value Available on Registry)", wrapperEmpty.ToString());
		}

		public void TestCertificateOfOriginClause()
		{
			ZString expectedResult = DocumentsDataRegistry.Instance.CertificateOfOriginStandardClause.Value;
			RegistryWrapper wrapper = new RegistryWrapper(Factory);
			AssertEquals("wrapper.CertificateOfOriginClause", expectedResult, wrapper.CertificateOfOriginClause);
			expectedResult = "GET THIS INTO YA";
			DocumentsDataRegistry.Instance.CertificateOfOriginStandardClause.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, expectedResult);
			AssertEquals("wrapper.CertificateOfOriginClause", expectedResult, wrapper.CertificateOfOriginClause);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Registry
======================================================================
Name                                    Type
----------------------------------------------------------------------
CertificateOfOriginClause               String
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get { return @"Registry : (No Default Field Value Available on Registry)"; }
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new RegistryWrapper(Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new RegistryWrapper(Factory);
		}
	}
}
