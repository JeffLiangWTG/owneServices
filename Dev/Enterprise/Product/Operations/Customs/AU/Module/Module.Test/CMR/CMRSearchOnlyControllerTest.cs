using System;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(CMRSearchOnlyController))]
	abstract class CMRSearchOnlyControllerTest : ZControllerBasherTest
	{
		[ExpectException(typeof(NotSupportedException))]
		public override void TestDeleteForm()
		{
			base.TestDeleteForm();
		}

		[ExpectException(typeof(NotSupportedException))]
		public override void TestEditForm()
		{
			base.TestEditForm();
		}

		[ExpectException(typeof(NotSupportedException))]
		public override void TestNewForm()
		{
			base.TestNewForm();
		}

		[ExpectException(typeof(NotSupportedException))]
		public override void TestViewForm()
		{
			base.TestViewForm();
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;
	}
}
