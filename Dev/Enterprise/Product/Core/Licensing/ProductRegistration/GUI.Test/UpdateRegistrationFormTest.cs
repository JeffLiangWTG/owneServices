namespace Enterprise.ProductRegistration.GUI.Test
{
	using System;
	using System.Threading;
	using System.Windows.Forms;
	using CargoWise.Application;
	using Enterprise.Integration.Licensing;
	using Enterprise.ProductRegistration.Client;
	using Enterprise.ZArchitecture.Environment;
	using Enterprise.ZArchitecture.GUI.Testing;
	using NUnit.Framework;

	[TestedType(typeof(UpdateRegistrationForm))]
	public class UpdateRegistrationFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new UpdateRegistrationForm();
		}

		[ExpectNoExceptions]
		public void TestUsingDbConnectionWithUpdateRegistrationForm()
		{
			try
			{
				ProductRegister.ForceValidRegistrationForTest = false;

				using (var form = new UpdateRegistrationForm())
				{
					form.Show();
					Application.DoEvents();
				}
			}
			finally
			{
				ProductRegister.ForceValidRegistrationForTest = true;
			}
		}

		[ExpectNoExceptions]
		public void TestDoNotCallShowResultIfFormDisposed()
		{
			var verificationEvent = new ManualResetEvent(false);
			var rego = new ProductRegistrationForTest(verificationEvent);
			var form = new UpdateRegistrationForm();
			Globals.IsTest_ForTest.Value = false;
			using (ObjectFactory.Substitute<IProductRegistration>(rego))
			{
				AssertEquals(false, rego.IsDoingFullVerification);

				form.Show();
				Application.DoEvents();

				form.Close();
				Application.DoEvents();

				AssertEquals(true, form.IsDisposed || form.IsDisposing);
				AssertEquals(true, rego.IsDoingFullVerification);

				AssertEquals(0, rego.KeyCallCount);
				verificationEvent.Set();

				Application.DoEvents();
				Thread.Sleep(100);
				Application.DoEvents();

				AssertEquals(false, rego.IsDoingFullVerification);
				AssertEquals("Should not call Rego.Key if the form is disposed.", 0, rego.KeyCallCount);
			}
		}

		class ProductRegistrationForTest : IProductRegistration
		{
			public ProductRegistrationForTest(ManualResetEvent resetEvent)
			{
				ResetEvent = resetEvent;
			}

			readonly ManualResetEvent ResetEvent;

			public int KeyCallCount { get; private set; }

			public ProductRegistrationVerifyResult VerifyResult { get; set; } = ProductRegistrationVerifyResult.OK;

			public bool IsDoingFullVerification { get; private set; }

			public IProductRegistrationKey Key
			{
				get
				{
					KeyCallCount++;
					throw new NotImplementedException();
				}
			}

			public IProductRegistrationKeyForTest KeyForTest => throw new System.NotImplementedException();

			public string LastError => throw new System.NotImplementedException();

			public ProductRegistrationVerifyResult FullVerify(CancellationToken cancelToken, int timeoutMs = 20000)
			{
				try
				{
					IsDoingFullVerification = true;
					ResetEvent.WaitOne();
				}
				finally
				{
					IsDoingFullVerification = false;
				}

				return VerifyResult;
			}

			public bool IsWiseTechGlobalInternalSystem()
			{
				throw new System.NotImplementedException();
			}

			public ProductRegistrationVerifyResult LocalVerify()
			{
				throw new System.NotImplementedException();
			}

			public ProductRegistrationRegisterResult Register(string productKey, CancellationToken cancelToken, int timeoutMs = 20000)
			{
				throw new System.NotImplementedException();
			}

			public void ResetKeyToDefault()
			{
				throw new System.NotImplementedException();
			}

			public ProductRegistrationRegisterResult TryAutoRegisterAfterUpgrade()
			{
				throw new System.NotImplementedException();
			}

			public ProductRegistrationUnregisterResult Unregister(CancellationToken cancelToken, int timeoutMs = 20000)
			{
				throw new System.NotImplementedException();
			}

			public ProductRegistrationVerifyResult Verify(CancellationToken cancelToken, int timeoutMs = 20000)
			{
				throw new System.NotImplementedException();
			}

			public bool IsWiseTechGlobalInternalUATSystem()
			{
				throw new NotImplementedException();
			}

			public bool IsWiseTechGlobalInternalDeveloperSystem()
			{
				throw new NotImplementedException();
			}

			public bool IsWiseTechGlobalInternalTrainingSystem()
			{
				throw new NotImplementedException();
			}

			public bool IsWiseTechGlobalInternalProdSystem()
			{
				throw new NotImplementedException();
			}

			public bool IsWiseTechGlobalInternalEDISystem()
			{
				throw new NotImplementedException();
			}

			public ProductRegistrationProduct GetProduct()
			{
				throw new NotImplementedException();
			}
		}
	}
}
