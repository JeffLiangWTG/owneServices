using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(InterchangeSenderProxyUser))]
	sealed class InterchangeSenderProxyUserTest : RegistryBusinessObjectTest
	{
		public void TestCodemaxLength()
		{
			AssertEquals(EDIInterchangeSchema.EI_From.MaxLength, BizObj.CodeInfo.MaxLength);
		}

		public override void TestMaxDescriptionLength()
		{
			AssertEquals(GlbStaffSchema.GS_Code.MaxLength, BizObj.DescriptionInfo.MaxLength);
		}

		public void TestDescriptionValid()
		{
			var obj = new InterchangeSenderProxyUser(Factory);
			obj.DescriptionValue = (NoResString)"asd";
			AssertHasError(obj.DescriptionValueInfo, "Enter a valid Staff Code.");

			obj.DescriptionValue = (NoResString)Factory.Load<IGlbStaff>(new ZGuid(Env.CurrentUserPK)).GS_Code;
			AssertNoError(obj.DescriptionValueInfo, "Enter a valid Staff Code.");
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone)
		{
			if (clone is InterchangeSenderProxyUser ip)
			{
				AssertEquals("NOBO", ip.Code);
				AssertEquals("~BP", ip.Description);
			}
			else
			{
				Fail();
			}
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = (InterchangeSenderProxyUser)GetNewBusinessObject();
			result.Code = "NOBO";
			result.Description = (NoResString)"~BP";
			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override int ExpectedDefaultMaxCodeLength => EDIInterchangeSchema.EI_From.MaxLength;

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => false;

		new InterchangeSenderProxyUser BizObj
		{
			get { return (InterchangeSenderProxyUser)base.BizObj; }
		}
	}
}
