using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ContainerTypeWrapper))]
	sealed class ContainerTypeWrapperTest : Base.Testing.GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			ContainerTypeWrapper wrapperEmpty = new ContainerTypeWrapper(null, Factory);
			AssertEquals("wrapperEmpty.ToString()", "", wrapperEmpty.ToString());
			AssertEquals("wrapperEmpty.Code", "", wrapperEmpty.Code);
			AssertEquals("wrapperEmpty.CodeAndDescription", "", wrapperEmpty.CodeAndDescription);
			AssertEquals("wrapperEmpty.Description", "", wrapperEmpty.Description);
			AssertEquals("wrapperEmpty.ISOCode", "", wrapperEmpty.ISOCode);
			AssertEquals("wrapperEmpty.TareWeight.ToString()", "", wrapperEmpty.TareWeight.ToString());
		}

		public void TestWrapperMappingFull()
		{
			RefContainer containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "CODE";
			containerType.RC_Description = "DESCRIPTION";
			containerType.RC_ISOType = "ISOT";
			containerType.RC_TareWeight = 12.34m;
			ContainerTypeWrapper wrapperFull = new ContainerTypeWrapper(containerType, Factory);
			AssertEquals("wrapperFull.Code", "CODE", wrapperFull.Code);
			AssertEquals("wrapperFull.CodeAndDescription", "CODE - DESCRIPTION", wrapperFull.CodeAndDescription);
			AssertEquals("wrapperFull.Description", "DESCRIPTION", wrapperFull.Description);
			AssertEquals("wrapperFull.ISOCode", "ISOT", wrapperFull.ISOCode);
			AssertEquals("wrapperFull.TareWeight.ToString()", "12.340 KG", wrapperFull.TareWeight.ToString());
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
ContainerType                      (Default Field: CodeAndDescription)
======================================================================
Name                                    Type
----------------------------------------------------------------------
TareWeight                              ValueAndUnit
Code                                    String
CodeAndDescription                      String
Description                             String
ISOCode                                 String
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Registry : (No Default Field Value Available on Registry)
TareWeight : 12.340 KG";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			RefContainer containerType = Factory.New<RefContainer>();
			containerType.RC_TareWeight = 12.34m;
			return new ContainerTypeWrapper(containerType, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new ContainerTypeWrapper(null, Factory);
		}
	}
}
