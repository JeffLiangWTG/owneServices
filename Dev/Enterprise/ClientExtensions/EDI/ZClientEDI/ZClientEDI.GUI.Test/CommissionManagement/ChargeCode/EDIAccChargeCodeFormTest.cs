using System;
using System.Threading;
using AuthenticationService.Client.Models;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.CommissionManagement.GUI.Testing
{
	using System.Windows.Forms;
	using CargoWise.Application;
	using Enterprise.Integration.Rating;
	using Enterprise.ZArchitecture.GUI.Testing;
	using Moq;
	using NUnit.Framework;

	[TestedType(typeof(EDIAccChargeCodeForm))]
	public class EDIAccChargeCodeFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			return new EDIAccChargeCodeForm(chargeCode);
		}

		protected override void SetUp()
		{
			base.SetUp();

			if (!ObjectFactory.HasBeenSubstituted<IAuthTokenProvider>())
			{
				var authTokenProvider = new Mock<IAuthTokenProvider>();
				authTokenProvider
					.Setup(m => m.GetToken(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Action<LoginInfo>>(),
						It.IsAny<CancellationToken>(), It.IsAny<bool>()))
					.Returns(("some token", string.Empty));

				ObjectFactory.Substitute(authTokenProvider.Object);
			}
		}

		protected override void TearDown()
		{
			ObjectFactory.DisposeSubstitutions();
			base.TearDown();
		}
	}
}
