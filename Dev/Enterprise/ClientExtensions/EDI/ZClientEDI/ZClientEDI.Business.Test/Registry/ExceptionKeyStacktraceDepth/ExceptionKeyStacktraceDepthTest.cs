using System;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(ExceptionKeyStacktraceDepth))]
	class ExceptionKeyStacktraceDepthTest : RegistryBusinessObjectTemplateTestCase<ExceptionKeyStacktraceDepth>
	{
		public void TestValidation()
		{
			var exceptionKeyStackDepth = new ExceptionKeyStacktraceDepth { ExceptionType = "", StackDepth = 0 };
			AssertHasErrors(exceptionKeyStackDepth.ExceptionTypeInfo);
			AssertHasErrors(exceptionKeyStackDepth.StackDepthInfo);

			exceptionKeyStackDepth.ExceptionType = "System.NullReferenceException";
			exceptionKeyStackDepth.StackDepth = 1;

			AssertNoErrors(exceptionKeyStackDepth.ExceptionTypeInfo);
			AssertNoErrors(exceptionKeyStackDepth.StackDepthInfo);
		}

		protected override ExceptionKeyStacktraceDepth GetBusinessObjectToClone()
		{
			return new ExceptionKeyStacktraceDepth(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
		}

		protected override ExceptionKeyStacktraceDepth GetBusinessObjectToSerialise()
		{
			return new ExceptionKeyStacktraceDepth { ExceptionType = "System.NullReferenceException", StackDepth = 5 };
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}
	}
}
